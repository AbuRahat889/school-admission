import { useEffect, useState } from 'react';
import type { HoldStatus } from '../api/types';

// API values stay uppercase English; only the display text is Bangla (CLAUDE.md "UI language").
export const STATUS_LABEL_BN: Record<HoldStatus, string> = {
  HELD: 'অপেক্ষমাণ',
  CONFIRMED: 'নিশ্চিত',
  EXPIRED: 'মেয়াদ শেষ',
  CANCELLED: 'বাতিল',
};

export const statusLabel = (s: string): string => STATUS_LABEL_BN[s as HoldStatus] ?? 'অজানা';

/**
 * Status as it should be shown right now. The server already derives EXPIRED when polled,
 * but between polls a HELD hold whose expiresAt has passed is shown as EXPIRED immediately.
 */
export function effectiveStatus(status: HoldStatus, expiresAt: string, nowMs: number): HoldStatus {
  if (status !== 'HELD') return status;
  const expires = Date.parse(expiresAt);
  return Number.isFinite(expires) && expires <= nowMs ? 'EXPIRED' : 'HELD';
}

/** Remaining time as "মম:সস" (minutes may exceed 59 for long holds), never negative. */
export function formatCountdown(remainingMs: number): string {
  const total = Math.max(0, Math.ceil(remainingMs / 1000));
  const minutes = Math.floor(total / 60);
  const seconds = total % 60;
  return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
}

/** Current time, re-rendering every `intervalMs` (drives countdowns client-side). */
export function useNow(intervalMs = 1000): number {
  const [now, setNow] = useState(() => Date.now());
  useEffect(() => {
    const id = setInterval(() => setNow(Date.now()), intervalMs);
    return () => clearInterval(id);
  }, [intervalMs]);
  return now;
}
