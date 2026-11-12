# План локального запуска Directory Service через Docker Compose

Дата: 2026-11-12. Основание: согласованный в чате план.

Итоговый статус: все восемь этапов выполнены и проверены в рабочем checkout.
Запущен обычный проект `directory-service` с профилем monitoring; временные
проекты удалены. Изменения подготовлены к Git-коммиту, коммит не создавался.

## Цель и границы

После клонирования репозитория разработчик выполняет `cp .env.example .env`,
настраивает доступ к GitHub Packages и выполняет `docker compose up`. Для запуска
достаточно Docker с Compose; локальные .NET SDK, Node.js и Postgres не требуются.
Сборка может скачивать образы и зависимости из сети.

Согласованное изменение исходного требования, 2026-11-12: оставляем GitHub Packages
с личным токеном и допускаем дополнительный шаг его настройки перед первым
запуском. В дальнейшем настройка потребуется также при истечении или отзыве токена.

Единственный внешний адрес — `http://localhost/` с настраиваемым портом nginx.
Пути `/api/*` направляются на backend, остальные — на frontend. Postgres доступен
в Docker-сети и хранит данные в именованном volume.

В задачу не входят production deployment, TLS, production secrets, multi-host и CI.

## Принятые решения

- Канонический Compose и общий `.env.example` размещаются в корне репозитория:
  `F:/Work/DirectoryService`. Backend находится в подкаталоге `DirectoryService`,
  frontend — в `client`. У каждого приложения отдельный build context.
- Основные сервисы: `postgres`, `backend`, `frontend`, `nginx`, `seq`.
- Seq сохраняется для текущей интеграции логов; backend также пишет в stdout.
- Prometheus и Grafana переносятся в дополнительный профиль мониторинга.
- Приложения обращаются к зависимостям по service name, а не через `localhost`.
- Порты на host публикует только nginx, с локальной привязкой к `127.0.0.1`.
- Миграции выполняются через `Database.MigrateAsync()` перед запуском backend.
  Эта возможность включается явно настройкой Compose.
- Frontend запускается из standalone-сборки Next.js в Node runtime-stage.
- Текущий Next rewrite для обычного dev-запуска сохраняется отдельно от
  маршрутизации API через nginx в Docker.
- Пакеты `MexanicS.*` остаются в GitHub Packages. Используем PAT classic с правом
  `read:packages` от пользователя, имеющего доступ к нужным пакетам. Во время
  Docker build credentials передаются через BuildKit secret только на шаг restore.

## Порядок выполнения

### 1. Обеспечить восстановление зависимостей и актуальность миграций

Статус: выполнен. Весь план завершён, итоговые проверки приведены на этапе 8.

- [x] Проверить источники и зависимости `MexanicS.SharedKernel`,
  `MexanicS.SharedCore`, `MexanicS.SharedFramework` версии `0.1.0`.
- [x] Проверить восстановление без личного NuGet cache и неявных настроек машины.
- [x] Восстановить локальный `dotnet-ef` и проверить версию: используется `10.0.8`.
- [x] Решить доставку пакетов для нового разработчика: сохраняем GitHub Packages
  с токеном. Дополнительный шаг настройки явно согласован пользователем.
- [x] Выбрать способ передачи токена при Docker build: BuildKit secret только
  на шаг restore. Реализация и проверка относятся к этапу 3.
- [x] Проверить расхождения EF-модели и snapshot: сначала команда подтвердила
  изменения, после обновления миграций расхождений нет.
- [x] Актуализировать миграции для создания пустой БД. Пользователь пересоздал
  `Initial` с идентификатором `20261111181222_Initial`, включающим `pg_trgm` и GIN
  индекс `idx_departments_name_trgm`. Добавочная миграция поверх старой не создана.
- [x] Проверить отсутствие pending model changes: пользователь получил
  `No changes have been made to the model since the last migration`.
- [x] Проверить применение миграций к отдельной пустой БД. Не использовать текущую
  рабочую БД для разрушающих проверок.

Изменение истории: прежняя локальная `20260713113534_Initial` заменена пользователем
на новую `Initial`. Проверяем сценарий новой пустой БД. Совместимость с базами,
в которых уже применена прежняя миграция, этой проверкой не подтверждается;
для обновления такой БД нужен отдельный согласованный путь.

Готовность этапа: определён воспроизводимый способ restore, миграции соответствуют
модели и проверены на пустой БД. Непроверенные условия остаются явно отмеченными.

#### Результаты первичной диагностики, 2026-11-12

- Три пакета `MexanicS.*` версии `0.1.0` привязаны через package source mapping
  к источнику `github`. Все три уже находятся в пользовательском NuGet cache.
- Анонимный GET `https://nuget.pkg.github.com/mexanicS/index.json` вернул HTTP 401.
- Запросы `.nuspec` версии `0.1.0` всех трёх пакетов в NuGet.org flat container
  вернули HTTP 404. Простая замена источника сейчас не решит задачу.
- В пользовательском NuGet config нет секции credentials; переменные окружения
  с проверенными стандартными именами NuGet/GitHub credentials не обнаружены.
  Значения секретов не выводились и не копировались.
- Метаданные `SharedCore` и `SharedFramework` указывают на репозиторий
  `https://github.com/mexanicS/SharedLibraries`. Это сведения из локального cache;
  доступность и содержимое исходников в удалённом репозитории пока не проверены.
- Пользователь выбрал GitHub Packages с токеном и согласовал дополнительный шаг
  настройки. Источник зависимостей сохраняется. Инструкция для первой проверки:
  `F:/Work/DirectoryService/DirectoryService/docs/github-packages-setup.md`.
- Установлен .NET SDK `10.0.300`, но глобальный `dotnet-ef` имеет версию `8.0.8`.
  В локальном manifest
  `F:/Work/DirectoryService/DirectoryService/.config/dotnet-tools.json` закреплена
  версия `10.0.8`, соответствующая пакетам EF Core проекта. Пользователь успешно
  восстановил инструмент; отдельная проверка `dotnet ef --version` подтвердила
  использование версии `10.0.8` (exit code 0).
- Первоначально Docker Engine был недоступен. Пользователь запустил Docker
  Desktop и подтвердил `docker version`: доступны Client и Server.
- Первоначальное расхождение модели устранено пользователем пересозданием
  `Initial`. Новая миграция и snapshot содержат `ltree`, `pg_trgm` и GIN-индекс
  `idx_departments_name_trgm` с `gin_trgm_ops`. Скриншот пользователя подтверждает
  успешную сборку и отсутствие pending model changes.
- Команда `dotnet ef migrations script 0 20261111181222_Initial --no-build`
  успешно сгенерировала SQL (exit code 0), без применения к БД. Результат:
  `F:/Work/DirectoryService/DirectoryService/artifacts/migrations-validation/initial.sql`.
  SQL содержит создание расширений, доменных таблиц, индексов и запись в
  `__EFMigrationsHistory`. Затем применение к Postgres проверено пользователем.
- Полный restore без прежнего cache завершился успешно за 446,4 секунды
  (около 7 минут 26 секунд), результат подтверждён пользователем скриншотом.
  В каталоге проверки `bd67425184d841a89bf6764f39512395` присутствуют извлечённые
  версии `0.1.0` всех трёх пакетов `MexanicS.*`; `project.assets.json` всех шести
  проектов solution не содержат ошибок restore. Это подтверждает восстановление
  зависимостей. Компиляция затем прошла у пользователя при проверке EF;
  применение миграций к отдельной пустой БД также проверено.
- Подготовлен скрипт `scripts/restore-with-github-packages.ps1`: скрытый ввод PAT,
  credentials только в окружении текущего процесса на время проверки, отдельные
  package/HTTP/plugin cache и build artifacts для каждого запуска. Пользователь
  успешно выполнил проверку с настоящим токеном в локальном терминале.
- Пользователь запустил отдельный контейнер `ds-migrations-check` на `postgres:17`
  с `--rm`, тестовой БД `ds_migrations_check` и портом `127.0.0.1:55432`.
  `pg_isready` подтвердил `accepting connections`.
- `dotnet ef database update` с явной строкой подключения к тестовой БД успешно
  применил `20261111181222_Initial` и завершился сообщением `Done.`.
- Последующие SQL-запросы пользователя подтвердили пять таблиц в схеме
  `DirectoryService`: `department`, `department_locations`, `location`,
  `department_positions`, `positions`; запись `20261111181222_Initial` с
  `ProductVersion = 10.0.8` в `__EFMigrationsHistory`; расширения `ltree`, `pg_trgm`;
  индекс `idx_departments_name_trgm`. Результат подтверждён скриншотом пользователя.
- Пользователь подтвердил очистку временного контейнера `ds-migrations-check`
  после проверки миграций.

### 2. Определить точку запуска и конфигурацию

Статус: выполнен. Общие `.env.example`, `.env` и Compose в корне репозитория
`F:/Work/DirectoryService` созданы. Все пять сервисов описаны, конфигурация
проверена. Старый Compose в папке backend переименован.
На момент начала этапа в корне не было Compose и `.env.example`; существующие
файлы находились в подкаталоге backend. Корневой `.gitignore` содержит правило `.env`.

Начальный набор переменных согласован с принятой схемой: `HTTP_PORT`,
`POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_PORT`,
`BACKEND_PORT`, `FRONTEND_PORT`, `ASPNETCORE_ENVIRONMENT`, `SEQ_URL`,
`GITHUB_USERNAME`, `GITHUB_PACKAGES_TOKEN`, а также существующие настройки
Grafana для дополнительного профиля. Секретный токен заполняется только в
локальном `.env`; в `.env.example` остаётся пустое значение.
Пользователь создал общий шаблон. Проверка подтвердила наличие всех 13 ожидаемых
переменных и пустое значение `GITHUB_PACKAGES_TOKEN` в шаблоне. Затем пользователь
создал локальный корневой `.env` и заполнил GitHub login/PAT. Проверка без вывода
значений подтвердила наличие всех переменных, отсутствие пустых и повторяющихся
полей. `git check-ignore` подтвердил исключение `.env`, а `git ls-files` — что он
не отслеживается. Авторизацию Docker build проверим на этапе 3.

Пользователь создал корневой Compose с именем проекта `directory-service`,
начальным сервисом `postgres`, именованным volume `postgres-data` и общей сетью
`directory-service`. `docker compose config --quiet` успешно проверил файл;
`config --services` показал `postgres`. Postgres использует проверенный образ
`postgres:17`; настройки БД и порт берутся из `.env`. Порт Postgres на host
не публикуется.
Остальные сервисы и настройки добавляем постепенно; `docker compose up` всего
стенда станет рабочим после выполнения следующих этапов.

Пользователь добавил `backend` и объявление build secret в корневой Compose.
Повторная проверка `config --quiet` прошла, `config --services` показал `backend`
и `postgres`. Проверка структуры без вывода значений подтвердила build context
`./DirectoryService`, Dockerfile `Dockerfile`, единственный build arg
`GITHUB_USERNAME` и build secret `github_packages_token` из
`GITHUB_PACKAGES_TOKEN`. Backend получает `ASPNETCORE_ENVIRONMENT`,
`ASPNETCORE_HTTP_PORTS`, `ConnectionStrings__Database` с хостом `postgres` и
`ConnectionStrings__Seq`; подключён к сети `directory-service`, порты на host
не публикует. Проверка конфигурации не означает успешную сборку образа.

Пользователь добавил и сохранил `frontend`. Проверка `config --quiet` прошла;
Compose видит `backend`, `frontend`, `postgres`. Подтверждены build context
`./client`, Dockerfile `Dockerfile`, `NODE_ENV=production`, непустой `PORT`,
`HOSTNAME=0.0.0.0`, общая сеть `directory-service` и отсутствие опубликованных
портов frontend. Документация установленного Next.js подтверждает использование
`PORT`/`HOSTNAME` standalone-сервером. Его Dockerfile пока не создан.

Пользователь добавил `seq`. Проверка `config --quiet` прошла; подтверждены образ
`datalust/seq:2026.1.17526`, `ACCEPT_EULA=Y`, локальный режим
`SEQ_FIRSTRUN_NOAUTHENTICATION=true`, объявление и подключение volume
`seq-data:/data`, общая сеть и отсутствие опубликованных портов Seq.
Backend настроен на `http://seq:5341`. В Compose сейчас четыре сервиса:
`backend`, `frontend`, `postgres`, `seq`.
Параметры и версия проверены по официальной странице образа и списку тегов:
`https://hub.docker.com/r/datalust/seq`,
`https://hub.docker.com/r/datalust/seq/tags`.

Пользователь добавил `nginx` на проверенном официальном образе
`nginx:1.30.6-alpine`. Проверка общего файла `config --quiet` прошла. Все пять
сервисов подключены к сети `directory-service`; только nginx публикует порт.
Единственная публикация: `127.0.0.1:${HTTP_PORT}:80`.
Порты приложений передаются через `BACKEND_PORT`/`FRONTEND_PORT`, фильтр
`NGINX_ENVSUBST_FILTER=BACKEND_PORT|FRONTEND_PORT` сохраняет переменные nginx
`$host` и `$uri` при обработке шаблона. Bind mount шаблона
`./nginx/default.conf.template` подключается read-only с `create_host_path=false`.
Сам шаблон создадим на этапе 7. Настройки nginx подтверждены в общем Compose,
без запуска контейнеров. Проверка синтаксиса не подтверждает готовность всего
стенда: Dockerfile-ы, автомиграции, healthchecks и шаблон proxy ещё впереди.
Документация образа: `https://hub.docker.com/_/nginx`.

Пользователь переименовал прежний Compose в
`F:/Work/DirectoryService/DirectoryService/docker-compose.legacy.yml`.
Проверка подтвердила отсутствие старого имени и наличие нового. Содержимое
прежнего стенда с мониторингом сохранено. Ссылка в `DirectoryService.sln`
обновлена на `../docker-compose.yml`. Документация мониторинга теперь явно
использует `docker compose -f docker-compose.legacy.yml ...` для прежнего стенда.
Проверка `docker compose config --quiet` без явного `-f` из папки backend прошла:
Compose автоматически находит общий файл, видит все пять сервисов и подставляет
окружение. Перенос мониторинга в профиль общего Compose остаётся на этапе 6.

- [x] Создать `.env.example` в корне репозитория.
- [x] Создать локальный `.env` из шаблона и заполнить GitHub login/PAT.
- [x] Создать канонический Compose в корне репозитория (сейчас `postgres`, `backend`,
  `frontend`, `seq`, `nginx`).
- [x] Задать отдельные build context для backend и frontend.
- [x] Вынести настройки БД, внешний и внутренние порты приложений, URL Seq в окружение.
- [x] Передавать backend `ConnectionStrings__Database` с хостом `postgres`,
  `ConnectionStrings__Seq` с адресом `seq`, настройки HTTP listener.
- [x] Передавать frontend `PORT` и `HOSTNAME`; слушать `0.0.0.0`.
- [x] Явно определить `environment`/`env_file`: общий `.env` сам по себе служит
  источником подстановок Compose и не передаёт все значения контейнерам.
- [x] Проверить исключение корневого `.env` из Git: `git check-ignore` подтвердил
  правило. `.env.example` доступен для добавления в Git.
- [x] Устранить неоднозначность с существующим Compose в каталоге backend.

Исключение локальных env-файлов из build context реализуем и проверим через
`.dockerignore` приложений на этапах 3 и 5.

### 3. Собрать backend через multi-stage Dockerfile

Статус: выполнен. Пользователь создал
`F:/Work/DirectoryService/DirectoryService/.dockerignore`; проверено исключение
`bin`/`obj`, локальных env-файлов, IDE-файлов, логов, артефактов и каталогов,
не нужных для публикации backend. На момент начала этапа Dockerfile и ignore-файла
не было.

Подготовлен Dockerfile в
`F:/Work/DirectoryService/DirectoryService/artifacts/backend-image/Dockerfile`.
SDK-stage использует `mcr.microsoft.com/dotnet/sdk:10.0.300-noble`, runtime-stage —
`mcr.microsoft.com/dotnet/aspnet:10.0.8-noble`. Версии соответствуют установленным
SDK/runtime и проверены в Microsoft Container Registry. Сначала копируются NuGet
config, props и пять csproj, затем restore; после копирования `src` выполняется
Release publish с `--no-restore` и `UseAppHost=false`. GitHub token подключается
через `RUN --mount=type=secret` в окружение только restore-команды; username
поступает через build arg. Runtime получает только publish, curl для будущего
HTTP healthcheck и запускает DLL от `APP_UID`.

`docker buildx build --check` успешно проверил подготовленный Dockerfile и
метаданные обоих базовых образов: `Check complete, no warnings found` (exit code 0).
Проверка не выполняла restore/publish, не передавала GitHub token и не собирала
конечный образ. Пользователь скопировал вариант в канонический
`F:/Work/DirectoryService/DirectoryService/Dockerfile`; сравнение подтвердило
полное совпадение содержимого с проверенным вариантом. Общий Compose проходит
`config --quiet`, настоящий `.env` по-прежнему игнорируется Git.

Пользователь успешно выполнил `docker compose --progress plain build backend`.
Restore с BuildKit secret, Release publish и сборка runtime завершились;
результат подтверждён скриншотом `Image directory-service-backend Built`.

Проверен образ `directory-service-backend`, ID
`sha256:fe79b50226b035555a349851ee98f587b8c4afb6adaae94643b943b5d0764655`,
размер около 239 MiB. В метаданных нет переменных GitHub/token/password,
в runtime-history нет команд SDK restore/publish. Entrypoint запускает
`DirectoryService.Presentation.dll`, пользователь — `1654`.

Отдельный одноразовый контейнер без сети и с заменённым entrypoint подтвердил:
DLL приложения присутствует; SDK, `/source`, исходники `.cs`/`.csproj`, каталоги
`bin`/`obj` в `/app` отсутствуют; доступны ASP.NET Core и .NET runtime `10.0.8`,
а также curl. Приложение и его подключение к БД этой проверкой не запускались.

Образ экспортирован и проверен на точное значение настроенного GitHub PAT без
вывода значения: просканировано 26 записей image archive, токен не обнаружен.
Проверены содержимое слоёв и image metadata; временный tar после проверки удалён.
Проверочные скрипты находятся в игнорируемом каталоге `artifacts/backend-image`.

- [x] SDK-stage на .NET 10: файлы проектов и NuGet config → restore → исходники →
  `dotnet publish` в Release.
- [x] Реализовать передачу GitHub Packages credentials через BuildKit secret
  только на шаг restore.
- [x] Проверить отсутствие токена в слоях и runtime-образе.
- [x] Runtime-stage на ASP.NET 10: только runtime и результат publish; запуск DLL.
- [x] Зафиксировать совместимые версии базовых образов; исключить плавающий `latest`.
- [x] Добавить backend `.dockerignore`: `**/bin`, `**/obj`, `.git`, IDE-файлы,
  локальные env-файлы, логи и ненужные артефакты тестов.
- [x] Проверить сборку и содержимое финального образа; SDK и исходников в нём нет.

### 4. Добавить миграции при старте и готовность backend

Статус: выполнен. Пользователь добавил в `Program.cs` применение миграций после
`builder.Build()` и до запуска HTTP/hosted services. Код проверен; Release build
проекта Presentation с `--no-restore` прошёл с нулём ошибок и предупреждений.
При ошибке миграции код пишет Fatal log, вызывает `Log.CloseAndFlushAsync()`
и повторно выбрасывает исключение для ненулевого кода завершения.

Условие: `Database:ApplyMigrations` (по умолчанию false). Пользователь добавил
`Database__ApplyMigrations: "true"` в `backend.environment`; проверка общего
Compose подтвердила валидность и включённый флаг без вывода значений секретов.

Пользователь добавил перед `app.Run()` маршрут `/health/ready`, проверяющий
`dbContext.Database.CanConnectAsync` и возвращающий 200 с `{ status: "ready" }`
при доступной БД или 503 иначе. Проверка кода и повторная Release-сборка прошли:
ноль ошибок, ноль предупреждений. В контейнере HTTP стартует после успешных
миграций, поэтому probe дополнительно проверяет текущую доступность БД.
Документация установленного EF Core подтверждает, что ошибки подключения при
`CanConnectAsync` перехватываются внутри этого метода; сам метод не проверяет
актуальность схемы.

Пользователь выполнил из корня репозитория
`docker compose --project-name ds-backend-check up --detach postgres seq`.
Созданы отдельные volumes и сеть проекта, Postgres/Seq работают. Проверка
`pg_isready` подтвердила готовность Postgres; SQL-запрос подтвердил отсутствие
таблиц домена и `__EFMigrationsHistory` (count = 0) перед запуском backend.
Внешние порты Postgres/Seq не публикуются, флаг автомиграций включён.

Пользователь выполнил
`docker compose --project-name ds-backend-check --progress plain up --detach --build backend`.
Обновлённый образ `ds-backend-check-backend` собран, контейнер запущен.

Положительная проверка пройдена: `/health/ready` вернул HTTP 200 и
`{"status":"ready"}`; SQL подтвердил пять доменных таблиц, запись
`20261111181222_Initial`/`10.0.8` в истории, расширения `ltree`/`pg_trgm` и индекс
`idx_departments_name_trgm`. До старта backend этих таблиц не было, поэтому
проверка подтверждает автоматическое применение миграции. В логах
`Applying database migrations` и `Database migrations applied` предшествуют
`Now listening on`. Backend, Postgres и Seq находятся в состоянии running.
Дополнительный GET `/api/locations` внутри backend вернул HTTP 200, пустой список
и `isError=false`. Это проверка внутреннего backend, не nginx-маршрутизации.

Один существующий интеграционный тест `MetricsEndpointTests` пройден
(`dotnet test --no-restore`, 1 passed, 0 failed). Он использует существующую
фабрику с `EnsureCreatedAsync`, которая продолжает работать с выключенными
по умолчанию автомиграциями вне Compose. Новые тесты для этой проверки не добавлялись.

Пользователь проверил одноразовый backend через
`docker compose --project-name ds-backend-check run --rm --no-deps`, заменив
только для этого контейнера пароль подключения к тестовому Postgres на неверный.
Предоставленный вывод подтвердил Fatal message
`Database migration failed. Backend startup aborted`, ошибку аутентификации
`28P01` внутри `MigrateAsync` и `$LASTEXITCODE = 139` (ненулевой).
После отрицательного теста основной backend по-прежнему возвращает readiness 200;
все три основные тестовые службы running, одноразового контейнера в списке нет.

Следующее действие пользователя — удалить только временный стенд командой
`docker compose --project-name ds-backend-check down --volumes`.
Очистка подтверждена перед Docker-сборкой frontend: контейнеров и volumes
`ds-backend-check` нет. Docker Linux Engine снова доступен (`linux/amd64`).

- [x] До запуска HTTP и hosted services создать DI scope и вызвать `MigrateAsync`.
- [x] Включать автомиграции явной настройкой Compose.
- [x] При ошибке вывести понятный лог, сбросить буфер логов и завершить процесс
  с ненулевым кодом; не скрывать ошибку бесконечными перезапусками.
- [x] Добавить `/health/ready`: успешные миграции до запуска HTTP и доступная БД.
- [x] Учесть существующие интеграционные тесты, использующие `EnsureCreatedAsync`;
  не смешивать этот способ создания схемы с миграциями в одном сценарии.
- [x] Сохранить вывод логов backend в stdout.

### 5. Собрать frontend через standalone Next.js

Статус: выполнен. Прочитаны `client/AGENTS.md` и документация установленного
Next.js 16.3.5 по standalone output и rewrites. Пользователь обновил
`next.config.ts`: проверены
`output: "standalone"`, сохранённый `reactCompiler: true` и возврат пустых rewrites
при `NODE_ENV !== "development"`. Наличие `package-lock.json` подтверждено.
Proxy для `npm run dev` сохранён; production build для Docker будет использовать
маршрутизацию `/api/*` через nginx. HTTP-клиент уже отправляет относительные
запросы `/api/...`; менять его для этого не требуется. Production-сборка конфига
затем успешно выполнена в Docker.

Пользователь создал `F:/Work/DirectoryService/client/.dockerignore`;
содержимое проверено. Исключены `node_modules`, `.next`, остальные build artifacts,
локальные `.env*`, TypeScript cache/генерируемый `next-env.d.ts`, Git/IDE-файлы,
логи, тесты и документация. Исходники приложения, package/lockfile и конфигурации
Next/TypeScript остаются в build context. Lockfile version 3: спецификации всех
dependencies/devDependencies в корневой записи совпадают с package.json.

Подготовлен вариант `artifacts/frontend-image/Dockerfile`: обе стадии используют
официальный `node:24.22.3-bookworm-slim` (тег проверен по Docker Hub). Установленный
Next.js требует Node >=20.9.0. Build-stage выполняет `npm ci --include=dev` и
`npm run build`; runtime-stage получает только `.next/standalone`, `.next/static`
и `public`, запускает `node server.js` от пользователя `node`. `NODE_ENV=production`
задан только в runtime-stage, чтобы инструменты сборки были установлены в build.
`PORT`/`HOSTNAME` поступают из Compose. Телеметрия Next отключена в обеих стадиях.

Пользователь скопировал вариант в `F:/Work/DirectoryService/client/Dockerfile`;
сравнение подтвердило совпадение с подготовленным вариантом. Docker Linux Engine
доступен. `docker buildx build --check` проверил канонический Dockerfile,
метаданные Node-образа и загрузку `.dockerignore`: `Check complete, no warnings
found` (exit code 0). Общий Compose проходит `config --quiet`. Очистка тестового
проекта этапа 4 подтверждена: его контейнеров и volumes нет.

Пользователь успешно выполнил `docker compose --progress plain build frontend`;
`npm ci`, production build и сборка runtime-образа завершились. Проверен образ
`directory-service-frontend` размером около 255,5 MiB, ID
`sha256:5fdfdcb7decae63deecf5c4d1bb5c6171a6b9a4005308bed99cd1a7cc809bff8`.
CMD — `node server.js`, пользователь — `node`, в image environment нет переменных
GitHub/token/password.

Одноразовый контейнер без сети и внешних портов подтвердил: Node v24.22.3,
UID 1000, production mode; присутствуют готовый server.js, статика и public;
нет `/app/src`, локальных `.env*` и всех 10 project devDependencies. В production
routes manifest rewrites отсутствуют. Главная страница и `/locations`, а также
выбранные CSS/JS вернули HTTP 200. В логе — `Ready in 0ms`, сборка при старте
не выполняется. Проверочный контейнер остановлен и автоматически удалён.
Это проверка frontend отдельно; полный маршрут через nginx проверим на этапах 7–8.

- [x] Прочитать применимые инструкции `client/AGENTS.md` и документацию
  установленной версии Next.js перед изменением frontend.
- [x] Включить `output: "standalone"`.
- [x] Build-stage: `npm ci` и production build.
- [x] Node runtime-stage: `.next/standalone`, `.next/static`, `public` и запуск
  `server.js`; при старте контейнера сборка не выполняется.
- [x] Добавить frontend `.dockerignore`: `node_modules`, `.next`, `out`, coverage,
  локальные env-файлы и IDE-мусор.
- [x] Проверить фактическую production-сборку и запуск готового frontend-образа.
- [x] Подготовить frontend к маршрутизации через nginx: относительные запросы
  `/api/...` и отсутствие Next rewrites в production. Полная nginx-маршрутизация
  проверяется на этапах 7–8.

### 6. Собрать Compose и зависимости по готовности

Статус: выполнен. Полная цепочка старта и блокировка nginx при сбое backend
проверены на этапе 8. Пользователь добавил Postgres healthcheck; общий Compose прошёл
проверку. Подтверждены `CMD pg_isready`, TCP host `127.0.0.1`, порт/пользователь/БД
из окружения; interval 5s, timeout 3s, retries 12, start_period 10s.
Проверка по TCP не принимает за готовность временный Unix-socket server,
используемый официальным образом во время первичной инициализации.

Пользователь добавил backend `depends_on.postgres.condition=service_healthy`
и curl healthcheck. Compose прошёл проверку; подтверждены curl `--fail`, URL
`/health/ready` на настроенном порту, interval 5s, timeout 3s, retries 12,
start_period 30s. Curl и endpoint ранее проверены в работающем runtime-образе.

Пользователь добавил frontend healthcheck через Node fetch главной страницы по
`http://127.0.0.1:` + `process.env.PORT`, с `AbortSignal.timeout(2000)` и явным
exit code 0 только для HTTP 200. Проверены Compose, форма CMD/node/--eval,
обработка ошибки, interval 5s, timeout 3s, retries 12, start_period 10s.
Node fetch уже проверен в standalone-образе.

Пользователь добавил nginx зависимости от healthy backend
и frontend, а также healthcheck через
`wget -q -T 2 --spider http://127.0.0.1/` (interval 5s, timeout 3s, retries 12,
start_period 10s). Наличие wget и эта команда проверены на официальном
`nginx:1.30.6-alpine` во временном контейнере без сети и внешних портов; проверка
прошла, контейнер остановлен и удалён. Общий Compose прошёл проверку: зависимости
используют `service_healthy`, все пять сервисов подключены к одной сети, наружу
опубликован только nginx. В полном стенде запрос `/` должен пройти
через nginx к frontend; его конфигурационный шаблон создадим на этапе 7.
Реальную цепочку старта проверим после создания шаблона nginx.

Следующий шаг — необязательный профиль `monitoring`. Дополнения для Prometheus
подготовлены в `artifacts/monitoring-profile/prometheus.compose.yml` и проверены
через Compose как overlay к корневому файлу. Конфигурация использует
`backend:${BACKEND_PORT}`, монтирует существующие alerts read-only и не публикует
порты. Пользователь перенёс эти блоки в корневой Compose: проверены профиль
`monitoring`, зависимость от healthy backend, config mount, volume и read-only
alerts bind. Фактический scrape target после подстановки — `backend:8080`.
Обычный `docker compose config --services` перечисляет только пять основных
сервисов, Prometheus включается только с профилем. Это проверка конфигурации,
запуск и сбор метрик ещё не проверены.
`configs.content` поддерживается начиная с Compose 2.23.1; установленная версия
поддерживает эту возможность.

Пользователь перенёс дополнения из `artifacts/monitoring-profile/grafana.compose.yml`
в корневой Compose. Проверены образ, профиль, сеть, зависимость от Prometheus,
read-only provisioning/dashboards, отдельный volume и наличие учётных данных
из `.env`. Signup отключён. Пути bind mount существуют. С профилем Compose
содержит семь сервисов, без профиля — пять основных; порты публикует только
nginx. Доступ к UI мониторинга через nginx настроим на этапе 7.

- [x] Подключить основные сервисы к общей Docker-сети.
- [x] Postgres: зафиксированный образ, именованный volume, `pg_isready` healthcheck.
- [x] Backend настроен стартовать после здорового Postgres; readiness probe
  проверяет его доступность после миграций. Полная цепочка старта ещё не проверена.
- [x] Frontend получает HTTP healthcheck.
- [x] Nginx настроен стартовать после healthy backend и frontend; получает свой healthcheck.
- [x] Использовать `depends_on.condition: service_healthy`, подобрать таймауты,
  интервалы, retries и start period для первой сборки/инициализации.
- [x] Проверить наличие утилит healthcheck внутри runtime-образов.
- [x] Публиковать только порт nginx; Postgres, backend, frontend и Seq без `ports`.
- [x] Сохранить Prometheus/Grafana в дополнительном профиле; при включении
  мониторинга заменить адрес backend через host на Docker service name.

### 7. Настроить nginx

Статус: выполнен. Пользователь перенёс основной шаблон из
`artifacts/nginx/default.conf.template` в корневой `nginx/default.conf.template`;
сравнение SHA-256 подтвердило точное совпадение, Compose прошёл проверку.
Он сохраняет полный URI `/api` для backend, направляет остальные запросы на
frontend и передаёт proxy headers. Docker DNS используется во время запросов,
чтобы nginx мог обновлять адреса пересозданных контейнеров. Порты подставляются
официальным entrypoint nginx с фильтром `BACKEND_PORT|FRONTEND_PORT`; переменные
nginx должны сохраниться.

Подготовленный шаблон проверен в одноразовом `nginx:1.30.6-alpine` без сети и
внешних портов: официальный entrypoint выполнил envsubst, `nginx -t` завершился
с кодом 0 (`syntax is ok`, `test is successful`). Контейнер автоматически удалён.
Основные маршруты затем проверены с реальными backend/frontend в `ds-stack-check`:
`/` и `/locations` вернули 200 HTML, выбранные `/_next/` CSS/JS — 200 с
соответствующими Content-Type, `/api/locations` — 200 JSON с пустым списком.

Подготовлен следующий вариант шаблона с `/grafana/` и `/prometheus/`, а также
`artifacts/nginx/monitoring-routes.compose.yml` с настройками сервисов для этих
префиксов. Grafana сохраняет префикс в запросах и получает root_url и
serve_from_sub_path. Prometheus объявляет внешний URL с `/prometheus/`, но
внутренний route-prefix `/`; nginx убирает префикс, сохраняя query string.
Поэтому существующий Grafana datasource `http://prometheus:9090` сохраняется.
Добавлен WebSocket Upgrade для Grafana Live. Optional service names разрешаются
во время запросов, отсутствие профиля не должно блокировать запуск nginx.
Пользователь перенёс дополнения в корневые файлы. Подтверждены точное совпадение
шаблона, успешный `config --quiet`, root_url Grafana
`http://localhost:80/grafana/`, serve_from_sub_path=true и оба Prometheus-флага.
Только nginx публикует `127.0.0.1:80`; условия зависимостей healthcheck сохранены.

Обновлённый шаблон прошёл `nginx -T` в одноразовом контейнере без сети/портов.
Подстановка портов подтверждена, `$request_uri`, `$uri`, `$is_args`, `$args` и
proxy header variables сохранены. Контейнер удалён автоматически. Compose с
подготовленным overlay проходит `config --quiet`; root_url Grafana подставляется
из HTTP_PORT. HTTP-маршруты мониторинга, статика, dashboard provisioning и
datasource затем успешно проверены на этапе 8; отдельная WebSocket-сессия
Grafana Live не проверялась.

Справочные источники: [Grafana reverse proxy](https://grafana.com/tutorials/run-grafana-behind-a-proxy/),
[Prometheus flags](https://prometheus.io/docs/prometheus/latest/command-line/prometheus/),
[nginx proxy_pass](https://nginx.org/en/docs/http/ngx_http_proxy_module.html#proxy_pass).

- [x] Настроить `/api/*` на backend с сохранением `/api`; HTTP 200 JSON проверен.
- [x] Настроить остальные пути, включая `/_next/*` и статику, на frontend.
- [x] Передавать `Host`, `X-Forwarded-For`, `X-Forwarded-Proto`.
- [x] Подставлять окружение в upstream config через шаблон, сохраняя служебные
  переменные nginx, например `$host`.
- [x] Проверить одну origin для frontend и API: оба отвечают через localhost:80.

### 8. Проверить приёмку и обновить README

Статус: выполнен. Проверены обычный запуск, HTTP, миграции, сохранение/удаление
данных, блокировка nginx при сбое backend, содержимое образов и документация.

Проверки выполнять в отдельном Compose-проекте и на его собственных volumes.

Подготовлен первый запуск основных сервисов без monitoring: проект
`ds-stack-check`. Проверка Docker подтвердила отсутствие его контейнеров и volumes;
TCP-порт 80 свободен. Следующая команда пользователя из корня репозитория:
`docker compose --project-name ds-stack-check up --detach --build --wait --wait-timeout 180`.
Пользователь выполнил команду: сборка и `up --wait` завершились успешно.
Проверены пять running сервисов; Postgres/backend/frontend/nginx healthy,
Seq running. Только nginx имеет host binding `127.0.0.1:80->80/tcp`.

Проверка HTTP через nginx подтвердила `/` и `/locations` — 200 HTML, CSS/JS —
200, `/api/locations` — 200 application/json, `isError=false`, `items=[]`,
`totalCount=0`. В БД пять таблиц схемы DirectoryService, public.__EFMigrationsHistory
с `20261111181222_Initial` (EF 10.0.8), расширения ltree/pg_trgm и GIN index
idx_departments_name_trgm. Логи backend доступны через Compose; миграции успешно
завершились перед `Now listening`.

Следующий шаг пользователя — включить monitoring в том же тестовом проекте:
`docker compose --project-name ds-stack-check --profile monitoring up --detach --wait --wait-timeout 180`.
После этого проверим GUI, сбор метрик и provisioning dashboards.

Пользователь включил профиль. Проверены семь running сервисов; основные probes
остались healthy, host binding есть только у nginx. `promtool check config`
подтвердил корректный config и четыре alert rules. `/grafana` перенаправляет на
`/grafana/login`, `/prometheus` — на `/prometheus/query`; страницы, API health и
выбранные CSS/JS Grafana и Prometheus отвечают через nginx с HTTP 200.
Grafana health: database=ok, version=13.1.0. Prometheus target
`http://backend:8080/metrics` — up, lastError пустой; PromQL
`up{job="directory-service"}` возвращает 1. Grafana API подтверждает provisioned
dashboard DirectoryService Overview с 12 панелями и datasource health=OK.
Тот же PromQL через Grafana datasource proxy успешно возвращает 1.

В логах Grafana обнаружены не мешающие этому dashboard ошибки: отсутствующие
директории provisioning/plugins и provisioning/alerting, а также permission
denied при фоновой установке рекомендованных плагинов Elasticsearch/Zipkin.
Добавлены .gitkeep в обе директории provisioning. Подготовлено отключение
автоматической предустановки через `GF_PLUGINS_PREINSTALL_DISABLED=true`;
наличие настройки проверено в defaults.ini запущенного образа и официальной
документации Grafana. Пользователь добавил переменную в корневой Compose и
пересоздал Grafana. Проверены running state, эффективное значение переменной
и новые логи: level=error — 0, Failed to install plugin — 0. Dashboard по-прежнему
provisioned, 12 панелей, datasource=OK, Prometheus up=1, API проекта отвечает.

Подготовлена проверка сохранения данных в том же изолированном проекте. Через
nginx создана и прочитана обратно тестовая локация
`Compose persistence check 66737a1a3a`, ID
`f3697ead-ded1-42b9-aa02-4076e879677f`. Baseline сохранён в
`artifacts/compose-acceptance/persistence-baseline.json`: project ds-stack-check,
volume ds-stack-check_postgres-data, CreatedAt 2026-11-12T10:28:20Z.
Перед записью проверено, что localhost:80 опубликован именно nginx этого проекта.
Следующее действие пользователя — `down` без `-v` с профилем monitoring, затем
`up --detach --wait --wait-timeout 180` с тем же именем проекта и профилем.
После этого проверим сохранение ID/имени локации и исходного volume.

Пользователь выполнил down/up без -v. Все семь сервисов снова running, основные
probes healthy. Созданная локация прочитана через nginx: HTTP 200, прежние ID,
имя и адрес, isError=false. Volume CreatedAt сохранился: 2026-11-12T10:28:20Z.
При сравнении даты нормализованы через DateTimeOffset: ConvertFrom-Json может
превратить ISO строку в DateTime, поэтому строковое сравнение здесь ненадёжно.
Сохранение данных после down/up подтверждено.

Пользователь сообщил No data в Grafana после перезапуска. На screenshot выбран
фиксированный интервал 2026-11-12 16:53:44–17:01:15 +07, до запуска нового
Prometheus (17:35 +07). Проверены оба запроса: query_range up за этот интервал
возвращает 0 series; текущий up{job="directory-service"}=1, target healthy/up,
lastError пустой. Причина — старый временной диапазон, а не поломка мониторинга.
Следующее действие в UI — Last 15 minutes / Refresh или URL с from=now-15m&to=now.
Проверка down -v и восстановления чистой БД ещё впереди.

Перед этой проверкой проверены все четыре volume: ds-stack-check_postgres-data,
ds-stack-check_seq-data, ds-stack-check_prometheus-data, ds-stack-check_grafana-data.
Имена и labels принадлежат ds-stack-check; каждый используется только контейнером
этого же проекта, посторонних пользователей volumes нет. Подготовлены команды
`docker compose --project-name ds-stack-check --profile monitoring down --volumes`
и повторный `up --detach --wait --wait-timeout 180`. Пользователь выполнил
проверку. Новый volume создан 2026-11-12T10:52:03Z вместо исходного 10:28:20Z.
Прежняя локация возвращает HTTP 404 record.not.found, GET списка — HTTP 200,
totalCount=0, isError=false, SQL count=0. Воссозданы пять доменных таблиц,
__EFMigrationsHistory с Initial, расширения ltree/pg_trgm и GIN index.
По логам миграции завершились до Now listening. Основные сервисы healthy,
monitoring running; frontend 200, Grafana database=ok, Prometheus target up.

Следующий шаг — отрицательный сценарий старта в отдельном проекте
`ds-startup-failure-check`, подготовлен override
`artifacts/compose-acceptance/startup-failure.compose.yml`. Используются уже
собранные образы, backend получает заведомо неверный тестовый пароль, host ports
nginx удалены через !reset []. У основного ds-stack-check настройки не меняются.
Пользователь выполнил сценарий: Compose завершился ошибкой зависимости,
backend exited с кодом 139. Проверка Docker подтверждает: backend не running,
лог содержит Applying database migrations, FTL Database migration failed.
Backend startup aborted и PostgreSQL 28P01 password authentication failed.
Now listening отсутствует. Nginx остался created, StartedAt/FinishedAt нулевые,
то есть не запускался. Все host port bindings проекта пусты. Основной
ds-stack-check продолжает отвечать: frontend 200, API isError=false.
Код 139 зафиксирован как ненулевой; отдельная причина именно этого кода не
исследовалась, он не объявляется стандартным кодом ошибки аутентификации.

Перед очисткой ds-startup-failure-check проверены его два volume (postgres-data,
seq-data): labels/имена принадлежат этому проекту, используются только его
контейнерами. Пользователь выполнил `down --volumes`; Docker подтвердил отсутствие
его контейнеров, volumes и сети. Основной ds-stack-check продолжает работать.

Финальная сверка фактически собранных образов:

- Backend `sha256:e7288ba1e7f168ed7479aed92cdd236a553aac87c8097d9fd1bc3a22a5eeb4c9`:
  .NET runtime 10.0.8, UID 1654, curl есть; SDK, /source, *.cs/*.csproj и bin/obj
  отсутствуют. Проверка выполнена в одноразовом контейнере без сети и портов.
- Frontend `sha256:34d2fd5f396764a0b687c7e945388cf66a182aaf65b1aa22db3378a01b2eff17`:
  Node v24.22.3, UID 1000, NODE_ENV=production; standalone/static/public есть,
  /app/src, локальные env-файлы и все 10 project devDependencies отсутствуют;
  production rewrites — 0. Страницы и CSS/JS вернули HTTP 200.
- В environment обоих образов нет имён GitHub/token/password/connection variables.
  Текущий backend-образ экспортирован и просканирован: 26 entries, текущий PAT
  не найден; временный tar после проверки удалён.

Git: настоящий корневой .env не отслеживается и игнорируется правилом .env;
artifacts также игнорируются. .env.example существует, проходит config --quiet
и не игнорируется; пока untracked, как другие новые Docker-файлы. Коммит не
создавался, посторонние изменения рабочего дерева сохранены.

README подготовлен в `artifacts/README.docker-local.md` для переноса пользователем
в корень репозитория. Он сохраняет описание проекта и описывает только Docker
как локальную зависимость, дополнительную настройку PAT, команды старта, адреса,
логи, monitoring, смену порта, пересборку, down/down --volumes и особенности
паролей существующих БД. Документация monitoring и GitHub Packages обновлена;
legacy-режим в ней явно отделён от общего Compose. Пользователь заменил
канонический README; SHA-256 подтвердил совпадение с подготовленным вариантом
(185 строк). Compose с реальным .env и .env.example проходит config --quiet.
Git diff --check прошёл, .env по-прежнему игнорируется и не отслеживается,
.env.example не игнорируется.

Финальный запуск выполнен пользователем: ds-stack-check удалён с volumes, затем
из корня выполнен обычный `docker compose up --detach --build --wait` без
--project-name и следующим up включён monitoring. Проверено стандартное имя
directory-service: семь running сервисов, Postgres/backend/frontend/nginx healthy;
host port только у nginx, 127.0.0.1:80. Оба временных проекта полностью удалены:
контейнеров, volumes и сетей с их labels не осталось.

В новой БД обычного проекта пять таблиц домена, __EFMigrationsHistory с Initial,
ltree и pg_trgm; locations_count=0. Логи подтверждают применение миграций до
запуска HTTP. Через localhost `/`, `/locations`, `/api/locations`, Grafana login
и health, Prometheus ready/query возвращают HTTP 200. API isError=false,
totalCount=0, Prometheus up=1. Grafana provisioned dashboard содержит 12 панелей,
datasource health=OK.

Образы обычного проекта имеют другие ID из-за метаданных Compose. Проверено
полное совпадение RootFS layers и runtime env/entrypoint/cmd/user с образами,
для которых выполнены runtime audit и сканирование токена. Текущие ID:
backend sha256:e83b3e4f37b61e2224d2f76cc5e7916bd099c57fe3faf43945869b126cabdb32;
frontend sha256:5fdfdcb7decae63deecf5c4d1bb5c6171a6b9a4005308bed99cd1a7cc809bff8.

Все проверки выполнялись в существующем checkout; отдельный свежий git clone
не создавался. Воспроизводимость зависимостей проверена изолированным restore
без личного NuGet cache и сборкой внутри Docker. Шаги самого Compose не используют
установленные на host SDK/Node/Postgres. Новые файлы остаются готовыми к коммиту.

- [x] Чистый запуск: `cp .env.example .env` → настройка GitHub Packages token →
  `docker compose up` без SDK/Node/БД на host и личного package cache. Дополнительный
  шаг настройки токена включён в README как согласованное изменение требования.
- [x] `docker compose ps`: основные сервисы healthy, monitoring running и отвечает по HTTP.
- [x] Главная страница и `/locations` отвечают через nginx.
- [x] `http://localhost/api/locations`: ожидаемый JSON API, на пустой БД —
  корректный пустой список; не HTML frontend и не proxy error.
- [x] В БД есть доменные таблицы, расширения и `__EFMigrationsHistory`.
- [x] `docker compose logs backend` показывает запуск и миграции; сценарий ошибки проверен на этапе 4.
- [x] На host опубликован только nginx, включая профиль monitoring.
- [x] `down` останавливает сервисы, последующий `up` сохраняет данные.
- [x] `down -v` удаляет volume; следующий `up` создаёт рабочую схему заново.
- [x] Ошибка миграции завершает backend и не позволяет nginx начать обслуживание.
- [x] Финальные образы не содержат SDK, лишних исходников и project devDependencies.
- [x] Настоящий `.env` игнорируется Git; `.env.example` создан, не игнорируется и готов к коммиту.
- [x] README описывает запуск, адрес, логи, `up --build` после изменения исходников,
  остановку, удаление данных и смену порта.
- [x] README объясняет: смена пароля Postgres в `.env` не меняет пароль базы,
  уже созданной в существующем volume.

## Как продолжаем работу

Идём последовательно по этапам. В текущем этапе сначала проверяем факты и
объясняем назначение действий. Выполненные проверки и принятые решения отмечаем
в этом файле. Непройденную проверку не считаем завершённой.

Сохранение этого плана в Markdown не означает создание Git-коммита.
