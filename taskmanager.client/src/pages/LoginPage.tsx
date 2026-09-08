import { useState, type FormEvent } from 'react';

import { useAuth } from '../auth/AuthContext';

export function LoginPage() {
  const { error, isLoading, login, loginWindows, clearError } = useAuth();
  const [loginValue, setLoginValue] = useState('admin');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    clearError();

    try {
      await login({ login: loginValue.trim(), password });
    } catch {
      // AuthContext exposes the user-facing error state.
    }
  }

  async function handleWindowsLogin() {
    clearError();

    try {
      await loginWindows();
    } catch {
      // AuthContext exposes the user-facing error state.
    }
  }

  return (
    <main className="login-page">
      <section className="login-shell" aria-label="Вход в TaskManager">
        <div className="login-brand">
          <div className="brand-mark">T</div>
          <div>
            <p className="eyebrow">CORPORATE WORKSPACE</p>
            <h1>TaskManager</h1>
          </div>
        </div>

        <div className="login-copy">
          <span className="status-pill">
            <span className="status-dot" />
            Корпоративная система
          </span>
          <h2>Войдите в рабочее пространство</h2>
          <p>
            Управляйте задачами, сотрудниками, встречами и рабочими процессами
            из одного интерфейса.
          </p>
        </div>

        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            <span>Логин</span>
            <input
              autoComplete="username"
              disabled={isLoading}
              onChange={(event) => setLoginValue(event.target.value)}
              placeholder="Введите логин"
              value={loginValue}
            />
          </label>

          <label>
            <span>Пароль</span>
            <div className="password-field">
              <input
                autoComplete="current-password"
                disabled={isLoading}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Введите пароль"
                type={showPassword ? 'text' : 'password'}
                value={password}
              />
              <button
                className="password-toggle"
                disabled={isLoading}
                onClick={() => setShowPassword((value) => !value)}
                type="button"
              >
                {showPassword ? 'Скрыть' : 'Показать'}
              </button>
            </div>
          </label>

          {error && (
            <div className="auth-error" role="alert">
              {error}
            </div>
          )}

          <button className="primary-button" disabled={isLoading} type="submit">
            {isLoading ? 'Выполняем вход…' : 'Войти'}
          </button>
        </form>

        <div className="login-divider">
          <span />
          <em>или</em>
          <span />
        </div>

        <button
          className="windows-button"
          disabled={isLoading}
          onClick={handleWindowsLogin}
          type="button"
        >
          <span className="windows-icon" aria-hidden="true">
            ▦
          </span>
          Войти через Windows
        </button>

        <div className="login-footer">
          <span>TaskManager</span>
          <span>Безопасный корпоративный доступ</span>
        </div>
      </section>
    </main>
  );
}
