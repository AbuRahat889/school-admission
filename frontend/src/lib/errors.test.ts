import { describe, expect, it } from 'vitest';
import { GENERIC_ERROR_BN, toBanglaFieldErrors, toBanglaMessage } from './errors';

describe('toBanglaMessage', () => {
  it('maps known server messages exactly', () => {
    expect(toBanglaMessage('This class is full. No seats are available right now.', 409)).toBe(
      'এই ক্লাসে কোনো আসন খালি নেই',
    );
    expect(toBanglaMessage('This hold has expired. Please place a new hold.', 409)).toBe(
      'মেয়াদ শেষ হয়ে গেছে, আবার আবেদন করুন',
    );
  });

  it('maps not-found messages that contain ids', () => {
    expect(toBanglaMessage('Hold 0000-1111 was not found.', 404)).toBe('আবেদনটি খুঁজে পাওয়া যায়নি');
    expect(toBanglaMessage('Class abc was not found.', 404)).toBe('ক্লাসটি খুঁজে পাওয়া যায়নি');
  });

  it('falls back by status code for unknown English text, never echoing it', () => {
    const msg = toBanglaMessage('Some brand new English error', 409);
    expect(msg).toBe('বর্তমান অবস্থায় এই কাজটি করা যাচ্ছে না');
    expect(msg).not.toMatch(/[A-Za-z]/);
  });

  it('uses a server-problem message for 5xx and generic for odd statuses', () => {
    expect(toBanglaMessage(undefined, 503)).toMatch(/সার্ভারে সমস্যা/);
    expect(toBanglaMessage(undefined, 418)).toBe(GENERIC_ERROR_BN);
  });
});

describe('toBanglaFieldErrors', () => {
  it('maps per-field messages including JSON type errors', () => {
    expect(
      toBanglaFieldErrors(
        { name: ['Name is required.'], totalSeats: ['Has an invalid value or the JSON is malformed.'] },
        400,
      ),
    ).toEqual({ name: 'ক্লাসের নাম দিন', totalSeats: 'মোট আসন একটি পূর্ণসংখ্যা হতে হবে' });
  });
});
