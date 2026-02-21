# Authentication Console (React + Vite)

This Vite app now serves as a lightweight console for interacting with the authentication API. It lets you log in, log out, and verify redirect behavior while persisting the issued JWT inside an HttpOnly cookie.

## Getting Started

```bash
cd src/Web
npm install
npm run dev
```

By default the client targets `https://as.shareservice.com:7229/api`. Override this by defining an environment variable in `.env.local`:

```
VITE_AUTH_API_BASE_URL=https://as.shareservice.com:7229/api
```

When calling the API from the browser, make sure you are using `fetch`/XHR with `credentials: 'include'` so that cookies issued by the server are stored and sent automatically.

> **Note:** The authentication cookie is scoped to the `https://as.shareservice.com:7229` origin. If you call the API directly via `https://localhost:7194`, the browser will treat it as a different site and omit the cookie, resulting in 401 responses. Use a hosts entry or reverse proxy so local requests continue to flow through `as.shareservice.com`.
