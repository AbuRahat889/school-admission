import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { STUDENT_ID_KEY, isValidStudentId, readStudentId, writeStudentId } from './studentId';

interface StudentIdState {
  /** Raw value as typed (digits only), also what is stored in localStorage. */
  studentId: string;
  isValid: boolean;
  setStudentId: (value: string) => void;
}

const StudentIdContext = createContext<StudentIdState | null>(null);

/** Bangla digits (০-৯) are converted to 0-9; everything else that isn't a digit is dropped. */
export function normalizeStudentIdInput(raw: string): string {
  return raw
    .replace(/[০-৯]/g, (d) => String(d.charCodeAt(0) - 0x09e6))
    .replace(/[^0-9]/g, '')
    .slice(0, 17);
}

export function StudentIdProvider({ children }: { children: ReactNode }) {
  const [studentId, setState] = useState(readStudentId);

  const setStudentId = useCallback((value: string) => {
    writeStudentId(value);
    setState(value);
  }, []);

  // Keep several open tabs in sync.
  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key === STUDENT_ID_KEY) setState(e.newValue ?? '');
    };
    window.addEventListener('storage', onStorage);
    return () => window.removeEventListener('storage', onStorage);
  }, []);

  const value = useMemo(
    () => ({ studentId, isValid: isValidStudentId(studentId), setStudentId }),
    [studentId, setStudentId],
  );
  return <StudentIdContext.Provider value={value}>{children}</StudentIdContext.Provider>;
}

export function useStudentId(): StudentIdState {
  const ctx = useContext(StudentIdContext);
  if (!ctx) throw new Error('useStudentId must be used inside <StudentIdProvider>');
  return ctx;
}
