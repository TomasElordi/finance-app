export function monthLabel(year: number, month: number, style: "long" | "short" = "long"): string {
  const label = new Date(Date.UTC(year, month - 1, 1)).toLocaleDateString("es-AR", {
    month: style,
    timeZone: "UTC",
  });
  return label.replace(".", "");
}

export function expensesHref(year: number, month: number, accountId?: string): string {
  const params = new URLSearchParams({ year: String(year), month: String(month) });
  if (accountId) params.set("account", accountId);
  return `?${params.toString()}`;
}
