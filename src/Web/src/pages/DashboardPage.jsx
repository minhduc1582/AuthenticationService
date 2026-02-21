import { useAuth } from '../context/AuthContext.jsx';

const DashboardPage = () => {
  const { user } = useAuth();

  return (
    <section className="panel">
      <header>
        <h1>Account overview</h1>
        <p>Review your profile, roles, and session details.</p>
      </header>

      <div className="info-grid">
        <div>
          <p className="label">Email</p>
          <p>{user?.email}</p>
        </div>
        <div>
          <p className="label">Display name</p>
          <p>{user?.displayName ?? 'Not set'}</p>
        </div>
        <div>
          <p className="label">Roles</p>
          <p>{user?.roles?.join(', ') || 'User'}</p>
        </div>
        <div>
          <p className="label">Email confirmed</p>
          <p>{user?.emailConfirmed ? 'Yes' : 'No'}</p>
        </div>
      </div>
    </section>
  );
};

export default DashboardPage;
