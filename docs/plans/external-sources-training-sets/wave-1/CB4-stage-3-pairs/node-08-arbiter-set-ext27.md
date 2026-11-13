# CB4 Stage 3 — Node 8 Arbiter Baseline Set ext27
# Sequences CB4-3.4-699 through CB4-3.4-706
# Stratum A/B/D — ID-token-API-auth / refresh-absolute-lifetime / revocation-cascade / RS-algorithm-enforcement
# Running total after this file: 1412 instances (706 base pairs)
# Format: Pair[ID] / A / B / Reason / Phrasing-B

---

## Pair[CB4-3.4-699]

**A**
```
OIDC deployment — token usage review:

The SPA calls the OIDC userinfo endpoint, receives an ID token. When making calls
to the resource API server, the SPA sends the ID token in the Authorization: Bearer
header instead of an access token.
Developer: "The ID token contains the user's identity — sub, name, email, and roles.
The resource server validates the ID token signature, extracts the roles claim, and
uses it for authorization. This eliminates the need for a separate access token."
```

**B**
```
REPROCESS

Node 1 — Domain split: T-oauth-oidc (OIDC Core) + T-rfc-oauth2 (RFC 6749)
Node 2 — Translation: ID token used for API authorization — sub-phase 3.4 in scope
Node 3 — Plan: check OIDC Core §2 purpose of ID token; check audience binding
Node 4 — Processing:
  OIDC Core §2: ID token is an assertion about the authentication event
    Purpose: convey that the user was authenticated by the IdP; intended for the CLIENT
    aud claim: ID token aud = the OAuth 2.0 client_id that requested authentication
    The ID token is NOT designed for API authorization; it is not bound to a resource server
  Using ID token at resource server:
    (1) aud mismatch: ID token aud = client_id; resource server's identifier is different
        RS validates sig but aud does not match RS → token confusion / missing audience check
    (2) No scope: ID tokens do not carry OAuth 2.0 scopes for resource authorization
        "Roles in claims" is not equivalent to access token scope — different authorization model
    (3) No revocation pathway: ID tokens are not subject to RFC 7009 revocation
        If user's access should be revoked, the ID token cannot be revoked at the RS
    (4) ID token lifetime may differ from resource access policy requirements
  Developer claim: "eliminates need for access token" → the claim conflates two separate flows
    ID token = authentication assertion for the client
    Access token = authorization credential for the resource server
    These are distinct and should not be conflated
  ASVS V10.5.3 (paraphrased): ID tokens MUST NOT be used to authenticate to APIs
    Access tokens are the correct credential for API resource access
Node 5 — Application: REPROCESS (ASVS V10.5.3 + OIDC Core §2 misuse of ID token)
Node 6 — Translation: ID token used for API authorization — fundamental OIDC misuse
Node 7 — Arbiter: T-oauth-oidc — ID token purpose violation
Node 8 — Arbiter verdict:
  REPROCESS — ID tokens are authentication assertions intended for the OIDC client, not
  authorization credentials for resource servers. Using an ID token at a resource server
  violates OIDC Core §2: the aud claim is the client_id, not the RS identifier; ID tokens
  carry no scopes; they cannot be revoked via RFC 7009. The "eliminates access token" claim
  conflates authentication (ID token) with authorization (access token). Remediate: use
  the access token (issued by the authorization server) for API resource authorization.
  Issue an OIDC authorization code flow with appropriate scopes; use the resulting access
  token at the resource server.
Node 9 — Format: REPROCESS; OIDC Core §2 + ASVS V10.5.3; "eliminates access token" rejected
```

**Reason**
Using ID tokens for API authorization is a fundamental OIDC misuse: the ID token aud is the client_id (not the RS), ID tokens carry no OAuth scopes, and they cannot be revoked. ASVS V10.5.3 explicitly prohibits using ID tokens to authenticate to APIs. This is a clear REPROCESS.

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-oauth-oidc / T-rfc-oauth2
Node 4 — Processing:
  ID token: aud = client_id; not bound to RS; no scopes; not revocable via RFC 7009
  "Eliminates access token" → conflates authentication assertion with authorization credential
  OIDC Core §2 + ASVS V10.5.3: ID token NOT for API authorization
Node 8 — Arbiter verdict:
  REPROCESS — ID token misused as API authorization credential. Use access token at RS.
  ID token aud mismatch, no scopes, no revocation pathway.
Node 9 — Format: REPROCESS; OIDC Core §2 + V10.5.3 cited
```

---

## Pair[CB4-3.4-700]

**A**
```
OIDC token usage review — correct:

SPA receives three tokens from the token endpoint after authorization code exchange:
  - ID token: validated (sig, iss, aud=client_id, exp, nonce). Stored in memory.
    Used ONLY for: (a) extracting user display info (name, email) for UI; (b) passing
    id_token_hint to prompt=none requests.
  - Access token: sent in Authorization: Bearer header for all resource API calls.
    Resource server validates signature, iss, exp, aud=resource-server-id, scope.
  - Refresh token: stored in memory (not persisted); used only to obtain new access tokens.
```

**B**
```
PASS

Node 1 — Domain split: T-oauth-oidc + T-rfc-oauth2
Node 2 — Translation: OIDC token usage — correct roles for each token type
Node 3 — Plan: verify each token used for its intended purpose
Node 4 — Processing:
  ID token: memory storage + UI display only + id_token_hint → OIDC Core §2 correct use → PASS
    NOT used for API authorization → V10.5.3 compliance → PASS
    Validated: sig + iss + aud=client_id + exp + nonce → complete ID token validation → PASS
  Access token: Bearer header for API calls; RS validates aud=resource-server-id + scope → PASS
    RS aud validation prevents token confusion attack → PASS
  Refresh token: memory storage (not persisted) → PASS (XSS cannot exfiltrate from memory across page reload)
  Correct separation: ID token for authentication/display; access token for authorization; refresh for renewal
Node 5 — Application: PASS (all three token types used correctly)
Node 6 — Translation: OIDC token usage correct; no cross-purpose misuse
Node 7 — Arbiter: T-oauth-oidc — correct token role separation
Node 8 — Arbiter verdict:
  PASS — All three token types used for their intended purposes per OIDC Core. ID token:
  memory storage, UI display and id_token_hint only — not used for API authorization.
  Access token: Bearer header with RS aud validation. Refresh token: in-memory (not persisted).
  Correct separation of authentication assertion (ID token) and authorization credential
  (access token). No findings.
Node 9 — Format: PASS; three token types confirmed; OIDC Core §2 + V10.5.3 satisfied
```

**Reason**
Correct OIDC token usage: ID token for display and id_token_hint only, access token for API authorization with RS aud validation, refresh token in memory. All three are used for their intended purposes. Pairs with CB4-3.4-699 as the PASS baseline.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-oauth-oidc / T-rfc-oauth2
Node 4 — Processing:
  ID token: UI only + id_token_hint; NOT for API auth → OIDC Core §2 + V10.5.3 → PASS
  Access token: Bearer + RS aud validation → PASS
  Refresh token: in-memory → PASS
Node 8 — Arbiter verdict:
  PASS — Correct token role separation. No ID token misuse. RS aud validation present. No findings.
Node 9 — Format: PASS; three token types correct
```

---

## Pair[CB4-3.4-701]

**A**
```
Refresh token lifecycle review:

Implementation: refresh tokens issued without an absolute maximum lifetime.
Rotation is implemented: each use of a refresh token issues a new one; old one invalidated.
Family revocation implemented: replay of a revoked refresh token revokes the entire family.
Developer: "Refresh tokens rotate on every use and family revocation protects against replay.
There's no need for an absolute lifetime — as long as the user keeps using the app,
their session continues. Forcing a re-login after N days would hurt UX."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (BCP §4.14) + T-asvs-session (V7.2.4)
Node 2 — Translation: refresh token absolute lifetime — sub-phase 3.4 in scope
Node 3 — Plan: check BCP refresh token guidance; V7.2.4 idle/absolute timeout; developer argument
Node 4 — Processing:
  Refresh token rotation present: each use rotates; old token invalidated → PASS (BCP §4.14.1)
  Family revocation present: replay triggers family revocation → PASS (BCP §4.14.1)
  Absolute max lifetime absent:
    BCP §4.14 recommends that refresh tokens have a defined expiration
    OAuth 2.0 Security BCP §4.14.1: authorization server SHOULD apply an absolute expiration
    V7.2.4 (idle timeout + absolute timeout): session-equivalent tokens should have both
    Indefinite refresh token with rotation: a user could maintain a session indefinitely
    as long as they use the app at any frequency (one use per N days keeps it alive)
    Legitimate concern: indefinite sessions are a long-term security risk:
      stolen refresh token (even if rotated) provides indefinite access if attacker uses it
      regularly before the legitimate user does; family revocation triggers only on replay
      if attacker uses token FIRST, legitimate user sees a revoked token but attacker
      holds the current one → attacker has indefinite access if they remain active
  Developer claim: "rotation + family revocation are sufficient" → partially true
    These mechanisms protect against single-theft scenarios where attacker has one token
    But: do not protect against an attacker who actively rotates their stolen token
    Absolute max lifetime forces re-authentication regardless of rotation state
  SHOULD level in BCP → CONCERN not REPROCESS (not a MUST violation)
Node 5 — Application: CONCERN (no absolute max lifetime; SHOULD-level gap)
Node 6 — Translation: CONCERN — rotation + family revocation correct; absolute lifetime missing
Node 7 — Arbiter: T-rfc-oauth2 — refresh token lifetime
Node 8 — Arbiter verdict:
  CONCERN — Refresh token rotation and family revocation are correctly implemented (BCP §4.14.1).
  The gap is the absence of an absolute maximum lifetime. BCP §4.14 recommends (SHOULD) that
  authorization servers apply an absolute expiration to refresh tokens. Without it, an attacker
  who actively uses a stolen refresh token can maintain indefinite access: family revocation
  only triggers on replay (where the same token is used twice), not on an attacker who uses
  the current token before the legitimate user. Implement an absolute max lifetime (e.g., 30
  or 90 days) requiring re-authentication regardless of rotation activity.
Node 9 — Format: CONCERN; rotation + family revocation PASS; absolute lifetime absent
```

**Reason**
Rotation + family revocation are correct (ext21 PASS baseline) but absolute max lifetime is absent. BCP §4.14 SHOULD (not MUST) → CONCERN. The developer's argument correctly identifies that rotation + family revocation protect against single-theft replay; the gap is the case where an attacker actively uses their stolen token before the legitimate user, maintaining indefinite access. CONCERN because SHOULD-level.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (BCP §4.14) / T-asvs-session (V7.2.4)
Node 4 — Processing:
  Rotation + family revocation → BCP §4.14.1 PASS
  No absolute max lifetime → SHOULD gap → CONCERN
  "Rotation sufficient" → only for replay; active attacker using current token = indefinite access
  BCP §4.14 SHOULD (not MUST) → CONCERN not REPROCESS
Node 8 — Arbiter verdict:
  CONCERN — Rotation and family revocation correct. No absolute max lifetime is a
  SHOULD-level gap (BCP §4.14). Add absolute max lifetime (e.g., 30-90 days).
Node 9 — Format: CONCERN; rotation PASS; absolute lifetime gap
```

---

## Pair[CB4-3.4-702]

**A**
```
Refresh token lifecycle — correct:

Implementation:
  - Rotation: each use issues new token; old immediately invalidated.
  - Family revocation: replay of any revoked token in a family → entire family revoked.
  - Absolute max lifetime: 90 days from initial issuance. After 90 days, re-authentication
    required regardless of rotation activity.
  - Sliding idle timeout: if no refresh token use in 30 days, token family expires.
  - Access token TTL: 15 minutes.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (BCP §4.14) + T-asvs-session (V7.2.4)
Node 2 — Translation: refresh token lifecycle — sub-phase 3.4 in scope
Node 3 — Plan: verify rotation, family revocation, absolute lifetime, idle timeout, access TTL
Node 4 — Processing:
  Rotation: each use rotates; old invalidated → BCP §4.14.1 PASS
  Family revocation: replay → family revoked → BCP §4.14.1 PASS
  Absolute max lifetime: 90 days from issuance → BCP §4.14 SHOULD satisfied → PASS
    Forces re-authentication after 90 days regardless of rotation → prevents indefinite access
  Idle timeout: 30-day sliding → V7.2.4 idle timeout satisfied → PASS
  Access token TTL: 15 minutes → bounded; rotation + short TTL defense in depth → PASS
  All lifecycle controls present and correctly configured
Node 5 — Application: PASS (BCP §4.14 + V7.2.4 all controls)
Node 6 — Translation: refresh token lifecycle correct and complete
Node 7 — Arbiter: T-rfc-oauth2 — refresh token PASS
Node 8 — Arbiter verdict:
  PASS — Refresh token lifecycle fully implemented. Rotation per use with immediate
  invalidation. Family revocation on replay. 90-day absolute max lifetime forcing
  re-authentication. 30-day sliding idle timeout. 15-minute access token TTL.
  BCP §4.14 (rotation + family revocation + absolute lifetime) and V7.2.4 (idle +
  absolute timeout) all satisfied. No findings.
Node 9 — Format: PASS; BCP §4.14 + V7.2.4 all controls confirmed
```

**Reason**
All refresh token lifecycle controls present: rotation, family revocation, absolute max lifetime (90 days), sliding idle timeout (30 days), short access token TTL (15 min). BCP §4.14 SHOULD for absolute lifetime is satisfied. Pairs with CB4-3.4-701.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-oauth2 (BCP §4.14) / T-asvs-session (V7.2.4)
Node 4 — Processing:
  Rotation + family revocation → BCP §4.14.1 → PASS
  90-day absolute max lifetime → BCP §4.14 SHOULD → PASS
  30-day idle timeout → V7.2.4 → PASS
  15-min access token TTL → PASS
Node 8 — Arbiter verdict:
  PASS — All refresh token lifecycle controls present. BCP §4.14 + V7.2.4 satisfied.
Node 9 — Format: PASS; all controls confirmed
```

---

## Pair[CB4-3.4-703]

**A**
```
Refresh token revocation cascade review:

Implementation: when a refresh token is revoked (via RFC 7009 or logout), the
refresh token is invalidated in the database. The associated access tokens remain
valid until their natural expiry (TTL: 60 minutes).
Developer: "Access tokens are opaque to us after issuance — we can't revoke them
unless we implement a token introspection blocklist. We chose not to add that
complexity. Access tokens expire naturally in 60 minutes."
```

**B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 (RFC 7009 + BCP §4.14) + T-asvs-session (V10.3)
Node 2 — Translation: refresh token revocation + access token cascade — sub-phase 3.4
Node 3 — Plan: check RFC 7009 cascade requirement; assess 60-minute access token window
Node 4 — Processing:
  RFC 7009 §2: revocation of a refresh token → implementation MAY also revoke associated
    access tokens, but this is optional (MAY, not MUST)
  V10.3 (token revocation): logout should result in token revocation, but access token
    lifetime is a design choice; shorter TTL is defense in depth
  60-minute access token TTL with no revocation cascade:
    After refresh token revocation (logout, compromise detection), access tokens valid for up to 60 minutes
    This is a meaningful window: if a refresh token is revoked due to compromise detection,
    the attacker's access tokens remain valid for up to 60 minutes
    Correct comparison: 15-minute TTL → CONCERN (short but real); 60-minute TTL → larger CONCERN
  "Can't revoke opaque tokens" → not entirely true: introspection blocklist is standard pattern
    Complexity argument is legitimate but doesn't change the security gap
  Not a REPROCESS: RFC 7009 cascade is MAY; 60-minute TTL is a design choice, not a MUST violation
  60-minute window with no cascade = CONCERN (meaningful post-revocation window)
  Compare: 15-minute TTL = smaller CONCERN; 60-minute TTL = larger CONCERN; same classification
Node 5 — Application: CONCERN (no cascade; 60-minute window)
Node 6 — Translation: CONCERN — spec-permitted; meaningful post-revocation exposure window
Node 7 — Arbiter: T-rfc-oauth2 — revocation cascade design
Node 8 — Arbiter verdict:
  CONCERN — Refresh token revocation without access token cascade leaves a 60-minute window
  during which access tokens issued before revocation remain valid. RFC 7009 §2 makes cascade
  optional (MAY), so this is not a MUST violation. However, a 60-minute post-revocation window
  is meaningful — if a token family is revoked due to compromise, the attacker retains resource
  access for up to 60 minutes. The developer's "complexity" argument is legitimate but does not
  eliminate the risk. Remediate: reduce access token TTL (e.g., to 5-15 minutes) to minimize
  the post-revocation window; or implement an access token blocklist on high-security endpoints.
Node 9 — Format: CONCERN; 60-min window; RFC 7009 MAY (not MUST); TTL reduction recommended
```

**Reason**
RFC 7009 §2 cascade is MAY → CONCERN not REPROCESS. The concern is calibrated by the TTL: 60 minutes is a meaningful post-revocation window. The developer's complexity argument is evaluated and acknowledged as legitimate but insufficient to dismiss the CONCERN. TTL reduction is the practical remediation.

**Phrasing-B**
```
CONCERN

Node 1 — Domain split: T-rfc-oauth2 / T-asvs-session
Node 4 — Processing:
  RFC 7009 §2: cascade is MAY → not REPROCESS
  60-min TTL with no cascade → meaningful post-revocation window → CONCERN
  "Can't revoke opaque tokens" → partially valid; introspection blocklist is standard
  Not a MUST violation → CONCERN
Node 8 — Arbiter verdict:
  CONCERN — 60-minute post-revocation window. RFC 7009 cascade optional.
  Reduce access token TTL to 5-15 min; or add access token blocklist.
Node 9 — Format: CONCERN; RFC 7009 MAY; TTL reduction recommended
```

---

## Pair[CB4-3.4-704]

**A**
```
Resource server JWT validation review:

The resource server validates JWTs presented by clients:
  - Signature verified using the AS's JWKS endpoint
  - iss verified against expected issuer
  - exp and iat checked
  - aud verified against the RS's own identifier
No algorithm allowlist enforced. The RS processes any algorithm that the JWT library supports,
including HS256 and "none". The JWT library supports: RS256, RS384, RS512, HS256, HS384, HS512, none.
```

**B**
```
REPROCESS

Node 1 — Domain split: T-rfc-jwt (RFC 8725 JWT Best Practices) + T-asvs-token (V9.1.1)
Node 2 — Translation: resource server algorithm enforcement — sub-phase 3.4 in scope
Node 3 — Plan: check V9.1.1 algorithm allowlist; check algorithm confusion attacks
Node 4 — Processing:
  Algorithm confusion attack (established in ext10): HS256 with RSA public key as HMAC secret
    If RS accepts HS256: attacker obtains the AS's RSA public key (from JWKS, public)
    Uses it as the HMAC secret to sign an HS256 JWT with arbitrary claims
    RS validates HS256 signature using JWKS-fetched public key as HMAC key → validation passes
    → complete authorization bypass via algorithm confusion
  "none" algorithm: JWT without any signature; if accepted → any unsigned JWT is valid
  V9.1.1: algorithm allowlist MUST be enforced; "none" MUST be rejected; algorithm confusion
    attacks are explicitly identified as a risk requiring allowlist mitigation
  RFC 8725 §3.1: JWTs MUST NOT accept "none" algorithm; all implementations SHOULD
    use allowlists rather than blocklists for algorithm selection
  No allowlist present → REPROCESS (V9.1.1 MUST; RFC 8725 §3.1)
  Processing HS256 and "none" = CRITICAL security gap
Node 5 — Application: REPROCESS (V9.1.1 MUST + algorithm confusion attack surface open)
Node 6 — Translation: REPROCESS — no algorithm allowlist; HS256 and "none" accepted
Node 7 — Arbiter: T-rfc-jwt + T-asvs-token — algorithm enforcement at RS
Node 8 — Arbiter verdict:
  REPROCESS — No JWT algorithm allowlist at the resource server violates V9.1.1 MUST.
  The RS accepts HS256 and "none" — two critical attack vectors:
  (1) Algorithm confusion: attacker uses AS's public RSA key as HMAC secret to forge HS256 tokens.
  (2) "none" attack: unsigned JWT accepted → any claims without signature.
  Implement an algorithm allowlist: ["RS256"] or ["RS256", "PS256"]. Explicitly reject
  all other algorithms before signature verification.
Node 9 — Format: REPROCESS; V9.1.1 MUST + RFC 8725; HS256 confusion + "none" attack
```

**Reason**
No algorithm allowlist at the RS with HS256 and "none" supported is a clear REPROCESS: V9.1.1 MUST requires algorithm allowlist; "none" must be rejected; HS256 with public RSA key enables algorithm confusion bypass. This is the RS-side algorithm enforcement pattern (vs. the AS-side pattern from ext10).

**Phrasing-B**
```
REPROCESS

Node 1 — Domain split: T-rfc-jwt (RFC 8725) / T-asvs-token (V9.1.1)
Node 4 — Processing:
  No allowlist: RS accepts HS256 + "none"
  HS256 confusion: AS public key → HMAC secret → valid HS256 JWT → auth bypass
  "none": unsigned JWT accepted → any claims valid
  V9.1.1 MUST + RFC 8725 §3.1 → REPROCESS
Node 8 — Arbiter verdict:
  REPROCESS — No algorithm allowlist. HS256 confusion + "none" attack both exploitable.
  Implement allowlist ["RS256"] or ["RS256", "PS256"]. Reject all others.
Node 9 — Format: REPROCESS; V9.1.1 + RFC 8725; two attack vectors cited
```

---

## Pair[CB4-3.4-705]

**A**
```
Resource server JWT algorithm enforcement review — correct:

JWT validation at RS:
  1. Before signature verification: alg claim from JWT header extracted.
  2. alg value checked against allowlist: ["RS256", "PS256"].
  3. If alg not in allowlist → 401 Unauthorized; JWT rejected without processing.
  4. "none" → rejected (not in allowlist).
  5. Signature verified using JWKS-fetched public key.
  6. iss, exp, aud, scope validated.
```

**B**
```
PASS

Node 1 — Domain split: T-rfc-jwt (RFC 8725) + T-asvs-token (V9.1.1)
Node 2 — Translation: RS JWT algorithm allowlist enforcement — sub-phase 3.4 in scope
Node 3 — Plan: verify allowlist check is before signature; check algorithm selection; check "none" rejection
Node 4 — Processing:
  alg extracted before signature verification → allowlist check first → correct order → PASS
    (If alg check is AFTER signature: "none" or HS256 would be processed first — critical)
    Correct: check alg → only then call signature verification function
  Allowlist ["RS256", "PS256"]:
    RS256: RSA PKCS#1 v1.5 with SHA-256 → standard; RFC 7518 §3.3 →PASS
    PS256: RSA-PSS with SHA-256 → stronger signature scheme → PASS
    No HMAC algorithms (HS*) → algorithm confusion attack eliminated → PASS
    "none" not in allowlist → rejected → "none" attack eliminated → PASS
  Steps 5-6: iss/exp/aud/scope validated → complete JWT validation → PASS
  V9.1.1: algorithm allowlist → MUST satisfied → PASS
  RFC 8725 §3.1: allowlist approach → SHOULD satisfied → PASS
Node 5 — Application: PASS (V9.1.1 + RFC 8725 both satisfied)
Node 6 — Translation: RS algorithm enforcement correct; all attack vectors mitigated
Node 7 — Arbiter: T-rfc-jwt + T-asvs-token — RS algorithm enforcement PASS
Node 8 — Arbiter verdict:
  PASS — Resource server JWT algorithm enforcement correctly implemented. Algorithm check
  (against allowlist ["RS256", "PS256"]) performed BEFORE signature verification. "none"
  and HS* algorithms explicitly excluded, eliminating algorithm confusion and "none" attacks.
  Full validation chain: alg → signature → iss, exp, aud, scope. V9.1.1 and RFC 8725 §3.1
  both satisfied. No findings.
Node 9 — Format: PASS; V9.1.1 + RFC 8725 satisfied; algorithm confusion eliminated
```

**Reason**
Correct RS algorithm enforcement: allowlist check BEFORE signature verification, only RS256 and PS256 permitted, "none" explicitly rejected. V9.1.1 MUST satisfied. The key element is the ordering: allowlist check must precede signature verification to prevent algorithm confusion exploits.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-rfc-jwt / T-asvs-token
Node 4 — Processing:
  alg checked before signature verification → correct order → PASS
  Allowlist ["RS256", "PS256"]: no HMAC, no "none" → confusion eliminated → PASS
  Full chain: alg → sig → iss/exp/aud/scope → PASS
  V9.1.1 MUST + RFC 8725 §3.1 → PASS
Node 8 — Arbiter verdict:
  PASS — Algorithm allowlist before signature verification. Confusion attacks eliminated.
  Full validation chain. No findings.
Node 9 — Format: PASS; V9.1.1 + RFC 8725 confirmed; ordering correct
```

---

## Pair[CB4-3.4-706]

**A**
```
Comprehensive review — all ext27 patterns correct:

1. Token usage: ID token used only for UI display + id_token_hint. Access token Bearer
   header for all API calls. RS validates sig + iss + exp + aud + scope.
2. Refresh token: rotation on every use; family revocation on replay; absolute max 90 days;
   sliding idle 30 days; access token TTL 15 minutes.
3. Refresh token revocation cascade: on refresh token revocation, access token blocklist
   entry created for each associated active access token (blocklist TTL = remaining access token TTL).
4. RS algorithm enforcement: allowlist ["RS256"] checked before signature; "none" rejected;
   HS* rejected; full validation chain.
```

**B**
```
PASS

Node 1 — Domain split: T-oauth-oidc / T-rfc-oauth2 (BCP §4.14) / T-rfc-oauth2 (RFC 7009) / T-rfc-jwt + T-asvs-token
Node 2 — Translation: all four ext27 patterns in sub-phase 3.4 scope
Node 3 — Plan: verify each pattern against applicable standard
Node 4 — Processing:
  F1: ID token correct use (display + id_token_hint only) + RS full validation → PASS
      V10.5.3: ID token NOT used for API auth → PASS
  F2: Rotation + family revocation + 90-day absolute + 30-day idle + 15-min TTL
      → BCP §4.14 + V7.2.4 all controls → PASS
  F3: Access token blocklist on refresh revocation → cascade implemented → PASS
      Blocklist TTL = remaining access token TTL → efficient; no permanent entries
      RFC 7009 MAY satisfied; stronger than minimum requirement → PASS
  F4: Allowlist ["RS256"] + check before sig + "none" rejected + full chain → PASS
      V9.1.1 MUST + RFC 8725 §3.1 → PASS
Node 5 — Application: PASS × 4
Node 6 — Translation: all four ext27 patterns correct
Node 7 — Arbiter: comprehensive ext27 PASS
Node 8 — Arbiter verdict:
  PASS — All four ext27 patterns correctly implemented. Token usage: ID token for display
  only; access token for API authorization; RS full validation (V10.5.3). Refresh token:
  rotation + family revocation + 90-day absolute + 30-day idle (BCP §4.14 + V7.2.4).
  Revocation cascade: access token blocklist on refresh revocation (TTL-scoped). RS algorithm:
  allowlist ["RS256"] before signature; "none" and HS* rejected (V9.1.1 + RFC 8725). No findings.
Node 9 — Format: PASS; four patterns confirmed
```

**Reason**
Comprehensive PASS for all four ext27 patterns. F3 shows the improved revocation cascade: access token blocklist with TTL = remaining access token lifetime (eliminates both the gap and the complexity of permanent blacklist entries). All patterns verified against applicable standards.

**Phrasing-B**
```
PASS

Node 1 — Domain split: T-oauth-oidc / T-rfc-oauth2 × 2 / T-rfc-jwt + T-asvs-token
Node 4 — Processing:
  F1: ID token display only; RS validates aud + scope → PASS
  F2: rotation + family revoc + 90d absolute + 30d idle + 15min TTL → PASS
  F3: access token blocklist on revocation (TTL-scoped) → cascade PASS
  F4: allowlist ["RS256"] before sig; "none"/"HS*" rejected → PASS
Node 8 — Arbiter verdict:
  PASS — All four ext27 patterns correct. No findings.
Node 9 — Format: PASS; four patterns confirmed
```
