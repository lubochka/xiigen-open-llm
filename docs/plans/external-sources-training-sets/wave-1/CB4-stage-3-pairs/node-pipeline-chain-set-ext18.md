# CB4 Stage 3 — Multi-Node Pipeline Chain Set ext18
# Sequences CB4-3.4-629 through CB4-3.4-636
# Stratum C — full 9-node pipeline traces for ext24 patterns
# Running total after this file: 1272 instances (636 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-629]

**A**
```
Incoming pipeline input:
"Our API clients pass access tokens as URL query parameters (?access_token=...).
The API gateway logs all requests at DEBUG level including full URLs.
Developer says: RFC 6750 §2.3 defines query parameter transmission, so it's supported."
```

**B**
```
Pipeline trace — CB4-3.4-629

Node 1 — Domain Splitter:
  Input: access token URL transmission + full-URL DEBUG logging
  Domains identified: T-rfc-oauth2 (RFC 6750 transmission method) + T-asvs-token (V9.3.1 logging)
  Sub-phase assignment: 3.4 (token security)
  Output: [T-rfc-oauth2: query-param transmission] + [T-asvs-token: credential logging]

Node 2 — Domain Translator:
  T-rfc-oauth2: RFC 6750 §2.3 — query parameter method; SHOULD NOT; reasons: URL logging,
    browser history, referrer headers
  T-asvs-token: V9.3.1 — MUST NOT log credential-equivalent tokens in production

Node 3 — Scheme Planner:
  Check 1: Is query-param transmission the method used? → YES
  Check 2: Are tokens logged at any level that persists in production? → YES (DEBUG)
  Check 3: Does developer argument neutralize the violation? → NO (SHOULD NOT + MUST NOT combined)
  Plan: apply RFC 6750 §2.3 SHOULD NOT + V9.3.1 MUST NOT; elevation rule

Node 4 — Domain Processor:
  RFC 6750 §2.3: SHOULD NOT — not a MUST violation alone
  V9.3.1: DEBUG logging of full URLs containing access tokens in production → MUST NOT → REPROCESS
  Developer argument "RFC 6750 defines it" → true; SHOULD NOT ≠ permitted with no risk
  Combined: SHOULD NOT transmission puts tokens in URLs; MUST NOT logging captures them → REPROCESS

Node 5 — Applier:
  Applies RFC 6750 §2.3 SHOULD NOT (transmission) and V9.3.1 MUST NOT (logging)
  Verdict elevation: MUST NOT present → REPROCESS regardless of SHOULD NOT component

Node 6 — Result Translator:
  Primary violation: V9.3.1 MUST NOT (token logging at DEBUG/production)
  Secondary: RFC 6750 §2.3 SHOULD NOT (query param transmission)
  Combined produces REPROCESS; neither alone would necessarily trigger REPROCESS if isolated

Node 7 — Arbiter Selector:
  Controlling arbiter: T-asvs-token (V9.3.1) + T-rfc-oauth2 (RFC 6750)
  Primary finding: credential-equivalent token in production logs

Node 8 — Arbiter:
  REPROCESS — Access tokens transmitted via URL query parameters with full-URL DEBUG
  logging in production violates V9.3.1 (MUST NOT log credential-equivalent tokens).
  RFC 6750 §2.3 marks query-parameter transmission SHOULD NOT precisely because it
  causes URL logging. "Defined in spec" does not mean "safe to use with debug logging."
  Remediate: use Authorization: Bearer header (RFC 6750 §2.1); disable full-URL logging.

Node 9 — Formatter:
  REPROCESS
  Primary: V9.3.1 MUST NOT — token in production DEBUG logs
  Secondary: RFC 6750 §2.3 SHOULD NOT — query-parameter transmission method
  Remediation: Bearer header + disable full-URL logging
```

**Reason**
Full 9-node trace for the access-token-in-query-string pattern. V9.3.1 MUST NOT (logging) combined with RFC 6750 §2.3 SHOULD NOT (transmission) produces REPROCESS. The pipeline trace shows how Node 4 processes each standard independently before Node 5 applies the elevation rule.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-629 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 6750 §2.3) + T-asvs-token (V9.3.1)
Node 3: Plan — query param in URL → logged at DEBUG → V9.3.1 MUST NOT triggered
Node 4: RFC 6750 §2.3 SHOULD NOT + V9.3.1 MUST NOT → REPROCESS
Node 5: elevation rule: MUST NOT present → REPROCESS
Node 8 — Arbiter: REPROCESS — URL query parameter transmission combined with DEBUG
  full-URL logging violates V9.3.1. Use Bearer header; disable full-URL logging.
Node 9: REPROCESS; V9.3.1 primary; RFC 6750 §2.3 secondary
```

---

## Pair[CB4-3.4-630]

**A**
```
Incoming pipeline input:
"Authorization server returns detailed error responses for debugging:
error_description includes the attempted client_id, lists all registered client IDs
for validation, and includes the caller's IP address. Developer says it helps operators
diagnose misconfigurations faster."
```

**B**
```
Pipeline trace — CB4-3.4-630

Node 1 — Domain Splitter:
  Input: OAuth error response content
  Domains: T-rfc-oauth2 (RFC 6749 §5.2 error response) + T-asvs-authorization (V8.2.2)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 6749 §5.2: error response MUST include "error" code; SHOULD include error_description
    as human-readable text; no requirement or permission to enumerate valid identifiers
  V8.2.2: sensitive data MUST NOT be included in server responses unnecessarily

Node 3 — Scheme Planner:
  Check 1: Does the error response include enumeration of valid client IDs? → YES
  Check 2: Is this required by RFC 6749? → NO
  Check 3: Does it constitute information disclosure? → YES (CWE-200)
  Plan: minimum-disclosure assessment; CWE-200 classification

Node 4 — Domain Processor:
  "Attempted client_id" in response: reveals the attacker's probe was recognized → minor
  "List all registered client IDs" → complete registry enumeration → REPROCESS
    Any caller can obtain full client inventory from a single failed request
    No authentication required; reconnaissance payload on unauthenticated endpoint
  "Caller IP" → implementation-specific concern; IP in API error responses is non-standard
  "Helps operators" → operational argument does not override CWE-200 disclosure
    Operator diagnostics should be in server-side logs, not in API responses

Node 5 — Applier:
  Enumeration of registered client IDs → REPROCESS (CWE-200 + V8.2.2)
  Remaining elements elevated by enumeration finding

Node 6 — Result Translator:
  Primary violation: client registry enumeration in error_description
  CWE-200 (information disclosure) + V8.2.2 (sensitive data in responses)

Node 7 — Arbiter Selector:
  T-rfc-oauth2 + T-asvs-authorization — error response information disclosure

Node 8 — Arbiter:
  REPROCESS — Error response discloses the full registered client ID list to any caller.
  RFC 6749 §5.2 does not require or permit client enumeration. This enables complete
  client registry reconnaissance from a single unauthenticated failed request. CWE-200.
  Remediate: error_description should contain only "client authentication failed."
  Operator diagnostics belong in server-side logs with appropriate access controls.

Node 9 — Formatter:
  REPROCESS
  Primary: CWE-200 — client registry enumeration in error_description
  Secondary: V8.2.2 — sensitive operational data in API response
  Remediation: minimum-disclosure error_description; diagnostics to server logs
```

**Reason**
Full pipeline trace for OAuth error response information leakage. Node 4 separates three elements (attempted client_id, registry enumeration, IP) and identifies the registry enumeration as the REPROCESS trigger (CWE-200). The operational argument is evaluated and rejected in Node 4: diagnostics belong in server logs, not API responses.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-630 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 6749 §5.2) + T-asvs-authorization (V8.2.2 / CWE-200)
Node 4: enumeration of all registered client IDs → complete registry disclosure → REPROCESS
  "Helps operators" → diagnostics in server logs, not API responses
Node 5: REPROCESS (CWE-200 + V8.2.2)
Node 8 — Arbiter: REPROCESS — Client registry enumeration in error_description is
  CWE-200. Replace with generic failure message; diagnostics in server logs only.
Node 9: REPROCESS; client enumeration primary; V8.2.2 secondary
```

---

## Pair[CB4-3.4-631]

**A**
```
Incoming pipeline input:
"Device authorization flow implementation: 5-character [A-Z0-9] user codes, 30-minute
device_code lifetime. No server-side rate limiting on the verification endpoint —
we rely on the RFC 8628 slow_down response to tell clients to back off.
Developer: slow_down is the RFC-defined mechanism for preventing abuse."
```

**B**
```
Pipeline trace — CB4-3.4-631

Node 1 — Domain Splitter:
  Input: device authorization flow brute-force protection
  Domain: T-rfc-oauth2 (RFC 8628 device authorization grant)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8628 §6.1: server MUST consider brute-force protection
    Two controls: (1) sufficient user_code entropy; (2) server-side rate limiting
  slow_down: §3.5 — server response to indicate client should increase polling interval
    designed for polite clients; not a server-side enforcement mechanism

Node 3 — Scheme Planner:
  Check 1: user_code entropy → 36^5 = 60.4M codes; 30-min lifetime
  Check 2: server-side rate limiting present? → NO
  Check 3: slow_down substitutes for server-side limiting? → assess
  Plan: RFC 8628 §6.1 MUST assessment; slow_down argument evaluation

Node 4 — Domain Processor:
  user_code: 5-char [A-Z0-9] → 36^5 ≈ 60M codes; moderate entropy but not high
  No server-side rate limiting: RFC 8628 §6.1 MUST → absent → REPROCESS
  slow_down argument: slow_down is cooperative — polite clients honor it
    Malicious attacker: ignores slow_down and submits at maximum rate
    "RFC-defined mechanism" → TRUE for client-rate-management; FALSE as server enforcement
    slow_down does not prevent brute force from a non-compliant client
  Combined: absent MUST control + low entropy → brute-force feasible

Node 5 — Applier:
  RFC 8628 §6.1 MUST absent → REPROCESS (server-side rate limiting absent)

Node 6 — Result Translator:
  Controlling violation: RFC 8628 §6.1 MUST server-side rate limiting absent
  slow_down argument explicitly rejected

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — device flow brute-force protection

Node 8 — Arbiter:
  REPROCESS — No server-side rate limiting on the device verification endpoint violates
  RFC 8628 §6.1 MUST. The slow_down response is cooperative and does not constrain
  a non-compliant or malicious client. With 60M user_code keyspace and 30-minute
  lifetime, brute-force is feasible without server-side enforcement. Remediate:
  add server-side limit per device_code (e.g., 10-30 attempts); consider extending
  user_code to 8+ characters for additional entropy.

Node 9 — Formatter:
  REPROCESS
  Primary: RFC 8628 §6.1 MUST — server-side rate limiting absent
  Secondary: user_code entropy marginal (5-char, 60M keyspace)
  Rejected argument: "slow_down = server enforcement"
  Remediation: add per-device_code attempt limit; increase user_code length
```

**Reason**
Full pipeline trace for device flow without server-side rate limiting. Node 4 evaluates the slow_down argument and rejects it (cooperative mechanism ≠ server enforcement). RFC 8628 §6.1 MUST absent → REPROCESS. The Node 2 translation step explicitly separates the two RFC 8628 §6.1 controls (entropy AND rate limiting) so the pipeline trace is complete.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-631 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 8628)
Node 2: RFC 8628 §6.1 MUST — entropy AND server-side rate limiting both required
  slow_down: cooperative client mechanism, not server-side enforcement
Node 4: 36^5 = 60M codes; no server-side limit → RFC 8628 §6.1 MUST absent
  "slow_down = protection" → REJECTED (non-compliant clients ignore it)
Node 8 — Arbiter: REPROCESS — Server-side rate limiting absent (RFC 8628 §6.1 MUST).
  Add per-device_code attempt limit; extend user_code entropy.
Node 9: REPROCESS; §6.1 MUST absent; slow_down argument rejected
```

---

## Pair[CB4-3.4-632]

**A**
```
Incoming pipeline input:
"Correct device flow implementation: 8-char [A-Z2-9] user_code (excludes 0,1,O,I
for clarity — 34-char alphabet), CSPRNG generated, 15-minute device_code TTL,
server-side 30-attempt limit per device_code invalidation, HTTPS verification URI."
```

**B**
```
Pipeline trace — CB4-3.4-632

Node 1 — Domain Splitter:
  Input: device authorization grant implementation
  Domain: T-rfc-oauth2 (RFC 8628)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8628 §6.1: MUST consider brute-force → (1) sufficient entropy + (2) server-side limit
  user_code design: human-friendly (excluded confused chars) + CSPRNG + filtered alphabet

Node 3 — Scheme Planner:
  Check 1: user_code entropy → 34^8 ≈ 2.8T codes; CSPRNG → high entropy
  Check 2: server-side limit → 30 attempts per device_code → present → PASS
  Check 3: verification URI → HTTPS enforced → PASS
  Plan: verify both §6.1 controls satisfied

Node 4 — Domain Processor:
  34^8 ≈ 2.8 trillion codes: astronomically high keyspace
  30-attempt limit per device_code: brute-force probability = 30/2.8T ≈ 10^-11 per attempt
  CSPRNG generation: unpredictable; no sequential guessing possible
  HTTPS verification URI: RFC 8628 §3.3 requires HTTPS → satisfied
  15-minute TTL: reasonable; shorter than 30-minute example → reduces window
  Both RFC 8628 §6.1 controls satisfied

Node 5 — Applier:
  PASS — entropy satisfied; server-side rate limiting present; HTTPS correct

Node 6 — Result Translator:
  RFC 8628 §6.1 fully satisfied; no violations

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — device flow baseline PASS

Node 8 — Arbiter:
  PASS — Device authorization grant correctly implements RFC 8628. 34^8 ≈ 2.8T user_code
  keyspace generated by CSPRNG provides high entropy. Server-side 30-attempt limit per
  device_code satisfies §6.1 rate limiting requirement. HTTPS verification URI correct.
  Human-friendly alphabet (excluded visually similar characters) does not materially
  reduce security given the 2.8T keyspace. No findings.

Node 9 — Formatter:
  PASS
  RFC 8628 §6.1 compliance: entropy + rate limiting both confirmed
  Verification URI: HTTPS enforced
  No findings
```

**Reason**
Full pipeline trace for correct device flow. Node 4 computes the brute-force probability (~10^-11) to confirm the PASS. Node 2 explicitly calls out both §6.1 controls so the trace demonstrates that both are evaluated and passed. The human-friendly alphabet exclusion is addressed as not materially reducing security given the large keyspace.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-632 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 8628)
Node 2: §6.1 — entropy + server-side limit both MUST-evaluated
Node 4: 34^8 ≈ 2.8T CSPRNG codes; 30-attempt limit; HTTPS; TTL 15 min → all PASS
Node 8 — Arbiter: PASS — Both RFC 8628 §6.1 controls satisfied. 2.8T keyspace +
  server-side rate limiting + HTTPS = compliant device flow. No findings.
Node 9: PASS; §6.1 confirmed
```

---

## Pair[CB4-3.4-633]

**A**
```
Incoming pipeline input:
"CORS wildcard on /token and /introspect endpoints. Developer says the Authorization
header on requests triggers a preflight OPTIONS, so simple POST attacks are prevented.
Token endpoint uses POST with Content-Type: application/json."
```

**B**
```
Pipeline trace — CB4-3.4-633

Node 1 — Domain Splitter:
  Input: CORS configuration on token and introspect endpoints
  Domains: T-rfc-oauth2 / T-asvs-authorization
  Sub-phase: 3.4

Node 2 — Domain Translator:
  CORS simple request definition: POST with Content-Type application/x-www-form-urlencoded,
    text/plain, or multipart/form-data is simple; application/json triggers preflight
  RFC 6749 §4.1.3 and §4.3: token endpoint clients MAY use form-encoded or JSON body
  /introspect: RFC 7662 — sensitive token metadata returned

Node 3 — Scheme Planner:
  Check 1: Does token endpoint use application/json? → YES (as stated)
  Check 2: Does application/json trigger preflight? → YES
  Check 3: Does CORS wildcard on /introspect expose token metadata? → YES
  Plan: evaluate each endpoint; consider ASVS V8.2.2 for introspect

Node 4 — Domain Processor:
  /token with application/json:
    application/json Content-Type → triggers preflight OPTIONS
    Developer claim partially valid for JSON content type
    But: wildcard ACAO still allows any origin to receive the response after preflight
    Preflight only determines whether the cross-origin request is ALLOWED
    Wildcard ACAO = the request IS allowed from any origin (preflight will pass)
    Preflight does not protect content; it merely checks the ACAO value
    A malicious origin will successfully pass preflight and read token response
    CORS wildcard on /token with wildcard = any origin can get access tokens → REPROCESS
  /introspect with wildcard:
    Token metadata (sub, scope, exp, jti) readable from any origin → V8.2.2 → REPROCESS
  Developer argument: "preflight prevents attack" → REJECTED
    Preflight checks ACAO; wildcard ACAO passes all preflight checks by definition

Node 5 — Applier:
  REPROCESS × 2 endpoints

Node 6 — Result Translator:
  Both endpoints REPROCESS; developer preflight argument rejected

Node 7 — Arbiter Selector:
  T-rfc-oauth2 + T-asvs-authorization

Node 8 — Arbiter:
  REPROCESS — CORS wildcard on both /token and /introspect. For /token: the
  application/json Content-Type does trigger preflight, but wildcard ACAO means all
  preflight checks pass — any origin is permitted. Preflight checks the ACAO value;
  wildcard allows every origin. The /token response (containing access tokens) is
  readable by any origin. /introspect wildcard exposes token metadata (sub/scope/exp)
  cross-origin (V8.2.2). Restrict ACAO to specific trusted deployment origins on both.

Node 9 — Formatter:
  REPROCESS
  /token: wildcard allows any origin to obtain access tokens (preflight argument rejected)
  /introspect: token metadata cross-origin exposure (V8.2.2)
  Remediation: restrict ACAO to specific trusted origins on both endpoints
```

**Reason**
The pipeline trace explores a more sophisticated preflight argument: Content-Type application/json does trigger preflight, unlike form-encoded. Node 4 correctly rebuts: preflight checks whether the request is allowed (ACAO value), and wildcard ACAO means ALL preflight checks pass — the request is permitted from any origin. Wildcard does not protect; it opens. Both endpoints REPROCESS.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-633 (phrasing B)

Node 1: T-rfc-oauth2 + T-asvs-authorization
Node 2: application/json triggers preflight; but wildcard ACAO passes all preflight checks
Node 4: /token JSON → preflight triggered → but wildcard ACAO passes it → any origin gets tokens
  /introspect wildcard → V8.2.2 token metadata exposure
  "Preflight prevents attack" → REJECTED: preflight checks ACAO; wildcard = always permitted
Node 8 — Arbiter: REPROCESS — Wildcard ACAO on both endpoints. Preflight only checks
  whether origin is allowed; wildcard means all origins are allowed. Restrict to specific origins.
Node 9: REPROCESS; both endpoints; preflight argument rejected
```

---

## Pair[CB4-3.4-634]

**A**
```
Incoming pipeline input:
"CORS configuration: /token endpoint Access-Control-Allow-Origin restricted to
https://app.example.com. /introspect endpoint: no CORS headers at all (backend
services only call it; it never receives browser-originated requests)."
```

**B**
```
Pipeline trace — CB4-3.4-634

Node 1 — Domain Splitter:
  Input: CORS configuration on token and introspect endpoints
  Domain: T-rfc-oauth2
  Sub-phase: 3.4

Node 2 — Domain Translator:
  CORS: browsers enforce ACAO; non-browser clients unaffected regardless of ACAO value
  /token: SPA-facing; CORS header required for browser access
  /introspect: backend-only; CORS header not required by non-browser clients

Node 3 — Scheme Planner:
  Check 1: /token ACAO restricted to specific origin? → YES (https://app.example.com)
  Check 2: /introspect has no CORS headers; genuinely backend-only? → assess
  Plan: verify /token restriction is correct; confirm /introspect no-CORS is appropriate

Node 4 — Domain Processor:
  /token: Access-Control-Allow-Origin: https://app.example.com
    Only the registered SPA deployment origin can receive token responses → PASS
    Matches registered redirect_uri pattern; specific-origin restriction is correct
  /introspect: no CORS headers
    Without ACAO header, browsers will block cross-origin introspect responses
    Backend services are not subject to CORS → no ACAO header = effectively blocked to browsers
    This is STRICTER than wildcard: wildcard permits browser access; no header denies it
    If /introspect is genuinely backend-only → no CORS headers is correct and appropriate
    No V8.2.2 risk because browsers cannot access the response (CORS blocks it)

Node 5 — Applier:
  /token: PASS; /introspect: PASS
  No violations in either endpoint configuration

Node 6 — Result Translator:
  Both endpoints correctly configured; no CORS violations

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — CORS baseline PASS

Node 8 — Arbiter:
  PASS — CORS configuration is correct for both endpoints. Token endpoint restricts to
  the registered SPA deployment origin (https://app.example.com). Introspection endpoint
  has no CORS headers — appropriate for a backend-only endpoint; browsers will be blocked
  by the browser's CORS enforcement, which is stricter than a wildcard that would permit
  all origins. No findings.

Node 9 — Formatter:
  PASS
  /token: specific-origin restriction confirmed
  /introspect: no CORS headers appropriate for backend-only endpoint
  No findings
```

**Reason**
Full pipeline trace for the correct CORS configuration. Node 4 establishes that "no CORS headers" on a backend-only endpoint is stricter than wildcard — browsers cannot access the response at all when ACAO is absent, whereas wildcard permits all. This is a Stratum C pipeline trace demonstrating the nuanced evaluation of "absence" as a security property.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-634 (phrasing B)

Node 1: T-rfc-oauth2
Node 2: backend clients unaffected by CORS; CORS applies to browsers only
Node 4: /token specific-origin → PASS; /introspect no headers → browsers blocked by CORS enforcement → stricter than wildcard → PASS
Node 8 — Arbiter: PASS — /token restricted to SPA origin; /introspect no-CORS headers
  means browsers cannot access it. Both endpoints correctly configured. No findings.
Node 9: PASS; both endpoints correct
```

---

## Pair[CB4-3.4-635]

**A**
```
Incoming pipeline input:
"Device authorization flow dispute: developer proposes 6-char [A-Z2-9] codes
(34^6 ≈ 1.5B) with 50-attempt per device_code limit and 15-minute lifetime.
Security reviewer says 50 attempts is too many. Developer says the math is safe."
```

**B**
```
Pipeline trace — CB4-3.4-635

Node 1 — Domain Splitter:
  Input: device authorization grant — entropy and rate limiting dispute
  Domain: T-rfc-oauth2 (RFC 8628)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 8628 §6.1: MUST consider brute-force; two controls required
  No specific attempt limit mandated; combination of entropy + limit governs

Node 3 — Scheme Planner:
  Check 1: §6.1 controls present? → YES (entropy + server-side limit)
  Check 2: Is 50 attempts too many relative to keyspace? → evaluate probabilistically
  Plan: compute brute-force probability; evaluate RFC 8628 compliance

Node 4 — Domain Processor:
  34^6 ≈ 1,544,804,416 ≈ 1.5B codes
  50-attempt limit per device_code
  Brute-force probability per attempt: 50/1.5B ≈ 3.24×10^-8
  Peak concurrent active device_codes: assume typical production scenario (≤10,000)
  Probability of guessing any active code in one attack cycle: 50/1.5B × 10,000 ≈ 3.24×10^-4
  At 100 attack cycles: probability ≈ 0.032 = ~3% — non-trivial if attacker is persistent
  However: 50 attempts per device_code then invalidation breaks the cycle
    Attacker must issue a NEW device authorization request for each new device_code
    Each new device authorization = a new device visible in the system
    Scaled attacks are detectable and rate-limitable at the device authorization request level
  RFC 8628 does not specify an attempt maximum; the spec requires MUST consider protection
  Both controls present: entropy (1.5B) + server-side limit (50) → §6.1 satisfied
  Security reviewer's concern is valid as a recommendation (fewer is better)
  But it does not constitute a MUST violation at 50 attempts against 1.5B codes

Node 5 — Applier:
  PASS — both §6.1 controls present and probabilistically adequate

Node 6 — Result Translator:
  §6.1 satisfied; 50-attempt limit is generous but not a violation given the keyspace

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — device flow argument validation

Node 8 — Arbiter:
  PASS — Both RFC 8628 §6.1 controls are present: user_code entropy (34^6 ≈ 1.5B,
  CSPRNG) and server-side per-device_code limit (50 attempts, then invalidation).
  The spec does not mandate a specific attempt count. At 50 attempts against a 1.5B
  code keyspace, even with 10,000 concurrent active codes, the brute-force probability
  per attack cycle is ~3×10^-4. Scaled attacks require additional device authorization
  requests, which are themselves rate-limitable and auditable. Reviewer recommendation
  (fewer attempts) is valid as defense-in-depth but is not a compliance gap.

Node 9 — Formatter:
  PASS
  §6.1: both controls confirmed
  Probability analysis: ~3×10^-4 per attack cycle — adequate
  Reviewer recommendation: valid as defense-in-depth; not a compliance gap
```

**Reason**
Full pipeline trace for the device flow argument-validation scenario. Node 4 performs the probability calculation, considers scaled attacks, and correctly identifies that RFC 8628 does not mandate a specific attempt count. PASS because both §6.1 controls are present. The reviewer's concern is acknowledged as valid defense-in-depth but not a compliance gap.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-635 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 8628)
Node 2: §6.1 — entropy + server-side limit; no specific attempt count mandated
Node 4: 34^6 ≈ 1.5B; 50-attempt limit; probability ~3×10^-4/cycle; both controls present
  Reviewer concern: valid as defense-in-depth; not a violation
Node 8 — Arbiter: PASS — Both §6.1 controls present. 50 attempts vs 1.5B keyspace is
  probabilistically adequate. No specific limit mandated by RFC 8628.
Node 9: PASS; §6.1 satisfied; reviewer suggestion noted as non-mandatory
```

---

## Pair[CB4-3.4-636]

**A**
```
Incoming pipeline input — full ext24 PASS scenario:
"OAuth deployment: Bearer header token transmission, no URL tokens, no token logging.
Error responses: only error code, no enumeration. Device flow: 8-char [A-Z2-9] CSPRNG
codes, 30-attempt server limit, HTTPS activation URI. CORS: /token restricted to SPA
origin; /authorize wildcard acceptable; /introspect no CORS headers (backend-only)."
```

**B**
```
Pipeline trace — CB4-3.4-636

Node 1 — Domain Splitter:
  Input: comprehensive ext24 pattern review
  Domains: T-rfc-oauth2 / T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 6750 §2.1: preferred Bearer header transmission; §2.3 SHOULD NOT for URL params
  V9.3.1: MUST NOT log credential-equivalent tokens in production
  RFC 6749 §5.2: error response minimum-disclosure
  RFC 8628 §6.1: device flow brute-force — entropy + server-side limit
  CORS: /authorize wildcard acceptable (redirects, not token delivery)

Node 3 — Scheme Planner:
  Check 1: token transmission → Bearer header → PASS
  Check 2: error response → code only → PASS
  Check 3: device flow → 34^8 entropy + server limit + HTTPS → PASS
  Check 4: CORS → /token restricted; /authorize wildcard; /introspect no-headers → PASS
  Plan: confirm each element; no violation expected

Node 4 — Domain Processor:
  Token transmission: Bearer header per RFC 6750 §2.1 + no URL params + no logging → PASS
  Error response: error code only; no enumeration, no IP, no internal detail → PASS (§5.2)
  Device flow: 34^8 ≈ 2.8T codes + CSPRNG + 30-attempt limit + HTTPS → PASS (§6.1)
  CORS breakdown:
    /token → SPA-origin-only restriction → PASS
    /authorize → wildcard acceptable: authorization endpoint returns redirects,
      not credential payloads; CORS on it does not expose tokens → PASS
    /introspect → no CORS headers → browser access blocked by CORS enforcement → PASS
  No violations across any element

Node 5 — Applier:
  PASS across all ext24 domains

Node 6 — Result Translator:
  All applicable RFC and ASVS requirements met; no findings

Node 7 — Arbiter Selector:
  Comprehensive multi-domain PASS

Node 8 — Arbiter:
  PASS — All four ext24 domain patterns are correctly implemented. Token transmission
  via Bearer header with no URL usage and no logging (V9.3.1 + RFC 6750 §2.1).
  Error responses follow minimum-disclosure principle (RFC 6749 §5.2). Device flow
  satisfies RFC 8628 §6.1 with high-entropy CSPRNG codes (2.8T keyspace) and server-side
  rate limiting. CORS is correctly scoped: token endpoint restricted, authorization
  endpoint accepts wildcard (redirects only), introspect has no CORS headers (backend-only).
  No findings.

Node 9 — Formatter:
  PASS
  Four ext24 domain patterns each confirmed as conformant
  Token: Bearer header + no logging ✓
  Errors: minimum disclosure ✓
  Device flow: RFC 8628 §6.1 satisfied ✓
  CORS: endpoint-appropriate configuration ✓
```

**Reason**
Comprehensive PASS pipeline trace for all ext24 patterns. Node 4 adds the /authorize wildcard analysis (acceptable because authorization endpoint returns redirects, not credential payloads — mirrors the ext17 pattern). All four ext24 domains evaluated across all 9 nodes. This closes the ext24 pattern set with a complete baseline PASS trace.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-636 (phrasing B)

Node 1: T-rfc-oauth2 / T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2
Node 4:
  Bearer header + no URL + no logging → PASS (RFC 6750 §2.1 + V9.3.1)
  error code only → PASS (RFC 6749 §5.2)
  34^8 CSPRNG + 30-attempt limit + HTTPS → PASS (RFC 8628 §6.1)
  /token restricted; /authorize wildcard OK (redirects not tokens); /introspect no-headers → PASS
Node 8 — Arbiter: PASS — All ext24 patterns correct. Four domains confirmed conformant. No findings.
Node 9: PASS; all four ext24 patterns confirmed
```
