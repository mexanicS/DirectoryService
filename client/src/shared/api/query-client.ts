import { QueryClient } from "@tanstack/react-query";
import { ApiError } from "./api-error";

export function createQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 60_000,
        refetchOnWindowFocus: false,
        retry: (failureCount, error) =>
          failureCount < 2 && error instanceof ApiError && error.retryable,
      },
    },
  });
}
