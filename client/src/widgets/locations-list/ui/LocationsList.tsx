"use client";

import { useEffect, useState } from "react";
import { fetchLocations, type LocationListResult } from "@/entities/location";
import { sections } from "@/shared/config/routes";

const dateFormatter = new Intl.DateTimeFormat("ru-RU", {
  dateStyle: "medium",
});

const buttonClassName =
  "inline-flex min-h-10 items-center justify-center rounded-md border bg-background px-4 py-2 text-sm font-medium transition-colors hover:bg-accent focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring disabled:cursor-not-allowed disabled:opacity-50";

export function LocationsList() {
  const [data, setData] = useState<LocationListResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [requestVersion, setRequestVersion] = useState(0);

  useEffect(() => {
    const controller = new AbortController();

    async function loadLocations() {
      try {
        const result = await fetchLocations({
          page,
          signal: controller.signal,
        });

        if (!controller.signal.aborted) setData(result);
      } catch (cause) {
        if (!controller.signal.aborted) {
          setError(
            cause instanceof Error
              ? cause.message
              : "Не удалось загрузить локации. Попробуйте ещё раз.",
          );
        }
      } finally {
        if (!controller.signal.aborted) setLoading(false);
      }
    }

    void loadLocations();

    return () => controller.abort();
  }, [page, requestVersion]);

  function retry() {
    setError(null);
    setLoading(true);
    setRequestVersion((version) => version + 1);
  }

  function changePage(nextPage: number) {
    setError(null);
    setLoading(true);
    setPage(nextPage);
  }

  const pageCount = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;

  return (
    <main className="min-h-[calc(100vh-4rem)] bg-muted/30 px-4 py-12">
      <div className="mx-auto w-full max-w-5xl">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-3xl font-semibold tracking-tight">
            {sections.locations.label}
          </h1>
          {!loading && !error && data && (
            <span className="rounded-full border bg-background px-3 py-1 text-sm text-muted-foreground">
              Всего: {data.totalCount}
            </span>
          )}
        </div>
        <p className="mt-3 text-sm text-muted-foreground">
          Названия, адреса и связанные подразделения.
        </p>

        {loading ? (
          <section
            role="status"
            aria-live="polite"
            className="mt-8 rounded-xl border bg-card p-6"
          >
            <h2 className="font-medium">Загружаем локации</h2>
            <p className="mt-2 text-sm text-muted-foreground">
              Получаем актуальный список.
            </p>
            <div
              aria-hidden="true"
              className="mt-6 space-y-3 motion-safe:animate-pulse"
            >
              {[1, 2, 3].map((row) => (
                <div key={row} className="h-12 rounded-md bg-muted" />
              ))}
            </div>
          </section>
        ) : error ? (
          <section
            role="alert"
            className="mt-8 rounded-xl border bg-card p-8 text-center"
          >
            <h2 className="text-lg font-semibold">
              Не удалось загрузить локации
            </h2>
            <p className="mt-3 text-sm text-muted-foreground">{error}</p>
            <button
              type="button"
              onClick={retry}
              className={buttonClassName + " mt-6"}
            >
              Повторить
            </button>
          </section>
        ) : data?.items.length === 0 ? (
          <section className="mt-8 rounded-xl border bg-card p-12 text-center">
            <h2 className="text-lg font-semibold">Локаций пока нет</h2>
            <p className="mt-3 text-sm text-muted-foreground">
              Список появится здесь, когда в систему добавят первую локацию.
            </p>
          </section>
        ) : data ? (
          <section
            aria-label="Список локаций"
            className="mt-8 overflow-hidden rounded-xl border bg-card"
          >
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm">
                <thead className="border-b bg-muted/50 text-muted-foreground">
                  <tr>
                    <th scope="col" className="px-5 py-4 font-medium">
                      Название
                    </th>
                    <th scope="col" className="px-5 py-4 font-medium">
                      Адрес
                    </th>
                    <th scope="col" className="px-5 py-4 font-medium">
                      Создана
                    </th>
                    <th scope="col" className="px-5 py-4 font-medium">
                      Подразделения
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y">
                  {data.items.map((location) => (
                    <tr key={location.id}>
                      <td className="min-w-52 px-5 py-4">
                        <div className="font-medium">{location.name}</div>
                        <div className="mt-1 text-xs text-muted-foreground">
                          {location.id}
                        </div>
                      </td>
                      <td className="min-w-52 px-5 py-4">{location.address}</td>
                      <td className="whitespace-nowrap px-5 py-4">
                        <time dateTime={location.createdAt}>
                          {dateFormatter.format(new Date(location.createdAt))}
                        </time>
                      </td>
                      <td className="px-5 py-4 tabular-nums">
                        {location.departmentCount}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            {pageCount > 1 && (
              <div className="flex flex-wrap items-center justify-between gap-3 border-t px-5 py-4">
                <p className="text-sm text-muted-foreground">
                  Страница {data.page} из {pageCount}
                </p>
                <div className="flex gap-2">
                  <button
                    type="button"
                    disabled={data.page <= 1}
                    onClick={() => changePage(data.page - 1)}
                    className={buttonClassName}
                  >
                    Назад
                  </button>
                  <button
                    type="button"
                    disabled={data.page >= pageCount}
                    onClick={() => changePage(data.page + 1)}
                    className={buttonClassName}
                  >
                    Далее
                  </button>
                </div>
              </div>
            )}
          </section>
        ) : null}
      </div>
    </main>
  );
}
