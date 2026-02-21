import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

const AppLayout = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  return (
    <div className="app-shell">
      <header className="app-header">
        <Link to="/" className="brand">
          Auth Service
        </Link>
        <nav className="app-nav">
          <NavLink to="/" end>
            Overview
          </NavLink>
          <NavLink to="/apps">Apps</NavLink>
          {user?.roles?.includes('Admin') && (
            <NavLink to="/admin/users">
              Admin
            </NavLink>
          )}
        </nav>
        <div className="user-actions">
          <div className="user-chip">
            <span className="avatar">{user?.email?.[0]?.toUpperCase()}</span>
            <div>
              <p>{user?.displayName ?? user?.email}</p>
              <small>{user?.roles?.join(', ') || 'User'}</small>
            </div>
          </div>
          <button type="button" className="ghost" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </header>
      <main className="app-content">
        <Outlet />
      </main>
    </div>
  );
};

export default AppLayout;
