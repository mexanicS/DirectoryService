import type { PagedResult } from "./types";

export function isPagedResult<T>(
  value: unknown,
  isItem: (item: unknown) => item is T,
): value is PagedResult<T> {
  if (typeof value !== "object" || value === null) return false;
  const result = value as Partial<PagedResult<T>>;

  return Array.isArray(result.items) &&
    result.items.every(isItem) &&
    typeof result.totalCount === "number" &&
    Number.isInteger(result.totalCount) && result.totalCount >= 0 &&
    typeof result.page === "number" &&
    Number.isInteger(result.page) && result.page >= 1 &&
    typeof result.pageSize === "number" &&
    Number.isInteger(result.pageSize) && result.pageSize >= 1;
}
