// Entries are calendar dates: store them as UTC midnight, matching how the
// API buckets months (UTC), and always display them in UTC.
export function toEntryDate(date: string): string {
  return `${date}T00:00:00.000Z`;
}

export function formatEntryDate(date: string): string {
  return new Date(date).toLocaleDateString("es-AR", {
    year: "numeric",
    month: "short",
    day: "numeric",
    timeZone: "UTC",
  });
}
