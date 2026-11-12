# Directory Service

Система для описания того, какие отделы есть в компании, где они расположены и какие должности в них бывают.

## Локальный запуск через Docker Compose

Нужны актуальный Docker с работающим Linux Engine и поддержкой BuildKit secrets,
Docker Compose 2.23.1 или новее и доступ к интернету для скачивания образов и зависимостей.
Локальные .NET SDK, Node.js и Postgres не требуются.

Команды выполняются из корня репозитория: здесь находятся `docker-compose.yml`,
`.env.example`, каталоги `DirectoryService`, `client` и `nginx`.

### Первый запуск

1. Создайте локальный файл настроек:

   ```sh
   cp .env.example .env
   ```

   В PowerShell можно использовать `Copy-Item .env.example .env`.

2. Заполните в `.env` доступ к GitHub Packages:

   ```dotenv
   GITHUB_USERNAME=YOUR_GITHUB_LOGIN
   GITHUB_PACKAGES_TOKEN=YOUR_PAT
   ```

   Пакеты `MexanicS.*` загружаются из GitHub Packages. Нужен PAT classic с правом
   `read:packages`; владелец токена должен иметь доступ к этим пакетам.
   Если организация требует SSO, авторизуйте токен для неё.
   [Инструкция GitHub](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).

   Токен передаётся сборке backend как BuildKit secret на шаге restore.
   Настоящий `.env` игнорируется Git; в репозитории хранится `.env.example`.
   Не передавайте токен в Dockerfile или аргументе `--build-arg`.

3. Запустите проект:

   ```sh
   docker compose up
   ```

   Первый запуск скачивает образы и зависимости, собирает приложения, создаёт БД
   и автоматически применяет миграции. Логи отображаются в терминале.

4. Откройте [http://localhost/](http://localhost/).

Для запуска в фоне с ожиданием готовности:

```sh
docker compose up --detach --wait --wait-timeout 180
```

Это локальный стенд без TLS и production-настроек.

### Как устроен стенд

Основные сервисы: `postgres`, `backend`, `frontend`, `nginx`, `seq`.
Только nginx публикует порт на host, с привязкой к `127.0.0.1`.

- `/api/*` направляется на backend с сохранением `/api`.
- Остальные пути приложения, включая `/_next/*`, направляются на frontend.
- Postgres и приложения обращаются друг к другу по Docker service names.
- Backend запускается после healthy Postgres; до запуска HTTP применяет миграции.
- Nginx ждёт healthy backend и frontend.
- Ошибка миграций выводится в логи и завершает backend с ненулевым кодом.

Backend собирается в SDK-stage, запускается в ASP.NET runtime-stage.
Frontend собирается через `npm ci` и production build, затем запускает готовый
Next.js standalone server. При старте контейнеров сборка приложений не выполняется.

### Проверка и логи

```sh
docker compose ps
docker compose logs --tail 100 backend
docker compose logs --follow backend
```

У Postgres, backend, frontend и nginx настроены healthchecks. Остальные сервисы
могут отображаться как running без healthcheck. Вывод вида `5432/tcp` в `ps`
обозначает порт контейнера; публикация на host выглядит как `127.0.0.1:80->80/tcp`.

Проверка API в PowerShell:

```powershell
curl.exe -i http://localhost/api/locations
```

В Linux/macOS используйте `curl -i http://localhost/api/locations`.
На новой БД текущий API возвращает HTTP 200 и JSON с пустым `items`.

### Мониторинг

Prometheus и Grafana включаются дополнительным профилем:

```sh
docker compose --profile monitoring up --detach --wait --wait-timeout 180
```

- [Grafana](http://localhost/grafana/)
- [Prometheus](http://localhost/prometheus/)

Логин и пароль Grafana берутся из `GRAFANA_ADMIN_USER` и `GRAFANA_ADMIN_PASSWORD`
в `.env`. При первом создании volume Grafana автоматически загружает datasource
Prometheus и dashboard **DirectoryService Overview**.

Prometheus собирает метрики по `http://backend:8080/metrics` при стандартном
`BACKEND_PORT=8080`; адрес подставляется из `.env`. Для этого локального режима
используется `ASPNETCORE_ENVIRONMENT=Development`, в котором включён endpoint метрик.

В Grafana выбирайте актуальный период, например **Last 15 minutes**, и нажимайте
Refresh. Сбор метрик выполняется раз в 15 секунд. Для графиков частоты и задержек
отправьте несколько API-запросов и дождитесь нескольких сборов. После удаления
volumes история метрик начинается заново.

### Изменения кода и настроек

После изменения исходников пересоберите образы:

```sh
docker compose up --detach --build --wait --wait-timeout 180
```

Если используете мониторинг, добавляйте `--profile monitoring` перед `up`.
Логи backend всегда доступны через `docker compose logs backend`.

Настройки находятся в корневом `.env`. Например, чтобы изменить внешний адрес
на `http://localhost:8088/`, задайте `HTTP_PORT=8088` и повторите команду `up`.
Если мониторинг включён, повторите её с профилем, чтобы обновить URL его сервисов.

`POSTGRES_PORT`, `BACKEND_PORT`, `FRONTEND_PORT` — порты внутри Docker-сети.
`HTTP_PORT` — единственный опубликованный порт.

### Остановка и данные

Остановить основной стенд и удалить его контейнеры и сеть, сохранив данные:

```sh
docker compose down
```

Если включали мониторинг, используйте ту же команду с профилем:

```sh
docker compose --profile monitoring down
```

Удалить также данные Postgres, Seq, Prometheus и Grafana:

```sh
docker compose --profile monitoring down --volumes
```

Следующий запуск создаст volumes заново и применит миграции к новой БД.
Обычный `down` без `--volumes` сохраняет данные между запусками.

Смена `POSTGRES_PASSWORD` в `.env` не меняет пароль уже инициализированной БД
в существующем volume. Для сохранения данных измените пароль в самой БД
согласованно с настройками приложения. Для сброса локальных данных используйте
`down --volumes`, затем запустите стенд с новым паролем.
Учётные данные администратора Grafana также инициализируются при первом создании
её хранилища; смена переменных не заменяет пароль существующего пользователя.

Проверен сценарий новой БД с текущей миграцией `20261111181222_Initial`.
Обновление БД со старой историей миграций требует отдельного пути обновления.

### Диагностика

- Ошибки restore 401/403: проверьте GitHub login, PAT classic, `read:packages`,
  доступ к пакетам и требования SSO. Для Docker-запуска не требуется выполнять
  `dotnet restore` на host.
- Backend exited или nginx не запускается: выполните
  `docker compose logs --tail 100 backend` и `docker compose ps --all`.
- Порт занят: измените `HTTP_PORT` в `.env` или остановите стенд, занимающий порт.
- Grafana показывает No data: проверьте период времени и target `directory-service`
  в Prometheus; диагностический запрос `up{job="directory-service"}` должен вернуть 1.

Старый вариант инфраструктуры для backend, запускаемого на host через `dotnet run`,
сохранён в `DirectoryService/docker-compose.legacy.yml`.
Подробнее: [мониторинг](DirectoryService/monitoring/README.md),
[план и результаты проверок](DirectoryService/docs/docker-compose-local-plan.md).
