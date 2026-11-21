// Builds an API path from a template, checking every interpolated value. Server action
// arguments come from the client as plain JSON whatever their TypeScript types say, so
// an "id" like "../../oauth/x" or "1?x=y" must never reach the URL as-is.
//
//   apiPath`/entry/${id}`                       -> id must be a UUID
//   apiPath`/budget/x?year=${year}&month=${m}`  -> numbers must be integers
const UUID = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
const INTEGER = /^-?\d{1,9}$/;

export function apiPath(strings: TemplateStringsArray, ...values: unknown[]): string {
  return strings.reduce((path, part, i) => {
    if (i === 0) return part;
    return path + encodeURIComponent(checkSegment(values[i - 1])) + part;
  }, "");
}

function checkSegment(value: unknown): string {
  if (typeof value === "number" && Number.isSafeInteger(value)) return String(value);
  if (typeof value === "string" && (UUID.test(value) || INTEGER.test(value))) return value;
  throw new Error("Invalid API path parameter");
}
