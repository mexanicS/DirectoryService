import type { PagedResult } from "@/shared/api";

export type Location = {
  id: string;
  name: string;
  address: string;
  createdAt: string;
  departmentCount: number;
};

export type LocationListResult = PagedResult<Location>;
