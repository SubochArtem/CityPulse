<p align="center">
  <img src="docs/screenshots/home.jpg" alt="CityPulse home page" width="900">
</p>

<h1 align="center">CityPulse</h1>

<p align="center">
  A civic participation platform: residents propose ideas, discuss them and vote on how their city changes.
</p>

<p align="center">
  <b>English</b> · <a href="README.ru.md">Русский</a>
</p>

<p align="center">
  <a href="https://github.com/SubochArtem/CityPulse/actions/workflows/sonar.yml"><img src="https://github.com/SubochArtem/CityPulse/actions/workflows/sonar.yml/badge.svg" alt="SonarQube"></a>
  <a href="https://github.com/SubochArtem/CityPulse/actions/workflows/frontend.yml"><img src="https://github.com/SubochArtem/CityPulse/actions/workflows/frontend.yml/badge.svg" alt="Frontend"></a>
  <a href="https://github.com/SubochArtem/CityPulse/actions/workflows/docker-images.yml"><img src="https://github.com/SubochArtem/CityPulse/actions/workflows/docker-images.yml/badge.svg" alt="Build and deploy"></a>
  <a href="https://sonarcloud.io/summary/new_code?id=SubochArtem_CityPulse"><img src="https://sonarcloud.io/api/project_badges/measure?project=SubochArtem_CityPulse&metric=alert_status" alt="Quality Gate"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT License"></a>
</p>

---

## Contents

- [Features](#features)
- [Screenshots](#screenshots)
- [Architecture](#architecture)
- [Tech stack](#tech-stack)
- [Getting started](#getting-started)
- [Local development](#local-development)
- [Testing](#testing)
- [Deployment](#deployment)
- [Project structure](#project-structure)
- [License](#license)

## Features

- **Cities and polls.** Each city runs polls about what to build, fix or replace. A poll is either a residents' vote or a manager's decision, has a budget and a deadline, and closes automatically.
- **Ideas.** Residents of a city submit ideas with photos to its active poll. Moderators approve or reject them and can hide them.
- **Discussion.** Likes and comments on every idea.
- **Live activity feed.** A wall of idea posters on the home page, updated in real time over SignalR: each idea shows its photo, what happened to it (proposed, approved, commented), likes and the newest comment. A row of poll "stories" on top filters the wall by poll.
- **Administration.** Managing cities, polls, idea moderation and users, with access driven by permissions.
- **Secure sign-in.** Auth0 behind a Backend-for-Frontend: tokens never reach the browser, which holds only an httpOnly session cookie.
- **Two languages and themes.** English and Russian, light and dark ("night wall") modes, responsive layout.

## Screenshots

| Home (live feed) | Cities |
| --- | --- |
| ![Home](docs/screenshots/home.jpg) | ![Cities](docs/screenshots/cities.jpg) |
| **City** | **Poll** |
| ![City](docs/screenshots/city.jpg) | ![Poll](docs/screenshots/poll.jpg) |
| **Idea** | **Administration** |
| ![Idea](docs/screenshots/idea.jpg) | ![Administration](docs/screenshots/admin-polls.jpg) |
| **Dark theme** | **Mobile** |
| ![Dark theme](docs/screenshots/home-dark.jpg) | <img src="docs/screenshots/cities-mobile.jpg" alt="Mobile" width="240"> |

**Activity feed**

| Poll stories and idea posters | Comments pasted under the posters |
| --- | --- |
| ![Activity feed](docs/screenshots/activity-feed.jpg) | ![Activity feed posters](docs/screenshots/activity-feed-posters.jpg) |

<p align="center">
  <img src="docs/screenshots/activity-feed-mobile.jpg" alt="Activity feed on a phone" width="300">
</p>

## Architecture

CityPulse is a set of .NET microservices behind a YARP API gateway, with a React single-page app in front.

```mermaid
flowchart LR
    Browser["Browser<br/>React SPA"]
    Auth0(["Auth0"])

    subgraph Edge
        Nginx["nginx :5173<br/>static files + proxy"]
        Gateway["Gateway (YARP) :8080<br/>BFF, routing, Swagger"]
    end

    subgraph Services
        Users["Users<br/>N-layer"]
        Polls["Polls<br/>Onion"]
        Feedback["Feedback<br/>Vertical slices"]
    end

    UsersDb[("PostgreSQL<br/>users")]
    PollsDb[("PostgreSQL<br/>polls + Hangfire")]
    Mongo[("MongoDB")]
    Redis[("Redis")]
    Minio[("MinIO<br/>images")]
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
    Polls -- events --> Rabbit
    Users -- events --> Rabbit
    Rabbit --> Feedback
    Rabbit --> Polls
```

| Component | Responsibility | Architecture | Storage |
| --- | --- | --- | --- |
| **Gateway** | Single entry point: routing with YARP, Backend-for-Frontend (Auth0 OIDC login, server-side tokens, refresh, CSRF check), aggregated Swagger | — | Redis (sessions, Data Protection keys) |
| **Users** | Profiles, home city, roles and permissions, Auth0 webhook and Management API sync | Strict N-layer: Presentation → Business → DataAccess | PostgreSQL, Redis (cache) |
| **Polls** | Cities, polls, ideas, images, scheduled poll status changes, gRPC server for other services | Onion: Domain, Application, Infrastructure, API | PostgreSQL, Hangfire, MinIO |
| **Feedback** | Likes and comments, activity feed, SignalR hub `/hubs/activity` | Vertical slices under `Features/` | MongoDB, Redis (cache, SignalR backplane) |
| **Frontend** | SPA served by nginx, which proxies `/api`, `/bff` and `/hubs` to the gateway | Feature folders | — |

**Communication**

- **HTTP** — the browser talks only to the gateway, same-origin through nginx (or the Vite dev proxy).
- **gRPC** — Users and Feedback read cities and ideas from Polls (`:8081`, HTTP/2).
- **Events** — Polls publishes integration events (poll and idea created, status changed, deleted) through the MassTransit transactional outbox to RabbitMQ; Users publishes user status changes. Feedback consumes them to build the activity feed and pushes updates to clients over SignalR; Polls reacts to user status changes.
- **Shared contracts** — `src/Backend/Services/Shared` holds NuGet packages (common API setup, auth, events, gRPC, querying) published to GitHub Packages.

**Authentication flow**

1. The SPA sends the user to `/bff/login`; the gateway runs the OIDC code flow with Auth0 as a confidential client.
2. Tokens are stored server-side in Redis. The browser receives only an httpOnly `SameSite=Lax` `citypulse.session` cookie.
3. On every `/api` call the gateway attaches the access token, refreshes it when it is about to expire and requires an `X-CSRF: 1` header on unsafe methods.
4. Services validate the JWT and authorize by permissions issued by Auth0.

## Tech stack

**Backend**

- .NET 8, ASP.NET Core, YARP reverse proxy
- Entity Framework Core + PostgreSQL, MongoDB.Driver
- MassTransit + RabbitMQ (transactional outbox), gRPC, SignalR with Redis backplane
- Hangfire (scheduled jobs), MinIO (S3-compatible image storage)
- MediatR, FluentValidation, Mapster / AutoMapper, ErrorOr
- Auth0 (OIDC, JWT, Management API), Serilog, Swagger
- xUnit, Moq, Testcontainers, Respawn, Bogus

**Frontend**

- React 19, TypeScript, Vite
- Tailwind CSS 4, shadcn/ui (Radix)
- TanStack Query, React Router, openapi-fetch with types generated from Swagger
- i18next (English, Russian), `@microsoft/signalr`

**Infrastructure**

- Docker, Docker Compose, nginx, Caddy (automatic HTTPS)
- GitHub Actions: build, tests, SonarCloud analysis, NuGet package publishing, Docker images, deployment
- GitHub Container Registry, Azure Virtual Machine

## Getting started

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) with Docker Compose v2
- A GitHub personal access token with `read:packages` (the services restore shared NuGet packages from GitHub Packages)
- An [Auth0](https://auth0.com/) tenant with:
  - an API (its identifier is the audience) with RBAC and permissions in the access token enabled;
  - a **Regular Web Application** for the gateway BFF, with callback `http://localhost:5173/bff/callback`, logout URL `http://localhost:5173` and refresh token rotation;
  - a **Single Page Application** for Swagger UI;
  - a **Machine to Machine** application for the Management API (used by Users).

### 1. Configure the environment

```bash
git clone https://github.com/SubochArtem/CityPulse.git
cd CityPulse
cp .env.example .env
```

Fill in `.env`:

| Variable | Description |
| --- | --- |
| `MINIO_ROOT_USER`, `MINIO_ROOT_PASSWORD` | MinIO credentials |
| `MINIO_PUBLIC_URL` | Public base URL for images, default `http://localhost:9000` |
| `RABBITMQ_DEFAULT_USER`, `RABBITMQ_DEFAULT_PASS` | RabbitMQ credentials |
| `MONGO_INITDB_ROOT_USERNAME`, `MONGO_INITDB_ROOT_PASSWORD` | MongoDB credentials |
| `POSTGRES_USER`, `POSTGRES_PASSWORD` | PostgreSQL credentials (both databases) |
| `AUTH0_DOMAIN`, `AUTH0_AUDIENCE` | Auth0 tenant domain and API identifier |
| `AUTH0_CLIENT_ID`, `AUTH0_CLIENT_SECRET` | SPA application used by Swagger UI |
| `AUTH0_WEB_CLIENT_ID`, `AUTH0_WEB_CLIENT_SECRET` | Regular Web Application used by the gateway BFF |
| `AUTH0_MANAGEMENT_API_CLIENT_ID`, `AUTH0_MANAGEMENT_API_CLIENT_SECRET`, `AUTH0_MANAGEMENT_API_AUDIENCE` | Machine to Machine application for the Management API |
| `AUTH0_WEBHOOK_SECRET` | Shared secret for the Auth0 post-registration webhook |
| `GITHUB_TOKEN` | Token with `read:packages` for GitHub NuGet |

### 2. Run the whole stack

```bash
docker compose --profile app up -d --build
```

Database migrations are applied on startup. When every container is healthy:

| URL | What |
| --- | --- |
| http://localhost:5173 | Web app |
| http://localhost:8080/swagger | Swagger UI for all services |
| http://localhost:15672 | RabbitMQ management |
| http://localhost:9001 | MinIO console |

Stop everything with `docker compose --profile app down` (add `-v` to remove the data volumes).

## Local development

Start only the infrastructure (databases, RabbitMQ, Redis, MinIO):

```bash
docker compose up -d
```

Run a service from your IDE or the command line, for example:

```bash
dotnet run --project src/Backend/Services/Polls/Polls.API
```

Run the frontend with hot reload (the Vite dev server proxies `/api`, `/bff` and `/hubs` to the gateway on `:8080`; stop the `citypulse_frontend` container first, it uses the same port):

```bash
cd src/frontend
npm ci
npm run dev
```

Useful frontend scripts:

| Command | What |
| --- | --- |
| `npm run lint` | ESLint |
| `npm run typecheck` | TypeScript check |
| `npm run format:check` | Prettier check |
| `npm run build` | Production build |
| `npm run gen:api` | Regenerate API types from the gateway Swagger |

## Testing

Backend tests are xUnit; integration tests start real PostgreSQL, MongoDB and RabbitMQ with Testcontainers, so Docker must be running.

```bash
dotnet test                                          # everything
dotnet test src/Backend/Services/Polls/Polls.Tests   # one service
```

CI runs the backend build and tests with coverage on SonarCloud, and lint, format check and build for the frontend.

## Deployment

Production runs the whole stack with Docker Compose on a single Linux VM in Azure. Every merge to `main` builds new images and deploys them automatically.

```mermaid
flowchart LR
    Dev["Merge to main"] --> Build

    subgraph Actions["GitHub Actions: Docker images"]
        Build["build ×5<br/>users, polls, feedback,<br/>gateway, frontend"]
        Deploy["deploy<br/>environment: production"]
        Build --> Deploy
    end

    Build -- "push :sha and :latest" --> GHCR[("GitHub Container<br/>Registry")]
    Deploy -- "SSH: copy deploy/,<br/>compose pull + up" --> VM

    subgraph VM["Azure VM: /opt/citypulse"]
        Caddy["Caddy :443<br/>Let's Encrypt"]
        Stack["frontend, gateway,<br/>services, databases"]
        Caddy --> Stack
    end

    GHCR -- "pull images" --> VM
    Users(["Users"]) -- HTTPS --> Caddy
```

### How the pipeline works

| Event | Workflow | What happens |
| --- | --- | --- |
| Pull request | `sonar.yml`, `frontend.yml` | Backend build, tests and SonarCloud analysis; frontend lint, format check and build. Nothing is published. |
| Push to `main` | `docker-images.yml` | Builds the five images in parallel and pushes them to `ghcr.io/subochartem/citypulse-<service>` tagged with the commit SHA and `latest`. When all builds succeed, the `deploy` job connects to the VM over SSH, copies `deploy/`, pulls the images of that commit and restarts the stack, waiting until every container is healthy. |
| Tag `*-v*` | `publish-*.yml` | Publishes a shared NuGet package to GitHub Packages. |

The workflow can also be started by hand from the Actions tab (**Docker images → Run workflow**).

### Production stack

`deploy/` holds everything the server needs besides secrets:

| File | What |
| --- | --- |
| `deploy/docker-compose.yml` | The same services as locally, but with images from GHCR (`IMAGE_TAG`), `restart: unless-stopped`, log rotation and no ports exposed except Caddy |
| `deploy/Caddyfile` | HTTPS for `DOMAIN` with an automatic Let's Encrypt certificate. `/api`, `/bff` and `/hubs` go to the gateway, `/polls-images` to MinIO, everything else to the frontend |
| `deploy/.env.example` | Template for the server `.env` |

Data (PostgreSQL, MongoDB, MinIO, Redis, Caddy certificates) lives in Docker volumes and survives deploys. Database migrations run when Users and Polls start. The whole stack uses about 1 GB of RAM.

### Setting up a server

1. Create a Linux VM with at least 2 vCPU and 4 GB RAM (for example Azure `B2als_v2`) and open ports 22, 80 and 443. Give it a DNS name.
2. Install Docker Engine with the Compose plugin and add a swap file.
3. Create `/opt/citypulse` and put a `.env` there based on `deploy/.env.example`: set `DOMAIN`, strong database passwords and the Auth0 values.
4. In Auth0, add `https://<DOMAIN>/bff/callback` to the web application's callback URLs and `https://<DOMAIN>` to its logout URLs, and point the post-registration webhook to `https://<DOMAIN>/api/v1/auth0webhook`.
5. Create an SSH key for deployments and add its public part to the server user's `authorized_keys`.
6. In GitHub, create the `production` environment with these secrets:

   | Secret | Value |
   | --- | --- |
   | `DEPLOY_HOST` | Server DNS name |
   | `DEPLOY_USER` | User that owns `/opt/citypulse` and is in the `docker` group |
   | `DEPLOY_SSH_KEY` | Private deploy key |
   | `DEPLOY_KNOWN_HOSTS` | Output of `ssh-keyscan -t ed25519 <DEPLOY_HOST>` |

7. Run the **Docker images** workflow. The images are public, so the server pulls them without logging in.

### Operations

```bash
cd /opt/citypulse
docker compose ps                                  # status
docker compose logs -f gateway                     # logs of one service
IMAGE_TAG=<previous commit sha> docker compose up -d   # roll back to an earlier version
```

If the VM is stopped when a merge happens, the `deploy` job fails with a connection timeout while the images are still published. Start the VM and use **Re-run failed jobs** on that run.

## Project structure

```text
CityPulse/
├── .github/workflows/           # CI, image build and deployment, NuGet publishing
├── docker-compose.yml           # infrastructure + app profile
├── deploy/                      # production compose file, Caddyfile, .env template
├── docker/                      # MinIO bucket policy
├── docs/screenshots/            # images for this README
└── src/
    ├── Backend/
    │   ├── Gateway/
    │   │   ├── Gateway.API      # YARP, BFF, Swagger aggregation
    │   │   └── Gateway.Tests
    │   └── Services/
    │       ├── Users/           # Presentation, Business, DataAccess, Tests
    │       ├── Polls/           # Domain, Application, Infrastructure, API, Tests
    │       ├── Feedback/        # Feedback.API (Features/*), Tests
    │       └── Shared/          # NuGet packages: Common.Api, Contracts.*
    └── frontend/                # React SPA, nginx config, Dockerfile
```

## License

Distributed under the [MIT License](LICENSE).
