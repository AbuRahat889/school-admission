import { describe, expect, it } from 'vitest';
import { isValidStudentId } from './studentId';
import { normalizeStudentIdInput } from './studentIdContext';

describe('student id', () => {
  it('accepts exactly 17 ASCII digits', () => {
    expect(isValidStudentId('12345678901234567')).toBe(true);
    expect(isValidStudentId('1234567890123456')).toBe(false);
    expect(isValidStudentId('123456789012345678')).toBe(false);
    expect(isValidStudentId('1234567890123456a')).toBe(false);
  });

  it('normalizes input: Bangla digits -> 0-9, drops other chars, caps at 17', () => {
    expect(normalizeStudentIdInput('২০১৭ 1234-567')).toBe('20171234567');
    expect(normalizeStudentIdInput('1'.repeat(20))).toBe('1'.repeat(17));
  });
});
