import Link from "next/link";
import { routes } from "@/shared/config/routes";

export default function NotFound() {
  return (
    <main className="flex min-h-[calc(100vh-4rem)] items-center bg-muted/30 px-4 py-12">
      <div className="mx-auto w-full max-w-5xl">
        <div className="max-w-lg rounded-xl border bg-card p-8 shadow-sm">
          <p className="text-sm font-medium text-muted-foreground">
            Ошибка 404
          </p>
          <h1 className="mt-3 text-3xl font-semibold tracking-tight">
            Страница не найдена
          </h1>
          <p className="mt-4 text-sm leading-6 text-muted-foreground">
            Проверьте адрес или вернитесь к разделам Directory Service.
          </p>
          <Link
            href={routes.locations}
            className="mt-8 inline-flex min-h-10 items-center rounded-md bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition-colors hover:bg-primary/90 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
          >
            Перейти к локациям
          </Link>
        </div>
      </div>
    </main>
  );
}
