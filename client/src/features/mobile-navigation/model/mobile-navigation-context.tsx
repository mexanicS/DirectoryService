"use client";

import { createContext, useContext, useState, type ReactNode } from "react";

type MobileNavigationContextValue = {
  isOpen: boolean;
  toggle: () => void;
  close: () => void;
};

const MobileNavigationContext =
  createContext<MobileNavigationContextValue | null>(null);

export function MobileNavigationProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [isOpen, setIsOpen] = useState(false);

  return (
    <MobileNavigationContext.Provider
      value={{
        isOpen,
        toggle: () => setIsOpen((open) => !open),
        close: () => setIsOpen(false),
      }}
    >
      {children}
    </MobileNavigationContext.Provider>
  );
}

export function useMobileNavigation() {
  const context = useContext(MobileNavigationContext);

  if (!context) {
    throw new Error("MobileNavigationProvider is missing");
  }

  return context;
}
