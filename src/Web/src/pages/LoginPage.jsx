import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { ApiError } from '../lib/apiClient.js';
import { useAuth } from '../context/AuthContext.jsx';

const LoginPage = () => {
  const { login, initializing, csrfReady } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname ?? '/';

  const [form, setForm] = useState({
    email: '',
    password: '',
    rememberMe: false,
  });
  const [status, setStatus] = useState({ type: '', message: '' });
  const [submitting, setSubmitting] = useState(false);

  const update = (field) => (event) => {
    const value = field === 'rememberMe' ? event.target.checked : event.target.value;
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (submitting) return;
    setSubmitting(true);
    setStatus({ type: '', message: '' });

    try {
      const result = await login(form);
      setStatus({ type: 'success', message: result?.message ?? 'Signed in successfully.' });
      navigate(result?.redirectUrl ?? from, { replace: true });
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to login.';
      setStatus({ type: 'error', message });
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="auth-page">
      <section className="auth-card">
        <header>
          <p className="eyebrow">Authentication Service</p>
          <h1>Welcome back</h1>
          <p className="caption">Use your credentials to access protected resources.</p>
        </header>

        <form className="auth-form" onSubmit={handleSubmit}>
          <label>
            <span>Email</span>
            <input
              type="email"
              name="email"
              autoComplete="username"
              required
              value={form.email}
              onChange={update('email')}
              placeholder="you@example.com"
              disabled={submitting || initializing}
            />
          </label>

          <label>
            <span>Password</span>
            <input
              type="password"
              name="password"
              autoComplete="current-password"
              required
              value={form.password}
              onChange={update('password')}
              placeholder="••••••••"
              disabled={submitting || initializing}
            />
          </label>

          <label className="remember-me">
            <input
              type="checkbox"
              checked={form.rememberMe}
              onChange={update('rememberMe')}
              disabled={submitting || initializing || !csrfReady}
            />
            Remember me on this device
          </label>

          <button type="submit" disabled={submitting || initializing || !csrfReady}>
            {submitting ? 'Signing in...' : 'Sign in'}
          </button>
        </form>

        {status.message && (
          <p className={status.type === 'error' ? 'error' : 'success'} role="status">
            {status.message}
          </p>
        )}

        <p className="help-text">
          Need an account? <Link to="/register">Create one now.</Link>
        </p>
      </section>
    </div>
  );
};

export default LoginPage;
