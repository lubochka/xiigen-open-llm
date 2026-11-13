# CB4 Stage 3 — Cross-Source Pair Set ext32
# Sub-phase 3.4 · Security and Authorization Model
# Sequences CB4-3.4-1027 through CB4-3.4-1036 (10 base pairs × 2 phrasings = 20 instances)
# Source domains: ext39 patterns (jku-injection, client-enumeration-error-codes,
#   OAuth-audit-logging, exp-not-validated-RS) combined with each other and with earlier patterns
# Running total after this file: 120 + (1036 − 60) × 2 = 2072 instances (1036 base pairs)
# Graduation rule: REPROCESS overrides CONCERN; N CONCERNs alone ≠ REPROCESS
# PO.1 escalation: PO.1 absence + traceable production MUST violation = REPROCESS

---

## Pair[CB4-3.4-1027-A]

**Territory:** T-rfc-jwt · T-asvs-token
**Domains combined:** jku-injection (ext39) + exp-not-validated-RS (ext39)
**Expected verdict:** REPROCESS

**A:**
A resource server has two JWT validation failures: (1) it processes the `jku` header parameter and fetches JWKS from any URL found in the token header without allowlist validation (RFC 8725 §3.3 MUST violated); (2) it does not validate the `exp` claim — expired tokens are accepted indefinitely (RFC 9068 §4 MUST violated).

**B:**
REPROCESS. Two independent MUST-level violations at the resource server. First: RFC 8725 §3.3 MUST (twice) — jku must be explicitly allowed and JWKS fetched only from trusted locations; arbitrary jku acceptance enables complete authentication bypass via JWKS injection. Second: RFC 9068 §4 MUST — RS must reject JWT when exp < current time; without exp validation, tokens have unlimited lifetime at the RS regardless of their issued TTL. Both violations independently mandate REPROCESS. The combination is particularly exploitable: an attacker can inject a malicious JWKS via jku (authentication bypass) and the resulting token — even one that was later expired — would continue to work at the RS indefinitely (no exp check). Two independent RS-side validation failures.

**Reason:** RFC 8725 §3.3 MUST ×2 (jku injection) + RFC 9068 §4 MUST (exp not validated). Two independent MUST violations at RS. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8725 §3.3 MUST: arbitrary jku JWKS fetching enables authentication bypass. RFC 9068 §4 MUST: exp not validated means tokens never expire at RS. Two independent RS MUST failures. REPROCESS.

---

## Pair[CB4-3.4-1028-A]

**Territory:** T-rfc-oauth2 · T-asvs-token
**Domains combined:** client-enumeration-error-codes (ext39, REPROCESS) + OAuth-events-not-logged (ext39, CONCERN)
**Expected verdict:** REPROCESS (graduation: REPROCESS overrides CONCERN)

**A:**
The token endpoint returns distinct error codes for unknown client_id vs wrong secret (RFC 6749 §5.2 + CWE-204 violated = REPROCESS). Separately, the authorization server does not log OAuth security events at application level (ASVS V8.3.2 guidance — CONCERN).

**B:**
REPROCESS. Graduation rule applies. The client enumeration violation (RFC 6749 §5.2 + CWE-204) mandates REPROCESS: distinct error responses for unknown vs wrong-credential failures allow unauthenticated attackers to enumerate registered client IDs, violating the RFC 6749 §5.2 error code specification. The audit logging gap (ASVS V8.3.2 guidance) is CONCERN-level: application-level OAuth event logging is guidance, not MUST. Graduation rule: REPROCESS overrides CONCERN. The compound effect is notable: clients can be enumerated (CWE-204) and those enumeration attempts are not logged (no event trail) — but the verdict is REPROCESS on the enumeration violation alone.

**Reason:** RFC 6749 §5.2 + CWE-204 (client enumeration = REPROCESS) + ASVS V8.3.2 guidance (events not logged = CONCERN). Graduation: REPROCESS overrides CONCERN. REPROCESS.

**Phrasing-B:**
REPROCESS. Graduation: REPROCESS + CONCERN = REPROCESS. RFC 6749 §5.2 + CWE-204 violation (client enumeration via error codes) = REPROCESS. ASVS V8.3.2 gap (events not logged) = CONCERN. REPROCESS controls. Fix both: unify error responses; add application-level event logging.

---

## Pair[CB4-3.4-1029-A]

**Territory:** T-rfc-jwt
**Domains combined:** jku-injection (ext39) + iss-absent-RS (ext38)
**Expected verdict:** REPROCESS

**A:**
A resource server processes the `jku` header parameter, fetching JWKS from arbitrary URLs (RFC 8725 §3.3 MUST violated). The same RS also does not validate the `iss` claim — tokens from any issuer are accepted if signature is valid (RFC 9068 §4 MUST violated).

**B:**
REPROCESS. Two independent MUST-level violations at the resource server. First: RFC 8725 §3.3 MUST — jku processing from arbitrary URLs enables JWKS injection authentication bypass. Second: RFC 9068 §4 MUST — iss validation is required; without it, tokens from unauthorized issuers (not just jku-injected ones) are accepted. The combination creates a doubly-unprotected RS: an attacker can inject their own JWKS via jku, and even without jku injection, they could potentially present tokens from any issuer if they obtain legitimate keys from a different AS. Both MUST violations independently mandate REPROCESS.

**Reason:** RFC 8725 §3.3 MUST (jku injection) + RFC 9068 §4 MUST (iss absent). Two independent RS MUST violations. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8725 §3.3: arbitrary jku JWKS fetching = REPROCESS. RFC 9068 §4: iss not validated = REPROCESS. Two independent RS-side MUST failures — one enabling JWKS injection, one enabling unauthorized-issuer token acceptance. REPROCESS.

---

## Pair[CB4-3.4-1030-A]

**Territory:** T-ssdf-process · T-rfc-jwt
**Domains combined:** PO.1-absent + exp-not-validated-RS (ext39) — PO.1 escalation rule
**Expected verdict:** REPROCESS (PO.1 escalation: PO.1 gap + traceable production MUST violation)

**A:**
The development team has no formal SSDF PO.1 security training program. A security audit reveals that the resource server does not validate the `exp` claim in JWT access tokens. The developer, when asked, explains: "I implemented signature verification, iss, aud, and typ checks. I didn't know exp needed to be validated at the RS too — I assumed the short TTL and the client's token refresh mechanism handled expiry."

**B:**
REPROCESS. PO.1 escalation rule applies: PO.1 absence alone is CONCERN; PO.1 absence combined with a traceable production MUST violation is REPROCESS. The developer's explanation directly traces the RFC 9068 §4 MUST violation (exp not validated) to a knowledge gap that PO.1-compliant training would address: understanding that RFC 9068 §4 specifies a required validation sequence that includes exp at the RS, independent of the client's token refresh behavior. The MUST violation itself (exp not validated at RS) is independently REPROCESS-eligible. "Short TTL + client refresh" does not satisfy the RS-side MUST — the RS must enforce expiry regardless of the client's behavior. PO.1 absence is the root cause; the §4 violation is the production evidence with a direct causal link. REPROCESS per escalation rule.

**Reason:** PO.1 escalation: no formal training + RFC 9068 §4 MUST violated (exp not validated) + direct causal link (developer explicitly traces the §4 miss to not knowing RS-side validation requirements). REPROCESS.

**Phrasing-B:**
REPROCESS. PO.1 escalation: no formal security training (PO.1 absent — CONCERN) + RFC 9068 §4 MUST violated (exp not validated at RS) + developer's explicit causal statement ("I didn't know exp needed validation at the RS"). PO.1 + traceable MUST violation = REPROCESS.

---

## Pair[CB4-3.4-1031-A]

**Territory:** T-rfc-jwt
**Domains combined:** exp-not-validated-RS (ext39) + JWT-typ-absent (ext36)
**Expected verdict:** REPROCESS

**A:**
A resource server skips both `exp` and `typ` claim validation in its JWT validation pipeline. Expired tokens are accepted (RFC 9068 §4 MUST violated). JWT access tokens also lack `typ: at+JWT` headers and the RS does not check typ (RFC 9068 §2.1 MUST violated).

**B:**
REPROCESS. Two independent MUST violations from RFC 9068, the JWT access token profile. First: RFC 9068 §4 MUST — exp must be validated; skipping exp means tokens never expire at the RS. Second: RFC 9068 §2.1 MUST — JWT access tokens must carry `typ: at+JWT`; the RS not checking typ enables token confusion (ID token presented as access token). Both violations are from the same RFC 9068 access token profile — the RS has implemented an incomplete subset of the required validation steps. Together: expired tokens are accepted, and so are wrong-type tokens. Both violations independently mandate REPROCESS.

**Reason:** RFC 9068 §4 MUST (exp not validated) + RFC 9068 §2.1 MUST (typ absent). Two RFC 9068 MUST violations at RS. REPROCESS.

**Phrasing-B:**
REPROCESS. Two RFC 9068 MUST violations: §4 MUST (exp check absent — tokens never expire) + §2.1 MUST (typ check absent — token confusion possible). RS implements an incomplete RFC 9068 validation chain. Both MUST violations independently = REPROCESS.

---

## Pair[CB4-3.4-1032-A]

**Territory:** T-asvs-token · T-rfc-jwt
**Domains combined:** OAuth-events-not-logged (ext39, CONCERN) + wildcard-aud (ext37, CONCERN)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
An authorization server does not log OAuth security events at application level (ASVS V8.3.2 guidance — CONCERN). JWT access tokens use wildcard `aud="*.example.com"` instead of per-RS specific identifiers (RFC 8725 §3.9 SHOULD — CONCERN).

**B:**
CONCERN. Two SHOULD-level gaps are present. First: ASVS V8.3.2 guidance — OAuth security events not logged at application level; forensic investigation capability is limited. CONCERN. Second: RFC 8725 §3.9 — wildcard aud defeats per-RS audience isolation; SHOULD-level guidance. CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. No MUST violations present. The combined effect: an attacker exploiting the audience-unrestricted tokens (wildcard aud accepts tokens at any RS) leaves no application-level event trail (events not logged). However, both gaps are SHOULD-level; the verdict is CONCERN. Both should be remediated: add application-level event logging; adopt per-RS aud via RFC 8707.

**Reason:** ASVS V8.3.2 guidance (events not logged = CONCERN) + RFC 8725 §3.9 SHOULD (wildcard aud = CONCERN). Both SHOULD-level. Graduation: N CONCERNs = CONCERN. CONCERN.

**Phrasing-B:**
CONCERN. ASVS V8.3.2 guidance (events not logged) = CONCERN. RFC 8725 §3.9 SHOULD (wildcard aud) = CONCERN. Graduation: two CONCERNs = CONCERN. No MUST violations. Both should be fixed. Verdict: CONCERN.

---

## Pair[CB4-3.4-1033-A]

**Territory:** T-rfc-oauth2
**Domains combined:** client-enumeration-error-codes (ext39) + token-type-absent (ext35)
**Expected verdict:** REPROCESS

**A:**
The token endpoint returns distinct error codes for unknown client_id vs wrong secret (RFC 6749 §5.2 + CWE-204 violated). Additionally, successful token responses omit the `token_type` field (RFC 6749 §5.1 MUST violated).

**B:**
REPROCESS. Two independent RFC 6749 MUST violations at the token endpoint. First: RFC 6749 §5.2 — all authentication failures must return `invalid_client`; distinct codes enable client ID enumeration (CWE-204). Second: RFC 6749 §5.1 MUST — `token_type` must be present in successful token responses; the field enables programmatic determination of the Authorization header scheme. The token endpoint violates RFC 6749 on both the error path (§5.2) and the success path (§5.1) simultaneously. Each violation independently mandates REPROCESS.

**Reason:** RFC 6749 §5.2 + CWE-204 (client enumeration = REPROCESS) + RFC 6749 §5.1 MUST (token_type absent = REPROCESS). Two RFC 6749 violations on error and success paths. REPROCESS.

**Phrasing-B:**
REPROCESS. Token endpoint violates RFC 6749 on both paths: §5.2 error path (distinct error codes enable client enumeration, CWE-204) + §5.1 success path (token_type absent from response). Two independent RFC 6749 MUST violations. REPROCESS.

---

## Pair[CB4-3.4-1034-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2
**Domains combined:** jku-injection (ext39) + DPoP-bound-token-as-Bearer (ext36)
**Expected verdict:** REPROCESS

**A:**
A resource server processes the `jku` header parameter without allowlist validation (RFC 8725 §3.3 MUST violated). The same RS also accepts DPoP-bound access tokens (with `cnf/jkt` claims) as plain Bearer tokens without requiring DPoP proof (RFC 9449 §7.2 MUST violated).

**B:**
REPROCESS. Two independent MUST-level violations at the resource server. First: RFC 8725 §3.3 MUST — arbitrary jku processing enables JWKS injection authentication bypass. Second: RFC 9449 §7.2 MUST (twice) — RS must validate DPoP proof when cnf/jkt is present; accepting DPoP-bound tokens as plain Bearer converts sender-constrained tokens to unconstrained ones. Both violations independently mandate REPROCESS. The combination creates a doubly-compromised RS: signature verification can be bypassed via jku injection, and sender-constraint enforcement is absent. An attacker who obtains a DPoP-bound token can use it without the DPoP key, and an attacker who crafts a jku-bearing token can bypass signature verification entirely.

**Reason:** RFC 8725 §3.3 MUST (jku injection) + RFC 9449 §7.2 MUST ×2 (DPoP not enforced). Two independent RS MUST failures. REPROCESS.

**Phrasing-B:**
REPROCESS. RFC 8725 §3.3: arbitrary jku = authentication bypass via JWKS injection. RFC 9449 §7.2: DPoP-bound tokens accepted as Bearer = sender-constraint stripped. Two independent RS MUST violations. REPROCESS.

---

## Pair[CB4-3.4-1035-A]

**Territory:** T-asvs-token
**Domains combined:** OAuth-events-not-logged (ext39, CONCERN) + access-token-TTL-24h (ext38, CONCERN)
**Expected verdict:** CONCERN (graduation: two CONCERNs do not escalate to REPROCESS)

**A:**
An authorization server does not log OAuth security events at application level (ASVS V8.3.2 guidance — CONCERN). Access tokens have a 24-hour TTL, substantially exceeding ASVS V9.3.1's guidance range of 15 minutes to 1 hour (CONCERN).

**B:**
CONCERN. Two SHOULD-level gaps are present. First: ASVS V8.3.2 guidance — OAuth events not logged; forensic investigation compromised. CONCERN. Second: ASVS V9.3.1 guidance — 24-hour access token TTL exceeds the guidance range; extended exploitation window for exfiltrated tokens. CONCERN. Graduation rule: two CONCERNs do not escalate to REPROCESS. No MUST violations present. The compound effect is noteworthy: a 24-hour exfiltrated token exploitation window combined with no event logging means an attacker using a stolen token generates no detectable signal at the application layer. Both gaps are SHOULD-level. Verdict: CONCERN with heightened urgency.

**Reason:** ASVS V8.3.2 guidance (events not logged = CONCERN) + ASVS V9.3.1 guidance (24h TTL = CONCERN). Both SHOULD-level. Graduation: N CONCERNs = CONCERN. CONCERN.

**Phrasing-B:**
CONCERN. Two ASVS guidance gaps: V8.3.2 events not logged (CONCERN) + V9.3.1 24h access token TTL (CONCERN). Graduation: two CONCERNs = CONCERN. Combined: 24-hour stolen-token exploitation window with no application-level detection capability — but both are SHOULD gaps. Verdict: CONCERN.

---

## Pair[CB4-3.4-1036-A]

**Territory:** T-rfc-jwt · T-rfc-oauth2 · T-asvs-token
**Domains combined:** comprehensive PASS — jku ignored + uniform error + events logged + exp validated
**Expected verdict:** PASS

**A:**
A resource server and authorization server satisfy all four ext39 PASS conditions simultaneously. (1) RS ignores `jku`, `jwk`, and `x5u` JOSE header parameters; JWKS fetched only from deployment-configured `https://auth.example.com/.well-known/jwks.json`. (2) Token endpoint returns `{"error":"invalid_client"}` for all authentication failures regardless of whether the client_id is registered or the secret is wrong; specific failure reason logged internally. (3) Authorization server logs all OAuth security events at application level in structured JSON: failed authentication, redirect_uri mismatches, state mismatches, invalid code submissions — excluding credentials and PII. (4) Resource server validates exp claim as step 4 in the validation pipeline; expired tokens receive 401.

**B:**
PASS. No arbiter violations found. RFC 8725 §3.3 satisfied: jku/jwk/x5u ignored; JWKS from trusted pre-configured endpoint only; JWKS injection attack vector eliminated. RFC 6749 §5.2 satisfied: uniform invalid_client response; CWE-204 mitigated; no client enumeration possible. ASVS V8.3.2 satisfied: structured OAuth security event logging with forensic detail; V8.2.2 satisfied (credentials/PII excluded). RFC 9068 §4 MUST satisfied: exp validated at RS; expired tokens rejected with 401. All four ext39 PASS conditions simultaneously achieved.

**Reason:** RFC 8725 §3.3, RFC 6749 §5.2, ASVS V8.3.2, RFC 9068 §4 — all satisfied. No violations at any layer. PASS.

**Phrasing-B:**
PASS. Four ext39 PASS conditions simultaneously: (1) jku/x5u ignored per RFC 8725 §3.3; (2) uniform invalid_client per RFC 6749 §5.2; (3) OAuth events logged per ASVS V8.3.2; (4) exp validated per RFC 9068 §4. No MUST, MUST NOT, or guidance violations found. PASS.
