export const routes = {
  home: "/",
  locations: "/locations",
  departments: "/departments",
  positions: "/positions",
} as const;

export const sections = {
  locations: {
    href: routes.locations,
    label: "Локации",
    description: "Справочник локаций",
  },
  departments: {
    href: routes.departments,
    label: "Подразделения",
    description: "Структура подразделений",
  },
  positions: {
    href: routes.positions,
    label: "Позиции",
    description: "Справочник позиций",
  },
} as const;

export const navigationItems = Object.values(sections);
