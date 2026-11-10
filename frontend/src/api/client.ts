import {
  GENERIC_ERROR_BN,
  NETWORK_ERROR_BN,
  toBanglaFieldErrors,
  toBanglaMessage,
} from "../lib/errors";
import { readStudentId } from "../lib/studentId";
import type {
  ClassCreated,
  ClassSummary,
  ErrorResponse,
  Hold,
  HoldConfirmed,
  HoldCreated,
  MyHold,
} from "./types";

export const API_URL: string = (
  import.meta.env.VITE_API_URL || "http://localhost:8080"
).replace(/\/+$/, "");

/**
 * Every failed request becomes an ApiError.
 * - message:       Bangla, safe to show to the user
 * - serverMessage: the backend's original (English) ErrorResponse.message, for logs/debugging only
 */
export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number, // 0 = network failure / server unreachable
    readonly serverMessage?: string,
    readonly fieldErrors: Record<string, string> = {},
  ) {
    super(message);
    this.name = "ApiError";
  }

  get isNetworkError(): boolean {
    return this.status === 0;
  }
}

/** Normalizes anything thrown (ApiError or an unexpected bug) into an ApiError with Bangla text. */
export function asApiError(e: unknown): ApiError {
  if (e instanceof ApiError) return e;
  console.error("Unexpected error", e);
  return new ApiError(GENERIC_ERROR_BN, -1);
}

// --- connectivity events (used by the global error banner) -------------------------------

type ApiEvent = { type: "ok" } | { type: "error"; error: ApiError };
const listeners = new Set<(e: ApiEvent) => void>();

export function subscribeApiEvents(fn: (e: ApiEvent) => void): () => void {
  listeners.add(fn);
  return () => listeners.delete(fn);
}

const emit = (e: ApiEvent) => listeners.forEach((fn) => fn(e));

// --- core request -----------------------------------------------------------------------

function isErrorResponse(body: unknown): body is ErrorResponse {
  return (
    typeof body === "object" &&
    body !== null &&
    "message" in body &&
    "statusCode" in body
  );
}

async function request<T>(
  method: string,
  path: string,
  body?: unknown,
): Promise<T> {
  const headers: Record<string, string> = { Accept: "application/json" };
  const studentId = readStudentId();
  if (studentId) headers["X-Student-Id"] = studentId;
  if (body !== undefined) headers["Content-Type"] = "application/json";

  let res: Response;
  try {
    res = await fetch(`${API_URL}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    const error = new ApiError(NETWORK_ERROR_BN, 0);
    emit({ type: "error", error });
    throw error;
  }

  if (res.status === 204) {
    emit({ type: "ok" });
    return undefined as T;
  }

  let parsed: unknown = undefined;
  const text = await res.text().catch(() => "");
  if (text) {
    try {
      parsed = JSON.parse(text);
    } catch {
      parsed = undefined; // e.g. an HTML error page from a proxy
    }
  }

  if (!res.ok) {
    const serverMessage = isErrorResponse(parsed) ? parsed.message : undefined;
    const fieldErrors = isErrorResponse(parsed)
      ? toBanglaFieldErrors(parsed.errors, res.status)
      : {};
    const error = new ApiError(
      toBanglaMessage(serverMessage, res.status),
      res.status,
      serverMessage,
      fieldErrors,
    );
    emit({ type: "error", error });
    throw error;
  }

  emit({ type: "ok" });
  return parsed as T;
}

// --- endpoints (CLAUDE.md contract) ------------------------------------------------------

export const api = {
  listClasses: () => request<ClassSummary[]>("GET", "/classes"),
  getClass: (classId: string) =>
    request<ClassSummary>("GET", `/classes/${encodeURIComponent(classId)}`),
  createClass: (name: string, totalSeats: number) =>
    request<ClassCreated>("POST", "/classes", { name, totalSeats }),
  createHold: (classId: string) =>
    request<HoldCreated>(
      "POST",
      `/classes/${encodeURIComponent(classId)}/holds`,
    ),
  getHold: (holdId: string) =>
    request<Hold>("GET", `/holds/${encodeURIComponent(holdId)}`),
  confirmHold: (holdId: string) =>
    request<HoldConfirmed>(
      "POST",
      `/holds/${encodeURIComponent(holdId)}/confirm`,
    ),
  cancelHold: (holdId: string) =>
    request<void>("DELETE", `/holds/${encodeURIComponent(holdId)}`),
  myHolds: () => request<MyHold[]>("GET", "/me/holds"),
};
