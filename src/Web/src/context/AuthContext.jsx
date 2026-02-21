import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError, clearCsrfToken, refreshCsrfToken, request } from '../lib/apiClient.js';

const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [initializing, setInitializing] = useState(true);
  const [csrfReady, setCsrfReady] = useState(false);

  const loadCurrentUser = useCallback(async () => {
    try {
      const response = await request('/account/me');
      setUser(response);
    } catch (error) {
      if (!(error instanceof ApiError) || error.status !== 401) {
        console.error('Failed to load current user.', error);
      }
      setUser(null);
    } finally {
      setInitializing(false);
    }
  }, []);

  const primeCsrf = useCallback(async () => {
    try {
      await refreshCsrfToken();
    } catch (error) {
      console.warn('Unable to initialize anti-forgery token. Continuing anyway.', error);
    } finally {
      setCsrfReady(true);
    }
  }, []);

  useEffect(() => {
    loadCurrentUser();
  }, [loadCurrentUser]);

  useEffect(() => {
    primeCsrf();
  }, [primeCsrf]);

  const login = useCallback(
    async (payload) => {
      const result = await request('/account/login', {
        method: 'POST',
        body: payload,
        requireCsrf: true,
      });
      await loadCurrentUser();
      await refreshCsrfToken();
      return result;
    },
    [loadCurrentUser],
  );

  const register = useCallback(
    async (payload) => {
      const result = await request('/account/register', {
        method: 'POST',
        body: payload,
        requireCsrf: true,
      });
      await loadCurrentUser();
      return result;
    },
    [loadCurrentUser],
  );

  const logout = useCallback(async () => {
    try {
      await request('/account/logout', { method: 'POST', requireCsrf: true });
    } finally {
      clearCsrfToken();
      setUser(null);
    }
  }, []);

  const value = useMemo(
    () => ({
      user,
      initializing,
      csrfReady,
      login,
      register,
      logout,
      refreshUser: loadCurrentUser,
      hasRole: (role) => user?.roles?.some((candidate) => candidate === role) ?? false,
    }),
    [user, initializing, csrfReady, login, register, logout, loadCurrentUser],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
