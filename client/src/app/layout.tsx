import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import { MobileNavigationProvider } from "@/features/mobile-navigation";
import { Header } from "@/widgets/header";
import { Sidebar } from "@/widgets/sidebar";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Directory Service",
  description: "Административный интерфейс Directory Service",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="ru"
      className={geistSans.variable + " " + geistMono.variable + " h-full antialiased"}
    >
      <body className="min-h-full">
        <MobileNavigationProvider>
          <Header />
          <div className="flex min-h-[calc(100vh-4rem)]">
            <Sidebar />
            <div className="min-w-0 flex-1">{children}</div>
          </div>
        </MobileNavigationProvider>
      </body>
    </html>
  );
}
