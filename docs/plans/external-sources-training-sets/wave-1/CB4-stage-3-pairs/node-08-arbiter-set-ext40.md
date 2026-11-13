# CB4 Stage 3 — Node 08 Arbiter Set ext40
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1037 through CB4-3.4-1044 (8 base pairs × 2 phrasings = 16 instances)
# New domains: mtls-certificate-binding, oidc-prompt-login, pkce-challenge-storage, state-parameter-echo, authz-code-lifetime, id-token-alg-none, jwt-revocation-check, client-secret-url-exposure
# Running total after this file: 120 + (1044 − 60) × 2 = 2088 instances (1044 base pairs)
# Sources: RFC 8705 §3.1, OIDC Core §3.1.2.1, RFC 7636 §4.4, RFC 6749 §4.1.2, RFC 6749 §10.5, RFC 8725 §2.1, RFC 7009, RFC 6749 §2.3.1

---

## Pair[CB4-3.4-1037-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** mTLS certificate-bound access token — RS does not validate `cnf/x5t#S256` against TLS client certificate
**Expected verdict:** REPROCESS

**A:**
A resource server accepts mTLS (mutual TLS) certificate-bound access tokens (RFC 8705). The AS embeds a `cnf` claim in the JWT with an `x5t#S256` value (SHA-256 thumbprint of the client's TLS certificate). The RS validates the token's signature, `iss`, `aud`, `exp`, and `scope`. However, the RS does not extract the client's TLS certificate from the mTLS connection and does not compare it against the `cnf/x5t#S256` value in the token. The rationale: "Validating the certificate thumbprint adds latency and complexity. The mTLS connection itself proves the client has a certificate — that's sufficient."

**B:**
REPROCESS. RFC 8705 §3.1 is explicit: "To bind an access token to a client certificate, the authorization server associates the certificate with the access token. When the resource server receives the access token, it MUST verify that the `x5t#S256` ... matches the certificate used by the client when making the request." The entire value of mTLS certificate-bound tokens is that a stolen bearer token cannot be replayed — the attacker must also possess the private key corresponding to the bound certificate. Accepting the `cnf` claim without validating it against the actual TLS certificate presented nullifies this protection entirely: any client that can present a valid signed token can use it, regardless of whether it holds the bound key. The "mTLS connection proves client has a certificate" argument is rejected: the RS cannot know whether the certificate used in the current connection matches the one bound at issuance without explicitly comparing the thumbprints.

**Reason:** RFC 8705 §3.1 MUST: RS must verify x5t#S256 in cnf claim matches TLS client certificate. Omitting thumbprint comparison nullifies certificate binding entirely — token replay protection lost. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8705 §3.1 MUST: RS must verify cnf/x5t#S256 against TLS client certificate on each request. Skipping thumbprint validation makes mTLS certificate binding a no-op — attacker with stolen token but different certificate passes validation. Certificate binding's replay protection is only effective if the RS checks it. REPROCESS.

---

## Pair[CB4-3.4-1037-B]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates `cnf/x5t#S256` against TLS client certificate — certificate binding enforced
**Expected verdict:** PASS

**A:**
The resource server handles mTLS certificate-bound access tokens per RFC 8705. On each request, the RS: (1) extracts the SHA-256 thumbprint of the TLS client certificate from the connection; (2) retrieves the `cnf/x5t#S256` value from the validated JWT access token; (3) compares the two values byte-for-byte. If they do not match, the request is rejected with 401. The comparison is performed after signature, iss, aud, and exp checks. Requests without a client certificate on a certificate-required endpoint are rejected with 401 before JWT processing.

**B:**
PASS. RFC 8705 §3.1 MUST satisfied: RS verifies cnf/x5t#S256 against TLS client certificate on every request. Certificate binding enforcement is complete: a token can only be used by the client whose certificate thumbprint is embedded in the `cnf` claim at issuance. Token replay by a different client (certificate mismatch) is rejected. Requests without a client certificate are rejected before token processing. No violations found.

**Reason:** RFC 8705 §3.1 MUST satisfied. cnf/x5t#S256 compared against TLS client cert on every request. Certificate binding replay protection fully operational. PASS.

**Phrasing-B:**
PASS. RS extracts TLS client cert thumbprint and compares against cnf/x5t#S256 in JWT per RFC 8705 §3.1 MUST. Token-cert mismatch → 401. No-cert → 401 before JWT processing. mTLS certificate binding fully enforced. PASS.

---

## Pair[CB4-3.4-1038-A]

**Territory:** T-oauth-oidc · T-asvs-token
**Domain:** OIDC `prompt=login` parameter ignored — AS returns existing session without re-authentication
**Expected verdict:** REPROCESS

**A:**
An OpenID Connect (OIDC) Relying Party (RP) sends an authorization request with `prompt=login` to signal that the user must re-authenticate (not use an existing session). The Authorization Server has a valid user session and returns an `id_token` with the current session's `auth_time` value without prompting the user to authenticate again. The AS developer reasoning: "The user is already logged in. Forcing re-authentication creates friction. `prompt=login` is a hint, not a mandate."

**B:**
REPROCESS. OIDC Core §3.1.2.1 is normative: "If this parameter contains `login`, the Authorization Server MUST prompt the End-User for reauthentication. If it cannot reauthenticate the End-User, it MUST return an error, typically `login_required`." This is a mandatory capability: the RP uses `prompt=login` for security-sensitive flows — step-up authentication before a high-risk operation, confirming identity before a password change, re-establishing identity after a suspicious event. Returning a stale session assertion in place of fresh authentication violates the semantics of the `prompt` parameter and defeats its purpose entirely. The "hint, not a mandate" characterization is incorrect — OIDC Core uses MUST language. If re-authentication is not possible, the AS MUST return `login_required`; it MUST NOT silently return the existing session.

**Reason:** OIDC Core §3.1.2.1 MUST: prompt=login requires AS to reauthenticate the user. Silent session reuse violates the MUST; "hint" characterization is incorrect. REPROCESS.

**Phrasing-B:**
REPROCESS. OIDC Core §3.1.2.1 MUST: prompt=login requires AS to force re-authentication; if unable, MUST return login_required error. Returning existing session on prompt=login violates the mandatory re-authentication requirement — RP's step-up security flow is silently bypassed. REPROCESS.

---

## Pair[CB4-3.4-1038-B]

**Territory:** T-oauth-oidc · T-asvs-token
**Domain:** OIDC `prompt=login` honored — AS forces re-authentication; `login_required` returned when impossible
**Expected verdict:** PASS

**A:**
The Authorization Server handles the `prompt=login` parameter per OIDC Core. When an authorization request includes `prompt=login`: the AS ignores any existing session and presents the authentication UI (User Interface) to the user. The resulting `id_token` contains an `auth_time` reflecting the new authentication event. If the AS cannot present interactive authentication (e.g., the `prompt` parameter also contains `none`), it returns `error=login_required` to the redirect URI. The `prompt=login` handling is consistent for all AS flows including SSO (Single Sign-On) sessions.

**B:**
PASS. OIDC Core §3.1.2.1 MUST satisfied: AS forces re-authentication on prompt=login; existing sessions are not reused. New auth_time in id_token reflects actual re-authentication. login_required error returned when interactive authentication is not possible. No violations found.

**Reason:** OIDC Core §3.1.2.1 MUST satisfied. prompt=login → forced re-auth; existing session ignored. login_required returned when re-auth not possible. PASS.

**Phrasing-B:**
PASS. AS honors OIDC Core §3.1.2.1 MUST for prompt=login: existing session ignored; user presented authentication UI; auth_time updated. login_required returned if interactive auth unavailable. PASS.

---

## Pair[CB4-3.4-1039-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PKCE `code_challenge` not stored server-side — AS accepts any `code_verifier` at code exchange
**Expected verdict:** REPROCESS

**A:**
An Authorization Server implements PKCE (Proof Key for Code Exchange, RFC 7636) for public clients. On the authorization request, the AS accepts and logs the `code_challenge` and `code_challenge_method` parameters. However, these values are not persisted with the authorization code server-side. At the token endpoint, when the client presents `code_verifier`, the AS performs no verification: it does not retrieve a stored challenge, does not compute `BASE64URL(SHA256(code_verifier))`, and does not compare the result against the challenge. The AS issues tokens for any `code_verifier` value that is present and non-empty. The developer comment: "We validate that code_verifier is present — that proves the client sent it. The PKCE flow is implemented."

**B:**
REPROCESS. RFC 7636 §4.4 states the AS "MUST ... verify that the code_challenge parameter stored with the authorization code was generated from the code_verifier that the client provides." The entire security property of PKCE is that only the initiator of the authorization request — the one who generated the random `code_verifier` — can exchange the code. If the AS accepts any non-empty `code_verifier` without comparing it against the stored `code_challenge`, PKCE provides zero protection: an attacker who intercepts the authorization code can exchange it immediately with any arbitrary verifier string. The "verifier is present = PKCE is implemented" argument is rejected entirely: presence without cryptographic verification is not PKCE. RFC 7636 §4.4's MUST requires the actual comparison. Storing the challenge at authorization request time and comparing at token request time is the core mechanism.

**Reason:** RFC 7636 §4.4 MUST: AS must verify code_verifier against stored code_challenge. Accepting any non-empty verifier is not PKCE — authorization code interception attack is fully viable. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7636 §4.4 MUST: AS must store code_challenge at authorization request and verify code_verifier at token request. Accepting any verifier string without cryptographic comparison negates all PKCE protection. Authorization code interception attack fully viable. "Verifier is present" ≠ PKCE implemented. REPROCESS.

---

## Pair[CB4-3.4-1039-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PKCE `code_challenge` stored server-side and verified at code exchange
**Expected verdict:** PASS

**A:**
The Authorization Server stores `code_challenge` and `code_challenge_method` values server-side, associated with the authorization code, at authorization request time. At the token endpoint, when `code_verifier` is submitted: for `S256` method, the AS computes `BASE64URL(SHA256(ASCII(code_verifier)))` and performs a constant-time comparison with the stored `code_challenge`. Mismatch results in `error=invalid_grant`. For requests where the original authorization included a code_challenge but no code_verifier is submitted at exchange, the exchange is rejected. Public clients are required to use PKCE; token requests without PKCE from public clients are rejected.

**B:**
PASS. RFC 7636 §4.4 MUST satisfied: code_challenge stored at authorization request; code_verifier cryptographically verified at token request via S256. Constant-time comparison prevents timing attacks. Missing verifier on PKCE-required flow is rejected. PKCE protection fully operational. No violations found.

**Reason:** RFC 7636 §4.4 MUST satisfied. code_challenge stored; code_verifier verified via S256 with constant-time comparison. Missing verifier rejected. PKCE authorization code interception protection enforced. PASS.

**Phrasing-B:**
PASS. AS stores code_challenge at authorization request; verifies BASE64URL(SHA256(code_verifier)) == code_challenge at token exchange per RFC 7636 §4.4 MUST. Constant-time comparison; missing verifier rejected. PKCE protection fully operational. PASS.

---

## Pair[CB4-3.4-1040-A]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** AS omits `state` parameter from authorization response when client included it
**Expected verdict:** REPROCESS

**A:**
A client sends an authorization request with a `state` parameter value (a randomly generated CSRF (Cross-Site Request Forgery) token). The Authorization Server processes the request and returns the authorization response to the redirect URI, but omits the `state` parameter from the response. The AS rationale: "State is a client-side construct — it's the client's responsibility to manage. The AS doesn't need to echo it back."

**B:**
REPROCESS. RFC 6749 §4.1.2 states: "If the `state` request parameter was present in the client authorization request, it MUST be returned to the client in the response." This is REQUIRED (RFC 2119 MUST). The `state` parameter serves as a CSRF (Cross-Site Request Forgery) token: the client generates a random value, stores it in the session, and verifies the echoed value in the response matches the stored value before accepting the authorization code. If the AS silently drops the `state`, the client cannot perform CSRF verification — every authorization response is accepted regardless of whether it was initiated by the client or injected by an attacker. The "client-side construct" argument is rejected: while the client generates `state`, the AS is REQUIRED to echo it back to enable CSRF verification. Omitting it breaks the CSRF protection mechanism.

**Reason:** RFC 6749 §4.1.2 MUST (REQUIRED): if state present in request, AS must return it in response. Omitting state breaks CSRF protection — client cannot verify the response was initiated by it. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.2 REQUIRED: AS must echo state parameter in authorization response when client included it. Missing state prevents CSRF verification at client — any injected authorization response is accepted. "Client-side construct" argument rejected; echoing state is a normative MUST. REPROCESS.

---

## Pair[CB4-3.4-1040-B]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** AS echoes `state` parameter unmodified in authorization response
**Expected verdict:** PASS

**A:**
The Authorization Server unconditionally echoes the `state` parameter value in the authorization response when it was present in the authorization request. The AS treats `state` as an opaque string — it neither validates, transforms, nor stores the value; it simply returns it unmodified. The authorization response includes `state` as a query parameter on the redirect URI when code flow is used, and as a fragment parameter for implicit flow. The client validates the echoed `state` against its session-stored value before processing the authorization code.

**B:**
PASS. RFC 6749 §4.1.2 REQUIRED satisfied: state parameter echoed in authorization response when present in request. State value treated as opaque and returned unmodified — correct AS behavior. Client-side CSRF verification is enabled. No violations found.

**Reason:** RFC 6749 §4.1.2 REQUIRED satisfied. State echoed unmodified in authorization response. Client CSRF verification enabled. PASS.

**Phrasing-B:**
PASS. AS echoes state parameter unmodified in authorization response per RFC 6749 §4.1.2 REQUIRED. State opaque to AS; returned as-is. Client can verify against session-stored value. CSRF protection mechanism intact. PASS.

---

## Pair[CB4-3.4-1041-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Authorization code lifetime set to 10 minutes — extended beyond recommended short-lived window
**Expected verdict:** CONCERN

**A:**
An Authorization Server issues authorization codes with a 10-minute expiry. The implementation note: "10 minutes gives clients ample time to exchange the code even under adverse network conditions. Short-lived codes create a bad UX (User Experience) when network latency is high." RFC 6749 §10.5 is acknowledged: the implementation considers 10 minutes to still qualify as "short-lived" in practice.

**B:**
CONCERN. RFC 6749 §10.5 states: "Authorization codes MUST be short-lived and single-use. If the authorization server observes multiple attempts to exchange an authorization code, the authorization server SHOULD revoke all tokens granted based on the compromised authorization code." RFC 6749 does not specify a maximum lifetime — "short-lived" is not quantified in the RFC itself. However, ASVS V3.5.2 states: "Verify that OAuth 2.0 authorization codes are single-use, expire in the shortest possible time, and are bound to the client for which they were issued." The OWASP OAuth 2.0 Security Cheat Sheet and common AS implementations treat 1–2 minutes as the practical "short-lived" interpretation. 10 minutes is widely considered excessive: the interception window for an authorization code is extended 5–10× beyond accepted practice, increasing CSRF and replay risk. No RFC uses MUST language to prohibit a specific maximum lifetime — this is a guidance-level gap. CONCERN. Correct practice: reduce to ≤2 minutes.

**Reason:** RFC 6749 §10.5 SHOULD short-lived (not quantified); ASVS V3.5.2 guidance: "shortest possible time." 10-minute codes exceed accepted practice (1–2 min). No MUST for specific max lifetime. Guidance gap = CONCERN.

**Phrasing-B:**
CONCERN. RFC 6749 §10.5: authorization codes MUST be short-lived (duration not specified). ASVS V3.5.2: "shortest possible time." 10-minute lifetime exceeds accepted practice (1–2 minutes widely used). Interception window 5–10× broader than recommended. No RFC MUST for specific numeric limit. Guidance-level gap = CONCERN. Recommend: reduce to ≤2 minutes.

---

## Pair[CB4-3.4-1041-B]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Authorization code lifetime set to 60 seconds — single-use, client-bound, reuse-detected
**Expected verdict:** PASS

**A:**
The Authorization Server issues authorization codes with a 60-second expiry. Codes are single-use: after a successful token exchange or a failed attempt, the code is immediately invalidated. Codes are bound to the client_id that requested them — a code cannot be exchanged by a different client_id. Code reuse attempts trigger immediate invalidation of all tokens previously issued for that code and an alert to the security log.

**B:**
PASS. RFC 6749 §10.5 MUST satisfied: codes are short-lived (60 seconds) and single-use. ASVS V3.5.2 satisfied: single-use, short lifetime, client-bound. Code reuse detection with token revocation meets RFC 6749 §10.5 SHOULD for observing multiple exchange attempts. No violations found.

**Reason:** RFC 6749 §10.5 MUST satisfied. Codes short-lived (60s), single-use, client-bound. Reuse detection + token revocation per RFC 6749 §10.5 SHOULD. ASVS V3.5.2 satisfied. PASS.

**Phrasing-B:**
PASS. 60-second authorization codes, single-use, client_id-bound per RFC 6749 §10.5 MUST and ASVS V3.5.2. Reuse attempt triggers immediate invalidation of all dependent tokens. PASS.

---

## Pair[CB4-3.4-1042-A]

**Territory:** T-rfc-jwt · T-oauth-oidc
**Domain:** OIDC RP accepts `id_token` with `alg: none` — unsigned token accepted as valid
**Expected verdict:** REPROCESS

**A:**
An OpenID Connect (OIDC) Relying Party (RP) validates `id_tokens` from the Authorization Server. The validation logic checks `iss`, `aud`, `exp`, and `nonce` claims. For the signature check, the RP reads the `alg` header of the JWT. If `alg` is `none`, the RP skips signature verification entirely, treating the payload as self-asserted. The developer reasoning: "Some internal AS deployments don't sign tokens. `alg: none` is a valid JWT algorithm per RFC 7518. We support it for flexibility."

**B:**
REPROCESS. RFC 8725 §2.1 states: "Implementations MUST NOT accept algorithms that offer no or insufficient security, e.g., `alg: none`." OIDC Core §2 mandates that id_tokens be signed. Accepting `alg: none` at an RP means any party can construct an arbitrary `id_token` with any claims — there is no cryptographic binding between the token and the issuing AS. This enables complete identity impersonation: an attacker fabricates an `id_token` with any `sub`, `email`, or role claims, sets `alg: none`, and the RP accepts it as valid. The "valid per RFC 7518" argument is rejected: RFC 8725 §2.1's MUST NOT explicitly overrides the algorithmic flexibility of RFC 7518. "Internal deployments" do not exempt an RP from this requirement — if the token crosses a trust boundary (even internal), it must be signed.

**Reason:** RFC 8725 §2.1 MUST NOT: implementations must not accept alg:none. Unsigned id_token enables complete identity impersonation. "Valid per RFC 7518" rejected — RFC 8725 §2.1 explicitly prohibits accepting it. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8725 §2.1 MUST NOT: accept alg:none or algorithms offering no security. OIDC id_token accepted without signature = attacker can fabricate arbitrary identity claims. "RFC 7518 allows alg:none" rejected; RFC 8725 §2.1 explicitly prohibits accepting it. REPROCESS.

---

## Pair[CB4-3.4-1042-B]

**Territory:** T-rfc-jwt · T-oauth-oidc
**Domain:** OIDC RP rejects `id_token` with `alg: none`; only allowlisted signing algorithms accepted
**Expected verdict:** PASS

**A:**
The Relying Party validates `id_token` signatures using a pre-configured allowlist of algorithms: `RS256`, `RS384`, `RS512`, `ES256`, `ES384`, `ES512`. The allowlist is explicit — any algorithm not on the list results in token rejection with an appropriate error. `alg: none` is not on the allowlist. The RP fetches the AS's JWKS (JSON Web Key Set) from the pre-configured endpoint at startup and refreshes it on key rotation. Tokens with unrecognized `kid` values trigger a JWKS refresh before rejection.

**B:**
PASS. RFC 8725 §2.1 MUST NOT satisfied: alg:none not accepted; explicit algorithm allowlist prevents algorithm confusion attacks. OIDC Core signature requirement satisfied. Pre-configured JWKS endpoint follows RFC 8725 §3.3 requirements (no dynamic key URL resolution). No violations found.

**Reason:** RFC 8725 §2.1 MUST NOT satisfied. Explicit algorithm allowlist; alg:none rejected. Algorithm confusion attacks prevented. Pre-configured JWKS per RFC 8725 §3.3. PASS.

**Phrasing-B:**
PASS. RP uses explicit algorithm allowlist (RS256/ES256 family); alg:none not included per RFC 8725 §2.1 MUST NOT. Pre-configured JWKS endpoint per RFC 8725 §3.3. OIDC id_token signature requirement satisfied. PASS.

---

## Pair[CB4-3.4-1043-A]

**Territory:** T-rfc-jwt · T-oauth-oidc
**Domain:** RS does not check revocation for JWT access tokens — revoked tokens accepted until expiry
**Expected verdict:** CONCERN

**A:**
A resource server validates JWT access tokens using signature, iss, aud, exp, and scope checks. The AS maintains a token revocation endpoint (RFC 7009) and a revocation list. The RS does not call the revocation endpoint or check any revocation list on each request. Revoked tokens remain valid at the RS until their `exp` is reached. The RS rationale: "JWTs are stateless — checking revocation on every request would require a database call, defeating the purpose of stateless tokens and adding per-request latency."

**B:**
CONCERN. RFC 7009 (Token Revocation) defines how clients and ASes interact for revocation — it does not mandate that an RS check revocation on every request for JWT access tokens. The stateless nature of JWTs is a known architectural trade-off: revocation is not instantaneous; tokens remain valid until expiry. There is no RFC MUST that requires an RS to poll a revocation list for JWT access tokens. ASVS V9.3.2 guidance recommends short access token lifetimes (typically 15 minutes to 1 hour) precisely to limit the revocation window. The gap here is that no compensating control (short TTL, introspection per RFC 7662, or revocation-list endpoint consultation) is mentioned. If tokens have short TTLs (per ASVS V9.3.1 guidance), the window is bounded. If token TTLs are long (e.g., 24 hours), the absence of revocation checking is a more significant concern. The RS accepts the stateless JWT trade-off, which is architecturally valid — but without explicit TTL information, this is a guidance gap. No RFC MUST for per-request revocation checking of JWT access tokens. CONCERN.

**Reason:** RFC 7009 does not impose MUST on RS to check JWT revocation per request. Stateless JWT trade-off is architectural; ASVS V9.3.2 compensates via short TTL guidance. No MUST for per-request revocation check. CONCERN.

**Phrasing-B:**
CONCERN. No RFC MUST requires RS to check JWT access token revocation on each request — stateless JWT design accepts this trade-off. ASVS V9.3.2 recommends short TTL to bound the revocation window. Without knowing the token TTL, this is a guidance-level gap; long TTLs (>1h) amplify the concern. CONCERN.

---

## Pair[CB4-3.4-1043-B]

**Territory:** T-rfc-jwt · T-oauth-oidc
**Domain:** Short-TTL JWT access tokens with active introspection for sensitive RS endpoints
**Expected verdict:** PASS

**A:**
The RS uses 15-minute JWT access tokens (short TTL per ASVS V9.3.1). For standard API endpoints, the RS performs stateless JWT validation (signature, iss, aud, exp, scope) without revocation checking — accepting the stateless JWT trade-off. For high-sensitivity endpoints (financial operations, admin actions), the RS performs active introspection (RFC 7662) against the AS token introspection endpoint, which returns the current revocation status. The introspection result is cached for the duration of the 15-minute token TTL to bound the per-request latency overhead.

**B:**
PASS. ASVS V9.3.1 guidance satisfied: 15-minute access token TTL (TTL = Time-To-Live) bounds the maximum revocation window for standard endpoints. RFC 7662 introspection used for high-sensitivity endpoints provides real-time revocation checking where the risk justifies the overhead. Introspection result caching is bounded to the token TTL — no stale state beyond expiry. Proportional security controls (stateless for low-risk; active introspection for high-risk) reflect appropriate risk management. No violations found.

**Reason:** ASVS V9.3.1 guidance satisfied via short TTL (15 min). RFC 7662 introspection for high-sensitivity endpoints. Revocation window bounded. Proportional controls appropriate. PASS.

**Phrasing-B:**
PASS. 15-minute JWT TTL bounds stateless revocation window per ASVS V9.3.1. High-sensitivity endpoints use RFC 7662 introspection for real-time revocation status. Proportional controls (stateless + active introspection) address revocation trade-off appropriately. PASS.

---

## Pair[CB4-3.4-1044-A]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** `client_secret` transmitted in URL query parameter — exposed in logs and browser history
**Expected verdict:** REPROCESS

**A:**
A confidential OAuth client authenticates to the token endpoint by including `client_id` and `client_secret` as URL query parameters: `POST /token?grant_type=authorization_code&code=...&client_id=myapp&client_secret=s3cr3t`. The developer rationale: "Query parameters are simple to add and work with all HTTP clients. The connection is HTTPS, so the values are encrypted in transit."

**B:**
REPROCESS. RFC 6749 §2.3.1 is explicit: "The authorization server MUST NOT accept client authentication using URI query parameters." This is an unconditional MUST NOT. The rationale for this prohibition: (1) URLs including query parameters are routinely recorded in server access logs, proxy logs, browser history, and Referer headers — placing credentials in the URL exposes them to all log consumers; (2) even over HTTPS, the URL is visible to the TLS termination point (e.g., load balancer, reverse proxy) in plaintext before encryption is applied at that layer; (3) URL length limits and URL encoding behaviors can cause unexpected exposure or truncation. The correct methods are HTTP Basic authentication with Base64-encoded `client_id:client_secret` in the Authorization header (§2.3.1), or as `client_id` and `client_secret` POST body parameters (§2.3.1). "HTTPS encrypts it" does not override the MUST NOT — the RFC prohibition is specifically about the log/history exposure risk, not transport encryption.

**Reason:** RFC 6749 §2.3.1 MUST NOT: client authentication must not use URI query parameters. Query params logged in access logs, proxy logs, browser history — credential exposure independent of TLS. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §2.3.1 MUST NOT: AS must not accept client credentials in URI query parameters. HTTPS transit encryption irrelevant — query params persist in server logs, proxy logs, and browser history. "HTTPS encrypts it" argument rejected. Use Authorization header or POST body per §2.3.1. REPROCESS.

---

## Pair[CB4-3.4-1044-B]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** `client_secret` transmitted in HTTP Basic Authorization header — not in URL query parameters
**Expected verdict:** PASS

**A:**
The confidential OAuth client authenticates to the token endpoint using HTTP Basic authentication: the `client_id` and `client_secret` are concatenated as `client_id:client_secret`, Base64-encoded, and transmitted in the `Authorization: Basic <credentials>` request header. The token endpoint rejects any request that includes `client_id` or `client_secret` as URL query parameters, returning `error=invalid_request`. Client library documentation explicitly documents that POST body parameter authentication is also accepted per RFC 6749 §2.3.1, but URL query parameter authentication is not accepted under any circumstances.

**B:**
PASS. RFC 6749 §2.3.1 MUST NOT satisfied: client credentials not transmitted in URI query parameters. HTTP Basic authentication via Authorization header is the preferred method per §2.3.1. Explicit rejection of query-parameter credentials prevents accidental misconfiguration. No violations found.

**Reason:** RFC 6749 §2.3.1 MUST NOT satisfied. Credentials in Authorization header; URI query parameter authentication explicitly rejected. PASS.

**Phrasing-B:**
PASS. Client credentials transmitted via Authorization: Basic header per RFC 6749 §2.3.1. URI query parameter credential submission rejected at token endpoint. MUST NOT requirement satisfied. PASS.
