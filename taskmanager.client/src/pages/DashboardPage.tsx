import { useAuth } from '../auth/AuthContext';

export function DashboardPage() {
  const { user, logout, hasPermission } = useAuth();
  if (!user) return null;

  const cards = [
    hasPermission('Tasks.ViewOwn') ? { href: '#tasks', icon: '✓', title: 'Задачи', text: 'Мои задачи, задачи подразделения и Kanban.', permission: 'Tasks.ViewOwn' } : null,
    hasPermission('Users.View') ? { href: '#admin', icon: '◌', title: 'Пользователи и структура', text: 'Сотрудники, отделы, должности и руководители.', permission: 'Users.View' } : null,
    hasPermission('Calendar.View') ? { href: '#calendar', icon: '◫', title: 'Календарь', text: 'События, сроки задач и встречи.', permission: 'Calendar.View' } : null,
    hasPermission('Meetings.View') ? { href: '#meetings', icon: '◎', title: 'Встречи', text: 'Совещания и рабочие встречи.', permission: 'Meetings.View' } : null,
  ].filter(Boolean) as Array<{href:string;icon:string;title:string;text:string;permission:string}>;

  return (
    <div className="dashboard-page-full">
      <header className="dashboard-header">
        <div>
          <p className="eyebrow">TASKMANAGER / OVERVIEW</p>
          <h1>Добро пожаловать, {user.displayName}</h1>
          <p className="header-subtitle">Рабочее пространство компании.</p>
        </div>
        <div className="dashboard-header-actions">
          <a className="profile-card dashboard-profile-link" href="#profile">
            <span className="profile-card-label">ПРОФИЛЬ</span>
            <strong>{user.login}</strong>
            <small>{user.roles.join(' · ') || 'Без роли'}</small>
          </a>
          <button className="logout-button" type="button" onClick={() => void logout()}>Выйти</button>
        </div>
      </header>

      <section className="permission-banner">
        <div><span className="permission-label">ДОСТУП</span><strong>Роль: {user.roles.join(' · ') || 'Без роли'}</strong></div>
        <div className="permission-count"><strong>{user.permissions.length}</strong><span>permissions</span></div>
      </section>

      <section className="dashboard-grid">
        {cards.map(card => (
          <a className="dashboard-card" href={card.href} key={card.href}>
            <div className="dashboard-card-icon">{card.icon}</div>
            <div><h2>{card.title}</h2><p>{card.text}</p></div>
            <span className="card-permission">{card.permission}</span>
          </a>
        ))}
      </section>
    </div>
  );
}
