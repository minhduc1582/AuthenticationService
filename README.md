# AuthenticationService
AuthService is a centralized authentication service that issues JWT tokens for securing local APIs.

## App Registration & Client Credentials
- Authenticated users can manage first-party applications via `/api/apps` (create, list, detail, secret rotation, expiration, enable/disable).
- Applications receive a `client_id`/`client_secret` pair (secret is hashed at rest) along with configured scopes; token requests are served through `POST /connect/token` (client credentials grant).
- Configure JWT signing details under the `Jwt` section in `appsettings*.json` (Issuer, Audience, SigningKey, AccessTokenLifetimeMinutes).
- The SPA (Vite/React under `src/Web`) exposes the feature at `/apps`, including creation, secret rotation, enable/disable toggles, and expiration controls. Newly generated secrets are shown once in a secure modal—prompt users to copy immediately.

## Browser Cookie & Routing Notes
- The default Identity cookie name (`__Host-auth`) uses the `__Host-` prefix, which requires `Secure=true`, `Path=/`, and **no** `Domain` attribute. If you need a shared cookie across subdomains, configure `Authentication:Cookie:Name` to a value that does not start with `__Host-` before setting `Authentication:Cookie:Domain`; the antiforgery cookie will automatically switch away from the `__Host-` prefix when a domain is provided.
- For local testing, ensure the SPA and API traffic flows through the same HTTPS origin (e.g., `https://as.shareservice.com:7229`). Requests sent directly to `https://localhost:7194` will not include the auth cookie, since it is scoped to the externally routed host.
