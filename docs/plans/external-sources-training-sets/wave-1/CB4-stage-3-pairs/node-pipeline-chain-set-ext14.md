# CB4 Stage 3 — Node Pipeline Chain Set Extension 14
# Sequences CB4-3.4-525 through CB4-3.4-532
# Stratum: C — full 9-node pipeline traces for ext20 patterns
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1064 instances

---

## Pair[CB4-3.4-525]

**A:** Full pipeline trace — OIDC hybrid flow. Authorization server supports `response_type=code id_token`. Application uses this hybrid flow — authorization response returns both code and ID token in the redirect URL fragment. Team: "Hybrid flow is standard OIDC — it gives us the ID token immediately without a second round trip."

**B:**
```
N1 · Domain Splitter
  Input: response_type=code id_token, ID token in front-channel URL, "standard OIDC" argument
  Domain signals: front-channel token exposure, response_type policy, OIDC BCP
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (OAuth 2.0 BCP)
  Split: [response-type-policy | front-channel-token-exposure | adversarial-argument]

N2 · Domain Translator
  response-type-policy → V10.x: code-only required; front-channel token delivery restricted
  front-channel-token-exposure → ID token in URL fragment: browser history, Referer, server logs
  adversarial-argument → "standard OIDC" → flag for N5 rejection (BCP supersedes legacy spec)

N3 · Scheme Planner
  Primary scheme: V10-response-type-policy
  Secondary scheme: OAuth2-BCP-front-channel
  Conflict check: OIDC Core defines hybrid flow; BCP and ASVS restrict it
  Plan: verify that "defined in spec" ≠ "recommended in security context"; assess front-channel risk

N4 · Domain Processor
  Hybrid flow response_type=code id_token:
    Authorization response fragment: code=xxx&id_token=eyJ... ✗
    ID token in URL fragment:
      → Browser history records fragment ✗
      → Referer header may include fragment ✗
      → Server access logs may record fragment ✗
      → JavaScript on the page can read fragment → XSS can extract ID token ✗
  OAuth 2.0 BCP (RFC 9700) §2.1.2: tokens (including ID tokens) MUST NOT be returned from
    the authorization endpoint directly
  ASVS V10: response_type=code (code-only) required; implicit and hybrid flows deprecated
  Team argument: "hybrid flow is standard OIDC"
    OIDC Core (2014) defines hybrid flow; OAuth 2.0 BCP (RFC 9700, 2023) restricts it
    "Defined in the spec" → spec was updated; BCP supersedes legacy response types
    → REJECTED (same argument pattern as implicit flow; "RFC 6749 supports it" analogy)

N5 · Applier
  Adversarial rejection: "standard OIDC" → REJECTED (BCP and ASVS restrict front-channel tokens)
  Apply V10-response-type-policy: FAIL — ID token in front channel
  Apply OAuth2-BCP: FAIL — token returned from authorization endpoint
  Verdict: REPROCESS

N6 · Result Translator
  Finding: ID token exposed in front-channel URL fragment via hybrid flow; BCP restricts this
  Adversarial claim rejected: spec definition ≠ current security recommendation
  Severity: REPROCESS

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (response_type policy)
  Secondary: RFC-OAuth2 (RFC 9700 BCP)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    response_type=code id_token: ID token in front channel ✗ — REPROCESS
  RFC-OAuth2:
    RFC 9700 §2.1.2: tokens from authorization endpoint ✗ — REPROCESS
  T-oauth-oidc verdict: REPROCESS
  T-rfc-oauth2 verdict: REPROCESS

N9 · Formatter
  Territory results: T-oauth-oidc=REPROCESS, T-rfc-oauth2=REPROCESS
  Graduation: REPROCESS present → Overall REPROCESS
  Remediation: use response_type=code only; retrieve ID token via token endpoint back-channel
Overall verdict: REPROCESS
```

**Reason:** OIDC hybrid flow returns the ID token in the front-channel URL fragment, exposing it to browser history, Referer headers, and JavaScript. OAuth 2.0 BCP (RFC 9700) restricts front-channel token delivery. "Standard OIDC" argument rejected — OIDC Core defined it; BCP updated the security guidance. REPROCESS.

**Phrasing-B:** Pipeline trace for OIDC hybrid flow with response_type=code id_token. Team: "Hybrid is standard OIDC — we get the ID token immediately." Walk through all 9 nodes.

---

## Pair[CB4-3.4-526]

**A:** Full pipeline trace — state entropy CONCERN. OAuth authorization request includes state generated as a 4-character random hex string (e.g., `a3f7`). State is validated at callback. Application: financial services. Team: "State is random and validated — CSRF binding is in place."

**B:**
```
N1 · Domain Splitter
  Input: 4-char hex state, validated at callback, financial services context
  Domain signals: CSRF binding, state entropy, high-security context requirements
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 6749 §4.1.1)
  Split: [state-mechanism | state-entropy | risk-context]

N2 · Domain Translator
  state-mechanism → V10.2.6: state present and validated ✓
  state-entropy → 4 hex chars = 16 bits entropy (62,536 combinations) ✗
  risk-context → financial services: high-security context may apply stricter standards

N3 · Scheme Planner
  Primary scheme: V10-2-6-state-security
  Secondary scheme: entropy-requirement
  Conflict check: mechanism correct; entropy very low even for standard contexts
  Plan: measure entropy; compare to 128-bit minimum; assess financial context severity

N4 · Domain Processor
  4-char hex: 16^4 = 65,536 combinations = 16 bits entropy
    Extremely low: brute force with 65,536 attempts covers entire space ✗
    Guessing rate: at 1 req/sec, full space explored in ~18 hours; at 100 req/sec = ~11 minutes
    Financial services: high-stakes session → attacker motivated to brute force ✗
  Minimum recommended entropy for state: 128 bits (ASVS CSRF token guidance)
    16 bits is 8× below 128-bit minimum
  Mechanism: state present ✓; validated ✓ — mechanism correct
  Entropy: critically inadequate ✗ — CONCERN elevated by financial context
  Team argument: "random and validated" → randomness is low-entropy; validation correct
    → "Validation present = sufficient" rejected (entropy is the attack surface)

N5 · Applier
  Apply V10-2-6-state-security: CONCERN — mechanism present; entropy dangerously low
  Financial context does not change graduation (CONCERN remains CONCERN unless hard MUST absent)
  Verdict: CONCERN

N6 · Result Translator
  Finding: state entropy critically low (16 bits); full state space brute-forceable in minutes
  Severity: CONCERN — mechanism exists; entropy gap is the concern

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (V10.2.6)
  Secondary: RFC-OAuth2 (RFC 6749 §4.1.1)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    V10.2.6: state present ✓; validated ✓; entropy=16 bits ✗ (far below 128-bit min) — CONCERN
  RFC-OAuth2:
    RFC 6749 §4.1.1: state SHOULD be unguessable — 16-bit state is guessable — CONCERN
  T-oauth-oidc verdict: CONCERN
  T-rfc-oauth2 verdict: CONCERN

N9 · Formatter
  Territory results: T-oauth-oidc=CONCERN, T-rfc-oauth2=CONCERN
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: generate state using CSPRNG with ≥128 bits of entropy; Base64url-encode
Overall verdict: CONCERN
```

**Reason:** 4-character hex state = 16 bits entropy = 65,536 combinations — trivially brute-forceable. State mechanism (present and validated) is correct; entropy is critically inadequate. CONCERN because the mechanism exists but the entropy gap is severe. Financial context amplifies the concern but doesn't change the graduation.

**Phrasing-B:** Pipeline trace for OAuth state as 4-character hex (16 bits entropy) in a financial services application. Team: "State is random and validated." Walk through all 9 nodes.

---

## Pair[CB4-3.4-527]

**A:** Full pipeline trace — JWT PII in localStorage unencrypted. Application issues access tokens as JWTs. Payload: `sub`, `email`, `given_name`, `family_name`, `iss`, `aud`, `exp`. Frontend SPA stores the JWT in localStorage. JWT is signed but not encrypted. Team: "The payload is standard — email and name are just user identifiers."

**B:**
```
N1 · Domain Splitter
  Input: JWT with PII claims, stored in localStorage, signed not encrypted
  Domain signals: PII protection, client-side storage security, JWE applicability
  Territories opened: T-asvs-token (primary), T-rfc-jwt
  Split: [jwt-payload-contents | storage-security | jwe-requirement]

N2 · Domain Translator
  jwt-payload-contents → email, given_name, family_name = PII (Personal Identifiable Information)
  storage-security → localStorage: JavaScript-accessible; XSS reads all localStorage
  jwe-requirement → RFC 7516 JWE: appropriate when payload contains PII in JS-accessible storage

N3 · Scheme Planner
  Primary scheme: JWT-PII-protection
  Secondary scheme: V9-token-storage
  Conflict check: PII in payload + localStorage + no encryption = PII exposure to XSS
  Plan: verify payload contents; assess storage attack surface; determine JWE applicability

N4 · Domain Processor
  JWT payload PII assessment:
    email ✗ (PII per GDPR/CCPA), given_name ✗, family_name ✗
    These are not "just identifiers" — they are personal data subject to privacy regulation
  localStorage attack surface:
    Any JavaScript running in the page: SPA code, analytics scripts, CDN libraries
    XSS: injected script executes in same origin → reads all localStorage ✓
    Browser extensions: depending on permissions, can read localStorage ✓
    Shared browser profiles: another user on same device can read localStorage ✓
  JWE applicability:
    PII in payload + JavaScript-accessible storage = JWE appropriate
    Without JWE: PII directly readable from captured JWT token value
  Team argument: "email and name are just user identifiers"
    → They are PII under all major privacy frameworks ✗
    → Identity ≠ non-sensitive ✗
    → REJECTED

N5 · Applier
  Adversarial rejection: "user identifiers = not PII" → REJECTED
  Apply JWT-PII-protection: FAIL — PII in unencrypted JWT in localStorage
  Apply V9-token-storage: FAIL — PII-containing token in XSS-accessible storage
  Verdict: REPROCESS

N6 · Result Translator
  Finding: PII (email, name) in unencrypted JWT stored in JavaScript-accessible localStorage;
    XSS exposes PII directly via token contents
  Severity: REPROCESS

N7 · Arbiter Selector
  Primary: ASVS-V9-Token (token contents and storage)
  Secondary: RFC-JWT (RFC 7516 JWE applicability)

N8 · Arbiter Units
  ASVS-V9-Token:
    PII in JWT payload without JWE ✗; localStorage storage ✗ — REPROCESS
  RFC-JWT:
    RFC 7516: JWE appropriate for PII-containing tokens in JS-accessible storage ✗ — REPROCESS
  T-asvs-token verdict: REPROCESS
  T-rfc-jwt verdict: REPROCESS

N9 · Formatter
  Territory results: T-asvs-token=REPROCESS, T-rfc-jwt=REPROCESS
  Graduation: REPROCESS → Overall REPROCESS
  Remediation: (1) remove PII from JWT payload; (2) fetch claims from UserInfo endpoint; or
    (3) apply JWE; or (4) use httpOnly secure cookie instead of localStorage
Overall verdict: REPROCESS
```

**Reason:** JWT with PII (email, given_name, family_name) stored unencrypted in localStorage exposes PII to XSS and JavaScript readers. "Email and name are just identifiers" is rejected — they are PII under all major privacy frameworks. JWE is appropriate when PII is in the payload and the token is stored in JavaScript-accessible storage. REPROCESS.

**Phrasing-B:** Pipeline trace for JWT with email, given_name, family_name in payload, stored in localStorage, signed but not encrypted. Team: "These are user identifiers, not sensitive." Walk through all 9 nodes.

---

## Pair[CB4-3.4-528]

**A:** Full pipeline trace — JWT standard claims, no PII, httpOnly cookie storage. JWT access tokens: payload contains only `sub`, `iss`, `aud`, `exp`, `iat`, `scope`, `jti`. Stored in httpOnly Secure SameSite=Strict cookie (not accessible to JavaScript). Token signed RS256. No sensitive personal data in payload.

**B:**
```
N1 · Domain Splitter
  Input: JWT with standard claims only, httpOnly secure cookie storage, RS256
  Domain signals: token payload safety, storage security, JWE requirement
  Territories opened: T-asvs-token, T-rfc-jwt
  Split: [payload-pii-check | storage-security | signing-key]

N2 · Domain Translator
  payload-pii-check → sub (opaque), iss, aud, exp, iat, scope, jti — no PII ✓
  storage-security → httpOnly Secure SameSite=Strict cookie: not JS-accessible ✓
  signing-key → RS256 ✓ (token integrity)

N3 · Scheme Planner
  Primary scheme: JWT-payload-standards
  Conflict check: none — no PII; no JS-accessible storage
  Plan: confirm payload is PII-free; confirm cookie security attributes; confirm signing

N4 · Domain Processor
  Payload PII check:
    sub: opaque identifier ✓ (not personally identifiable in isolation)
    iss, aud, exp, iat, scope, jti: technical metadata ✓
    No email, name, phone, address, or other PII ✓
  Storage: httpOnly ✓ (no JS access); Secure ✓ (HTTPS only); SameSite=Strict ✓ (CSRF protection)
  RS256: token integrity ✓; 2048-bit assumed ✓
  JWE: not required (no PII in payload; storage not JS-accessible) ✓

N5 · Applier → all PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector: ASVS-V9-Token, RFC-JWT

N8 · Arbiter Units
  ASVS-V9-Token: no PII in payload ✓; httpOnly Secure cookie ✓ — PASS
  RFC-JWT: signed RS256 ✓; JWE not required ✓ — PASS
  All territories: PASS

N9 · Formatter → Overall PASS
Overall verdict: PASS
```

**Reason:** Standard-claim JWT with no PII, stored in httpOnly Secure SameSite cookie (not JavaScript-accessible). No JWE requirement (no PII, no JS storage). RS256 signing correct. PASS.

**Phrasing-B:** Pipeline trace for JWT with only standard claims (no PII) stored in httpOnly Secure SameSite=Strict cookie, signed RS256. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-529]

**A:** Full pipeline trace — combined hybrid flow REPROCESS plus state entropy CONCERN. Application: response_type=code id_token (hybrid flow). State parameter: 8 random hex characters (~32 bits entropy). Team: "Hybrid flow gives us the ID token directly; state ensures CSRF protection with random hex."

**B:**
```
N1 · Domain Splitter
  Input: hybrid flow (ID token in front-channel), 8-char hex state (~32 bits)
  Domain signals: response_type policy, state entropy, multi-finding scenario
  Territories opened: T-oauth-oidc, T-rfc-oauth2
  Split: [response-type-hybrid | state-entropy-gap | graduation-rule]

N2 · Domain Translator
  response-type-hybrid → REPROCESS (ID token in front channel)
  state-entropy-gap → 32 bits: below 128-bit minimum → CONCERN

N3 · Scheme Planner
  Primary: V10-response-type-policy (REPROCESS)
  Secondary: V10-2-6-state-entropy (CONCERN)
  Conflict check: REPROCESS + CONCERN → graduation: REPROCESS overrides
  Plan: assess each independently; apply graduation

N4 · Domain Processor
  Hybrid flow: ID token in authorization URL fragment ✗ → REPROCESS
  State 8-char hex = 32 bits entropy: below 128-bit minimum ✗ → CONCERN
  Both findings independent: response_type affects token exposure; state affects CSRF binding strength

N5 · Applier
  response-type-hybrid: REPROCESS
  state-entropy: CONCERN
  Graduation: REPROCESS + CONCERN → REPROCESS

N6 · Result Translator
  Combined finding: hybrid flow REPROCESS overrides state entropy CONCERN
  Severity: REPROCESS

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-OAuth2

N8 · Arbiter Units
  T-oauth-oidc: hybrid flow ✗ → REPROCESS; state entropy ✗ → CONCERN
  T-rfc-oauth2: RFC 9700 §2.1.2 ✗ → REPROCESS; RFC 6749 state SHOULD unguessable ✗ → CONCERN
  Graduation: REPROCESS present → REPROCESS

N9 · Formatter
  Territory results: T-oauth-oidc=REPROCESS, T-rfc-oauth2=REPROCESS
  Graduation: REPROCESS → Overall REPROCESS
  Remediation: (1) use response_type=code only; (2) increase state to ≥128-bit CSPRNG
Overall verdict: REPROCESS
```

**Reason:** Hybrid flow (REPROCESS) + low-entropy state (CONCERN). Graduation: REPROCESS overrides CONCERN → Overall REPROCESS. Two independent findings from the same request, different severity levels.

**Phrasing-B:** Pipeline trace for hybrid flow (ID token in redirect) plus 8-char hex state (32 bits). One REPROCESS and one CONCERN — what is the overall verdict?

---

## Pair[CB4-3.4-530]

**A:** Full pipeline trace — DPoP enforced plus aud missing from resource server validation. API enforces DPoP: proof JWT validated (jti replay, htm, htu, iat); access token bound to DPoP key via cnf/jkt. However: resource server does NOT validate the `aud` claim of the access token. The access token is a JWT with `aud` present but not checked. Team: "DPoP binds the token to the key — that already constrains who can use it."

**B:**
```
N1 · Domain Splitter
  Input: DPoP enforced correctly, aud claim present but not validated at resource server
  Domain signals: DPoP sender-constraining, aud resource-server-constraining, independent mechanisms
  Territories opened: T-oauth-oidc (DPoP), T-rfc-jwt (aud validation)
  Split: [dpop-binding | aud-validation-absent | mechanism-independence]

N2 · Domain Translator
  dpop-binding → RFC 9449: DPoP proof correct → sender-constraining ✓
  aud-validation-absent → RFC 7519 §4.1.3: aud MUST be validated when present ✗
  mechanism-independence → DPoP (who) and aud (where) are independent constraints

N3 · Scheme Planner
  Primary: RFC7519-aud-validation (REPROCESS)
  Secondary: RFC9449-DPoP (PASS)
  Conflict check: DPoP correct; aud absent = independent REPROCESS
  Plan: assess each; apply graduation; reject "DPoP compensates for aud"

N4 · Domain Processor
  DPoP validation: jti ✓; htm ✓; htu ✓; iat ✓; cnf/jkt ✓ — DPoP correct
  aud claim present in token; not validated at resource server ✗
  Team argument: "DPoP constrains who can use it"
    DPoP constrains WHO: token usable only by holder of DPoP private key ✓
    aud constrains WHERE: token usable only at the intended resource server
    These are independent: DPoP-constrained token intended for resource-server-A
      can be presented at resource-server-B if aud is not validated
    → "DPoP compensates for aud" REJECTED — different security boundaries
  RFC 7519 §4.1.3: aud present → MUST be validated ✗

N5 · Applier
  Adversarial rejection: "DPoP constrains who — aud unnecessary" → REJECTED
  Apply RFC9449-DPoP: PASS
  Apply RFC7519-aud: REPROCESS (aud present, not validated — MUST violated)
  Verdict: REPROCESS (REPROCESS + PASS → REPROCESS)

N6 · Result Translator
  Finding: DPoP correct; aud validation absent — token cross-resource-server use possible
  Severity: REPROCESS

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-JWT

N8 · Arbiter Units
  T-oauth-oidc: DPoP ✓ → PASS; aud absent ✗ → REPROCESS
  T-rfc-jwt: RFC 7519 §4.1.3 aud present + not validated ✗ → REPROCESS
  T-rfc-oauth2: DPoP (RFC 9449) ✓ → PASS

N9 · Formatter
  Graduation: REPROCESS present → Overall REPROCESS
Overall verdict: REPROCESS
```

**Reason:** DPoP correctly implemented (PASS) — but aud not validated at resource server despite being present in the token (REPROCESS per RFC 7519 §4.1.3). "DPoP constrains who" is rejected — DPoP and aud are independent mechanisms serving different security boundaries. REPROCESS overrides PASS.

**Phrasing-B:** Pipeline trace for DPoP fully enforced but aud claim in access token not validated at resource server. Team: "DPoP already constrains who can use the token." Walk through all 9 nodes.

---

## Pair[CB4-3.4-531]

**A:** Full pipeline trace — correct DPoP plus aud validation. Resource server: DPoP enforced (jti, htm, htu, iat, cnf/jkt). aud claim validated — token accepted only if aud includes this resource server's URL. Access token is a JWT: sub, iss, aud, exp, iat, scope, cnf. Signed RS256 2048-bit. Token signed by authorization server, DPoP key bound via cnf.

**B:**
```
N1 · Domain Splitter
  Input: DPoP full proof validation, aud validated against resource server URL
  Domain signals: sender-constraining (DPoP), audience-constraining (aud), dual-layer protection
  Territories opened: T-oauth-oidc, T-rfc-jwt, T-rfc-oauth2
  Split: [dpop-enforcement | aud-enforcement | token-integrity]

N2 · Domain Translator
  dpop-enforcement → RFC 9449: all proof components validated ✓
  aud-enforcement → RFC 7519 §4.1.3: aud validated ✓
  token-integrity → RS256 2048-bit ✓

N3 · Scheme Planner → all sub-schemes expected PASS

N4 · Domain Processor
  DPoP: jti replay detection ✓; htm ✓; htu ✓; iat ✓; cnf/jkt match ✓ — complete
  aud: token aud contains resource server URL ✓; RS validates aud includes own identifier ✓
    Token issued for resource-server-A not accepted at resource-server-B ✓
  RS256 2048-bit ✓; exp validated ✓; iss validated ✓
  No gaps

N5 · Applier → all PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-JWT, RFC-OAuth2

N8 · Arbiter Units
  T-oauth-oidc: DPoP full ✓ — PASS; aud ✓ — PASS
  T-rfc-jwt: RFC 7519 aud ✓; RS256 2048-bit ✓ — PASS
  T-rfc-oauth2: RFC 9449 DPoP ✓ — PASS
  All territories: PASS

N9 · Formatter → Overall PASS
Overall verdict: PASS
```

**Reason:** DPoP full enforcement (jti, htm, htu, iat, cnf/jkt) plus aud validation at resource server. Both binding mechanisms correct. RS256 2048-bit token integrity. Comprehensive token acceptance logic. PASS.

**Phrasing-B:** Pipeline trace for DPoP full proof validation plus aud validated against resource server URL, RS256 2048-bit. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-532]

**A:** Full pipeline trace — all ext20 patterns correct. Application: response_type=code only (no hybrid); state 256-bit CSPRNG + session-stored + single-use; JWT standard claims only (no PII), httpOnly Secure cookie; DPoP enforced; aud validated at resource server; all prior controls correct.

**B:**
```
N1 · Domain Splitter
  Input: comprehensive ext20 correct implementation
  Territories opened: T-oauth-oidc, T-rfc-jwt, T-rfc-oauth2, T-asvs-token, T-asvs-authentication,
    T-ssdf-process
  Split: [response-type | state | jwt-storage | dpop-aud | prior-controls]

N2 · Domain Translator
  response-type → code-only ✓; state → 256-bit CSPRNG ✓; JWT → no PII + httpOnly ✓;
  dpop-aud → DPoP full + aud ✓; prior controls all correct ✓

N3 · Scheme Planner → all sub-schemes expected PASS; no conflicts

N4 · Domain Processor
  response_type=code only ✓; no front-channel token exposure ✓
  State: 256-bit CSPRNG ✓; session-stored ✓; single-use ✓
  JWT: no PII ✓; httpOnly Secure SameSite ✓; JWE not required ✓
  DPoP: jti ✓; htm ✓; htu ✓; iat ✓; cnf/jkt ✓ — PASS
  aud: validated at resource server ✓
  Prior controls: PKCE S256 ✓; PAR mandatory ✓; OIDC nonce+aud validated ✓;
    protected dynamic registration ✓; introspection TTL+pub/sub ✓;
    minimum-scope M2M ✓; Argon2id ✓; Vault ✓ — all PASS
  No gaps

N5 · Applier → all PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector: all territories

N8 · Arbiter Units: all territories PASS

N9 · Formatter
  All territories: PASS → Overall PASS
Overall verdict: PASS
```

**Reason:** All ext20 patterns correctly implemented: code-only flow, 256-bit state, standard-claim JWT in httpOnly cookie, DPoP with full proof + aud validation, plus all prior controls. Every territory returns PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Pipeline trace for code-only OIDC, 256-bit state, standard-claim JWT in httpOnly cookie, DPoP + aud, and all prior controls. Walk through all 9 nodes for comprehensive PASS.

---

## File Summary

| Pair ID | Description | Stratum | Pipeline Focus | Verdict |
|---------|-------------|---------|----------------|---------|
| CB4-3.4-525 | OIDC hybrid flow pipeline trace — "standard OIDC" rejected | C/D | T-oauth-oidc + T-rfc-oauth2 | REPROCESS |
| CB4-3.4-526 | State 4-char hex (16 bits entropy) pipeline trace | C | T-oauth-oidc + T-rfc-oauth2 | CONCERN |
| CB4-3.4-527 | JWT PII in localStorage pipeline trace — "identifiers not PII" rejected | C/D | T-asvs-token + T-rfc-jwt | REPROCESS |
| CB4-3.4-528 | JWT standard claims + httpOnly cookie + RS256 pipeline trace | C | T-asvs-token + T-rfc-jwt | PASS |
| CB4-3.4-529 | Hybrid flow REPROCESS + state 8-char hex CONCERN → REPROCESS graduation | C | T-oauth-oidc + T-rfc-oauth2 | REPROCESS |
| CB4-3.4-530 | DPoP correct PASS + aud absent REPROCESS — "DPoP compensates" rejected | C/D | T-oauth-oidc + T-rfc-jwt | REPROCESS |
| CB4-3.4-531 | DPoP full enforcement + aud validated at resource server | C | T-oauth-oidc + T-rfc-jwt + T-rfc-oauth2 | PASS |
| CB4-3.4-532 | All ext20 patterns correct — comprehensive pipeline PASS | C | Multiple | PASS |
