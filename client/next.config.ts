import type { NextConfig } from "next";
import { API_BASE_PATH, BACKEND_API_URL } from "@/shared/api/config";

const nextConfig: NextConfig = {
  output: "standalone",
  reactCompiler: true,

  async rewrites() {
    if (process.env.NODE_ENV !== "development") {
      return [];
    }

    return [
      {
        source: API_BASE_PATH + "/:path*",
        destination: BACKEND_API_URL + ":path*",
      },
    ];
  },
};

export default nextConfig;