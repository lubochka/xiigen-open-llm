# CB4 Stage 3 — Node Cross-Source Set ext33
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1053 through CB4-3.4-1062 (10 pairs × 2 phrasings = 20 instances)
# Covers: cross-source combinations of ext40 patterns with each other and with prior sub-phase patterns
# Running total after this file: 120 + (1062 − 60) × 2 = 2124 instances (1062 base pairs)
# Graduation rule: N CONCERNs do NOT escalate to REPROCESS (demonstrated: 1058, 1061)
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS (demonstrated: 1056)
# REPROCESS overrides CONCERN when both apply to same scenario (demonstrated: 1054, 1060)

---

## Pair[CB4-3.4-1053]

**Territory:** T-rfc-jwt · T-asvs-token · T-oauth-oidc
**Domain:** mTLS certificate-bound token at RS with no cnf/x5t#S256 validation AND OAuth security events not logged
**Expected verdict:** REPROCESS

**A:**
A resource server accepts mTLS (mutual TLS) certificate-bound JWT access tokens (RFC 8705). The RS validates signature, `iss`, `aud`, `exp`, and `scope` but does not compare the TLS client certificate thumbprint against `cnf/x5t#S256`. Separately, the RS and its backing Authorization Server log no security-relevant events at the application layer: failed authentications, certificate mismatches, and invalid token submissions generate no structured log entries (only network-layer access logs that omit request body and certificate details). The developer team acknowledges both issues but considers them separate concerns for different teams to address.

**B:**
REPROCESS. Two violations compound: (1) RFC 8705 §3.1 MUST: RS must verify cnf/x5t#S256 against TLS client certificate — skipping this makes certificate binding a no-op and token replay (with a different certificate) undetectable; (2) ASVS V8.3.2 guidance gap: no application-level logging for security events, including the certificate mismatch events that would detect RS-targeting attacks. The RFC 8705 §3.1 MUST is the primary driver of REPROCESS. The ASVS V8.3.2 logging gap independently warrants CONCERN but is overshadowed by the REPROCESS finding. Combined: REPROCESS (primary: certificate binding violation). Logging gap is a secondary issue that should be addressed concurrently but does not independently change the verdict.

**Reason:** RFC 8705 §3.1 MUST: cnf/x5t#S256 must be compared against TLS client cert. ASVS V8.3.2 logging gap (secondary CONCERN). Primary MUST violation drives REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8705 §3.1 MUST: RS must verify cnf/x5t#S256 against TLS client cert on each request — not done. ASVS V8.3.2 guidance gap: no application-level security event logging. Primary citation RFC 8705 §3.1 MUST drives REPROCESS verdict. Logging gap addressed concurrently as secondary concern. REPROCESS.

---

## Pair[CB4-3.4-1054]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PKCE `code_challenge` not stored (REPROCESS) combined with 10-minute authorization code lifetime (CONCERN) — graduation rule: REPROCESS overrides CONCERN
**Expected verdict:** REPROCESS

**A:**
An Authorization Server implements PKCE (Proof Key for Code Exchange, RFC 7636) but does not store `code_challenge` values with authorization codes — at code exchange, any non-empty `code_verifier` is accepted without cryptographic comparison. Additionally, authorization codes have a 10-minute lifetime (beyond accepted "short-lived" practice of 1–2 minutes per ASVS V3.5.2 guidance). Both issues are present in the same implementation.

**B:**
REPROCESS. Two findings, two severity levels: (1) RFC 7636 §4.4 MUST (×2): code_challenge not stored; code_verifier not cryptographically verified — this is an unconditional MUST violation that enables authorization code interception attacks. This finding is REPROCESS; (2) ASVS V3.5.2 / RFC 6749 §10.5 guidance: 10-minute authorization codes exceed accepted "shortest possible time" practice — no numeric MUST violated; this finding is CONCERN. The graduation rule applies: REPROCESS does not escalate further; CONCERN does not demote REPROCESS. The combined verdict is REPROCESS (driven by the RFC 7636 §4.4 MUST violation). Both issues must be remediated: PKCE verification is critical; code lifetime should also be reduced to ≤2 minutes.

**Reason:** RFC 7636 §4.4 MUST ×2 (REPROCESS) + ASVS V3.5.2 guidance gap (CONCERN). Graduation rule: REPROCESS overrides CONCERN. Combined verdict = REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 7636 §4.4 MUST: code_challenge not stored; code_verifier not verified → REPROCESS. ASVS V3.5.2 guidance: 10-min codes exceed "shortest possible time" → CONCERN. REPROCESS overrides CONCERN per graduation rule. Combined: REPROCESS. Remediate both: fix PKCE verification (critical) and reduce code lifetime to ≤2 minutes.

---

## Pair[CB4-3.4-1055]

**Territory:** T-rfc-jwt · T-oauth-oidc · T-asvs-token
**Domain:** OIDC RP accepts `id_token` with `alg: none` (REPROCESS) AND DPoP-bound token accepted without `cnf/jkt` validation at RS (REPROCESS) — dual MUST violations
**Expected verdict:** REPROCESS

**A:**
A system has two connected security failures: (1) The OIDC (OpenID Connect) Relying Party accepts `id_tokens` where `alg` is `none`, skipping signature verification entirely, justified as "supporting flexible internal deployments"; (2) The Resource Server accepts DPoP (Demonstrating Proof of Possession, RFC 9449)-bound access tokens — tokens containing a `cnf/jkt` (JWK (JSON Web Key) Thumbprint) claim — without comparing the `jkt` value against the DPoP proof presented in the request. Both failures are in separate components maintained by different teams.

**B:**
REPROCESS. Two independent RFC MUST NOT / MUST violations: (1) RFC 8725 §2.1 MUST NOT: RP must not accept alg:none — unsigned id_tokens enable complete identity impersonation (any party can fabricate arbitrary claims); (2) RFC 9449 §7.2 MUST: RS must verify that the `jkt` value in the token's `cnf` claim matches the public key in the DPoP proof — skipping this makes DPoP binding meaningless. Both are unconditional normative failures. Two REPROCESS findings do not escalate to a higher severity — REPROCESS is REPROCESS. Both violations require independent remediation.

**Reason:** RFC 8725 §2.1 MUST NOT: alg:none accepted at RP → REPROCESS. RFC 9449 §7.2 MUST: cnf/jkt not validated at RS → REPROCESS. Dual MUST violations; combined verdict REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8725 §2.1 MUST NOT: RP accepts alg:none — identity impersonation via unsigned id_token. RFC 9449 §7.2 MUST: RS accepts DPoP-bound token without verifying cnf/jkt against DPoP proof — possession proof nullified. Two independent REPROCESS findings; combined verdict REPROCESS. Both require separate remediation.

---

## Pair[CB4-3.4-1056]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domain:** PO.1 (no formal security policy) AND `state` parameter omitted from authorization response — PO.1 escalation to REPROCESS
**Expected verdict:** REPROCESS

**A:**
An Authorization Server returns authorization responses to clients without including the `state` parameter, even when clients include it in their authorization requests. The AS has no formal OAuth security policy document (PO.1 gap): no documented requirements for state parameter handling, CSRF protection, or authorization response integrity. When the issue is raised with the development team, the response is: "We didn't realize state needed to be echoed; there's no policy saying we have to."

**B:**
REPROCESS. PO.1 (absence of formal security policy): no documented requirements for authorization response integrity, CSRF protection, or OAuth state parameter handling — this is a guidance-level gap (CONCERN in isolation). However, the PO.1 escalation rule applies: PO.1 absence + a traceable production MUST violation = REPROCESS. The traceable production MUST violation: RFC 6749 §4.1.2 REQUIRED: "If the `state` request parameter was present in the client authorization request, it MUST be returned to the client in the response." This MUST is being violated in production — the state parameter is actively omitted. The PO.1 gap explains why the violation went undetected (no policy against it) but does not mitigate it. Combined: PO.1 gap (escalation trigger) + RFC 6749 §4.1.2 MUST violation (traceable production fault) = REPROCESS. Remediate: (1) fix state echo immediately; (2) establish OAuth security policy documenting response integrity requirements.

**Reason:** PO.1 absence (CONCERN) + RFC 6749 §4.1.2 MUST violation (state not echoed in production) = REPROCESS per PO.1 escalation rule.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.2 MUST: state must be echoed in authorization response — actively violated in production. PO.1 gap: no formal OAuth security policy documenting this requirement. PO.1 absence alone = CONCERN; PO.1 + traceable production MUST violation = REPROCESS per escalation rule. Fix state echo immediately; document OAuth response integrity requirements in formal policy.

---

## Pair[CB4-3.4-1057]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-asvs-token
**Domain:** AS omits `state` from authorization response AND RS does not validate `exp` claim — dual MUST violations from separate sub-phase domains
**Expected verdict:** REPROCESS

**A:**
A deployment has two independently identified vulnerabilities: (1) The Authorization Server omits the `state` parameter from authorization responses even when clients include it (from ext40 domain 4); (2) The Resource Server does not validate the `exp` claim on JWT access tokens — expired tokens are accepted indefinitely as long as other checks (signature, iss, aud, scope) pass (from ext39 domain 4). Both issues have been confirmed in production.

**B:**
REPROCESS. Two independent MUST violations: (1) RFC 6749 §4.1.2 REQUIRED: state must be echoed in authorization response — CSRF protection is broken at the authorization endpoint; (2) RFC 9068 §4 MUST: RS must reject JWT when `exp` < current Unix time — expired tokens are accepted indefinitely, providing unlimited replay window for stolen tokens. Both are unconditional MUST violations from authoritative RFCs. The combined verdict is REPROCESS; two MUST violations do not escalate above REPROCESS. Both require remediation: state echo is a one-line fix; exp validation is a critical RS-side check.

**Reason:** RFC 6749 §4.1.2 REQUIRED (state not echoed) + RFC 9068 §4 MUST (exp not validated) — two independent MUST violations; combined verdict REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §4.1.2 REQUIRED: state omitted from authorization response → CSRF bypass. RFC 9068 §4 MUST: exp not validated at RS → stolen tokens valid indefinitely. Two independent MUST violations; combined verdict REPROCESS. Remediate both independently.

---

## Pair[CB4-3.4-1058]

**Territory:** T-rfc-oauth2 · T-rfc-jwt · T-asvs-token
**Domain:** 10-minute authorization code lifetime (CONCERN) combined with no JWT revocation check at RS (CONCERN) — graduation rule: N CONCERNs ≠ REPROCESS
**Expected verdict:** CONCERN

**A:**
An OAuth 2.0 system has two guidance gaps simultaneously: (1) Authorization codes have a 10-minute lifetime — beyond the 1–2 minute accepted practice per ASVS V3.5.2 / RFC 6749 §10.5 "short-lived" guidance; (2) The Resource Server does not check revocation status for JWT access tokens — it relies on stateless validation (signature, iss, aud, exp, scope) without consulting the AS revocation list or performing RFC 7662 introspection. Both are acknowledged by the team.

**B:**
CONCERN. Two guidance gaps: (1) Authorization code lifetime: 10 minutes exceeds ASVS V3.5.2 "shortest possible time" guidance — no RFC numeric MUST violated; (2) JWT revocation check absent: no RFC MUST for per-request revocation checking of JWT access tokens at RS — stateless JWT trade-off is architecturally accepted; ASVS V9.3.2 guidance (short TTL as compensating control) not met. The graduation rule is explicit: N CONCERNs do NOT escalate to REPROCESS regardless of count. Two guidance gaps remain CONCERN. Combined verdict: CONCERN. Recommendations: (1) reduce authorization code lifetime to ≤2 minutes; (2) implement ≤15-minute JWT TTL and/or RFC 7662 introspection for sensitive endpoints.

**Reason:** ASVS V3.5.2 guidance gap (10-min codes) = CONCERN + ASVS V9.3.2 guidance gap (no revocation check) = CONCERN. Two CONCERNs do NOT escalate to REPROCESS. Combined verdict = CONCERN.

**Phrasing-B:**
CONCERN. 10-minute authorization code lifetime: ASVS V3.5.2 guidance gap (CONCERN). JWT revocation check absent: ASVS V9.3.2 guidance gap (CONCERN). Graduation rule: 2 CONCERNs = CONCERN, not REPROCESS. No RFC MUST violated in either case. Combined verdict: CONCERN. Address both: reduce code lifetime to ≤2 min; use short JWT TTL and/or RFC 7662 introspection.

---

## Pair[CB4-3.4-1059]

**Territory:** T-rfc-oauth2 · T-cwe-authn · T-asvs-token
**Domain:** `client_secret` in URL query parameter (REPROCESS) combined with scope enlargement at authorization code exchange (REPROCESS) — dual MUST violations
**Expected verdict:** REPROCESS

**A:**
A confidential OAuth client sends `client_secret` as a URL query parameter when calling the token endpoint (`POST /token?client_secret=s3cr3t&...`). At the same token endpoint, when a client presents an authorization code and requests a `scope` value that is larger than the scope authorized in the original authorization request (e.g., authorization granted `read`; token request asks for `read write`), the AS grants the enlarged scope without rejecting the request.

**B:**
REPROCESS. Two independent MUST violations: (1) RFC 6749 §2.3.1 MUST NOT: the AS must not accept client authentication via URI query parameters — client_secret in URL is exposed in server logs, proxy logs, and browser history regardless of HTTPS; (2) RFC 6749 §4.1.3: the scope in a token request "MUST NOT include any scope not originally granted by the resource owner" — granting enlarged scope beyond the authorization grant violates least-privilege and authorization integrity. Both are unconditional normative failures. Combined verdict: REPROCESS. Both require independent remediation.

**Reason:** RFC 6749 §2.3.1 MUST NOT (client_secret in URL) + RFC 6749 §4.1.3 MUST NOT (scope enlargement). Two MUST violations; combined verdict REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §2.3.1 MUST NOT: client_secret in URI query param exposes credentials in logs. RFC 6749 §4.1.3 MUST NOT: scope enlarged beyond original authorization grant. Two independent MUST violations from RFC 6749; combined verdict REPROCESS. Fix: use Authorization header for client auth; enforce scope at token endpoint to original grant.

---

## Pair[CB4-3.4-1060]

**Territory:** T-rfc-jwt · T-asvs-token · T-rfc-oauth2
**Domain:** mTLS cnf/x5t#S256 not validated at RS (REPROCESS) combined with 10-minute authorization code lifetime (CONCERN) — REPROCESS overrides CONCERN
**Expected verdict:** REPROCESS

**A:**
A deployment has two findings: (1) The Resource Server accepts mTLS certificate-bound JWT access tokens but does not compare the TLS client certificate thumbprint against the `cnf/x5t#S256` value embedded in the token (RFC 8705 §3.1 MUST violation); (2) The Authorization Server issues authorization codes with a 10-minute expiry, exceeding the 1–2 minute accepted practice for "short-lived" codes (ASVS V3.5.2 / RFC 6749 §10.5 guidance gap). The teams responsible for each component have been separately notified.

**B:**
REPROCESS. Two findings at different severity levels: (1) RFC 8705 §3.1 MUST: RS must verify cnf/x5t#S256 against TLS client certificate — not done; certificate binding is nullified; stolen tokens are replayable with any certificate. This is REPROCESS; (2) ASVS V3.5.2 guidance: 10-minute codes exceed "shortest possible time" — no RFC numeric MUST violated. This is CONCERN. REPROCESS overrides CONCERN when both apply to the same scenario. Combined verdict: REPROCESS (driven by RFC 8705 §3.1 MUST). The authorization code lifetime issue remains a CONCERN that must be addressed, but it does not change the verdict. Remediate both: certificate thumbprint comparison is critical; code lifetime should be reduced.

**Reason:** RFC 8705 §3.1 MUST (cnf/x5t#S256 not validated = REPROCESS) + ASVS V3.5.2 guidance gap (10-min codes = CONCERN). REPROCESS overrides CONCERN. Combined verdict = REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8705 §3.1 MUST: cnf/x5t#S256 not compared against TLS client cert → REPROCESS. ASVS V3.5.2 guidance: 10-min codes exceed "shortest possible time" → CONCERN. REPROCESS overrides CONCERN per graduation rule. Combined verdict: REPROCESS. Remediate certificate binding (critical); also reduce code lifetime to ≤2 minutes.

---

## Pair[CB4-3.4-1061]

**Territory:** T-rfc-jwt · T-asvs-token · T-oauth-oidc
**Domain:** JWT access token revocation check absent (CONCERN) combined with 24-hour access token TTL (CONCERN) — graduation rule: N CONCERNs ≠ REPROCESS
**Expected verdict:** CONCERN

**A:**
A resource server uses JWT access tokens with a 24-hour TTL (Time-To-Live). The RS validates signature, iss, aud, exp, and scope but performs no revocation check — revoked tokens remain valid for up to 24 hours after revocation. The AS maintains a revocation endpoint (RFC 7009) but the RS never consults it. Two ASVS guidance gaps are identified: (1) ASVS V9.3.1 guidance: access token TTL should typically be 15 minutes to 1 hour; 24 hours significantly exceeds this; (2) ASVS V9.3.2 guidance: revocation check absent with no compensating control (not even a short TTL to bound the window).

**B:**
CONCERN. Two guidance gaps: (1) 24-hour JWT TTL: ASVS V9.3.1 guidance states "usually 15 min to 1 hour" — "usually" is not a MUST; no RFC MUST for specific numeric maximum; this is a guidance-level gap (CONCERN); (2) JWT revocation check absent: RFC 7009 does not impose MUST on RS for per-request revocation checking of JWT tokens — stateless JWT trade-off is architecturally accepted; ASVS V9.3.2 guidance gap with no compensating control (the long TTL compounds the revocation gap severity). The graduation rule applies: N CONCERNs do NOT escalate to REPROCESS. Two ASVS guidance gaps remain CONCERN even when they compound each other. Combined verdict: CONCERN. Note: the interaction between findings is significant — a 24-hour TTL without revocation checking means stolen tokens are valid for up to 24 hours after discovery. Recommendations: (1) reduce TTL to ≤1 hour per ASVS V9.3.1; (2) implement RFC 7662 introspection for sensitive endpoints.

**Reason:** ASVS V9.3.1 guidance gap (24h TTL) = CONCERN + ASVS V9.3.2 guidance gap (no revocation check) = CONCERN. Two CONCERNs do NOT escalate to REPROCESS. Combined verdict = CONCERN.

**Phrasing-B:**
CONCERN. 24-hour JWT TTL: ASVS V9.3.1 "usually 15 min to 1 hour" — guidance only, no MUST (CONCERN). JWT revocation check absent: RFC 7009 no MUST for RS per-request check; ASVS V9.3.2 guidance gap (CONCERN). Two CONCERNs; graduation rule: N CONCERNs ≠ REPROCESS. Combined verdict: CONCERN. Note: 24-hour TTL without revocation compounds the exposure window. Recommend: reduce TTL to ≤1 hour; add RFC 7662 introspection for sensitive paths.

---

## Pair[CB4-3.4-1062]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-oauth-oidc · T-asvs-token
**Domain:** Comprehensive compliant implementation — mTLS binding, prompt handling, PKCE, state, code lifetime, algorithm policy, revocation strategy, client auth — all correct
**Expected verdict:** PASS

**A:**
A production OAuth 2.0 / OIDC deployment is reviewed against the ext40 domain set. Findings: (1) mTLS certificate-bound JWT access tokens: RS extracts TLS client certificate SHA-256 thumbprint on every request and compares it byte-for-byte against `cnf/x5t#S256` in the JWT; mismatches rejected with 401; (2) OIDC prompt=login: AS forces re-authentication when prompt=login received; stale sessions are invalidated; `login_required` returned when interactive auth is not possible; auth_time in id_token reflects new authentication; (3) PKCE: code_challenge and code_challenge_method persisted with authorization code; at token exchange, BASE64URL(SHA256(code_verifier)) compared against stored challenge via constant-time comparison; any mismatch → invalid_grant; (4) State parameter: AS echoes state unmodified in every authorization response where it was present in the request; (5) Authorization code lifetime: 60 seconds; single-use; codes invalidated immediately after exchange or failed attempt; reuse detected and all dependent tokens revoked; (6) id_token algorithms: explicit allowlist (RS256, ES256); alg:none and all non-allowlisted algorithms rejected; (7) JWT revocation: 15-minute access token TTL; standard endpoints use stateless validation; high-sensitivity endpoints use RFC 7662 introspection with TTL-bounded caching; (8) Client authentication: client_secret transmitted via HTTP Basic Authorization header; URI query parameter authentication explicitly rejected at token endpoint.

**B:**
PASS. All ext40 domain requirements satisfied: (1) RFC 8705 §3.1 MUST: cnf/x5t#S256 validated against TLS client cert on every request; (2) OIDC Core §3.1.2.1 MUST: prompt=login honored; login_required returned when re-auth not possible; (3) RFC 7636 §4.4 MUST (×2): code_challenge stored; code_verifier cryptographically verified via constant-time S256 comparison; (4) RFC 6749 §4.1.2 REQUIRED: state echoed unmodified in authorization response; (5) RFC 6749 §10.5 MUST + ASVS V3.5.2: 60-second codes are short-lived, single-use, reuse-detected; (6) RFC 8725 §2.1 MUST NOT: alg:none rejected via explicit allowlist; (7) ASVS V9.3.1/V9.3.2: 15-minute TTL + introspection for sensitive endpoints addresses revocation guidance; (8) RFC 6749 §2.3.1 MUST NOT: client credentials not in URI query parameters. No violations found.

**Reason:** All RFC 8705 §3.1, OIDC Core §3.1.2.1, RFC 7636 §4.4, RFC 6749 §4.1.2, RFC 6749 §10.5, RFC 8725 §2.1, ASVS V9.3, and RFC 6749 §2.3.1 requirements satisfied across all ext40 domains. PASS.

**Phrasing-B:**
PASS. RFC 8705 §3.1 MUST: cert thumbprint validated. OIDC Core §3.1.2.1 MUST: prompt=login honored. RFC 7636 §4.4 MUST ×2: PKCE storage and verification. RFC 6749 §4.1.2 REQUIRED: state echoed. RFC 6749 §10.5 MUST: 60s codes, single-use. RFC 8725 §2.1 MUST NOT: alg:none rejected. ASVS V9.3 guidance: short TTL + introspection. RFC 6749 §2.3.1 MUST NOT: client_secret not in URL. All ext40 requirements met. PASS.
