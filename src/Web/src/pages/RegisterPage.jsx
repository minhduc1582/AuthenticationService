import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { ApiError } from '../lib/apiClient.js';
import { useAuth } from '../context/AuthContext.jsx';

const RegisterPage = () => {
  const { register, initializing, csrfReady } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
  });
  const [status, setStatus] = useState({ type: '', message: '' });
  const [submitting, setSubmitting] = useState(false);

  const update = (field) => (event) => {
    setForm((prev) => ({ ...prev, [field]: event.target.value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    if (form.password !== form.confirmPassword) {
      setStatus({ type: 'error', message: 'Passwords do not match.' });
      return;
    }

    setSubmitting(true);
    setStatus({ type: '', message: '' });

    try {
      const payload = {
        email: form.email,
        password: form.password,
        confirmPassword: form.confirmPassword,
        firstName: form.firstName || undefined,
        lastName: form.lastName || undefined,
      };

      const result = await register(payload);
      setStatus({ type: 'success', message: result?.message ?? 'Registration successful.' });
      navigate('/login');
    } catch (error) {
      const message = error instanceof ApiError ? error.message : 'Unable to register.';
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
          <h1>Create an account</h1>
          <p className="caption">Register to access protected resources.</p>
        </header>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="form-grid">
            <label>
              <span>First name</span>
              <input
                type="text"
                value={form.firstName}
                onChange={update('firstName')}
                placeholder="Ada"
                disabled={submitting || initializing}
              />
            </label>
            <label>
              <span>Last name</span>
              <input
                type="text"
                value={form.lastName}
                onChange={update('lastName')}
                placeholder="Lovelace"
                disabled={submitting || initializing}
              />
            </label>
          </div>

          <label>
            <span>Email</span>
            <input
              type="email"
              name="email"
              autoComplete="email"
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
              autoComplete="new-password"
              required
              minLength={6}
              value={form.password}
              onChange={update('password')}
              placeholder="••••••••"
              disabled={submitting || initializing}
            />
          </label>

          <label>
            <span>Confirm password</span>
            <input
              type="password"
              name="confirmPassword"
              autoComplete="new-password"
              required
              minLength={6}
              value={form.confirmPassword}
              onChange={update('confirmPassword')}
              placeholder="••••••••"
              disabled={submitting || initializing}
            />
          </label>

          <button type="submit" disabled={submitting || initializing || !csrfReady}>
            {submitting ? 'Creating account...' : 'Register'}
          </button>
        </form>

        {status.message && (
          <p className={status.type === 'error' ? 'error' : 'success'} role="status">
            {status.message}
          </p>
        )}

        <p className="help-text">
          Already registered? <Link to="/login">Sign in here.</Link>
        </p>
      </section>
    </div>
  );
};

export default RegisterPage;
