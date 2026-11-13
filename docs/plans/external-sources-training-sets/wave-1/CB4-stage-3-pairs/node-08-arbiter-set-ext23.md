# CB4 Stage 3 — Node 08 Arbiter Set Extension 23
# Sequences CB4-3.4-595 through CB4-3.4-602
# Stratum: A/B/D — baselines, team context, adversarial rejection
# New patterns: JWKS key rollover, OIDC front-channel logout, prompt=none silent auth,
#   token exchange (RFC 8693), scope downgrade in assertion
# Instances: 16 (8 base pairs × 2 phrasings)
# Running total after this file: 1204 instances

---

## Pair[CB4-3.4-595]

**A:** JWKS key rollover review. Authorization server rotates signing keys quarterly. During rotation: (1) new key added to JWKS endpoint; (2) previous key kept in JWKS for 24 hours (rollover window); (3) new tokens signed with new key; (4) previously-issued tokens (signed with old key) remain verifiable during the 24-hour rollover window; (5) old key removed after 24 hours. Team: "24-hour rollover ensures no valid tokens are rejected during the rotation."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

V9.2 (signing key lifecycle) / RFC 7517 (JWKS): PASS
  Key rollover design:
    New key added to JWKS before signing new tokens ✓ (pre-publication)
    New tokens signed with new key ✓
    Previous key retained in JWKS for 24 hours ✓
      Previously-issued tokens: max TTL = 60 minutes (assumed); all expire within 24 hours ✓
      24-hour window: covers all outstanding valid tokens signed with old key ✓
    Old key removed after 24 hours ✓

  JWKS rollover security properties:
    No token rejection during rollover: old tokens verifiable until they naturally expire ✓
    Forward security: new tokens use new key after rotation ✓
    No verification window gap: JWKS always contains keys needed to verify any valid token ✓

  Key rollover pattern correctly implemented ✓

Overall verdict: PASS
```

**Reason:** JWKS key rollover with pre-publication, 24-hour dual-key window, and removal after old tokens expire correctly implements forward-secure key rotation without disrupting in-flight tokens. PASS.

**Phrasing-B:** Quarterly signing key rotation: new key pre-published to JWKS, 24-hour dual-key window for existing tokens, old key removed after rollover. Is this the correct JWKS rotation pattern?

---

## Pair[CB4-3.4-596]

**A:** JWKS key rollover — window too short. Authorization server rotates signing keys. Rollover window: 5 minutes (new key added to JWKS; old key available for 5 minutes, then removed). Access tokens: 60-minute TTL. Team: "5 minutes is enough — our rotation is nearly instantaneous."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

V9.2 (signing key lifecycle): CONCERN
  Access token TTL: 60 minutes
  Rollover window: 5 minutes

  Problem:
    Tokens signed with old key: valid for up to 60 minutes ✓ (from issuance perspective)
    Old key in JWKS for only 5 minutes ✗
    Tokens issued in the minute before rotation: still valid for 59 more minutes ✗
    After 5 minutes: old key removed from JWKS ✗
    Resource server tries to verify 10-minute-old token: old key not in JWKS ✗
    → Token verification fails → valid tokens rejected ✗

  Impact:
    Users with valid tokens suddenly get 401 errors 5 minutes after key rotation ✗
    Security impact: not a direct security vulnerability; operational disruption
    Correct practice: rollover window ≥ max access token TTL

  Finding level: CONCERN
    Not a direct security gap (shorter window doesn't create a security attack vector)
    Operational reliability gap: valid tokens may be rejected during rotation
    CONCERN (not REPROCESS — no security attack; operational impact)

Team argument: "nearly instantaneous rotation is better security"
  Faster rotation is good ✓; but rollover window must accommodate outstanding valid tokens ✗
  → CONCERN

Overall verdict: CONCERN
```

**Reason:** 5-minute JWKS rollover window with 60-minute access token TTL means valid tokens will fail verification after the old key is removed. Rollover window must be at least as long as the maximum token TTL. CONCERN (operational reliability gap; not a direct security vulnerability).

**Phrasing-B:** Signing key rotation with 5-minute rollover window. Access tokens have 60-minute TTL. Team: "Nearly instantaneous rotation is better security." Is a 5-minute window sufficient?

---

## Pair[CB4-3.4-597]

**A:** OIDC front-channel logout review. OIDC relying party (RP — Relying Party) implements front-channel logout (OIDC Front-Channel Logout spec). On logout: authorization server loads a hidden iframe in the browser pointing to the RP's logout endpoint. Team: "Front-channel logout is defined in OIDC — it's the standard way to propagate logout to all RPs."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

OIDC Front-Channel Logout: CONCERN
Team argument: "standard OIDC logout propagation"

Front-channel logout analysis:
  Mechanism: authorization server renders hidden iframes in browser pointing to each RP ✓
  RP logout endpoint called via browser-side iframe ✓

Security limitations of front-channel logout:
  (1) Depends on browser: user must have browser open/active during logout propagation ✗
  (2) Browser security features (SameSite=Strict cookies, partitioned storage) may prevent
      the RP's logout endpoint from processing the request correctly ✗
  (3) ITP (Intelligent Tracking Prevention) / privacy features may block the iframe ✗
  (4) Ad blockers may block the iframe requests ✗
  (5) If RP logout endpoint fails silently: RP session remains active ✗

  Comparison to back-channel logout:
    Back-channel logout (OIDC Back-Channel Logout spec): server-to-server logout token delivery
    More reliable: not subject to browser restrictions ✓
    More secure: logout not visible in browser ✓

Team argument: "standard OIDC"
  Front-channel logout is defined in OIDC specs ✓
  However: back-channel logout is more reliable and not subject to browser fragility ✗
  → CONCERN: front-channel logout is spec-defined but brittle in modern browser environments

Finding: CONCERN — front-channel logout present; browser reliability limitations noted
Note: for high-security contexts (banking, healthcare), back-channel logout is strongly preferred
Overall verdict: CONCERN
```

**Reason:** Front-channel logout is defined in OIDC specs but relies on browser-rendered iframes, which modern browsers may block (SameSite cookies, ITP, ad blockers). CONCERN — the mechanism exists but browser reliability limitations make it fragile. Back-channel logout provides more reliable propagation. "Standard OIDC" argument is correct that it's defined; the concern is operational reliability.

**Phrasing-B:** OIDC front-channel logout via hidden iframes. Team: "Standard OIDC logout propagation." Is front-channel logout the recommended approach?

---

## Pair[CB4-3.4-598]

**A:** OIDC back-channel logout correctly implemented. Authorization server: OIDC Back-Channel Logout spec. On logout: authorization server sends a logout token (signed JWT with `sub`, `sid`, `iss`, `iat`, `jti`, `events` claim) via POST to each RP's back-channel logout endpoint. RP validates: signature, iss, aud, iat, jti uniqueness, `events` claim. RP terminates the session for the `sub`/`sid` pair.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-JWT

OIDC Back-Channel Logout: PASS
  Logout token: signed JWT ✓
  Claims: sub ✓; sid ✓; iss ✓; iat ✓; jti ✓; events claim ✓
  RP validation:
    Signature validated ✓
    iss validated ✓; aud validated ✓; iat validated ✓
    jti uniqueness checked ✓ (replay prevention for logout tokens)
    events claim: {"http://schemas.openid.net/event/backchannel-logout": {}} ✓
  Session termination: sub/sid pair session invalidated ✓

Back-channel logout advantages:
  Server-to-server: not subject to browser restrictions ✓
  Reliable: works regardless of browser state ✓
  jti replay prevention: logout token cannot be reused ✓

Overall verdict: PASS
```

**Reason:** Back-channel logout with complete logout token validation (signature, iss, aud, iat, jti uniqueness, events claim) and session termination for sub/sid pair. Server-to-server propagation not subject to browser restrictions. PASS.

**Phrasing-B:** OIDC back-channel logout: signed logout token, jti uniqueness check, session terminated on receipt. Is back-channel logout the more reliable pattern?

---

## Pair[CB4-3.4-599]

**A:** OIDC `prompt=none` silent authentication. Application uses OIDC `prompt=none` in authorization requests to silently check if the user is still authenticated. If the user is authenticated, an authorization code is returned without user interaction. If not authenticated, `login_required` error is returned. Team: "prompt=none enables seamless user experience — automatic session renewal without interruption."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session

T-oauth-oidc:
  prompt=none: OIDC Core parameter — silent authentication check ✓
  Authorization code returned if authenticated ✓
  login_required error if not authenticated ✓

  Analysis:
    prompt=none is a defined OIDC parameter ✓
    Used correctly (check if authenticated; handle both success and failure) ✓
    Security considerations:
      (1) prompt=none request must include PKCE S256 ✓ (prevents code interception)
      (2) redirect_uri must be registered exactly ✓ (prevents authorization code theft)
      (3) id_token_hint should be included to bind the silent check to the expected user ✓
          (prevents session mix-up if multiple users share the browser)

  If PKCE, exact redirect_uri, and id_token_hint are all present:
    prompt=none is a compliant, correct use of OIDC ✓

T-asvs-session:
  Session renewal without user interaction: acceptable for SSO (Single Sign-On) scenarios ✓
  User's session is not extended beyond authorization server session lifetime ✓

Finding: whether PASS depends on whether PKCE + exact redirect_uri + id_token_hint are present
  The description says PKCE and redirect_uri are in place ✓ (per prior controls)
  → PASS (correct use of OIDC Core prompt=none with required controls)

Overall verdict: PASS
```

**Reason:** `prompt=none` is a defined OIDC Core parameter for silent authentication checks. Used correctly with PKCE, exact redirect_uri (from prior controls), and code-only flow. PASS. Note: if PKCE or exact redirect_uri were absent, this would be a concern.

**Phrasing-B:** OIDC prompt=none for silent session check. Returns code if authenticated, login_required if not. Is prompt=none a compliant, secure pattern when used with PKCE and exact redirect_uri?

---

## Pair[CB4-3.4-600]

**A:** Token exchange review — RFC 8693. Authorization server supports token exchange (RFC 8693): a service can exchange an access token for a narrower-scope access token. `subject_token` = original access token; `actor_token` absent; output: new access token with `audience` restricted to the downstream service and `scope` narrowed to what the downstream service needs. Original scope: `read:data write:data admin:config`. Exchanged scope: `read:data`. Team: "Token exchange with scope narrowing is how we implement least-privilege delegation."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

RFC 8693 (Token Exchange) / V8.1.2 (least privilege): PASS
  Token exchange mechanism:
    subject_token: original access token ✓ (proof of authorization)
    Requested audience: downstream service URL ✓ (audience restriction)
    Requested scope: read:data (subset of original) ✓ (scope narrowing)
    Output: new access token with narrower scope and restricted audience ✓

  Scope narrowing via exchange:
    Original: read:data write:data admin:config
    Exchanged: read:data
    Principle: downstream service receives only what it needs ✓
    admin:config removed: downstream cannot accidentally call admin endpoints ✓

  Least-privilege implementation:
    Each service receives a token with only its required permissions ✓
    V8.1.2 least-privilege satisfied via token exchange ✓

  RFC 8693 §2.1: token exchange request parameters correct ✓
    grant_type: urn:ietf:params:oauth:grant-type:token-exchange ✓
    subject_token_type: urn:ietf:params:oauth:token-type:access_token ✓

Overall verdict: PASS
```

**Reason:** Token exchange (RFC 8693) with scope narrowing for downstream service access correctly implements least-privilege delegation. The exchanged token carries only the scope the downstream service needs. PASS.

**Phrasing-B:** RFC 8693 token exchange: original token with broad scope exchanged for narrow-scope token restricted to downstream service audience. Is token exchange with scope narrowing a correct least-privilege pattern?

---

## Pair[CB4-3.4-601]

**A:** Token exchange with scope widening. Authorization server: RFC 8693 token exchange. A service exchanges a `read:data` access token for a new token requesting `scope=admin:config`. Team: "Token exchange allows us to elevate permissions when needed — the user already authorized the read:data scope, so admin is an extension of that."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

RFC 8693 (Token Exchange) / V8.1.2: REPROCESS
Team argument: "read:data authorization extends to admin:config"

Token exchange scope analysis:
  Original token: scope=read:data (authorization for data read only)
  Requested exchange: scope=admin:config (system configuration access)

RFC 8693 §7.1 (security considerations):
  Exchange should NOT produce a token with more privileges than the subject_token
  Scope widening via exchange: creates a privilege escalation path ✗
  The subject token holder should not be able to exchange for a broader scope ✗

V8.1.2 (least privilege):
  Token exchange should narrow, not widen, scope ✓ (principle)
  Exchange for admin:config from read:data: scope widening ✗

Team argument: "user already authorized read:data = authorization for admin extension"
  read:data authorization is specific to data reading ✗
  admin:config requires independent user consent ✗
  Token exchange is not a mechanism for scope escalation ✗
  → REJECTED

Finding: REPROCESS — token exchange used for privilege escalation (scope widening)
Overall verdict: REPROCESS
```

**Reason:** Token exchange for a scope broader than the subject token's scope constitutes privilege escalation. RFC 8693 §7.1 warns against producing tokens with more privileges than the subject token. "Read:data extends to admin:config" is rejected — scopes require independent authorization; exchange cannot manufacture new permissions. REPROCESS.

**Phrasing-B:** Token exchange: service exchanges read:data token requesting admin:config. Team: "User authorized read:data — admin is an extension." Is scope widening via token exchange acceptable?

---

## Pair[CB4-3.4-602]

**A:** Comprehensive review — all ext23 patterns correctly implemented. JWKS key rollover: quarterly, 24-hour dual-key window (covers all outstanding access tokens); back-channel logout implemented with jti-validated logout tokens; prompt=none with PKCE S256 and exact redirect_uri; token exchange for scope narrowing (not widening); JWT client assertions with complete validation; resource indicators; SBOM auto-updated; all prior controls correct.

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  JWKS rotation: quarterly ✓; 24-hour dual-key window ✓ — PASS
  Back-channel logout: logout token validated ✓ (sig, iss, aud, iat, jti) ✓ — PASS
  prompt=none: with PKCE S256 + exact redirect_uri ✓ — PASS
  Token exchange: scope narrowing only ✓ (RFC 8693 §7.1) — PASS
  Resource indicators ✓; JWT assertions with jti ✓ — PASS

T-rfc-jwt:
  JWKS rollover: old key retained until outstanding tokens expire ✓ — PASS
  Logout tokens: jti uniqueness ✓ — PASS

T-ssdf-process:
  SBOM auto-updated ✓; PO.1 training ✓; Vault ✓ — PASS

T-asvs-authentication: Argon2id ✓ — PASS

All prior controls: PKCE S256 ✓; code-only ✓; PAR ✓; nonce+aud ✓; state 256-bit ✓;
  DPoP ✓; mTLS ✓; refresh family revocation ✓; UserInfo sub-check ✓;
  dynamic reg protected ✓; introspection TTL+pub/sub ✓; M2M minimum-scope ✓ — all PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** All ext23 patterns correctly implemented: JWKS rollover window, back-channel logout, correct prompt=none usage, scope-narrowing token exchange, plus all prior controls. Every territory PASS. Comprehensive multi-territory PASS.

**Phrasing-B:** JWKS 24-hour rollover, back-channel logout with jti, correct prompt=none, scope-narrowing token exchange, JWT assertions, SBOM, all prior controls. Does this pass all territories?

---

## File Summary

| Pair ID | Description | Stratum | Arbiter | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-595 | JWKS key rollover: quarterly, 24-hour dual-key window | A | V9-Token + RFC-JWT | PASS |
| CB4-3.4-596 | JWKS rollover 5-minute window with 60-minute tokens → CONCERN (valid tokens rejected) | B | V9-Token + RFC-JWT | CONCERN |
| CB4-3.4-597 | OIDC front-channel logout via iframes — browser reliability concern | B | V10-OAuth | CONCERN |
| CB4-3.4-598 | OIDC back-channel logout with logout token + jti validation | A | V10-OAuth + RFC-JWT | PASS |
| CB4-3.4-599 | OIDC prompt=none silent check with PKCE + exact redirect_uri | A | V10-OAuth + V7-Session | PASS |
| CB4-3.4-600 | RFC 8693 token exchange with scope narrowing — least-privilege delegation | A | V10-OAuth + RFC-OAuth2 | PASS |
| CB4-3.4-601 | Token exchange scope widening — "authorization extends" rejected | B/D | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-602 | All ext23 patterns correctly implemented — comprehensive PASS | B | Multiple | PASS |
