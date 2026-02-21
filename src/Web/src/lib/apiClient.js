const DEFAULT_API_BASE = 'https://localhost:7194/api';

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? DEFAULT_API_BASE).replace(/\/$/, '');

let csrfToken = null;
let csrfPromise = null;

export class ApiError extends Error {
  constructor(message, status, details) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

const parseResponse = async (response) => {
  if (response.status === 204) {
    return null;
  }

  const contentType = response.headers.get('content-type');
  if (contentType?.includes('application/json')) {
    return await response.json();
  }

  return response.text();
};

const ensureCsrfToken = async () => {
  if (csrfToken) {
    return csrfToken;
  }

  if (!csrfPromise) {
    csrfPromise = fetch(`${apiBaseUrl}/account/antiforgery`, {
      credentials: 'include',
    })
      .then(async (response) => {
        if (!response.ok) {
          throw new ApiError('Unable to establish anti-forgery token.', response.status);
        }
        const payload = await response.json();
        csrfToken = payload?.token ?? null;
        return csrfToken;
      })
      .finally(() => {
        csrfPromise = null;
      });
  }

  return csrfPromise;
};

const buildError = async (response) => {
  const payload = await parseResponse(response);
  let message = 'Request failed';

  if (typeof payload === 'string' && payload.trim().length > 0) {
    message = payload;
  } else if (payload?.message) {
    message = payload.message;
  }

  return new ApiError(message, response.status, payload);
};

export const clearCsrfToken = () => {
  csrfToken = null;
};

export const refreshCsrfToken = async () => {
  clearCsrfToken();
  return ensureCsrfToken();
};

export const request = async (path, options = {}, attempt = 0) => {
  const {
    method = 'GET',
    body,
    headers = {},
    requireCsrf,
  } = options;

  const config = {
    method,
    headers: {
      Accept: 'application/json',
      ...headers,
    },
    credentials: 'include',
  };

  if (body !== undefined) {
    config.headers['Content-Type'] = 'application/json';
    config.body = JSON.stringify(body);
  }

  if ((requireCsrf ?? method !== 'GET') && method !== 'HEAD') {
    const token = await ensureCsrfToken();
    if (token) {
      config.headers['X-CSRF-TOKEN'] = token;
    }
  }

  const response = await fetch(`${apiBaseUrl}${path}`, config);

  if (!response.ok) {
    if ((response.status === 400 || response.status === 403) && attempt === 0) {
      // Retry once after refreshing the antiforgery token/cookie pair.
      clearCsrfToken();
      await refreshCsrfToken();
      return request(path, options, attempt + 1);
    }

    if (response.status === 401 || response.status === 403) {
      throw new ApiError(response.status === 401 ? 'Unauthorized' : 'Forbidden', response.status);
    }

    throw await buildError(response);
  }

  return parseResponse(response);
};

const appsBase = '/apps';

export const appRegistrationsApi = {
  list: () => request(appsBase),
  get: (id) => request(`${appsBase}/${id}`),
  create: (payload) =>
    request(appsBase, {
      method: 'POST',
      body: payload,
      requireCsrf: true,
    }),
  regenerateSecret: (id) =>
    request(`${appsBase}/${id}/secret`, {
      method: 'POST',
      requireCsrf: true,
    }),
  setExpiration: (id, expirationUtc) =>
    request(`${appsBase}/${id}/expiration`, {
      method: 'PATCH',
      body: { expirationUtc },
      requireCsrf: true,
    }),
  toggleStatus: (id, isEnabled) =>
    request(`${appsBase}/${id}/status`, {
      method: 'PATCH',
      body: { isEnabled },
      requireCsrf: true,
    }),
};
