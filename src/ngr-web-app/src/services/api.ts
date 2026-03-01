import { oktaAuth } from '../lib/okta';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000';

function authHeaders(): Record<string, string> {
  const token = oktaAuth.getAccessToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

/**
 * Handle API responses — on 401, redirect to Okta login preserving the current URL.
 * All other errors throw as before.
 */
async function handleResponse<T>(res: Response): Promise<T> {
  if (res.status === 401) {
    const currentUri = window.location.pathname + window.location.search;
    await oktaAuth.signInWithRedirect({ originalUri: currentUri });
    // Return a never-resolving promise so callers don't proceed
    return new Promise<T>(() => {});
  }
  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || `HTTP ${res.status}`);
  }
  return res.json() as Promise<T>;
}

async function handleVoidResponse(res: Response): Promise<void> {
  if (res.status === 401) {
    const currentUri = window.location.pathname + window.location.search;
    await oktaAuth.signInWithRedirect({ originalUri: currentUri });
    return new Promise<void>(() => {});
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
