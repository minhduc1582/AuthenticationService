import { Navigate, Outlet, useLocation } from 'react-router-dom';
import PageLoader from './PageLoader.jsx';
import { useAuth } from '../context/AuthContext.jsx';

const ProtectedRoute = () => {
  const location = useLocation();
  const { user, initializing } = useAuth();

  if (initializing) {
    return <PageLoader />;
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
