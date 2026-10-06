import type { Metadata } from "next";
import Link from "next/link";
import { routes } from "@/shared/config/routes";

export const metadata: Metadata = {
  title: "Разделы | Directory Service",
};

const sections = [
  {
    title: "Локации",
    description: "Справочник локаций",
    href: routes.locations,
  },
  {
    title: "Подразделения",
    description: "Структура подразделений",
    href: routes.departments,
  },
  {
    title: "Позиции",
    description: "Справочник позиций",
    href: routes.positions,
  },
] as const;

export default function Home() {
  return (
    <main className="min-h-screen bg-muted/30 px-4 py-16">
      <div className="mx-auto w-full max-w-5xl">
        <p className="text-sm font-medium text-muted-foreground">
          Directory Service
        </p>
        <h1 className="mt-3 text-4xl font-semibold tracking-tight">Разделы</h1>
        <p className="mt-3 text-base text-muted-foreground">
          Выберите справочник, с которым хотите работать.
        </p>

        <div className="mt-10 grid gap-4 sm:grid-cols-3">
          {sections.map((section) => (
            <Link
              key={section.href}
              href={section.href}
              className="group rounded-xl border bg-card p-6 shadow-sm transition-colors hover:border-foreground/30 hover:bg-accent focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
            >
              <h2 className="text-lg font-semibold">{section.title}</h2>
              <p className="mt-2 text-sm text-muted-foreground">
                {section.description}
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
