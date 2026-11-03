import { ApiError } from "./api-error";
import { API_BASE_PATH } from "./config";
import type { ApiEnvelope, ApiErrorDetail } from "./types";

type GetOptions = {
  signal?: AbortSignal;
  query?: Record<string, string | number>;
};

async function get<T>(path: string, options: GetOptions = {}): Promise<T> {
  const endpoint = path.replace(/^\/+|\/+$/g, "");
  const params = new URLSearchParams();

  for (const [key, value] of Object.entries(options.query ?? {})) {
    params.set(key, String(value));
  }

  const query = params.size > 0 ? "?" + params.toString() : "";
  const url = API_BASE_PATH + "/" + endpoint + query;
  let response: Response;

  try {
    response = await fetch(url, {
      method: "GET",
      headers: { Accept: "application/json" },
      cache: "no-store",
      signal: options.signal,
    });
  } catch (error) {
    if (options.signal?.aborted) throw error;
    throw new ApiError("Не удалось связаться с сервером. Попробуйте ещё раз.", 0);
  }

  let envelope: ApiEnvelope<T> | null = null;

  try {
    envelope = await response.json();
  } catch (error) {
    if (options.signal?.aborted) throw error;
    if (response.ok) {
      throw new ApiError("Сервер вернул некорректный ответ.", response.status);
    }
  }

  const errors = Array.isArray(envelope?.errorList)
    ? envelope.errorList.filter((error): error is ApiErrorDetail =>
        error !== null &&
        typeof error === "object" &&
        typeof error.code === "string" &&
        typeof error.message === "string" &&
        typeof error.type === "number",
      )
    : [];

  if (!response.ok || envelope?.isError || errors.length > 0) {
    const fallback = response.status >= 500
      ? "Сервис временно недоступен. Попробуйте ещё раз."
      : "Не удалось выполнить запрос (код " + response.status + ").";

    throw new ApiError(
      errors.map((error) => error.message).join(" ") || fallback,
      response.status,
      errors,
    );
  }

  if (envelope?.isError !== false || envelope.result == null) {
    throw new ApiError("Сервер вернул некорректный ответ.", response.status);
  }

  return envelope.result;
}

export const httpClient = { get };
