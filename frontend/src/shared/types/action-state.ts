export type ActionState<
  TErrors extends Record<string, unknown> = Record<string, string | undefined>,
> =
  | { status: "idle" }
  | { status: "success" }
  | { status: "error"; errors: TErrors; message: string };
