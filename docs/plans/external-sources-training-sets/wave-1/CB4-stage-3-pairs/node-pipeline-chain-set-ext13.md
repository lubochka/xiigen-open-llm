# CB4 Stage 3 — Node Pipeline Chain Set Extension 13
# Sequences CB4-3.4-499 through CB4-3.4-506
# Stratum: C — full 9-node pipeline traces for ext19 patterns
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1012 instances

---

## Pair[CB4-3.4-499]

**A:** Full pipeline trace — OIDC nonce not validated. OIDC login flow. Client generates a nonce, includes it in the authorization request. Upon receiving the ID token, the client checks that the `nonce` field is present and non-empty but does NOT compare it to the nonce that was sent. Team: "We check that the token has a nonce — that confirms the server respected our request."

**B:**
```
N1 · Domain Splitter
  Input: OIDC nonce present in token, nonce match validation absent, "presence = confirmation"
  Domain signals: OIDC ID token validation, replay protection, nonce binding
  Territories opened: T-oauth-oidc (primary), T-rfc-jwt (ID token claim validation)
  Split: [nonce-generation | nonce-binding-validation | adversarial-argument]

N2 · Domain Translator
  nonce-generation → cryptographically random nonce per attempt ✓ (generation side)
  nonce-binding-validation → OIDC Core §3.1.3.7 / V10.2: nonce in token MUST match sent nonce
  adversarial-argument → "presence = confirmation" → flag for N5 rejection

N3 · Scheme Planner
  Primary scheme: OIDC-nonce-binding
  Secondary scheme: V10-ID-token-validation
  Conflict check: nonce generation correct; binding validation absent → gap at validation step
  Plan: verify what nonce validation steps are performed; identify missing step; reject argument

N4 · Domain Processor
  Nonce lifecycle:
    Client generates random nonce per attempt ✓
    Nonce included in authorization request ✓
    Server includes nonce in ID token ✓ (server side correct)
    Client checks: nonce field present and non-empty ✓
    Client checks: nonce value matches stored nonce for this session ✗ — ABSENT
  Security impact of absent match check:
    Attacker captures a previously issued ID token (different nonce, same user)
    Presents it to the application within its validity window
    Client accepts: nonce is present ✓, non-empty ✓ — replay succeeds
    Nonce binding purpose is to prevent exactly this — without value comparison, nonce provides
      no actual replay protection
  Team argument: "presence confirms server respected our request"
    → Server including any nonce proves server processed a request; does NOT prove it processed
      THIS request with THIS nonce
    → REJECTED: presence check ≠ binding check

N5 · Applier
  Adversarial rejection: "nonce present = confirmation" → REJECTED
    Nonce presence proves server included a nonce; value match proves binding to this request
  Apply OIDC-nonce-binding: FAIL — value match validation absent
  Apply V10-ID-token-validation: FAIL — required validation step missing
  Verdict: REPROCESS

N6 · Result Translator
  Finding: nonce validation incomplete — presence checked but value not compared to sent nonce;
    ID token replay attack possible with a previous valid token
  Adversarial claim rejected: presence is insufficient; match is the binding step
  Severity: REPROCESS — required validation step absent

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (OIDC ID token validation)
  Secondary: RFC-JWT (RFC 7519 claim validation)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    OIDC nonce: generated ✓; included in request ✓; server includes in token ✓;
    Client validates value match ✗ — REPROCESS (binding step absent)
  RFC-JWT:
    RFC 7519 / OIDC Core: nonce value MUST be validated against the value sent ✗ — REPROCESS
  T-oauth-oidc verdict: REPROCESS
  T-rfc-jwt verdict: REPROCESS

N9 · Formatter
  Territory results: T-oauth-oidc=REPROCESS, T-rfc-jwt=REPROCESS
  Graduation: REPROCESS present → Overall REPROCESS
  Remediation: store nonce in session before redirect; compare token nonce to stored value;
    enforce single-use (invalidate after successful validation)
Overall verdict: REPROCESS
```

**Reason:** Nonce presence check does not constitute nonce validation. The binding security property requires comparing the value in the token to the value sent in the authorization request. Without the match check, captured ID tokens with valid-but-wrong nonces are accepted. "Presence = confirmation" is rejected. REPROCESS because required validation step is absent.

**Phrasing-B:** Pipeline trace for OIDC login where client verifies nonce is present and non-empty but never checks it matches what was sent. Team: "Nonce present confirms server processed our request." Walk through all 9 nodes.

---

## Pair[CB4-3.4-500]

**A:** Full pipeline trace — OIDC nonce correctly validated. OIDC login. Before each authorization redirect, client generates a cryptographically random nonce and stores it in the server-side session. On ID token receipt: (1) nonce claim present; (2) nonce value equals session-stored nonce; (3) nonce not previously used (replay detection); (4) session nonce cleared after validation. ID token also validated for `iss`, `aud`, `exp`, `nbf`.

**B:**
```
N1 · Domain Splitter
  Input: nonce per-attempt with session storage, value match + single-use + clearance, full claim validation
  Domain signals: OIDC nonce binding, ID token replay protection, token claim validation
  Territories opened: T-oauth-oidc, T-rfc-jwt
  Split: [nonce-generation-storage | nonce-binding-validation | full-claim-validation]

N2 · Domain Translator
  nonce-generation-storage → random per attempt; server-side session (not cookie exposed) ✓
  nonce-binding-validation → value match + single-use + clearance → complete binding lifecycle
  full-claim-validation → iss, aud, exp, nbf all validated → comprehensive token acceptance logic

N3 · Scheme Planner
  Primary scheme: OIDC-nonce-binding
  Secondary scheme: V10-ID-token-validation
  Conflict check: none — all steps present
  Plan: verify nonce lifecycle completeness; verify claim validation; aggregate

N4 · Domain Processor
  Nonce lifecycle:
    Random nonce generated per attempt ✓ (per-attempt freshness)
    Stored in server-side session before redirect ✓ (safe storage)
    Token received: nonce present ✓; value matches stored nonce ✓ (binding confirmed)
    Replay detection: nonce not previously seen ✓ (single-use enforced)
    Session nonce cleared after validation ✓ (prevents reuse)
  Token claim validation:
    iss validated ✓; aud validated ✓; exp validated ✓; nbf validated ✓
  No gaps found

N5 · Applier
  Apply OIDC-nonce-binding: PASS — complete lifecycle implemented
  Apply V10-ID-token-validation: PASS — all required claims validated
  Verdict: PASS

N6 · Result Translator
  Finding: OIDC nonce correctly implemented; complete ID token validation
  Severity: PASS

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth
  Secondary: RFC-JWT

N8 · Arbiter Units
  ASVS-V10-OAuth: OIDC nonce fully validated ✓ — PASS; ID token claims validated ✓ — PASS
  RFC-JWT: OIDC Core nonce binding complete ✓ — PASS
  T-oauth-oidc verdict: PASS
  T-rfc-jwt verdict: PASS

N9 · Formatter
  Territory results: T-oauth-oidc=PASS, T-rfc-jwt=PASS
  Graduation: all PASS → Overall PASS
Overall verdict: PASS
```

**Reason:** Complete nonce lifecycle: random per-attempt generation, server-side session storage, value match on receipt, single-use enforcement, clearance after validation. Plus full claim validation (iss, aud, exp, nbf). All OIDC nonce binding requirements satisfied. PASS.

**Phrasing-B:** Pipeline trace for OIDC nonce with per-attempt random generation, session storage, value match, single-use enforcement, clearance, plus full claim validation. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-501]

**A:** Full pipeline trace — PAR optional for public clients. PAR (Pushed Authorization Requests — RFC 9126) endpoint implemented. Public clients (browser-based SPAs) use traditional query-string authorization requests. PAR is available but only used by confidential backend clients. Team: "PAR is supported — clients that want extra security can use it."

**B:**
```
N1 · Domain Splitter
  Input: PAR available, public SPA clients use traditional query-string, PAR optional
  Domain signals: authorization request parameter exposure, PAR security benefit for public clients
  Territories opened: T-oauth-oidc (primary), T-rfc-oauth2 (RFC 9126)
  Split: [par-availability | public-client-parameter-exposure | par-requirement-level]

N2 · Domain Translator
  par-availability → RFC 9126 PAR endpoint exists ✓
  public-client-parameter-exposure → PKCE challenge, state, scope, redirect_uri in URL
    → browser history, referrer headers, server logs
  par-requirement-level → RFC 9126 / ASVS V10: PAR is recommended for high-security contexts;
    not a hard MUST for all deployments

N3 · Scheme Planner
  Primary scheme: RFC9126-PAR-security-benefit
  Secondary scheme: V10-public-client-security
  Conflict check: PAR is available but not applied to the clients that benefit most
  Plan: assess security impact of traditional requests for SPAs; determine CONCERN vs PASS

N4 · Domain Processor
  Public SPA clients using traditional requests:
    Authorization URL contains: state, PKCE challenge, scope, redirect_uri as query params
    URL appears in: browser history ✗; Referer headers ✗; server access logs ✗
    PAR would send these parameters via back-channel POST to authorization server first
    Authorization URL then contains only an opaque request_uri reference
  PAR requirement level:
    RFC 9126: PAR is specified; not normatively required by ASVS for all client types
    CONCERN: security improvement available; not applied to clients that benefit most (SPAs)
    Not REPROCESS — PAR absence is not a hard MUST violation

N5 · Applier
  Apply RFC9126-PAR-security-benefit: CONCERN — PAR available but not used by public clients
  Apply V10-public-client-security: CONCERN — authorization parameters exposed in URL for SPAs
  Team argument "clients that want extra security can use it": CONCERN (improvement available;
    the clients most exposed — SPAs — are not using it)
  Verdict: CONCERN

N6 · Result Translator
  Finding: PAR available but not required/used for public clients; authorization parameters
    exposed in URL for SPA clients
  Severity: CONCERN — PAR not mandated; security improvement available but not applied

N7 · Arbiter Selector
  Primary: ASVS-V10-OAuth (PAR, public client security)
  Secondary: RFC-OAuth2 (RFC 9126)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    PAR available ✓; not required for public clients ✗ (CONCERN — improvement available)
    Public clients: URL parameter exposure — CONCERN
  RFC-OAuth2:
    RFC 9126: PAR optional in spec; CONCERN for not applying to highest-benefit clients
  T-oauth-oidc verdict: CONCERN
  T-rfc-oauth2 verdict: CONCERN

N9 · Formatter
  Territory results: T-oauth-oidc=CONCERN, T-rfc-oauth2=CONCERN
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: require PAR for all clients; authorization endpoint should only accept request_uri
Overall verdict: CONCERN
```

**Reason:** PAR available but not used by public SPA clients — the clients that benefit most from PAR's back-channel parameter delivery. Authorization parameters (PKCE challenge, state, scope) remain in the URL for SPAs, exposed to browser history and logs. CONCERN (not REPROCESS) because PAR is recommended, not mandated.

**Phrasing-B:** Pipeline trace for PAR available but not required — SPAs use traditional query-string authorization. Team: "Clients that want extra security can use PAR." Walk through all 9 nodes.

---

## Pair[CB4-3.4-502]

**A:** Full pipeline trace — PAR required for all clients. PAR endpoint available. Server configuration: authorization endpoint rejects requests with inline parameters (`client_id`, `redirect_uri`, `scope`, `state`, `code_challenge` directly in URL) — only `request_uri` references accepted. All clients required to call PAR endpoint first. PKCE S256 required. Code-only flow.

**B:**
```
N1 · Domain Splitter
  Input: PAR mandatory, authorization endpoint accepts only request_uri, PKCE S256, code-only
  Domain signals: authorization request security, parameter exposure elimination
  Territories opened: T-oauth-oidc, T-rfc-oauth2
  Split: [par-enforcement | request-uri-only | pkce-and-flow]

N2 · Domain Translator
  par-enforcement → RFC 9126: PAR required; server enforces request_uri-only ✓
  request-uri-only → authorization parameters never in URL → no browser/log exposure
  pkce-and-flow → V10.4.6 S256; response_type=code ✓

N3 · Scheme Planner
  Primary scheme: RFC9126-PAR-mandatory
  Secondary scheme: V10-4-6-PKCE
  Conflict check: none
  Plan: verify PAR enforcement; confirm PKCE S256; confirm code-only; aggregate

N4 · Domain Processor
  PAR enforcement: inline parameters rejected at authorization endpoint ✓
    request_uri required ✓; expires quickly (RFC 9126 §5) ✓; single-use ✓
  Authorization parameters delivered via back-channel POST ✓
    Not in URL → not in browser history, Referer, or server access logs ✓
  PKCE S256 ✓; plain rejected ✓; response_type=code ✓
  No gaps

N5 · Applier
  Apply RFC9126-PAR-mandatory: PASS
  Apply V10-4-6-PKCE: PASS
  Verdict: PASS

N6 · Result Translator → PASS

N7 · Arbiter Selector: ASVS-V10-OAuth, RFC-OAuth2

N8 · Arbiter Units
  ASVS-V10-OAuth: PAR mandatory ✓; PKCE S256 ✓; code-only ✓ — PASS
  RFC-OAuth2: RFC 9126 PAR with request_uri-only ✓ — PASS
  T-oauth-oidc verdict: PASS
  T-rfc-oauth2 verdict: PASS

N9 · Formatter
  All territories: PASS → Overall PASS
Overall verdict: PASS
```

**Reason:** PAR required for all clients with server enforcement (inline parameters rejected). Authorization parameters never exposed in URL. PKCE S256 and code-only flow correct. Strongest PAR configuration. PASS.

**Phrasing-B:** Pipeline trace for PAR mandatory with authorization endpoint accepting only request_uri. PKCE S256, code-only. Walk through all 9 nodes to confirm PASS.

---

## Pair[CB4-3.4-503]

**A:** Full pipeline trace — token endpoint no rate limiting. Token endpoint (`POST /oauth/token`) accepts unlimited requests. No per-client, per-IP, or per-authorization-code rate limiting. PKCE S256 enforced. Client credentials required for confidential clients. Team: "Cryptographically random codes and PKCE make brute force infeasible — rate limiting is overhead."

**B:**
```
N1 · Domain Splitter
  Input: no rate limiting on token endpoint, PKCE correct, client credentials required
  Domain signals: brute force defense, resource protection, token endpoint security
  Territories opened: T-oauth-oidc (primary), T-asvs-authentication (brute force)
  Split: [token-endpoint-authentication | rate-limiting-purpose | adversarial-argument]

N2 · Domain Translator
  token-endpoint-authentication → client credentials + PKCE ✓ (present controls)
  rate-limiting-purpose → V8.1 / brute force: defense-in-depth for unlimited attempts
  adversarial-argument → "PKCE + random codes = rate limiting unnecessary"

N3 · Scheme Planner
  Primary scheme: V8-rate-limiting-defense-in-depth
  Secondary scheme: V10-token-endpoint-security
  Conflict check: authentication controls present; rate limiting absent = defense-in-depth gap
  Plan: enumerate rate-limiting use cases not covered by PKCE; reject argument; classify level

N4 · Domain Processor
  Authentication controls at token endpoint: PKCE ✓; client credentials ✓ (confidential)
  Rate limiting absent — use cases not covered by authentication controls:
    (1) Public clients (SPAs): no client_secret; PKCE is auth → unlimited PKCE exchanges possible
    (2) Stolen refresh token: attacker exchanges rapidly before legitimate client notices
    (3) Credential stuffing on confidential client secret: unlimited guessing without lockout
    (4) Resource exhaustion: database + token issuance load unbounded per caller
  PKCE prevents third-party interception of valid codes — does NOT prevent:
    (a) Rapid exchange of stolen tokens/codes
    (b) Brute force on client credentials
    (c) Resource exhaustion
  Requirement level: rate limiting is defense-in-depth — no hard MUST; CONCERN (not REPROCESS)
  Team argument: "PKCE makes brute force infeasible" → REJECTED (different threat model)

N5 · Applier
  Adversarial rejection: "PKCE = rate limiting substitute" → REJECTED
  Apply V8-rate-limiting: CONCERN — rate limiting absent; defense-in-depth gap
  Authentication controls (PKCE, client credentials): PASS (mechanism correct)
  Verdict: CONCERN

N6 · Result Translator
  Finding: token endpoint no rate limiting — defense-in-depth gap; authentication controls present
  Severity: CONCERN

N7 · Arbiter Selector
  Primary: ASVS-V8-AuthZ (rate limiting / brute force protection)
  Secondary: ASVS-V10-OAuth (token endpoint security)

N8 · Arbiter Units
  ASVS-V8-AuthZ:
    Rate limiting absent ✗ — CONCERN (not hard MUST; defense-in-depth gap)
  ASVS-V10-OAuth:
    Token endpoint auth controls correct ✓ — PASS
    Rate limiting absent → CONCERN (endpoint exposure for public clients)
  T-asvs-authentication verdict: CONCERN
  T-oauth-oidc verdict: CONCERN

N9 · Formatter
  Territory results: T-asvs-authentication=CONCERN, T-oauth-oidc=CONCERN
  Graduation: 0 REPROCESS, 1+ CONCERN → Overall CONCERN
  Remediation: add per-client-id rate limiting; exponential back-off on repeated failures
Overall verdict: CONCERN
```

**Reason:** Token endpoint no rate limiting is CONCERN — authentication controls (PKCE, client credentials) are present but do not cover rapid exchange of stolen tokens, credential stuffing against client secrets, or resource exhaustion. "PKCE makes brute force infeasible" is rejected — PKCE protects against code interception, not against post-possession rapid exchange. CONCERN (not REPROCESS) because rate limiting is defense-in-depth, not a hard MUST.

**Phrasing-B:** Pipeline trace for token endpoint with no rate limiting. PKCE S256 correct, client credentials required. Team: "Cryptographic controls make brute force infeasible." Walk through all 9 nodes.

---

## Pair[CB4-3.4-504]

**A:** Full pipeline trace — OIDC aud claim not validated. Application accepts OIDC ID tokens. Validation performed: JWKS signature check; `iss` verified against expected issuer URL; `exp` and `nbf` checked. `aud` claim present in token but not verified against the application's `client_id`. Team: "We verify the signature and the issuer — if both are valid, the token is authentic."

**B:**
```
N1 · Domain Splitter
  Input: aud claim present but not validated, signature + issuer validated, "authentic = valid"
  Domain signals: OIDC aud validation, token confusion attack, ID token acceptance logic
  Territories opened: T-oauth-oidc (primary), T-rfc-jwt (RFC 7519 §4.1.3)
  Split: [signature-issuer-validation | aud-claim-validation | adversarial-argument]

N2 · Domain Translator
  signature-issuer-validation → RFC 7519: signature ✓; iss ✓ — partial validation done
  aud-claim-validation → RFC 7519 §4.1.3: aud MUST be validated when present
  adversarial-argument → "signature+issuer = authentic" → flag for N5 rejection

N3 · Scheme Planner
  Primary scheme: RFC7519-aud-validation
  Secondary scheme: OIDC-token-confusion-prevention
  Conflict check: two of three required validations present; aud absent = security gap
  Plan: explain aud purpose; construct token confusion scenario; reject argument; verdict

N4 · Domain Processor
  Aud claim purpose:
    Identifies intended recipient(s) of the token
    aud = client_id of the application the token was issued FOR
  Token confusion attack scenario:
    Attacker has a valid ID token issued by the same IdP FOR attacker's own application
      (attacker registered their own OAuth client with the same IdP)
    Token: correct signature ✓; correct iss ✓; correct exp ✓; aud=attacker-client-id
    Attacker presents this token to victim application
    Victim application: signature ✓; issuer ✓; exp ✓ — aud not checked ✗
    Victim application accepts token → attacker authenticated as the token's subject
  RFC 7519 §4.1.3: "if aud claim is present, it MUST be validated"
  Team argument: "signature + issuer = token is authentic"
    → Authentic means the IdP issued it; aud determines it was issued FOR THIS APP
    → REJECTED: authenticity ≠ intended-recipient validation

N5 · Applier
  Adversarial rejection: "signature+issuer sufficient" → REJECTED
    RFC 7519 §4.1.3: aud MUST be validated when present — independent required step
  Apply RFC7519-aud-validation: FAIL — aud not validated
  Apply OIDC-token-confusion: FAIL — token confusion attack possible
  Verdict: REPROCESS

N6 · Result Translator
  Finding: aud claim validation absent; token confusion attack possible with token issued for
    a different client by the same IdP
  Severity: REPROCESS

N7 · Arbiter Selector
  Primary: ASVS-V9-Token / ASVS-V10-OAuth (ID token validation)
  Secondary: RFC-JWT (RFC 7519 §4.1.3)

N8 · Arbiter Units
  ASVS-V10-OAuth:
    OIDC ID token aud not validated ✗ — REPROCESS (token confusion possible)
  RFC-JWT:
    RFC 7519 §4.1.3: aud present but not validated ✗ — REPROCESS
  T-oauth-oidc verdict: REPROCESS
  T-rfc-jwt verdict: REPROCESS

N9 · Formatter
  Territory results: T-oauth-oidc=REPROCESS, T-rfc-jwt=REPROCESS
  Graduation: REPROCESS present → Overall REPROCESS
  Remediation: validate aud claim against application's client_id; reject tokens with
    aud that does not include this application's identifier
Overall verdict: REPROCESS
```

**Reason:** `aud` claim validation is a required step per RFC 7519 §4.1.3 when the claim is present. Without it, a valid ID token issued by the same IdP for a different application passes all other validations and is accepted — token confusion attack. "Signature + issuer = authentic" is rejected: authenticity proves IdP issued it; `aud` proves it was issued for this application. REPROCESS.

**Phrasing-B:** Pipeline trace for OIDC ID token validation where signature, issuer, exp, and nbf are checked but aud is not. Team: "Signature and issuer are sufficient for authenticity." Walk through all 9 nodes.

---

## Pair[CB4-3.4-505]

**A:** Full pipeline trace — combined: OIDC aud not validated plus PAR optional. Application: OIDC ID token accepted without aud validation. PAR available but not required for public clients. All other controls correct (PKCE S256, code-only, Argon2id, Vault).

**B:**
```
N1 · Domain Splitter
  Input: aud validation absent, PAR optional for public clients, other controls correct
  Domain signals: token confusion, authorization parameter exposure, overall security posture
  Territories opened: T-oauth-oidc (primary), T-rfc-jwt, T-asvs-authentication, T-ssdf-process
  Split: [aud-validation-gap | par-usage-gap | correct-controls]

N2 · Domain Translator
  aud-validation-gap → RFC 7519 §4.1.3 → REPROCESS candidate
  par-usage-gap → PAR optional for public clients → CONCERN candidate
  correct-controls → PKCE ✓; code-only ✓; Argon2id ✓; Vault ✓ → PASS

N3 · Scheme Planner
  Primary: RFC7519-aud-validation (REPROCESS)
  Secondary: RFC9126-PAR-optional (CONCERN)
  Conflict check: REPROCESS + CONCERN → graduation rule applies
  Plan: assess independently; apply graduation (REPROCESS overrides CONCERN and PASS)

N4 · Domain Processor
  aud not validated: token confusion attack possible ✗ → REPROCESS
  PAR optional for SPAs: authorization parameters in URL ✗ → CONCERN (not MUST)
  PKCE S256 ✓; code-only ✓; Argon2id ✓; Vault ✓ → PASS

N5 · Applier
  RFC7519-aud: REPROCESS
  RFC9126-PAR: CONCERN
  Correct controls: PASS
  Graduation: REPROCESS + CONCERN + PASS → REPROCESS

N6 · Result Translator
  Finding: aud validation absent (REPROCESS) overrides PAR CONCERN
  Severity: REPROCESS

N7 · Arbiter Selector: T-oauth-oidc + T-rfc-jwt + T-asvs-authentication + T-ssdf-process

N8 · Arbiter Units
  T-rfc-jwt: aud absent ✗ → REPROCESS
  T-oauth-oidc: aud absent ✗; PAR optional → REPROCESS (aud) + CONCERN (PAR)
  T-asvs-authentication: Argon2id ✓ → PASS
  T-ssdf-process: Vault ✓ → PASS

N9 · Formatter
  Territory results: T-rfc-jwt=REPROCESS, T-oauth-oidc=REPROCESS, T-asvs-authentication=PASS,
    T-ssdf-process=PASS
  Graduation: REPROCESS present → Overall REPROCESS
Overall verdict: REPROCESS
```

**Reason:** aud validation absent (REPROCESS) + PAR optional (CONCERN) + correct controls (PASS). Graduation rule: REPROCESS present → Overall REPROCESS. PAR CONCERN and authentication/Vault PASS do not compensate for or reduce the REPROCESS finding on aud validation.

**Phrasing-B:** Pipeline trace for: OIDC aud not validated (REPROCESS), PAR optional for SPAs (CONCERN), other controls correct (PASS). What is the overall verdict?

---

## Pair[CB4-3.4-506]

**A:** Full pipeline trace — all ext19 patterns correct plus token endpoint rate limiting. OIDC: nonce validated (value match + single-use); aud validated against client_id; at_hash validated; sig, iss, exp, nbf all validated. PAR required for all clients. Token endpoint: 10 requests per client_id per 60 seconds; exponential back-off on repeat failures. All prior controls correct.

**B:**
```
N1 · Domain Splitter
  Input: complete OIDC validation, PAR mandatory, token endpoint rate limiting, all prior controls
  Domain signals: ID token security, authorization request security, endpoint protection
  Territories opened: T-oauth-oidc, T-rfc-jwt, T-rfc-oauth2, T-asvs-authentication, T-ssdf-process
  Split: [oidc-token-validation | par-enforcement | token-endpoint-rate-limiting | prior-controls]

N2 · Domain Translator
  oidc-token-validation → OIDC Core + RFC 7519: nonce, aud, at_hash, sig, iss, exp, nbf
  par-enforcement → RFC 9126: mandatory; request_uri-only
  token-endpoint-rate-limiting → per-client-id rate limit + exponential back-off
  prior-controls → PKCE S256, code-only, Argon2id, Vault, dynamic registration, cache, scope

N3 · Scheme Planner
  Multi-territory; all sub-schemes expected to PASS
  Conflict check: none
  Plan: verify each control; aggregate

N4 · Domain Processor
  OIDC ID token validation:
    nonce: value match + single-use ✓; aud: validated against client_id ✓;
    at_hash ✓; sig ✓; iss ✓; exp ✓; nbf ✓ — complete
  PAR: mandatory; inline parameters rejected at authorization endpoint ✓
  Token endpoint rate limiting: 10 req/client_id/60s ✓; exponential back-off ✓
  Prior controls: all correct (PKCE S256, code-only, Argon2id, Vault, etc.) ✓
  No gaps

N5 · Applier → all PASS

N6 · Result Translator → PASS (all controls correct)

N7 · Arbiter Selector: all territories

N8 · Arbiter Units
  T-oauth-oidc: OIDC validation complete ✓; PAR mandatory ✓ — PASS
  T-rfc-jwt: nonce, aud, at_hash, sig, iss, exp, nbf ✓ — PASS
  T-rfc-oauth2: rate limiting ✓; prior OAuth controls ✓ — PASS
  T-asvs-authentication: Argon2id ✓ — PASS
  T-ssdf-process: Vault ✓ — PASS
  All territories: PASS

N9 · Formatter
  All territories PASS → Overall PASS
Overall verdict: PASS
```

**Reason:** All ext19 patterns correctly implemented: complete OIDC ID token validation (nonce, aud, at_hash, and all standard claims), PAR required for all clients, token endpoint rate-limited with back-off, plus all prior controls correct. Every territory returns PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Pipeline trace for complete OIDC validation (nonce, aud, at_hash), mandatory PAR, token endpoint rate limiting, and all prior controls correct. Walk through all 9 nodes to confirm comprehensive PASS.

---

## File Summary

| Pair ID | Description | Stratum | Pipeline Focus | Verdict |
|---------|-------------|---------|----------------|---------|
| CB4-3.4-499 | OIDC nonce presence-only check — "nonce present = confirmation" rejected | C/D | T-oauth-oidc + T-rfc-jwt | REPROCESS |
| CB4-3.4-500 | OIDC nonce fully validated: value match + single-use + clearance | C | T-oauth-oidc + T-rfc-jwt | PASS |
| CB4-3.4-501 | PAR available but not required for SPA clients | C | T-oauth-oidc + T-rfc-oauth2 | CONCERN |
| CB4-3.4-502 | PAR required for all clients — request_uri-only authorization endpoint | C | T-oauth-oidc + T-rfc-oauth2 | PASS |
| CB4-3.4-503 | Token endpoint no rate limiting — "PKCE substitutes" rejected | C/D | T-asvs-authentication + T-oauth-oidc | CONCERN |
| CB4-3.4-504 | OIDC aud not validated — "signature+issuer sufficient" rejected | C/D | T-oauth-oidc + T-rfc-jwt | REPROCESS |
| CB4-3.4-505 | aud REPROCESS + PAR CONCERN + correct controls → REPROCESS graduation | C | Multiple | REPROCESS |
| CB4-3.4-506 | All ext19 + prior controls correct — comprehensive pipeline PASS | C | Multiple | PASS |
