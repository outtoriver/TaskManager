import { useEffect, useState, type ReactNode } from 'react';
import { useAuth } from '../auth/AuthContext';
import './AppPageShell.css';

interface AppPageShellProps {
  title: string;
  eyebrow: string;
  subtitle?: string;
  backLabel?: string;
  children: ReactNode;
}

export function AppPageShell({
  title,
  eyebrow,
  subtitle,
  backLabel = 'Назад',
  children,
}: AppPageShellProps) {
  const { user, logout, hasPermission } = useAuth();
  const [hash, setHash] = useState(window.location.hash || '#dashboard');

  useEffect(() => {
    const handler = () => setHash(window.location.hash || '#dashboard');
    window.addEventListener('hashchange', handler);
    return () => window.removeEventListener('hashchange', handler);
  }, []);

  if (!user) return null;

  return (
    <div className="workspace-shell">
      <aside className="workspace-sidebar">
        <a className="workspace-brand" href="#dashboard" aria-label="TaskManager — обзор">
          <span className="workspace-brand-mark">T</span>
          <span>
            <strong>TaskManager</strong>
            <small>Corporate workspace</small>
          </span>
        </a>

        <nav className="workspace-nav" aria-label="Основная навигация">
          <a href="#dashboard" className={`workspace-nav-item ${hash === '#dashboard' ? 'active' : ''}`}
            >
            <span>⌂</span> Обзор
          </a>
          {hasPermission('Tasks.ViewOwn') && (
            <a href="#tasks" className={`workspace-nav-item ${hash === '#tasks' ? 'active' : ''}`}>
              <span>✓</span> Задачи
            </a>
          )}
          {hasPermission('Calendar.View') && (
            <a href="#calendar" className="workspace-nav-item">
              <span>◫</span> Календарь
            </a>
          )}
          {hasPermission('Users.View') && (
            <a href="#users" className={`workspace-nav-item ${hash === '#users' || hash === '#admin' ? 'active' : ''}`}>
              <span>◌</span> Пользователи
            </a>
          )}
          {hasPermission('Meetings.View') && (
            <a href="#meetings" className={`workspace-nav-item ${hash === '#meetings' ? 'active' : ''}`}>
              <span>◎</span> Встречи
            </a>
          )}
          {hasPermission('Settings.Manage') && (
            <a href="#settings" className={`workspace-nav-item ${hash === '#settings' ? 'active' : ''}`}>
              <span>⚙</span> Настройки
            </a>
          )}
        </nav>

        <div className="workspace-sidebar-footer">
          <div className="workspace-user">
            <div className="workspace-avatar">{user.displayName.charAt(0).toUpperCase()}</div>
            <div>
              <strong>{user.displayName}</strong>
              <small>{user.roles.join(', ') || 'Без роли'}</small>
            </div>
          </div>
          <button type="button" className="workspace-logout" onClick={() => void logout()}>
            Выйти
          </button>
        </div>
      </aside>

      <main className="workspace-main">
        <header className="workspace-header">
          <div className="workspace-header-copy">
            <button type="button" className="workspace-back" onClick={() => { window.location.hash = '#dashboard'; }}>
              <span aria-hidden="true">←</span>
              {backLabel}
            </button>
            <div>
              <p className="eyebrow">{eyebrow}</p>
              <h1>{title}</h1>
              {subtitle && <p>{subtitle}</p>}
            </div>
          </div>
          <div className="workspace-profile-chip">
            <span>{user.login}</span>
            <small>{user.roles.join(' · ') || 'Без роли'}</small>
          </div>
        </header>
        <section className="workspace-content">{children}</section>
      </main>
    </div>
  );
}
