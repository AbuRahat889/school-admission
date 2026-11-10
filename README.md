# School Admission System

A Dockerized school admission system with an ASP.NET Core .NET 8 Web API, PostgreSQL, and a React/Vite frontend. Students can view classes, place temporary holds, and confirm or cancel applications.

## Prerequisites

- Docker
- Docker Compose

## Run

```bash
docker compose up --build
```

Open the frontend at [http://localhost:3000](http://localhost:3000). The backend is available at [http://localhost:8080](http://localhost:8080), with Swagger at `/swagger`.

### Environment variables

Compose reads these values from the root `.env` file and also has the documented defaults, so a fresh clone can start without a local `.env`. `.env.example` contains the same safe defaults.

| Name | Default | Purpose |
|---|---|---|
| `POSTGRES_DB` | `school_admission` | PostgreSQL database name |
| `POSTGRES_USER` | `postgres` | PostgreSQL user |
| `POSTGRES_PASSWORD` | `postgres` | PostgreSQL password |
| `ConnectionStrings__Default` | `Host=db;Port=5432;Database=school_admission;Username=postgres;Password=postgres` | Backend PostgreSQL connection string |
| `HOLD_DURATION_SECONDS` | `600` | Duration of a new hold |
| `VITE_API_URL` | `http://localhost:8080` | Backend URL baked into the frontend at build time |

Stop the containers with:

```bash
docker compose down
```

The PostgreSQL data is stored in the `pgdata` named volume, so it remains available after `docker compose down` followed by `docker compose up`.

## API

For hold-related endpoints, `X-Student-Id` is the required 17-digit birth registration number. The create-class and read-hold endpoints do not require this header.

| Method | Path | Header | Success |
|---|---|---|---|
| POST | `/classes` | — | 201 Created |
| GET | `/classes` | — | 200 OK |
| GET | `/classes/{classId}` | — | 200 OK |
| POST | `/classes/{classId}/holds` | `X-Student-Id` | 201 Created |
| GET | `/holds/{holdId}` | — | 200 OK |
| POST | `/holds/{holdId}/confirm` | `X-Student-Id` | 200 OK |
| DELETE | `/holds/{holdId}` | `X-Student-Id` | 204 No Content |
| GET | `/me/holds` | `X-Student-Id` | 200 OK |

Invalid input returns a 4xx response with a JSON error body. A full class returns 409 Conflict.

## Tests

There is no backend test project in the solution. The available solution-level command is:

```bash
dotnet test backend/SchoolAdmission.sln
```

Frontend tests are present and run with:

```bash
cd frontend
npm ci
npm test
```

## Project structure

```text
backend/                 ASP.NET Core API, EF Core model, migrations, and services
frontend/                React/Vite application and Vitest tests
docker-compose.yml       PostgreSQL, backend, and frontend services
backend/Dockerfile       Backend build/runtime image
frontend/Dockerfile      Frontend build/Nginx image
```
