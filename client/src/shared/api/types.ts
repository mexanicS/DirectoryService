export type ApiEnvelope<T> = {
  result: T | null;
  errorList: unknown[] | null;
  isError: boolean;
  timeGenerated: string;
};

export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
};
