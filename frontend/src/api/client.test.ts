import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { API_URL, ApiError, api } from './client';

const store = new Map<string, string>();

beforeEach(() => {
  store.clear();
  vi.stubGlobal('localStorage', {
    getItem: (k: string) => store.get(k) ?? null,
    setItem: (k: string, v: string) => void store.set(k, v),
    removeItem: (k: string) => void store.delete(k),
  });
});

afterEach(() => vi.unstubAllGlobals());

const jsonResponse = (status: number, body: unknown) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });

describe('api client', () => {
  it('defaults to http://localhost:8080', () => {
    expect(API_URL).toBe('http://localhost:8080');
  });

  it('sends X-Student-Id from localStorage on every request', async () => {
    store.set('studentId', '12345678901234567');
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse(200, []));
    vi.stubGlobal('fetch', fetchMock);

    await api.listClasses();

    const [url, init] = fetchMock.mock.calls[0]!;
    expect(url).toBe('http://localhost:8080/classes');
    expect((init as RequestInit).headers).toMatchObject({ 'X-Student-Id': '12345678901234567' });
  });

  it('turns an ErrorResponse into an ApiError with Bangla message and the server message kept', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue(
        jsonResponse(409, {
          statusCode: 409,
          message: 'This class is full. No seats are available right now.',
          errors: {},
          timestamp: '2026-01-01T00:00:00Z',
        }),
      ),
    );

    const err = await api.createHold('c1').catch((e: unknown) => e);
    expect(err).toBeInstanceOf(ApiError);
    expect((err as ApiError).status).toBe(409);
    expect((err as ApiError).message).toBe('এই ক্লাসে কোনো আসন খালি নেই');
    expect((err as ApiError).serverMessage).toBe('This class is full. No seats are available right now.');
  });

  it('reports network failures as status 0 with the unreachable message', async () => {
    vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('Failed to fetch')));
    const err = (await api.myHolds().catch((e: unknown) => e)) as ApiError;
    expect(err.isNetworkError).toBe(true);
    expect(err.message).toBe('সার্ভারের সাথে সংযোগ করা যাচ্ছে না');
  });

  it('handles 204 and non-JSON error bodies', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 204 })));
    await expect(api.cancelHold('h1')).resolves.toBeUndefined();

    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('<html>Bad Gateway</html>', { status: 502 })));
    const err = (await api.listClasses().catch((e: unknown) => e)) as ApiError;
    expect(err.status).toBe(502);
    expect(err.message).toMatch(/সার্ভারে সমস্যা/);
  });
});
