# CB4 Stage 3 — Action 3 Extended Pair Set
# Node: Node 8 (Arbiter Units) — ext32
# Sequences: CB4-3.4-829 through CB4-3.4-836
# Instances: 16 (8 base pairs × 2 phrasings each)
# Stratum: A/B/D (new domains: JWT jti claim, authorization code lifetime, JWT sub claim, PAR HTTPS)
# Running total after this file: 1672 instances (836 base pairs)

---

## Pair[CB4-3.4-829]

### A
JWT access tokens issued without jti (JWT ID) claim (RFC 7519 §4.1.7 SHOULD):

An authorization server issues JWT access tokens without a jti claim. Tokens contain iss, sub, aud, exp, iat, and scope claims but no jti unique identifier. The team states: "We handle token lifecycle through refresh token revocation and access token TTL. We don't need jti because our 15-minute access token TTL provides sufficient revocation granularity — we don't need per-token tracking."

### B
CONCERN. RFC 7519 §4.1.7 states: "The jti (JWT ID) Claim provides a unique identifier for the JWT. The identifier value MUST be assigned in a manner that ensures that there is a negligible probability that the same value will be accidentally assigned to a different data object." The jti claim is a SHOULD-level recommendation in RFC 7519 for access tokens.

The team's TTL-based granularity argument is partially accepted but incomplete. A 15-minute TTL limits the window for which a compromised access token remains valid without active revocation. However, jti serves purposes beyond TTL-based management:

(1) Replay detection: without jti, the RS cannot detect whether the same token is being presented multiple times from different sources (a compromised token replayed by an attacker simultaneously with its legitimate holder). With jti tracking in a short-lived store, the RS can detect concurrent use anomalies.

(2) Precision revocation: if a specific token needs to be revoked before its TTL (e.g., a logout that includes outstanding access tokens, or detection of a specific token's compromise), the revocation system needs the jti to identify the specific token. Without jti, revocation must operate at the refresh-token or user level, not at the individual access-token level.

(3) Audit trail: jti enables per-token audit logging — each API request can be correlated to the specific token used.

The gap is SHOULD-level — no MUST requires jti. CONCERN.

### Reason
RFC 7519 §4.1.7 is SHOULD not MUST. jti enables replay detection, precision revocation, and audit correlation. TTL-based management is real mitigation but incomplete for per-token use cases. CONCERN.

### Phrasing-B
CONCERN — RFC 7519 §4.1.7 SHOULD jti claim absent. Precision revocation and replay detection absent.

JWT access tokens lack jti. RFC 7519 §4.1.7 SHOULD: jti provides unique per-token identifier. Without jti: (a) RS cannot detect concurrent token replay from different sources; (b) precision revocation of individual tokens before TTL requires out-of-band tracking not anchored to the token itself; (c) per-token audit correlation requires correlating other claims (iat+sub+aud, not guaranteed unique). 15-minute TTL mitigates the severity of the gap but does not eliminate the use cases for jti. SHOULD-level: CONCERN.

---

## Pair[CB4-3.4-830]

### A
JWT access tokens with jti claim and server-side precision revocation:

JWT access tokens include a jti claim (UUID v4, globally unique per token). The authorization server maintains a Redis blocklist keyed by jti with TTL equal to the token's remaining lifetime. Resource servers validate jti against the blocklist on every request. At access token revocation events (explicit revocation, logout, compromise detection), the specific jti is added to the blocklist — only the specific token is revoked, not the entire session.

### B
PASS. RFC 7519 §4.1.7 SHOULD satisfied. Per-token precision revocation and replay detection enabled.

jti claim: UUID v4 provides globally unique per-token identifier. RFC 7519 §4.1.7 SHOULD uniqueness requirement satisfied. No two tokens will share a jti in practice (UUID v4 collision probability negligible).

Precision revocation: Redis blocklist keyed by jti with TTL-matched entries. Specific compromised tokens can be revoked without terminating the entire session or all tokens for the subject. Logout can precisely revoke outstanding access tokens by jti. TTL-matched entries prevent unbounded blocklist growth.

Replay detection capability: RS blocklist check on every request enables detection of concurrent use — if a single jti appears in rapid requests from different IP addresses, anomaly detection can flag potential token theft. PASS.

### Reason
RFC 7519 §4.1.7 SHOULD satisfied with UUID v4 jti. Precision revocation via Redis JTI blocklist. TTL-matched entries. PASS.

### Phrasing-B
PASS — RFC 7519 §4.1.7 SHOULD satisfied. UUID v4 jti + Redis JTI blocklist with TTL-matched entries.

jti: UUID v4 per token; globally unique. §4.1.7 SHOULD uniqueness satisfied.
Precision revocation: jti blocklist in Redis, TTL = remaining token lifetime. Specific token revocation without session termination. Audit per-token correlation via jti. RS blocklist check enables concurrent-use anomaly detection. PASS.

---

## Pair[CB4-3.4-831]

### A
Authorization codes issued with 24-hour lifetime (RFC 6749 §4.1.2 short-lifetime requirement):

An authorization server issues authorization codes with a 24-hour expiry. The team implements single-use enforcement (codes are marked as consumed on first exchange). Team: "We set 24 hours to give users flexibility — they can start an authorization flow and return to the application the next day to complete it. Single-use enforcement means only one exchange is allowed regardless of how long the code is valid."

### B
REPROCESS. RFC 6749 §4.1.2 states: "The authorization code MUST expire shortly after it is issued to mitigate the risk of leaks." A 24-hour lifetime does not constitute "shortly" by any reasonable interpretation of the temporal scope of that requirement.

The single-use argument is rejected as a complete substitute for short lifetime. Single-use enforcement reduces the risk of multiple-use replay attacks but does not address the interception-then-use threat that the short-lifetime requirement is designed to mitigate. A 24-hour window means:

(1) If an authorization code is intercepted (from a browser referrer header, URL in an error page, application log, phishing redirect, or MITM on HTTP redirect), the attacker has a full day to attempt to use the code before it expires.

(2) Single-use enforcement relies on the legitimate user NOT completing the exchange first — if the attacker uses the code first, the legitimate user's exchange attempt will fail. But the attacker may hold the code without using it (to study the target application) until they have a complete attack plan. A 24-hour window provides ample time.

(3) The "user flexibility" use case — returning the next day — is a UX preference that does not satisfy the security requirement. OAuth 2.0 authorization flows are designed to be completed within a single session; returning the next day should initiate a new authorization request.

RFC 6749 §4.1.2 MUST expire shortly: violated. REPROCESS.

### Reason
RFC 6749 §4.1.2 MUST short lifetime is a separate requirement from single-use. 24 hours is not "shortly." The single-use argument addresses a different threat (multiple-use replay) but not the interception-and-use-within-window threat.

### Phrasing-B
REPROCESS — RFC 6749 §4.1.2 MUST expire shortly violated. 24-hour authorization code lifetime is not "shortly."

RFC 6749 §4.1.2: authorization code MUST expire shortly after issuance. "Shortly" is universally interpreted as minutes (the spec references 10 minutes as a reference point; ext16 confirmed 10-minute single-use codes as PASS). 24 hours provides an extended interception-and-use window — an attacker who intercepts the code (browser log, referrer, error page, MITM) has a full day to execute their attack.

Single-use argument: addresses replay prevention, not interception-and-use. If the attacker uses the code first, single-use fails the legitimate user instead. Short lifetime limits the useful window after interception regardless of who uses it first. Both controls are required; one does not substitute for the other. REPROCESS.

---

## Pair[CB4-3.4-832]

### A
Authorization codes issued with 60-second lifetime; single-use enforced:

Authorization codes expire after 60 seconds. Single-use enforcement marks codes as consumed on first exchange; replay attempts return 400 invalid_grant; replay also triggers revocation of all tokens in the associated authorization session.

### B
PASS. RFC 6749 §4.1.2 short-lifetime requirement satisfied. Single-use enforcement with replay-triggered revocation also present.

60-second lifetime: well within "shortly" — the code window is too small to be useful for interception-and-use attacks. An attacker who intercepts a code has approximately 60 seconds to construct and execute an exchange request, which requires knowledge of the redirect URI, client_id, and (for PKCE flows) the code_verifier. In practice, this window is not exploitable for non-automated attacks.

Single-use enforcement: first exchange consumes the code; subsequent attempts return 400. Replay-triggered revocation: if replay is detected, all tokens associated with the session are revoked — even if the attacker exchanged first, the legitimate user's subsequent attempt triggers revocation, invalidating the attacker's tokens. RFC 6749 §4.1.2 dual requirements satisfied (short lifetime + single-use with revocation). No findings.

### Reason
60-second lifetime satisfies RFC 6749 §4.1.2 "shortly." Single-use + revocation-on-replay satisfies the second §4.1.2 condition. Both requirements met.

### Phrasing-B
PASS — 60-second authorization code lifetime + single-use + replay-triggered revocation.

60 seconds: "shortly" satisfied. Interception-and-use window: 60 seconds insufficient for manual attacks; automated attacks blocked by PKCE code_verifier requirement (known only to legitimate client).
Single-use + replay revocation: RFC 6749 §4.1.2 both conditions satisfied. PASS.

---

## Pair[CB4-3.4-833]

### A
JWT access tokens issued without sub (subject) claim (RFC 9068 §2.2 MUST sub present):

An authorization server issues JWT access tokens for user-initiated flows without a sub claim. Tokens contain iss, aud, exp, iat, jti, and scope but no sub. Resource servers receive these tokens and validate signature, iss, aud, exp, and scope but cannot determine which user's authorization the token represents. Team: "The sub claim is a user identifier and we prefer not to expose user identifiers in tokens for privacy reasons."

### B
REPROCESS. RFC 9068 §2.2 (JWT Profile for OAuth 2.0 Access Tokens) states: "The JWT access token MUST contain a sub claim." Sub identifies the principal on whose behalf the access was granted. Without sub, the RS cannot:

(1) Apply per-user authorization rules (rate limiting per user, per-user data access restrictions, per-user feature gating).

(2) Implement user-level revocation — revoking all tokens for a specific compromised user account requires identifying which tokens belong to that user; without sub, this is not possible from the token alone.

(3) Produce meaningful audit logs — RS access logs tied to tokens without sub cannot answer "which user accessed this resource."

The privacy argument is rejected as a rationale for omitting a MUST claim. The sub value in RFC 9068 tokens is a PPID (Pairwise Pseudonymous Identifier) or opaque identifier — it does not need to be a human-readable username or email. ASVS V10 and OIDC support opaque or pairwise-pseudonymous sub values that protect user privacy while satisfying the MUST requirement. Omitting sub entirely is not the appropriate privacy control.

RFC 9068 §2.2 MUST sub: violated. REPROCESS.

### Reason
RFC 9068 §2.2 MUST sub is unconditional for JWT access tokens. Privacy concern is valid but is addressable through PPID/opaque sub values — not through sub omission. Without sub, per-user authorization and revocation at the RS are non-functional.

### Phrasing-B
REPROCESS — RFC 9068 §2.2 MUST sub claim absent. Privacy argument does not satisfy MUST.

RFC 9068 §2.2: sub MUST be present in JWT access tokens. Sub identifies the principal for the token's authorization. Without sub: RS cannot apply per-user policies, cannot support user-level revocation, cannot produce user-correlated audit logs.

Privacy argument: valid concern, wrong remediation. Sub does not need to expose PII — PPID (pairwise pseudonymous sub) and opaque identifiers are supported by RFC 9068 and OIDC and protect user privacy without omitting the MUST claim. Sub omission is not a compliant privacy control. REPROCESS.

---

## Pair[CB4-3.4-834]

### A
JWT access tokens with opaque sub claim; RS validates sub against expected principal:

JWT access tokens include sub: "usr-9f3a7b2c-d411-4e8c-8902-3a5d6e7f8b1c" — a UUID v4-based opaque identifier that does not expose user PII. The RS validates the sub claim matches the expected principal format (UUID pattern) and logs the sub value for audit purposes. Per-user rate limiting is applied by the RS using the sub value. User-level revocation is supported: all tokens for a specific sub can be added to the blocklist.

### B
PASS. RFC 9068 §2.2 MUST sub satisfied. Opaque sub value addresses privacy concern without omitting the required claim.

Sub claim: UUID v4-based PPID satisfies RFC 9068 §2.2 MUST sub while protecting user privacy — no PII (username, email, real name) is included in the sub value. RS validates sub format as expected UUID pattern. Per-user authorization controls (rate limiting, data access restrictions) are functional. User-level revocation is supported by sub. Audit logs correlate requests to sub values for traceability. Privacy requirement satisfied through opaque sub rather than sub omission. No findings.

### Reason
Opaque sub satisfies RFC 9068 §2.2 MUST and addresses privacy without exposing PII. Per-user RS controls functional.

### Phrasing-B
PASS — RFC 9068 §2.2 MUST sub satisfied via opaque UUID-based PPID. Privacy maintained without omitting MUST claim.

sub = UUID v4-based PPID. No PII in sub. RFC 9068 §2.2 MUST satisfied. RS validates sub format. Per-user rate limiting, user-level revocation, and audit correlation all functional. Privacy + compliance simultaneously achieved via opaque identifier. PASS.

---

## Pair[CB4-3.4-835]

### A
Pushed Authorization Request (PAR) endpoint served over plain HTTP (RFC 9126 §3 MUST TLS):

An authorization server's Pushed Authorization Request (PAR) endpoint is accessible at http://as.example.com/par — plain HTTP without TLS. Clients POST authorization parameters (client_id, response_type, redirect_uri, scope, code_challenge, state) to the endpoint and receive a request_uri. The team argues: "The PAR endpoint is a server-to-server call from our application backend — it never goes over the public internet. We have TLS on the authorization endpoint itself."

### B
REPROCESS. RFC 9126 §3 states: "The authorization server MUST require the use of TLS when sending requests to the pushed authorization request endpoint." This is an unconditional MUST.

The "server-to-server, never public internet" argument is rejected for the same category of reason as HTTP JWKS ("internal network is trusted"): internal networks are not immune to on-path attacks. VLAN hopping, compromised internal hosts, misconfigured network segments, and insider threats all create on-path attack opportunities within "internal" networks. An adversary who can intercept PAR requests receives the full authorization request parameters before they are committed to the AS — including code_challenge values, redirect URIs, scopes requested, and any custom request parameters.

More critically, the PAR request may include the client's authorization parameters in a way that allows an on-path attacker to substitute parameters before they are registered. If the client sends code_challenge to the PAR endpoint and the attacker replaces it with a known code_challenge (from a code_verifier the attacker knows), the legitimate client will fail to exchange the code while the attacker's crafted verifier succeeds.

The TLS-on-authorization-endpoint-only argument does not satisfy the PAR endpoint's MUST TLS requirement — each endpoint has its own transport security requirement.

RFC 9126 §3 MUST TLS: violated. REPROCESS.

### Reason
RFC 9126 §3 MUST TLS is unconditional and endpoint-specific. "Server-to-server over internal network" is rejected (same category as JWKS HTTP). Authorization parameters in transit are as sensitive as tokens themselves.

### Phrasing-B
REPROCESS — RFC 9126 §3 MUST TLS violated on PAR endpoint. Internal-network argument rejected.

PAR endpoint on plain HTTP. RFC 9126 §3: PAR endpoint MUST use TLS. Endpoint-specific MUST — TLS on authorization endpoint does not satisfy TLS on PAR endpoint.

"Internal network safe" rejected: same threat model as JWKS HTTP (on-path MITM within internal network). PAR request body contains code_challenge, redirect_uri, scope — parameters that could be manipulated by an on-path attacker to substitute a known code_verifier challenge. Interception of PAR body before registration allows parameter injection. RFC 9126 §3 MUST: violated. REPROCESS.

---

## Pair[CB4-3.4-836]

### A
PAR endpoint served over HTTPS with client authentication required:

The PAR endpoint is at https://as.example.com/par — HTTPS with TLS 1.3 and a valid CA certificate. All PAR requests require client authentication (confidential clients must include client_secret_post or a registered authentication method). The endpoint returns request_uri and expires_in. The request_uri is single-use and bound to the client_id.

### B
PASS. RFC 9126 §3 MUST TLS satisfied. Client authentication and request_uri controls also correct.

HTTPS: TLS 1.3 with valid CA certificate. RFC 9126 §3 MUST TLS satisfied. Authorization parameters transmitted over TLS are protected from on-path interception and substitution.

Client authentication: confidential clients must authenticate at the PAR endpoint. RFC 9126 §2 requires PAR to enforce the same client authentication that the token endpoint requires — satisfied. Unauthenticated parties cannot register authorization requests on behalf of registered clients.

request_uri single-use + client_id binding: per ext25 patterns (RFC 9126 §4.1 MUST one-time-use; client_id binding satisfied). PASS at PAR lifecycle level as well.

No findings across transport, authentication, and lifecycle requirements.

### Reason
RFC 9126 §3 MUST TLS satisfied. Client authentication matches token endpoint policy. Request_uri controls correct. PASS.

### Phrasing-B
PASS — PAR endpoint correctly served over HTTPS with client authentication and request_uri controls.

HTTPS TLS 1.3 valid CA. RFC 9126 §3 MUST TLS satisfied. Authorization parameters protected in transit.
Client authentication required at PAR endpoint: RFC 9126 §2 satisfied.
request_uri single-use + client_id binding: RFC 9126 §4.1 MUST conditions satisfied.
All PAR transport, authentication, and lifecycle requirements met. PASS.
