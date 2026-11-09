// The student id (17-digit birth registration number) lives in localStorage so it
// survives reloads and is shared by every page and the API client.

export const STUDENT_ID_KEY = 'studentId';
export const STUDENT_ID_PATTERN = /^[0-9]{17}$/;

export const isValidStudentId = (value: string): boolean => STUDENT_ID_PATTERN.test(value);

/** localStorage can throw (private mode, blocked storage); treat that as "no id". */
export function readStudentId(): string {
  try {
    return localStorage.getItem(STUDENT_ID_KEY) ?? '';
  } catch {
    return '';
  }
}

export function writeStudentId(value: string): void {
  try {
    if (value) localStorage.setItem(STUDENT_ID_KEY, value);
    else localStorage.removeItem(STUDENT_ID_KEY);
  } catch {
    // Storage unavailable: the id still works for this session via React state.
  }
}
