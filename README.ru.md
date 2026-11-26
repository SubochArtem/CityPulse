<p align="center">
  <img src="docs/screenshots/home.jpg" alt="Главная страница CityPulse" width="900">
</p>

<h1 align="center">CityPulse</h1>

<p align="center">
  Платформа городского участия: жители предлагают идеи, обсуждают их и голосуют за то, как меняется их город.
</p>

<p align="center">
  <a href="README.md">English</a> · <b>Русский</b>
</p>

<p align="center">
  <a href="https://github.com/SubochArtem/CityPulse/actions/workflows/sonar.yml"><img src="https://github.com/SubochArtem/CityPulse/actions/workflows/sonar.yml/badge.svg" alt="SonarQube"></a>
  <a href="https://github.com/SubochArtem/CityPulse/actions/workflows/frontend.yml"><img src="https://github.com/SubochArtem/CityPulse/actions/workflows/frontend.yml/badge.svg" alt="Frontend"></a>
  <a href="https://github.com/SubochArtem/CityPulse/actions/workflows/docker-images.yml"><img src="https://github.com/SubochArtem/CityPulse/actions/workflows/docker-images.yml/badge.svg" alt="Сборка и деплой"></a>
  <a href="https://sonarcloud.io/summary/new_code?id=SubochArtem_CityPulse"><img src="https://sonarcloud.io/api/project_badges/measure?project=SubochArtem_CityPulse&metric=alert_status" alt="Quality Gate"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="Лицензия MIT"></a>
</p>

---

## Содержание

- [Возможности](#возможности)
- [Скриншоты](#скриншоты)
- [Архитектура](#архитектура)
- [Технологии](#технологии)
- [Запуск](#запуск)
- [Локальная разработка](#локальная-разработка)
- [Тестирование](#тестирование)
- [Развёртывание](#развёртывание)
- [Структура проекта](#структура-проекта)
- [Лицензия](#лицензия)

## Возможности

- **Города и опросы.** В каждом городе идут опросы о том, что построить, починить или заменить. Опрос — это либо голосование жителей, либо решение руководителя; у него есть бюджет и срок, а закрывается он автоматически.
- **Идеи.** Жители города предлагают идеи с фотографиями в активный опрос. Модераторы одобряют или отклоняют их и могут скрывать.
- **Обсуждение.** Лайки и комментарии к каждой идее.
- **Живая лента активности.** Стена постеров идей на главной странице, обновляется в реальном времени через SignalR: у каждой идеи фото, что с ней произошло (предложена, одобрена, прокомментирована), лайки и последний комментарий. Ряд «историй» опросов сверху фильтрует стену по опросу.
- **Администрирование.** Управление городами, опросами, модерация идей и пользователей; доступ определяется разрешениями.
- **Безопасный вход.** Auth0 за Backend-for-Frontend: токены не попадают в браузер, у него есть только httpOnly cookie сессии.
- **Два языка и темы.** Английский и русский, светлая и тёмная («ночная стена») темы, адаптивная вёрстка.

## Скриншоты

| Главная (живая лента) | Города |
| --- | --- |
| ![Главная](docs/screenshots/home.jpg) | ![Города](docs/screenshots/cities.jpg) |
| **Город** | **Опрос** |
| ![Город](docs/screenshots/city.jpg) | ![Опрос](docs/screenshots/poll.jpg) |
| **Идея** | **Администрирование** |
| ![Идея](docs/screenshots/idea.jpg) | ![Администрирование](docs/screenshots/admin-polls.jpg) |
| **Тёмная тема** | **Мобильная версия** |
| ![Тёмная тема](docs/screenshots/home-dark.jpg) | <img src="docs/screenshots/cities-mobile.jpg" alt="Мобильная версия" width="240"> |

**Лента активности**

| Истории опросов и постеры идей | Комментарии под постерами |
| --- | --- |
| ![Лента активности](docs/screenshots/activity-feed.jpg) | ![Постеры идей](docs/screenshots/activity-feed-posters.jpg) |

<p align="center">
  <img src="docs/screenshots/activity-feed-mobile.jpg" alt="Лента активности на телефоне" width="300">
</p>

## Архитектура

CityPulse — набор микросервисов на .NET за API-шлюзом YARP и одностраничное React-приложение перед ними.

```mermaid
flowchart LR
    Browser["Браузер<br/>React SPA"]
    Auth0(["Auth0"])

    subgraph Edge
        Nginx["nginx :5173<br/>статика + прокси"]
        Gateway["Gateway (YARP) :8080<br/>BFF, маршрутизация, Swagger"]
    end

    subgraph Services
        Users["Users<br/>N-слойная"]
        Polls["Polls<br/>Onion"]
        Feedback["Feedback<br/>Vertical slices"]
    end

    UsersDb[("PostgreSQL<br/>users")]
    PollsDb[("PostgreSQL<br/>polls + Hangfire")]
    Mongo[("MongoDB")]
    Redis[("Redis")]
    Minio[("MinIO<br/>изображения")]
    Rabbit{{"RabbitMQ"}}

    Browser --> Nginx --> Gateway
    Gateway <-. OIDC .-> Auth0
    Gateway --> Users & Polls & Feedback
    Gateway --- Redis

    Users --> UsersDb
    Users --- Redis
    Polls --> PollsDb
    Polls --> Minio
    Feedback --> Mongo
    Feedback --- Redis

    Users -- gRPC --> Polls
    Feedback -- gRPC --> Polls
    Polls -- события --> Rabbit
    Users -- события --> Rabbit
    Rabbit --> Feedback
    Rabbit --> Polls
```

| Компонент | Ответственность | Архитектура | Хранилище |
| --- | --- | --- | --- |
| **Gateway** | Единая точка входа: маршрутизация через YARP, Backend-for-Frontend (вход через Auth0 OIDC, токены на сервере, обновление, проверка CSRF), общий Swagger | — | Redis (сессии, ключи Data Protection) |
| **Users** | Профили, родной город, роли и разрешения, вебхук Auth0 и синхронизация через Management API | Строгая N-слойная: Presentation → Business → DataAccess | PostgreSQL, Redis (кэш) |
| **Polls** | Города, опросы, идеи, изображения, плановая смена статусов опросов, gRPC-сервер для других сервисов | Onion: Domain, Application, Infrastructure, API | PostgreSQL, Hangfire, MinIO |
| **Feedback** | Лайки и комментарии, лента активности, SignalR-хаб `/hubs/activity` | Вертикальные срезы в `Features/` | MongoDB, Redis (кэш, backplane SignalR) |
| **Frontend** | SPA, которое раздаёт nginx и проксирует `/api`, `/bff` и `/hubs` на шлюз | Папки по фичам | — |

**Взаимодействие**

- **HTTP** — браузер общается только со шлюзом, с того же origin через nginx (или dev-прокси Vite).
- **gRPC** — Users и Feedback читают города и идеи из Polls (`:8081`, HTTP/2).
- **События** — Polls публикует интеграционные события (создание, смена статуса и удаление опросов и идей) в RabbitMQ через транзакционный outbox MassTransit; Users публикует смену статуса пользователя. Feedback собирает из них ленту активности и рассылает обновления клиентам через SignalR; Polls реагирует на смену статуса пользователя.
- **Общие контракты** — в `src/Backend/Services/Shared` лежат NuGet-пакеты (общая настройка API, авторизация, события, gRPC, запросы), которые публикуются в GitHub Packages.

**Аутентификация**

1. SPA отправляет пользователя на `/bff/login`; шлюз выполняет OIDC code flow с Auth0 как конфиденциальный клиент.
2. Токены хранятся на сервере в Redis. Браузер получает только httpOnly cookie `citypulse.session` с `SameSite=Lax`.
3. При каждом запросе к `/api` шлюз подставляет access token, обновляет его перед истечением и требует заголовок `X-CSRF: 1` для изменяющих методов.
4. Сервисы проверяют JWT и авторизуют по разрешениям, которые выдаёт Auth0.

## Технологии

**Backend**

- .NET 8, ASP.NET Core, обратный прокси YARP
- Entity Framework Core + PostgreSQL, MongoDB.Driver
- MassTransit + RabbitMQ (транзакционный outbox), gRPC, SignalR с backplane на Redis
- Hangfire (плановые задачи), MinIO (S3-совместимое хранилище изображений)
- MediatR, FluentValidation, Mapster / AutoMapper, ErrorOr
- Auth0 (OIDC, JWT, Management API), Serilog, Swagger
- xUnit, Moq, Testcontainers, Respawn, Bogus

**Frontend**

- React 19, TypeScript, Vite
- Tailwind CSS 4, shadcn/ui (Radix)
- TanStack Query, React Router, openapi-fetch с типами, сгенерированными из Swagger
- i18next (английский, русский), `@microsoft/signalr`

**Инфраструктура**

- Docker, Docker Compose, nginx, Caddy (автоматический HTTPS)
- GitHub Actions: сборка, тесты, анализ SonarCloud, публикация NuGet-пакетов, Docker-образы, деплой
- GitHub Container Registry, виртуальная машина Azure

## Запуск

### Что понадобится

- [Docker](https://docs.docker.com/get-docker/) с Docker Compose v2
- Персональный токен GitHub с правом `read:packages` (сервисы скачивают общие NuGet-пакеты из GitHub Packages)
- Тенант [Auth0](https://auth0.com/), в котором есть:
  - API (его идентификатор — audience) с включёнными RBAC и разрешениями в access token;
  - **Regular Web Application** для BFF шлюза с callback `http://localhost:5173/bff/callback`, logout URL `http://localhost:5173` и ротацией refresh-токенов;
  - **Single Page Application** для Swagger UI;
  - **Machine to Machine** приложение для Management API (его использует Users).

### 1. Настройте окружение

```bash
git clone https://github.com/SubochArtem/CityPulse.git
cd CityPulse
cp .env.example .env
```

Заполните `.env`:

| Переменная | Описание |
| --- | --- |
| `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD` | Учётные данные MinIO |
| `MINIO_PUBLIC_URL` | Публичный адрес изображений, по умолчанию `http://localhost:9000` |
| `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS` | Учётные данные RabbitMQ |
| `MONGO_INITDB_ROOT_USERNAME`, `MONGO_INITDB_ROOT_PASSWORD` | Учётные данные MongoDB |
| `POSTGRES_USER`, `POSTGRES_PASSWORD` | Учётные данные PostgreSQL (для обеих баз) |
| `AUTH0_DOMAIN`, `AUTH0_AUDIENCE` | Домен тенанта Auth0 и идентификатор API |
| `AUTH0_CLIENT_ID`, `AUTH0_CLIENT_SECRET` | SPA-приложение для Swagger UI |
| `AUTH0_WEB_CLIENT_ID`, `AUTH0_WEB_CLIENT_SECRET` | Regular Web Application для BFF шлюза |
| `AUTH0_MANAGEMENT_API_CLIENT_ID`, `AUTH0_MANAGEMENT_API_CLIENT_SECRET`, `AUTH0_MANAGEMENT_API_AUDIENCE` | Machine to Machine приложение для Management API |
| `AUTH0_WEBHOOK_SECRET` | Общий секрет для вебхука Auth0 после регистрации |
| `GITHUB_TOKEN` | Токен с `read:packages` для GitHub NuGet |

### 2. Запустите весь стек

```bash
docker compose --profile app up -d --build
```

Миграции баз данных применяются при старте. Когда все контейнеры станут healthy:

| Адрес | Что там |
| --- | --- |
| http://localhost:5173 | Веб-приложение |
| http://localhost:8080/swagger | Swagger UI всех сервисов |
| http://localhost:15672 | Панель RabbitMQ |
| http://localhost:9001 | Консоль MinIO |

Остановить всё: `docker compose --profile app down` (с `-v` удалятся и тома с данными).

## Локальная разработка

Запустите только инфраструктуру (базы данных, RabbitMQ, Redis, MinIO):

```bash
docker compose up -d
```

Запустите сервис из IDE или командной строки, например:

```bash
dotnet run --project src/Backend/Services/Polls/Polls.API
```

Запустите фронтенд с горячей перезагрузкой (dev-сервер Vite проксирует `/api`, `/bff` и `/hubs` на шлюз `:8080`; сначала остановите контейнер `citypulse_frontend`, он занимает тот же порт):

```bash
cd src/frontend
npm ci
npm run dev
```

Полезные скрипты фронтенда:

| Команда | Что делает |
| --- | --- |
| `npm run lint` | ESLint |
| `npm run typecheck` | Проверка TypeScript |
| `npm run format:check` | Проверка Prettier |
| `npm run build` | Продакшен-сборка |
| `npm run gen:api` | Перегенерация типов API из Swagger шлюза |

## Тестирование

Тесты бэкенда написаны на xUnit; интеграционные тесты поднимают настоящие PostgreSQL, MongoDB и RabbitMQ через Testcontainers, поэтому Docker должен быть запущен.

```bash
dotnet test                                          # все тесты
dotnet test src/Backend/Services/Polls/Polls.Tests   # один сервис
```

CI собирает бэкенд и запускает тесты с покрытием в SonarCloud, а для фронтенда — линтер, проверку форматирования и сборку.

## Развёртывание

В продакшене весь стек запущен через Docker Compose на одной Linux-виртуалке в Azure. Каждый мерж в `main` собирает новые образы и автоматически разворачивает их.

```mermaid
flowchart LR
    Dev["Мерж в main"] --> Build

    subgraph Actions["GitHub Actions: Docker images"]
        Build["build ×5<br/>users, polls, feedback,<br/>gateway, frontend"]
        Deploy["deploy<br/>окружение production"]
        Build --> Deploy
    end

    Build -- "push :sha и :latest" --> GHCR[("GitHub Container<br/>Registry")]
    Deploy -- "SSH: копирует deploy/,<br/>compose pull + up" --> VM

    subgraph VM["Azure VM: /opt/citypulse"]
        Caddy["Caddy :443<br/>Let's Encrypt"]
        Stack["frontend, gateway,<br/>сервисы, базы данных"]
        Caddy --> Stack
    end

    GHCR -- "pull образов" --> VM
    Users(["Пользователи"]) -- HTTPS --> Caddy
```

### Как работает пайплайн

| Событие | Workflow | Что происходит |
| --- | --- | --- |
| Pull request | `sonar.yml`, `frontend.yml` | Сборка бэкенда, тесты и анализ SonarCloud; линтер, проверка форматирования и сборка фронтенда. Ничего не публикуется. |
| Push в `main` | `docker-images.yml` | Параллельно собирает пять образов и пушит их в `ghcr.io/subochartem/citypulse-<service>` с тегами SHA коммита и `latest`. Если все сборки прошли, job `deploy` подключается к виртуалке по SSH, копирует `deploy/`, скачивает образы этого коммита и перезапускает стек, дожидаясь, пока все контейнеры станут healthy. |
| Тег `*-v*` | `publish-*.yml` | Публикует общий NuGet-пакет в GitHub Packages. |

Workflow можно запустить и вручную на вкладке Actions (**Docker images → Run workflow**).

### Продакшен-стек

В `deploy/` лежит всё, что нужно серверу, кроме секретов:

| Файл | Что |
| --- | --- |
| `deploy/docker-compose.yml` | Те же сервисы, что и локально, но с образами из GHCR (`IMAGE_TAG`), `restart: unless-stopped`, ротацией логов и без открытых портов, кроме Caddy |
| `deploy/Caddyfile` | HTTPS для `DOMAIN` с автоматическим сертификатом Let's Encrypt. `/api`, `/bff` и `/hubs` идут в gateway, `/polls-images` в MinIO, всё остальное во фронтенд |
| `deploy/.env.example` | Шаблон `.env` для сервера |

Данные (PostgreSQL, MongoDB, MinIO, Redis, сертификаты Caddy) хранятся в Docker volumes и переживают деплои. Миграции баз выполняются при старте Users и Polls. Весь стек занимает около 1 ГБ оперативной памяти.

### Настройка сервера

1. Создайте Linux-виртуалку минимум с 2 vCPU и 4 ГБ RAM (например, Azure `B2als_v2`) и откройте порты 22, 80 и 443. Задайте ей DNS-имя.
2. Установите Docker Engine с плагином Compose и добавьте swap-файл.
3. Создайте `/opt/citypulse` и положите туда `.env` на основе `deploy/.env.example`: укажите `DOMAIN`, надёжные пароли баз данных и значения Auth0.
4. В Auth0 добавьте `https://<DOMAIN>/bff/callback` в callback URLs веб-приложения и `https://<DOMAIN>` в его logout URLs, а вебхук после регистрации направьте на `https://<DOMAIN>/api/v1/auth0webhook`.
5. Создайте SSH-ключ для деплоя и добавьте его публичную часть в `authorized_keys` пользователя на сервере.
6. В GitHub создайте окружение `production` с секретами:

   | Секрет | Значение |
   | --- | --- |
   | `DEPLOY_HOST` | DNS-имя сервера |
   | `DEPLOY_USER` | Пользователь, которому принадлежит `/opt/citypulse` и который входит в группу `docker` |
   | `DEPLOY_SSH_KEY` | Приватный ключ для деплоя |
   | `DEPLOY_KNOWN_HOSTS` | Вывод `ssh-keyscan -t ed25519 <DEPLOY_HOST>` |

7. Запустите workflow **Docker images**. Образы публичные, поэтому сервер скачивает их без логина.

### Эксплуатация

```bash
cd /opt/citypulse
docker compose ps                                  # состояние
docker compose logs -f gateway                     # логи одного сервиса
IMAGE_TAG=<sha предыдущего коммита> docker compose up -d   # откат на прошлую версию
```

Если во время мержа виртуалка остановлена, job `deploy` падает по таймауту подключения, но образы всё равно публикуются. Запустите виртуалку и нажмите **Re-run failed jobs** в этом запуске.

## Структура проекта

```text
CityPulse/
├── .github/workflows/           # CI, сборка образов и деплой, публикация NuGet
├── docker-compose.yml           # инфраструктура + профиль app
├── deploy/                      # продакшен compose-файл, Caddyfile, шаблон .env
├── docker/                      # политика бакета MinIO
├── docs/screenshots/            # изображения для README
└── src/
    ├── Backend/
    │   ├── Gateway/
    │   │   ├── Gateway.API      # YARP, BFF, сводный Swagger
    │   │   └── Gateway.Tests
    │   └── Services/
    │       ├── Users/           # Presentation, Business, DataAccess, Tests
    │       ├── Polls/           # Domain, Application, Infrastructure, API, Tests
    │       ├── Feedback/        # Feedback.API (Features/*), Tests
    │       └── Shared/          # NuGet-пакеты: Common.Api, Contracts.*
    └── frontend/                # React SPA, конфиг nginx, Dockerfile
```

## Лицензия

Распространяется по [лицензии MIT](LICENSE).
