import { ApiError, httpClient, isPagedResult, type PaginationParams } from "@/shared/api";
import type { Location, LocationListResult } from "../model/types";

function isLocation(value: unknown): value is Location {
  if (typeof value !== "object" || value === null) return false;
  const location = value as Partial<Location>;

  return typeof location.id === "string" &&
    typeof location.name === "string" &&
    typeof location.address === "string" &&
    typeof location.createdAt === "string" &&
    Number.isFinite(Date.parse(location.createdAt)) &&
    typeof location.departmentCount === "number" &&
    Number.isInteger(location.departmentCount) && location.departmentCount >= 0;
}

export async function fetchLocations({
  page = 1,
  pageSize = 20,
  signal,
}: PaginationParams & { signal?: AbortSignal } = {}): Promise<LocationListResult> {
  const result = await httpClient.get<unknown>("locations", {
    signal,
    query: { page, pageSize },
  });

  if (!isPagedResult(result, isLocation)) {
    throw new ApiError("Не удалось получить список локаций: некорректный ответ сервера.", 200);
  }

  return result;
}
