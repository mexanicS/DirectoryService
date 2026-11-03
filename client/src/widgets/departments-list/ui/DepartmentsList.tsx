"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { departmentQueries } from "@/entities/department";
import { sections } from "@/shared/config/routes";
import { PagedList } from "@/shared/ui/paged-list";

const dateFormatter = new Intl.DateTimeFormat("ru-RU", { dateStyle: "medium" });

export function DepartmentsList() {
  const [page, setPage] = useState(1);
  const query = useQuery(departmentQueries.list({ page }));

  return (
    <PagedList
      title={sections.departments.label}
      description="Названия, идентификаторы и структура подразделений."
      loadingTitle="Загружаем подразделения"
      errorTitle="Не удалось загрузить подразделения"
      emptyTitle="Подразделений пока нет"
      emptyDescription="Список появится здесь, когда в систему добавят первое подразделение."
      listLabel="Список подразделений"
      query={query}
      onPageChange={setPage}
    >
      <table className="w-full text-left text-sm">
        <thead className="border-b bg-muted/50 text-muted-foreground">
          <tr>
            <th scope="col" className="px-5 py-4 font-medium">Название</th>
            <th scope="col" className="px-5 py-4 font-medium">Идентификатор</th>
            <th scope="col" className="px-5 py-4 font-medium">Путь</th>
            <th scope="col" className="px-5 py-4 font-medium">Статус</th>
            <th scope="col" className="px-5 py-4 font-medium">Создано</th>
          </tr>
        </thead>
        <tbody className="divide-y">
          {query.data?.items.map((department) => (
            <tr key={department.id}>
              <td className="min-w-52 px-5 py-4">
                <div className="font-medium">{department.name}</div>
                <div className="mt-1 text-xs text-muted-foreground">{department.id}</div>
              </td>
              <td className="px-5 py-4">{department.identifier}</td>
              <td className="min-w-52 max-w-xs break-all px-5 py-4">{department.path}</td>
              <td className="whitespace-nowrap px-5 py-4">
                {department.isActive ? "Активно" : "Неактивно"}
              </td>
              <td className="whitespace-nowrap px-5 py-4">
                <time dateTime={department.createdAt}>
                  {dateFormatter.format(new Date(department.createdAt))}
                </time>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </PagedList>
  );
}
