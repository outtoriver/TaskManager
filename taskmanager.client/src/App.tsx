import './App.css';
import { AuthProvider } from './auth/AuthContext';
import { ProtectedRoute } from './auth/ProtectedRoute';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';

function App() {
  return (
    <AuthProvider>
      <ProtectedRoute fallback={<LoginPage />}>
        <DashboardPage />
      </ProtectedRoute>
    </AuthProvider>
  );
}

export default App;
