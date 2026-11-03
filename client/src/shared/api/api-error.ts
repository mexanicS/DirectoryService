import type { ApiErrorDetail } from "./types";

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly errors: ApiErrorDetail[] = [],
  ) {
    super(message);
    this.name = "ApiError";
  }

  get retryable() {
    // Backend ErrorType.FAILURE = 2; other types are business errors.
    return this.status === 0 ||
      (this.status >= 500 && this.errors.every((error) => error.type === 2));
  }
}
