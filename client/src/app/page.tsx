import type { Metadata } from "next";
import Link from "next/link";
import { navigationItems } from "@/shared/config/routes";

export const metadata: Metadata = {
  title: "Разделы | Directory Service",
};

export default function Home() {
  return (
    <main className="min-h-[calc(100vh-4rem)] bg-muted/30 px-4 py-16">
      <div className="mx-auto w-full max-w-5xl">
        <p className="text-sm font-medium text-muted-foreground">
          Directory Service
        </p>
        <h1 className="mt-3 text-4xl font-semibold tracking-tight">Разделы</h1>
        <p className="mt-3 text-base text-muted-foreground">
          Выберите справочник, с которым хотите работать.
        </p>

        <div className="mt-10 grid gap-4 sm:grid-cols-3">
          {navigationItems.map(({ href, label, description }) => (
            <Link
              key={href}
              href={href}
              className="group rounded-xl border bg-card p-6 shadow-sm transition-colors hover:border-foreground/30 hover:bg-accent focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
            >
              <h2 className="text-lg font-semibold">{label}</h2>
              <p className="mt-2 text-sm text-muted-foreground">
                {description}
              </p>
              <span
                aria-hidden="true"
                className="mt-8 block text-xl transition-transform group-hover:translate-x-1"
              >
                →
              </span>
            </Link>
          ))}
        </div>
      </div>
    </main>
  );
}
