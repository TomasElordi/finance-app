"use client";

import { useActionState, useState } from "react";
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
import { RegisterActionState } from "@/src/features/auth/register/types/register-action-state";
import { registerAction } from "@/src/features/auth/register/actions/register";
import Link from "next/link";
import {
  InputGroup,
  InputGroupAddon,
  InputGroupInput,
} from "@/src/shared/components/ui/input-group";
import { EyeIcon, EyeOffIcon } from "lucide-react";

const initialState: RegisterActionState = { status: "idle" };

export default function RegisterCard() {
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [state, formAction, pending] = useActionState(
    registerAction,
    initialState,
  );
  const [showPassword, setShowPassword] = useState(false);
  const nameError = state.status === "error" && !!state.errors.name;
  const emailError = state.status === "error" && !!state.errors.email;
  const passwordError = state.status === "error" && !!state.errors.password;
  const confirmPasswordError =
    state.status === "error" && !!state.errors.confirm_password;
  return (
    <Card className="w-full max-w-sm">
      <CardHeader>
        <CardTitle>Register to your account</CardTitle>
        <CardDescription>
          Enter your email below to Register to your account
        </CardDescription>
        <CardAction>
          <Button variant="link">
            <Link href="/login"> Sign in</Link>
          </Button>
        </CardAction>
      </CardHeader>
      <CardContent>
        <form action={formAction}>
          <div className="flex flex-col gap-4">
            <Field className="grid gap-2" data-invalid={nameError}>
              <FieldLabel htmlFor="name">Name</FieldLabel>
              <Input
                id="name"
                name="name"
                type="name"
                placeholder="John Doe"
                value={name}
                onChange={(e) => setName(e.target.value)}
                aria-invalid={nameError}
                required
              />
              {nameError && (
                <FieldDescription>{state.errors.name}</FieldDescription>
              )}
            </Field>
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
              </div>
              <InputGroup>
                <InputGroupInput
                  id="password"
                  type={showPassword ? "text" : "password"}
                  name="password"
                  aria-invalid={passwordError}
                  required
                />
                <InputGroupAddon align="inline-end">
                  <Button
                    variant="ghost"
                    onClick={(e) => {
                      e.preventDefault();
                      setShowPassword(!showPassword);
                    }}
                  >
                    {!showPassword ? <EyeIcon /> : <EyeOffIcon />}
                  </Button>
                </InputGroupAddon>
              </InputGroup>
              {passwordError && (
                <FieldDescription className="text-sm text-destructive">
                  {state.errors.password}
                </FieldDescription>
              )}
            </Field>
            <Field className="grid gap-2" data-invalid={confirmPasswordError}>
              <div className="flex items-center">
                <FieldLabel htmlFor="confirm_password">
                  Confirm password
                </FieldLabel>
              </div>
              <InputGroup>
                <InputGroupInput
                  id="confirm_password"
                  type={showPassword ? "text" : "password"}
                  name="confirm_password"
                  aria-invalid={confirmPasswordError}
                  required
                />
                <InputGroupAddon align="inline-end">
                  <Button
                    variant="ghost"
                    onClick={() => setShowPassword(!showPassword)}
                  >
                    {!showPassword ? <EyeIcon /> : <EyeOffIcon />}
                  </Button>
                </InputGroupAddon>
              </InputGroup>
              {confirmPasswordError && (
                <FieldDescription className="text-sm text-destructive">
                  {state.errors.confirm_password}
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
              {pending ? "Cargando..." : "Register"}
            </Button>
            {/* <Button variant="outline" className="w-full">
              Register with Google
            </Button> */}
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
