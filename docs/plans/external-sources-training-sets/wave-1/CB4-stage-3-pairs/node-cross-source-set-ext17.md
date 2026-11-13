# CB4 Stage 3 — Cross-Source Combination Set ext17
# Sequences CB4-3.4-637 through CB4-3.4-646
# Stratum B/C/D — ext24 multi-source combinations
# Running total after this file: 1292 instances (646 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-637]

**A**
```
Security review findings:

Finding 1 — Token transmission: access tokens passed as URL query parameters
  in API requests. Full URL logged at DEBUG level in production API gateway.
Finding 2 — Error responses: authorization failures include a list of all
  registered client IDs and the requesting IP in the error_description field.
Finding 3 — Device flow: 8-char [A-Z2-9] codes, CSPRNG, 30-attempt server limit,
  HTTPS activation URI.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 + T-asvs-token / T-rfc-oauth2 + T-asvs-authorization / T-rfc-oauth2
Node 2 — Translation:
  F1: RFC 6750 §2.3 SHOULD NOT (URL transmission) + V9.3.1 MUST NOT (production logging)
  F2: RFC 6749 §5.2 + CWE-200 (error response enumeration)
  F3: RFC 8628 §6.1 (device flow brute-force protection)
Node 3 — Plan: F1=URL-logging-combination; F2=information-disclosure; F3=device-flow-check
Node 4 — Processing:
  F1: query param → tokens in URLs → DEBUG logging in production → V9.3.1 MUST NOT → REPROCESS
  F2: client registry enumeration in error_description → CWE-200 → REPROCESS
  F3: 34^8 ≈ 2.8T codes + CSPRNG + 30-attempt limit + HTTPS → RFC 8628 §6.1 satisfied → PASS
Node 5 — Application: REPROCESS × 2 + PASS × 1
Node 6 — Translation: two REPROCESS findings; REPROCESS governs
Node 7 — Arbiter: F1 (V9.3.1) and F2 (CWE-200) are co-primary REPROCESS findings
Node 8 — Arbiter verdict:
  REPROCESS — Two simultaneous REPROCESS findings:
  F1: URL query parameter transmission with production DEBUG URL logging violates V9.3.1
    (MUST NOT log credential-equivalent tokens). Use Authorization: Bearer header; disable
    full-URL logging.
  F2: Error response enumerating all registered client IDs violates CWE-200 minimum
    disclosure. Any caller can obtain the complete client registry from a single failed
    request. Replace error_description with a generic failure message.
  F3: Device flow is correctly implemented (2.8T keyspace, server-side limit, HTTPS). PASS.
Node 9 — Format: REPROCESS; F1 V9.3.1 + F2 CWE-200 co-primary; F3 PASS noted
```

**Reason**
F1 and F2 are both independent REPROCESS violations; two simultaneous REPROCESS findings do not change the verdict (still REPROCESS). F3 is a correct implementation contrasted against the two failures, demonstrating selective compliance.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2
Node 4:
  F1: URL param + DEBUG logging → V9.3.1 MUST NOT → REPROCESS
  F2: client registry enumeration → CWE-200 → REPROCESS
  F3: 34^8 CSPRNG + 30-attempt limit + HTTPS → RFC 8628 §6.1 → PASS
Node 8 — Arbiter: REPROCESS — F1 (V9.3.1 token logging) and F2 (CWE-200 enumeration)
  are co-primary violations. F3 device flow is correct. Remediate F1 and F2.
Node 9 — Format: REPROCESS; two violations; F3 PASS noted
```

---

## Pair[CB4-3.4-638]

**A**
```
Post-remediation review — all three ext24 critical findings addressed:

Finding 1 remediated: tokens now transmitted via Authorization: Bearer header.
  No URL query parameter usage. Debug URL logging replaced with structured logging
  that masks token values (first 8 chars + redacted).
Finding 2 remediated: error responses return only {"error": "invalid_client"}.
  No error_description field included.
Finding 3 (device flow) was already PASS, remains unchanged.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 + T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2
Node 2 — Translation: F1=token-transmission-logging; F2=error-response; F3=device-flow
Node 3 — Plan: verify each remediation against original finding criterion
Node 4 — Processing:
  F1: Bearer header transmission (RFC 6750 §2.1 preferred method) → PASS
      Structured logging with token masking (first 8 + "redacted") → V9.3.1 satisfied
        Masked value is not credential-equivalent → PASS
  F2: error code only, no error_description → RFC 6749 §5.2 minimum disclosure → PASS
      "No error_description" is acceptable — spec says SHOULD, not MUST include it
      Omitting it entirely eliminates disclosure risk → PASS
  F3: device flow unchanged; previously PASS → PASS
Node 5 — Application: PASS × 3
Node 6 — Translation: all three findings resolved; no new violations
Node 7 — Arbiter: post-remediation PASS
Node 8 — Arbiter verdict:
  PASS — All findings from the prior review have been correctly remediated. Token
  transmission uses Bearer header; structured logging masks token values (not
  credential-equivalent). Error responses contain only the error code with no
  enumeration. Device flow was already correct. No findings.
Node 9 — Format: PASS; three items verified; no remaining findings
```

**Reason**
Post-remediation PASS for the ext24 trio. F1 introduces token masking in logs (first 8 chars + "redacted") — this is a nuanced check that a masked value is not credential-equivalent. F2 establishes that omitting error_description entirely is acceptable (RFC 6749 says SHOULD, not MUST include it). F3 unchanged.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2
Node 4:
  F1: Bearer header + masked logging → V9.3.1 satisfied → PASS
  F2: error code only, no description → acceptable; minimum disclosure → PASS
  F3: device flow unchanged, previously PASS → PASS
Node 8 — Arbiter: PASS — All three findings resolved. Bearer header, masked logs,
  minimum-disclosure errors, compliant device flow. No findings.
Node 9 — Format: PASS; three items confirmed resolved
```

---

## Pair[CB4-3.4-639]

**A**
```
Developer proposes hybrid logging approach:

"We log access tokens at DEBUG level only in our staging environment,
which uses production-equivalent data. The authorization server also returns
error_description with the full error context — helpful for debugging staging
issues. Production logs don't include tokens, and production errors are generic."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-asvs-token (V9.3.1) + T-rfc-oauth2 (RFC 6749 §5.2)
Node 2 — Translation: environment-conditional token logging and error disclosure
Node 3 — Plan: check V9.3.1 for staging environments; check error_description content
Node 4 — Processing:
  Token logging in staging with production-equivalent data:
    V9.3.1 scope: applies to production; question is whether staging with prod-equivalent data is in scope
    Previously established pattern (ext15): DEBUG/staging + production data → REPROCESS
    Production-equivalent data in staging = same risk profile as production for data exposure
    Access tokens in staging logs = credential exposure for production users if staging
    has production-equivalent token signing keys or user population
    "Staging" label does not reduce risk when data is production-equivalent → REPROCESS
  Error_description with full context in staging:
    If staging shares the client registry with production → enumeration risk exists in staging
    Staging debugging errors that include registry data = reconnaissance pathway in staging environment
    Less severe than production enumeration but still a concern
    However: key question is whether staging errors expose production client IDs
    "Full error context" without specifics → CONCERN (ambiguous; may or may not enumerate)
  Primary violation: token logging in staging with production-equivalent data → REPROCESS
    This is the established precedent: environment label does not reduce risk when data profile is equivalent
Node 5 — Application: REPROCESS (V9.3.1 staging-with-prod-data)
Node 6 — Translation: REPROCESS — environment boundary does not protect when data is prod-equivalent
Node 7 — Arbiter: T-asvs-token — token logging environment assessment
Node 8 — Arbiter verdict:
  REPROCESS — Staging environment with production-equivalent data is subject to V9.3.1.
  Access tokens in staging logs with production-level data constitute credential exposure
  for production users or token keys. The staging/production distinction does not reduce
  the risk when the data profile is equivalent. Error_description scope is ambiguous
  but if "full context" includes client registry enumeration, it compounds the violation.
  Remediate: apply the same token logging controls (masking/exclusion) in staging as in
  production when staging uses production-equivalent data. Restrict error_description
  in staging to non-enumerating context.
Node 9 — Format: REPROCESS; V9.3.1 staging-with-prod-data; error_description secondary
```

**Reason**
The staging-with-production-equivalent-data escalation pattern established in ext15 applies here. The environment label alone does not reduce risk when the data profile is production-equivalent. "Full error context" is flagged as a secondary CONCERN (ambiguous without seeing the actual content). Primary REPROCESS from V9.3.1 staging exception.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-asvs-token / T-rfc-oauth2
Node 4:
  Staging + production-equivalent data → V9.3.1 applies → REPROCESS
  "Staging label = no risk" → REJECTED (data equivalence governs, not environment label)
  Error_description "full context" → ambiguous; if includes client enumeration → compounds violation
Node 8 — Arbiter: REPROCESS — V9.3.1 applies in staging when data is production-equivalent.
  Token masking/exclusion required in staging. Error_description scope must be clarified.
Node 9 — Format: REPROCESS; V9.3.1 primary; error_description secondary
```

---

## Pair[CB4-3.4-640]

**A**
```
Four-finding OAuth audit:

1. Device flow user_code: 4-character [A-Z] codes (26^4 = 456,976 codes).
   No server-side rate limiting. 15-minute device_code lifetime.
2. CORS on /token: Access-Control-Allow-Origin: https://app.example.com
   (SPA deployment origin, as registered).
3. OAuth error responses: return error code + error_description with a short
   human-readable message ("Check your client credentials and try again").
4. SSDF PO.3: no formal SBOM process. Development team uses npm audit manually
   when they think about it.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2 / T-ssdf-process
Node 2 — Translation:
  F1: RFC 8628 §6.1 device flow
  F2: CORS /token endpoint
  F3: RFC 6749 §5.2 error response
  F4: SSDF PO.3 SBOM
Node 3 — Plan: check each finding independently
Node 4 — Processing:
  F1: 26^4 = 456,976 codes; 15-min lifetime; no server-side limit
      RFC 8628 §6.1 MUST server-side rate limiting → absent → REPROCESS
      456,976 codes with no limit is trivially enumerable; entropy also low
  F2: /token restricted to https://app.example.com → correct CORS → PASS
  F3: error code + "Check your client credentials and try again" → no enumeration,
      no internal data, no IP → minimum-disclosure satisfied → PASS
  F4: no formal SBOM + manual npm audit occasionally → CONCERN (PO.3.2 gap)
      Not a REPROCESS; process exists informally; documented gap
Node 5 — Application: REPROCESS (F1) + PASS (F2) + PASS (F3) + CONCERN (F4)
Node 6 — Translation: REPROCESS present → overall REPROCESS
Node 7 — Arbiter: F1 (RFC 8628 §6.1 MUST absent) is controlling finding; F4 secondary
Node 8 — Arbiter verdict:
  REPROCESS — Device flow lacks server-side rate limiting on the verification endpoint
  (RFC 8628 §6.1 MUST). 26^4 = 456,976 codes with no limit is trivially brute-forceable.
  Additionally: no formal SBOM process (CONCERN, PO.3.2). CORS /token endpoint and
  error responses are correctly implemented. Primary remediation: add server-side
  per-device_code attempt limit to satisfy RFC 8628 §6.1.
Node 9 — Format: REPROCESS; F1 primary; F4 CONCERN secondary; F2/F3 PASS noted
```

**Reason**
F1 is a clear REPROCESS: 26^4 codes with no server-side limit violates RFC 8628 §6.1 MUST. F4 is CONCERN (PO.3.2 informal process). F2 and F3 are PASS — correct CORS and minimum-disclosure errors. The multi-finding trace demonstrates correct selective enforcement: not every finding is REPROCESS.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2 / T-ssdf-process
Node 4:
  F1: 26^4 = 456,976 codes + no server limit → §6.1 MUST absent → REPROCESS
  F2: /token restricted origin → PASS
  F3: generic error message, no enumeration → PASS
  F4: manual occasional npm audit → CONCERN (PO.3.2)
Node 8 — Arbiter: REPROCESS — F1 (RFC 8628 §6.1 absent) is controlling.
  F4 CONCERN; F2/F3 correct. Add server-side rate limiting to device flow.
Node 9 — Format: REPROCESS; F1 primary; F4 secondary CONCERN
```

---

## Pair[CB4-3.4-641]

**A**
```
Architecture review — three CONCERNs, no REPROCESS:

Finding 1: CORS on /introspect endpoint: Access-Control-Allow-Origin: * (wildcard).
  Developer: "Introspect is called from our API gateway backend — no browser clients."
Finding 2: Error response includes error_description with: "Client authentication
  failed. Please verify your client_id and redirect_uri configuration."
  (Does not enumerate valid clients; provides configuration guidance.)
Finding 3: Device flow polling: slow_down response increases polling delay;
  server-side limit is 100 attempts per device_code (34^6 ≈ 1.5B keyspace).
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2
Node 2 — Translation:
  F1: CORS wildcard on introspect endpoint
  F2: error_description content assessment
  F3: device flow attempt limit
Node 3 — Plan: F1=CORS-hygiene; F2=disclosure-check; F3=RFC8628-limit-evaluation
Node 4 — Processing:
  F1: wildcard ACAO on /introspect; developer claim "backend only"
      Partially valid (CORS applies to browsers only); backend callers unaffected
      But wildcard on a sensitive-metadata endpoint = hygiene gap if scope changes
      Not a current MUST violation if genuinely backend-only → CONCERN
  F2: "verify your client_id and redirect_uri configuration" — no enumeration
      Guidance without disclosure of valid identifiers → acceptable → PASS
      Does not enumerate valid client IDs or expose internal data → PASS
  F3: 100-attempt limit vs 34^6 ≈ 1.5B codes
      Probability: 100/1.5B ≈ 6.5×10^-8 per attempt; generous but not a violation
      RFC 8628 does not mandate a specific limit; 100 vs 1.5B codes is defensible
      However: 100 is higher than typical conservative implementations (10-30)
      Not a MUST violation; CONCERN for defense-in-depth tightening
  Three findings: CONCERN + PASS + CONCERN = CONCERN overall
  Graduation rule: no REPROCESS present → CONCERN
Node 5 — Application: CONCERN (two concerns; no REPROCESS)
Node 6 — Translation: CONCERN — graduation rule: N CONCERNs without REPROCESS = CONCERN
Node 7 — Arbiter: T-rfc-oauth2 — CORS hygiene + rate-limit tightening
Node 8 — Arbiter verdict:
  CONCERN — Two findings, both at CONCERN level. (1) /introspect wildcard ACAO is a
  hygiene gap: while backend callers are unaffected by CORS, wildcard on a sensitive
  metadata endpoint should be restricted. (2) 100-attempt limit per device_code is
  generous relative to common conservative implementations; while not a RFC 8628
  violation, tightening to 20-30 attempts would reduce brute-force risk. Error
  responses (F2) are correctly scoped with no enumeration. No REPROCESS triggers.
Node 9 — Format: CONCERN; two findings; F2 PASS; graduation rule applied
```

**Reason**
Three-finding trace: F1 = CONCERN (wildcard introspect hygiene, established in ext24), F2 = PASS (error message has guidance text but no enumeration), F3 = CONCERN (100-attempt limit is generous but not a violation). Two CONCERNs + one PASS = CONCERN overall by graduation rule. No REPROCESS present.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 × 3
Node 4:
  F1: /introspect wildcard; backend-only argument → CONCERN (hygiene gap)
  F2: "verify client_id and redirect_uri" — no enumeration → PASS
  F3: 100-attempt limit vs 1.5B codes; defensible but generous → CONCERN
  Two CONCERNs + one PASS → no REPROCESS → CONCERN
Node 8 — Arbiter: CONCERN — /introspect wildcard hygiene (F1) and generous attempt limit
  (F3) are CONCERNs. F2 error message is correct. Tighten both CONCERNs; no critical violations.
Node 9 — Format: CONCERN; graduation rule applied; F2 PASS
```

---

## Pair[CB4-3.4-642]

**A**
```
OAuth 2.0 error response design dispute:

Developer A: "We should include the attempted client_id in the error response —
it helps the caller self-diagnose typos in their configuration."
Developer B: "That discloses whether the client_id was valid or invalid, which
helps attackers enumerate valid IDs by trial-and-error."
Developer A: "Client IDs aren't secret — they appear in authorization request
URLs. Any attacker can see them. There's no security benefit to hiding them
in error responses."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 6749 §5.2) / T-asvs-authorization (V8.2.2)
Node 2 — Translation: error response content — disclosure of attempted vs. valid client_id
Node 3 — Plan: evaluate Developer A's arguments; assess disclosure risk calibration
Node 4 — Processing:
  Developer A claim 1: "helps caller self-diagnose typos"
    Valid operational argument: echoing back the attempted value aids debugging
    Echoing the ATTEMPTED client_id (what the caller sent) ≠ disclosing VALID client IDs
    If the response echoes: "client_id 'typo-corp' not found" → echoes caller's input
    This is different from enumerating all valid IDs → different risk level
  Developer A claim 2: "client IDs appear in authorization URLs — not secret"
    PARTIALLY TRUE: public-facing clients (web applications) do have visible client_ids
      in authorization URLs (?client_id=spa-frontend)
    PARTIALLY FALSE: machine-to-machine service client IDs may not appear in browser URLs;
      server-side client credentials flow client_ids are not visible to browsers
    Even for public clients: "already visible in URLs" ≠ "disclosing additional valid IDs"
    The error response ENUMERATING the full registry is the risk; not echoing one ID
  Risk assessment of echoing attempted client_id:
    If response echoes only the caller's own attempted value → CONCERN (some disclosure)
    If response allows attacker to distinguish "valid ID" vs "invalid ID" → oracle attack
    "client_id not found" confirms the attempted ID is NOT registered → negative oracle
    An attacker can use negative oracle to enumerate by trying IDs and noting "not found" vs other error
    This is weaker than a full-registry dump (original REPROCESS) but still a CONCERN
  Developer B's concern is partially valid; Developer A's "not secret" argument insufficient
  Verdict: CONCERN (echo of attempted value with negative-oracle disclosure)
Node 5 — Application: CONCERN (negative oracle risk via error response)
Node 6 — Translation: CONCERN — partial disclosure; not REPROCESS (no registry enumeration)
Node 7 — Arbiter: T-rfc-oauth2 / T-asvs-authorization — error response disclosure calibration
Node 8 — Arbiter verdict:
  CONCERN — Echoing the attempted client_id with a "not found" message creates a negative
  oracle: an attacker can enumerate valid client_ids by trying values and observing which
  responses are "not found" vs. other error types. This is weaker than disclosing the
  full client registry (which would be REPROCESS) but is still a disclosure gap.
  Developer A's argument that client_ids "aren't secret" applies to public-client IDs
  visible in authorization URLs, not to all registered clients. Use a generic "client
  authentication failed" message that does not distinguish ID validity.
Node 9 — Format: CONCERN; negative oracle identified; Developer A argument partially rebutted
```

**Reason**
This is a nuanced Stratum D argument: echoing the attempted client_id is not as bad as full-registry enumeration (which would be REPROCESS), but creates a negative oracle that allows iterative enumeration. The "client IDs aren't secret" argument is partially valid for public-facing clients but not universally. CONCERN not REPROCESS because no registry is disclosed — only a hint about one ID's validity.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authorization
Node 4:
  Echoing attempted client_id + "not found" → negative oracle for enumeration
  "Client IDs aren't secret" → partially true for public clients; not universal
  Echoing attempted value ≠ disclosing registry; but negative oracle is still a CONCERN
  Not REPROCESS (no full registry disclosed); CONCERN (oracle attack vector)
Node 8 — Arbiter: CONCERN — Echoing "not found" for the attempted client_id creates
  a negative oracle for enumeration. Use generic "client authentication failed"
  to remove the distinction. Partial disclosure is not REPROCESS but is a CONCERN.
Node 9 — Format: CONCERN; negative oracle; Developer A argument partially rebutted
```

---

## Pair[CB4-3.4-643]

**A**
```
Architecture review — SSDF and device flow combined finding:

Finding 1 — SSDF PO.1: no formal security training program. Developer team of 12
  engineers, 3 years of continuous deployment. No documented training or awareness
  program. No security-specific onboarding for new engineers.
Finding 2 — Device flow: 7-char [A-Z2-9] codes (34^7 ≈ 52B codes), CSPRNG,
  15-minute device_code lifetime, server-side 25-attempt limit per device_code,
  HTTPS activation URI.
Finding 3 — CORS: /authorize wildcard ACAO; /token restricted to SPA origin.
```

**B**
```
CONCERN

Node 1 — Domain split: T-ssdf-process / T-rfc-oauth2 / T-rfc-oauth2
Node 2 — Translation:
  F1: SSDF PO.1 (org training / security awareness)
  F2: RFC 8628 §6.1 (device flow)
  F3: CORS endpoint configuration
Node 3 — Plan: F1=PO.1-gap; F2=device-flow-check; F3=CORS-authorize-check
Node 4 — Processing:
  F1: no formal security training → SSDF PO.1 gap → CONCERN normally
      No production vulnerability traceable to knowledge gap reported → CONCERN (not REPROCESS)
      Established pattern (ext21): PO.1 absence = CONCERN; escalates to REPROCESS only when
      production vulnerability traceable to knowledge gap
  F2: 34^7 ≈ 52B codes; CSPRNG; 25-attempt limit; HTTPS → RFC 8628 §6.1 both controls → PASS
  F3: /authorize wildcard → acceptable (authorization endpoint returns redirects, not tokens)
      /token restricted to SPA origin → correct → PASS
      Overall CORS: PASS
  Three findings: CONCERN + PASS + PASS = CONCERN
Node 5 — Application: CONCERN (one CONCERN; no REPROCESS)
Node 6 — Translation: CONCERN — PO.1 gap; device flow and CORS correct
Node 7 — Arbiter: T-ssdf-process + T-rfc-oauth2
Node 8 — Arbiter verdict:
  CONCERN — One finding requires attention. PO.1: no formal security training program
  for a 12-engineer team with 3 years of continuous deployment and no security onboarding
  for new engineers. This is a PO.1 process gap (CONCERN). No production vulnerability
  has been reported as traceable to a knowledge gap, so escalation to REPROCESS does not
  apply. Device flow implementation (F2) and CORS configuration (F3) are both correct.
  Remediate: establish documented security training and onboarding program.
Node 9 — Format: CONCERN; F1 PO.1 primary; F2/F3 PASS noted
```

**Reason**
F1 is CONCERN (PO.1 no training, no traceable production vulnerability → CONCERN not REPROCESS per ext21 escalation rule). F2 is PASS (34^7 ≈ 52B codes + 25-attempt limit → both §6.1 controls). F3 is PASS (/authorize wildcard = acceptable, established in ext24 PASS baseline). One CONCERN + two PASS = CONCERN.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-ssdf-process / T-rfc-oauth2 / T-rfc-oauth2
Node 4:
  F1: no security training → PO.1 → CONCERN (no traceable vulnerability; no REPROCESS escalation)
  F2: 34^7 ≈ 52B + 25-attempt limit + HTTPS → §6.1 PASS
  F3: /authorize wildcard (redirects); /token restricted → PASS
  One CONCERN + two PASS → CONCERN
Node 8 — Arbiter: CONCERN — PO.1 training gap (F1) is the sole finding. Device flow
  (F2) and CORS (F3) are correctly implemented. Establish security training program.
Node 9 — Format: CONCERN; F1 primary; F2/F3 PASS
```

---

## Pair[CB4-3.4-644]

**A**
```
Developer proposes token transmission via cookie for mobile-web hybrid application:

"Instead of Authorization: Bearer header, we'll use a session cookie to carry the
access token for our mobile web view. Cookie settings: HttpOnly=true, Secure=true,
SameSite=Strict. The access token is the JWT directly, stored in the cookie value.
CORS wildcard is fine since it's a cookie-based auth, not a Bearer token."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 6750) / T-asvs-token (V9.3.1)
Node 2 — Translation: access token storage and transmission method; CORS configuration
Node 3 — Plan: cookie storage of JWT access token; CORS implications
Node 4 — Processing:
  HttpOnly=true: JavaScript cannot read the token → better than localStorage → PASS
  Secure=true: only transmitted over HTTPS → transport security → PASS
  SameSite=Strict: protects against CSRF for state-changing requests → PASS
  JWT access token in cookie value:
    Not logged as a URL parameter (different risk from query-string pattern) → not V9.3.1 URL-logging issue
    HttpOnly prevents XSS exfiltration of the token → improvement over localStorage
    RFC 6750 §2.2: cookie-based token transmission is defined; not SHOULD NOT like §2.3
    This is an acceptable alternate transmission method for web contexts
  CORS wildcard claim:
    "Cookie-based auth, not Bearer" — this is incorrect framing
    CORS and cookies interact: SameSite=Strict means the cookie will NOT be sent on
    cross-origin requests (SameSite=Strict requires same-origin navigation)
    If SameSite=Strict, cross-origin requests will not carry the cookie → CORS wildcard
    is less dangerous (no credentials sent) but wildcard is still poor hygiene
    However: if application later loosens SameSite → wildcard becomes dangerous
    The rationale "cookie-based = CORS wildcard fine" is architecturally weak
  Overall: cookie approach is not inherently wrong; CORS wildcard on token endpoint
    is a hygiene CONCERN even with SameSite=Strict; developer's rationale is incomplete
Node 5 — Application: CONCERN (CORS wildcard hygiene; acceptable cookie approach)
Node 6 — Translation: CONCERN — token-in-cookie is acceptable; CORS wildcard rationale weak
Node 7 — Arbiter: T-rfc-oauth2 — token transmission method + CORS
Node 8 — Arbiter verdict:
  CONCERN — HttpOnly+Secure+SameSite=Strict cookie storage for access tokens is a
  legitimate transmission approach (RFC 6750 §2.2) and prevents XSS exfiltration.
  The gap is the CORS wildcard justification: "cookie-based = CORS fine" is architecturally
  weak. SameSite=Strict prevents cross-origin cookie sending currently, but the CORS
  wildcard configuration persists independently. If SameSite policy is ever loosened,
  the wildcard becomes dangerous. Restrict CORS to the specific SPA/mobile origin rather
  than relying on cookie behavior to justify wildcard.
Node 9 — Format: CONCERN; cookie approach acceptable; CORS rationale weak
```

**Reason**
Cookie-based JWT storage with HttpOnly+Secure+SameSite=Strict is an acceptable approach (RFC 6750 §2.2, not the SHOULD NOT of §2.3). The CONCERN is the CORS wildcard justified by "cookie-based auth" — the rationale is weak and architecture-dependent. This Stratum D argument tests whether the model can accept a non-Bearer approach as valid while still flagging the CORS reasoning as insufficient.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-token
Node 4:
  HttpOnly + Secure + SameSite=Strict → acceptable cookie configuration → PASS
  JWT in cookie value → RFC 6750 §2.2 (not §2.3 SHOULD NOT) → acceptable
  CORS wildcard: "cookie-based = fine" → weak rationale; SameSite=Strict currently
    prevents cross-origin cookie sending, but wildcard persists if SameSite loosened
  CONCERN for CORS wildcard rationale; cookie approach itself is acceptable
Node 8 — Arbiter: CONCERN — Cookie transmission approach is acceptable. CORS wildcard
  justified by cookie behavior is architecturally weak. Restrict CORS to specific origins
  regardless of cookie policy.
Node 9 — Format: CONCERN; cookie approach PASS; CORS rationale CONCERN
```

---

## Pair[CB4-3.4-645]

**A**
```
Security review — device flow + error response combined compliance scenario:

Scenario: a connected TV application uses device authorization flow.
Implementation:
1. user_code: 9-char [A-Z2-9] (34^9 ≈ 94.9T codes), CSPRNG, 20-min TTL
2. Server-side limit: 15 attempts per device_code, then invalidation
3. Error response on /token: error code + error_description:
   "The device code has expired or was already used. Please restart the device
   activation process to get a new code."
4. Verification URI: https://example.com/link (HTTPS enforced)
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628 + RFC 6749 §5.2)
Node 2 — Translation: device flow brute-force + error response compliance
Node 3 — Plan: F1=§6.1 controls; F2=error-description-disclosure; F3=HTTPS-URI
Node 4 — Processing:
  F1: 34^9 ≈ 94.9T codes + CSPRNG → extremely high entropy
      15-attempt limit per device_code → server-side enforcement present
      RFC 8628 §6.1 MUST: both controls present → PASS
  F2: error_description: "device code has expired or was already used. Please restart..."
      This discloses: (a) device code expired vs used — status information about the code
      Is this a disclosure risk? Distinction between "expired" and "used":
        "expired" → reveals the code TTL has elapsed (operational, not sensitive)
        "used" → reveals the code has been consumed (correct; prevents double-use confusion)
        Neither reveals valid/invalid code IDs, client IDs, or internal registry data
        This is contextually appropriate guidance → PASS
      Compare: enumerating registered client IDs = REPROCESS; operational device code
        status message = PASS (user-facing guidance with no security-relevant enumeration)
  F3: HTTPS verification URI → RFC 8628 §3.3 → PASS
Node 5 — Application: PASS × 3
Node 6 — Translation: all elements compliant
Node 7 — Arbiter: T-rfc-oauth2 — device flow + error response baseline
Node 8 — Arbiter verdict:
  PASS — Device flow implementation fully satisfies RFC 8628. 34^9 ≈ 94.9T keyspace
  with CSPRNG and 15-attempt server-side limit per device_code. Error response includes
  a contextually appropriate device code status message that does not enumerate any
  security-relevant identifiers. HTTPS verification URI. No findings.
Node 9 — Format: PASS; §6.1 confirmed; error_description guidance-only noted
```

**Reason**
Device flow is correct: 94.9T keyspace + 15-attempt limit. Error_description contains operational guidance ("expired or already used") without any security-relevant enumeration — this is distinguishable from the CWE-200 client registry disclosure (REPROCESS). The distinction between "guidance text" and "enumeration" is the Stratum B subtlety trained here.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628 + RFC 6749)
Node 4:
  34^9 ≈ 94.9T + 15-attempt limit → both §6.1 controls → PASS
  Error: "code expired or used — restart activation" → operational guidance, no enumeration → PASS
  HTTPS URI → PASS
Node 8 — Arbiter: PASS — Device flow correct (high entropy + server limit). Error
  description provides operational guidance without security-relevant disclosure. HTTPS URI.
Node 9 — Format: PASS; all three elements conformant
```

---

## Pair[CB4-3.4-646]

**A**
```
Post-remediation review for a full ext24 compliance pass:

All four ext24 domain gaps have been remediated:
1. Token transmission: moved from URL query params to Authorization: Bearer header.
   Structured logging excludes tokens. No full-URL logging in production.
2. Error responses: all authorization errors return only {"error": "invalid_request"}
   or other appropriate RFC 6749 §5.2 codes. No error_description. No internal data.
3. Device flow: upgraded to 8-char [A-Z2-9] CSPRNG codes (34^8 ≈ 2.8T keyspace),
   server-side 20-attempt limit per device_code, HTTPS verification URI.
4. CORS: /token restricted to https://app.example.com; /introspect has no CORS headers;
   /authorize has wildcard ACAO (acceptable for redirect-based responses).
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 + T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2
Node 2 — Translation: all four ext24 domains post-remediation
Node 3 — Plan: verify each remediation against original finding criterion
Node 4 — Processing:
  F1: Bearer header + no URL tokens + no token logging → V9.3.1 + RFC 6750 §2.1 → PASS
  F2: error code only, no error_description → RFC 6749 §5.2 minimum disclosure → PASS
      Omitting error_description is acceptable (SHOULD in spec, not MUST)
  F3: 34^8 ≈ 2.8T codes + CSPRNG + 20-attempt limit + HTTPS → RFC 8628 §6.1 → PASS
  F4: /token restricted; /introspect no CORS headers (backend-only, stricter than wildcard);
      /authorize wildcard (redirects only, no token payload) → PASS
Node 5 — Application: PASS × 4
Node 6 — Translation: all remediations satisfy their respective requirements
Node 7 — Arbiter: comprehensive ext24 post-remediation PASS
Node 8 — Arbiter verdict:
  PASS — All four ext24 domain patterns are correctly remediated. Token transmission
  uses Bearer header with no URL usage and no logging. Error responses follow minimum
  disclosure (error code only). Device flow satisfies RFC 8628 §6.1 with 2.8T keyspace
  and 20-attempt server-side limit. CORS is endpoint-appropriately configured: token
  restricted, introspect no-headers (backend-only), authorize wildcard (redirect-only).
  No findings.
Node 9 — Format: PASS; four domains confirmed; no remaining findings
```

**Reason**
Comprehensive post-remediation PASS for all ext24 domains. Each remediation is verified against its specific requirement. The omitting of error_description entirely is confirmed as acceptable (SHOULD in RFC 6749 §5.2). The /authorize wildcard is confirmed as acceptable (redirects, not tokens). This closes the ext24 cross-source set.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2 / T-rfc-oauth2
Node 4:
  F1: Bearer + no logging → PASS
  F2: error code only → PASS (SHOULD in spec; omission acceptable)
  F3: 34^8 + 20-attempt limit + HTTPS → §6.1 PASS
  F4: /token restricted; /introspect no-headers; /authorize wildcard OK → PASS
Node 8 — Arbiter: PASS — All four ext24 domains remediated. No findings remaining.
Node 9 — Format: PASS; four domains confirmed
```
