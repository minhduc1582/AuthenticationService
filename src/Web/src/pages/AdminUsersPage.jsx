import { useCallback, useEffect, useMemo, useState } from 'react';
import { ApiError, request } from '../lib/apiClient.js';

const AVAILABLE_ROLES = ['Admin', 'User'];

const emptyForm = {
  id: '',
  email: '',
  firstName: '',
  lastName: '',
  emailConfirmed: false,
  lockoutEnabled: false,
  lockoutEnd: '',
  roles: [],
  isDeleted: false,
};

const AdminUsersPage = () => {
  const [users, setUsers] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');
  const [includeDeleted, setIncludeDeleted] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [selectedUser, setSelectedUser] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [status, setStatus] = useState('');

  const totalPages = useMemo(() => Math.max(1, Math.ceil(total / pageSize)), [total, pageSize]);

  const formatLockoutInput = (value) => {
    if (!value) return '';
    const date = new Date(value);
    const tzOffset = date.getTimezoneOffset();
    const local = new Date(date.getTime() - tzOffset * 60000);
    return local.toISOString().slice(0, 16);
  };

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const params = new URLSearchParams({
        page: page.toString(),
        pageSize: pageSize.toString(),
        includeDeleted: includeDeleted ? 'true' : 'false',
      });
      if (search.trim()) {
        params.append('search', search.trim());
      }
      const data = await request(`/admin/users?${params.toString()}`);
      setUsers(data?.items ?? []);
      setTotal(data?.totalCount ?? 0);
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Unable to load users.';
      setError(message);
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, includeDeleted]);

  useEffect(() => {
    loadUsers();
  }, [loadUsers]);

  useEffect(() => {
    if (!selectedUser) {
      setForm(emptyForm);
      return;
    }

    setForm({
      id: selectedUser.id,
      email: selectedUser.email ?? '',
      firstName: selectedUser.firstName ?? '',
      lastName: selectedUser.lastName ?? '',
      emailConfirmed: Boolean(selectedUser.emailConfirmed),
      lockoutEnabled: Boolean(selectedUser.lockoutEnabled),
      lockoutEnd: formatLockoutInput(selectedUser.lockoutEnd),
      roles: selectedUser.roles ?? [],
      isDeleted: Boolean(selectedUser.isDeleted),
    });
  }, [selectedUser]);

  const selectUser = async (userId) => {
    if (selectedUser?.id === userId) {
      setSelectedUser(null);
      return;
    }

    try {
      const data = await request(`/admin/users/${userId}`);
      setSelectedUser(data);
      setStatus('');
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Unable to load user details.';
      setStatus(message);
    }
  };

  const updateForm = (field) => (event) => {
    const value =
      event.target.type === 'checkbox' ? event.target.checked : event.target.value;
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const toggleRole = (role) => {
    setForm((prev) => {
      const hasRole = prev.roles.includes(role);
      return {
        ...prev,
        roles: hasRole
          ? prev.roles.filter((r) => r !== role)
          : [...prev.roles, role],
      };
    });
  };

  const saveUser = async () => {
    if (!selectedUser) return;
    setSaving(true);
    setStatus('');
    try {
      const payload = {
        email: form.email,
        firstName: form.firstName || null,
        lastName: form.lastName || null,
        emailConfirmed: form.emailConfirmed,
        lockoutEnabled: form.lockoutEnabled,
        lockoutEnd: form.lockoutEnd ? new Date(form.lockoutEnd).toISOString() : null,
        unlockUser: !form.lockoutEnabled || !form.lockoutEnd,
      };

      await request(`/admin/users/${selectedUser.id}`, {
        method: 'PUT',
        body: payload,
        requireCsrf: true,
      });

      await request(`/admin/users/${selectedUser.id}/roles`, {
        method: 'POST',
        body: { roles: form.roles },
        requireCsrf: true,
      });

      setStatus('User updated successfully.');
      await loadUsers();
      const refreshed = await request(`/admin/users/${selectedUser.id}`);
      setSelectedUser(refreshed);
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Unable to save user.';
      setStatus(message);
    } finally {
      setSaving(false);
    }
  };

  const softDelete = async () => {
    if (!selectedUser) return;
    setSaving(true);
    setStatus('');
    try {
      await request(`/admin/users/${selectedUser.id}`, {
        method: 'DELETE',
        requireCsrf: true,
      });
      setStatus('User archived.');
      await loadUsers();
      const refreshed = await request(`/admin/users/${selectedUser.id}`);
      setSelectedUser(refreshed);
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Unable to archive user.';
      setStatus(message);
    } finally {
      setSaving(false);
    }
  };

  const restoreUser = async () => {
    if (!selectedUser) return;
    setSaving(true);
    setStatus('');
    try {
      await request(`/admin/users/${selectedUser.id}/restore`, {
        method: 'POST',
        requireCsrf: true,
      });
      setStatus('User restored.');
      await loadUsers();
      const refreshed = await request(`/admin/users/${selectedUser.id}`);
      setSelectedUser(refreshed);
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Unable to restore user.';
      setStatus(message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="panel">
      <header>
        <h1>User management</h1>
        <p>Search, update, and govern accounts.</p>
      </header>

      <div className="toolbar">
        <input
          type="search"
          placeholder="Search by email or name"
          value={search}
          onChange={(event) => {
            setSearch(event.target.value);
            setPage(1);
          }}
        />
        <label>
          <input
            type="checkbox"
            checked={includeDeleted}
            onChange={(event) => {
              setIncludeDeleted(event.target.checked);
              setPage(1);
            }}
          />
          Include archived
        </label>
        <button type="button" className="ghost" onClick={loadUsers}>
          Refresh
        </button>
      </div>

      {error && <p className="error">{error}</p>}

      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Email</th>
              <th>Name</th>
              <th>Roles</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {!loading && users.length === 0 && (
              <tr>
                <td colSpan={5} style={{ textAlign: 'center' }}>
                  No users found.
                </td>
              </tr>
            )}
            {users.map((user) => (
              <tr key={user.id} className={selectedUser?.id === user.id ? 'selected' : ''}>
                <td>{user.email}</td>
                <td>{user.displayName || `${user.firstName ?? ''} ${user.lastName ?? ''}`}</td>
                <td>{user.roles?.join(', ') || 'User'}</td>
                <td>
                  {user.isDeleted ? (
                    <span className="badge danger">Archived</span>
                  ) : (
                    <span className="badge success">Active</span>
                  )}
                </td>
                <td>
                  <button type="button" onClick={() => selectUser(user.id)}>
                    {selectedUser?.id === user.id ? 'Close' : 'Edit'}
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="pagination">
        <button type="button" disabled={page === 1} onClick={() => setPage((prev) => prev - 1)}>
          Previous
        </button>
        <span>
          Page {page} of {totalPages}
        </span>
        <button
          type="button"
          disabled={page >= totalPages}
          onClick={() => setPage((prev) => prev + 1)}
        >
          Next
        </button>
        <select
          value={pageSize}
          onChange={(event) => {
            setPageSize(Number(event.target.value));
            setPage(1);
          }}
        >
          {[10, 20, 50].map((size) => (
            <option key={size} value={size}>
              {size} per page
            </option>
          ))}
        </select>
      </div>

      {selectedUser && (
        <div className="detail-drawer">
          <div className="drawer-content">
            <header>
              <div>
                <h2>Edit user</h2>
                <p>{selectedUser.email}</p>
              </div>
              <button type="button" className="ghost" onClick={() => setSelectedUser(null)}>
                Close
              </button>
            </header>

            <div className="form-grid detail">
              <label>
                <span>First name</span>
                <input value={form.firstName} onChange={updateForm('firstName')} />
              </label>
              <label>
                <span>Last name</span>
                <input value={form.lastName} onChange={updateForm('lastName')} />
              </label>
              <label>
                <span>Email</span>
                <input type="email" value={form.email} onChange={updateForm('email')} required />
              </label>
              <label>
                <span>Lockout ends</span>
                <input
                  type="datetime-local"
                  value={form.lockoutEnd}
                  onChange={updateForm('lockoutEnd')}
                />
              </label>
            </div>

            <div className="toggles">
              <label>
                <input
                  type="checkbox"
                  checked={form.emailConfirmed}
                  onChange={updateForm('emailConfirmed')}
                />
                Email confirmed
              </label>
              <label>
                <input
                  type="checkbox"
                  checked={form.lockoutEnabled}
                  onChange={updateForm('lockoutEnabled')}
                />
                Lockout enabled
              </label>
            </div>

            <div className="roles">
              <p>Roles</p>
              {AVAILABLE_ROLES.map((role) => (
                <label key={role}>
                  <input
                    type="checkbox"
                    checked={form.roles.includes(role)}
                    onChange={() => toggleRole(role)}
                  />
                  {role}
                </label>
              ))}
            </div>

            {status && <p className="status-note">{status}</p>}

            <div className="drawer-actions">
              <button type="button" onClick={saveUser} disabled={saving}>
                {saving ? 'Saving...' : 'Save changes'}
              </button>
              {selectedUser.isDeleted ? (
                <button type="button" className="ghost" onClick={restoreUser} disabled={saving}>
                  Restore user
                </button>
              ) : (
                <button type="button" className="ghost" onClick={softDelete} disabled={saving}>
                  Archive user
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </section>
  );
};

export default AdminUsersPage;
