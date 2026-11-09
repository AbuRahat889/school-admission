import { Link } from 'react-router-dom';
import { api } from '../api/client';
import type { MyHold } from '../api/types';
import { LoadError } from '../components/LoadError';
import { StatusBadge } from '../components/StatusBadge';
import { effectiveStatus, formatCountdown, useNow } from '../lib/holdStatus';
import { useStudentId } from '../lib/studentIdContext';
import { usePageTitle } from '../lib/usePageTitle';
import { usePolling } from '../lib/usePolling';

const POLL_MS = 3000;

export function MyHoldsPage() {
  usePageTitle('আমার আবেদন');
  const { studentId, isValid } = useStudentId();
  const now = useNow(1000);
  // Keyed by student id: switching ids starts a fresh list; no id -> no polling.
  const holds = usePolling(api.myHolds, POLL_MS, studentId, isValid);

  return (
    <section>
      <h1>আমার আবেদন</h1>

      {!isValid && (
        <div className="banner banner-warning">
          আপনার আবেদনগুলো দেখতে উপরে শিক্ষার্থীর ১৭ অঙ্কের জন্ম নিবন্ধন নম্বর দিন।
        </div>
      )}

      {isValid && holds.loading && !holds.data && <p className="muted">লোড হচ্ছে…</p>}

      {isValid && holds.error && !holds.data && <LoadError error={holds.error} onRetry={holds.refresh} />}

      {isValid && holds.data && holds.data.length === 0 && (
        <div className="empty">
          <p>আপনি এখনো কোনো ক্লাসে আবেদন করেননি।</p>
          <Link to="/" className="btn btn-primary">
            ক্লাস তালিকা দেখুন
          </Link>
        </div>
      )}

      {isValid && holds.data && holds.data.length > 0 && (
        <ul className="hold-list">
          {holds.data.map((h) => (
            <HoldRow key={h.holdId} hold={h} now={now} />
          ))}
        </ul>
      )}
    </section>
  );
}

function HoldRow({ hold, now }: { hold: MyHold; now: number }) {
  const status = effectiveStatus(hold.status, hold.expiresAt, now);
  return (
    <li className="hold-row card">
      <div className="hold-row-main">
        <div className="hold-class">{hold.className}</div>
        {status === 'HELD' && (
          <div className="small muted">
            মেয়াদ শেষ হতে বাকি:{' '}
            <strong className="tabular">{formatCountdown(Date.parse(hold.expiresAt) - now)}</strong>
          </div>
        )}
      </div>
      <StatusBadge status={status} />
      <Link to={`/holds/${hold.holdId}`} className="btn btn-small">
        {status === 'HELD' ? 'নিশ্চিত করুন' : 'বিস্তারিত দেখুন'}
      </Link>
    </li>
  );
}
