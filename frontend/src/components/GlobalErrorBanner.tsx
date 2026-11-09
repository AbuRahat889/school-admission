import { useEffect, useState } from 'react';
import { subscribeApiEvents } from '../api/client';

type BannerState = { kind: 'network' | 'server'; message: string } | null;

/**
 * App-wide banner for problems that are not about one specific action:
 *  - server unreachable (network failure), and
 *  - server-side failures (5xx).
 * It clears itself as soon as any request succeeds again. Request-specific 4xx errors
 * (class full, not your hold, validation, ...) are shown next to the action instead.
 */
export function GlobalErrorBanner() {
  const [state, setState] = useState<BannerState>(null);

  useEffect(
    () =>
      subscribeApiEvents((e) => {
        if (e.type === 'ok') setState(null);
        else if (e.error.isNetworkError) setState({ kind: 'network', message: e.error.message });
        else if (e.error.status >= 500) setState({ kind: 'server', message: e.error.message });
      }),
    [],
  );

  if (!state) return null;
  return (
    <div className="global-banner" role="alert">
      <div className="container global-banner-inner">
        <span>
          {state.message}
          {state.kind === 'network' && '। স্বয়ংক্রিয়ভাবে আবার চেষ্টা করা হচ্ছে…'}
        </span>
        <button type="button" className="btn btn-small" onClick={() => setState(null)}>
          বন্ধ করুন
        </button>
      </div>
    </div>
  );
}
