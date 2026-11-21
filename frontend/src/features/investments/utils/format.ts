import { GroupColor } from "../types/investment";

export type DisplayCurrency = "USD" | "ARS";

export function formatMoney(amount: number, currency: DisplayCurrency): string {
  return new Intl.NumberFormat("es-AR", {
    style: "currency",
    currency,
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatPercent(value: number): string {
  return `${new Intl.NumberFormat("es-AR", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)}%`;
}

export function formatQuantity(value: number): string {
  return new Intl.NumberFormat("es-AR", { maximumFractionDigits: 4 }).format(value);
}

export function gainClass(value: number): string {
  if (value > 0.005) return "text-green-600 dark:text-green-500";
  if (value < -0.005) return "text-destructive";
  return "";
}

// Full class names so Tailwind picks them up.
export const GROUP_COLORS: Record<GroupColor, { label: string; row: string; dot: string }> = {
  amber: { label: "Amarillo", row: "bg-amber-50 dark:bg-amber-950/30", dot: "bg-amber-400" },
  slate: { label: "Gris", row: "bg-slate-100 dark:bg-slate-800/40", dot: "bg-slate-400" },
  green: { label: "Verde", row: "bg-green-50 dark:bg-green-950/30", dot: "bg-green-500" },
  red: { label: "Rojo", row: "bg-red-50 dark:bg-red-950/30", dot: "bg-red-400" },
  blue: { label: "Azul", row: "bg-blue-50 dark:bg-blue-950/30", dot: "bg-blue-500" },
  purple: { label: "Violeta", row: "bg-purple-50 dark:bg-purple-950/30", dot: "bg-purple-500" },
  teal: { label: "Turquesa", row: "bg-teal-50 dark:bg-teal-950/30", dot: "bg-teal-500" },
  pink: { label: "Rosa", row: "bg-pink-50 dark:bg-pink-950/30", dot: "bg-pink-400" },
};

// Target percents are usually typed with two decimals (1.87 + 1.88...), so a sum like
// 100.01 is rounding noise, not a misconfiguration.
export function targetIsOff(targetPercent: number): boolean {
  return Math.abs(targetPercent - 100) > 0.05;
}
