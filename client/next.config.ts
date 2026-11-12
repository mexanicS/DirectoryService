import type { NextConfig } from "next";
import { API_BASE_PATH } from "@/shared/api/config";

const nextConfig: NextConfig = {
  output: "standalone",
  reactCompiler: true,

  async rewrites() {
    if (process.env.NODE_ENV !== "development") {
      return [];
    }

    const backendApiUrl = (
      process.env.NEXT_PUBLIC_API_URL || "http://localhost:5128/api/"
    ).replace(/\/+$/, "") + "/";

    return [
      {
        source: API_BASE_PATH + "/:path*",
        destination: backendApiUrl + ":path*",
      },
    ];
  },
};

export default nextConfig;
