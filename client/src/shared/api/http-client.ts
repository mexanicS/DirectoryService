import { API_BASE_PATH } from "./config";

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
    throw new Error("Не удалось связаться с сервером. Попробуйте ещё раз.");
  }

  if (!response.ok) {
    throw new Error(
      response.status >= 500
        ? "Сервис временно недоступен. Попробуйте ещё раз."
        : "Не удалось выполнить запрос (код " + response.status + ").",
    );
  }

  try {
    return (await response.json()) as T;
  } catch (error) {
    if (options.signal?.aborted) throw error;
    throw new Error("Сервер вернул некорректный ответ.");
  }
}

export const httpClient = { get };
