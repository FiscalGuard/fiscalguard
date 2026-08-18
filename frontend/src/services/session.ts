import type { AuthResponse } from '../types/api';

const key = 'fiscalguard.session';

export function getSession(): AuthResponse | null {
  const raw = localStorage.getItem(key);
  return raw ? JSON.parse(raw) as AuthResponse : null;
}

export function setSession(session: AuthResponse) {
  localStorage.setItem(key, JSON.stringify(session));
}

export function clearSession() {
  localStorage.removeItem(key);
}
