import { describe, expect, it } from 'vitest';
import { effectiveStatus, formatCountdown, statusLabel } from './holdStatus';

describe('effectiveStatus', () => {
  const t = Date.parse('2026-01-01T10:00:00Z');

  it('turns a lapsed HELD hold into EXPIRED', () => {
    expect(effectiveStatus('HELD', '2026-01-01T10:00:00Z', t)).toBe('EXPIRED');
    expect(effectiveStatus('HELD', '2026-01-01T09:59:59Z', t)).toBe('EXPIRED');
    expect(effectiveStatus('HELD', '2026-01-01T10:00:01Z', t)).toBe('HELD');
  });

  it('never changes other statuses', () => {
    expect(effectiveStatus('CONFIRMED', '2020-01-01T00:00:00Z', t)).toBe('CONFIRMED');
    expect(effectiveStatus('CANCELLED', '2020-01-01T00:00:00Z', t)).toBe('CANCELLED');
    expect(effectiveStatus('EXPIRED', '2099-01-01T00:00:00Z', t)).toBe('EXPIRED');
  });
});

describe('formatCountdown', () => {
  it('formats mm:ss and clamps at zero', () => {
    expect(formatCountdown(600_000)).toBe('10:00');
    expect(formatCountdown(61_000)).toBe('01:01');
    expect(formatCountdown(500)).toBe('00:01');
    expect(formatCountdown(0)).toBe('00:00');
    expect(formatCountdown(-5000)).toBe('00:00');
    expect(formatCountdown(3_700_000)).toBe('61:40');
  });
});

describe('statusLabel', () => {
  it('maps API statuses to Bangla', () => {
    expect(statusLabel('HELD')).toBe('অপেক্ষমাণ');
    expect(statusLabel('CONFIRMED')).toBe('নিশ্চিত');
    expect(statusLabel('EXPIRED')).toBe('মেয়াদ শেষ');
    expect(statusLabel('CANCELLED')).toBe('বাতিল');
    expect(statusLabel('SOMETHING_NEW')).toBe('অজানা');
  });
});
