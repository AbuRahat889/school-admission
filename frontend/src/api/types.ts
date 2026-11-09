// Shapes mirror the API contract in CLAUDE.md exactly.

/** Stored statuses plus the derived EXPIRED (server returns it for lapsed HELD holds). */
export type HoldStatus = 'HELD' | 'CONFIRMED' | 'CANCELLED' | 'EXPIRED';

export interface ClassCreated {
  id: string;
  name: string;
  totalSeats: number;
}

export interface ClassSummary {
  id: string;
  name: string;
  totalSeats: number;
  heldSeats: number;
  confirmedSeats: number;
  availableSeats: number;
}

export interface HoldCreated {
  holdId: string;
  classId: string;
  studentId: string;
  status: HoldStatus;
  expiresAt: string;
}

export interface Hold {
  holdId: string;
  classId: string;
  status: HoldStatus;
  expiresAt: string;
}

export interface HoldConfirmed {
  holdId: string;
  status: HoldStatus;
}

export interface MyHold {
  holdId: string;
  classId: string;
  className: string;
  status: HoldStatus;
  expiresAt: string;
}

/** Uniform error body returned by the backend for every 4xx/5xx. */
export interface ErrorResponse {
  statusCode: number;
  message: string;
  errors: Record<string, string[]>;
  timestamp: string;
}
