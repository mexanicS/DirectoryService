export type ApiErrorDetail = {
  code: string;
  message: string;
  type: number;
  invalidField?: string | null;
};

export type ApiEnvelope<T> = {
  result: T | null;
  errorList: ApiErrorDetail[] | null;
  isError: boolean;
  timeGenerated: string;
};

export type PaginationParams = {
  page?: number;
  pageSize?: number;
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
};
