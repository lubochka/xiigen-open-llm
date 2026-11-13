# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext30
# Sequences: CB4-3.4-777 through CB4-3.4-784
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: token response caching, client credentials scope minimization, OIDC nonce in ID token, ROPC prohibition)
# Running total after this file: 1568 instances (784 base pairs)

---

## Pair[CB4-3.4-777]

### A
Token endpoint response missing Cache-Control: no-store header (RFC 6749 §5.1 MUST):

An authorization server's token endpoint returns access tokens and refresh tokens without including Cache-Control: no-store in the HTTP response headers. The response includes Content-Type: application/json and a JSON body with access_token, token_type, expires_in, and optionally refresh_token. No caching headers are set. Team: "Token responses have short-lived access tokens — even if cached, they expire quickly. We don't set Cache-Control because our API gateway handles response header normalization."

### B
REPROCESS. RFC 6749 §5.1 states: "The authorization server MUST include the HTTP Cache-Control response header field with a value of no-store in any response containing tokens, secrets, or other sensitive information." This is an unconditional MUST applied directly to the token endpoint response.

The team's two arguments are rejected:

"Short-lived tokens minimize caching risk" — accepted as a mitigating factor for access tokens but does not address the requirement. Refresh tokens are long-lived (hours to days in most deployments) and are included in the same response. A cached token response in a shared proxy, CDN, or browser cache exposes long-lived refresh tokens to any party that can read the cache. RFC 6749 §5.1 MUST is not conditional on token lifetime.

"API gateway handles headers" — the API gateway is not the authorization server's token endpoint. RFC 6749 §5.1 requires the token endpoint itself to include the header, not a downstream component. If the gateway strips or normalizes headers, the requirement is at the origin (the AS token endpoint response), and the AS is responsible for ensuring its responses are correct before gateway processing.

RFC 6749 §5.1 MUST Cache-Control: no-store: violated. REPROCESS.

### Reason
RFC 6749 §5.1 MUST is unconditional on token responses. Short token lifetime argument is partially correct but does not satisfy the MUST. API gateway argument displaces the responsibility from the origin where the requirement applies.

### Phrasing-B
REPROCESS — RFC 6749 §5.1 MUST Cache-Control: no-store absent from token endpoint response.

Token endpoint returns access_token and refresh_token values in HTTP responses without Cache-Control: no-store. RFC 6749 §5.1 MUST: the AS MUST include Cache-Control: no-store in responses containing tokens. No-store ensures that no intermediate cache (shared proxy, CDN, browser HTTP cache) retains the token response. Refresh token caching particularly dangerous — long-lived credential available from cached response. Short-token-lifetime argument does not apply to refresh tokens and does not modify the MUST requirement. API gateway header normalization is not a substitute for origin compliance. REPROCESS.

---

## Pair[CB4-3.4-778]

### A
Token endpoint response correctly includes Cache-Control: no-store and Pragma: no-cache:

The authorization server's token endpoint includes the following headers in every response containing tokens: Cache-Control: no-store; Pragma: no-cache. The response body contains the token fields. Client libraries and resource servers can rely on these headers to prevent any HTTP caching layer from storing token responses.

### B
PASS. RFC 6749 §5.1 MUST Cache-Control: no-store satisfied.

Cache-Control: no-store instructs all HTTP caches (shared proxies, CDN edges, browser caches) not to store the response. Pragma: no-cache is included for compatibility with HTTP/1.0 caches that may not honor Cache-Control. Both headers are present in every token endpoint response.

The combination prevents token values (access tokens, refresh tokens, authorization codes in response bodies) from being retained by any caching layer. No party other than the original requester can retrieve the token response from a cache. RFC 6749 §5.1 requirements satisfied. No findings.

### Reason
RFC 6749 §5.1 MUST satisfied. Both Cache-Control: no-store and Pragma: no-cache present on every token response.

### Phrasing-B
PASS — RFC 6749 §5.1 Cache-Control: no-store and Pragma: no-cache present on token responses.

Token endpoint sends Cache-Control: no-store and Pragma: no-cache on every response containing token material. HTTP caching layers prohibited from retaining response. Access tokens and refresh tokens not cacheable by intermediaries. RFC 6749 §5.1 MUST satisfied. PASS.

---

## Pair[CB4-3.4-779]

### A
Client credentials service account registered with over-broad scope (admin:all):

A background service that generates monthly user activity reports is registered as a confidential OAuth client with client_credentials grant type. The registered scope is admin:all — a scope that grants full administrative access across all API resources including user management, billing modification, and system configuration. The service actually uses only the read:user-activity endpoint. The team explains: "We set admin:all during initial setup to avoid scope configuration overhead. It's an internal service and we trust it."

### B
REPROCESS. The service account is registered with scope admin:all — dramatically beyond the read:user-activity access it actually requires. This violates the principle of least privilege (CWE-269, ASVS V8.1.2) and constitutes a privilege escalation risk.

The "internal service, trusted" argument is rejected. Internal services are the primary targets of lateral movement attacks: compromised service accounts, supply chain attacks on service dependencies, misconfigured service-to-service routing, or stolen credentials all lead to use of the service account's full registered scope. A service account with admin:all access represents a single point of compromise that grants full administrative control — billing, user accounts, system configuration — to any attacker who obtains its client_secret.

CWE-269: The grant of admin:all to a service that requires only read:user-activity is an improper privilege assignment. The scope was not assigned based on actual need but on operational convenience ("avoid configuration overhead"). This convenience-driven over-privilege is the canonical CWE-269 scenario.

ASVS V8.1.2 (or equivalent least-privilege requirement): services must operate with only the minimum access required for their function. A reporting service requiring read:user-activity must be registered with scope=read:user-activity only. Admin:all is not the minimum required scope by any definition.

The correct remediation: re-register the service with scope=read:user-activity; verify the service operates correctly; remove admin:all from the registration. REPROCESS.

### Reason
CWE-269 privilege over-assignment + ASVS V8.1.2 least-privilege violation. admin:all for a read-only reporting service is not minimum access. "Internal and trusted" does not modify least-privilege obligations — internal compromise is a real threat vector.

### Phrasing-B
REPROCESS — CWE-269 and least-privilege violation. Service registered with admin:all; actual need is read:user-activity only.

A reporting service with admin:all scope can modify billing, delete users, and reconfigure the system — none of which it needs. CWE-269: privilege assignment not based on actual need. ASVS V8.1.2: minimum access required.

"Internal trust" rejected: compromised service accounts (credential theft, supply chain attack on dependencies, misconfigured routing) are a routine lateral movement vector in real-world incidents. admin:all makes this service a high-value target whose compromise gives an attacker the most powerful access in the system. Scope must match actual need. REPROCESS.

---

## Pair[CB4-3.4-780]

### A
Client credentials service account with minimal scope matching actual service need:

The same reporting service is re-registered with scope=read:user-activity only. The service authenticates with client_credentials, receives a token with scope=read:user-activity, and makes requests only to GET /api/user-activity/monthly endpoints. Any attempt by the service to call endpoints requiring other scopes (billing, user management, admin operations) returns 403 because the token scope does not include those values. Token TTL: 60 minutes (reporting service runs hourly jobs).

### B
PASS. Least-privilege service account configuration correctly implemented.

Scope registration: read:user-activity only — exactly the permission set required for the service's function. No additional scopes present. CWE-269 privilege over-assignment eliminated. ASVS V8.1.2 minimum access requirement satisfied.

Scope enforcement: resource server returns 403 for any request with insufficient scope. The service cannot access billing, user management, or admin endpoints even if a misconfiguration or attack attempts to use those paths — the token itself constrains the service's access regardless of code behavior.

TTL: 60-minute TTL appropriate for a service making hourly batch requests — tokens are refreshed (via new client_credentials exchange) at each job invocation. No long-lived refresh tokens required for client_credentials (the flow is designed for machine-to-machine without user interaction and tokens are obtained fresh per invocation or as needed). No findings.

### Reason
Minimal scope satisfies CWE-269 and ASVS V8.1.2. Scope enforcement at RS provides defense-in-depth. PASS.

### Phrasing-B
PASS — minimal scope registration and enforcement for client credentials service account.

Scope: read:user-activity only. Matches actual service need exactly. CWE-269 eliminated. ASVS V8.1.2 satisfied.
RS enforcement: 403 on scope-insufficient requests. Token constrains service access regardless of code. 60-minute TTL appropriate for hourly batch jobs. PASS.

---

## Pair[CB4-3.4-781]

### A
ID token issued without nonce claim when nonce was provided in authorization request (OIDC Core §3.1.3.7.11):

An OIDC client includes nonce=a7b3c9d2f0e1 in its authorization request. The authorization server processes the request and issues an ID token. The ID token contains iss, sub, aud, exp, iat, and at_hash claims but no nonce claim. The client's ID token validation checks signature, iss, aud, and exp but cannot validate nonce (absent from token). Team: "We validated all the standard claims. The nonce is an optional claim per the OIDC spec."

### B
REPROCESS. OIDC Core §3.1.3.7.11 states: "If present in the Authentication Request, Authorization Servers MUST include a nonce Claim in the ID Token with the Claim Value being the nonce value sent in the Authentication Request." The nonce was present in the authentication request; therefore the AS MUST include the nonce in the ID token. This is an unconditional MUST triggered by the presence of nonce in the request.

The team's characterization of nonce as "optional" is partially correct for the case where the client did not provide a nonce. When the client provides a nonce, it becomes a MUST requirement for the AS to echo it in the ID token. The team's argument conflates "nonce is an optional feature" (the client may or may not include it in requests) with "nonce is always optional in ID tokens" (false when nonce was present in the request).

The security consequence of missing nonce: the nonce is the token-level replay attack binding. Without the nonce in the ID token, the client cannot verify that this specific ID token was issued in response to this specific authorization request. An attacker who can replay a previously obtained ID token from the same client-issuer pair (captured from a prior session, obtained from a compromised log, or injected via a confused client library) cannot be detected without nonce validation.

OIDC Core §3.1.3.7.11 MUST: violated. REPROCESS.

### Reason
OIDC Core §3.1.3.7.11 MUST is triggered by the presence of nonce in the authorization request. The AS's failure to echo nonce in the ID token is a MUST violation. "Optional claim" argument misapplies the optionality to the wrong condition.

### Phrasing-B
REPROCESS — OIDC Core §3.1.3.7.11 MUST violated. nonce present in authorization request but absent from ID token.

The client's authorization request included nonce=a7b3c9d2f0e1. OIDC Core §3.1.3.7.11: AS MUST include nonce claim in the ID token when nonce was in the authorization request. ID token lacks nonce. The "optional claim" argument applies to the case where the client did not include a nonce — once present in the request, the AS's inclusion in the ID token is a MUST. Without nonce in the ID token, the client cannot perform nonce-match validation, eliminating the token-level replay binding. REPROCESS.

---

## Pair[CB4-3.4-782]

### A
ID token correctly includes nonce matching the authorization request value; client validates match:

The same OIDC client includes nonce=a7b3c9d2f0e1 in the authorization request. The AS issues an ID token that includes nonce: "a7b3c9d2f0e1". The client's validation logic: (1) reads the nonce from the ID token; (2) compares it to the nonce stored in the session at the time of the authorization request; (3) rejects the ID token if the nonce is absent or does not match; (4) marks the nonce as used to prevent replay of the same nonce value.

### B
PASS. OIDC Core §3.1.3.7.11 MUST satisfied. Nonce present in ID token and client validation correct.

AS behavior: nonce echoed in ID token with the same value as the authorization request. OIDC Core §3.1.3.7.11 MUST satisfied. The AS has correctly bound the ID token to the specific authorization request that generated it.

Client validation: (1) nonce presence check; (2) value match against session-stored nonce; (3) rejection on mismatch or absence; (4) single-use enforcement (nonce marked as used). Complete nonce lifecycle — generation, echoing, comparison, and invalidation — correctly implemented. Token-level replay attack binding is active. An ID token from a different session or a replayed token from a prior session would produce a nonce mismatch and be rejected. No findings.

### Reason
OIDC Core §3.1.3.7.11 satisfied by AS echo. Client validates nonce match and single-use. Replay binding complete.

### Phrasing-B
PASS — OIDC Core §3.1.3.7.11 satisfied. nonce in ID token matches authorization request; client validates and invalidates.

AS echoes nonce="a7b3c9d2f0e1" in ID token matching authorization request value. §3.1.3.7.11 MUST satisfied.
Client: checks nonce present → compares to session-stored value → rejects on mismatch → marks as used. Token replay binding active. Any replay produces nonce mismatch rejection. PASS.

---

## Pair[CB4-3.4-783]

### A
Resource Owner Password Credentials (ROPC) grant type enabled for user authentication:

An application allows users to authenticate by posting credentials directly to the authorization server's token endpoint: POST /token with grant_type=password, username, and password fields. The AS validates the credentials and returns an access token and refresh token directly, without any browser redirect. The application team argues: "ROPC is simpler for our users — no redirect pages. RFC 6749 explicitly defines the password grant type as a supported grant. Our users prefer entering credentials directly in our app's UI."

### B
REPROCESS. RFC 9700 §2.4 (OAuth 2.0 Security Best Current Practice) states: "The resource owner password credentials grant MUST NOT be used." This is a MUST NOT in the published BCP for OAuth 2.0 security. RFC 9700 supersedes the original permission for ROPC that appeared in RFC 6749.

The team's arguments are rejected:

"RFC 6749 defines ROPC" — correct as a historical statement. RFC 9700, published as BCP 240, explicitly supersedes the permissive treatment of ROPC in RFC 6749. The security community's collective experience since 2012 established that ROPC creates unacceptable security risks that outweigh its convenience: (a) the application receives the user's credentials directly, requiring the user to trust the application as much as the AS itself — eliminating the trust boundary that OAuth 2.0 was designed to provide; (b) phishing applications can trivially implement ROPC-compatible flows that send credentials to attacker infrastructure while appearing identical to legitimate apps; (c) the authorization server cannot enforce multi-factor authentication (MFA) or adaptive authentication challenges in the ROPC flow because the credentials arrive pre-validated by the application.

"Users prefer direct credential entry" — the UX preference argument does not modify MUST NOT. Simpler credential collection by the application is precisely the pattern OAuth 2.0 was designed to eliminate — it re-centralizes credential handling in client applications.

RFC 9700 §2.4 MUST NOT: violated. REPROCESS.

### Reason
RFC 9700 §2.4 MUST NOT is current BCP for OAuth 2.0. It supersedes RFC 6749's original ROPC definition. The convenience arguments do not address the security rationale for MUST NOT: elimination of the authorization server trust boundary, phishing risk, and MFA bypass.

### Phrasing-B
REPROCESS — RFC 9700 §2.4 MUST NOT ROPC violated. RFC 6749 permission superseded by current BCP.

RFC 9700 §2.4 (BCP 240): ROPC MUST NOT be used. This is the current normative requirement for OAuth 2.0 security; it supersedes RFC 6749's original definition of the grant type.

ROPC security failures: (1) application receives user credentials directly — trust boundary eliminated; (2) phishing applications implement identical flows; (3) AS cannot enforce MFA in ROPC flow — MFA enforcement requires AS-controlled authentication UI. User preference for "direct credential entry in our app" is exactly what OAuth 2.0 was designed to prevent by directing authentication to the AS. Correct remediation: implement authorization code flow with PKCE; user authenticates at AS, not in the application. REPROCESS.

---

## Pair[CB4-3.4-784]

### A
ROPC disabled; all user authentication through authorization code flow with PKCE:

The authorization server has no password grant type registered or enabled. All user-facing flows use authorization_code with PKCE S256. The application redirects users to the AS's authorization endpoint, the user authenticates at the AS (which can enforce MFA, adaptive authentication, and session policies), receives an authorization code, and the code is exchanged at the token endpoint with the code_verifier. The team removed ROPC after reviewing RFC 9700.

### B
PASS. ROPC is disabled. All user authentication flows through the authorization code flow with PKCE.

ROPC absence: the password grant type is not registered or handled by the token endpoint. RFC 9700 §2.4 MUST NOT: satisfied by disabling the grant type. Phishing mitigation active — application cannot collect user credentials; user authenticates at the AS's controlled UI. AS can enforce MFA, account lockout, and adaptive authentication challenges independently of the application.

Authorization code + PKCE S256: the correct replacement for ROPC. User experience: authentication at the AS's UI with redirect back to the application. Code exchange with code_verifier provides code interception protection. RFC 9700-compliant flow. The team's proactive review of RFC 9700 and removal of ROPC demonstrates the process improvement that BCP adoption is designed to produce. No findings.

### Reason
ROPC disabled. RFC 9700 §2.4 MUST NOT satisfied. Authorization code + PKCE S256 is the BCP-compliant user authentication flow.

### Phrasing-B
PASS — ROPC disabled; authorization code + PKCE S256 replaces it per RFC 9700 §2.4.

ROPC: not registered; password grant type not handled by token endpoint. RFC 9700 §2.4 MUST NOT satisfied.
Authorization code + PKCE S256: user redirected to AS authorization endpoint; AS enforces authentication (including MFA if configured); code exchanged with code_verifier. Trust boundary between application and AS maintained — application never handles user credentials. RFC 9700-compliant. PASS.
