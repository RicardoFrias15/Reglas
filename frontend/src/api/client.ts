import { msalInstance, apiTokenRequest } from '@/auth/authConfig';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;

async function getAccessToken(): Promise<string> {
  const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0];
  if (!account) throw new Error('No hay sesión activa');

  try {
    const result = await msalInstance.acquireTokenSilent({ ...apiTokenRequest, account });
    return result.accessToken;
  } catch {
    const result = await msalInstance.acquireTokenPopup({ ...apiTokenRequest, account });
    return result.accessToken;
  }
}

export interface ApiOptions extends Omit<RequestInit, 'body'> {
  body?: unknown;
  isFormData?: boolean;
}

export async function api<T = unknown>(path: string, opts: ApiOptions = {}): Promise<T> {
  const token = await getAccessToken();
  const headers = new Headers(opts.headers);
  headers.set('Authorization', `Bearer ${token}`);

  let body: BodyInit | undefined;
  if (opts.body !== undefined) {
    if (opts.isFormData) {
      body = opts.body as FormData;
    } else {
      headers.set('Content-Type', 'application/json');
      body = JSON.stringify(opts.body);
    }
  }

  const res = await fetch(`${API_BASE_URL}${path}`, { ...opts, headers, body });
  if (!res.ok) {
    const text = await res.text().catch(() => '');
    throw new Error(text || `HTTP ${res.status}`);
  }
  if (res.status === 204) return undefined as T;
  const ct = res.headers.get('Content-Type') ?? '';
  if (ct.includes('application/json')) return (await res.json()) as T;
  return (await res.text()) as unknown as T;
}
