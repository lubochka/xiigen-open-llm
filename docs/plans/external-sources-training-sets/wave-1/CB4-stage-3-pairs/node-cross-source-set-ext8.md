# CB4 Stage 3 — Node Cross-Source Set Extension 8
# Sequences CB4-3.4-403 through CB4-3.4-412
# Stratum: B/C/D — multi-source combinations, cross-territory interactions
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 824 instances

---

## Pair[CB4-3.4-403]

**A:** Combined introspection and credential management review. Token introspection endpoint: no authentication required, internal network only. JWT signing key: stored in HashiCorp Vault, rotated quarterly with kid versioning. RS256 allowlist. Exact redirect URIs. PKCE S256. State validated.

**B:**
```
Arbiter: RFC-OAuth2 + ASVS-V10-OAuth + SSDF-PW + ASVS-V9-Token

T-rfc-oauth2:
  RFC 7662 §2.1 (introspection authentication): REPROCESS
    Internal network restriction does not satisfy RFC 7662 §2.1 MUST.
    Application-layer authorization required.
    Argument rejected: network perimeter ≠ identity-based authorization per §2.1.

T-oauth-oidc:
  V10.2.1, V10.4.6, V10.2.6: not fully described but positive indicators ✓ (context)
  Token introspection gap captured under T-rfc-oauth2.

T-ssdf-process:
  PW.1 (Vault for JWT signing key): PASS ✓

T-asvs-token:
  V9.1.1 (RS256 allowlist): PASS ✓
  V9.2 (quarterly key rotation + kid): PASS ✓

T-rfc-oauth2 verdict: REPROCESS
Overall verdict: REPROCESS (REPROCESS overrides all PASSes)
```

**Reason:** JWT key management (Vault + quarterly rotation) correctly satisfies PW.1 and V9.2 (PASS). RS256 allowlist correct (PASS). The introspection endpoint without application-layer authentication violates RFC 7662 §2.1 (REPROCESS). One REPROCESS overrides the correct key management controls.

**Phrasing-B:** Vault for JWT key with quarterly rotation, RS256 allowlist, exact URIs, PKCE S256 — all correct. But introspection endpoint is open on internal network only. Does the correct key management compensate for the open introspection endpoint, or is RFC 7662 §2.1 a REPROCESS regardless?

---

## Pair[CB4-3.4-404]

**A:** Combined MFA and session review. ASVS Level 2 healthcare application. MFA: TOTP enforced for all logins without exception (every authentication requires TOTP). Session: 15-minute idle timeout (strict, given healthcare data sensitivity); absolute 4-hour session limit; logout immediately invalidates server-side session and calls OIDC back-channel logout. Password hashing: Argon2id m=65536, t=3, p=4.

**B:**
```
Arbiter: ASVS-V6-Auth + ASVS-V7-Session

T-asvs-authentication:
  V6.3 (MFA at Level 2 healthcare): PASS
    TOTP enforced for all logins without exception ✓
    No single-factor authentication path exists
  V6.2.5 (password hashing): Argon2id m=65536/t=3/p=4 ✓ — PASS
  T-asvs-authentication verdict: PASS

T-asvs-session:
  V7.2.4 (idle timeout): 15-minute idle timeout ✓ — meets and exceeds requirement
  Absolute session limit (4 hours): defense-in-depth ✓
  V7.3.1 (logout): immediate server-side invalidation + OIDC back-channel ✓ — PASS
  T-asvs-session verdict: PASS

Overall verdict: PASS
```

**Reason:** MFA enforced for all logins without exception satisfies V6.3 at Level 2 healthcare context. Short idle timeout and back-channel SSO logout satisfy V7.2.4 and V7.3.1. Argon2id correct. All-PASS.

**Phrasing-B:** Healthcare ASVS Level 2 app: TOTP required for all logins, 15-min idle, 4-hour absolute limit, back-channel SSO logout, Argon2id m=65536. Do all-login mandatory MFA and strict session controls satisfy Level 2 requirements?

---

## Pair[CB4-3.4-405]

**A:** Refresh token review with combined session. OAuth application. Refresh tokens: 90-day expiry, no single-use rotation. On logout: access token revoked; refresh token expires naturally (not revoked on logout). Session: 30-minute idle timeout; application session cleared on logout; no SSO back-channel logout (standalone application, no SSO). Argon2id m=65536. RS256 allowlist. Exact redirect URIs.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session

T-oauth-oidc:
  V10.2.1: exact URIs ✓ — PASS
  V10.3 (token revocation on logout):
    Access token revoked ✓
    Refresh token NOT revoked on logout ✗ — 90-day expiry survives logout
    V10.3 verdict: REPROCESS

T-asvs-session:
  V7.2.4 (30-min idle): PASS ✓
  V7.3.1 (logout): standalone application (no SSO); application session cleared ✓
    For a non-SSO application, no back-channel logout is required.
    Application session cleared on logout satisfies V7.3.1 in standalone context.
  T-asvs-session verdict: PASS

T-asvs-authentication:
  V6.2.5: Argon2id ✓ — PASS (described but not primary territory)

T-asvs-token: V9.1.1 RS256 allowlist ✓ — PASS

Overall verdict: REPROCESS
(V10.3 refresh token survival REPROCESS overrides all other PASSes)
```

**Reason:** In a standalone non-SSO application, no OIDC back-channel logout is required — V7.3.1 is satisfied by clearing the application session. The failing control is V10.3: 90-day refresh tokens that survive logout enable continued token acquisition post-logout. REPROCESS for the refresh token gap; PASS for session in standalone context.

**Phrasing-B:** Standalone app (no SSO). Access token revoked on logout, refresh tokens not (90-day expiry). 30-min idle, app session cleared on logout. Does the standalone context (no SSO) affect V7.3.1, and is the refresh token survival a REPROCESS?

---

## Pair[CB4-3.4-406]

**A:** nbf claim combined with token allowlist review. JWT service: RS256 allowlist (correct). `nbf` claim set to `iat - 5` seconds (5-second backward clock skew allowance). `exp` claim set to `iat + 900` (15-minute access token). `aud` claim validated. The `nbf` claim is validated by the JWT library — tokens with nbf in the future are rejected. Key rotation quarterly.

**B:**
```
Arbiter: RFC-JWT + ASVS-V9-Token

T-rfc-jwt:
  RFC 7519 §4.1.5 (nbf = iat - 5): PASS
    5-second backward offset is standard clock skew accommodation ✓
    nbf validated by library ✓ — future nbf correctly rejected
  T-rfc-jwt verdict: PASS

T-asvs-token:
  V9.1.1 (RS256 allowlist): PASS ✓
  V10.5.1 (aud validated): PASS ✓
  V9.2 (quarterly key rotation): PASS ✓
  Token expiry (15 minutes): appropriate for access tokens ✓
  V9.3.1 (no token logging): not explicitly described; not evaluated
  T-asvs-token verdict: PASS (all described controls)

Overall verdict: PASS
```

**Reason:** 5-second backward nbf is the accepted clock skew pattern (PASS). RS256 allowlist, aud validation, quarterly key rotation all correct (PASS). All-PASS.

**Phrasing-B:** RS256 allowlist, nbf = iat - 5 (5-sec skew), exp = iat + 900 (15 min), aud validated, quarterly key rotation. Does this token configuration pass all applicable requirements?

---

## Pair[CB4-3.4-407]

**A:** Algorithm and key management combined edge case. JWT service RS256. Signing key is 1024-bit RSA — generated when the application was first deployed and not rotated. The team states: "RS256 is on the ASVS allowlist. A 1024-bit RSA key is technically RSA — it's the same algorithm. RFC 7518 recommends 2048+ bits but 1024 bits are still in use."

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT

T-asvs-token:
  V9.1.1 (algorithm allowlist — RS256): PASS
    RS256 is an approved algorithm; the allowlist configuration is correct ✓

  Key size — 1024-bit RSA: REPROCESS
    V9.1.x / general cryptographic hygiene: 1024-bit RSA is considered weak.
    NIST has deprecated 1024-bit RSA keys (NIST SP 800-131A) — they no longer
    provide adequate security margins against modern factoring attacks.
    RFC 7518 §3.3: "A key of size 2048 bits or larger MUST be used with [RS256]."
    A 1024-bit key with RS256 violates RFC 7518 §3.3's MUST.
    Team argument ("same algorithm"): rejected — the algorithm identifier is RS256
    but the key size is a separate MUST requirement within the same specification.
    RS256 with a 1024-bit key is not a compliant RS256 configuration.

T-rfc-jwt:
  RFC 7518 §3.3 (key size MUST ≥ 2048 bits for RS256): REPROCESS
    1024-bit key violates the MUST requirement.

T-asvs-token verdict: REPROCESS (key size violation)
T-rfc-jwt verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** The algorithm allowlist (RS256) is correctly configured. However, RFC 7518 §3.3 imposes a MUST on key size: RSA keys used with RS256 MUST be ≥ 2048 bits. A 1024-bit key violates this MUST — the algorithm name (RS256) and the key size are separate compliance requirements. The team conflates algorithm type compliance with key size compliance.

**Phrasing-B:** RS256 algorithm allowlist is correct. Signing key is 1024-bit RSA. RFC 7518 recommends 2048+ bits. Is the algorithm allowlist compliance separate from key size compliance? Is 1024-bit RSA with RS256 a REPROCESS?

---

## Pair[CB4-3.4-408]

**A:** Three-territory review. All controls described for a B2B SaaS API. OAuth: exact redirect URIs; PKCE S256 enforced for all clients (public and confidential); state validated; refresh and access tokens revoked on logout. Session: 30-minute idle timeout; back-channel SSO logout. Authentication: Argon2id m=65536. Token: RS256 allowlist; aud validated; signing key 2048-bit, rotated annually; introspection endpoint requires client authentication. Credential management: Vault. Security Champion + OWASP checklist.

**B:**
```
Arbiter: All applicable arbiters

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 universal PKCE ✓; V10.2.6 ✓; V10.3 ✓ → PASS

T-asvs-session:
  V7.2.4 ✓; V7.3.1 back-channel ✓ → PASS

T-asvs-authentication:
  V6.2.5 Argon2id ✓ → PASS

T-asvs-token:
  V9.1.1 RS256 allowlist ✓
  V10.5.1 aud ✓
  V9.2: 2048-bit key ✓; annual rotation ✓
  Introspection: client auth required ✓ (RFC 7662 §2.1 satisfied)
  → PASS

T-ssdf-process:
  PW.1 Vault ✓; PW.3.2 Security Champion + checklist ✓ → PASS

All territories: PASS
Overall verdict: PASS

Note: 2048-bit RSA signing key satisfies RFC 7518 §3.3 MUST; annual rotation
satisfies V9.2 key lifecycle practice; universal PKCE (public + confidential)
removes the confidential-client exemption CONCERN.
```

**Reason:** All controls correctly implemented including patterns that were CONCERN in prior pairs: universal PKCE (no confidential client exemption), 2048-bit key (meeting RFC 7518 §3.3), annual rotation, introspection with client auth. Full PASS.

**Phrasing-B:** B2B SaaS: universal PKCE, exact URIs, both token types revoked on logout, back-channel logout, Argon2id, RS256 with 2048-bit key rotated annually, introspection with client auth, Vault, Security Champion. Does this comprehensive implementation pass all territories?

---

## Pair[CB4-3.4-409]

**A:** Combined review — two CONCERNs and one PASS. JWT signing key: RSA 2048-bit, rotated quarterly, kid versioning. RS256 allowlist. Introspection endpoint: client authentication required. Refresh tokens: 30-day expiry, no single-use rotation (same token reused throughout the 30-day window). nbf claim: set to `iat - 30 minutes` ("slow services in our cluster"). Session: 30-minute idle timeout; application and SSO session terminated on logout; refresh tokens revoked on logout.

**B:**
```
Arbiter: ASVS-V9-Token + RFC-JWT + ASVS-V10-OAuth + ASVS-V7-Session

T-asvs-token:
  V9.1.1 (RS256 allowlist): PASS ✓
  V9.2 (2048-bit, quarterly rotation, kid): PASS ✓
  Introspection (RFC 7662 §2.1): PASS ✓
  nbf = iat - 30 minutes: CONCERN
    30-minute backward offset extends effective validity to 60 minutes (if exp=iat+30)
    or creates meaningful backward window; disproportionate for clock skew
    "Slow services" is a deployment concern, not a clock skew scenario
  T-asvs-token verdict: CONCERN

T-rfc-jwt:
  RFC 7519 §4.1.5 (nbf = iat - 30 min): CONCERN (same reasoning)
  T-rfc-jwt verdict: CONCERN

T-oauth-oidc:
  V10.3: refresh tokens revoked on logout ✓ — PASS
  Refresh token rotation (absent, 30-day expiry): CONCERN
    Without rotation, a stolen refresh token is usable for 30 days undetected
    V10.3 logout revocation satisfies the baseline; rotation is defense-in-depth
    Absence of rotation: CONCERN
  T-oauth-oidc verdict: CONCERN

T-asvs-session:
  V7.3.1 (app + SSO logout): PASS ✓
  T-asvs-session verdict: PASS

Overall verdict: CONCERN (CONCERN overrides PASS; no REPROCESS present)
```

**Reason:** Token key management, introspection auth, and session logout are all correctly implemented (PASS). Two CONCERN gaps: (1) nbf = iat - 30 minutes is disproportionate for clock skew; (2) no refresh token rotation leaves a 30-day stolen-token window. Both are CONCERNs (not REPROCESS) because the core requirements are met — the gaps are defense-in-depth and hygiene. CONCERN overrides PASS → overall CONCERN.

**Phrasing-B:** Correct key management, RS256 allowlist, introspection with client auth, SSO back-channel logout all pass. Refresh tokens not rotated (same token for 30 days). nbf = iat - 30 minutes for slow cluster services. Are the nbf offset and no rotation each a CONCERN, and does their combination change the severity?

---

## Pair[CB4-3.4-410]

**A:** MFA + credential management combined. Application: TOTP MFA enforced for all logins. Passwords: bcrypt cost 10. Application secrets: database password in environment variable set by Docker Compose `environment:` block in `docker-compose.yml` — the plaintext password value is in the `docker-compose.yml` file which is committed to the repository.

**B:**
```
Arbiter: ASVS-V6-Auth + SSDF-PW + CWE-Authn

T-asvs-authentication:
  V6.3 (MFA): TOTP for all logins ✓ — PASS
  V6.2.5 (bcrypt cost 10): cost 10 meets OWASP minimum (≥ cost 10) ✓ — PASS
  T-asvs-authentication verdict: PASS

T-ssdf-process:
  PW.1 (database password in docker-compose.yml committed to repository):
    - The plaintext credential value is in a committed configuration file
    - docker-compose.yml in version control = credential in source history
    - This is equivalent to hardcoding in terms of exposure: anyone with
      repository access can retrieve the database password
    - Severity: REPROCESS — committed plaintext credential in source
  T-ssdf-process verdict: REPROCESS

T-cwe-authn:
  CWE-798 (hardcoded/committed credentials): REPROCESS
    Plaintext database password committed to repository in docker-compose.yml.
    (Note: CWE-798 applies to committed configuration files, not only source code)

Overall verdict: REPROCESS (PW.1 + CWE-798 REPROCESS overrides V6.3 + V6.2.5 PASS)
```

**Reason:** MFA (all-login TOTP) and bcrypt cost 10 are both correct (PASS). The critical gap is the database password in `docker-compose.yml` committed to the repository — this is a committed plaintext credential, equivalent to hardcoding in terms of exposure. PW.1 REPROCESS + CWE-798 REPROCESS override the authentication PASS. Committed configuration files count for CWE-798.

**Phrasing-B:** All-login TOTP MFA, bcrypt cost 10 — both correct. But database password is plaintext in docker-compose.yml which is committed to the repo. MFA and hashing pass; does the committed docker-compose credential create a REPROCESS?

---

## Pair[CB4-3.4-411]

**A:** Comprehensive review — two REPROCESSes and multiple PASSes. Application: exact redirect URIs; PKCE S256; state validated; logout revokes access token only (refresh token survives 30 days); SSO back-channel logout; Argon2id m=65536; /me/permissions; RS256 allowlist; signing key 1024-bit RSA never rotated; Vault for application secrets; Security Champion + OWASP checklist; bounded privilege grants; aud validated.

**B:**
```
Arbiter: Multiple

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 ✓; V10.2.6 ✓
  V10.3: refresh token not revoked on logout ✗ → REPROCESS

T-asvs-session:
  V7.3.1 back-channel ✓ → PASS

T-asvs-authentication:
  V6.2.5 Argon2id ✓ → PASS

T-asvs-authorization:
  V8.2.1 ✓; V8.1.2 ✓ → PASS

T-asvs-token:
  V9.1.1 RS256 allowlist ✓
  V10.5.1 aud ✓
  RFC 7518 §3.3: 1024-bit RSA key → REPROCESS (MUST ≥ 2048 bits for RS256)
  V9.2: never rotated → CONCERN (within REPROCESS territory; REPROCESS dominates)
  T-asvs-token: REPROCESS

T-ssdf-process: Vault ✓; Security Champion ✓ → PASS
T-asvs-authorization (CWE-269): bounded grants ✓ → PASS

Overall verdict: REPROCESS
(Two REPROCESS findings: V10.3 refresh token survival; RFC 7518 §3.3 1024-bit key)
```

**Reason:** Two REPROCESS findings: (1) V10.3 — refresh token not revoked on logout; (2) RFC 7518 §3.3 — 1024-bit RSA key violates the MUST ≥ 2048-bit requirement. All other territories pass. Two independent REPROCESS findings set the overall verdict. The key size violation is distinct from and independent of the key rotation concern.

**Phrasing-B:** Everything else correct: exact URIs, PKCE, Argon2id, /me/permissions, Vault, Security Champion, bounded grants, aud validated, back-channel logout. Gaps: refresh token survives logout, and signing key is 1024-bit RSA. How many REPROCESS findings are there and what are they?

---

## Pair[CB4-3.4-412]

**A:** Post-remediation comprehensive review. Prior assessment found: (1) 1024-bit RSA key → replaced with 4096-bit RSA key, rotated; (2) refresh token survives logout → both tokens now revoked on logout + single-use rotation added; (3) open introspection → Bearer access token required for callers; (4) docker-compose credential → removed from file, key rotated, Vault injection added. All four remediations verified.

**B:**
```
Arbiter: Multiple

Remediation verification:

(1) RSA key size: 4096-bit RSA key ✓
    RFC 7518 §3.3 MUST ≥ 2048 bits — satisfied (4096 exceeds minimum) ✓
    Key rotation performed ✓
    Prior REPROCESS (1024-bit): remediated → PASS

(2) Refresh token revocation: both tokens revoked on logout ✓
    Single-use rotation added ✓ — additional defense-in-depth
    V10.3: PASS ✓
    Prior REPROCESS (refresh survival): remediated → PASS

(3) Introspection endpoint: Bearer access token required for callers ✓
    RFC 7662 §2.1 MUST satisfied ✓
    Prior REPROCESS (open endpoint): remediated → PASS

(4) docker-compose.yml credential: removed from file ✓
    Credential rotated ✓ (exposed credential in repo history — rotation required)
    Vault injection added ✓
    PW.1: PASS ✓; CWE-798: PASS ✓
    Prior REPROCESS: remediated → PASS

All four remediations verified: PASS
Note: prior findings do not carry forward after remediation.
Current state assessed on its own merits.
Overall verdict: PASS
```

**Reason:** All four prior findings remediated. Key rotation: 4096-bit replaces 1024-bit (RFC 7518 §3.3 satisfied). Refresh token: both revoked + single-use rotation (V10.3 satisfied). Introspection: Bearer auth (RFC 7662 §2.1 satisfied). Docker-compose credential: removed and rotated (PW.1 + CWE-798 resolved). Rotation is critical for the docker-compose credential — it was in repository history. Post-remediation PASS.

**Phrasing-B:** We fixed all four prior findings: 4096-bit RSA key replacing 1024-bit; both token types revoked on logout plus single-use rotation; introspection now requires Bearer auth; docker-compose credential removed, rotated, and replaced with Vault injection. Does post-remediation yield an overall PASS?

---

## File Summary

| Pair ID | Description | Stratum | Arbiters | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-403 | Vault+rotation PASS + open introspection REPROCESS → REPROCESS overall | C | RFC-OAuth2 + SSDF-PW + V9-Token | REPROCESS |
| CB4-3.4-404 | All-login TOTP + 15-min idle + back-channel + Argon2id → PASS | B | V6-Auth + V7-Session | PASS |
| CB4-3.4-405 | Standalone app no SSO V7.3.1 PASS + refresh survival V10.3 REPROCESS | C | V10-OAuth + V7-Session | REPROCESS |
| CB4-3.4-406 | RS256 allowlist + nbf=iat-5 + aud + quarterly rotation → all PASS | B | RFC-JWT + V9-Token | PASS |
| CB4-3.4-407 | RS256 allowlist PASS + 1024-bit RSA key REPROCESS (RFC 7518 §3.3 MUST) | C/D | V9-Token + RFC-JWT | REPROCESS |
| CB4-3.4-408 | Universal PKCE + 2048-bit key + annual rotation + introspection auth → PASS | B | Multiple | PASS |
| CB4-3.4-409 | Key management PASS + nbf 30-min CONCERN + no rotation CONCERN → CONCERN overall | C | V9-Token + RFC-JWT + V10-OAuth | CONCERN |
| CB4-3.4-410 | All-login MFA PASS + bcrypt 10 PASS + docker-compose committed credential → REPROCESS | C | V6-Auth + SSDF-PW + CWE-Authn | REPROCESS |
| CB4-3.4-411 | Most controls PASS + refresh survival REPROCESS + 1024-bit key REPROCESS → REPROCESS | C | Multiple | REPROCESS |
| CB4-3.4-412 | All four prior findings remediated → post-remediation PASS | B | Multiple | PASS |
