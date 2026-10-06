"use client";

import Link from "next/link";
import { useMobileNavigation } from "@/features/mobile-navigation";
import { routes } from "@/shared/config/routes";

export function Header() {
  const { isOpen, toggle } = useMobileNavigation();

  return (
    <header className="sticky top-0 z-30 flex h-16 items-center gap-3 border-b bg-background/95 px-4 backdrop-blur md:px-6">
      <button
        type="button"
        aria-label={isOpen ? "Закрыть меню" : "Открыть меню"}
        aria-expanded={isOpen}
        aria-controls={isOpen ? "mobile-sidebar" : undefined}
        onClick={toggle}
        className="inline-flex size-10 items-center justify-center rounded-md border hover:bg-accent focus-visible:outline-2 focus-visible:outline-ring md:hidden"
      >
        {isOpen ? (
          <svg
            aria-hidden="true"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            className="size-5"
          >
            <path d="M5 5l14 14M19 5L5 19" />
          </svg>
        ) : (
          <svg
            aria-hidden="true"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            className="size-5"
          >
            <path d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        )}
      </button>
      <Link
        href={routes.home}
        className="text-base font-semibold tracking-tight hover:text-muted-foreground"
      >
        Directory Service
      </Link>
    </header>
  );
}
