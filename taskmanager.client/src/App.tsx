import './App.css';
import { useEffect, useState } from 'react';
import { AuthProvider } from './auth/AuthContext';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { AdministrationPage } from './pages/AdministrationPage';
import { TasksPage } from './pages/TasksPage';
import { ProfilePage } from './pages/ProfilePage';

function RouterView() {
  const [hash, setHash] = useState(window.location.hash || '#dashboard');
  useEffect(() => {
    const handler = () => setHash(window.location.hash || '#dashboard');
    window.addEventListener('hashchange', handler);
    return () => window.removeEventListener('hashchange', handler);
  }, []);

  if (hash === '#tasks') return <TasksPage />;
  if (hash === '#profile') return <ProfilePage />;
  if (hash === '#admin' || hash === '#users') return <AdministrationPage />;
  return <DashboardPage />;
}

export default function App() {
  return <AuthProvider><ProtectedRoute fallback={<LoginPage />}><RouterView /></ProtectedRoute></AuthProvider>;
}
