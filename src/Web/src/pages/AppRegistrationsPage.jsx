import { useEffect, useMemo, useState } from 'react';
import PageLoader from '../components/PageLoader.jsx';
import Modal from '../components/Modal.jsx';
import { useAppRegistrations } from '../hooks/useAppRegistrations.js';

const formatDateTime = (value) => {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '—';
  }

  return date.toLocaleString();
};

const toInputDateValue = (value) => {
  if (!value) {
    return '';
  }
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '';
  }
  const pad = (n) => n.toString().padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(
    date.getMinutes(),
  )}`;
};

const fromInputDateValue = (value) => {
  if (!value) {
    return null;
  }
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return null;
  }
  return date.toISOString();
};

const parseScopes = (value) =>
  value
    .split(/[\s,]+/)
    .map((scope) => scope.trim())
    .filter(Boolean);

const AppRegistrationsPage = () => {
  const {
    apps,
    loading,
    error,
    selectedApp,
    selectApp,
    detailLoading,
    createApp,
    regenerateSecret,
    toggleStatus,
    setExpiration,
    pendingAction,
    scopesCatalog,
  } = useAppRegistrations();

  const [createOpen, setCreateOpen] = useState(false);
  const [secretResult, setSecretResult] = useState(null);
  const [message, setMessage] = useState(null);
  const [expirationInput, setExpirationInput] = useState('');

  useEffect(() => {
    setExpirationInput(selectedApp ? toInputDateValue(selectedApp.expirationUtc) : '');
  }, [selectedApp]);

  const handleCreate = async (payload) => {
    const result = await createApp(payload);
    setSecretResult(result.clientSecret);
    setMessage('Application registered successfully.');
    return true;
  };

  const handleRegenerate = async () => {
    if (!selectedApp) {
      return;
    }
    const result = await regenerateSecret(selectedApp.id);
    setSecretResult(result);
    setMessage('Client secret regenerated. Copy it now; it will not be shown again.');
  };

  const handleToggle = async (app) => {
    await toggleStatus(app.id, !app.isEnabled);
    setMessage(`Application ${!app.isEnabled ? 'enabled' : 'disabled'} successfully.`);
  };

  const handleExpirationSave = async () => {
    if (!selectedApp) {
      return;
    }
    const isoValue = fromInputDateValue(expirationInput);
    await setExpiration(selectedApp.id, isoValue);
    setMessage('Expiration updated.');
  };

  const scopesHint = useMemo(() => {
    if (!scopesCatalog.length) {
      return 'Scopes will default to the system defaults when left empty.';
    }
    return `Available scopes: ${scopesCatalog.join(', ')}`;
  }, [scopesCatalog]);

  if (loading) {
    return <PageLoader />;
  }

  return (
    <section className="panel">
      <header>
        <h1>App registrations</h1>
        <p>Register first-party applications, manage secrets, and control access scopes.</p>
      </header>

      <div className="toolbar">
        <button type="button" onClick={() => setCreateOpen(true)}>
          Register app
        </button>
        {message && (
          <div className="success inline">
            <span>{message}</span>
            <button type="button" onClick={() => setMessage(null)}>
              ×
            </button>
          </div>
        )}
      </div>

      {error && <div className="error">{error}</div>}

      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Client ID</th>
              <th>Status</th>
              <th>Scopes</th>
              <th>Expires</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {apps.length === 0 && (
              <tr>
                <td colSpan={6}>No app registrations yet.</td>
              </tr>
            )}
            {apps.map((app) => (
              <tr key={app.id} className={selectedApp?.id === app.id ? 'selected' : ''}>
                <td>
                  <button type="button" className="linkish" onClick={() => selectApp(app.id)}>
                    {app.name}
                  </button>
                </td>
                <td className="mono">{app.clientId}</td>
                <td>
                  <span className={`badge ${app.isEnabled ? 'success' : 'danger'}`}>
                    {app.isEnabled ? 'Enabled' : 'Disabled'}
                  </span>
                </td>
                <td>
                  <div className="scope-list">
                    {(app.scopes ?? []).length === 0 && <span className="scope">default</span>}
                    {(app.scopes ?? []).map((scope) => (
                      <span key={scope} className="scope">
                        {scope}
                      </span>
                    ))}
                  </div>
                </td>
                <td>{formatDateTime(app.expirationUtc)}</td>
                <td>
                  <div className="action-group">
                    <button type="button" onClick={() => selectApp(app.id)}>
                      View
                    </button>
                    <button type="button" onClick={() => handleToggle(app)}>
                      {app.isEnabled ? 'Disable' : 'Enable'}
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="detail-drawer">
        <div className="drawer-content">
          {!selectedApp && <p>Select an app to view details.</p>}
          {selectedApp && (
            <>
              <header>
                <div>
                  <h2>{selectedApp.name}</h2>
                  <p className="muted">Client ID: {selectedApp.clientId}</p>
                </div>
                <div className="action-group">
                  <button type="button" disabled={pendingAction || detailLoading} onClick={handleRegenerate}>
                    Regenerate secret
                  </button>
                  <button type="button" className="ghost" onClick={() => selectApp(null)}>
                    Close
                  </button>
                </div>
              </header>

              <div className="form-grid detail">
                <label>
                  Description
                  <input type="text" value={selectedApp.description ?? ''} readOnly />
                </label>
                <label>
                  Status
                  <input type="text" value={selectedApp.isEnabled ? 'Enabled' : 'Disabled'} readOnly />
                </label>
                <label>
                  Created
                  <input type="text" value={formatDateTime(selectedApp.createdAt)} readOnly />
                </label>
                <label>
                  Last updated
                  <input type="text" value={formatDateTime(selectedApp.updatedAt)} readOnly />
                </label>
                <label>
                  Last secret rotation
                  <input type="text" value={formatDateTime(selectedApp.lastSecretRotatedAt)} readOnly />
                </label>
                <label>
                  Expiration
                  <div className="expiration-control">
                    <input
                      type="datetime-local"
                      value={expirationInput}
                      onChange={(event) => setExpirationInput(event.target.value)}
                    />
                    <button type="button" disabled={pendingAction} onClick={handleExpirationSave}>
                      Save
                    </button>
                  </div>
                </label>
              </div>

              <div className="scope-group">
                <p>Scopes</p>
                <div className="scope-list">
                  {(selectedApp.scopes ?? []).length === 0 && <span className="scope">default</span>}
                  {(selectedApp.scopes ?? []).map((scope) => (
                    <span key={scope} className="scope">
                      {scope}
                    </span>
                  ))}
                </div>
              </div>
            </>
          )}
        </div>
      </div>

      {createOpen && (
        <CreateAppModal
          busy={pendingAction}
          scopesHint={scopesHint}
          onClose={() => setCreateOpen(false)}
          onSubmit={handleCreate}
        />
      )}

      {secretResult && (
        <SecretModal
          result={secretResult}
          onClose={() => {
            setSecretResult(null);
          }}
        />
      )}
    </section>
  );
};

const CreateAppModal = ({ onClose, onSubmit, busy, scopesHint }) => {
  const [form, setForm] = useState({
    name: '',
    description: '',
    scopes: '',
    expiration: '',
  });
  const [error, setError] = useState(null);

  const handleChange = (event) => {
    const { name, value } = event.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError(null);

    try {
      const payload = {
        name: form.name.trim(),
        description: form.description.trim() || null,
        scopes: parseScopes(form.scopes),
        expirationUtc: fromInputDateValue(form.expiration),
      };

      const shouldClose = await onSubmit(payload);
      if (shouldClose) {
        onClose();
      }
    } catch (err) {
      setError(err?.message ?? 'Unable to create application.');
    }
  };

  return (
    <Modal
      title="Register application"
      onClose={onClose}
      footer={
        <button type="submit" form="create-app-form" disabled={busy}>
          Create
        </button>
      }
    >
      <form id="create-app-form" className="app-form" onSubmit={handleSubmit}>
        <label>
          Name
          <input name="name" value={form.name} onChange={handleChange} required />
        </label>
        <label>
          Description
          <input name="description" value={form.description} onChange={handleChange} placeholder="Optional" />
        </label>
        <label>
          Scopes
          <textarea
            name="scopes"
            value={form.scopes}
            onChange={handleChange}
            placeholder="space or comma separated"
            rows={2}
          />
          <small className="muted">{scopesHint}</small>
        </label>
        <label>
          Expiration
          <input
            type="datetime-local"
            name="expiration"
            value={form.expiration}
            onChange={handleChange}
            placeholder="Optional"
          />
        </label>
        {error && <div className="error">{error}</div>}
      </form>
    </Modal>
  );
};

const SecretModal = ({ result, onClose }) => {
  const [copied, setCopied] = useState(null);

  const handleCopy = async (value, field) => {
    await navigator.clipboard.writeText(value);
    setCopied(field);
    setTimeout(() => setCopied(null), 2000);
  };

  return (
    <Modal title="Client credentials" onClose={onClose}>
      <div className="secret-card">
        <p className="muted">
          Store these credentials securely. The client secret is shown only once; regenerating it will invalidate the
          previous secret immediately.
        </p>
        <div className="secret-row">
          <div>
            <p className="label">Client ID</p>
            <code>{result.clientId}</code>
          </div>
          <button type="button" onClick={() => handleCopy(result.clientId, 'clientId')}>
            {copied === 'clientId' ? 'Copied!' : 'Copy'}
          </button>
        </div>
        {result.clientSecret && (
          <div className="secret-row">
            <div>
              <p className="label">Client secret</p>
              <code>{result.clientSecret}</code>
            </div>
            <button type="button" onClick={() => handleCopy(result.clientSecret, 'clientSecret')}>
              {copied === 'clientSecret' ? 'Copied!' : 'Copy'}
            </button>
          </div>
        )}
        <small className="muted">Generated at: {formatDateTime(result.generatedAt ?? new Date().toISOString())}</small>
      </div>
    </Modal>
  );
};

export default AppRegistrationsPage;
