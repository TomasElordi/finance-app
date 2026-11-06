"use client";

import { ArrowRight } from "lucide-react";
import { formatCurrency } from "@/src/features/accounts/utils/format";
import { SharedSettlement } from "../types/shared";
import { deleteSettlementAction } from "../actions/settlements";
import { formatShortDate } from "../utils/split";
import ConfirmDeleteButton from "./confirm-delete-button";

export default function SettlementsList({
  groupId,
  settlements,
}: {
  groupId: string;
  settlements: SharedSettlement[];
}) {
  return (
    <section className="flex flex-col gap-3">
      <h2 className="text-sm font-semibold text-muted-foreground">Pagos entre miembros</h2>
      <div className="flex flex-col divide-y rounded-lg border">
        {settlements.map((s) => (
          <div key={s.id} className="flex items-center justify-between gap-3 px-3 py-2 text-sm">
            <div className="flex flex-wrap items-center gap-2">
              <span className="tabular-nums text-muted-foreground">{formatShortDate(s.date)}</span>
              <span>{s.fromName}</span>
              <ArrowRight className="h-4 w-4 text-muted-foreground" />
              <span>{s.toName}</span>
              {s.note && <span className="text-muted-foreground">· {s.note}</span>}
            </div>
            <div className="flex items-center gap-2">
              <span className="tabular-nums">{formatCurrency(s.amount)}</span>
              <ConfirmDeleteButton
                title="¿Eliminar este pago?"
                description="El saldo vuelve a incluir esta deuda."
                onConfirm={() => deleteSettlementAction(groupId, s.id)}
              />
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}
