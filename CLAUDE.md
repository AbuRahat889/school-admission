# School Admission System — take-home assignment

## Stack (fixed)
- Backend: C# .NET 8 Web API, EF Core, PostgreSQL. Runs on http://localhost:8080
- Frontend: React 18 + Vite + TypeScript. Backend URL from VITE_API_URL
- Docker: backend/Dockerfile, frontend/Dockerfile, root docker-compose.yml
  (postgres + backend + frontend, named volume for DB). `docker compose up` must be enough.

## API contract (must match exactly, extra fields allowed, no renames)
POST /classes {name,totalSeats} -> 201 {id,name,totalSeats}
GET /classes -> [{id,name,totalSeats,heldSeats,confirmedSeats,availableSeats}]
GET /classes/{classId} -> same shape as above
POST /classes/{classId}/holds (header X-Student-Id) -> 201 {holdId,classId,studentId,status,expiresAt}; 409 if full
GET /holds/{holdId} -> {holdId,classId,status,expiresAt}; status: HELD|CONFIRMED|CANCELLED
POST /holds/{holdId}/confirm (header X-Student-Id) -> 200 {holdId,status}
DELETE /holds/{holdId} (header X-Student-Id) -> 204
GET /me/holds (header X-Student-Id) -> [{holdId,classId,className,status,expiresAt}]

## Rules
1. held + confirmed never exceeds totalSeats under concurrent requests (use SELECT ... FOR UPDATE on the class row inside a transaction)
2. HOLD_DURATION_SECONDS env var, default 600. Expiry is derived: a HELD hold with expiresAt < now is EXPIRED; no background job, no API call needed
3. One active (HELD or CONFIRMED) hold per student per class (partial unique index)
4. Only own, non-expired HELD hold can be confirmed
5. Confirm is idempotent — confirming twice returns 200, count increases once
6. Data survives docker compose down/up
7. Invalid input -> 4xx, never 500. X-Student-Id must be exactly 17 digits, else 400
8. UI: polling every 3–5s, countdown, EXPIRED shown without reload, student id in localStorage, human-readable errors

## Workflow
- Work in small steps. After each step: build/test, show me a summary, then commit with a conventional message (feat/fix/test/chore/docs(scope): ...). Never one big commit.
- Keep a running list in AI_USAGE.md of what you did and any mistakes you made that I caught.
- Do not add Redis or any other service beyond postgres, backend, frontend.

## Commits
- Plain conventional commit messages only, one line subject, optional short body.
- NEVER add trailers: no Co-Authored-By, no "Generated with Claude Code", no Claude-Session.

## UI language
- All user-facing text in the frontend is Bangla (Bengali). Labels, buttons, status badges, error messages, placeholders, page titles — everything. Code, variable names and comments stay English.
- Status display: HELD → "অপেক্ষমাণ", CONFIRMED → "নিশ্চিত", EXPIRED → "মেয়াদ শেষ", CANCELLED → "বাতিল". API values stay uppercase English.
- Use a Bangla-capable font stack: "Noto Sans Bengali", "Hind Siliguri", system-ui, sans-serif. Load Noto Sans Bengali from Google Fonts in index.html.
- Numbers may stay in Latin digits (0-9).



