import { useCallback, useEffect, useMemo, useState } from 'react';
import { ApiError, appRegistrationsApi } from '../lib/apiClient.js';

const normalizeError = (error) => {
  if (error instanceof ApiError) {
    return error.details?.message ?? error.message;
  }

  return 'Something went wrong. Please try again.';
};

export const useAppRegistrations = () => {
  const [apps, setApps] = useState([]);
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [selectedId, setSelectedId] = useState(null);
  const [selectedApp, setSelectedApp] = useState(null);
  const [error, setError] = useState(null);
  const [pendingAction, setPendingAction] = useState(false);

  const fetchApps = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await appRegistrationsApi.list();
      setApps(result ?? []);
    } catch (err) {
      setError(normalizeError(err));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchApps();
  }, [fetchApps]);

  const loadDetails = useCallback(
    async (id) => {
      if (!id) {
        setSelectedApp(null);
        return;
      }

      setDetailLoading(true);
      setError(null);
      try {
        const detail = await appRegistrationsApi.get(id);
        setSelectedApp(detail);
      } catch (err) {
        setError(normalizeError(err));
      } finally {
        setDetailLoading(false);
      }
    },
    [],
  );

  const selectApp = useCallback(
    async (id) => {
      setSelectedId(id);
      await loadDetails(id);
    },
    [loadDetails],
  );

  const createApp = useCallback(
    async (payload) => {
      setPendingAction(true);
      setError(null);
      try {
        const result = await appRegistrationsApi.create(payload);
        const { appRegistration } = result;
        setApps((current) => [appRegistration, ...current]);
        setSelectedId(appRegistration.id);
        setSelectedApp(appRegistration);
        return result;
      } catch (err) {
        setError(normalizeError(err));
        throw err;
      } finally {
        setPendingAction(false);
      }
    },
    [],
  );

  const regenerateSecret = useCallback(
    async (id) => {
      setPendingAction(true);
      setError(null);
      try {
        const result = await appRegistrationsApi.regenerateSecret(id);
        await loadDetails(id);
        return result;
      } catch (err) {
        setError(normalizeError(err));
        throw err;
      } finally {
        setPendingAction(false);
      }
    },
    [loadDetails],
  );

  const toggleStatus = useCallback(async (id, isEnabled) => {
    setPendingAction(true);
    setError(null);
    try {
      await appRegistrationsApi.toggleStatus(id, isEnabled);
      setApps((current) =>
        current.map((app) => (app.id === id ? { ...app, isEnabled } : app)),
      );
      setSelectedApp((current) => (current?.id === id ? { ...current, isEnabled } : current));
    } catch (err) {
      setError(normalizeError(err));
      throw err;
    } finally {
      setPendingAction(false);
    }
  }, []);

  const setExpiration = useCallback(async (id, expirationUtc) => {
    setPendingAction(true);
    setError(null);
    try {
      await appRegistrationsApi.setExpiration(id, expirationUtc);
      setApps((current) =>
        current.map((app) => (app.id === id ? { ...app, expirationUtc } : app)),
      );
      setSelectedApp((current) => (current?.id === id ? { ...current, expirationUtc } : current));
    } catch (err) {
      setError(normalizeError(err));
      throw err;
    } finally {
      setPendingAction(false);
    }
  }, []);

  const scopesCatalog = useMemo(() => {
    const set = new Set();
    apps.forEach((app) => (app.scopes ?? []).forEach((scope) => set.add(scope)));
    return Array.from(set).sort();
  }, [apps]);

  return {
    apps,
    loading,
    error,
    detailLoading,
    selectedApp,
    selectApp,
    createApp,
    regenerateSecret,
    toggleStatus,
    setExpiration,
    pendingAction,
    refresh: fetchApps,
    scopesCatalog,
  };
};

export default useAppRegistrations;
