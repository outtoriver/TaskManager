import { useState, type FormEvent } from 'react';
import { useAuth } from '../auth/AuthContext';
import { changePassword } from '../api/authApi';

export function ProfilePage() {
  const { user } = useAuth();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  if (!user) return null;
  const isLocal = user.authenticationType.toLowerCase() === 'local';

  async function submit(e: FormEvent) {
    e.preventDefault(); setMessage(''); setError('');
    if (!isLocal) { setError('Для Windows-учётной записи пароль меняется средствами Windows / Active Directory.'); return; }
    if (newPassword.length < 8) { setError('Новый пароль должен содержать минимум 8 символов.'); return; }
    if (newPassword !== confirmPassword) { setError('Новые пароли не совпадают.'); return; }
    try { setSaving(true); await changePassword(currentPassword, newPassword); setCurrentPassword(''); setNewPassword(''); setConfirmPassword(''); setMessage('Пароль успешно изменён.'); }
    catch (e) { setError(e instanceof Error ? e.message : 'Не удалось изменить пароль.'); }
    finally { setSaving(false); }
  }

  return <div className="profile-page">
    <header className="profile-page-header">
      <button className="tasks-back" type="button" onClick={() => window.history.length > 1 ? window.history.back() : (window.location.hash = '#dashboard')}>← Назад</button>
      <div><span className="eyebrow">TASKMANAGER / PROFILE</span><h1>Профиль</h1><p>Учётная запись и безопасность.</p></div>
    </header>
    <div className="profile-layout">
      <section className="profile-card-large"><div className="profile-avatar-large">{user.displayName.charAt(0).toUpperCase()}</div><h2>{user.displayName}</h2><p>{user.login}</p><div className="profile-meta"><span>Тип входа</span><strong>{user.authenticationType}</strong><span>Роль</span><strong>{user.roles.join(' · ') || 'Без роли'}</strong><span>Отдел</span><strong>{user.departmentName || 'Не назначен'}</strong><span>Должность</span><strong>{user.positionName || 'Не назначена'}</strong></div></section>
      <section className="profile-security-card"><div><span className="eyebrow">SECURITY</span><h2>Смена пароля</h2><p>{isLocal ? 'Введите текущий пароль и новый пароль. Минимальная длина — 8 символов.' : 'Windows-учётные записи используют пароль Active Directory.'}</p></div>{message&&<div className="notice success">{message}</div>}{error&&<div className="notice error">{error}</div>}{isLocal&&<form onSubmit={submit} className="profile-password-form"><label>Текущий пароль<input type="password" value={currentPassword} onChange={e=>setCurrentPassword(e.target.value)} autoComplete="current-password"/></label><label>Новый пароль<input type="password" value={newPassword} onChange={e=>setNewPassword(e.target.value)} autoComplete="new-password"/></label><label>Повторите новый пароль<input type="password" value={confirmPassword} onChange={e=>setConfirmPassword(e.target.value)} autoComplete="new-password"/></label><button className="task-primary-button" disabled={saving}>{saving?'Сохранение...':'Изменить пароль'}</button></form>}</section>
    </div>
  </div>;
}
