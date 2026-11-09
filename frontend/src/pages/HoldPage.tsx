import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { api, asApiError, type ApiError } from '../api/client';
import type { HoldStatus } from '../api/types';
import { StatusBadge } from '../components/StatusBadge';
import { effectiveStatus, formatCountdown, useNow } from '../lib/holdStatus';
import { useStudentId } from '../lib/studentIdContext';
import { usePageTitle } from '../lib/usePageTitle';
import { usePolling } from '../lib/usePolling';

const POLL_MS = 3000;

export function HoldPage() {
  usePageTitle('আবেদনের অবস্থা');
  const { holdId = '' } = useParams();
  const { isValid: hasStudentId } = useStudentId();
  const now = useNow(1000);

  const hold = usePolling(() => api.getHold(holdId), POLL_MS, holdId);
  const className = useClassName(hold.data?.classId);

  // Status returned by a successful action, shown until the next poll result arrives.
  const [override, setOverride] = useState<HoldStatus>();
  useEffect(() => setOverride(undefined), [hold.data]);

  const [busy, setBusy] = useState<'confirm' | 'cancel'>();
  const [actionError, setActionError] = useState<ApiError>();

  if (!hold.data) {
    return (
      <section className="narrow">
        <h1>আবেদনের অবস্থা</h1>
        {hold.loading && <p className="muted">লোড হচ্ছে…</p>}
        {hold.error && (
          <div className="banner banner-error" role="alert">
            {hold.error.status === 404 ? 'আবেদনটি খুঁজে পাওয়া যায়নি' : hold.error.message}
          </div>
        )}
        <Link to="/">ক্লাস তালিকায় ফিরে যান</Link>
      </section>
    );
  }

  const status = effectiveStatus(override ?? hold.data.status, hold.data.expiresAt, now);
  const remainingMs = Date.parse(hold.data.expiresAt) - now;
  const isHeld = status === 'HELD';

  async function run(kind: 'confirm' | 'cancel') {
    if (kind === 'cancel' && !window.confirm('আপনি কি এই আবেদনটি বাতিল করতে চান?')) return;
    setBusy(kind);
    setActionError(undefined);
    try {
      if (kind === 'confirm') {
        const res = await api.confirmHold(holdId);
        setOverride(res.status);
      } else {
        await api.cancelHold(holdId);
        setOverride('CANCELLED');
      }
    } catch (e) {
      setActionError(asApiError(e));
    } finally {
      setBusy(undefined);
      hold.refresh();
    }
  }

  return (
    <section className="narrow">
      <h1>আবেদনের অবস্থা</h1>

      <div className="card hold-card">
        <div className="hold-head">
          <div>
            <div className="muted small">ক্লাস</div>
            <div className="hold-class">{className ?? '—'}</div>
          </div>
          <StatusBadge status={status} />
        </div>

        {isHeld && (
          <p className={`countdown${remainingMs <= 60_000 ? ' countdown-urgent' : ''}`} aria-live="polite">
            মেয়াদ শেষ হতে বাকি: <strong>{formatCountdown(remainingMs)}</strong>
          </p>
        )}

        <StatusMessage status={status} />

        {actionError && (
          <div className="banner banner-error" role="alert">
            {actionError.message}
          </div>
        )}

        {isHeld && !hasStudentId && (
          <p className="hint hint-error">নিশ্চিত বা বাতিল করতে উপরে শিক্ষার্থীর জন্ম নিবন্ধন নম্বর দিন।</p>
        )}

        <div className="actions">
          <button
            type="button"
            className="btn btn-primary"
            disabled={!isHeld || !hasStudentId || busy !== undefined}
            onClick={() => void run('confirm')}
          >
            {busy === 'confirm' ? 'অপেক্ষা করুন…' : 'ভর্তি ফি দিন'}
          </button>
          <button
            type="button"
            className="btn btn-danger"
            disabled={!isHeld || !hasStudentId || busy !== undefined}
            onClick={() => void run('cancel')}
          >
            {busy === 'cancel' ? 'অপেক্ষা করুন…' : 'বাতিল'}
          </button>
        </div>
      </div>

      {hold.error && (
        <p className="hint hint-error">সর্বশেষ তথ্য হালনাগাদ করা যায়নি: {hold.error.message}</p>
      )}

      <p>
        <Link to="/me">আমার সব আবেদন দেখুন</Link>
      </p>
    </section>
  );
}

function StatusMessage({ status }: { status: HoldStatus }) {
  switch (status) {
    case 'CONFIRMED':
      return (
        <div className="banner banner-success" role="status">
          অভিনন্দন! আপনার ভর্তি নিশ্চিত হয়েছে।
        </div>
      );
    case 'EXPIRED':
      return (
        <div className="banner banner-warning" role="status">
          মেয়াদ শেষ হয়ে গেছে, আবার আবেদন করুন। <Link to="/">ক্লাস তালিকায় যান</Link>
        </div>
      );
    case 'CANCELLED':
      return (
        <div className="banner banner-warning" role="status">
          এই আবেদনটি বাতিল করা হয়েছে। <Link to="/">নতুন করে আবেদন করুন</Link>
        </div>
      );
    default:
      return <p className="muted">মেয়াদ শেষ হওয়ার আগে ভর্তি ফি দিয়ে আসনটি নিশ্চিত করুন।</p>;
  }
}

/** GET /holds has no class name, so look it up once per class id. Failures just leave it blank. */
function useClassName(classId: string | undefined): string | undefined {
  const [name, setName] = useState<{ id: string; name: string }>();
  useEffect(() => {
    if (!classId || name?.id === classId) return;
    let cancelled = false;
    api
      .getClass(classId)
      .then((c) => !cancelled && setName({ id: classId, name: c.name }))
      .catch(() => undefined);
    return () => {
      cancelled = true;
    };
  }, [classId, name?.id]);
  return name && name.id === classId ? name.name : undefined;
}
