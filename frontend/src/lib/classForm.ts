// Client-side validation for the create-class form. Mirrors the backend rules
// (name required, ≤ 200 chars, no control characters; totalSeats integer > 0) so most
// mistakes are caught before a request; the server stays the source of truth.

export const MAX_NAME_LENGTH = 200;
const MAX_INT = 2147483647;

export interface ClassFormValues {
  name: string;
  totalSeats: string; // raw input text
}

export type ClassFormErrors = Partial<Record<keyof ClassFormValues, string>>;

// eslint-disable-next-line no-control-regex
const CONTROL_CHARS = /[\u0000-\u001f\u007f-\u009f]/;

export function validateClassForm(v: ClassFormValues): ClassFormErrors {
  const errors: ClassFormErrors = {};

  const name = v.name.trim();
  if (!name) errors.name = 'ক্লাসের নাম দিন';
  else if (name.length > MAX_NAME_LENGTH) errors.name = 'ক্লাসের নাম সর্বোচ্চ ২০০ অক্ষরের হতে পারে';
  else if (CONTROL_CHARS.test(name)) errors.name = 'ক্লাসের নামে অগ্রহণযোগ্য অক্ষর আছে';

  const seats = v.totalSeats.trim();
  if (!seats) errors.totalSeats = 'মোট আসনের সংখ্যা দিন';
  else if (!/^\d+$/.test(seats)) errors.totalSeats = 'মোট আসন একটি পূর্ণসংখ্যা হতে হবে';
  else if (Number(seats) < 1) errors.totalSeats = 'মোট আসন অন্তত ১ হতে হবে';
  else if (Number(seats) > MAX_INT) errors.totalSeats = 'মোট আসনের সংখ্যা অনেক বেশি';

  return errors;
}
