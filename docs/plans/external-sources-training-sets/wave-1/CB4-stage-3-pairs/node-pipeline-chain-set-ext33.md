# CB4 Stage 3 — Node Pipeline Chain Set ext33
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1019 through CB4-3.4-1026 (8 base pairs × 2 phrasings = 16 instances)
# Full 9-node pipeline traces for all 8 ext39 arbiter patterns
# Running total after this file: 120 + (1026 − 60) × 2 = 2052 instances (1026 base pairs)
# Domains: jku-injection, client-enumeration-error-codes, OAuth-audit-logging, exp-not-validated-RS

---

## Pair[CB4-3.4-1019-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS fetches JWKS from `jku` header without allowlist — JWKS injection attack
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: Resource server JWT validation — JOSE header parameter handling. Security sub-domain: jku header parameter, RFC 8725 §3.3. Routed to JWT header parameter security branch.

[Node 2 — Domain Translator] Standard terminology: `jku` (JWK Set URL) is a JOSE header parameter that specifies a URL from which the RS can fetch the key set used to verify the token's signature. RFC 8725 §3.3 imposes restrictions on when and how this parameter may be used. Input: RS dynamically fetches JWKS from any URL found in the jku header without allowlist validation.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 8725 §3.3 MUST requirements for jku. (2) Analyze jku injection attack. (3) Evaluate "spec-compliant" argument. (4) Determine verdict severity.

[Node 4 — Domain Processor] RFC 8725 §3.3: "The `jku` and `x5u` Header Parameter values, if present, MUST be explicitly allowed by the application ... and their contents MUST be fetched only from locations that are explicitly trusted." Two MUST requirements. Attack vector: attacker creates token with jku pointing to attacker-controlled server; AS's signing key pair is separate; attacker generates their own key pair; publishes public key at their server; signs token with their private key; RS fetches attacker's public key; validates attacker's token.

[Node 5 — Applier] Input: RS fetches from arbitrary jku. Neither MUST is satisfied: jku not explicitly allowed by application; JWKS fetched from location that is not explicitly trusted. Both RFC 8725 §3.3 MUSTs violated. "Spec-compliant" argument evaluated: RFC 8725 §3.3 prohibits passive jku acceptance — the application MUST explicitly allow jku use and restrict fetching to trusted locations.

[Node 6 — Result Translator] Finding: RFC 8725 §3.3 MUST (×2) violated. jku injection attack vector present. Authentication bypass possible. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 8725 §3.3. MUST (×2). Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 8725 §3.3 MUST (twice): jku must be explicitly allowed; JWKS fetched only from explicitly trusted locations. RS accepting arbitrary jku values enables complete authentication bypass via JWKS injection.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 8725 §3.3 | Normative level: MUST (×2) | Gap: jku header followed without allowlist; arbitrary JWKS URL trusted | Remediation: Ignore jku/jwk/x5u headers; use only deployment-configured JWKS endpoint.

**B:**
REPROCESS. RFC 8725 §3.3 MUST (twice): jku must be explicitly allowed by the application and JWKS fetched only from explicitly trusted locations. Dynamically following arbitrary jku header values violates both MUSTs and enables complete authentication bypass: attacker provides their own JWKS URL, RS validates attacker-signed tokens. Remediation: ignore jku; use deployment-configured JWKS endpoint.

**Reason:** RFC 8725 §3.3 MUST (×2) violated. jku injection authentication bypass. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8725 §3.3: jku must be explicitly allowed + JWKS only from trusted locations. RS following arbitrary jku values enables JWKS injection: attacker serves own keys at attacker-controlled URL, RS validates attacker's tokens. Both MUSTs violated. REPROCESS.

---

## Pair[CB4-3.4-1020-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS ignores `jku` header — uses only deployment-configured JWKS
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: RS JWT validation — dangerous JOSE header parameter handling.

[Node 2 — Domain Translator] Standard terminology: RS strips jku, jwk, x5u from consideration. Validates using only pre-configured JWKS URL from deployment config.

[Node 3 — Scheme Planner] Validation plan: (1) Verify jku/jwk/x5u not processed. (2) Verify JWKS from trusted pre-configured source. (3) Render verdict.

[Node 4 — Domain Processor] RFC 8725 §3.3: jku/x5u must be from explicitly trusted locations. RS ignoring these parameters satisfies the requirement by not processing them at all. Pre-configured JWKS URL is explicitly trusted at deployment. All requirements satisfied.

[Node 5 — Applier] No violations. PASS.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 8725 §3.3. No violations.

[Node 8 — Arbiter] PASS. RFC 8725 §3.3 satisfied: jku/jwk/x5u headers ignored; JWKS from deployment-configured trusted endpoint only.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 8725 §3.3 | jku/x5u headers ignored; trusted JWKS only | No violations.

**B:**
PASS. RFC 8725 §3.3 satisfied: RS ignores jku, jwk, and x5u JOSE header parameters; JWKS fetched only from deployment-configured trusted endpoint. JWKS injection attack vector eliminated. No violations found.

**Reason:** RFC 8725 §3.3 satisfied. Dangerous header parameters ignored. Trusted JWKS source only. PASS.

**Phrasing-B:**
PASS. RS ignores jku/jwk/x5u; uses only deployment-configured JWKS URL. RFC 8725 §3.3 satisfied by design: no untrusted JWKS fetching possible. PASS.

---

## Pair[CB4-3.4-1021-A]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** Token endpoint distinguishes unknown client_id from wrong secret — client enumeration
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: Token endpoint error response for client authentication failure. Security sub-domain: client authentication error enumeration, RFC 6749 §5.2 + CWE-204.

[Node 2 — Domain Translator] Standard terminology: RFC 6749 §5.2 specifies error codes for failed client authentication. CWE-204 addresses observable response discrepancy that enables information disclosure. Input: distinct error codes for "client_id not found" vs "wrong client_secret."

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 6749 §5.2 error code requirements. (2) Assess enumeration attack. (3) Determine verdict.

[Node 4 — Domain Processor] RFC 6749 §5.2: failed client authentication → "invalid_client." No distinct code for unknown vs wrong credential. CWE-204: distinct responses for different failure types disclose system state (whether client_id is registered) to unauthenticated attackers. A registered client_id + wrong secret → different error from unregistered client_id → attacker can enumerate registered clients.

[Node 5 — Applier] Input: distinct "unknown_client" vs "invalid_client" responses. RFC 6749 §5.2 violated (custom error code not in spec). CWE-204 applicable. Client enumeration enabled.

[Node 6 — Result Translator] Finding: RFC 6749 §5.2 violated. CWE-204 applies. Client enumeration enabled. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter (RFC 6749 §5.2) + CWE-TOP25 arbiter (CWE-204). Primary: RFC 6749 §5.2. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 6749 §5.2: all authentication failures must return invalid_client; no distinct unknown_client code. CWE-204: observable discrepancy enables client ID enumeration. Remediation: unified invalid_client response.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standards: RFC 6749 §5.2, CWE-204 | Gap: client enumeration via distinct error codes | Remediation: Return uniform invalid_client for all authentication failures.

**B:**
REPROCESS. RFC 6749 §5.2: client authentication failures must return invalid_client; a custom unknown_client error for unregistered IDs violates the error code specification. CWE-204: the distinct responses allow unauthenticated clients to enumerate registered client IDs. Remediation: unify error responses; log details internally.

**Reason:** RFC 6749 §5.2 violated + CWE-204 client enumeration. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 6749 §5.2 provides one error code for failed auth: invalid_client. Custom unknown_client code is spec-non-compliant and enables client ID enumeration (CWE-204). REPROCESS — unify error response.

---

## Pair[CB4-3.4-1022-A]

**Territory:** T-rfc-oauth2 · T-cwe-authn
**Domain:** Token endpoint returns uniform `invalid_client` — no enumeration possible
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: Token endpoint authentication error response uniformity.

[Node 2 — Domain Translator] Standard terminology: all authentication failures (unknown client_id, wrong secret, method mismatch) → {"error":"invalid_client"}. Internal logging records specific failure reason. HTTP 401.

[Node 3 — Scheme Planner] Validation plan: (1) Verify uniform error response. (2) Verify internal logging. (3) Render verdict.

[Node 4 — Domain Processor] RFC 6749 §5.2: invalid_client for all auth failures. CWE-204 mitigated: no distinguishable response. Internal logging maintains operator visibility.

[Node 5 — Applier] No violations. PASS.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 6749 §5.2. No violations.

[Node 8 — Arbiter] PASS. RFC 6749 §5.2 satisfied: uniform invalid_client error. CWE-204 mitigated. Internal logging provides operator visibility without exposing to callers.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 6749 §5.2 | Uniform invalid_client; CWE-204 mitigated | No violations.

**B:**
PASS. RFC 6749 §5.2 satisfied: uniform invalid_client for all client authentication failures regardless of failure type. CWE-204 mitigated: no client ID enumeration possible. Specific failure reason logged internally. No violations found.

**Reason:** RFC 6749 §5.2 satisfied. Uniform error prevents enumeration. CWE-204 mitigated. PASS.

**Phrasing-B:**
PASS. All authentication failures return invalid_client per RFC 6749 §5.2. No response discrepancy → CWE-204 mitigated. Internal logging maintains forensic capability. PASS.

---

## Pair[CB4-3.4-1023-A]

**Territory:** T-asvs-token · T-oauth-oidc
**Domain:** OAuth security events not logged at application level
**Pipeline verdict:** CONCERN

**A:**
[Node 1 — Domain Splitter] Input classified: Authorization server audit logging configuration. Security sub-domain: OAuth security event logging, ASVS V8.3.2.

[Node 2 — Domain Translator] Standard terminology: ASVS V8.3.2 requires security event logs sufficient for forensic investigation. OAuth-relevant events: failed client authentication, redirect_uri mismatch, state mismatch, invalid code submission. Input: no application-level OAuth event logging; only network-layer access logs available.

[Node 3 — Scheme Planner] Validation plan: (1) Identify ASVS V8.3.2 guidance. (2) Assess network-layer logs adequacy. (3) Determine severity.

[Node 4 — Domain Processor] ASVS V8.3.2 guidance: security events must be logged with forensic detail. Network-layer logs miss POST body parameters (client_id, state values) essential for correlation. No RFC MUST-level OAuth event logging requirement. Gap is ASVS guidance-level.

[Node 5 — Applier] Application-level OAuth events not logged. Network logs insufficient for OAuth forensics. ASVS V8.3.2 guidance not met. No MUST violation. Verdict: CONCERN.

[Node 6 — Result Translator] Finding: ASVS V8.3.2 guidance not met. Guidance-level gap. Verdict: CONCERN.

[Node 7 — Arbiter Selector] OWASP-ASVS arbiter. ASVS V8.3.2. Guidance-level. Verdict: CONCERN.

[Node 8 — Arbiter] CONCERN. ASVS V8.3.2: security event logs should capture forensic detail. OAuth security events not logged at application level; network-layer logs insufficient. Guidance-level gap = CONCERN.

[Node 9 — Formatter] Verdict: **CONCERN** | Standard: ASVS V8.3.2 | Normative level: Guidance | Gap: OAuth security events not logged at application level | Recommendation: Add structured application-level logging for failed auth, redirect_uri mismatch, state mismatch events (excluding credentials/PII).

**B:**
CONCERN. ASVS V8.3.2 guidance: security event log must capture forensic detail. OAuth events (failed auth, redirect_uri mismatch, state mismatch) are not logged at application level; network-layer logs lack the POST body content needed for correlation. No RFC MUST-level logging requirement. Guidance gap = CONCERN. Recommendation: structured application-level logging of security events, excluding credentials and PII.

**Reason:** ASVS V8.3.2 guidance not met. Guidance-level gap. CONCERN.

**Phrasing-B:**
CONCERN. ASVS V8.3.2: security events must be logged for forensic investigation. OAuth security events absent from application-level log. Network logs insufficient. No RFC MUST. Guidance gap = CONCERN.

---

## Pair[CB4-3.4-1024-A]

**Territory:** T-asvs-token · T-oauth-oidc
**Domain:** OAuth security events logged with forensic detail — ASVS V8.3.2 satisfied
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: AS security event logging implementation.

[Node 2 — Domain Translator] Standard terminology: structured JSON event log captures: failed client auth (event_type, client_id, IP, timestamp), redirect_uri mismatches (client_id, submitted/registered URIs), state mismatches (session context), invalid codes (client_id, code_hash, IP). Credentials and PII excluded.

[Node 3 — Scheme Planner] Validation plan: (1) Verify ASVS V8.3.2 guidance met. (2) Verify credential/PII exclusion. (3) Render verdict.

[Node 4 — Domain Processor] ASVS V8.3.2: security events logged with forensic detail. V8.2.2: credentials/PII excluded. All OAuth security events captured. Guidance satisfied.

[Node 5 — Applier] No violations. PASS.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] OWASP-ASVS arbiter. ASVS V8.3.2. No violations.

[Node 8 — Arbiter] PASS. ASVS V8.3.2 satisfied: structured logging of OAuth security events with forensic detail. V8.2.2 satisfied: credentials/PII excluded.

[Node 9 — Formatter] Verdict: **PASS** | Standards: ASVS V8.3.2, V8.2.2 | OAuth security events logged; credentials/PII excluded | No violations.

**B:**
PASS. ASVS V8.3.2 satisfied: application-level structured logging captures all OAuth security events with forensic detail (event_type, client_id, timestamp, IP, outcome). ASVS V8.2.2 satisfied: credentials and PII excluded from log payloads. No violations found.

**Reason:** ASVS V8.3.2 and V8.2.2 satisfied. OAuth security event logging implemented correctly. PASS.

**Phrasing-B:**
PASS. Application-level JSON logging for OAuth security events: failed auth, redirect_uri mismatch, state mismatch, invalid code — with forensic detail. Credentials/PII excluded. ASVS V8.3.2 and V8.2.2 satisfied. PASS.

---

## Pair[CB4-3.4-1025-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS does not validate `exp` claim — expired tokens accepted indefinitely
**Pipeline verdict:** REPROCESS

**A:**
[Node 1 — Domain Splitter] Input classified: Resource server JWT validation — exp claim handling. Security sub-domain: expiry validation, RFC 9068 §4. Routed to JWT claim validation branch.

[Node 2 — Domain Translator] Standard terminology: the `exp` (expiration time) claim defines the time after which a JWT MUST NOT be accepted for processing. RFC 9068 §4 requires the RS to validate exp. Input: RS validates sig, iss, aud, typ, scope but does not check exp; expired tokens are accepted.

[Node 3 — Scheme Planner] Validation plan: (1) Identify RFC 9068 §4 MUST for exp. (2) Evaluate "UX preference" argument. (3) Determine verdict.

[Node 4 — Domain Processor] RFC 9068 §4: "If the JWT has an `exp` claim, the JWT MUST be rejected if the current Unix time is greater than or equal to the value of that claim." MUST. The exp claim defines the access token's bounded lifecycle. Without exp validation: a 15-minute access token becomes perpetually valid at the RS — the issued TTL has no practical effect on the RS's acceptance decision.

[Node 5 — Applier] Input: RS skips exp check. Expired tokens accepted indefinitely. Direct RFC 9068 §4 MUST violation. "UX graceful acceptance" argument rejected: MUST is unconditional; correct UX is client refresh on 401.

[Node 6 — Result Translator] Finding: RFC 9068 §4 MUST violated. exp not validated. Tokens never expire at RS. Verdict: REPROCESS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §4. MUST. Verdict: REPROCESS.

[Node 8 — Arbiter] REPROCESS. RFC 9068 §4 MUST: RS must reject JWT when exp < current time. Skipping exp validation means access tokens never expire at the RS regardless of issued TTL. REPROCESS.

[Node 9 — Formatter] Verdict: **REPROCESS** | Standard: RFC 9068 §4 | Normative level: MUST | Gap: exp not validated; tokens accepted after expiry | Remediation: Assert exp > current_time in validation pipeline; reject expired tokens with 401.

**B:**
REPROCESS. RFC 9068 §4 MUST: RS must reject JWT when exp < current Unix time. Without exp validation, tokens issued with a 15-minute TTL are valid forever at the RS. "Graceful UX" does not override MUST; 401 responses trigger client-side token refresh. REPROCESS.

**Reason:** RFC 9068 §4 MUST violated. exp check absent. Tokens perpetually valid at RS. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 9068 §4 MUST: exp validated. Skipping exp check means the access token's intended TTL is never enforced at the RS — stolen token exploitable indefinitely. MUST is unconditional. REPROCESS.

---

## Pair[CB4-3.4-1026-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domain:** RS validates `exp` claim; expired tokens rejected with 401
**Pipeline verdict:** PASS

**A:**
[Node 1 — Domain Splitter] Input classified: RS exp claim validation.

[Node 2 — Domain Translator] Standard terminology: RS validation step 4: assert exp > current_time. Expired token → 401. Client handles 401 with token refresh.

[Node 3 — Scheme Planner] Validation plan: (1) Verify exp check present. (2) Verify 401 response. (3) Render verdict.

[Node 4 — Domain Processor] RFC 9068 §4 MUST: exp validated. exp > current_time asserted. Expired → 401. Correct. No clock skew tolerance needed for exp ceiling (conservative is acceptable).

[Node 5 — Applier] No violations. PASS.

[Node 6 — Result Translator] No violations. PASS.

[Node 7 — Arbiter Selector] IETF-RFCS arbiter. RFC 9068 §4. No violations.

[Node 8 — Arbiter] PASS. RFC 9068 §4 MUST satisfied: exp validated; expired tokens rejected with 401. Token lifecycle correctly enforced at RS.

[Node 9 — Formatter] Verdict: **PASS** | Standard: RFC 9068 §4 | exp validated; expired tokens rejected | No violations.

**B:**
PASS. RFC 9068 §4 MUST satisfied: RS validates exp claim; tokens with exp < current_time rejected with 401. Token lifecycle enforcement operational at RS — 15-minute TTL is actually enforced at the RS, not just issued. Client handles 401 with refresh. No violations found.

**Reason:** RFC 9068 §4 MUST satisfied. exp validated. Token lifecycle enforced. PASS.

**Phrasing-B:**
PASS. RS asserts exp > current_time per RFC 9068 §4 MUST. Expired tokens → 401 → client refresh. Token TTL actually enforced at RS. PASS.
