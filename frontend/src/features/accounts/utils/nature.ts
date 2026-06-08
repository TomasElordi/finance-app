import { NatureType } from "../types/account";

const NATURE_LABELS: Record<string, string> = {
  Asset: "Activo",
  Liability: "Pasivo",
  Equity: "Patrimonio Neto",
  Income: "Resultado Positivo",
  Expense: "Resultado Negativo",
};

const NATURE_ORDER: Record<string, number> = {
  Asset: 0,
  Liability: 1,
  Equity: 2,
  Income: 3,
  Expense: 4,
};

const NATURE_COLORS: Record<string, string> = {
  Asset: "bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300",
  Liability: "bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300",
  Equity:
    "bg-purple-100 text-purple-700 dark:bg-purple-900/40 dark:text-purple-300",
  Income:
    "bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300",
  Expense:
    "bg-orange-100 text-orange-700 dark:bg-orange-900/40 dark:text-orange-300",
};

export const NATURE_OPTIONS = [
  NatureType.Asset,
  NatureType.Liability,
  NatureType.Equity,
  NatureType.Income,
  NatureType.Expense,
].map((value) => ({ value, label: getNatureLabel(NatureType[value]) }));

export function getNatureLabel(nature: string): string {
  return NATURE_LABELS[nature] ?? nature;
}

export function getNatureColor(nature: string): string {
  return NATURE_COLORS[nature] ?? "bg-muted text-muted-foreground";
}

export function getNatureOrder(nature: string): number {
  return NATURE_ORDER[nature] ?? 99;
}
