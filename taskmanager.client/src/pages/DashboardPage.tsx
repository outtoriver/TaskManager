import { useAuth } from '../auth/AuthContext';

const quickActions = [
  {
    title: 'Мои задачи',
    description: 'Задачи, назначенные вам',
    permission: 'Tasks.ViewOwn',
    icon: '✓',
  },
  {
    title: 'Пользователи',
    description: 'Сотрудники и структура',
    permission: 'Users.View',
    icon: '◌',
  },
  {
    title: 'Календарь',
    description: 'События и встречи',
    permission: 'Calendar.View',
    icon: '◫',
  },
  {
    title: 'Настройки',
    description: 'Системное управление',
    permission: 'Settings.Manage',
    icon: '⚙',
  },
];

export function DashboardPage() {
  const { user, logout, hasPermission } = useAuth();

  if (!user) {
    return null;
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="brand-mark small">T</div>
          <div>
            <strong>TaskManager</strong>
            <span>Corporate workspace</span>
          </div>
        </div>

        <nav className="sidebar-nav" aria-label="Основная навигация">
          <a className="nav-item active" href="#dashboard">
            <span>⌂</span>
            Обзор
          </a>
          {hasPermission('Tasks.ViewOwn') && (
            <a className="nav-item" href="#tasks">
              <span>✓</span>
              Задачи
            </a>
          )}
          {hasPermission('Calendar.View') && (
            <a className="nav-item" href="#calendar">
              <span>◫</span>
              Календарь
            </a>
          )}
          {hasPermission('Users.View') && (
            <a className="nav-item" href="#users">
              <span>◌</span>
              Пользователи
            </a>
          )}
          {hasPermission('Meetings.View') && (
            <a className="nav-item" href="#meetings">
              <span>◎</span>
              Встречи
            </a>
          )}
          {hasPermission('Settings.Manage') && (
            <a className="nav-item" href="#settings">
              <span>⚙</span>
              Настройки
            </a>
          )}
        </nav>

        <div className="sidebar-footer">
          <div className="user-mini">
            <div className="avatar">{user.displayName.charAt(0).toUpperCase()}</div>
            <div className="user-mini-copy">
              <strong>{user.displayName}</strong>
              <span>{user.roles.join(', ') || 'Без роли'}</span>
            </div>
          </div>
          <button className="logout-button" onClick={() => void logout()} type="button">
            Выйти
          </button>
        </div>
      </aside>

      <main className="dashboard-content">
        <header className="dashboard-header">
          <div>
            <p className="eyebrow">TASKMANAGER / OVERVIEW</p>
            <h1>Добро пожаловать, {user.displayName}</h1>
            <p className="header-subtitle">
              Система определила ваш профиль и применённые права доступа.
            </p>
          </div>
          <div className="profile-card">
            <span className="profile-card-label">Текущий пользователь</span>
            <strong>{user.login}</strong>
            <small>{user.authenticationType}</small>
          </div>
        </header>

        <section className="permission-banner">
          <div>
            <span className="permission-label">Доступ</span>
            <strong>{user.roles.length ? user.roles.join(' · ') : 'Без назначенной роли'}</strong>
          </div>
          <div className="permission-count">
            <strong>{user.permissions.length}</strong>
            <span>permissions</span>
          </div>
        </section>

        <section className="dashboard-grid">
          {quickActions
            .filter((action) => hasPermission(action.permission))
            .map((action) => (
              <a className="dashboard-card dashboard-card-link" key={action.permission} href={action.permission === 'Users.View' ? '#admin' : '#'}>
                <div className="dashboard-card-icon">{action.icon}</div>
                <div>
                  <h2>{action.title}</h2>
                  <p>{action.description}</p>
                </div>
                <span className="card-permission">{action.permission}</span>
              </a>
            ))}
        </section>

        <section className="identity-grid">
          <article className="info-card">
            <span className="card-kicker">Профиль</span>
            <h2>Данные пользователя</h2>
            <dl>
              <div><dt>Логин</dt><dd>{user.login}</dd></div>
              <div><dt>Подразделение</dt><dd>{user.departmentName ?? 'Не назначено'}</dd></div>
              <div><dt>Должность</dt><dd>{user.positionName ?? 'Не назначена'}</dd></div>
              <div><dt>Руководитель</dt><dd>{user.managerName ?? 'Не назначен'}</dd></div>
            </dl>
          </article>

          <article className="info-card">
            <span className="card-kicker">RBAC</span>
            <h2>Права доступа</h2>
            <div className="permission-list">
              {user.permissions.map((permission) => (
                <span key={permission}>{permission}</span>
              ))}
            </div>
          </article>
        </section>
      </main>
    </div>
  );
}
