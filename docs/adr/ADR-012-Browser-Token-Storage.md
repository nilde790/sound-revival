# ADR-012: Token storage in the browser — in-memory access token, refresh token in an `HttpOnly` cookie

- **Status**: Accepted
- **Date**: 2026-11-23
- **Supersedes**: [ADR-008](ADR-008-V2-Committed-Scope.md) in part: 
  token refresh moves from v2 to v1 (phase 3). 
  [ADR-003](ADR-003-Authentication.md) still holds: API calls carry a 
  JWT in the `Authorization: Bearer` header.

## Context

The React frontend (phase 4) receives a JWT at login and must keep it 
to attach it to every request. Today (v1 as planned):

- the token lives until it expires (`Jwt:ExpiryMinutes`) and the server 
  cannot revoke it; logout only discards it on the client (FR-3);
- token refresh and server-side revocation are v2 scope (ADR-008).

The main threat is **XSS**: script injected into the page (e.g. through 
a compromised npm package) can read anything JavaScript can read and 
send it elsewhere. OWASP advises against keeping session tokens in 
`localStorage`. Browsers also increasingly block cookies sent between 
different sites, which constrains where the frontend and the API are 
deployed.

The decision is needed now, before phase 2, because the deployment 
platform (phase 2) must support it.

## Decision drivers

- A token stolen through XSS must be useful for minutes, not hours
- A session must survive a page reload
- Real logout: the server can revoke a session
- Consistency with ADR-003 (stateless bearer tokens for API calls)
- Learning and market value (refresh token rotation, cookie flags, CORS 
  with credentials are common interview topics)
- Cost proportionate to a portfolio project

## Considered options

### Option 1 — JWT in `localStorage`
- ✅ Simplest; survives reloads; very common in tutorials
- ❌ Any XSS script can read the token and use it from anywhere until it 
  expires; no revocation

### Option 2 — JWT in memory only
- ✅ Much harder for XSS to exfiltrate
- ❌ Lost on every reload: the user logs in again each time

### Option 3 — Short access token in memory + refresh token in an `HttpOnly` cookie
- ✅ The long-lived credential is unreadable by JavaScript; a stolen 
  access token expires in minutes; reloads work through a refresh call; 
  logout revokes server-side
- ❌ A table, two endpoints, rotation logic, a frontend interceptor; 
  frontend and API must be deployed on the same site

### Option 4 — Backend for Frontend (BFF), session cookie only
- ✅ The browser never sees a token: the strongest option
- ❌ Contradicts ADR-003 and adds a server component; its benefits show 
  mostly with external identity providers, which this project doesn't use

## Decision

**Option 3.**

- **Access token**: the existing JWT, short-lived (target 15 minutes, 
  `Jwt:ExpiryMinutes`), returned in the response body, kept **in memory 
  only** by the frontend and sent as `Authorization: Bearer`.
- **Refresh token**: a random opaque value (at least 32 bytes from a 
  cryptographic generator, not a JWT), with a longer lifetime (target 
  7 days, configurable). The server stores only its **hash**, with the 
  user, expiry and revocation data. It travels in a cookie with 
  `HttpOnly; Secure; SameSite=Strict; Path=/api/auth`.
- **Rotation**: each refresh consumes the token and issues a new one. 
  Presenting an already-used token revokes all of that user's refresh 
  tokens (reuse detection: a stolen token is noticed the first time 
  either copy is used again).
- **Endpoints** (phase 3): `POST /api/auth/refresh` and 
  `POST /api/auth/logout`; login and register also set the cookie.
- **Same-site deployment**: the frontend and the API are served from 
  the same site: either one origin through a reverse proxy, or 
  subdomains of a domain we own. Chosen in the deployment ADR (phase 2). 
  Locally, the Vite dev server proxies `/api`.
- **CORS**: if the frontend and the API are different origins on the 
  same site, the policy allows credentials for the explicit frontend 
  origin only (never a wildcard).
- **CSRF**: only `refresh` and `logout` rely on the cookie. 
  `SameSite=Strict` keeps it off cross-site requests, and CORS stops 
  another site from reading the new access token.
- **XSS defenses remain necessary**: React's output escaping, no 
  `dangerouslySetInnerHTML` on user content, a Content Security Policy. 
  While a page is open, injected script can still call the API as the 
  user; this option limits theft, not every abuse.

## Consequences

- **Positive**: a token stolen through XSS is valid for minutes; 
  sessions survive reloads; logout is real (FR-3 gains server-side 
  revocation in v1); the auth design matches common professional 
  practice.
- **Negative**: a `RefreshTokens` table and migration, two endpoints, 
  rotation and reuse-detection logic with their tests, cleanup of 
  expired rows, an Axios interceptor that refreshes on `401` and 
  retries; a deployment constraint (possibly a paid domain).
- **Follow-ups**:
  - 02-Requirements (FR-3), 07-API-Design (auth endpoints), 
    08-Frontend-Design (auth state), 01-Vision and 09-Roadmap (token 
    refresh leaves v2, joins phase 3) are updated with the roadmap 
    revision.
  - The deployment ADR (phase 2) must satisfy the same-site constraint.
  - `12-Security.md` (phase 3) details lifetimes, cookie settings, 
    cleanup and the CSP.
  - Debt C13 (role claim name readable by the frontend) still applies 
    to the access token.

## Links

- [ADR-003](ADR-003-Authentication.md) — JWT-based authentication
- [ADR-008](ADR-008-V2-Committed-Scope.md) — v2 scope (partly superseded)
- FR-3, NFR-13 in [`../02-Requirements.md`](../02-Requirements.md)
- [OWASP HTML5 Security Cheat Sheet → Local Storage](https://cheatsheetseries.owasp.org/cheatsheets/HTML5_Security_Cheat_Sheet.html#local-storage)
- [IETF draft: OAuth 2.0 for Browser-Based Applications](https://datatracker.ietf.org/doc/draft-ietf-oauth-browser-based-apps/)
- [MDN: `Set-Cookie`](https://developer.mozilla.org/en-US/docs/Web/HTTP/Reference/Headers/Set-Cookie)
