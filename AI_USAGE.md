# AI Usage

## AI tools used

- GitHub Copilot
- Claude

## Which parts of the project each tool helped with

- Backend: `<WRITE DETAILS HERE>`
- Frontend: `<WRITE DETAILS HERE>`
- Docker: `<WRITE DETAILS HERE>`
- Tests: `<WRITE DETAILS HERE>`
- Documentation: `<WRITE DETAILS HERE>`

## Where AI made mistakes and how I caught and fixed them

1. `<WRITE REAL EXAMPLE HERE>`
2. `<WRITE REAL EXAMPLE HERE>`
3. `<WRITE REAL EXAMPLE HERE>`

## What I wrote or decided myself

`<WRITE DETAILS HERE>`

## How I verified the AI-generated code

- Backend: `<WRITE DETAILS HERE>`
- Frontend: `<WRITE DETAILS HERE>`
- Docker/manual testing: `<WRITE DETAILS HERE>`

## Code inconsistencies to review before submission

- The backend Dockerfile builds with a .NET 10 SDK image even though the project targets .NET 8.
- Expiry is treated as expired at `expiresAt <= now`, while the assignment wording says `expiresAt < now`.
- Unexpected server failures are logged and returned as HTTP 500; only known invalid-input and client/database cases are mapped to 4xx.
- No backend test project is tracked in the solution; the tracked automated tests are frontend Vitest tests.
