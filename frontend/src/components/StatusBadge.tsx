import type { HoldStatus } from '../api/types';
import { statusLabel } from '../lib/holdStatus';

export function StatusBadge({ status }: { status: HoldStatus }) {
  return <span className={`badge badge-${status.toLowerCase()}`}>{statusLabel(status)}</span>;
}
