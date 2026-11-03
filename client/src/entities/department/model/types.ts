import type { PagedResult } from "@/shared/api";

export type Department = {
  id: string;
  name: string;
  identifier: string;
  path: string;
  isActive: boolean;
  createdAt: string;
};

export type DepartmentListResult = PagedResult<Department>;
