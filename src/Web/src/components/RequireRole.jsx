import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.jsx';

const RequireRole = ({ roles }) => {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  const allowed = roles?.some((role) => user.roles?.includes(role));
  if (!allowed) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
};

export default RequireRole;
