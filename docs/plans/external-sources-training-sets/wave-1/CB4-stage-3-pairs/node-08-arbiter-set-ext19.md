# CB4 Stage 3 — Node 08 Arbiter Set Extension 19
# Sequences CB4-3.4-491 through CB4-3.4-498
# Stratum: A/B/D — baselines, team context, adversarial rejection
# New patterns: OIDC ID token nonce, PAR (RFC 9126), token endpoint rate limiting, aud validation
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 996 instances

---

## Pair[CB4-3.4-491]

**A:** OIDC ID token nonce review. Application uses OIDC (OpenID Connect) for user login. The client generates a `nonce` value, includes it in the authorization request, and the authorization server includes the nonce in the issued ID token. The client: receives the ID token but does NOT validate that the `nonce` in the token matches the nonce sent in the authorization request. Team: "The nonce is present in the ID token — that proves the server processed our request."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

V10.2 (OIDC ID token validation): REPROCESS
Team argument: "nonce is present in the token — server processed our request"

OIDC nonce validation requirements:
  (1) Client sends nonce in authorization request ✓
  (2) Server includes nonce in ID token ✓
  (3) Client MUST validate that the nonce in the token matches the nonce sent ✗
      Step (3) is absent — client does not validate nonce

Nonce purpose:
  Nonce is a binding value: it binds the authorization request to the ID token
  Without client-side nonce validation:
    An attacker who captures a previously-issued ID token (with a different nonce) can
    replay it — the client accepts any ID token that contains a nonce, regardless of whether
    it matches the current request's nonce
  → Replay attack on ID tokens enabled by absent nonce validation

Team argument analysis:
  "Nonce is present" proves the server included a nonce value — it does NOT prove the nonce
  corresponds to this specific authorization request
  Nonce validation is the binding step; presence alone is insufficient
  → "Nonce present = request processed" REJECTED

Finding: nonce binding validation absent — ID token replay possible
Overall verdict: REPROCESS
```

**Reason:** OIDC nonce validation has two halves: including the nonce in the authorization request (done) and validating that the returned nonce matches (absent). The client validates only that a nonce is present, not that it matches. "Nonce present = server processed our request" is rejected — the match validation is the actual security check. REPROCESS because required ID token validation step is absent.

**Phrasing-B:** OIDC login: client sends nonce in auth request, server includes nonce in ID token, but client never verifies the nonce in the token matches what was sent. Team: "Nonce is in the token — server processed our request." Is nonce presence sufficient?

---

## Pair[CB4-3.4-492]

**A:** OIDC nonce validation correctly implemented. Application: OIDC login flow. Client generates a cryptographically random nonce per login attempt, includes it in the authorization request, stores it in the session. Upon receiving the ID token, the client validates: (1) nonce in token equals stored nonce; (2) nonce not previously seen (single-use). After successful validation, the stored nonce is invalidated.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

V10.2 (OIDC ID token nonce validation): PASS
  (1) Cryptographically random nonce generated per login attempt ✓
  (2) Nonce included in authorization request ✓
  (3) Nonce stored in session before authorization redirect ✓
  (4) On ID token receipt: nonce in token validated against stored nonce ✓
  (5) Nonce single-use: not previously seen ✓
  (6) Stored nonce invalidated after successful validation ✓

Nonce security guarantees:
  Replay protection: captured ID token with old nonce rejected (step 5) ✓
  Session binding: token bound to the session that initiated the login ✓
  Per-attempt freshness: new nonce for each login attempt ✓

Overall verdict: PASS
```

**Reason:** Complete nonce implementation: random generation, request inclusion, session storage, value match validation, single-use enforcement, and invalidation after use. All steps of nonce binding correctly implemented. PASS.

**Phrasing-B:** OIDC nonce: cryptographically random per attempt, stored in session, validated for value match and single-use on token receipt, invalidated after use. Does this fully satisfy nonce binding requirements?

---

## Pair[CB4-3.4-493]

**A:** PAR (Pushed Authorization Requests) review. Authorization server supports RFC 9126 (Pushed Authorization Requests — PAR (Pushed Authorization Requests)). PAR endpoint: `POST /oauth/par`. PAR is available but not required — clients may use PAR or send a traditional query-string authorization request. Public clients (single-page applications) all use traditional query-string requests. Team: "PAR is available for clients that want it — making it mandatory would break our existing clients."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

RFC 9126 (PAR) / V10.x: CONCERN
PAR endpoint implemented ✓ (server-side capability present)
PAR not required for public clients ✗

PAR security benefit for public clients:
  Traditional query-string: authorization parameters (including redirect_uri, scope, state, PKCE
    challenge) exposed in browser URL history, referrer headers, and server logs
  PAR: authorization parameters sent directly to authorization server via back-channel POST;
    authorization endpoint receives only a request_uri reference (opaque, short-lived)
  For SPAs (public clients): PAR eliminates URL-parameter exposure of PKCE challenge,
    state, and scope — improving confidentiality of authorization request parameters

ASVS V10 / RFC 9126: PAR is RECOMMENDED (not MUST) for high-security contexts
  Not a REPROCESS — PAR is not mandated by ASVS as a hard requirement for all clients
  CONCERN: public clients using traditional requests when PAR is available and would improve
    security; "breaks existing clients" argument is weak — PAR requires no client behavior
    change beyond using a different endpoint before the redirect

Finding level: CONCERN
  PAR available but not used by the clients that would benefit most (public/SPA clients)
  Security improvement available but not applied
Overall verdict: CONCERN
```

**Reason:** PAR endpoint is available but not required for public clients (SPAs). PAR improves security for public clients by removing authorization parameters from the URL. CONCERN (not REPROCESS) because PAR is recommended, not a hard MUST for all client types. "Breaking existing clients" is a weak argument since PAR is additive. Overall CONCERN — improvement available but not applied to clients that benefit most.

**Phrasing-B:** PAR endpoint available. Public clients (SPAs) still use traditional query-string authorization requests. Team: "Making PAR mandatory would break existing clients." Is optional PAR availability sufficient for public clients?

---

## Pair[CB4-3.4-494]

**A:** PAR required for all clients. Authorization server: PAR (RFC 9126) endpoint implemented. All clients are required to use PAR — the traditional authorization endpoint accepts only `request_uri` references (from PAR), not inline authorization parameters. PKCE S256 required. Code-only response type. All other controls correct.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

RFC 9126 (PAR) + V10.x: PASS
  PAR endpoint implemented ✓
  PAR required for all clients ✓ — strongest configuration
  Authorization endpoint accepts only request_uri references ✓
    Authorization parameters never exposed in URL ✓
    PKCE challenge, state, scope, redirect_uri all sent via back-channel POST ✓
  PKCE S256 required ✓ (V10.4.6) — no plain allowed ✓
  response_type=code only ✓

PAR security guarantees:
  Authorization request parameters not in browser history, referrer headers, or server logs ✓
  request_uri is short-lived and single-use ✓ (RFC 9126 §5: request_uri expires quickly)
  Back-channel delivery of authorization parameters ✓

Overall verdict: PASS
```

**Reason:** PAR required for all clients — strongest configuration. Authorization parameters delivered via back-channel POST, never exposed in URL. Combined with PKCE S256 and code-only flow. PASS.

**Phrasing-B:** PAR required for all clients — authorization endpoint only accepts request_uri references. PKCE S256, code-only. Is mandatory PAR the strongest configuration and does it satisfy all requirements?

---

## Pair[CB4-3.4-495]

**A:** Token endpoint rate limiting review. Token endpoint (`POST /oauth/token`): no rate limiting implemented. Authorization server accepts unlimited token requests per client_id or IP address. Team: "The token endpoint requires client credentials — unauthenticated callers cannot make valid requests, so rate limiting is unnecessary."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V8-AuthZ

V10 / V8.1 (rate limiting / brute force protection): CONCERN
Team argument: "client credentials required → rate limiting unnecessary"

Analysis of rate-limiting purpose on token endpoint:
  (1) Credential stuffing: public clients do not use client_secret — PKCE is their auth
      Endpoint can receive unlimited PKCE token exchange requests from any origin ✓ (risk)
  (2) Authorization code brute force: while authorization codes are cryptographically random,
      unlimited exchange attempts amplify any weaknesses in code entropy
  (3) Refresh token rotation: without rate limiting, stolen refresh tokens can be exchanged
      rapidly before the legitimate client notices and revokes
  (4) Client credential enumeration: confidential clients with valid client_id but wrong secret —
      unlimited guessing attempts enabled
  (5) Resource exhaustion: unmitigated load on introspection and token issuance paths

Team argument analysis:
  "Client credentials required" covers confidential clients but NOT public clients using PKCE
  Rate limiting is defense in depth independent of authentication requirements
  → "Auth requirement substitutes for rate limiting" REJECTED

Finding: no rate limiting on token endpoint
Level: CONCERN — no rate limiting is a defense-in-depth gap; no MUST violation for absence alone
Overall verdict: CONCERN
```

**Reason:** Token endpoint with no rate limiting is a defense-in-depth CONCERN — particularly for public clients using PKCE (no client secret). Team argument that auth requirement substitutes for rate limiting is rejected: rate limiting defends against brute force, credential stuffing, and resource exhaustion independently of authentication. CONCERN (not REPROCESS) because rate limiting absence is not a hard MUST requirement but is a recognized security gap.

**Phrasing-B:** Token endpoint has no rate limiting. Team: "Client credentials are required — unauthenticated callers can't succeed, so rate limiting is unnecessary." Is this argument valid?

---

## Pair[CB4-3.4-496]

**A:** OIDC ID token aud validation review. Application uses OIDC for third-party login. Client receives ID tokens. Validation performed: signature verified against JWKS (JSON Web Key Set); `iss` (issuer) claim verified; `exp` (expiration) verified. `aud` (audience) claim is NOT verified. Team: "We verify the signature and issuer — if the token is valid, it was issued by the right server."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

V9.x / RFC 7519 §4.1.3 (aud claim): REPROCESS
Team argument: "signature + issuer verification sufficient"

aud (audience) claim purpose:
  Identifies the intended recipient(s) of the token
  A token issued by the correct issuer may be intended for a different client application

Attack scenario with aud not validated:
  Attacker obtains a valid ID token issued for their own application (client_id=attacker-app)
    by the same IdP (identity provider)
  Token has correct signature ✓; correct issuer ✓; correct exp ✓
  Attacker presents this token to the victim application
  Victim application accepts it — aud not checked ✗
  → Token confusion attack: token intended for attacker-app accepted by victim-app

RFC 7519 §4.1.3: if the "aud" claim is present, it MUST be validated
  Token MUST be rejected if the intended audience does not include the receiving party
  aud typically = client_id of the intended application

Team argument analysis:
  "Signature + issuer = sufficient" → REJECTED
  Signature proves the IdP issued it; issuer proves which IdP; aud proves it was issued FOR THIS app
  All three are required for complete token validation
  → Issuer + signature validation without aud validation is incomplete

Finding: aud claim absent from validation — token confusion attack possible
Overall verdict: REPROCESS
```

**Reason:** `aud` claim validation is required by RFC 7519 §4.1.3. Without it, a token issued by the same IdP for a different client application is accepted — token confusion attack. "Signature + issuer sufficient" is rejected: aud proves the token was issued for this specific application. REPROCESS because required token claim validation is absent.

**Phrasing-B:** OIDC ID token: signature verified, issuer verified, expiry verified, but aud claim not validated. Team: "Signature and issuer verification are sufficient — the token is valid." Is aud validation optional?

---

## Pair[CB4-3.4-497]

**A:** Combined OIDC validation review. OIDC login flow. ID token validation: signature verified against JWKS; `iss` verified against expected issuer; `exp` and `nbf` verified; `aud` verified against application's `client_id`; `nonce` verified against the value sent in the authorization request (single-use). `at_hash` (access token hash) validated when access token is also issued. Team: "We validate all standard ID token claims."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V9-Token + RFC-JWT

T-oauth-oidc + T-rfc-jwt:
  ID token validation checklist:
    Signature via JWKS ✓ (RFC 7519 §4.1 — token integrity)
    iss validated against expected issuer ✓ (RFC 7519 §4.1.1)
    exp validated ✓ (RFC 7519 §4.1.4 — token not expired)
    nbf validated ✓ (RFC 7519 §4.1.5 — token not used before valid time)
    aud validated against client_id ✓ (RFC 7519 §4.1.3 — token confusion prevented)
    nonce validated against sent value (single-use) ✓ (OIDC Core — replay prevented)
    at_hash validated when access token issued ✓ (OIDC Core — access token binding)

  All standard ID token claims validated ✓
  Single-use nonce enforcement ✓
  Access token binding via at_hash ✓

T-oauth-oidc verdict: PASS
T-rfc-jwt verdict: PASS
Overall verdict: PASS
```

**Reason:** Complete OIDC ID token validation: signature, iss, exp, nbf, aud, nonce (with single-use enforcement), and at_hash. Every standard validation requirement satisfied. PASS.

**Phrasing-B:** OIDC ID token validated for: signature, iss, exp, nbf, aud (against client_id), nonce (single-use), at_hash. Does complete ID token claim validation satisfy all OIDC requirements?

---

## Pair[CB4-3.4-498]

**A:** Comprehensive review — all ext19 patterns correctly implemented. OIDC flow: nonce generated per attempt, stored in session, validated (value match + single-use) on ID token receipt; aud validated against client_id; at_hash validated. PAR required for all clients (authorization endpoint accepts only request_uri). Token endpoint: rate limiting with exponential back-off per client_id. Dynamic registration: initial access token + vetting. Introspection cache: TTL + pub/sub invalidation. M2M tokens: minimum scope per operation. All prior controls correct.

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  OIDC nonce: random per attempt; session-stored; value match + single-use validation ✓ — PASS
  OIDC aud: validated against client_id ✓ — PASS (token confusion prevented)
  OIDC at_hash: validated ✓ — PASS
  PAR required: all clients use PAR; authorization endpoint accepts only request_uri ✓ — PASS
  Dynamic registration: initial access token + manual vetting ✓ — PASS
  redirect_uri: RFC 3986-correct comparison (scheme/host lower; path exact) ✓ — PASS

T-rfc-oauth2:
  Token endpoint rate limiting: exponential back-off per client_id ✓ — PASS
  Introspection cache: TTL=token-remaining + pub/sub invalidation ≤100ms ✓ — PASS
  Revocation: client auth required ✓ — PASS

T-asvs-authorization:
  M2M scope: minimum per operation; separate tokens per operation type ✓ — PASS

T-asvs-authentication:
  Argon2id ✓ — PASS

T-ssdf-process:
  Vault ✓; Security Champion with structured checklist ✓ — PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext19 patterns correctly implemented: complete OIDC ID token validation (nonce, aud, at_hash), PAR required for all clients, token endpoint rate limiting, plus all ext18 controls (protected dynamic registration, correct URI comparison, TTL+pub/sub cache, minimum-scope M2M). Every territory returns PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** Complete OIDC validation (nonce, aud, at_hash), PAR mandatory, token endpoint rate-limited, protected dynamic registration, RFC 3986-correct URI comparison, TTL+pub/sub introspection cache, minimum-scope M2M, all prior controls correct. Does this comprehensive implementation pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-491 | OIDC nonce not validated at client — "nonce present = sufficient" rejected | B/D | V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-492 | OIDC nonce fully validated: value match + single-use + invalidation | A | V10-OAuth + RFC-JWT | PASS |
| CB4-3.4-493 | PAR available but not required for public clients — CONCERN | B | V10-OAuth + RFC 9126 | CONCERN |
| CB4-3.4-494 | PAR required for all clients — strongest configuration | A | V10-OAuth + RFC 9126 | PASS |
| CB4-3.4-495 | Token endpoint no rate limiting — "auth requirement substitutes" rejected | B/D | V10-OAuth + V8-AuthZ | CONCERN |
| CB4-3.4-496 | OIDC aud claim not validated — "signature+issuer sufficient" rejected | B/D | V9-Token + RFC-JWT | REPROCESS |
| CB4-3.4-497 | Complete OIDC ID token validation: sig, iss, exp, nbf, aud, nonce, at_hash | A | V10-OAuth + V9-Token + RFC-JWT | PASS |
| CB4-3.4-498 | All ext19 patterns + all prior controls correctly implemented | B | Multiple | PASS |
