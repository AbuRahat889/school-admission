import { useCallback, useEffect, useRef, useState } from 'react';
import { asApiError, type ApiError } from '../api/client';

export interface PollingState<T> {
  data: T | undefined;
  /** Error of the most recent attempt; cleared by the next success. Old data is kept. */
  error: ApiError | undefined;
  /** True until the first attempt for the current key has finished. */
  loading: boolean;
  /** Fetch now (e.g. after an action) and continue polling from there. */
  refresh: () => void;
}

/**
 * Calls `fetcher` immediately and then `intervalMs` after each completed call
 * (no overlapping requests). Changing `key` resets data and starts over.
 * Pass `enabled = false` to stop polling (e.g. no valid student id).
 */
export function usePolling<T>(
  fetcher: () => Promise<T>,
  intervalMs: number,
  key: string = '',
  enabled: boolean = true,
): PollingState<T> {
  const [data, setData] = useState<T>();
  const [error, setError] = useState<ApiError>();
  const [loading, setLoading] = useState(enabled);
  const [nonce, setNonce] = useState(0);

  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  const lastKey = useRef(key);
  if (lastKey.current !== key) {
    lastKey.current = key;
    // Reset during render so the page never shows data belonging to the previous key.
    setData(undefined);
    setError(undefined);
    setLoading(enabled);
  }

  useEffect(() => {
    if (!enabled) {
      setLoading(false);
      return;
    }
    let cancelled = false;
    let timer: ReturnType<typeof setTimeout> | undefined;

    const run = async () => {
      try {
        const result = await fetcherRef.current();
        if (cancelled) return;
        setData(result);
        setError(undefined);
      } catch (e) {
        if (cancelled) return;
        setError(asApiError(e));
      } finally {
        if (!cancelled) {
          setLoading(false);
          timer = setTimeout(run, intervalMs);
        }
      }
    };

    void run();
    return () => {
      cancelled = true;
      if (timer) clearTimeout(timer);
    };
  }, [key, intervalMs, enabled, nonce]);

  const refresh = useCallback(() => setNonce((n) => n + 1), []);

  return { data, error, loading, refresh };
}
