"use client";

import { useState, useTransition } from "react";
import { Check, Copy } from "lucide-react";
import { revokeMcpClientsAction } from "../actions/revoke-mcp-clients";
import AlertConfirm from "./alert-confirm";
import { Button } from "./ui/button";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from "./ui/sheet";

type CopyKey = "url" | "cli";

export function ConnectMcpSheet({
  mcpUrl,
  open,
  onOpenChange,
}: {
  mcpUrl: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const [copied, setCopied] = useState<CopyKey | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [confirmRevoke, setConfirmRevoke] = useState(false);
  const [revoking, startRevoke] = useTransition();
  const [revokeMessage, setRevokeMessage] = useState<string | null>(null);
  const cliCommand = `claude mcp add --transport http finance-app ${mcpUrl}`;

  function revoke() {
    setConfirmRevoke(false);
    setRevokeMessage(null);
    startRevoke(async () => {
      const result = await revokeMcpClientsAction();
      if (result.status === "error") {
        setRevokeMessage(result.message);
      } else if (result.clients === 0) {
        setRevokeMessage("No había clientes conectados.");
      } else {
        setRevokeMessage(
          result.clients === 1
            ? "Se desconectó 1 cliente."
            : `Se desconectaron ${result.clients} clientes.`,
        );
      }
    });
  }

  async function copy(key: CopyKey, text: string) {
    try {
      await navigator.clipboard.writeText(text);
      setError(null);
      setCopied(key);
      setTimeout(() => setCopied(null), 1500);
    } catch {
      setError("No se pudo copiar, copialo a mano.");
    }
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="overflow-y-auto sm:max-w-md">
        <SheetHeader>
          <SheetTitle>Conectar MCP</SheetTitle>
          <SheetDescription>
            Conectá cualquier asistente o cliente compatible con MCP para
            cargar asientos, gastos de tarjeta y gastos en común, y consultar
            reportes y presupuestos desde el chat.
          </SheetDescription>
        </SheetHeader>

        <div className="flex flex-col gap-5 p-4">
          <section className="flex flex-col gap-2">
            <h3 className="text-sm font-medium">URL del servidor</h3>
            <div className="flex items-center gap-2">
              <code className="flex-1 break-all rounded-md bg-muted px-3 py-2 font-mono text-xs">
                {mcpUrl}
              </code>
              <Button
                variant="outline"
                size="icon"
                aria-label="Copiar URL"
                onClick={() => copy("url", mcpUrl)}
              >
                {copied === "url" ? (
                  <Check className="h-4 w-4" />
                ) : (
                  <Copy className="h-4 w-4" />
                )}
              </Button>
            </div>
          </section>

          <section className="flex flex-col gap-2">
            <h3 className="text-sm font-medium">Cómo conectarlo</h3>
            <ol className="list-decimal space-y-1.5 pl-5 text-sm text-muted-foreground">
              <li>
                En tu cliente, agregá un servidor MCP remoto (a veces figura
                como “custom connector” o “servidor HTTP”) y pegá la URL de
                arriba.
              </li>
              <li>
                Al conectarlo se abre el login de Finance App: entrá con tu
                email y contraseña de siempre.
              </li>
              <li>
                Listo. Pedile cosas como “cargá un gasto de $5000 en
                supermercado”.
              </li>
            </ol>
          </section>

          <section className="flex flex-col gap-2">
            <h3 className="text-sm font-medium">Ejemplos</h3>
            <p className="text-sm text-muted-foreground">
              <span className="text-foreground">Claude:</span> Settings →
              Connectors → Add custom connector.
            </p>
            <p className="text-sm text-muted-foreground">
              <span className="text-foreground">Claude Code:</span> corré este
              comando y después usá <code>/mcp</code> para iniciar sesión.
            </p>
            <div className="flex items-center gap-2">
              <code className="flex-1 break-all rounded-md bg-muted px-3 py-2 font-mono text-xs">
                {cliCommand}
              </code>
              <Button
                variant="outline"
                size="icon"
                aria-label="Copiar comando"
                onClick={() => copy("cli", cliCommand)}
              >
                {copied === "cli" ? (
                  <Check className="h-4 w-4" />
                ) : (
                  <Copy className="h-4 w-4" />
                )}
              </Button>
            </div>
          </section>

          <p className="text-xs text-muted-foreground">
            El cliente actúa con tu cuenta: solo ve y modifica tus datos.
          </p>

          <section className="flex flex-col gap-2 border-t pt-4">
            <h3 className="text-sm font-medium">Desconectar</h3>
            <p className="text-sm text-muted-foreground">
              Corta el acceso de todos los clientes conectados a tu cuenta. Para
              volver a usarlos tenés que conectarlos de nuevo.
            </p>
            <Button
              variant="outline"
              className="self-start"
              disabled={revoking}
              onClick={() => setConfirmRevoke(true)}
            >
              {revoking ? "Desconectando…" : "Desconectar todos los clientes"}
            </Button>
            {revokeMessage && (
              <p className="text-sm text-muted-foreground">{revokeMessage}</p>
            )}
          </section>

          {error && <p className="text-sm text-destructive">{error}</p>}
        </div>
      </SheetContent>
      <AlertConfirm
        open={confirmRevoke}
        title="¿Desconectar todos los clientes MCP?"
        description="Los clientes conectados (por ejemplo Claude) pierden el acceso a tu cuenta de inmediato."
        confirmText="Desconectar"
        onCancel={() => setConfirmRevoke(false)}
        onConfirm={revoke}
      />
    </Sheet>
  );
}
