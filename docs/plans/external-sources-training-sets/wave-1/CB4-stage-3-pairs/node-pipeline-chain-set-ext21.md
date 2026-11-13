# CB4 Stage 3 — Multi-Node Pipeline Chain Set ext21
# Sequences CB4-3.4-707 through CB4-3.4-714
# Stratum C — full 9-node pipeline traces for ext27 patterns
# Running total after this file: 1428 instances (714 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-707]

**A**
```
Incoming pipeline input:
"SPA sends ID token in Authorization: Bearer header to resource API.
Developer: ID token contains sub, roles, exp — all info the RS needs.
No separate access token flow implemented."
```

**B**
```
Pipeline trace — CB4-3.4-707

Node 1 — Domain Splitter:
  Input: token type used for resource server API authorization
  Domains: T-oauth-oidc + T-rfc-oauth2
  Sub-phase: 3.4

Node 2 — Domain Translator:
  OIDC Core §2: ID token = authentication assertion; aud = client_id
  ASVS V10.5.3: ID token MUST NOT be used to authenticate to APIs
  Access token: designed for resource authorization; carries scopes; aud = RS identifier

Node 3 — Scheme Planner:
  Check 1: Is an ID token being used at a resource server? → YES
  Check 2: Is this prohibited by ASVS V10.5.3? → YES (MUST NOT)
  Check 3: What is the access token role? → not implemented
  Plan: OIDC Core §2 misuse + V10.5.3 MUST NOT assessment

Node 4 — Domain Processor:
  ID token aud: set to client_id at authentication → RS identifier different → aud mismatch
  No scopes: ID token contains authentication claims; no resource authorization scopes
  No revocation: ID token cannot be revoked via RFC 7009
  "Contains all info the RS needs" → incorrect: roles in ID token ≠ access token scopes;
    access model is different; RS should validate aud=RS-id, not client_id

Node 5 — Applier:
  REPROCESS (V10.5.3 MUST NOT; OIDC Core §2 misuse)

Node 6 — Result Translator:
  ID token misused as API credential; V10.5.3 + OIDC Core §2 violated

Node 7 — Arbiter Selector:
  T-oauth-oidc — ID token purpose violation

Node 8 — Arbiter:
  REPROCESS — Using ID token at resource server violates ASVS V10.5.3 and OIDC Core §2.
  ID token aud = client_id (not RS); carries no scopes; cannot be revoked via RFC 7009.
  Implement authorization code flow with scopes; use the resulting access token at the RS.

Node 9 — Formatter:
  REPROCESS
  OIDC Core §2: ID token is authentication assertion for client, not RS
  V10.5.3: MUST NOT use ID token for API authentication
  Remediation: access token flow with proper scopes and RS aud validation
```

**Reason**
Full pipeline trace for ID token misused as API credential. Node 4 traces three independent gaps (aud mismatch, no scopes, no revocation) and rejects the developer's "contains all info" argument. V10.5.3 MUST NOT produces REPROCESS.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-707 (phrasing B)

Node 1: T-oauth-oidc + T-rfc-oauth2
Node 2: OIDC Core §2 ID token for client; V10.5.3 MUST NOT for API; access token for RS
Node 4: ID token aud=client_id ≠ RS; no scopes; not revocable; "has all info" → incorrect
Node 8 — Arbiter: REPROCESS — V10.5.3 MUST NOT. Use access token for RS auth.
Node 9: REPROCESS; V10.5.3 + OIDC Core §2 cited
```

---

## Pair[CB4-3.4-708]

**A**
```
Incoming pipeline input:
"OIDC: ID token used for UI display and id_token_hint only. Access token sent
in Bearer header to RS. RS validates sig, iss, exp, aud=resource-server-id, scope."
```

**B**
```
Pipeline trace — CB4-3.4-708

Node 1 — Domain Splitter:
  Input: correct OIDC token usage
  Domains: T-oauth-oidc + T-rfc-oauth2
  Sub-phase: 3.4

Node 2 — Domain Translator:
  OIDC Core §2: ID token for client (authentication assertion)
  V10.5.3: ID token MUST NOT be used for API auth
  Access token: Bearer header for RS; aud = RS identifier; scopes

Node 3 — Scheme Planner:
  Check 1: ID token used at RS? → NO (display + id_token_hint only)
  Check 2: Access token used for API? → YES (Bearer + RS aud validation)
  Plan: correct role separation; PASS expected

Node 4 — Domain Processor:
  ID token: display + id_token_hint → OIDC Core §2 correct use → PASS
    V10.5.3 satisfied: ID token NOT used for API auth
  Access token: Bearer → RS validates aud=resource-server-id + scope → PASS
    aud validation prevents token confusion; scope validates authorization intent

Node 5 — Applier:
  PASS (V10.5.3 satisfied; access token for RS; correct role separation)

Node 6 — Result Translator:
  Correct token role separation; no OIDC misuse

Node 7 — Arbiter Selector:
  T-oauth-oidc — token usage PASS

Node 8 — Arbiter:
  PASS — ID token used only for display and id_token_hint; access token presented to
  RS with RS-specific aud validation and scope checking. OIDC Core §2 and V10.5.3
  satisfied. No findings.

Node 9 — Formatter:
  PASS
  ID token: display + id_token_hint only ✓
  Access token: Bearer + aud=RS-id + scope ✓
  V10.5.3 satisfied ✓
```

**Reason**
Full pipeline trace for correct OIDC token role separation. Node 3 plans both checks (ID token not at RS, access token for API). Node 4 verifies both and confirms aud=resource-server-id prevents token confusion.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-708 (phrasing B)

Node 1: T-oauth-oidc + T-rfc-oauth2
Node 3: ID token at RS? → NO; access token for API? → YES
Node 4: ID token display + hint → PASS; access token + RS aud validation → PASS
Node 8 — Arbiter: PASS — Correct token role separation. V10.5.3 satisfied. No findings.
Node 9: PASS; role separation confirmed
```

---

## Pair[CB4-3.4-709]

**A**
```
Incoming pipeline input:
"Refresh tokens have no absolute max lifetime. Rotation and family revocation
are in place. Developer argues: continuous active use proves ongoing consent."
```

**B**
```
Pipeline trace — CB4-3.4-709

Node 1 — Domain Splitter:
  Input: refresh token lifetime design
  Domains: T-rfc-oauth2 (BCP §4.14) + T-asvs-session (V7.2.4)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  BCP §4.14: refresh token rotation REQUIRED; absolute expiration RECOMMENDED (SHOULD)
  V7.2.4: both idle timeout and absolute maximum session duration recommended

Node 3 — Scheme Planner:
  Check 1: Rotation present? → YES (PASS)
  Check 2: Family revocation present? → YES (PASS)
  Check 3: Absolute max lifetime present? → NO
  Plan: BCP §4.14 SHOULD gap; developer argument assessment

Node 4 — Domain Processor:
  Rotation + family revocation: BCP §4.14.1 satisfied → PASS
  No absolute max lifetime: BCP §4.14 SHOULD gap
  Developer argument: "active use = ongoing consent"
    Legitimate UX concern; but security argument is insufficient
    Attacker who steals a refresh token and uses it regularly (before legitimate user does)
    → maintains indefinite session; family revocation only triggers on replay (same token used twice)
    If attacker uses token FIRST each rotation cycle, legitimate user's token is the "revoked" one
    Active use does not guarantee the active user is the legitimate user
  SHOULD level → CONCERN not REPROCESS

Node 5 — Applier:
  CONCERN (no absolute max lifetime; SHOULD gap; developer argument rebutted)

Node 6 — Result Translator:
  CONCERN — rotation + family revocation PASS; absolute lifetime absent

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — refresh token lifetime

Node 8 — Arbiter:
  CONCERN — Rotation and family revocation correctly implemented. The gap is the absence
  of an absolute maximum lifetime. "Active use = ongoing consent" does not address an attacker
  who actively rotates a stolen token. Implement absolute max lifetime (e.g., 90 days).

Node 9 — Formatter:
  CONCERN
  BCP §4.14.1: rotation + family revocation PASS
  BCP §4.14 SHOULD: absolute max lifetime absent
  Developer argument: "active use" insufficient — attacker can be the active user
```

**Reason**
Full pipeline trace for refresh token without absolute max lifetime. Node 4 evaluates the "active use = ongoing consent" argument and rejects it with the attacker-rotates-first attack scenario. SHOULD level → CONCERN via Node 5.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-709 (phrasing B)

Node 1: T-rfc-oauth2 (BCP §4.14) / T-asvs-session (V7.2.4)
Node 3: rotation + family revoc → PASS; absolute max → absent
Node 4: "active use = consent" → attacker rotates first → indefinite access; BCP SHOULD → CONCERN
Node 8 — Arbiter: CONCERN — Rotation correct; no absolute max lifetime (BCP §4.14 SHOULD).
  Add 90-day absolute max to force re-authentication.
Node 9: CONCERN; rotation PASS; absolute lifetime gap
```

---

## Pair[CB4-3.4-710]

**A**
```
Incoming pipeline input:
"Refresh token lifecycle: rotation per use, family revocation on replay, 90-day
absolute max, 30-day idle timeout, 15-minute access token TTL."
```

**B**
```
Pipeline trace — CB4-3.4-710

Node 1 — Domain Splitter:
  Input: refresh token lifecycle — all controls stated
  Domains: T-rfc-oauth2 (BCP §4.14) + T-asvs-session (V7.2.4)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  BCP §4.14: rotation + family revocation REQUIRED; absolute expiration RECOMMENDED
  V7.2.4: idle + absolute timeouts

Node 3 — Scheme Planner:
  Five controls to verify:
  1. Rotation per use
  2. Family revocation on replay
  3. 90-day absolute max
  4. 30-day idle timeout
  5. 15-min access token TTL
  Plan: verify each; PASS expected

Node 4 — Domain Processor:
  Rotation: each use → new token; old invalidated → BCP §4.14.1 PASS
  Family revocation: replay → family revoked → BCP §4.14.1 PASS
  90-day absolute max: re-auth required at 90 days regardless of rotation → SHOULD satisfied → PASS
  30-day idle: sliding window; no use in 30 days → expired → V7.2.4 idle timeout → PASS
  15-min access token TTL: short; defense in depth; limits post-revocation window → PASS

Node 5 — Applier:
  PASS × 5 controls

Node 6 — Result Translator:
  All refresh token lifecycle controls present and correctly configured

Node 7 — Arbiter Selector:
  T-rfc-oauth2 + T-asvs-session — refresh token lifecycle PASS

Node 8 — Arbiter:
  PASS — All refresh token lifecycle controls implemented. Rotation, family revocation,
  90-day absolute max, 30-day idle, 15-min access token TTL. BCP §4.14 and V7.2.4
  both satisfied. No findings.

Node 9 — Formatter:
  PASS
  BCP §4.14.1: rotation + family revocation ✓
  BCP §4.14 SHOULD: absolute max (90d) ✓
  V7.2.4: idle timeout (30d) ✓
  Access token TTL: 15 min ✓
```

**Reason**
Full pipeline trace for the correct refresh token lifecycle PASS. Node 3 plans five checks; Node 4 verifies each against BCP §4.14 and V7.2.4. All five pass.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-710 (phrasing B)

Node 1: T-rfc-oauth2 / T-asvs-session
Node 3: five controls — rotation, family revoc, 90d absolute, 30d idle, 15min TTL
Node 4: all five PASS
Node 8 — Arbiter: PASS — All refresh token lifecycle controls correct. No findings.
Node 9: PASS; BCP §4.14 + V7.2.4 confirmed
```

---

## Pair[CB4-3.4-711]

**A**
```
Incoming pipeline input:
"RS accepts JWTs with any algorithm including HS256 and 'none'. Validates sig,
iss, exp, aud. Developer: the JWT library handles algorithm flexibility automatically."
```

**B**
```
Pipeline trace — CB4-3.4-711

Node 1 — Domain Splitter:
  Input: resource server JWT algorithm enforcement
  Domains: T-rfc-jwt (RFC 8725) + T-asvs-token (V9.1.1)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  V9.1.1: algorithm allowlist MUST be enforced; "none" MUST be rejected
  RFC 8725 §3.1: allowlists preferred over blocklists; implementations MUST NOT process "none"
  Algorithm confusion: HS256 with RSA public key → forge valid-appearing JWT

Node 3 — Scheme Planner:
  Check 1: Algorithm allowlist present? → NO
  Check 2: HS256 accepted? → YES → algorithm confusion attack surface open
  Check 3: "none" accepted? → YES → unsigned JWT attack surface open
  Plan: V9.1.1 MUST assessment; two attack vectors

Node 4 — Domain Processor:
  "JWT library handles flexibility" → developer delegates security to library defaults
    Library defaults: typically permissive; not a security configuration
    V9.1.1 requires explicit allowlist configuration; library defaults ≠ allowlist
  HS256 accepted: attacker obtains AS public key (JWKS public) → uses as HMAC secret
    Signs HS256 JWT with arbitrary claims → RS verifies HMAC using public key → PASS
    → complete authorization bypass; no private key required
  "none" accepted: unsigned JWT submitted → RS processes without signature → auth bypass
  V9.1.1 MUST + RFC 8725 §3.1 → REPROCESS

Node 5 — Applier:
  REPROCESS (V9.1.1 MUST; two attack vectors)

Node 6 — Result Translator:
  REPROCESS — algorithm allowlist absent; HS256 confusion + "none" attack open

Node 7 — Arbiter Selector:
  T-rfc-jwt + T-asvs-token — RS algorithm enforcement

Node 8 — Arbiter:
  REPROCESS — No algorithm allowlist violates V9.1.1 MUST. HS256 confusion attack: attacker
  uses AS public key as HMAC secret to forge tokens. "none" attack: unsigned JWT accepted.
  "Library handles it" is not a security configuration — V9.1.1 requires explicit allowlist.
  Configure allowlist ["RS256"] or ["RS256", "PS256"]; check before signature verification.

Node 9 — Formatter:
  REPROCESS
  V9.1.1 MUST: no allowlist
  Attack vector 1: HS256 confusion (public key as HMAC secret)
  Attack vector 2: "none" algorithm (unsigned JWT)
  "Library flexibility" argument rejected
```

**Reason**
Full pipeline trace for RS without algorithm allowlist. Node 4 traces both attack vectors (HS256 confusion and "none") and rejects the "library handles it" argument (library defaults are permissive, not a security configuration). V9.1.1 MUST → REPROCESS.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-711 (phrasing B)

Node 1: T-rfc-jwt + T-asvs-token
Node 2: V9.1.1 MUST allowlist; RFC 8725 §3.1 MUST NOT process "none"
Node 4: no allowlist; HS256 confusion + "none" attack open; "library handles it" rejected
Node 8 — Arbiter: REPROCESS — V9.1.1 MUST. HS256 + "none" attack vectors open.
  Configure explicit allowlist; check before signature.
Node 9: REPROCESS; V9.1.1 + RFC 8725; two vectors
```

---

## Pair[CB4-3.4-712]

**A**
```
Incoming pipeline input:
"RS JWT validation: alg extracted from JWT header first. Checked against allowlist
['RS256', 'PS256'] before any signature verification. Other alg values → 401 rejected.
Then: signature verified using JWKS public key, iss, exp, aud, scope validated."
```

**B**
```
Pipeline trace — CB4-3.4-712

Node 1 — Domain Splitter:
  Input: RS JWT algorithm enforcement — correct implementation
  Domains: T-rfc-jwt (RFC 8725) + T-asvs-token (V9.1.1)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  V9.1.1: algorithm allowlist MUST; "none" rejected
  RFC 8725 §3.1: allowlist approach; check BEFORE signature verification

Node 3 — Scheme Planner:
  Check 1: Allowlist present? → YES (['RS256', 'PS256'])
  Check 2: Check order — allowlist BEFORE signature? → YES
  Check 3: "none" in allowlist? → NO (rejected)
  Check 4: HS* in allowlist? → NO (eliminated)
  Plan: all V9.1.1 controls present; correct order → PASS

Node 4 — Domain Processor:
  alg extracted first → allowlist checked → signature only if alg allowed → correct order → PASS
  If alg check were AFTER signature: JWT library might process "none" or HS256 before rejection
  ['RS256', 'PS256']: both are asymmetric RSA-based → no HMAC confusion possible → PASS
  "none" not in allowlist → 401 before any processing → PASS
  iss/exp/aud/scope after signature → complete validation chain → PASS
  V9.1.1 MUST: all conditions met; RFC 8725 §3.1 allowlist approach used

Node 5 — Applier:
  PASS (V9.1.1 MUST + RFC 8725 §3.1 satisfied; correct order)

Node 6 — Result Translator:
  RS algorithm enforcement correct; no attack vectors open

Node 7 — Arbiter Selector:
  T-rfc-jwt + T-asvs-token — RS algorithm PASS

Node 8 — Arbiter:
  PASS — RS JWT algorithm enforcement correctly implemented. Allowlist ['RS256', 'PS256']
  checked BEFORE signature verification. "none" and HS* algorithms rejected. Full validation
  chain after allowlist check. V9.1.1 MUST and RFC 8725 §3.1 both satisfied. No findings.

Node 9 — Formatter:
  PASS
  V9.1.1 MUST: allowlist ['RS256', 'PS256'] ✓
  Order: allowlist checked before signature ✓
  "none" + HS* rejected ✓
  Full chain: sig → iss/exp/aud/scope ✓
```

**Reason**
Full pipeline trace for correct RS algorithm enforcement. Node 4 explicitly evaluates the ordering requirement (allowlist MUST be checked before signature verification) — the key technical detail that prevents the JWT library from processing insecure algorithms before rejection.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-712 (phrasing B)

Node 1: T-rfc-jwt + T-asvs-token
Node 3: allowlist present; checked before signature; "none" absent; HS* absent
Node 4: correct order; RSA-only allowlist → no HMAC confusion; full validation chain
Node 8 — Arbiter: PASS — Allowlist checked before signature. V9.1.1 + RFC 8725 satisfied.
Node 9: PASS; allowlist + ordering confirmed
```

---

## Pair[CB4-3.4-713]

**A**
```
Incoming pipeline input:
"Refresh token revocation (via RFC 7009 or logout): refresh token invalidated in DB.
Associated access tokens remain valid for up to 60 minutes (natural expiry).
Developer: 'Adding an access token blocklist adds complexity we'd rather avoid.'"
```

**B**
```
Pipeline trace — CB4-3.4-713

Node 1 — Domain Splitter:
  Input: refresh token revocation cascade to access tokens
  Domains: T-rfc-oauth2 (RFC 7009) + T-asvs-session (V10.3)
  Sub-phase: 3.4

Node 2 — Domain Translator:
  RFC 7009 §2: revocation of refresh token MAY also revoke associated access tokens
  V10.3: logout should result in token revocation; bounded access token lifetime is a design choice

Node 3 — Scheme Planner:
  Check 1: Refresh token invalidated on revocation? → YES (PASS)
  Check 2: Access tokens cascade-revoked? → NO
  Check 3: RFC 7009 cascade requirement level? → MAY
  Check 4: 60-minute TTL window significance? → assess
  Plan: MAY-level gap; 60-min window assessment

Node 4 — Domain Processor:
  Refresh token invalidated → PASS
  Access tokens not cascade-revoked: RFC 7009 §2 MAY → not MUST violation → not REPROCESS
  60-minute post-revocation window: meaningful if revocation triggered by compromise
    Attacker's access tokens remain valid for up to 60 minutes after detection
  "Blocklist adds complexity" → legitimate argument; acknowledged; but doesn't eliminate CONCERN
  Compare: 15-minute TTL → smaller CONCERN window; 60-minute → larger CONCERN window
  Not a REPROCESS (MAY not MUST); CONCERN calibrated by window size

Node 5 — Applier:
  CONCERN (RFC 7009 MAY; 60-minute post-revocation window)

Node 6 — Result Translator:
  CONCERN — spec-permitted; meaningful post-revocation exposure

Node 7 — Arbiter Selector:
  T-rfc-oauth2 — revocation cascade

Node 8 — Arbiter:
  CONCERN — Refresh token revocation without access token cascade leaves a 60-minute
  post-revocation window. RFC 7009 §2 cascade is optional (MAY), so this is not a MUST
  violation. However, a 60-minute window is meaningful in a compromise scenario. Options:
  (1) Reduce access token TTL to 5-15 minutes to minimize post-revocation exposure;
  (2) Implement a short-lived access token blocklist (entries expire with the token).

Node 9 — Formatter:
  CONCERN
  RFC 7009 §2: cascade is MAY (not MUST)
  60-minute post-revocation window: meaningful in compromise scenario
  Complexity argument: acknowledged; does not eliminate CONCERN
  Options: reduce TTL or implement blocklist
```

**Reason**
Full pipeline trace for revocation cascade gap. Node 4 evaluates the complexity argument (acknowledged but insufficient) and calibrates the CONCERN by the TTL window size. RFC 7009 MAY → CONCERN not REPROCESS. Options are provided in Node 8.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-713 (phrasing B)

Node 1: T-rfc-oauth2 (RFC 7009) / T-asvs-session (V10.3)
Node 2: RFC 7009 §2 cascade MAY (not MUST)
Node 4: refresh revoked → PASS; access tokens not cascade → 60-min window → CONCERN
  "Blocklist complex" → acknowledged; CONCERN remains
Node 8 — Arbiter: CONCERN — RFC 7009 §2 MAY gap. 60-min post-revocation window.
  Reduce TTL or add TTL-scoped blocklist.
Node 9: CONCERN; MAY gap; 60-min window
```

---

## Pair[CB4-3.4-714]

**A**
```
Incoming pipeline input — full ext27 PASS scenario:
"Tokens: ID token for display only; access token for RS. RS validates aud=RS-id, scope,
alg in ['RS256']. Refresh: rotation, family revoc, 90d absolute, 30d idle, 15min TTL.
Revocation cascade: access token blocklist on refresh revocation (TTL-scoped)."
```

**B**
```
Pipeline trace — CB4-3.4-714

Node 1 — Domain Splitter:
  Input: comprehensive ext27 pattern review
  Domains: T-oauth-oidc / T-rfc-oauth2 × 2 / T-rfc-jwt + T-asvs-token
  Sub-phase: 3.4

Node 2 — Domain Translator:
  V10.5.3: ID token for display only; access token for RS
  BCP §4.14 + V7.2.4: refresh token lifecycle
  RFC 7009 §2: revocation cascade
  V9.1.1 + RFC 8725: RS algorithm allowlist

Node 3 — Scheme Planner:
  Four patterns; each with sub-requirements:
  A: ID token use; RS aud + scope + alg validation
  B: refresh token lifecycle (5 controls)
  C: revocation cascade implementation
  D: RS algorithm allowlist

Node 4 — Domain Processor:
  A: ID token display only → V10.5.3 PASS; RS validates aud=RS-id + scope + alg → PASS
  B: rotation + family revoc + 90d absolute + 30d idle + 15min TTL → BCP §4.14 + V7.2.4 PASS
  C: access token blocklist on refresh revocation + TTL-scoped → stronger than MAY → PASS
  D: RS allowlist ['RS256'] checked before signature → V9.1.1 + RFC 8725 PASS

Node 5 — Applier:
  PASS × 4 patterns

Node 6 — Result Translator:
  All ext27 patterns correct

Node 7 — Arbiter Selector:
  Comprehensive multi-domain PASS

Node 8 — Arbiter:
  PASS — All four ext27 patterns correctly implemented. Token usage: ID token for display;
  RS validates aud, scope, and alg allowlist. Refresh lifecycle: rotation, family revocation,
  90-day absolute, 30-day idle, 15-min TTL. Revocation cascade: TTL-scoped access token
  blocklist on refresh revocation. RS algorithm allowlist ['RS256'] checked before signature.
  No findings.

Node 9 — Formatter:
  PASS
  V10.5.3: ID token use ✓
  BCP §4.14 + V7.2.4: refresh lifecycle ✓
  RFC 7009 cascade: implemented (TTL-scoped blocklist) ✓
  V9.1.1 + RFC 8725: RS algorithm allowlist ✓
```

**Reason**
Comprehensive PASS pipeline trace for all four ext27 patterns. Node 3 identifies sub-requirements for each; Node 4 verifies all. The revocation cascade is confirmed as "stronger than MAY" — the TTL-scoped blocklist implements the optional cascade mechanism efficiently.

**Phrasing-B**
```
Pipeline trace — CB4-3.4-714 (phrasing B)

Node 1: T-oauth-oidc / T-rfc-oauth2 × 2 / T-rfc-jwt + T-asvs-token
Node 4:
  A: ID token display only + RS aud/scope/alg → PASS
  B: five refresh lifecycle controls → PASS
  C: TTL-scoped access blocklist on revocation → PASS
  D: ['RS256'] before sig → PASS
Node 8 — Arbiter: PASS — All four ext27 patterns correct. No findings.
Node 9: PASS; four patterns confirmed
```
