import { queryOptions } from "@tanstack/react-query";
import type { PaginationParams } from "@/shared/api";
import { fetchDepartments } from "./fetch-departments";

export const departmentQueries = {
  all: () => ["departments"] as const,
  list: ({ page = 1, pageSize = 20 }: PaginationParams = {}) =>
    queryOptions({
      queryKey: [...departmentQueries.all(), "list", { page, pageSize }],
      queryFn: ({ signal }) => fetchDepartments({ page, pageSize, signal }),
    }),
};
