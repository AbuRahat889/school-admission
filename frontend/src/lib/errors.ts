// Maps backend (English) error messages to Bangla. Raw English server text is never shown.
// Order: exact known message -> known pattern -> status-code fallback -> generic.

export const NETWORK_ERROR_BN = 'সার্ভারের সাথে সংযোগ করা যাচ্ছে না';
export const GENERIC_ERROR_BN = 'কিছু একটা ভুল হয়েছে, আবার চেষ্টা করুন';

const DUPLICATE_HOLD = 'You already have an active hold for this class.';

/** 409 because the student already holds/has confirmed a seat in this class (vs. class full). */
export const isDuplicateHoldError = (e: { status: number; serverMessage?: string }): boolean =>
  e.status === 409 && e.serverMessage === DUPLICATE_HOLD;

const KNOWN_MESSAGES: Record<string, string> = {
  'This class is full. No seats are available right now.': 'এই ক্লাসে কোনো আসন খালি নেই',
  [DUPLICATE_HOLD]:'এই ক্লাসে আপনার ইতিমধ্যে একটি সক্রিয় আবেদন আছে',
  'This hold has expired. Please place a new hold.': 'মেয়াদ শেষ হয়ে গেছে, আবার আবেদন করুন',
  'This hold was cancelled and can no longer be confirmed.': 'এই আবেদনটি বাতিল করা হয়েছে, এটি আর নিশ্চিত করা যাবে না',
  'A confirmed admission cannot be cancelled.': 'নিশ্চিত ভর্তি বাতিল করা যায় না',
  'This hold belongs to another student.': 'এই আবেদনটি অন্য শিক্ষার্থীর, আপনি এটি পরিবর্তন করতে পারবেন না',
  'X-Student-Id header is required.': 'শিক্ষার্থীর জন্ম নিবন্ধন নম্বর দিন',
  'X-Student-Id must be exactly 17 digits.': 'জন্ম নিবন্ধন নম্বর ঠিক ১৭ অঙ্কের হতে হবে',
  'Name is required.': 'ক্লাসের নাম দিন',
  'Name must be at most 200 characters.': 'ক্লাসের নাম সর্বোচ্চ ২০০ অক্ষরের হতে পারে',
  'Name must not contain control characters.': 'ক্লাসের নামে অগ্রহণযোগ্য অক্ষর আছে',
  'totalSeats is required.': 'মোট আসনের সংখ্যা দিন',
  'totalSeats must be greater than 0.': 'মোট আসন অন্তত ১ হতে হবে',
  'Content-Type must be application/json.': 'অনুরোধের ধরন সঠিক নয়',
};

const KNOWN_PATTERNS: Array<[RegExp, string]> = [
  [/^Class .+ was not found\.$/, 'ক্লাসটি খুঁজে পাওয়া যায়নি'],
  [/^Hold .+ was not found\.$/, 'আবেদনটি খুঁজে পাওয়া যায়নি'],
  [/^totalSeats: /, 'মোট আসন একটি পূর্ণসংখ্যা হতে হবে'],
  [/^name: /, 'ক্লাসের নাম সঠিক নয়'],
];

const BY_STATUS: Record<number, string> = {
  400: 'অনুরোধটি সঠিক নয়, তথ্য যাচাই করে আবার চেষ্টা করুন',
  403: 'এই কাজটি করার অনুমতি আপনার নেই',
  404: 'খুঁজে পাওয়া যায়নি',
  405: 'এই কাজটি এখানে করা যায় না',
  409: 'বর্তমান অবস্থায় এই কাজটি করা যাচ্ছে না',
  415: 'অনুরোধের ধরন সঠিক নয়',
};

/** Bangla text for a server error message + HTTP status. */
export function toBanglaMessage(serverMessage: string | undefined, status: number): string {
  if (serverMessage) {
    const exact = KNOWN_MESSAGES[serverMessage];
    if (exact) return exact;
    for (const [re, bn] of KNOWN_PATTERNS) if (re.test(serverMessage)) return bn;
  }
  if (status >= 500) return 'সার্ভারে সমস্যা হয়েছে, কিছুক্ষণ পর আবার চেষ্টা করুন';
  return BY_STATUS[status] ?? GENERIC_ERROR_BN;
}

/** Bangla per-field errors (keys as sent by the server: name, totalSeats, X-Student-Id, body). */
export function toBanglaFieldErrors(
  errors: Record<string, string[]> | undefined,
  status: number,
): Record<string, string> {
  const out: Record<string, string> = {};
  for (const [field, messages] of Object.entries(errors ?? {})) {
    const first = messages[0];
    if (first === undefined) continue;
    // JSON-type errors arrive without a field prefix; add it so the patterns above can match.
    const msg = first.startsWith('Has an invalid value') ? `${field}: ${first}` : first;
    out[field] = toBanglaMessage(msg, status);
  }
  return out;
}
