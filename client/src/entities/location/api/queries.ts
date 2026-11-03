import { queryOptions } from "@tanstack/react-query";
import type { PaginationParams } from "@/shared/api";
import { fetchLocations } from "./fetch-locations";

export const locationQueries = {
  all: () => ["locations"] as const,
  list: ({ page = 1, pageSize = 20 }: PaginationParams = {}) =>
    queryOptions({
      queryKey: [...locationQueries.all(), "list", { page, pageSize }],
      queryFn: ({ signal }) => fetchLocations({ page, pageSize, signal }),
    }),
};
