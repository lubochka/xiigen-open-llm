# CB4 Stage 3 — Node Pipeline Chain Set ext34
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1045 through CB4-3.4-1052 (8 traces × 2 phrasings = 16 instances)
# Covers: full 9-node pipeline traces for all 8 ext40 arbiter patterns
# Running total after this file: 120 + (1052 − 60) × 2 = 2104 instances (1052 base pairs)
# Sources: RFC 8705 §3.1, OIDC Core §3.1.2.1, RFC 7636 §4.4, RFC 6749 §4.1.2, RFC 6749 §10.5, RFC 8725 §2.1, RFC 7009, RFC 6749 §2.3.1

---

## Trace[CB4-3.4-1045]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** mTLS certificate-bound access token — RS skips cnf/x5t#S256 thumbprint comparison
**Expected verdict:** REPROCESS

**A:**
A resource server accepts mTLS certificate-bound access tokens (RFC 8705). The AS embeds a `cnf` claim in the JWT with an `x5t#S256` value (SHA-256 thumbprint of the client's TLS certificate). The RS validates signature, `iss`, `aud`, `exp`, and `scope`. The RS does not extract the TLS client certificate from the connection and does not compare it against `cnf/x5t#S256`. Rationale: "The mTLS connection itself proves the client has a certificate — that's sufficient."

**B:**
Node 1 · Domain Splitter: Input classified as JWT access token validation at resource server. Territory: T-rfc-jwt (certificate-bound token handling), T-asvs-token. Sub-domain: mTLS certificate binding. Routed to certificate-binding validation branch.

Node 2 · Source Classifier: Primary source: RFC 8705 §3.1 (mTLS certificate-bound access tokens). Secondary source: ASVS V9.3.1 (access token validation at RS). cnf claim handling is subject to RFC 8705 §3.1 normative requirements.

Node 3 · Claim Extractor: Extracted claims — (1) RS validates signature, iss, aud, exp, scope: confirmed; (2) cnf/x5t#S256 present in JWT: confirmed; (3) RS extracts TLS client certificate from connection: NOT performed; (4) RS compares certificate thumbprint against cnf/x5t#S256: NOT performed; (5) Rationale: "mTLS connection proves client has a certificate."

Node 4 · Standard Matcher: RFC 8705 §3.1 MUST: "the resource server MUST verify that the x5t#S256 ... matches the certificate used by the client when making the request." Matched claim: RS does not compare thumbprint. Applicability: direct — RS accepts certificate-bound tokens without binding verification.

Node 5 · Violation Detector: Violation confirmed. RFC 8705 §3.1 MUST: RS must verify cnf/x5t#S256 against TLS client certificate. RS skips this verification entirely. Certificate-bound token is functionally equivalent to plain bearer token at this RS — any client with a valid signed token can use it regardless of which certificate it holds.

Node 6 · Severity Ranker: RFC 8705 §3.1 MUST (unconditional). Skipping thumbprint comparison nullifies the entire certificate binding replay-protection mechanism. Attack vector: attacker steals token, presents it with a different certificate, RS accepts. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 8705 §3.1 MUST: explicit requirement to compare x5t#S256; (2) RS implementation: thumbprint comparison absent; (3) Attack surface: token replay by different client is undetected; (4) Rationale "mTLS proves client has a certificate" rejected: cannot determine whether the presented certificate matches the bound certificate without comparison.

Node 8 · Arbiter: REPROCESS. RFC 8705 §3.1 MUST violated: RS accepts mTLS certificate-bound tokens without comparing cnf/x5t#S256 against the TLS client certificate. Certificate binding's replay protection is eliminated. "mTLS connection proves certificate" argument rejected — RS has no way to verify certificate identity without explicit comparison. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 8705 §3.1 MUST. Violation: cnf/x5t#S256 not compared against TLS client certificate. Remedy: extract client cert SHA-256 thumbprint from TLS connection; compare against cnf/x5t#S256 in JWT on every request; reject with 401 on mismatch.

**Reason:** RFC 8705 §3.1 MUST: RS must verify cnf/x5t#S256 matches TLS client certificate. Skipping comparison makes certificate binding a no-op. Pipeline detects MUST violation at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-asvs-token. mTLS certificate-bound token at RS. Certificate binding verification branch.
Node 2 · Source Classifier: RFC 8705 §3.1 primary. cnf/x5t#S256 thumbprint handling.
Node 3 · Claim Extractor: cnf/x5t#S256 in JWT: yes. RS extracts TLS cert: no. RS compares thumbprint: no.
Node 4 · Standard Matcher: RFC 8705 §3.1 MUST: verify x5t#S256 against TLS client cert. Matched: thumbprint comparison absent.
Node 5 · Violation Detector: MUST violated. Certificate binding without verification = no binding. Token replay with different certificate undetected.
Node 6 · Severity Ranker: RFC 8705 §3.1 MUST. Complete bypass of certificate binding security guarantee. REPROCESS.
Node 7 · Evidence Assembler: RFC 8705 §3.1 MUST; no thumbprint comparison; stolen token replayable with any certificate; "mTLS proves cert" rejected.
Node 8 · Arbiter: REPROCESS. RFC 8705 §3.1 MUST: cnf/x5t#S256 must be verified against TLS client cert. mTLS certificate binding nullified. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 8705 §3.1 MUST. Add: extract TLS client cert thumbprint; compare against cnf/x5t#S256; reject on mismatch.

---

## Trace[CB4-3.4-1046]

**Territory:** T-oauth-oidc · T-asvs-token
**Domain:** OIDC `prompt=login` ignored — existing session returned without re-authentication
**Expected verdict:** REPROCESS

**A:**
An OIDC (OpenID Connect) Relying Party sends an authorization request with `prompt=login`. The Authorization Server has a valid user session and returns an `id_token` with the current session's `auth_time` value without prompting re-authentication. Rationale: "The user is already logged in. `prompt=login` is a hint, not a mandate."

**B:**
Node 1 · Domain Splitter: Input classified as OIDC authorization request parameter handling. Territory: T-oauth-oidc (OIDC prompt parameter), T-asvs-token. Sub-domain: authentication enforcement at AS. Routed to OIDC prompt parameter handling branch.

Node 2 · Source Classifier: Primary source: OIDC Core §3.1.2.1 (prompt parameter values). Secondary source: ASVS V2.4.3 (re-authentication). `prompt=login` is a normative OIDC parameter with MUST-level semantics.

Node 3 · Claim Extractor: Extracted — (1) RP sent prompt=login: confirmed; (2) AS has valid user session: confirmed; (3) AS returned id_token with stale auth_time: confirmed; (4) User re-authentication performed: NOT performed; (5) Rationale: "prompt=login is a hint, not a mandate."

Node 4 · Standard Matcher: OIDC Core §3.1.2.1 MUST: "If this parameter contains `login`, the Authorization Server MUST prompt the End-User for reauthentication." Matched claim: AS returned existing session without re-authentication. Applicability: direct — AS ignored mandatory re-authentication requirement.

Node 5 · Violation Detector: Violation confirmed. OIDC Core §3.1.2.1 MUST: prompt=login requires re-authentication. AS returned stale session assertion. Security-sensitive RP flows relying on prompt=login (step-up authentication, pre-operation identity confirmation) are silently bypassed.

Node 6 · Severity Ranker: OIDC Core §3.1.2.1 MUST (unconditional). "Hint" characterization is textually unsupported — the specification uses MUST language. Step-up authentication security guarantee eliminated. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) OIDC Core §3.1.2.1 MUST: prompt=login → mandatory re-authentication or login_required error; (2) AS returned stale session auth_time; (3) User not re-authenticated; (4) "Hint" characterization contradicted by OIDC Core normative language; (5) Attack surface: high-risk operations protected by prompt=login bypassed.

Node 8 · Arbiter: REPROCESS. OIDC Core §3.1.2.1 MUST violated: AS must prompt for re-authentication when prompt=login; if unable, must return login_required. Returning existing session silently violates both obligations. Step-up authentication security guarantee eliminated. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: OIDC Core §3.1.2.1 MUST. Violation: prompt=login ignored; stale session returned. Remedy: AS must force re-authentication UI on prompt=login; return login_required if interactive auth not possible.

**Reason:** OIDC Core §3.1.2.1 MUST: prompt=login requires AS to force re-authentication. Returning existing session violates MUST. Pipeline detects violation at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-oauth-oidc, T-asvs-token. OIDC prompt parameter handling at AS. Authentication enforcement branch.
Node 2 · Source Classifier: OIDC Core §3.1.2.1 primary. prompt=login normative semantics.
Node 3 · Claim Extractor: prompt=login sent: yes. Re-authentication performed: no. Stale auth_time returned: yes.
Node 4 · Standard Matcher: OIDC Core §3.1.2.1 MUST: re-authenticate or return login_required. Matched: neither performed.
Node 5 · Violation Detector: MUST violated. prompt=login ignored; existing session returned. RP step-up flows bypassed silently.
Node 6 · Severity Ranker: OIDC Core §3.1.2.1 MUST. "Hint" characterization rejected by normative language. REPROCESS.
Node 7 · Evidence Assembler: OIDC Core §3.1.2.1 MUST; no re-auth performed; stale auth_time in id_token; step-up authentication bypass confirmed.
Node 8 · Arbiter: REPROCESS. OIDC Core §3.1.2.1 MUST: prompt=login requires re-auth or login_required. Existing session returned instead. REPROCESS.
Node 9 · Formatter: REPROCESS. OIDC Core §3.1.2.1 MUST. Fix: force re-authentication on prompt=login; return login_required when not possible.

---

## Trace[CB4-3.4-1047]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PKCE `code_challenge` not stored — AS accepts any non-empty `code_verifier`
**Expected verdict:** REPROCESS

**A:**
An Authorization Server implements PKCE (Proof Key for Code Exchange, RFC 7636) for public clients. The `code_challenge` and `code_challenge_method` are accepted at authorization request but not persisted with the authorization code. At the token endpoint, the AS confirms `code_verifier` is non-empty but performs no cryptographic verification against any stored challenge. Comment: "We validate that code_verifier is present — that proves the client sent it. The PKCE flow is implemented."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 PKCE implementation at authorization server. Territory: T-rfc-oauth2 (authorization code exchange), T-asvs-token. Sub-domain: PKCE code verifier validation. Routed to PKCE implementation branch.

Node 2 · Source Classifier: Primary source: RFC 7636 §4.4 (PKCE authorization server requirements). Secondary source: ASVS V3.5.2 (single-use authorization codes). code_challenge and code_verifier handling is subject to RFC 7636 §4.4 normative requirements.

Node 3 · Claim Extractor: Extracted — (1) code_challenge accepted at authorization request: confirmed; (2) code_challenge persisted with authorization code: NOT done; (3) code_verifier non-empty check at token endpoint: confirmed; (4) Cryptographic comparison of code_verifier against stored code_challenge: NOT performed; (5) Comment: "verifier present = PKCE implemented."

Node 4 · Standard Matcher: RFC 7636 §4.4 MUST: "the authorization server MUST ... verify that the code_challenge parameter stored with the authorization code was generated from the code_verifier that the client provides." Matched claims: code_challenge not stored; no cryptographic verification. Applicability: direct — both MUST requirements violated.

Node 5 · Violation Detector: Violation confirmed. RFC 7636 §4.4 MUST (×2): (1) code_challenge must be stored with the authorization code; (2) code_verifier must be cryptographically compared against stored challenge. Neither requirement met. An attacker who intercepts the authorization code can exchange it with any arbitrary verifier string — PKCE provides zero protection.

Node 6 · Severity Ranker: RFC 7636 §4.4 MUST (×2). "Presence check = PKCE" characterization is incorrect — presence without cryptographic comparison is not PKCE. Authorization code interception attacks are fully viable. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 7636 §4.4 MUST: store challenge; verify verifier; (2) code_challenge not stored: confirmed; (3) cryptographic comparison: absent; (4) Attack: intercepted code exchangeable with arbitrary verifier; (5) "Verifier present = PKCE" rejected — presence is not verification.

Node 8 · Arbiter: REPROCESS. RFC 7636 §4.4 MUST violated (×2): code_challenge not stored with authorization code; code_verifier not compared against stored challenge. Accepting any non-empty verifier negates all PKCE protection. Authorization code interception fully viable. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 7636 §4.4 MUST (×2). Violation: code_challenge not stored; no cryptographic verification. Remedy: persist code_challenge at authorization request; compute BASE64URL(SHA256(code_verifier)) at token request; compare via constant-time comparison; reject on mismatch.

**Reason:** RFC 7636 §4.4 MUST ×2: store code_challenge; verify code_verifier against it. Presence-only check is not PKCE. Pipeline detects MUST violations at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. PKCE implementation at AS. code_challenge/code_verifier verification branch.
Node 2 · Source Classifier: RFC 7636 §4.4 primary. PKCE server requirements.
Node 3 · Claim Extractor: code_challenge accepted: yes. code_challenge stored: no. code_verifier non-empty check: yes. Cryptographic comparison: no.
Node 4 · Standard Matcher: RFC 7636 §4.4 MUST: store challenge; verify verifier. Matched: both requirements absent.
Node 5 · Violation Detector: MUST violated ×2. No challenge stored; no cryptographic comparison. PKCE provides zero protection.
Node 6 · Severity Ranker: RFC 7636 §4.4 MUST ×2. Code interception attack fully viable. REPROCESS.
Node 7 · Evidence Assembler: RFC 7636 §4.4 MUST ×2; no storage; no comparison; intercepted code exchangeable with any verifier.
Node 8 · Arbiter: REPROCESS. RFC 7636 §4.4 MUST: store code_challenge; verify code_verifier cryptographically. Neither done. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 7636 §4.4 MUST ×2. Add: store challenge at auth request; BASE64URL(SHA256(verifier)) comparison at token request.

---

## Trace[CB4-3.4-1048]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** AS omits `state` from authorization response — CSRF protection broken
**Expected verdict:** REPROCESS

**A:**
A client sends an authorization request with a `state` parameter (a randomly generated CSRF (Cross-Site Request Forgery) token). The Authorization Server processes the request and redirects to the redirect URI without including `state` in the response. AS rationale: "State is a client-side construct — the AS doesn't need to echo it back."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 authorization response parameter handling. Territory: T-rfc-oauth2 (state parameter), T-cwe-authn. Sub-domain: CSRF (Cross-Site Request Forgery) protection at authorization server. Routed to state parameter echo branch.

Node 2 · Source Classifier: Primary source: RFC 6749 §4.1.2 (authorization response parameters). Secondary source: CWE-352 (Cross-Site Request Forgery). state parameter echo is a normative REQUIRED obligation in RFC 6749 §4.1.2.

Node 3 · Claim Extractor: Extracted — (1) Client sent state parameter in authorization request: confirmed; (2) AS included state in authorization response: NOT done; (3) AS rationale: "state is client-side, AS doesn't echo it."

Node 4 · Standard Matcher: RFC 6749 §4.1.2: "If the `state` request parameter was present in the client authorization request, it MUST be returned to the client in the response." REQUIRED (RFC 2119 MUST). Matched claim: state parameter absent from response. Applicability: direct — REQUIRED obligation unmet.

Node 5 · Violation Detector: Violation confirmed. RFC 6749 §4.1.2 MUST: state must be echoed in authorization response when present in request. State absent from response. Client cannot perform CSRF verification — any injected authorization response is accepted without challenge.

Node 6 · Severity Ranker: RFC 6749 §4.1.2 MUST (REQUIRED). CSRF protection mechanism broken at AS. "Client-side construct" rationale does not reduce severity — echoing state is a normative AS obligation. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 6749 §4.1.2 MUST: echo state in response; (2) State absent from authorization response: confirmed; (3) CSRF verification at client: impossible without echoed state; (4) Attack: injected authorization response accepted by client without state check; (5) "Client-side construct" rationale rejected — AS is required to echo it.

Node 8 · Arbiter: REPROCESS. RFC 6749 §4.1.2 MUST violated: state parameter present in authorization request but absent from response. Client CSRF verification broken. Injected authorization responses indistinguishable from legitimate responses at client. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 6749 §4.1.2 MUST (REQUIRED). Violation: state omitted from authorization response. Remedy: echo state parameter unmodified in authorization response whenever present in request.

**Reason:** RFC 6749 §4.1.2 MUST: state must be echoed in authorization response. Omission breaks CSRF protection. Pipeline detects MUST violation at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-cwe-authn. AS authorization response parameter handling. State echo / CSRF protection branch.
Node 2 · Source Classifier: RFC 6749 §4.1.2 primary. CWE-352 secondary. state parameter echo required.
Node 3 · Claim Extractor: state in request: yes. state in response: no. Rationale: "client-side construct."
Node 4 · Standard Matcher: RFC 6749 §4.1.2 MUST (REQUIRED): echo state in response. Matched: state absent from response.
Node 5 · Violation Detector: MUST violated. State omitted. Client CSRF verification impossible.
Node 6 · Severity Ranker: RFC 6749 §4.1.2 MUST. CSRF bypass. REPROCESS.
Node 7 · Evidence Assembler: RFC 6749 §4.1.2 MUST; state omitted; CSRF verification broken; injected responses undetectable.
Node 8 · Arbiter: REPROCESS. RFC 6749 §4.1.2 MUST: state must be echoed. Absent from response. CSRF protection eliminated. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 6749 §4.1.2 MUST. Fix: echo state unmodified in every authorization response where it was present in the request.

---

## Trace[CB4-3.4-1049]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** Authorization code lifetime 10 minutes — exceeds accepted short-lived practice
**Expected verdict:** CONCERN

**A:**
An Authorization Server issues authorization codes with a 10-minute expiry. Rationale: "10 minutes gives clients ample time under adverse network conditions. Short-lived codes create bad UX (User Experience) when latency is high. We consider 10 minutes 'short-lived' under RFC 6749 §10.5."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 authorization code lifetime configuration. Territory: T-rfc-oauth2 (code lifetime), T-asvs-token. Sub-domain: authorization code expiry management. Routed to code lifetime validation branch.

Node 2 · Source Classifier: Primary source: RFC 6749 §10.5 (authorization codes, SHOULD short-lived). Secondary source: ASVS V3.5.2 ("shortest possible time"). RFC 6749 §10.5 does not specify a numeric maximum — this is a guidance-level requirement.

Node 3 · Claim Extractor: Extracted — (1) Authorization code expiry: 10 minutes; (2) Codes are single-use: not explicitly stated but assumed per RFC 6749 §10.5; (3) Rationale: "10 minutes is short-lived, network conditions justify it"; (4) RFC 6749 §10.5 acknowledged by developer.

Node 4 · Standard Matcher: RFC 6749 §10.5: "Authorization codes MUST be short-lived and single-use." ASVS V3.5.2: "expire in the shortest possible time." "Short-lived" is not quantified in RFC 6749 §10.5. OWASP guidance and common practice: 1–2 minutes. Matched claim: 10-minute lifetime exceeds accepted "short-lived" interpretation by 5–10×.

Node 5 · Violation Detector: Guidance gap identified. RFC 6749 §10.5 does not define a numeric maximum — "short-lived" is qualitative. 10 minutes is broadly considered excessive (accepted practice: 1–2 minutes). ASVS V3.5.2 guidance not met: "shortest possible time" conflicts with 10-minute window. No RFC MUST for specific maximum lifetime. Guidance-level gap only.

Node 6 · Severity Ranker: No RFC MUST with numeric bound. ASVS V3.5.2 guidance ("shortest possible time") not met. Code interception window 5–10× broader than accepted practice. No unconditional MUST for specific duration. Severity: CONCERN.

Node 7 · Evidence Assembler: Evidence — (1) RFC 6749 §10.5: short-lived + single-use MUST (numeric duration not specified); (2) ASVS V3.5.2: "shortest possible time" (guidance); (3) Accepted practice: 1–2 minutes; (4) 10 minutes: 5–10× broader interception window; (5) No RFC MUST for maximum numeric value — CONCERN level.

Node 8 · Arbiter: CONCERN. RFC 6749 §10.5 MUST for "short-lived" satisfied in letter (codes do expire); violated in spirit (10 minutes exceeds accepted interpretation). ASVS V3.5.2 guidance gap: "shortest possible time" not met. No RFC MUST for specific maximum. Guidance-level gap = CONCERN. Recommend: reduce to ≤2 minutes.

Node 9 · Formatter: Verdict: CONCERN. Primary citation: RFC 6749 §10.5 / ASVS V3.5.2. Gap: 10-minute authorization codes exceed accepted practice (1–2 min). Recommendation: reduce to ≤2 minutes to align with ASVS V3.5.2 "shortest possible time" guidance.

**Reason:** RFC 6749 §10.5 short-lived not numerically defined; ASVS V3.5.2 "shortest possible time" guidance not met. No RFC MUST for specific max. 10-minute codes exceed practice. Pipeline classifies as CONCERN at Node 6.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-asvs-token. Authorization code lifetime configuration. Code expiry branch.
Node 2 · Source Classifier: RFC 6749 §10.5 (MUST short-lived, numeric not specified). ASVS V3.5.2 (shortest possible time).
Node 3 · Claim Extractor: Code expiry: 10 minutes. Single-use: assumed. Rationale: "still short-lived."
Node 4 · Standard Matcher: RFC 6749 §10.5: short-lived (unquantified). ASVS V3.5.2: shortest possible time. Practice: 1–2 min. 10 min: 5–10× broader.
Node 5 · Violation Detector: Guidance gap. No numeric MUST. ASVS V3.5.2 "shortest possible time" not met. Interception window excessive.
Node 6 · Severity Ranker: No RFC MUST with numeric bound. Guidance gap only. CONCERN.
Node 7 · Evidence Assembler: RFC 6749 §10.5 MUST (no numeric limit); ASVS V3.5.2 guidance gap; 10 min >> 1–2 min accepted practice.
Node 8 · Arbiter: CONCERN. ASVS V3.5.2 guidance not met; RFC 6749 §10.5 short-lived MUST borderline. No numeric MUST = CONCERN not REPROCESS. Recommend ≤2 minutes.
Node 9 · Formatter: CONCERN. RFC 6749 §10.5 / ASVS V3.5.2 gap. Recommend: reduce to ≤2 minutes.

---

## Trace[CB4-3.4-1050]

**Territory:** T-rfc-jwt · T-oauth-oidc
**Domain:** OIDC RP accepts `id_token` with `alg: none` — unsigned token treated as valid
**Expected verdict:** REPROCESS

**A:**
An OIDC (OpenID Connect) Relying Party validates `id_tokens`. The validation logic checks `iss`, `aud`, `exp`, `nonce`. For signature, the RP reads the `alg` header: if `alg` is `none`, signature verification is skipped entirely. Rationale: "`alg: none` is a valid JWT algorithm per RFC 7518. We support it for internal AS deployments."

**B:**
Node 1 · Domain Splitter: Input classified as OIDC id_token signature validation at Relying Party. Territory: T-rfc-jwt (JWT algorithm handling), T-oauth-oidc. Sub-domain: algorithm acceptance policy. Routed to JWT algorithm validation branch.

Node 2 · Source Classifier: Primary source: RFC 8725 §2.1 (JWT Best Current Practices, algorithm restrictions). Secondary source: OIDC Core §2 (id_token signature requirement). RFC 8725 §2.1 imposes MUST NOT on accepting alg:none.

Node 3 · Claim Extractor: Extracted — (1) RP checks iss, aud, exp, nonce: confirmed; (2) RP reads alg header: confirmed; (3) If alg=none, signature verification skipped: confirmed; (4) alg:none accepted as valid algorithm: confirmed; (5) Rationale: "RFC 7518 permits alg:none; needed for internal deployments."

Node 4 · Standard Matcher: RFC 8725 §2.1 MUST NOT: "Implementations MUST NOT accept algorithms that offer no or insufficient security, e.g., `alg: none`." Matched claim: RP accepts alg:none and skips signature verification. Applicability: direct — RFC 8725 §2.1 explicitly prohibits this.

Node 5 · Violation Detector: Violation confirmed. RFC 8725 §2.1 MUST NOT: alg:none must not be accepted. RP accepts unsigned tokens without signature verification. Any party can fabricate an id_token with arbitrary claims, set alg:none, and the RP will accept it as a valid assertion from the AS — complete identity impersonation.

Node 6 · Severity Ranker: RFC 8725 §2.1 MUST NOT (unconditional). "RFC 7518 permits alg:none" argument rejected: RFC 8725 §2.1's MUST NOT overrides RFC 7518 flexibility. Complete identity impersonation attack surface. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 8725 §2.1 MUST NOT: alg:none prohibited; (2) RP accepts alg:none and skips signature: confirmed; (3) Attack: attacker fabricates id_token with arbitrary sub/email/role, sets alg:none → RP accepts; (4) "RFC 7518 validity" argument rejected by RFC 8725 §2.1 MUST NOT; (5) "Internal AS" argument rejected: cross-trust-boundary tokens must be signed.

Node 8 · Arbiter: REPROCESS. RFC 8725 §2.1 MUST NOT violated: RP accepts alg:none and skips signature verification. Unsigned id_tokens enable complete identity impersonation. RFC 7518 "valid algorithm" defense explicitly overridden by RFC 8725 §2.1. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 8725 §2.1 MUST NOT. Violation: alg:none accepted; signature verification skipped. Remedy: maintain explicit algorithm allowlist (RS256/ES256 family); reject any token where alg is not on the allowlist.

**Reason:** RFC 8725 §2.1 MUST NOT: RP must not accept alg:none. Skipping signature enables identity impersonation. Pipeline detects MUST NOT violation at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-oauth-oidc. OIDC RP id_token algorithm handling. Algorithm acceptance policy branch.
Node 2 · Source Classifier: RFC 8725 §2.1 MUST NOT primary. OIDC Core §2 signature requirement secondary.
Node 3 · Claim Extractor: alg:none accepted: yes. Signature skipped on alg:none: yes. Rationale: "RFC 7518 allows it."
Node 4 · Standard Matcher: RFC 8725 §2.1 MUST NOT: accept alg:none prohibited. Matched: RP accepts alg:none.
Node 5 · Violation Detector: MUST NOT violated. No signature check. Complete identity impersonation possible.
Node 6 · Severity Ranker: RFC 8725 §2.1 MUST NOT. RFC 7518 defense rejected. REPROCESS.
Node 7 · Evidence Assembler: RFC 8725 §2.1 MUST NOT; alg:none accepted; unsigned token → arbitrary identity claims; RFC 7518 defense overridden.
Node 8 · Arbiter: REPROCESS. RFC 8725 §2.1 MUST NOT: alg:none must not be accepted. Identity impersonation via unsigned id_token. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 8725 §2.1 MUST NOT. Fix: explicit algorithm allowlist; reject alg:none and any non-allowlisted algorithm.

---

## Trace[CB4-3.4-1051]

**Territory:** T-rfc-jwt · T-oauth-oidc
**Domain:** RS accepts JWT access tokens without revocation check — revoked tokens valid until expiry
**Expected verdict:** CONCERN

**A:**
A resource server validates JWT access tokens using signature, iss, aud, exp, scope checks. The AS maintains a token revocation endpoint (RFC 7009) and revocation list. The RS does not check revocation on each request. Revoked tokens remain valid at the RS until `exp`. Rationale: "JWTs are stateless — per-request revocation checks defeat the purpose and add latency."

**B:**
Node 1 · Domain Splitter: Input classified as JWT access token revocation handling at resource server. Territory: T-rfc-jwt (stateless JWT trade-off), T-oauth-oidc. Sub-domain: token revocation enforcement. Routed to revocation checking branch.

Node 2 · Source Classifier: Primary source: RFC 7009 (Token Revocation — client/AS protocol). Secondary source: ASVS V9.3.2 (access token lifetime guidance). RFC 7009 does not impose MUST on RS to check revocation per request for JWT access tokens. ASVS V9.3.2 is guidance-level.

Node 3 · Claim Extractor: Extracted — (1) RS validates signature, iss, aud, exp, scope: confirmed; (2) Revocation endpoint exists at AS: confirmed; (3) RS checks revocation per request: NOT done; (4) Revoked tokens valid until exp: confirmed; (5) Rationale: "stateless JWT design accepts this trade-off."

Node 4 · Standard Matcher: RFC 7009: defines token revocation protocol between clients and AS — does not mandate RS behavior for JWT validation. ASVS V9.3.2: short access token lifetime recommended. No RFC MUST for per-request revocation checking of JWT access tokens at RS. Matched claim: RS accepts stateless JWT trade-off without compensating control documentation.

Node 5 · Violation Detector: No RFC MUST violated. RFC 7009 does not require RS to poll revocation for JWT tokens. Stateless JWT design inherently accepts time-bounded revocation gap. ASVS V9.3.2 guidance (short TTL) compensates when followed. Gap: no compensating controls (TTL, introspection) explicitly stated for this RS. Guidance-level gap only.

Node 6 · Severity Ranker: No RFC MUST for per-request JWT revocation checking. ASVS V9.3.2 is guidance level. Stateless JWT trade-off is architecturally accepted design. Gap severity depends on TTL length — unknown from input. Severity: CONCERN.

Node 7 · Evidence Assembler: Evidence — (1) RFC 7009: no MUST for RS per-request revocation checking; (2) ASVS V9.3.2: short TTL guidance (compensating control); (3) No TTL information provided in input; (4) Stateless JWT trade-off: architecturally valid; (5) Guidance gap: no compensating control documented.

Node 8 · Arbiter: CONCERN. No RFC MUST for RS to check JWT revocation per request. Stateless JWT trade-off is architecturally valid. ASVS V9.3.2 guidance gap: short TTL (compensating control) not documented. Severity varies with token TTL. CONCERN.

Node 9 · Formatter: Verdict: CONCERN. Primary citation: ASVS V9.3.2 guidance. Gap: revocation check absent; compensating control (short TTL or introspection) not documented. Recommendation: use ≤15-minute access token TTL (ASVS V9.3.1) and/or RFC 7662 introspection for sensitive endpoints.

**Reason:** RFC 7009 imposes no MUST on RS for per-request JWT revocation checking. Stateless JWT trade-off is valid. ASVS V9.3.2 guidance gap without documented compensating control. Pipeline classifies as CONCERN at Node 6.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-jwt, T-oauth-oidc. JWT access token revocation at RS. Revocation checking branch.
Node 2 · Source Classifier: RFC 7009 (no RS MUST). ASVS V9.3.2 guidance (short TTL). Stateless JWT trade-off.
Node 3 · Claim Extractor: Revocation check: no. Revocation endpoint exists: yes. Revoked tokens valid until exp: yes.
Node 4 · Standard Matcher: RFC 7009: no RS MUST for per-request check. ASVS V9.3.2: guidance only. No numeric MUST.
Node 5 · Violation Detector: No RFC MUST violated. Guidance gap: no compensating control documented.
Node 6 · Severity Ranker: RFC 7009 no MUST. ASVS V9.3.2 guidance. CONCERN.
Node 7 · Evidence Assembler: RFC 7009 no RS MUST; ASVS V9.3.2 guidance gap; stateless trade-off valid; compensating controls not documented.
Node 8 · Arbiter: CONCERN. No RFC MUST for JWT revocation per request. ASVS V9.3.2 guidance not met (compensating controls absent). CONCERN.
Node 9 · Formatter: CONCERN. ASVS V9.3.2 guidance gap. Recommend: short TTL (≤15 min) and/or RFC 7662 introspection for sensitive endpoints.

---

## Trace[CB4-3.4-1052]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** `client_secret` transmitted as URL query parameter — credential exposure in logs
**Expected verdict:** REPROCESS

**A:**
A confidential OAuth client authenticates to the token endpoint by including `client_id` and `client_secret` as URL query parameters: `POST /token?grant_type=authorization_code&code=...&client_id=myapp&client_secret=s3cr3t`. Rationale: "Query parameters are simple and work with all HTTP clients. The connection is HTTPS, so the values are encrypted in transit."

**B:**
Node 1 · Domain Splitter: Input classified as OAuth 2.0 client authentication method at token endpoint. Territory: T-rfc-oauth2 (client authentication), T-cwe-authn. Sub-domain: credential transmission security. Routed to client authentication method branch.

Node 2 · Source Classifier: Primary source: RFC 6749 §2.3.1 (client authentication methods). Secondary source: CWE-598 (Use of GET Request Method with Sensitive Query Strings) / CWE-312 (Cleartext Storage of Sensitive Information). RFC 6749 §2.3.1 imposes MUST NOT on URI query parameter authentication.

Node 3 · Claim Extractor: Extracted — (1) client_id in URL query parameter: confirmed; (2) client_secret in URL query parameter: confirmed; (3) Connection uses HTTPS: confirmed; (4) Rationale: "HTTPS encrypts it; query params are simple."

Node 4 · Standard Matcher: RFC 6749 §2.3.1 MUST NOT: "The authorization server MUST NOT accept client authentication using URI query parameters." Matched claim: client_secret transmitted as query parameter. Applicability: direct — MUST NOT directly triggered.

Node 5 · Violation Detector: Violation confirmed. RFC 6749 §2.3.1 MUST NOT: client credentials must not be in URI query parameters. HTTPS transit encryption does not prevent credential exposure: (1) server access logs record the full URL including query parameters; (2) proxy logs capture query parameters; (3) browser history stores query parameters; (4) TLS termination points see plaintext URL. "HTTPS encrypts it" does not address the log exposure risk.

Node 6 · Severity Ranker: RFC 6749 §2.3.1 MUST NOT (unconditional). HTTPS transit encryption explicitly does not remediate the log/history exposure. Credentials may persist in multiple log stores. Severity: REPROCESS.

Node 7 · Evidence Assembler: Evidence — (1) RFC 6749 §2.3.1 MUST NOT: no URI query parameter auth; (2) client_secret in query parameter: confirmed; (3) Exposure vectors: server access logs, proxy logs, browser history, TLS termination plaintext; (4) "HTTPS encrypts" rejected — logs capture post-termination URL; (5) Correct methods: Authorization header (Basic) or POST body.

Node 8 · Arbiter: REPROCESS. RFC 6749 §2.3.1 MUST NOT violated: client_secret transmitted as URI query parameter. HTTPS transit encryption does not prevent credential exposure in server/proxy logs and browser history. "HTTPS encrypts it" argument rejected. REPROCESS.

Node 9 · Formatter: Verdict: REPROCESS. Primary citation: RFC 6749 §2.3.1 MUST NOT. Violation: client_secret in URI query parameter. Remedy: use HTTP Basic (Authorization header) or POST body parameters for client authentication; token endpoint must reject URI query parameter credentials.

**Reason:** RFC 6749 §2.3.1 MUST NOT: client credentials must not be in URI query parameters. HTTPS irrelevant — logs capture URL. Pipeline detects MUST NOT violation at Node 5; Node 8 issues REPROCESS.

**Phrasing-B:**
Node 1 · Domain Splitter: T-rfc-oauth2, T-cwe-authn. Token endpoint client authentication method. Credential transmission branch.
Node 2 · Source Classifier: RFC 6749 §2.3.1 MUST NOT primary. CWE-312/CWE-598 secondary.
Node 3 · Claim Extractor: client_secret in URL query param: yes. HTTPS: yes. Rationale: "HTTPS encrypts it."
Node 4 · Standard Matcher: RFC 6749 §2.3.1 MUST NOT: no URI query param auth. Matched: client_secret in query param.
Node 5 · Violation Detector: MUST NOT violated. HTTPS irrelevant: logs capture URL with credentials post-termination.
Node 6 · Severity Ranker: RFC 6749 §2.3.1 MUST NOT. Credential log exposure. REPROCESS.
Node 7 · Evidence Assembler: RFC 6749 §2.3.1 MUST NOT; client_secret in URL; log/proxy/browser exposure; "HTTPS" rejected.
Node 8 · Arbiter: REPROCESS. RFC 6749 §2.3.1 MUST NOT: client credentials in URI query parameters prohibited. Log exposure confirmed. REPROCESS.
Node 9 · Formatter: REPROCESS. RFC 6749 §2.3.1 MUST NOT. Fix: use Authorization header (Basic) or POST body; reject query param credentials at token endpoint.
