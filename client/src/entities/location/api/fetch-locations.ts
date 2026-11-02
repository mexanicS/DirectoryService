import { httpClient, type ApiEnvelope } from "@/shared/api";
import type { Location, LocationListResult } from "../model/types";

type FetchLocationsOptions = {
  page?: number;
  pageSize?: number;
  signal?: AbortSignal;
};

function isLocation(value: unknown): value is Location {
  if (typeof value !== "object" || value === null) return false;

  const location = value as Partial<Location>;

  return (
    typeof location.id === "string" &&
    typeof location.name === "string" &&
    typeof location.address === "string" &&
    typeof location.createdAt === "string" &&
    Number.isFinite(Date.parse(location.createdAt)) &&
    typeof location.departmentCount === "number" &&
    Number.isInteger(location.departmentCount) &&
    location.departmentCount >= 0
  );
}

export async function fetchLocations({
  page = 1,
  pageSize = 20,
  signal,
}: FetchLocationsOptions = {}): Promise<LocationListResult> {
  const envelope = await httpClient.get<ApiEnvelope<LocationListResult>>(
    "locations",
    { signal, query: { page, pageSize } },
  );
  const result = envelope?.result;

  if (
    envelope?.isError ||
      !result ||
      !Array.isArray(result.items) ||
      !result.items.every(isLocation) ||
      !Number.isInteger(result.totalCount) ||
      result.totalCount < 0 ||
      !Number.isInteger(result.page) ||
      result.page < 1 ||
      !Number.isInteger(result.pageSize) ||
      result.pageSize < 1
  ) {
    throw new Error("Не удалось получить список локаций: некорректный ответ сервера.");
  }

  return {
    items: result.items,
    totalCount: result.totalCount,
    page: result.page,
    pageSize: result.pageSize,
  };
}
