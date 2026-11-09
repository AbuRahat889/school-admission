import type { ApiError } from '../api/client';

/**
 * Shown in place of page content when the first load failed, so a page is never blank.
 * Polling keeps retrying in the background; the button just retries right away.
 */
export function LoadError({ error, onRetry }: { error: ApiError; onRetry: () => void }) {
  return (
    <div className="empty" role="alert">
      <p>
        {error.isNetworkError
          ? 'তথ্য লোড করা যায়নি। সংযোগ ফিরে এলে স্বয়ংক্রিয়ভাবে আবার চেষ্টা করা হবে।'
          : error.message}
      </p>
      <button type="button" className="btn" onClick={onRetry}>
        আবার চেষ্টা করুন
      </button>
    </div>
  );
}
