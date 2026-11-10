# Assumptions and implementation decisions

This document records behavior implemented in the current code, rather than adding behavior that is not present.

## Concurrency and seat capacity

Hold creation starts a database transaction and locks the matching class row with `SELECT * FROM classes WHERE id = ... FOR UPDATE`. It then counts confirmed holds and non-expired held holds before inserting the new hold. Confirmation and cancellation also acquire the same class-row lock. Because all write paths serialize on the class row and the counts come from PostgreSQL, held plus confirmed seats cannot exceed `totalSeats`, including when multiple backend processes share the same database.

## Expiry

`EXPIRED` is derived and is never stored. A stored `HELD` hold is returned as `EXPIRED` when `expiresAt <= now` (the implementation includes the exact expiry instant), and class seat counts include only `HELD` rows with `expiresAt > now`. There is no background job and reads do not mutate the row. This makes expiry restart-safe and consistent across backend instances because the timestamp and comparison are in the database-backed data and request-time clock.

## One active hold per student and class

The database migration creates the partial unique index `ux_holds_active_student_class` on `(student_id, class_id)` where `status IN ('HELD','CONFIRMED')`. When a held row has expired, a new hold request retires that old row as `CANCELLED` under the class lock before inserting the replacement, because the index is based on stored status rather than time.

## Student ID validation

The API requires `X-Student-Id` on hold creation, confirmation, cancellation, and `GET /me/holds`. Missing or non-17-digit values are rejected with 400. The regular expression accepts exactly 17 ASCII digits, and the database also has a matching check constraint.

## Confirmation and status codes

Only the owner can confirm a hold. A missing hold returns 404 and another student's hold returns 403. An expired or cancelled hold returns 409. Confirming an already `CONFIRMED` hold returns 200 without changing it, so the confirmed count increases only once. Creating a hold for a full class returns 409.

## Delete and cancellation

Deleting a held hold changes its stored status to `CANCELLED`, which releases its held seat. Deleting an already cancelled or expired hold is a successful no-op and returns 204. A confirmed admission cannot be cancelled and returns 409.

## Error handling

The global error middleware converts application exceptions and malformed requests into the uniform `ErrorResponse` shape. Controller validation returns 400, and known database input/constraint failures are mapped to 4xx responses. Unexpected exceptions are logged and currently return 500 with a generic message, so the code does not literally guarantee that no 500 can ever occur.

## Payment simulation

The frontend's `ভর্তি ফি দিন` button directly calls the confirmation endpoint. There is no payment provider or payment service; pressing the button simulates a successful payment/confirmation.

## Assignment-rule inconsistencies to review

- The root compose file now consumes the root environment variables, but the backend Dockerfile uses the .NET 10 SDK image to build a project targeting .NET 8. The runtime image remains .NET 8.
- The code's expiry boundary is `expiresAt <= now`, while the assignment wording says `expiresAt < now`.
- Unexpected server failures still produce HTTP 500; only invalid input and known client/database errors are mapped to 4xx responses.
- There is no backend test project in the tracked solution; only frontend Vitest tests are present.
