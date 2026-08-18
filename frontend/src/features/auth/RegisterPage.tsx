import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../../services/api';
import { setSession } from '../../services/session';
import type { AuthResponse } from '../../types/api';
import { AuthForm } from './LoginPage';

export function RegisterPage() {
  const navigate = useNavigate();
  const [error, setError] = useState('');

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      const session = await api<AuthResponse>('/api/auth/register', {
        method: 'POST',
        body: JSON.stringify({
          responsibleName: form.get('responsibleName'),
          organizationName: form.get('organizationName'),
          email: form.get('email'),
          password: form.get('password'),
          phone: form.get('phone'),
          officeDocument: form.get('officeDocument'),
          acceptedTerms: form.get('acceptedTerms') === 'on'
        })
      });
      setSession(session);
      navigate('/app/dashboard');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao cadastrar');
    }
  }

  return <AuthForm title="Cadastro do escritorio" cta="Iniciar trial" error={error} onSubmit={submit} register />;
}
