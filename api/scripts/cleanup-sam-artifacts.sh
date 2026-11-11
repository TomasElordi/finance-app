#!/usr/bin/env bash
# Deletes old `sam deploy` artifacts from the SAM-managed S3 bucket.
#
# Keeps only the artifacts the stack currently references (the deployed Lambda
# zip), so a failed future deploy can still roll back to it. Everything else
# under the prefix -- old zips, packaged templates, noncurrent versions and
# delete markers -- is removed. Other prefixes in the bucket are left alone.
#
# Usage: scripts/cleanup-sam-artifacts.sh [stack-name] [s3-prefix]
#        DRY_RUN=true scripts/cleanup-sam-artifacts.sh   # only list what would be deleted
set -euo pipefail

STACK="${1:-finance-app}"
PREFIX="${2:-finance-app}"
DRY_RUN="${DRY_RUN:-false}"

BUCKET=$(aws cloudformation describe-stacks \
  --stack-name aws-sam-cli-managed-default \
  --query "Stacks[0].Outputs[?OutputKey=='SourceBucket'].OutputValue" \
  --output text)

# Every S3 key the deployed stack points at (e.g. the Lambda CodeUri).
KEEP=$(aws cloudformation get-template \
  --stack-name "$STACK" \
  --template-stage Processed \
  --query TemplateBody \
  --output json | jq -c '[.. | objects | .S3Key? | strings] | unique')

if [[ "$KEEP" == "[]" ]]; then
  echo "No S3 keys found in stack '$STACK'; refusing to delete anything." >&2
  exit 1
fi
echo "Bucket: $BUCKET"
echo "Keeping: $KEEP"

# Delete every version and delete marker under the prefix, except the current
# version of the keys the stack uses. Batches of 1000 (the delete-objects limit).
BATCHES=$(aws s3api list-object-versions --bucket "$BUCKET" --prefix "$PREFIX/" --output json |
  jq -c --argjson keep "$KEEP" '
    [ ((.Versions // []) | map(. + {current: .IsLatest}))[],
      ((.DeleteMarkers // []) | map(. + {current: false}))[] ]
    | map(select((.current and (.Key as $k | $keep | index($k) != null)) | not))
    | map({Key, VersionId})
    | _nwise(1000)
    | {Objects: ., Quiet: true}')

if [[ -z "$BATCHES" ]]; then
  echo "Nothing to delete."
  exit 0
fi

while IFS= read -r batch; do
  count=$(jq '.Objects | length' <<<"$batch")
  if [[ "$DRY_RUN" == "true" ]]; then
    echo "[dry run] would delete $count object versions:"
    jq -r '.Objects[] | "  \(.Key) (\(.VersionId))"' <<<"$batch"
  else
    aws s3api delete-objects --bucket "$BUCKET" --delete "$batch" >/dev/null
    echo "Deleted $count object versions."
  fi
done <<<"$BATCHES"
