"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { locationQueries } from "@/entities/location";
import { sections } from "@/shared/config/routes";
import { PagedList } from "@/shared/ui/paged-list";

const dateFormatter = new Intl.DateTimeFormat("ru-RU", { dateStyle: "medium" });

export function LocationsList() {
  const [page, setPage] = useState(1);
  const query = useQuery(locationQueries.list({ page }));

  return (
    <PagedList
      title={sections.locations.label}
      description="Названия, адреса и связанные подразделения."
      loadingTitle="Загружаем локации"
      errorTitle="Не удалось загрузить локации"
      emptyTitle="Локаций пока нет"
      emptyDescription="Список появится здесь, когда в систему добавят первую локацию."
      listLabel="Список локаций"
      query={query}
      onPageChange={setPage}
    >
      <table className="w-full text-left text-sm">
        <thead className="border-b bg-muted/50 text-muted-foreground">
          <tr>
            <th scope="col" className="px-5 py-4 font-medium">Название</th>
            <th scope="col" className="px-5 py-4 font-medium">Адрес</th>
            <th scope="col" className="px-5 py-4 font-medium">Создана</th>
            <th scope="col" className="px-5 py-4 font-medium">Подразделения</th>
          </tr>
        </thead>
        <tbody className="divide-y">
          {query.data?.items.map((location) => (
            <tr key={location.id}>
              <td className="min-w-52 px-5 py-4">
                <div className="font-medium">{location.name}</div>
                <div className="mt-1 text-xs text-muted-foreground">{location.id}</div>
              </td>
              <td className="min-w-52 px-5 py-4">{location.address}</td>
              <td className="whitespace-nowrap px-5 py-4">
                <time dateTime={location.createdAt}>
                  {dateFormatter.format(new Date(location.createdAt))}
                </time>
              </td>
              <td className="px-5 py-4 tabular-nums">{location.departmentCount}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </PagedList>
  );
}
