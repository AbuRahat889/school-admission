import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api, asApiError, type ApiError } from '../api/client';
import type { ClassSummary } from '../api/types';
import { isDuplicateHoldError } from '../lib/errors';
import { useStudentId } from '../lib/studentIdContext';
import { usePageTitle } from '../lib/usePageTitle';
import { usePolling } from '../lib/usePolling';

const POLL_MS = 3000;

export function ClassListPage() {
  usePageTitle('ক্লাস তালিকা');
  const { isValid } = useStudentId();
  const navigate = useNavigate();
  const classes = usePolling(api.listClasses, POLL_MS);

  const [pendingId, setPendingId] = useState<string>();
  const [applyError, setApplyError] = useState<{ classId: string; error: ApiError }>();

  async function apply(classId: string) {
    setPendingId(classId);
    setApplyError(undefined);
    try {
      const hold = await api.createHold(classId);
      navigate(`/holds/${hold.holdId}`);
    } catch (e) {
      setApplyError({ classId, error: asApiError(e) });
      classes.refresh();
    } finally {
      setPendingId(undefined);
    }
  }

  return (
    <section>
      <div className="page-head">
        <h1>ক্লাস তালিকা</h1>
        <Link to="/classes/new" className="btn">
          + নতুন ক্লাস
        </Link>
      </div>

      {!isValid && (
        <div className="banner banner-warning">আবেদন করতে উপরে শিক্ষার্থীর ১৭ অঙ্কের জন্ম নিবন্ধন নম্বর দিন।</div>
      )}

      {classes.loading && !classes.data && <p className="muted">লোড হচ্ছে…</p>}

      {classes.error && !classes.data && (
        <div className="banner banner-error" role="alert">
          {classes.error.message}
        </div>
      )}

      {classes.data && classes.data.length === 0 && (
        <div className="empty">
          <p>এখনো কোনো ক্লাস তৈরি হয়নি।</p>
          <Link to="/classes/new" className="btn btn-primary">
            ক্লাস তৈরি করুন
          </Link>
        </div>
      )}

      {classes.data && classes.data.length > 0 && (
        <ul className="card-grid">
          {classes.data.map((c) => (
            <ClassCard
              key={c.id}
              item={c}
              canApply={isValid}
              pending={pendingId === c.id}
              error={applyError?.classId === c.id ? applyError.error : undefined}
              onApply={() => void apply(c.id)}
            />
          ))}
        </ul>
      )}
    </section>
  );
}

interface ClassCardProps {
  item: ClassSummary;
  canApply: boolean;
  pending: boolean;
  error: ApiError | undefined;
  onApply: () => void;
}

function ClassCard({ item, canApply, pending, error, onApply }: ClassCardProps) {
  const full = item.availableSeats <= 0;
  return (
    <li className="card">
      <h2 className="card-title">{item.name}</h2>
      <dl className="stats">
        <Stat label="মোট আসন" value={item.totalSeats} />
        <Stat label="হোল্ডে" value={item.heldSeats} />
        <Stat label="নিশ্চিত" value={item.confirmedSeats} />
        <Stat label="খালি" value={item.availableSeats} highlight={full ? 'none' : 'some'} />
      </dl>

      <button
        type="button"
        className="btn btn-primary btn-block"
        disabled={full || !canApply || pending}
        onClick={onApply}
      >
        {pending ? 'অপেক্ষা করুন…' : 'আবেদন করুন'}
      </button>

      {full && !error && <p className="card-note">এই ক্লাসে কোনো আসন খালি নেই</p>}
      {error && (
        <p className="card-note card-note-error" role="alert">
          {error.message}
          {isDuplicateHoldError(error) && (
            <>
              {' '}
              <Link to="/me">আমার আবেদন দেখুন</Link>
            </>
          )}
        </p>
      )}
    </li>
  );
}

function Stat({ label, value, highlight }: { label: string; value: number; highlight?: 'none' | 'some' }) {
  return (
    <div className={`stat${highlight ? ` stat-${highlight}` : ''}`}>
      <dt>{label}</dt>
      <dd>{value}</dd>
    </div>
  );
}
