# Проверка доступа к GitHub Packages

Пакеты `MexanicS.*` загружаются из GitHub Packages. Перед первой сборкой нужно
настроить токен пользователя, имеющего доступ к этим пакетам.

## 1. Подготовить токен

В GitHub открой Settings → Developer settings → Personal access tokens →
Tokens (classic), либо перейди на [страницу создания PAT classic](https://github.com/settings/tokens/new).

- Тип токена: **Personal access token (classic)**.
- Название, например: `DirectoryService local restore`.
- Разрешение: **`read:packages`**. Для скачивания не нужны права публикации
  `write:packages` или удаления `delete:packages`.
- Владелец токена должен иметь доступ к нужным пакетам `MexanicS.*`.
- Если доступ управляется организацией с SSO, проверь необходимость авторизации
  токена для этой организации.
- Сохрани токен у себя. Вводить его нужно в скрытый запрос локального терминала,
  а не в сообщение чата или аргумент командной строки.

Можно использовать существующий действующий PAT classic с нужными разрешениями.

Основание: [GitHub Packages NuGet registry](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).

## 2. Выполнить проверку restore

Эта дополнительная проверка restore использует установленный .NET SDK 10.
Для запуска проекта через Compose она не требуется: достаточно Docker с Compose
и доступа к пакетам. Скопируйте корневой `.env.example` в `.env`, заполните
`GITHUB_USERNAME` и `GITHUB_PACKAGES_TOKEN`, затем выполните `docker compose up`.
Docker build использует эти значения для restore внутри SDK-stage.

Открой PowerShell в корне репозитория и выполни, заменив
`YOUR_GITHUB_LOGIN` на логин владельца токена:

```powershell
& './DirectoryService/scripts/restore-with-github-packages.ps1' -GitHubUsername YOUR_GITHUB_LOGIN
```

Когда появится `GitHub PAT classic with read:packages (hidden input)`, вставь токен
и нажми Enter. Скрипт использует существующий источник `github` из
`DirectoryService/nuget.config` относительно корня репозитория.

Для каждого запуска скрипт создаёт новый каталог внутри
`DirectoryService/artifacts/nuget-restore-check`. Пакеты,
HTTP/plugin cache и файлы restore изолированы от обычной рабочей сборки. NuGet
получает только конфигурацию репозитория и заново разрешает зависимости solution.

Токен передаётся через `NuGetPackageSourceCredentials_github` в окружении процесса
на время проверки. После завершения прежние значения окружения восстанавливаются.
Токен не записывается в NuGet config и не передаётся аргументом команды.
Это соответствует [механизму credentials NuGet](https://learn.microsoft.com/en-us/nuget/consume-packages/consuming-packages-authenticated-feeds#credentials-in-environment-variables).

Успешный результат:

```text
Restore succeeded with the repository config and a fresh package directory.
```

Каталог проверки сохраняется для диагностики. Обычные `bin`/`obj` проекта и
пользовательский cache не используются как место вывода этой проверки.

## 3. Если проверка завершилась ошибкой

- **401**: проверь тип токена, срок действия, правильность ввода и `read:packages`.
- **403**: проверь права владельца токена на пакеты и требования организации/SSO.
- **NU1101/NU1102**: проверь наличие пакетов и требуемых версий `0.1.0` в feed.
- Ошибка DNS/сети: проверь доступ к `api.nuget.org` и `nuget.pkg.github.com`.

Для совместного разбора достаточно кода ошибки и последних строк обычного вывода.
Не включай диагностический вывод MSBuild (`-v:diag`) при передаче логов с credentials
в окружении: такой вывод может содержать значения переменных.

## Использование с Docker Compose

Токен подключён к Docker build через
[BuildKit secrets](https://docs.docker.com/build/building/secrets/) только на шаге
restore. PowerShell-проверка сама по себе не заполняет корневой `.env` для Compose.
