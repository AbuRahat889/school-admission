# AI usage log

Running list of what the AI assistant (Claude Code) did, and mistakes caught by me.

## Steps

1. **chore(repo): init structure** — created `backend/`, `frontend/`, `.gitignore` for .NET, Node, Docker. Kept the existing `CLAUDE.md` line in `.gitignore`.

2. **feat(backend): scaffold .NET 8 Web API** — `backend/SchoolAdmission.Api` (net8.0) + `SchoolAdmission.sln`, Kestrel on `http://0.0.0.0:8080`, `GET /health` -> 200 `{"status":"ok"}`, Swagger UI. The installed `dotnet new webapi` template only supports net10.0 (only SDK 10 installed), so the project files were written by hand for net8.0. Local runs use `DOTNET_ROLL_FORWARD=Major` (shell only) because no .NET 8 runtime is installed; Docker will use the 8.0 images.

## Mistakes caught

_(none yet)_
