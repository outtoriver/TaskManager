import './App.css';
import { useEffect, useState } from 'react';
import { AuthProvider } from './auth/AuthContext';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { AdministrationPage } from './pages/AdministrationPage';
import { TasksPage } from './pages/TasksPage';

function RouterView() {
  const [hash, setHash] = useState(window.location.hash || '#dashboard');

  useEffect(() => {
    const handler = () => setHash(window.location.hash || '#dashboard');
    window.addEventListener('hashchange', handler);
    return () => window.removeEventListener('hashchange', handler);
  }, []);

  if (hash === '#tasks') return <TasksPage />;
  if (hash === '#admin' || hash === '#users') return <AdministrationPage />;
  return <DashboardPage />;
}

function App() {
  return (
    <AuthProvider>
      <ProtectedRoute fallback={<LoginPage />}>
        <RouterView />
      </ProtectedRoute>
    </AuthProvider>
  );
}

export default App;
