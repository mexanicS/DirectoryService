# Directory Service client

Минимальный фронтенд админки на Next.js App Router, TypeScript, Tailwind CSS и shadcn/ui.

## Запуск

    npm install
    npm run dev

Главная страница: http://localhost:3000.

Проверки:

    npm run lint
    npm run build

## Структура

- src/app — маршруты, корневой layout и глобальные стили.
- src/widgets — блоки интерфейса страниц.
- src/features — пользовательские действия и их состояние.
- src/entities — типы и логика предметных сущностей.
- src/shared/ui — компоненты shadcn/ui.
- src/shared/lib — общие утилиты, включая cn.
- src/shared/config/routes.ts — пути страниц.

Импорты идут от верхних слоёв к нижним: app → widgets → features → entities → shared. Нижний слой не импортирует верхний.

Для добавления компонента shadcn/ui используйте npx shadcn@latest add <component>: пути генерации настроены в components.json на src/shared/ui.
