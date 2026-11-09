import { describe, expect, it } from 'vitest';
import { validateClassForm } from './classForm';

describe('validateClassForm', () => {
  it('accepts a valid class', () => {
    expect(validateClassForm({ name: 'ষষ্ঠ শ্রেণি', totalSeats: '40' })).toEqual({});
  });

  it('requires a non-blank name and seats', () => {
    expect(validateClassForm({ name: '   ', totalSeats: '' })).toEqual({
      name: 'ক্লাসের নাম দিন',
      totalSeats: 'মোট আসনের সংখ্যা দিন',
    });
  });

  it('rejects long names and control characters', () => {
    expect(validateClassForm({ name: 'x'.repeat(201), totalSeats: '1' }).name).toMatch(/২০০/);
    expect(validateClassForm({ name: 'a\u0000b', totalSeats: '1' }).name).toMatch(/অগ্রহণযোগ্য/);
  });

  it('requires a positive whole number of seats', () => {
    expect(validateClassForm({ name: 'A', totalSeats: '0' }).totalSeats).toBe('মোট আসন অন্তত ১ হতে হবে');
    expect(validateClassForm({ name: 'A', totalSeats: '1.5' }).totalSeats).toMatch(/পূর্ণসংখ্যা/);
    expect(validateClassForm({ name: 'A', totalSeats: '-3' }).totalSeats).toMatch(/পূর্ণসংখ্যা/);
    expect(validateClassForm({ name: 'A', totalSeats: '99999999999' }).totalSeats).toMatch(/অনেক বেশি/);
  });
});
