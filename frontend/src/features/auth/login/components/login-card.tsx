"use client";

import { useActionState, useState } from "react";
import { loginAction } from "@/src/features/auth/login/actions/login";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/src/shared/components/ui/card";
import { Button } from "@/src/shared/components/ui/button";
import { Separator } from "@/src/shared/components/ui/separator";
import { Input } from "@/src/shared/components/ui/input";
import {
  Field,
  FieldDescription,
  FieldLabel,
} from "@/src/shared/components/ui/field";
import { LoginActionState } from "@/src/features/auth/login/types/login-action-state";
import Link from "next/link";

const initialState: LoginActionState = { status: "idle" };

export default function LoginCard() {
  const [email, setEmail] = useState("");
  const [state, formAction, pending] = useActionState(
    loginAction,
    initialState,
  );
  const emailError = state.status === "error" && !!state.errors.email;
  const passwordError = state.status === "error" && !!state.errors.password;
  return (
    <Card className="w-full max-w-sm">
      <CardHeader>
        <CardTitle>Login to your account</CardTitle>
        <CardDescription>
          Enter your email below to login to your account
        </CardDescription>
        <CardAction>
          <Button variant="link">
            <Link href="/register"> Sign up</Link>
          </Button>
        </CardAction>
      </CardHeader>
      <CardContent>
        <form action={formAction}>
          <div className="flex flex-col gap-6">
            <Field className="grid gap-2" data-invalid={emailError}>
              <FieldLabel htmlFor="email">Email</FieldLabel>
              <Input
                id="email"
                name="email"
                type="email"
                placeholder="m@example.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                aria-invalid={emailError}
                required
              />
              {emailError && (
                <FieldDescription>{state.errors.email}</FieldDescription>
              )}
            </Field>
            <Field className="grid gap-2" data-invalid={passwordError}>
              <div className="flex items-center">
                <FieldLabel htmlFor="password">Password</FieldLabel>
                <a
                  href="#"
                  className="ml-auto inline-block text-sm underline-offset-4 hover:underline"
                  tabIndex={-1}
                >
                  Forgot your password?
                </a>
              </div>
              <Input
                id="password"
                type="password"
                name="password"
                aria-invalid={passwordError}
                required
              />
              {passwordError && (
                <FieldDescription className="text-sm text-destructive">
                  {state.errors.password}
                </FieldDescription>
              )}
            </Field>
          </div>

          {state.status === "error" && state.message && (
            <div className="my-2">
              <p className="text-sm text-destructive">{state.message}</p>
            </div>
          )}
          <Separator className="my-2" />
          <div className="flex-col gap-2 my-2 space-y-1">
            <Button type="submit" className="w-full" disabled={pending}>
              {pending ? "Cargando..." : "Login"}
            </Button>
            {/* <Button variant="outline" className="w-full">
              Login with Google
            </Button> */}
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
