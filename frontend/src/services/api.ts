import { getSession } from './session';

const baseUrl = import.meta.env.VITE_API_URL ?? 'https://localhost:7296';

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const session = getSession();
  const isFormData = options.body instanceof FormData;
  const response = await fetch(`${baseUrl}${path}`, {
    ...options,
    headers: {
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
      ...(session?.accessToken ? { Authorization: `Bearer ${session.accessToken}` } : {}),
      ...options.headers
    }
  });

  if (!response.ok) {
    const problem = await response.json().catch(() => ({ title: 'Erro na requisicao' }));
    throw new Error(problem.title ?? 'Erro na requisicao');
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}
