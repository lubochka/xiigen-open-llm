# CB4 Stage 3 — Node Cross-Source Set Extension 10
# Sequences CB4-3.4-455 through CB4-3.4-464
# Stratum: B/C/D — multi-source combinations, cross-territory interactions
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 928 instances

---

## Pair[CB4-3.4-455]

**A:** Combined CORS, PKCE downgrade, and credential review. Token endpoint CORS: wildcard (`*`). PKCE: S256 enforced, plain rejected. Client secrets: stored in HashiCorp Vault. Session: session ID regenerated on login. Argon2id m=65536. Security Champion + OWASP checklist.

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth + SSDF-PW + ASVS-V7-Session + ASVS-V6-Auth

T-rfc-oauth2 / T-oauth-oidc:
  PKCE S256 enforced, plain rejected ✓ → PASS
  Token endpoint CORS wildcard (`*`) ✗ → REPROCESS
    Wildcard CORS on token endpoint creates cross-origin token request risk.
  T-oauth-oidc verdict: REPROCESS

T-asvs-session:
  V7.2.3 session ID regenerated ✓ → PASS

T-ssdf-process:
  PW.1 Vault for client secrets ✓ → PASS

T-asvs-authentication:
  V6.2.5 Argon2id ✓ → PASS

Overall verdict: REPROCESS
(CORS REPROCESS overrides all PASSes; PKCE S256 correct but CORS gap is separate)
```

**Reason:** PKCE is correctly implemented (PASS). Vault, session fixation, Argon2id all pass. The single gap is wildcard CORS on the token endpoint — a direct REPROCESS regardless of the correct PKCE and other controls. One REPROCESS overrides all PASSes.

**Phrasing-B:** PKCE S256 with plain rejected, Vault for secrets, session regenerated, Argon2id — all correct. But token endpoint CORS is wildcard. Does the correct PKCE compensate for the wildcard CORS?

---

## Pair[CB4-3.4-456]

**A:** Implicit flow with credential management. Browser-based SPA uses `response_type=token` (implicit). JWT signing key: 2048-bit RS256, rotated quarterly, stored in Vault. Scope enforced at resource server. Revocation endpoint with client authentication. Introspection with Bearer auth. Argon2id. Security Champion + OWASP checklist.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2 + ASVS-V9-Token + SSDF-PW

T-oauth-oidc:
  response_type=token (implicit) → REPROCESS
    Prohibited by V10 and OAuth 2.0 BCP regardless of other controls.

T-asvs-token:
  RS256 2048-bit quarterly rotation ✓; V9.2 ✓
  Scope enforced ✓; revocation auth ✓; introspection auth ✓ → PASS

T-ssdf-process:
  Vault ✓; Security Champion ✓ → PASS

T-asvs-authentication:
  Argon2id ✓ → PASS

Overall verdict: REPROCESS
(Implicit flow REPROCESS in oauth-oidc territory overrides all PASSes)
```

**Reason:** All token management, credential, and process controls correctly implemented (PASS). The single gap is the implicit flow — prohibited by V10 regardless of how well other controls are implemented. One REPROCESS overrides.

**Phrasing-B:** SPA uses implicit flow. JWT key management, scope enforcement, revocation auth, Vault, Argon2id, Security Champion all correct. Does correct key management and scope enforcement compensate for the implicit flow finding?

---

## Pair[CB4-3.4-457]

**A:** DPoP combined with session and code review. DPoP: fully implemented per RFC 9449 (ephemeral keys, all required claims, jti replay detection, cnf binding). Session: 30-minute idle timeout; session ID regenerated on login; back-channel SSO logout. Code review: Security Champion with OWASP checklist and merge authority. Password hashing: bcrypt cost 12. Token endpoint: HTTPS directly.

**B:**
```
Arbiter: ASVS-V9-Token + ASVS-V7-Session + SSDF-PW + ASVS-V6-Auth + RFC-OAuth2

T-asvs-token:
  DPoP RFC 9449 (all components) ✓ → PASS
  Token endpoint HTTPS directly ✓ (RFC 6749 §3.2) → PASS

T-asvs-session:
  V7.2.4 (30-min idle) ✓
  V7.2.3 (session ID regenerated) ✓
  V7.3.1 (back-channel logout) ✓ → PASS

T-ssdf-process:
  PW.3.2: Security Champion + OWASP checklist + merge authority ✓ → PASS

T-asvs-authentication:
  V6.2.5: bcrypt cost 12 ✓ (meets OWASP minimum cost ≥ 10) → PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** DPoP correctly implemented; session controls all correct; Security Champion with checklist satisfies PW.3.2; bcrypt cost 12 satisfies V6.2.5. Token endpoint on direct HTTPS. All-PASS.

**Phrasing-B:** DPoP fully implemented, 30-min idle + session regeneration + back-channel logout, Security Champion + OWASP checklist, bcrypt cost 12, token endpoint direct HTTPS. Do these controls pass all applicable territories?

---

## Pair[CB4-3.4-458]

**A:** Scope enforcement graduated assessment. Three resource server configurations:
(A) Resource server checks token signature, exp, aud, iss — does NOT check scope.
(B) Resource server checks signature, exp, aud, iss, AND scope claim — rejects requests where the token's scope does not include the required scope for the endpoint.
(C) Resource server checks signature, exp, aud, iss, scope AND DPoP proof with jti replay detection.

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth

Graduated scope enforcement assessment:

Scenario A — No scope checking:
  Scope claim present in token but not validated at resource server.
  Any valid token (any scope) can call any endpoint.
  Finding: REPROCESS — scope constraint on token nullified;
    authorization model broken.

Scenario B — Scope checking enforced:
  Resource server validates that the token's scope covers the requested
  operation before processing.
  Tokens with scope=read cannot call write endpoints ✓
  Scope enforcement: PASS ✓
  RFC 6749 §3.3 authorization model correctly implemented.
  Finding: PASS

Scenario C — Scope checking + DPoP:
  Scope enforced ✓; DPoP proof validated ✓; jti replay detection ✓
  Sender-constraining (DPoP) + scope enforcement together provide:
    - Proof that the correct client is presenting the token (DPoP)
    - Proof that the token is authorized for the operation (scope)
  Finding: PASS — strongest of the three configurations.

Summary:
  A: REPROCESS (no scope check)
  B: PASS (scope enforced)
  C: PASS (scope + DPoP — defense-in-depth)
```

**Reason:** The graduated assessment explicitly establishes: no scope check = REPROCESS; scope checked = PASS; scope + DPoP = PASS (defense-in-depth). Scope enforcement is independent of DPoP — both can coexist and each provides independent protection.

**Phrasing-B:** Three resource server configs: (A) no scope check; (B) scope enforced; (C) scope enforced + DPoP with replay detection. Is A a REPROCESS and B/C both PASS? What does DPoP add over scope-only?

---

## Pair[CB4-3.4-459]

**A:** Combined response_type restriction and PKCE. Authorization server: response_type=code only (implicit rejected). PKCE S256 enforced. However: the authorization server does not validate the `redirect_uri` at token exchange — at the authorization request, a redirect_uri is accepted; at token exchange, no redirect_uri check is performed. Team: "We validate redirect_uri at authorization time. The code is single-use — re-validating at exchange is redundant."

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2

V10.2.1 (redirect_uri validation): REPROCESS
RFC 6749 §4.1.3 requires that if a redirect_uri was included in the
authorization request, the same redirect_uri MUST be presented and verified
at the token exchange request.

Team argument ("validation at authorization time is sufficient"): rejected.
(1) RFC 6749 §4.1.3: "ensure that the 'redirect_uri' parameter is present
    if the 'redirect_uri' parameter was included in the initial authorization
    request as described in Section 4.1.1, and if included ensure that their
    values are identical."
(2) The redirect_uri check at token exchange is a MUST in RFC 6749 §4.1.3.
(3) Without exchange-time validation: an authorization code intercepted and
    submitted with a different redirect_uri by an attacker will be accepted.
    The single-use nature of the code prevents the legitimate client from
    using it, but the attacker can exchange it for tokens.

response_type=code only ✓ — PASS
PKCE S256 ✓ — PASS
redirect_uri not validated at exchange ✗ → REPROCESS

Evidence gap: redirect_uri not verified at token exchange.
Overall verdict: REPROCESS
```

**Reason:** RFC 6749 §4.1.3 requires redirect_uri verification at token exchange if it was included in the authorization request — this is a MUST. Validating only at authorization time is insufficient: an attacker who intercepts the code can present it at token exchange with a different redirect_uri. Response_type restriction and PKCE are correct but do not compensate for this gap.

**Phrasing-B:** Code flow only, PKCE S256, redirect_uri validated at authorization time but not re-validated at token exchange. RFC 6749 — is redirect_uri validation at exchange time required, or is authorization-time validation sufficient?

---

## Pair[CB4-3.4-460]

**A:** redirect_uri token exchange validation — correctly implemented. Authorization server: at token exchange (`POST /oauth/token`), if the authorization request included a `redirect_uri`, the token request must include the identical `redirect_uri` value. Mismatch → `error=invalid_grant`. response_type=code only. PKCE S256. Exact redirect URI registration. Back-channel SSO logout. Argon2id. Vault. Security Champion.

**B:**
```
Arbiter: ASVS-V10-OAuth + RFC-OAuth2 + Multiple

T-oauth-oidc:
  V10.2.1: exact redirect URIs registered ✓
  RFC 6749 §4.1.3: redirect_uri verified at token exchange ✓
    (identical value required; mismatch = invalid_grant) ✓
  response_type=code only ✓
  V10.4.6 PKCE S256 ✓ → PASS

T-asvs-session: V7.3.1 back-channel ✓ → PASS
T-asvs-authentication: V6.2.5 Argon2id ✓ → PASS
T-ssdf-process: Vault ✓; Security Champion ✓ → PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** redirect_uri verified at both authorization time and token exchange — satisfies both V10.2.1 (exact registration) and RFC 6749 §4.1.3 (exchange-time verification). All other controls correct. Comprehensive PASS.

**Phrasing-B:** Exact redirect URIs, redirect_uri verified again at token exchange (mismatch = invalid_grant), code flow only, PKCE S256, back-channel logout, Argon2id, Vault, Security Champion. Do these controls pass all territories?

---

## Pair[CB4-3.4-461]

**A:** Three-territory review with two correct patterns and one gap. (1) Token endpoint: HTTPS directly; CORS restricted to first-party origin; response_type=code only; PKCE S256; redirect_uri verified at exchange. (2) Session: 30-min idle timeout; session ID regenerated on login; back-channel SSO logout. (3) Password hashing: MD5(password + static salt). Team: "We salt the MD5 hash — salted MD5 is different from unsalted MD5. The salt prevents rainbow table attacks."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session + ASVS-V6-Auth

T-oauth-oidc:
  All controls described: HTTPS ✓; CORS restricted ✓; code-only ✓;
  PKCE S256 ✓; redirect_uri at exchange ✓ → PASS

T-asvs-session:
  V7.2.4 ✓; V7.2.3 ✓; V7.3.1 ✓ → PASS

T-asvs-authentication:
  V6.2.5: MD5(password + static salt) → REPROCESS
    MD5 is not a memory-hard password hashing algorithm.
    Team argument ("salted MD5 prevents rainbow tables"): partially correct
    but misses the primary concern.
    (1) A unique salt per password prevents rainbow table attacks ✓
    (2) A STATIC salt (same salt for all passwords) does NOT prevent rainbow
        tables — all passwords with the same static salt produce the same
        MD5 result for the same password.
    (3) Even with a unique salt per password, MD5 is designed for speed —
        billions of MD5 hashes per second on GPU hardware. Salted MD5 can
        be cracked by brute force in minutes for typical passwords.
    (4) V6.2.5 requires a memory-hard algorithm (bcrypt, Argon2id, scrypt) —
        specifically to resist GPU-accelerated brute force attacks.
    MD5 with static salt: doubly non-compliant (wrong algorithm + ineffective salt)
    Finding: REPROCESS

T-asvs-authentication verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** OAuth and session controls correctly implemented (PASS for both territories). MD5 with a static salt fails V6.2.5 doubly: (a) MD5 is not memory-hard; (b) a static salt doesn't prevent rainbow tables (only unique per-password salts do). The salt argument is partially correct in principle but wrong in implementation and irrelevant to the memory-hardness requirement. One REPROCESS overrides.

**Phrasing-B:** OAuth and session controls all correct. Password hashing uses MD5 with a static salt. We argue the salt prevents rainbow tables. Is salted MD5 sufficient for V6.2.5, and does a static salt behave differently from a unique per-password salt?

---

## Pair[CB4-3.4-462]

**A:** Post-remediation — all gaps from ext10 cross-source and ext11 pipeline patterns resolved. Prior findings addressed: (1) wildcard CORS on token endpoint → restricted to first-party origin; (2) implicit flow → migrated to response_type=code + PKCE S256; (3) scope non-enforcement → scope claim validated on all resource server requests; (4) redirect_uri not verified at exchange → now verified; (5) MD5 passwords → migrated to Argon2id m=65536, t=3, p=4; (6) unauthenticated revocation → client credentials now required. All six remediations verified.

**B:**
```
Arbiter: Multiple

(1) Token endpoint CORS: restricted to https://app.example.com ✓ — PASS
(2) response_type=code + PKCE S256 ✓ — PASS; implicit flow removed ✓
(3) Scope enforced at resource server ✓ — PASS
(4) redirect_uri verified at token exchange ✓ (RFC 6749 §4.1.3) — PASS
(5) Argon2id m=65536/t=3/p=4 ✓ (migration from MD5 complete) — PASS
    Note: password migration requires re-hashing — lazy re-hash on next login
    or forced password reset both acceptable; must not store MD5 hashes
    after migration window.
(6) Revocation endpoint: client credentials required ✓ (RFC 7009 §2.1) — PASS

All six remediations: PASS
Note: MD5 migration window must be managed — old MD5 hashes must be retired.
Overall verdict: PASS
```

**Reason:** All six remediations verified. Key note on MD5 migration: old MD5 hashes must be retired (not just new logins re-hashed) — a lazy re-hash approach or forced password reset ensures the migration window closes. Post-remediation PASS.

**Phrasing-B:** Six prior findings resolved: CORS restricted, migrated to code+PKCE, scope enforced, redirect_uri at exchange, migrated from MD5 to Argon2id, revocation with client auth. Does post-remediation yield an overall PASS?

---

## Pair[CB4-3.4-463]

**A:** Comprehensive review — new patterns from ext17 plus redirect_uri exchange validation. Application: all prior controls passing (exact URIs, PKCE S256, state, token revocation, back-channel SSO logout, Argon2id, /me/permissions, RS256 2048-bit rotated, introspection Bearer auth, Vault, Security Champion + checklist, aud/iss validated, scope enforced, CORS restricted, code-flow only, plain PKCE rejected, revocation with client auth). Plus: redirect_uri verified at token exchange; DPoP fully implemented with jti replay detection.

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  V10.2.1 exact URIs ✓; V10.4.6 S256 no-plain ✓; V10.2.6 state ✓; V10.3 ✓
  response_type=code only ✓; CORS restricted ✓
  RFC 6749 §4.1.3 redirect_uri at exchange ✓ → PASS

T-rfc-oauth2:
  RFC 6749 §3.2 HTTPS ✓; RFC 7009 §2.1 revocation ✓
  RFC 7662 §2.1 introspection ✓; RFC 6749 §3.3 scope enforced ✓ → PASS

T-asvs-token:
  V9.1.1 RS256 ✓; RFC 7518 §3.3 2048-bit ✓; V9.2 rotation ✓; V10.5.1 aud ✓
  DPoP RFC 9449 (all + jti replay) ✓ → PASS

T-asvs-session: V7.2.3 ✓; V7.3.1 ✓ → PASS
T-asvs-authentication: V6.2.5 Argon2id ✓ → PASS
T-asvs-authorization: V8.2.1 ✓; V8.1.2 ✓ → PASS
T-ssdf-process: PW.1 Vault ✓; PW.3.2 Security Champion ✓ → PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** Full implementation of all accumulated controls including the newest patterns: redirect_uri at exchange (RFC 6749 §4.1.3), DPoP with replay detection, scope enforcement, CORS restricted, code-only, no plain PKCE. Seven-territory comprehensive PASS.

**Phrasing-B:** All previous controls plus redirect_uri verified at exchange and DPoP with jti replay detection. Does this comprehensive implementation pass all territories including the newest ext17 and ext10 patterns?

---

## Pair[CB4-3.4-464]

**A:** Mixed scenario — three CONCERNs and no REPROCESS. Application: RS256 2048-bit (PASS); JWT signing key not rotated in 18 months (CONCERN); refresh tokens 60-day expiry, no single-use rotation (CONCERN); nbf = iat - 25 minutes for "service propagation latency" (CONCERN); token endpoint HTTPS directly (PASS); PKCE S256 (PASS); scope enforced (PASS); revocation with client auth (PASS); Vault (PASS); session ID regenerated on login (PASS); Argon2id (PASS).

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT + ASVS-V10-OAuth

T-asvs-token:
  V9.1.1 RS256 ✓ — PASS
  RFC 7518 §3.3 2048-bit ✓ — PASS
  V9.2 rotation: 18 months, no schedule → CONCERN
  nbf = iat - 25 min: disproportionate for clock skew (same as 30-min case)
    "Service propagation latency" is a deployment concern, not clock skew.
    25-minute backward nbf extends effective validity window significantly.
    → CONCERN
  T-asvs-token: CONCERN

T-oauth-oidc:
  PKCE S256 ✓; scope enforced ✓; revocation ✓; token endpoint TLS ✓
  Refresh token rotation absent: 60-day expiry, no single-use rotation → CONCERN
    Logout revocation exists (assumed from context of full-featured app).
    No rotation = no theft detection between login and logout.
    V10.3 baseline (logout revocation) satisfied; rotation is defense-in-depth.
    → CONCERN
  T-oauth-oidc: CONCERN

All other territories: PASS
(V9.2 CONCERN + nbf CONCERN + rotation CONCERN — three CONCERNs, no REPROCESS)
Overall verdict: CONCERN

Note: three CONCERNs in two territories both yield CONCERN; they do not
combine to produce a REPROCESS. Verdict graduation:
  1+ REPROCESS anywhere = REPROCESS overall
  1+ CONCERN, 0 REPROCESS = CONCERN overall
  All PASS = PASS
```

**Reason:** Three separate CONCERNs: key rotation absent (V9.2), nbf 25-minute backward offset (same as 30-minute — deployment concern not clock skew), and refresh token rotation absent (defense-in-depth gap). All core requirements are met — no MUST violations. Three CONCERNs do not combine to produce a REPROCESS; the overall verdict is CONCERN. This pair explicitly teaches the verdict graduation rule.

**Phrasing-B:** Three concerns: signing key not rotated in 18 months, nbf = iat - 25 minutes, no refresh token rotation. No REPROCESS findings. Do three CONCERNs combine to produce a REPROCESS, or is the overall verdict CONCERN?

---

## File Summary

| Pair ID | Description | Stratum | Arbiters | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-455 | PKCE PASS + Vault PASS + wildcard CORS REPROCESS → REPROCESS overall | C | RFC-OAuth2 + V10-OAuth | REPROCESS |
| CB4-3.4-456 | Implicit flow REPROCESS + all token/process controls PASS → REPROCESS | C | V10-OAuth + V9-Token + SSDF-PW | REPROCESS |
| CB4-3.4-457 | DPoP + session + PW.3.2 + bcrypt 12 → all PASS | C | V9-Token + V7-Session + SSDF-PW + V6-Auth | PASS |
| CB4-3.4-458 | Scope enforcement graduated: no check=REPROCESS; scope=PASS; scope+DPoP=PASS | D | RFC-OAuth2 + V10-OAuth | Mixed |
| CB4-3.4-459 | Code-only + PKCE PASS + redirect_uri not verified at exchange → REPROCESS (RFC 6749 §4.1.3) | C | V10-OAuth + RFC-OAuth2 | REPROCESS |
| CB4-3.4-460 | redirect_uri verified at exchange + all other controls → PASS | B | Multiple | PASS |
| CB4-3.4-461 | OAuth + session PASS + MD5 static-salt REPROCESS (wrong algo + ineffective salt) | C | V10-OAuth + V7-Session + V6-Auth | REPROCESS |
| CB4-3.4-462 | Six prior findings remediated (incl. MD5→Argon2id migration) → PASS | B | Multiple | PASS |
| CB4-3.4-463 | All accumulated controls + redirect_uri at exchange + DPoP → 7-territory PASS | B | Multiple | PASS |
| CB4-3.4-464 | Three CONCERNs (key rotation + nbf 25-min + no refresh rotation) → CONCERN (not REPROCESS) | C/D | V9-Token + V10-OAuth | CONCERN |
