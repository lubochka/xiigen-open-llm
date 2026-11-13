# CB4 Stage 3 — Node 8 Arbiter Baseline Set ext24
# Sequences CB4-3.4-621 through CB4-3.4-628
# Stratum A/B/D — access-token-query-string / error-response-leakage /
#                  device-flow-security / CORS-introspection
# Running total after this file: 1256 instances (628 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-621]

**A**
```
API gateway configuration review:

The API gateway logs all incoming requests at DEBUG level, including full URLs.
OAuth 2.0 access tokens are passed by clients as a query parameter:
  GET /api/orders?access_token=eyJhbGciOiJSUzI1NiJ9...&order_id=12345

Developer comment: "This is supported in RFC 6750 §2.3 — it's a defined bearer
token transmission method. The gateway only logs internally and the logs are
behind the same security perimeter."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 6750 bearer token)
Node 2 — Translation: bearer token transmission method — sub-phase 3.4 in scope
Node 3 — Plan: check RFC 6750 §2.3 requirements; check token in URL risks
Node 4 — Processing:
  RFC 6750 §2.3: "SHOULD NOT use" access tokens in URI query parameters
    Reason given in spec: URIs logged by servers, referrer headers, browser history —
    all of which expose the token outside the transport-layer protection
  Developer claim: "it's a defined method" — TRUE; RFC 6750 does define it
    However: RFC 6750 §2.3 includes an explicit SHOULD NOT and lists the exact risks
    "Defined" ≠ "recommended"; the spec documents it to label it as a last resort
  Developer claim: "logs are behind the same security perimeter" — insufficient
    Gateway logs full URLs including query params at DEBUG level
    Log aggregation systems, log forwarders, SIEM pipelines — all within perimeter
    but each system is an exposure point; tokens in logs are credential exposure (V9.3.1)
    "Same perimeter" argument is the same pattern as "internal network = no auth" — rejected
  RFC 6750 §2.3 is a SHOULD NOT, not a MUST NOT
    V9.3.1 (MUST NOT log credential-equivalent tokens) applies to the logging
    SHOULD NOT transmission + MUST NOT logging = combined REPROCESS
  The combination: tokens are in URLs (SHOULD NOT), and those URLs are logged
    in full at DEBUG level in production (MUST NOT) → REPROCESS
Node 5 — Application: REPROCESS (MUST NOT logging + SHOULD NOT transmission combined)
Node 6 — Translation: V9.3.1 MUST NOT violation for logging; RFC 6750 §2.3 SHOULD NOT
Node 7 — Arbiter: T-asvs-token + T-rfc-oauth2 — token exposure in logs via URL
Node 8 — Arbiter verdict:
  REPROCESS — Access tokens in URL query parameters combined with full-URL debug
  logging violates V9.3.1: credential-equivalent tokens MUST NOT appear in production
  logs. RFC 6750 §2.3 itself labels query-parameter transmission SHOULD NOT precisely
  because it leads to URL logging and referrer-header exposure. The perimeter argument
  does not neutralize the risk — log aggregators within the perimeter are additional
  exposure points. Remediate: transmit access tokens via Authorization: Bearer header
  (RFC 6750 §2.1) and remove full URL logging of requests with tokens.
Node 9 — Format: REPROCESS; V9.3.1 and RFC 6750 §2.3 both cited; logging + URL gap
```

**Reason**
Access token in query string (RFC 6750 §2.3 SHOULD NOT) combined with full-URL debug logging in production (V9.3.1 MUST NOT) triggers REPROCESS. The SHOULD NOT of transmission combines with the MUST NOT of logging to produce a definitive violation. The "defined in spec" and "internal perimeter" arguments both follow established rejected patterns.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-token
Node 4 — Processing:
  Query parameter transmission: RFC 6750 §2.3 SHOULD NOT (spec labels it for exceptional use only)
  Full URL DEBUG logging in production: V9.3.1 MUST NOT log credential-equivalent tokens
  "RFC 6750 defines it" → defined as a last resort, explicitly marked SHOULD NOT
  "Internal perimeter" → log aggregators within perimeter are still credential exposure points
  Combined: SHOULD NOT + MUST NOT → REPROCESS
Node 8 — Arbiter verdict:
  REPROCESS — Token in URL query parameter plus full-URL production logging violates
  V9.3.1. Transmit tokens via Authorization: Bearer header; disable full-URL token logging.
Node 9 — Format: REPROCESS; V9.3.1 primary; RFC 6750 §2.3 secondary
```

---

## Pair[CB4-3.4-622]

**A**
```
OAuth 2.0 error response implementation review:

When authorization fails, the authorization server returns:
  HTTP 400
  Content-Type: application/json
  {
    "error": "invalid_client",
    "error_description": "Client ID abc-corp-service-01 not found in registry.
      Expected client IDs: [spa-frontend, mobile-ios, mobile-android, api-gateway-01,
      internal-svc-01, analytics-svc-02]. Authentication attempt from IP 203.0.113.45.",
    "request_id": "req-a1b2c3d4"
  }
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authentication
Node 2 — Translation: error response information leakage — sub-phase 3.4 in scope
Node 3 — Plan: check RFC 6749 error response requirements; check information disclosure
Node 4 — Processing:
  RFC 6749 §5.2: error response MUST include "error" parameter; SHOULD include
    "error_description" as human-readable text; no requirement for internal detail
  "invalid_client" → correct error code per RFC 6749 §5.2 → PASS on code choice
  error_description content analysis:
    "Client ID abc-corp-service-01 not found in registry" → reveals attempted client_id
    "Expected client IDs: [spa-frontend, mobile-ios, ...]" → enumerates all registered
      client IDs in the authorization server → REPROCESS (reconnaissance payload)
    "Authentication attempt from IP 203.0.113.45" → reveals caller IP in error response
  Information disclosure classification:
    Enumeration of registered client IDs = complete client registry disclosure
    This allows an attacker to: enumerate all valid client IDs, target specific clients,
    craft attacks against known client IDs (e.g., impersonation, injection)
    CWE-200 (Exposure of Sensitive Information) + V8.2.2 (sensitive data in responses)
  "request_id" → acceptable; non-sensitive tracing identifier
  The enumeration of registered clients is the critical violation:
    No specification requires disclosing valid client IDs in error responses
    Violates the principle of minimum necessary disclosure
    V8.2.2: sensitive operational data (client registry) exposed to unauthenticated caller
Node 5 — Application: REPROCESS (client registry enumeration in error response)
Node 6 — Translation: CWE-200 information disclosure + V8.2.2
Node 7 — Arbiter: T-asvs-authorization + T-rfc-oauth2 — error response information leakage
Node 8 — Arbiter verdict:
  REPROCESS — The error response discloses the full list of registered client IDs to
  any caller who submits an unrecognized client_id. This is a reconnaissance payload:
  an attacker can enumerate all clients registered with the authorization server using
  a single failed request. CWE-200 and V8.2.2 violation. The caller IP disclosure
  is a secondary concern (often expected in server logs but not in API responses).
  Remediate: error_description should state only "client authentication failed" or
  similar — never enumerate valid identifiers. Remove client list enumeration and
  IP from error_description. Retain request_id for tracing.
Node 9 — Format: REPROCESS; client enumeration as primary; CWE-200 + V8.2.2 cited
```

**Reason**
The error response enumerates all registered client IDs — a reconnaissance payload. This is a REPROCESS: it discloses operational security data (the complete client registry) to an unauthenticated caller. RFC 6749 does not require this; it is a gratuitous disclosure violating minimum-necessary-disclosure and CWE-200/V8.2.2.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authorization
Node 4 — Processing:
  "invalid_client" code → correct (RFC 6749 §5.2)
  error_description enumerates all valid client IDs → CWE-200 reconnaissance payload
  IP address in error response → secondary concern
  request_id → acceptable tracing token
  Client registry enumeration = complete client inventory disclosure to unauthenticated caller
Node 8 — Arbiter verdict:
  REPROCESS — Enumerating all registered client IDs in an error response is a CWE-200
  violation. An attacker obtains the full client registry from a single failed request.
  Replace error_description with a generic authentication failure message.
Node 9 — Format: REPROCESS; client enumeration primary; CWE-200 cited
```

---

## Pair[CB4-3.4-623]

**A**
```
Security review of OAuth 2.0 Device Authorization Grant (RFC 8628) implementation:

Implementation details:
- device_code lifetime: 30 minutes
- user_code: 5-character uppercase alphanumeric [A-Z0-9], randomly generated
- Polling interval: 5 seconds
- Polling rate limiting: none ("the spec says to back off on slow_down response")
- user_code display: shown in the device UI and sent to https://example.com/activate
- No brute-force protection on the verification URI for user_code guessing
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628 device authorization grant)
Node 2 — Translation: device flow security — sub-phase 3.4 in scope
Node 3 — Plan: check user_code entropy; check polling rate limiting; check brute-force
Node 4 — Processing:
  user_code 5-char [A-Z0-9]: alphabet size = 36; keyspace = 36^5 = 60,466,176
    With a 30-minute device_code lifetime:
    No rate limiting → attacker can brute-force verification endpoint freely
    60 million codes ÷ (requests/sec without limiting) = trivially enumerable in minutes
    RFC 8628 §6.1: authorization server SHOULD use codes of sufficient entropy AND
      MUST rate-limit the verification endpoint to prevent brute-force
    MUST rate-limit → absence of rate limiting = REPROCESS
  user_code display → expected per spec → PASS
  device_code lifetime 30 minutes → reasonable; RFC 8628 does not mandate a maximum
  Polling interval 5 seconds → acceptable; RFC 8628 §3.5 minimum is server-defined
  "spec says to back off on slow_down" → CWE-307 (brute force) — slow_down is client-
    directed backoff; the server is still responsible for enforcing rate limits
    "Client backs off = server protected" → REJECTED
    slow_down is a cooperative mechanism; a malicious client ignores it
    Server-side rate limiting is the MUST requirement; slow_down is a supplement
Node 5 — Application: REPROCESS (no server-side rate limiting on verification endpoint)
Node 6 — Translation: RFC 8628 §6.1 MUST violation; CWE-307
Node 7 — Arbiter: T-rfc-oauth2 — device flow brute-force protection
Node 8 — Arbiter verdict:
  REPROCESS — No server-side rate limiting on the verification URI violates RFC 8628
  §6.1, which MUST rate-limit the verification endpoint. The user_code keyspace
  (5-char [A-Z0-9] = ~60M codes) with a 30-minute lifetime and no server-side limiting
  is trivially brute-forceable. The slow_down response mechanism applies to cooperative
  clients; a malicious client ignores it. CWE-307 (brute-force) applies. Remediate:
  add server-side rate limiting on /activate (e.g., max 10 attempts per device_code,
  then invalidate); consider extending user_code to 8+ characters for deeper entropy.
Node 9 — Format: REPROCESS; RFC 8628 §6.1 MUST cited; slow_down argument rejected
```

**Reason**
RFC 8628 §6.1 MUST rate-limit the verification endpoint. No server-side rate limiting with a 5-char [A-Z0-9] code (~60M keyspace) and 30-minute lifetime is a clear REPROCESS. The slow_down argument is rejected: it is client-cooperative, not server-enforced. CWE-307 brute-force pattern applies to the device flow.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628) / T-cwe-authn (CWE-307)
Node 4 — Processing:
  5-char [A-Z0-9] user_code → 36^5 = 60M keyspace
  30-minute device_code lifetime + no server-side rate limiting → brute-forceable
  "slow_down = protection" → REJECTED (client-cooperative; malicious clients ignore it)
  RFC 8628 §6.1 MUST rate-limit → absence = REPROCESS
Node 8 — Arbiter verdict:
  REPROCESS — Verification endpoint has no server-side rate limiting (RFC 8628 §6.1 MUST).
  5-char code with 60M keyspace is enumerable without server controls. slow_down is
  supplemental, not a replacement for server enforcement. Add server-side rate limiting.
Node 9 — Format: REPROCESS; RFC 8628 §6.1 primary; CWE-307 noted
```

---

## Pair[CB4-3.4-624]

**A**
```
OAuth 2.0 Device Authorization Grant (RFC 8628) — correct implementation review:

- device_code lifetime: 15 minutes
- user_code: 8-character uppercase [A-Z] (excluding easily confused: I, O, L),
  randomly generated by CSPRNG. Effective alphabet: 23 characters. Keyspace: 23^8 ≈ 78.3 billion
- Polling interval: 5 seconds, slow_down increases to 10 seconds after consecutive polls
- Server-side rate limiting: max 20 attempts per device_code; after limit, device_code invalidated
- Verification URI: https://example.com/activate (HTTPS enforced, no HTTP redirect)
- user_code comparison: case-insensitive, strips hyphens for display formatting
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628)
Node 2 — Translation: device authorization grant security — sub-phase 3.4 in scope
Node 3 — Plan: check entropy, lifetime, rate limiting, verification URI security
Node 4 — Processing:
  user_code: 23^8 ≈ 78.3 billion codes; CSPRNG generated → high-entropy, unpredictable
  Server-side rate limiting: 20 attempts per device_code → brute-force infeasible
    78B keyspace ÷ 20 attempts = attacker needs ~3.9B device_codes to expect success
    RFC 8628 §6.1 MUST rate-limit → satisfied → PASS
  device_code lifetime 15 minutes → reasonable, shorter than 30-minute example → PASS
  Polling: slow_down cooperative + server enforcement → defense in depth → PASS
  Verification URI HTTPS enforced → transport security for user_code entry → PASS
  user_code display formatting (hyphens stripped, case-insensitive) → user experience;
    case-insensitive comparison acceptable per RFC 8628 §6.1 (codes are shared via UI)
  No security gap identified
Node 5 — Application: PASS (all RFC 8628 §6.1 requirements met)
Node 6 — Translation: device flow correctly implemented
Node 7 — Arbiter: T-rfc-oauth2 — device flow baseline PASS
Node 8 — Arbiter verdict:
  PASS — Device authorization grant implementation meets RFC 8628 requirements.
  user_code entropy (~78.3 billion codes via CSPRNG) with server-side 20-attempt
  limit per device_code makes brute-force infeasible. Verification URI enforces HTTPS.
  Polling implements both cooperative (slow_down) and server-side enforcement.
  15-minute device_code lifetime is reasonable. Case-insensitive comparison for
  display-friendly codes is acceptable per RFC 8628. No findings.
Node 9 — Format: PASS; RFC 8628 §6.1 compliance confirmed
```

**Reason**
Correct device flow implementation: high-entropy user_code (78B keyspace from 23-char filtered alphabet), server-side rate limiting (20 attempts per device_code), HTTPS verification URI, CSPRNG generation. All RFC 8628 §6.1 MUST requirements satisfied. Pairs with CB4-3.4-623 as the PASS baseline for device flow.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628)
Node 4 — Processing:
  23^8 ≈ 78B keyspace + CSPRNG → unpredictable codes → PASS
  20-attempt limit per device_code → RFC 8628 §6.1 MUST satisfied → PASS
  HTTPS verification URI → transport security → PASS
  15-min lifetime + slow_down + server rate limit → defense in depth → PASS
Node 8 — Arbiter verdict:
  PASS — Device authorization grant correctly implements RFC 8628. High-entropy codes,
  server-side rate limiting, and HTTPS verification URI all satisfied. No findings.
Node 9 — Format: PASS; RFC 8628 conformance noted
```

---

## Pair[CB4-3.4-625]

**A**
```
CORS (Cross-Origin Resource Sharing) configuration review for OAuth 2.0 endpoints:

/token endpoint:    Access-Control-Allow-Origin: *
                    Access-Control-Allow-Methods: POST
                    Access-Control-Allow-Headers: Content-Type, Authorization

/introspect endpoint: Access-Control-Allow-Origin: *
                      Access-Control-Allow-Methods: POST
                      Access-Control-Allow-Headers: Content-Type, Authorization

Developer argument: "The token and introspect endpoints are POST-only. Modern browsers
require a preflight (OPTIONS) for non-simple requests with Authorization headers.
The wildcard origin is fine because it's protected by the required authentication
on each endpoint."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authentication
Node 2 — Translation: CORS configuration on token and introspection endpoints — sub-phase 3.4
Node 3 — Plan: check CORS wildcard on credentialed endpoints; check preflight argument
Node 4 — Processing:
  /token with CORS wildcard:
    Previously established pattern (ext17): CORS wildcard on token endpoint → REPROCESS
    Reason: POST with Content-Type: application/x-www-form-urlencoded is a SIMPLE request
    Simple requests do NOT trigger preflight OPTIONS
    A cross-origin page can submit a form POST to /token with client credentials
    wildcard ACAO allows the response to be read by any origin
    Developer claim: "preflight required for Authorization header" — TRUE for the
      Authorization header variant; but token endpoint clients use body parameters
      (grant_type, client_id, client_secret, code) not Authorization headers
    The token endpoint can be reached via simple POST from any origin → REPROCESS
  /introspect with CORS wildcard:
    Introspection endpoint reveals whether a token is active, its scope, subject, expiry
    Wildcard ACAO allows any origin to call introspection with an access token (if obtainable)
    and receive token metadata — including sub (user identity), scope, exp
    This is sensitive resource data; CORS wildcard = any origin can read it
    RFC 7662 §2.1: introspection MUST be authenticated → but CORS wildcard combined with
    authenticated introspection means: if an attacker obtains an access token (via
    other vector), they can introspect it from any origin they control
    More critically: CORS wildcard on an authenticated endpoint leaks token metadata
    to scripts on any compromised page in the user's browser
    V8.2.2: sensitive data exposure → REPROCESS for introspect wildcard
  Both endpoints: REPROCESS × 2
Node 5 — Application: REPROCESS (both endpoints)
Node 6 — Translation: CORS wildcard on token and introspect — both REPROCESS
Node 7 — Arbiter: T-rfc-oauth2 + T-asvs-authorization — CORS misconfiguration
Node 8 — Arbiter verdict:
  REPROCESS — CORS wildcard (Access-Control-Allow-Origin: *) on both the token and
  introspect endpoints violates the established pattern. Token endpoint: simple POST
  (form body) does not trigger preflight — a malicious page can call it from any origin
  and read the response. Introspect endpoint: wildcard allows any origin to read token
  metadata including sub, scope, and expiry (V8.2.2 sensitive data disclosure).
  The preflight argument incorrectly assumes Authorization-header access — token endpoint
  clients use form body parameters. Remediate: restrict ACAO to specific trusted origins
  (the SPA's deployment origin, not wildcard) on both endpoints.
Node 9 — Format: REPROCESS; both endpoints cited; preflight argument rejected
```

**Reason**
CORS wildcard on /token is REPROCESS (established in ext17): simple POST with form body triggers no preflight. CORS wildcard on /introspect is a new REPROCESS: token metadata (sub, scope, exp) is sensitive data (V8.2.2), and wildcard allows any origin to read introspection results. The preflight argument is the same rejected pattern from ext17 ("preflight blocks it").

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authorization
Node 4 — Processing:
  /token wildcard: form-body POST = simple request, no preflight → any origin can call and read → REPROCESS
  /introspect wildcard: token metadata (sub/scope/exp) readable from any origin → V8.2.2 → REPROCESS
  "Preflight required for Authorization header" → true for that variant;
    but token endpoint uses form body, not Authorization header → argument rejected
  Both endpoints REPROCESS
Node 8 — Arbiter verdict:
  REPROCESS — Wildcard CORS on /token (simple POST, no preflight needed) and /introspect
  (sensitive token metadata readable cross-origin) both require REPROCESS. Restrict
  ACAO to specific trusted deployment origins.
Node 9 — Format: REPROCESS; both endpoints; preflight argument rejected
```

---

## Pair[CB4-3.4-626]

**A**
```
CORS configuration review — restricted token endpoint, open introspect for API gateway:

/token endpoint:
  Access-Control-Allow-Origin: https://app.example.com
  (only the SPA's registered deployment origin)

/introspect endpoint:
  Access-Control-Allow-Origin: *
  (developer comment: "introspect is called by API gateways and backend services —
   not browsers. They won't be subject to CORS restrictions anyway. The wildcard
   is just declarative for completeness.")
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-authorization
Node 2 — Translation: CORS on token and introspect endpoints — sub-phase 3.4 in scope
Node 3 — Plan: token endpoint CORS check; introspect CORS check; developer argument
Node 4 — Processing:
  /token endpoint: restricted to https://app.example.com → PASS
    This is the correct pattern — only registered deployment origin allowed
  /introspect endpoint: wildcard ACAO → developer claim: "only called by backends"
    Developer argument: backend services are not browsers, so CORS doesn't apply
    This is PARTIALLY TRUE: CORS is a browser security mechanism; non-browser servers
    ignore CORS headers and call endpoints directly regardless of ACAO value
    However:
    1. If any browser-side JavaScript ever calls /introspect (e.g., for client-side
       token validation, status display), wildcard allows any origin to receive metadata
    2. Wildcard ACAO is a signal to browsers that the endpoint is safe for cross-origin
       access; if deployment evolves to include browser-facing introspect calls,
       this misconfiguration is already in place
    3. Setting ACAO: * on an endpoint that returns sensitive token metadata (even if
       currently backend-only) is a low-grade but real exposure risk if usage expands
    This is not a current MUST violation if /introspect is genuinely backend-only
    BUT: it is a configuration hygiene gap — wildcard on a sensitive endpoint
    with "no browsers call it" as the defense is a CONCERN
    /token endpoint is correctly restricted → not a concern there
  Overall: /token PASS; /introspect CONCERN (wildcard hygiene on sensitive endpoint)
Node 5 — Application: CONCERN
Node 6 — Translation: CONCERN — wildcard on sensitive endpoint with argument-dependent PASS
Node 7 — Arbiter: T-rfc-oauth2 — CORS hygiene on introspect
Node 8 — Arbiter verdict:
  CONCERN — Token endpoint CORS restriction to the registered SPA origin is correct.
  Introspect endpoint wildcard ACAO is a hygiene concern: while the developer is
  correct that CORS applies only to browsers and backend-to-backend calls are not
  CORS-restricted, setting ACAO: * on an endpoint returning sensitive token metadata
  is unnecessary and creates risk if the endpoint ever becomes browser-accessible.
  Restrict /introspect ACAO to the specific origins of known consumers (API gateway
  origins, internal tooling origins), or remove ACAO entirely if no browser calls it.
  Not a REPROCESS because there is no current MUST violation if introspect is
  genuinely backend-only.
Node 9 — Format: CONCERN; /token PASS; /introspect wildcard hygiene gap
```

**Reason**
Token endpoint is correctly restricted (PASS). Introspect wildcard is a hygiene gap: the developer's argument (CORS applies to browsers only; backends are unaffected) is partially correct, which prevents REPROCESS. But wildcard on a sensitive-metadata endpoint with "backends don't use CORS" as the defense is CONCERN — the configuration is risky if usage evolves. This is the Stratum B nuanced-argument variant.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2
Node 4 — Processing:
  /token: restricted to https://app.example.com → PASS
  /introspect: wildcard; "backends don't use CORS" → partially true; not a MUST violation
  Wildcard on sensitive-metadata endpoint = hygiene gap; risk if browser-accessible in future
  Not REPROCESS (no current MUST violation); CONCERN for hygiene
Node 8 — Arbiter verdict:
  CONCERN — Token endpoint correctly restricted. Introspect wildcard is a hygiene gap:
  backend CORS argument partially valid but unnecessary wildcard on sensitive endpoint.
  Restrict introspect ACAO to specific consumer origins or remove entirely.
Node 9 — Format: CONCERN; /token PASS; /introspect hygiene gap
```

---

## Pair[CB4-3.4-627]

**A**
```
Device authorization flow brute-force protection dispute:

Developer A: "Our device flow user_codes are 6 characters, [A-Z2-9] (excludes 0,1
  for visual clarity). 34^6 ≈ 1.5 billion codes. With a 10-minute device_code
  lifetime and our server-side limit of 50 attempts per device_code, an attacker
  would need to issue 30 million device_code requests to expect to guess one valid
  code. That's effectively infeasible."

Developer B: "But RFC 8628 §6.1 says the server MUST consider device flow
  brute-force protection. Isn't 50 attempts high?"

Developer A: "50 attempts against 1.5 billion codes with active device_codes in
  flight is still astronomically safe. We've measured our traffic — peak concurrent
  active device_codes are under 1,000. Probability of hitting a valid code in
  50 attempts: 50/1,500,000,000 × 1,000 = 1 in 30,000 per attacker attempt."
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628)
Node 2 — Translation: device flow brute-force entropy and rate limiting — sub-phase 3.4
Node 3 — Plan: verify MUST requirement; evaluate entropy claim; assess per-device_code limit
Node 4 — Processing:
  RFC 8628 §6.1: server MUST consider brute-force protection → two controls required:
    1. Sufficient code entropy; 2. Server-side rate limiting per device_code
  user_code entropy: 34^6 ≈ 1.544 billion codes; CSPRNG (assumed) → high entropy
  Per-device_code limit: 50 attempts
    Against 1.5B codes: single device_code brute-force probability per attempt = 50/1.5B ≈ 3.3×10^-8
    Developer A's math for concurrent codes: correct methodology
    1,000 active device_codes × 50/1.5B = 3.3×10^-5 per attack attempt = effectively infeasible
  Is 50 the right limit? RFC 8628 does not specify a number — only MUST consider
    50 is high relative to minimal implementations (some use 10-20) but the entropy
    compensates: the security is probabilistically sound given the keyspace
  Both MUST conditions are met: sufficient entropy AND server-side rate limiting
  per device_code (even if generous, the limit exists and is enforced)
  No additional violations identified
Node 5 — Application: PASS (both RFC 8628 §6.1 controls present and probabilistically adequate)
Node 6 — Translation: RFC 8628 §6.1 MUST satisfied
Node 7 — Arbiter: T-rfc-oauth2 — device flow PASS
Node 8 — Arbiter verdict:
  PASS — Device authorization flow brute-force protection satisfies RFC 8628 §6.1.
  34^6 ≈ 1.5 billion user_code keyspace with CSPRNG generation provides high entropy.
  Server-side limit of 50 attempts per device_code is present and enforced.
  Developer A's probability analysis is correct: with peak 1,000 concurrent active codes,
  the probability of brute-forcing a valid code per attacker attempt is negligible (~3×10^-5).
  RFC 8628 does not mandate a specific attempt limit — the combination of high entropy
  and enforced per-device_code limit satisfies the MUST requirement. No findings.
Node 9 — Format: PASS; both §6.1 controls confirmed; probability analysis validated
```

**Reason**
RFC 8628 §6.1 MUST requires both high entropy AND server-side rate limiting. Both are present: 34^6 ≈ 1.5B keyspace + 50-attempt limit per device_code. RFC 8628 does not specify a particular attempt count. The probability analysis is mathematically correct. This is the Stratum B argument-validation variant — confirm that the developer's math is right and the MUST conditions are met.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (RFC 8628)
Node 4 — Processing:
  34^6 ≈ 1.5B codes; server-side 50-attempt limit per device_code → both RFC 8628 §6.1 controls present
  50 attempts vs 1.5B keyspace with 1,000 concurrent codes → probability ~3×10^-5 → infeasible
  RFC 8628 does not mandate a specific limit → PASS
Node 8 — Arbiter verdict:
  PASS — Both §6.1 controls satisfied: sufficient entropy and server-side per-device_code limit.
  Developer A's probability analysis is correct. 50 attempts against 1.5B-code keyspace
  with ≤1,000 concurrent active codes is probabilistically adequate.
Node 9 — Format: PASS; RFC 8628 §6.1 satisfied; math verified
```

---

## Pair[CB4-3.4-628]

**A**
```
Comprehensive security review — four findings:

1. Access tokens: 15-minute TTL, transmitted in Authorization: Bearer header for all
   API calls. No URL query parameter usage. No token logging.
2. Error responses: authorization failures return {"error": "invalid_client"} only.
   error_description is omitted entirely.
3. Device flow: 8-char [A-Z2-9] codes (34^8 ≈ 2.8 trillion keyspace), CSPRNG,
   15-minute device_code lifetime, server-side 30-attempt limit per device_code,
   HTTPS verification URI.
4. CORS: /token endpoint restricts to https://app.example.com;
   /introspect has no CORS headers (backend-to-backend only, no browser clients).
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2
Node 2 — Translation: all four findings in sub-phase 3.4 scope
Node 3 — Plan: check each finding against applicable requirements
Node 4 — Processing:
  F1: Bearer header transmission (RFC 6750 §2.1 preferred method) + no URL tokens +
      no token logging → V9.3.1 and RFC 6750 fully satisfied → PASS
  F2: error=invalid_client only; no enumeration, no IP, no internal detail → PASS
      RFC 6749 §5.2 satisfied; minimum necessary disclosure → PASS
  F3: 34^8 ≈ 2.8T codes, CSPRNG, 30-attempt limit, HTTPS URI → RFC 8628 §6.1 PASS
      Entropy is high; rate limit is enforced; transport is HTTPS → PASS
  F4: /token restricted to registered SPA origin → PASS
      /introspect no CORS headers → cleaner than wildcard; no exposure → PASS
      "No CORS headers" is stricter than wildcard for a backend-only endpoint → PASS
Node 5 — Application: PASS × 4
Node 6 — Translation: all applicable requirements met across all four findings
Node 7 — Arbiter: comprehensive multi-domain PASS
Node 8 — Arbiter verdict:
  PASS — All four components are correctly implemented. Access tokens transmitted via
  Bearer header with no URL usage and no logging (V9.3.1 + RFC 6750). Error responses
  disclose minimum necessary information (RFC 6749 §5.2). Device flow has high-entropy
  codes (34^8 ≈ 2.8T), server-side rate limiting, and HTTPS verification (RFC 8628 §6.1).
  CORS correctly restricts the token endpoint to the registered SPA origin; introspect
  has no CORS headers, which is appropriate for a backend-only endpoint. No findings.
Node 9 — Format: PASS; four findings each confirmed as conformant
```

**Reason**
All four new ext24 domain baselines in a comprehensive PASS: Bearer-header access tokens with no logging (V9.3.1 + RFC 6750), minimum-disclosure error responses (RFC 6749 §5.2), high-entropy device flow with server-side limiting (RFC 8628 §6.1), and correct CORS — restricted for /token, no-headers (stricter than wildcard) for /introspect. Pairs with CB4-3.4-621 through 626 as the comprehensive PASS baseline.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-token / T-rfc-oauth2 / T-rfc-oauth2
Node 4 — Processing:
  F1: Bearer header + no URL + no logging → V9.3.1 + RFC 6750 → PASS
  F2: error code only, no enumeration → RFC 6749 §5.2 minimum disclosure → PASS
  F3: 34^8 CSPRNG + 30-attempt limit + HTTPS → RFC 8628 §6.1 → PASS
  F4: /token restricted; /introspect no CORS headers (stricter than wildcard) → PASS
Node 8 — Arbiter verdict:
  PASS — All four ext24 domain baselines correctly implemented. Bearer transmission,
  minimum-disclosure errors, high-entropy device flow, and appropriately restricted CORS.
Node 9 — Format: PASS; all four findings conformant
```
