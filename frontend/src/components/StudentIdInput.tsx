import { useId } from 'react';
import { normalizeStudentIdInput, useStudentId } from '../lib/studentIdContext';

export const STUDENT_ID_LABEL = 'শিক্ষার্থীর জন্ম নিবন্ধন নম্বর (১৭ অঙ্ক)';

export function StudentIdInput() {
  const { studentId, isValid, setStudentId } = useStudentId();
  const inputId = useId();
  const hintId = useId();

  let hint: { text: string; tone: 'ok' | 'error' | 'muted' };
  if (!studentId) hint = { text: 'আবেদন করতে নম্বরটি দিন', tone: 'muted' };
  else if (isValid) hint = { text: '✓ নম্বরটি সংরক্ষিত হয়েছে', tone: 'ok' };
  else hint = { text: `ঠিক ১৭টি অঙ্ক দিন (এখন ${studentId.length}টি)`, tone: 'error' };

  return (
    <div className="student-id">
      <label htmlFor={inputId}>{STUDENT_ID_LABEL}</label>
      <input
        id={inputId}
        className={`input${studentId && !isValid ? ' input-invalid' : ''}`}
        inputMode="numeric"
        autoComplete="off"
        placeholder="যেমন: 20171234567890123"
        maxLength={17}
        value={studentId}
        aria-invalid={studentId !== '' && !isValid}
        aria-describedby={hintId}
        onChange={(e) => setStudentId(normalizeStudentIdInput(e.target.value))}
      />
      <small id={hintId} className={`hint hint-${hint.tone}`}>
        {hint.text}
      </small>
    </div>
  );
}
