import type { ReactNode } from "react";
import type { UseQueryResult } from "@tanstack/react-query";
import type { PagedResult } from "@/shared/api";

const buttonClassName =
  "inline-flex min-h-10 items-center justify-center rounded-md border bg-background px-4 py-2 text-sm font-medium transition-colors hover:bg-accent focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring disabled:cursor-not-allowed disabled:opacity-50";

type PagedListProps<T> = {
  title: string;
  description: string;
  loadingTitle: string;
  errorTitle: string;
  emptyTitle: string;
  emptyDescription: string;
  listLabel: string;
  query: UseQueryResult<PagedResult<T>>;
  onPageChange: (page: number) => void;
  children: ReactNode;
};

export function PagedList<T>({
  title, description, loadingTitle, errorTitle, emptyTitle, emptyDescription,
  listLabel, query, onPageChange, children,
}: PagedListProps<T>) {
  const { data, error, isPending, isFetching, refetch } = query;
  const pageCount = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1;

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-muted/30 px-4 py-12">
      <div className="mx-auto w-full max-w-5xl">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-3xl font-semibold tracking-tight">{title}</h1>
          {data && (
            <span className="rounded-full border bg-background px-3 py-1 text-sm text-muted-foreground">
              Всего: {data.totalCount}
            </span>
          )}
        </div>
        <p className="mt-3 text-sm text-muted-foreground">{description}</p>

        {isPending ? (
          <section role="status" aria-live="polite" className="mt-8 rounded-xl border bg-card p-6">
            <h2 className="font-medium">{loadingTitle}</h2>
            <p className="mt-2 text-sm text-muted-foreground">Получаем актуальный список.</p>
            <div aria-hidden="true" className="mt-6 space-y-3 motion-safe:animate-pulse">
              {[1, 2, 3].map((row) => <div key={row} className="h-12 rounded-md bg-muted" />)}
            </div>
          </section>
        ) : (
          <>
            {error && (
              <section role="alert" className="mt-8 rounded-xl border bg-card p-8 text-center">
                <h2 className="text-lg font-semibold">{errorTitle}</h2>
                <p className="mt-3 text-sm text-muted-foreground">{error.message}</p>
                <button
                  type="button"
                  onClick={() => void refetch()}
                  disabled={isFetching}
                  className={buttonClassName + " mt-6"}
                >
                  {isFetching ? "Повторяем…" : "Повторить"}
                </button>
              </section>
            )}
            {data && isFetching && (
              <p role="status" className="mt-4 text-sm text-muted-foreground">Обновляем список…</p>
            )}
            {data?.items.length === 0 ? (
              <section className="mt-8 rounded-xl border bg-card p-12 text-center">
                <h2 className="text-lg font-semibold">{emptyTitle}</h2>
                <p className="mt-3 text-sm text-muted-foreground">{emptyDescription}</p>
              </section>
            ) : data && (
              <section
                aria-label={listLabel}
                aria-busy={isFetching}
                className="mt-8 overflow-hidden rounded-xl border bg-card"
              >
                <div className="overflow-x-auto">{children}</div>
                {pageCount > 1 && (
                  <div className="flex flex-wrap items-center justify-between gap-3 border-t px-5 py-4">
                    <p className="text-sm text-muted-foreground">Страница {data.page} из {pageCount}</p>
                    <div className="flex gap-2">
                      <button
                        type="button"
                        disabled={data.page <= 1 || isFetching}
                        onClick={() => onPageChange(data.page - 1)}
                        className={buttonClassName}
                      >
                        Назад
                      </button>
                      <button
                        type="button"
                        disabled={data.page >= pageCount || isFetching}
                        onClick={() => onPageChange(data.page + 1)}
                        className={buttonClassName}
                      >
                        Далее
                      </button>
                    </div>
                  </div>
                )}
              </section>
            )}
          </>
        )}
      </div>
    </main>
  );
}
