import { FormEvent, useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { OrganizationUser, UserInvitation, UserRole } from '../../types/api';

const roles: UserRole[] = ['Owner', 'Administrator', 'Accountant', 'Assistant', 'ReadOnly'];

export function UsersPage() {
  const [users, setUsers] = useState<OrganizationUser[]>([]);
  const [invitations, setInvitations] = useState<UserInvitation[]>([]);
  const [error, setError] = useState('');

  async function load() {
    try {
      const [userData, invitationData] = await Promise.all([
        api<OrganizationUser[]>('/api/users'),
        api<UserInvitation[]>('/api/users/invitations')
      ]);
      setUsers(userData);
      setInvitations(invitationData);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar usuarios');
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function invite(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await api('/api/users/invitations', {
        method: 'POST',
        body: JSON.stringify({ email: form.get('email'), role: form.get('role') })
      });
      event.currentTarget.reset();
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao convidar usuario');
    }
  }

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-fiscal-navy">Usuarios</h1>
        <p className="text-sm text-slate-500">Convites, perfis e status dos usuarios da organizacao.</p>
      </div>
      {error && <div className="rounded-md bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}
      <form onSubmit={invite} className="grid gap-3 rounded-md border border-slate-200 bg-white p-4 shadow-sm md:grid-cols-[1fr_220px_auto]">
        <input className="field" name="email" type="email" placeholder="E-mail do convidado" required />
        <select className="field" name="role" defaultValue="Accountant">
          {roles.map((role) => <option key={role} value={role}>{role}</option>)}
        </select>
        <button className="btn-primary">Convidar</button>
      </form>
      <section className="overflow-hidden rounded-md border border-slate-200 bg-white">
        <table className="w-full text-left text-sm">
          <thead className="bg-slate-50 text-slate-500">
            <tr><th className="p-3">Nome</th><th>E-mail</th><th>Perfil</th><th>Status</th></tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id} className="border-t border-slate-100">
                <td className="p-3 font-medium text-fiscal-ink">{user.name}</td>
                <td>{user.email}</td>
                <td>{user.role}</td>
                <td>{user.isActive ? 'Ativo' : 'Inativo'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
      <section className="rounded-md border border-slate-200 bg-white p-4">
        <h2 className="font-semibold text-fiscal-navy">Convites pendentes</h2>
        <div className="mt-3 space-y-2">
          {invitations.length === 0 && <p className="text-sm text-slate-500">Nenhum convite pendente.</p>}
          {invitations.map((invitation) => (
            <div key={invitation.id} className="flex flex-wrap items-center justify-between gap-2 rounded-md bg-slate-50 p-3 text-sm">
              <span>{invitation.email} - {invitation.role}</span>
              <span className="text-slate-500">Expira em {new Date(invitation.expiresAt).toLocaleDateString()}</span>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
