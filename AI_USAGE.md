# AI usage log

Running list of what the AI assistant (Claude Code) did, and mistakes caught by me.

## Steps

1. **chore(repo): init structure** — created `backend/`, `frontend/`, `.gitignore` for .NET, Node, Docker. Kept the existing `CLAUDE.md` line in `.gitignore`.

2. **feat(backend): scaffold .NET 8 Web API** — `backend/SchoolAdmission.Api` (net8.0) + `SchoolAdmission.sln`, Kestrel on `http://0.0.0.0:8080`, `GET /health` -> 200 `{"status":"ok"}`, Swagger UI. The installed `dotnet new webapi` template only supports net10.0 (only SDK 10 installed), so the project files were written by hand for net8.0. Local runs use `DOTNET_ROLL_FORWARD=Major` (shell only) because no .NET 8 runtime is installed; Docker will use the 8.0 images.
3. **feat(db): EF Core + Npgsql, Class/Hold entities, initial migration** — EF Core 8.0.11, Npgsql 8.0.11, EFCore.NamingConventions (snake_case). `classes` (id, name, total_seats > 0) and `holds` (id, class_id FK, student_id char(17) with 17-digit check, status HELD|CONFIRMED|CANCELLED stored as uppercase strings, expires_at, created_at, confirmed_at). Partial unique index `ux_holds_active_student_class` on (student_id, class_id) WHERE status IN ('HELD','CONFIRMED') for rule 3. Connection string comes from env var `ConnectionStrings__Default`, and the app fails fast if it is missing. A design-time factory lets `dotnet ef` run without a DB. Migration: `Data/Migrations/*_InitialCreate`.
   - Self-noticed slip: in step 2 my cleanup command didn't stop the test API process, which then locked `bin/Debug`. I couldn't kill it, so I generated the migration via a Release build.
4. **feat(docker): backend Dockerfile + docker-compose** — multi-stage `backend/Dockerfile` (build `sdk:10.0`, runtime `aspnet:8.0`, port 8080, non-root `$APP_UID`). Root `docker-compose.yml`: `db` = `postgres:16-alpine` with named volume `pgdata` and a `pg_isready` healthcheck (not published to the host; a local Postgres 18 already uses 5432). `backend` depends on `db` being healthy, with env `ConnectionStrings__Default` and `HOLD_DURATION_SECONDS=600`. EF migrations are applied at startup with a short retry. Verified with `docker compose up --build`: `GET /health` returned 200 and the tables and indexes exist.
   - Deviation from the request: asked for an `aspnet:10.0` runtime, but CLAUDE.md fixes the stack at .NET 8 and the project targets net8.0, so the runtime is `aspnet:8.0`. Flagged to the user.

## Mistakes caught

_(none yet)_
