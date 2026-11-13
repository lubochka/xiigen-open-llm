# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext29
# Sequences: CB4-3.4-751 through CB4-3.4-758
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: RFC 9207 iss response, scope elevation at refresh, credential logging, PKCE default-plain, exp claim absent)
# Running total after this file: 1516 instances (758 base pairs)

---

## Pair[CB4-3.4-751]

### A
Authorization response does not include the iss parameter (RFC 9207 AS issuer identification absent):

An authorization server does not include the iss parameter in its authorization code redirect responses. The authorization endpoint returns: GET /callback?code=abc123&state=xyz789 — no iss parameter. The team states: "We have a single authorization server; there's no mix-up scenario. The iss parameter is an optional response parameter in RFC 9207." The team correctly identifies RFC 9207's SHOULD-level language.

### B
CONCERN. RFC 9207 §2 states that authorization servers SHOULD include the iss parameter in authorization responses to enable clients to detect AS mix-up attacks. The team's characterization of the SHOULD-level language is accurate; this is not a MUST violation.

The iss parameter enables clients to verify that the authorization response they received originated from the AS they intended to contact. The mix-up attack vector: an attacker who can redirect a user through a malicious authorization server (MITM, phishing, misconfigured client) can substitute an authorization code or token from a different AS; without iss validation, the legitimate client cannot detect the substitution. With a single AS deployment, the risk is reduced — but not eliminated. A client configured to connect to as.example.com that receives an authorization response could encounter a mix-up if the AS is ever changed, a second AS is added, or if a client library is misconfigured to support multiple issuers.

RFC 9207 §2 SHOULD: not satisfied. The SHOULD-level gap is a real defense-in-depth omission. CONCERN.

### Reason
RFC 9207 §2 is SHOULD not MUST. The team correctly identifies this. Single-AS deployment reduces but does not eliminate the threat. CONCERN.

### Phrasing-B
CONCERN — RFC 9207 §2 SHOULD not satisfied. iss parameter absent from authorization responses.

RFC 9207 defines the iss parameter as a defense against AS mix-up attacks, where an attacker causes a client to send an authorization code from one AS to another AS's token endpoint. The iss parameter in the authorization response allows the client to confirm the code came from the expected AS before proceeding. SHOULD-level: absence is a defense-in-depth gap, not a MUST violation. Single-AS argument reduces the risk but does not satisfy the SHOULD recommendation. CONCERN.

---

## Pair[CB4-3.4-752]

### A
Authorization response correctly includes the iss parameter; client validates it:

Authorization server includes iss="https://as.example.com" in every authorization code response: GET /callback?code=abc123&state=xyz789&iss=https%3A%2F%2Fas.example.com. The client library validates: (1) iss is present; (2) iss matches the expected AS issuer the client was configured to use; (3) if iss is absent or mismatched, the client rejects the authorization response. State parameter also validated as single-use and session-bound.

### B
PASS. RFC 9207 §2 SHOULD satisfied. iss parameter present and validated by the client.

The authorization response includes the iss parameter identifying the AS that issued the code. Client-side iss validation ensures: any authorization response received from a mismatched or unexpected AS is rejected before the code is presented to the token endpoint. AS mix-up attack vector is blocked — a code from a different AS would carry a different iss value and be rejected. The defense is active even in single-AS deployments as a guard against future multi-AS configuration or client misconfiguration. RFC 9207 recommendation implemented. No findings.

### Reason
RFC 9207 §2 SHOULD satisfied with iss present and validated by the client. Mix-up attack vector blocked.

### Phrasing-B
PASS — RFC 9207 iss parameter present in authorization response; client validates against configured issuer.

iss="https://as.example.com" in every authorization response. Client rejects any response where iss is absent or does not match the configured issuer. State parameter single-use and session-bound. Mix-up defense active. RFC 9207 §2 SHOULD satisfied. PASS.

---

## Pair[CB4-3.4-753]

### A
Scope elevation at refresh token exchange — authorization server honors scope widening request:

A user originally granted scope=read:profile read:data to a client application. The application stores the refresh token. On the next refresh exchange, the client sends: grant_type=refresh_token, refresh_token=<token>, scope=read:profile read:data write:data admin:content. The authorization server processes the request and issues a new access token with scope=read:profile read:data write:data admin:content — honoring the elevated scope request. Team: "The user authenticated successfully; scope expansion at refresh is a supported pattern that allows apps to incrementally request permissions."

### B
REPROCESS. RFC 6749 §6 states: "The authorization server MAY issue a new access token with a different scope, provided that the scope is within the scope originally granted by the resource owner." The authorization server issued tokens with write:data and admin:content — scopes that were never granted by the resource owner in the original authorization. This is an unauthorized scope expansion at the token endpoint.

The "incremental permission" argument is rejected. Incremental permission acquisition in OAuth requires a new authorization request through the authorization endpoint — the resource owner must have the opportunity to consent to the new scopes. The refresh token exchange is a server-to-server credential flow that does not involve the resource owner. No consent for write:data or admin:content was ever obtained. Issuing tokens with those scopes from a refresh request grants capabilities that the user never authorized.

RFC 6749 §6 interpretation: the AS MAY issue tokens with a narrower scope than originally granted (the client can downscope). The AS must NOT issue tokens with scopes beyond the original grant. The phrase "within the scope originally granted" sets a ceiling, not a floor. The AS violated that ceiling by issuing write:data and admin:content without any authorization grant from the resource owner.

Practical impact: the application effectively escalates its own privilege level through a refresh request, bypassing the authorization code flow where user consent would be required. REPROCESS.

### Reason
RFC 6749 §6 is clear: refresh exchange scope must be within the original grant. The AS issued scopes (write:data, admin:content) never granted by the resource owner. Incremental permission requires a new authorization request, not a refresh exchange.

### Phrasing-B
REPROCESS — RFC 6749 §6 violated. Authorization server honored scope expansion beyond original grant.

RFC 6749 §6: refresh token exchange scope is bounded by the original resource owner grant. Original grant: read:profile read:data. Refresh request: read:profile read:data write:data admin:content. Issued token: includes write:data and admin:content — scopes never consented to. Scope ceiling (original grant) violated.

Incremental permission argument rejected: obtaining new scopes requires presenting the user with a new authorization request at the authorization endpoint. The refresh exchange runs server-to-server without user interaction — no mechanism for consent to new scopes exists in this flow. The AS must reject scope values not within the original grant and either honor only the intersection with the original grant or return 400 (invalid_scope). REPROCESS.

---

## Pair[CB4-3.4-754]

### A
Refresh token exchange correctly bounded to original scope:

A client with original scope=read:profile read:data sends a refresh request with the same scope values. The AS validates: the requested scope set is a subset of (or equal to) the original grant scope. The AS issues a new access token with scope=read:profile read:data. In a separate test, the client sends a refresh request with scope=read:profile only (downscoped). The AS issues a token with scope=read:profile — correctly honoring a narrower scope than the original grant.

### B
PASS. RFC 6749 §6 scope constraint correctly enforced in both directions.

Same-scope refresh: the client requested the original scope; the AS validated the request is within the original grant and issued accordingly. No scope expansion.

Downscoped refresh: the client requested a subset of the original grant (read:profile only). RFC 6749 §6 permits the AS to issue a narrower scope. The AS correctly honored the narrower scope, demonstrating that the scope boundary is validated (ceiling enforced) while legitimate downscoping is supported.

Both behaviors conform to RFC 6749 §6. No findings.

### Reason
RFC 6749 §6 scope ceiling enforced in both the same-scope and downscope cases. No expansion beyond original grant.

### Phrasing-B
PASS — refresh token scope correctly bounded to original grant. Downscoping also correctly supported.

Original grant: read:profile read:data. Same-scope refresh request honored within that bound. Downscoped refresh (read:profile only) honored as a valid narrowing. AS validates scope ceiling on every refresh request. RFC 6749 §6 satisfied — scope within original grant in all cases. PASS.

---

## Pair[CB4-3.4-755]

### A
Token endpoint logs full request body including client_secret in production:

The authorization server's token endpoint is configured with DEBUG-level request logging enabled in the production environment. The logger captures the complete HTTP POST body for each token request, which includes client_id, client_secret, grant_type, and code or refresh_token values. Logs are shipped to a centralized ELK (Elasticsearch, Logstash, Kibana) stack accessible to the operations, development, and support teams. The team states: "DEBUG logging helps us diagnose production token exchange failures quickly. The logs are access-controlled within our organization."

### B
REPROCESS. V8.2.2 MUST NOT: sensitive credentials must not be logged. The client_secret is a credential equivalent in sensitivity to a password — it authenticates the client to the authorization server. Capturing client_secret values in production logs violates this MUST NOT requirement unconditionally.

The "access-controlled within our organization" argument is rejected for two reasons: (1) V8.2.2 is unconditional — it does not permit logging of credentials even in access-controlled log stores; (2) log aggregators, SIEM systems, and backup pipelines multiply the credential exposure surface across the organization. An ELK stack accessible to multiple teams is not equivalent to a secure credential store. Any log consumer, backup, export, or external integration that touches the ELK data gains access to all logged client_secrets.

Additionally, the token response body or code values appearing in logs (code=<authorization_code>, refresh_token=<value>) constitute bearer credential logging — also MUST NOT under V8.2.2 and V9.3.1.

The correct diagnostic pattern: log the token request at INFO level with: timestamp, client_id, grant_type, requested scope, response_code. Explicitly exclude: client_secret, client_assertion, code, tokens, refresh_token. REPROCESS.

### Reason
V8.2.2 MUST NOT log credentials is unconditional. client_secret in production logs equals credential exposure to all log consumers. "Access-controlled" does not satisfy a MUST NOT.

### Phrasing-B
REPROCESS — V8.2.2 MUST NOT violated. client_secret captured in production DEBUG logs.

Token endpoint POST body logged at DEBUG level in production. Body contains client_secret. Logs shipped to ELK accessible to operations, development, and support teams. V8.2.2 MUST NOT log sensitive credentials — client_secret is a credential. Access control on the log store does not satisfy MUST NOT; it merely limits who has access to the credential leak, not whether the leak occurs. Every log retention period, backup, export, or SIEM integration extends the exposure surface further. REPROCESS. Log only non-credential metadata (timestamp, client_id, grant_type, scope, HTTP response code).

---

## Pair[CB4-3.4-756]

### A
Token endpoint logging correctly excludes credentials:

The authorization server's token endpoint logs are configured as follows: log level INFO in production; each token request produces one log line containing: ISO timestamp, client_id (logged), grant_type (logged), requested_scope (logged), response_http_status (logged). The logging framework includes an explicit redaction configuration: request body fields client_secret, client_assertion, code, refresh_token, and access_token are all in the redaction list and produce "[REDACTED]" in log output regardless of log level. Log review confirms: no credential values appear in any production log sample.

### B
PASS. Token endpoint logging correctly excludes all credential values. V8.2.2 MUST NOT satisfied.

The logged fields (client_id, grant_type, scope, HTTP status) are non-sensitive metadata useful for diagnostics without exposing credentials. The explicit redaction list ensures that credential fields are excluded regardless of the log level configured — even if the level were changed to DEBUG, the redaction layer would prevent credential capture. V8.2.2 MUST NOT log credentials: satisfied. V9.3.1 (token value logging): satisfied by redaction of access_token and refresh_token fields. No findings.

### Reason
Credential fields explicitly redacted at the logging framework level, not at the log level setting. Diagnostic utility maintained through non-sensitive metadata. V8.2.2 satisfied.

### Phrasing-B
PASS — token endpoint logging correctly redacts all credential fields regardless of log level.

Logged: timestamp, client_id, grant_type, scope, HTTP status. Redacted (framework-level, "[REDACTED]"): client_secret, client_assertion, code, refresh_token, access_token. Framework-level redaction is level-independent — changing log level does not expose credentials. V8.2.2 MUST NOT and V9.3.1 satisfied. PASS.

---

## Pair[CB4-3.4-757]

### A
PKCE code_challenge_method absent — authorization server defaults to plain method:

A client sends an authorization request with code_challenge=<challenge_value> but omits the code_challenge_method parameter. RFC 7636 §4.3 specifies that if the method is absent, plain is the default. The authorization server follows this default — it records the code_challenge with method=plain. At token exchange, the client presents the code_verifier as-is; the server compares code_challenge == code_verifier directly (plain verification). The client is a single-page application that has S256 capability. Team: "The client didn't send the method; we're following the RFC 7636 default behavior. Plain PKCE still prevents code interception."

### B
REPROCESS. RFC 7636 §4.2 states: "If the client is capable of using S256, it MUST use S256." The client is an S256-capable SPA. By omitting the code_challenge_method parameter and allowing the server to default to plain, the client has violated §4.2 MUST.

However, the server-side failure is the more significant finding for this evaluation: the server accepted an authorization request from an S256-capable client with a plain challenge and proceeded to enforce plain verification — enabling a PKCE downgrade from S256 to plain through parameter omission rather than through explicit parameter choice. The server should not accept method-absent requests from clients that have registered or indicated S256 capability; or the server should default to rejecting method-absent challenges and requiring explicit method specification.

The team's "following the RFC 7636 default" argument is technically correct about the default behavior — §4.3 does define plain as the default for absent method. However, this does not eliminate the §4.2 MUST obligation on S256-capable clients. The combination of the §4.2 MUST violation on the client and the server's failure to reject the downgraded request = REPROCESS. The plain method provides weaker protection than S256 — the code_verifier value is transmitted directly, and while it is not equivalent to no protection, it is weaker than the hash-based protection of S256.

The correct behavior: S256-capable clients MUST include code_challenge_method=S256; servers SHOULD reject code challenges from S256-capable clients that specify or default to plain. REPROCESS.

### Reason
RFC 7636 §4.2 MUST use S256 if capable — violated by the client. The server's acceptance of the plain-defaulted challenge enables the downgrade. The PKCE downgrade-via-omission vector is equivalent in security risk to the explicit plain-method downgrade covered previously.

### Phrasing-B
REPROCESS — RFC 7636 §4.2 MUST violated. S256-capable client defaulted to plain through method omission; server accepted.

RFC 7636 §4.2: S256-capable clients MUST use S256. The SPA is S256-capable. By omitting code_challenge_method, the client allowed the server to apply the §4.3 default (plain), producing a plain-verified PKCE exchange. The "following the default" argument is accurate about §4.3 but does not satisfy §4.2 — the client MUST have included code_challenge_method=S256 explicitly. The server enabling the omission-downgrade by not requiring explicit method specification from capable clients is a defense-in-depth failure. The result: same risk profile as an explicit plain-method request from an S256-capable client — previously established as REPROCESS. REPROCESS.

---

## Pair[CB4-3.4-758]

### A
Access tokens issued without an exp (expiration) claim — effectively non-expiring:

An authorization server issues JWT access tokens that do not include an exp claim. The tokens contain iss, sub, iat, scope, and aud claims but no exp value. Resource servers validate signature, iss, aud, and scope but cannot validate expiry because no exp is present. Tokens remain valid indefinitely from signature perspective. Team: "RFC 7519 lists exp as an OPTIONAL claim. We use short-lived refresh tokens to manage token lifecycle — the access token is revoked when the refresh token expires. We implement short-lived sessions through the application layer."

### B
REPROCESS. While RFC 7519 §4.1.4 lists exp as a SHOULD-present claim in general JWTs, RFC 9068 §2.2 (JWT Profile for OAuth 2.0 Access Tokens) states that conforming access tokens MUST contain an exp claim. Additionally, ASVS V9.2.1 (or equivalent) requires that all tokens issued have a bounded lifetime. Access tokens without exp are effectively non-expiring from the resource server's perspective.

The team's multi-part argument is addressed in sequence:

"RFC 7519 says exp is OPTIONAL" — correct for general-purpose JWTs; not correct for OAuth 2.0 access tokens under RFC 9068 §2.2 MUST. Access tokens are a specific JWT profile with stricter requirements.

"Short-lived refresh tokens manage lifecycle" — rejected. The access token is the credential presented to resource servers. Resource servers validate the access token directly; they do not consult the refresh token's state. An access token without exp cannot be expired at the RS — the RS has no mechanism to enforce a time boundary on a token that claims no expiry. Revocation of the refresh token does not invalidate outstanding access tokens unless a blocklist is in place; even with a blocklist, the absence of exp means the blocklist must be checked indefinitely for tokens that will never naturally expire.

"Application-layer session management" — the application session is distinct from the access token lifetime. A user whose application session ends may still hold a valid (non-expiring) access token that can be replayed independently of the session.

RFC 9068 §2.2 MUST exp present: violated. REPROCESS.

### Reason
RFC 9068 §2.2 MUST exp is specific to OAuth 2.0 JWT access tokens. General RFC 7519 OPTIONAL language does not override the profile-specific MUST. The refresh-token-lifecycle argument addresses a different control plane than the access token's own expiry enforcement at the RS.

### Phrasing-B
REPROCESS — RFC 9068 §2.2 MUST exp claim absent from access tokens. Non-expiring access tokens cannot be bounded by resource servers.

RFC 7519 exp OPTIONAL is general-JWT guidance. OAuth 2.0 access tokens conforming to RFC 9068 §2.2 MUST include exp — the profile-specific MUST overrides the base-spec OPTIONAL for access tokens in this context.

Access tokens without exp: the RS cannot enforce a time boundary; the token is valid as long as the signature is valid (which is indefinite for RSA/EC signatures without key rotation). Refresh token expiry controls a different credential (the refresh token); it does not expire the access token at the RS. Application session management controls client-side UX; it does not prevent API calls with the non-expiring token. All three arguments address controls that are orthogonal to the access token's own exp claim. REPROCESS.
