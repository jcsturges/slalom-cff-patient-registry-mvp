import { oktaAuth } from '../lib/okta';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

const SESSION_STORAGE_KEY = 'ngr-impersonation-session';

function authHeaders(): Record<string, string> {
  const token = oktaAuth.getAccessToken();
  const headers: Record<string, string> = token ? { Authorization: `Bearer ${token}` } : {};

  // Attach impersonation session ID when active
  try {
    const raw = sessionStorage.getItem(SESSION_STORAGE_KEY);
    if (raw) {
      const session = JSON.parse(raw) as { sessionId: string; expiresAt: string };
      if (new Date(session.expiresAt) > new Date()) {
        headers['X-Impersonation-Session-Id'] = session.sessionId;
      }
    }
  } catch {
    // Ignore parse errors
  }

  return headers;
}

/**
 * Handle API responses — on 401, redirect to Okta login preserving the current URL.
 * All other errors throw as before.
 */
async function handleResponse<T>(res: Response): Promise<T> {
  if (res.status === 401) {
    throw new Error('Unauthorized');
  }
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }
  return res.json() as Promise<T>;
}

async function handleVoidResponse(res: Response): Promise<void> {
  if (res.status === 401) {
    throw new Error('Unauthorized');
  }
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }
}

export async function apiGet<T>(
  path: string,
  params?: Record<string, string | number | boolean | undefined>,
): Promise<T> {
  const url = new URL(`${BASE_URL}${path}`);
  if (params) {
    for (const [k, v] of Object.entries(params)) {
      if (v !== undefined && v !== '') url.searchParams.set(k, String(v));
    }
  }
  const res = await fetch(url.toString(), {
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
  });
  return handleResponse<T>(res);
}

export async function apiPost<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  });
  return handleResponse<T>(res);
}

export async function apiPut<T>(path: string, body: unknown): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(body),
  });
  return handleResponse<T>(res);
}

export async function apiDelete(path: string): Promise<void> {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: 'DELETE',
    headers: { ...authHeaders() },
  });
  return handleVoidResponse(res);
}
