import { ApiError, httpClient, isPagedResult, type PaginationParams } from "@/shared/api";
import type { Department, DepartmentListResult } from "../model/types";

function isDepartment(value: unknown): value is Department {
  if (typeof value !== "object" || value === null) return false;
  const department = value as Partial<Department>;

  return typeof department.id === "string" &&
    typeof department.name === "string" &&
    typeof department.identifier === "string" &&
    typeof department.path === "string" &&
    typeof department.isActive === "boolean" &&
    typeof department.createdAt === "string" &&
    Number.isFinite(Date.parse(department.createdAt));
}

export async function fetchDepartments({
  page = 1,
  pageSize = 20,
  signal,
}: PaginationParams & { signal?: AbortSignal } = {}): Promise<DepartmentListResult> {
  const result = await httpClient.get<unknown>("departments", {
    signal,
    query: { "Pagination.Page": page, "Pagination.PageSize": pageSize },
  });

  if (!isPagedResult(result, isDepartment)) {
    throw new ApiError("Не удалось получить список подразделений: некорректный ответ сервера.", 200);
  }

  return result;
}
