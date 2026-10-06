"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useMobileNavigation } from "@/features/mobile-navigation";
import { navigationItems } from "@/shared/config/routes";
import { cn } from "@/shared/lib/utils";

function isActiveRoute(pathname: string, href: string) {
  return pathname === href || pathname.startsWith(href + "/");
}

function SidebarLinks({
  pathname,
  onNavigate,
}: {
  pathname: string;
  onNavigate?: () => void;
}) {
  return (
    <nav aria-label="Разделы">
      <ul className="space-y-1">
        {navigationItems.map(({ href, label }) => {
          const active = isActiveRoute(pathname, href);

          return (
            <li key={href}>
              <Link
                href={href}
                aria-current={active ? "page" : undefined}
                onClick={onNavigate}
                className={cn(
                  "flex min-h-10 items-center rounded-md border-l-2 px-3 py-2 text-sm font-medium transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-2 focus-visible:outline-ring",
                  active
                    ? "border-primary bg-accent text-accent-foreground"
                    : "border-transparent text-muted-foreground",
                )}
              >
                {label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}

export function Sidebar() {
  const pathname = usePathname();
  const { isOpen, close } = useMobileNavigation();

  return (
    <>
      <aside className="sticky top-16 hidden h-[calc(100vh-4rem)] w-64 shrink-0 self-start border-r bg-background p-4 md:block">
        <p className="mb-4 px-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Разделы
        </p>
        <SidebarLinks pathname={pathname} />
      </aside>

      {isOpen && (
        <>
          <button
            type="button"
            aria-label="Закрыть меню"
            onClick={close}
            className="fixed inset-0 z-40 bg-foreground/40 md:hidden"
          />
          <aside
            id="mobile-sidebar"
            aria-label="Боковое меню"
            className="fixed inset-y-0 left-0 z-50 w-72 max-w-[85vw] border-r bg-background p-4 shadow-xl md:hidden"
          >
            <div className="mb-4 flex items-center justify-between px-3">
              <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                Разделы
              </p>
              <button
                type="button"
                aria-label="Закрыть меню"
                onClick={close}
                className="inline-flex size-9 items-center justify-center rounded-md hover:bg-accent focus-visible:outline-2 focus-visible:outline-ring"
              >
                <span aria-hidden="true" className="text-xl leading-none">
                  ×
                </span>
              </button>
            </div>
            <SidebarLinks pathname={pathname} onNavigate={close} />
          </aside>
        </>
      )}
    </>
  );
}
