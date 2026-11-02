export const API_BASE_PATH = "/api";

export const BACKEND_API_URL = (
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5128/api/"
).replace(/\/+$/, "") + "/";
