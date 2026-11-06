import { Users } from "lucide-react";
import { CreateGroupForm, JoinGroupForm } from "./group-forms";

export default function SharedExpensesOnboarding({ joinCode }: { joinCode?: string }) {
  return (
    <div className="p-6 flex flex-col gap-8">
      <h1 className="text-2xl font-semibold">Gastos en común</h1>
      <div className="flex flex-col items-center gap-4 py-8 text-center">
        <div className="rounded-full bg-muted p-4">
          <Users className="h-8 w-8 text-muted-foreground" />
        </div>
        <div className="flex max-w-md flex-col gap-1">
          <p className="font-medium">Compartí gastos con otras personas</p>
          <p className="text-sm text-muted-foreground">
            Creá un grupo y pasale el código a tu pareja o amigos. Todos ven y cargan los mismos
            gastos, y la app calcula quién le debe a quién.
          </p>
        </div>
      </div>
      <div className="mx-auto grid w-full max-w-3xl grid-cols-1 gap-4 md:grid-cols-2">
        <div className="flex flex-col gap-3 rounded-lg border p-4">
          <h2 className="font-medium">Crear un grupo</h2>
          <CreateGroupForm />
        </div>
        <div className="flex flex-col gap-3 rounded-lg border p-4">
          <h2 className="font-medium">Tengo un código</h2>
          <JoinGroupForm initialCode={joinCode} />
        </div>
      </div>
    </div>
  );
}
