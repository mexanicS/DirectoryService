import Link from "next/link";
import { routes } from "@/shared/config/routes";

type SectionPlaceholderProps = {
  title: string;
};

export function SectionPlaceholder({ title }: SectionPlaceholderProps) {
  return (
    <main className="min-h-[calc(100vh-4rem)] bg-muted/30 px-4 py-12">
      <div className="mx-auto w-full max-w-5xl">
        <Link
          href={routes.home}
          className="text-sm text-muted-foreground underline-offset-4 hover:text-foreground hover:underline"
        >
          Все разделы
        </Link>
        <h1 className="mt-6 text-3xl font-semibold tracking-tight">{title}</h1>
        <section
          aria-label="Содержимое раздела"
          className="mt-8 flex min-h-72 items-center justify-center rounded-xl border border-dashed bg-card p-6 text-center"
        >
          <p className="text-sm text-muted-foreground">
            Здесь появится содержимое раздела.
          </p>
        </section>
      </div>
    </main>
  );
}
