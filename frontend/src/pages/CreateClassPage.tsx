import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { api, asApiError, type ApiError } from '../api/client';
import type { ClassCreated } from '../api/types';
import { MAX_NAME_LENGTH, validateClassForm, type ClassFormErrors, type ClassFormValues } from '../lib/classForm';
import { usePageTitle } from '../lib/usePageTitle';

const EMPTY: ClassFormValues = { name: '', totalSeats: '' };

export function CreateClassPage() {
  usePageTitle('নতুন ক্লাস তৈরি');
  const [values, setValues] = useState<ClassFormValues>(EMPTY);
  const [touched, setTouched] = useState<Partial<Record<keyof ClassFormValues, boolean>>>({});
  const [serverError, setServerError] = useState<ApiError>();
  const [serverFieldErrors, setServerFieldErrors] = useState<ClassFormErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const [created, setCreated] = useState<ClassCreated>();

  const clientErrors = validateClassForm(values);
  const errorFor = (field: keyof ClassFormValues) =>
    (touched[field] ? clientErrors[field] : undefined) ?? serverFieldErrors[field];

  const update = (field: keyof ClassFormValues, value: string) => {
    setValues((v) => ({ ...v, [field]: value }));
    setServerFieldErrors((e) => ({ ...e, [field]: undefined }));
  };

  async function onSubmit(e: FormEvent) {
    e.preventDefault();
    setTouched({ name: true, totalSeats: true });
    setServerError(undefined);
    setServerFieldErrors({});
    setCreated(undefined);
    if (Object.keys(clientErrors).length > 0) return;

    setSubmitting(true);
    try {
      const result = await api.createClass(values.name.trim(), Number(values.totalSeats.trim()));
      setCreated(result);
      setValues(EMPTY);
      setTouched({});
    } catch (err) {
      const apiErr = asApiError(err);
      const fe = apiErr.fieldErrors;
      setServerFieldErrors({ name: fe.name, totalSeats: fe.totalSeats });
      // Field-level problems are shown next to the field; everything else as a banner.
      if (!fe.name && !fe.totalSeats) setServerError(apiErr);
    } finally {
      setSubmitting(false);
    }
  }

  const nameError = errorFor('name');
  const seatsError = errorFor('totalSeats');

  return (
    <section className="narrow">
      <h1>নতুন ক্লাস তৈরি</h1>

      {created && (
        <div className="banner banner-success" role="status">
          “{created.name}” ক্লাসটি তৈরি হয়েছে ({created.totalSeats}টি আসন)।{' '}
          <Link to="/">ক্লাস তালিকায় দেখুন</Link>
        </div>
      )}
      {serverError && (
        <div className="banner banner-error" role="alert">
          {serverError.message}
        </div>
      )}

      <form className="form card" onSubmit={(e) => void onSubmit(e)} noValidate>
        <div className="field">
          <label htmlFor="class-name">ক্লাসের নাম</label>
          <input
            id="class-name"
            className={`input${nameError ? ' input-invalid' : ''}`}
            value={values.name}
            maxLength={MAX_NAME_LENGTH + 50}
            placeholder="যেমন: ষষ্ঠ শ্রেণি — ক শাখা"
            aria-invalid={!!nameError}
            aria-describedby="class-name-error"
            onChange={(e) => update('name', e.target.value)}
            onBlur={() => setTouched((t) => ({ ...t, name: true }))}
          />
          {nameError && (
            <small id="class-name-error" className="hint hint-error">
              {nameError}
            </small>
          )}
        </div>

        <div className="field">
          <label htmlFor="class-seats">মোট আসন</label>
          <input
            id="class-seats"
            className={`input${seatsError ? ' input-invalid' : ''}`}
            type="number"
            inputMode="numeric"
            min={1}
            step={1}
            value={values.totalSeats}
            placeholder="যেমন: 40"
            aria-invalid={!!seatsError}
            aria-describedby="class-seats-error"
            onChange={(e) => update('totalSeats', e.target.value)}
            onBlur={() => setTouched((t) => ({ ...t, totalSeats: true }))}
          />
          {seatsError && (
            <small id="class-seats-error" className="hint hint-error">
              {seatsError}
            </small>
          )}
        </div>

        <button type="submit" className="btn btn-primary" disabled={submitting}>
          {submitting ? 'তৈরি হচ্ছে…' : 'ক্লাস তৈরি করুন'}
        </button>
      </form>
    </section>
  );
}
