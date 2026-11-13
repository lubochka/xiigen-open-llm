# CB4 Stage 3 — Node Cross-Source Set Extension 7
# Sequences CB4-3.4-377 through CB4-3.4-386
# Stratum: B/C/D — multi-source combinations, cross-territory interactions
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 772 instances

---

## Pair[CB4-3.4-377]

**A:** Combined OAuth and privilege management review. OAuth: exact redirect URIs; PKCE S256 with CSPRNG verifier; state validated. Authorization: /me/permissions endpoint; deny-by-default enforced. Role assignment API: authenticated + admin-role gate present, but no bound check — a Coordinator role can assign the Admin role to any user. Code review: Security Champion with OWASP checklist. All other controls pass.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V8-AuthZ + CWE-Authz + SSDF-PW

T-oauth-oidc:
  V10.2.1: exact redirect URIs ✓ — PASS
  V10.4.6: PKCE S256 + CSPRNG verifier ✓ — PASS
  V10.2.6: state validated ✓ — PASS
  T-oauth-oidc verdict: PASS

T-asvs-authorization:
  V8.2.1: /me/permissions endpoint ✓ — PASS
  V8.1.2 deny-by-default: enforced generally ✓
  Role assignment bound check: ABSENT ✗
    Coordinator can assign Admin role — privilege grant exceeds caller's level
    V8.1.2 verdict: REPROCESS

T-cwe-authz:
  CWE-269 (improper privilege management): REPROCESS
    Privilege grant not bounded by caller role; escalation path exists

T-ssdf-process:
  PW.3.2: Security Champion + OWASP checklist ✓ — PASS

T-asvs-authorization verdict: REPROCESS
Overall verdict: REPROCESS (REPROCESS in any territory overrides all PASSes)
```

**Reason:** OAuth, SSDF controls all pass. The authorization territory fails on V8.1.2 + CWE-269 because the role assignment API lacks a privilege bound check — a Coordinator can grant Admin. One REPROCESS in any territory sets the overall verdict.

**Phrasing-B:** Exact URIs, PKCE S256, state validated, /me/permissions, deny-by-default, Security Champion. But our role assignment API lets a Coordinator assign Admin — no bound check. Does the privilege escalation gap create a REPROCESS despite all other controls passing?

---

## Pair[CB4-3.4-378]

**A:** Credential management and algorithm review. JWT signing key stored in HashiCorp Vault; application retrieves it at startup via `vault kv get secret/jwt-signing-key`. JWT algorithm allowlist: `["RS256"]`. Database password stored in a Kubernetes Secret (no etcd encryption configured). The Kubernetes Secret is mounted as an environment variable. No external secrets manager for the database credential.

**B:**
```
Arbiter: SSDF-PW + ASVS-V9-Token

T-ssdf-process:
  PW.1 (JWT signing key): PASS
    Vault kv get at startup — credential retrieved from dedicated secrets manager;
    key value not in source code or config files ✓
  PW.1 (database password — Kubernetes Secret):
    Kubernetes Secret without etcd encryption: base64-encoded in etcd
    base64 is encoding, not encryption; plaintext accessible via etcd or broad RBAC
    Verdict: CONCERN
    Note: correct K8s resource type (Secret not ConfigMap); gap is configuration
    (etcd encryption absent), not fundamental architecture
  T-ssdf-process verdict: CONCERN (CONCERN for db credential overrides JWT signing key PASS)

T-asvs-token:
  V9.1.1: RS256 allowlist — closed-world, single algorithm ✓ — PASS

Overall verdict: CONCERN (CONCERN in ssdf-process; PASS in token; CONCERN overrides)
```

**Reason:** JWT signing key in Vault is correct (PASS path). Database password in K8s Secret without etcd encryption is CONCERN (correct resource type; configuration gap). The mixed PW.1 result — one credential correctly managed, one in a partially-protected store — yields CONCERN for the territory. RS256 allowlist PASS. Overall CONCERN.

**Phrasing-B:** JWT signing key in HashiCorp Vault. Database password in a Kubernetes Secret (no etcd encryption). RS256 allowlist only. Does the Vault-managed signing key compensate for the K8s Secret database password, or is the database credential a separate CONCERN?

---

## Pair[CB4-3.4-379]

**A:** Three-territory review. Password hashing: Argon2id m=65536, t=3, p=4 for user accounts; scrypt N=1024, r=8, p=1 for service account credentials. Session: 30-minute idle timeout; logout terminates application session and calls OIDC back-channel logout. OAuth: exact redirect URIs; PKCE S256; state validated; access and refresh tokens revoked on logout.

**B:**
```
Arbiter: ASVS-V6-Auth + ASVS-V7-Session + ASVS-V10-OAuth

T-asvs-authentication:
  V6.2.5 (user accounts — Argon2id m=65536): PASS ✓
  V6.2.5 (service accounts — scrypt N=1024): REPROCESS
    scrypt N=1024 is 32× below OWASP minimum N=32768
    Memory per hash ~1 MB vs recommended ~32 MB
    Memory-hard property near-nullified
    Service account credentials are not excluded from V6.2.5 — both
    user account and service account credentials must use compliant hashing
  T-asvs-authentication verdict: REPROCESS (one non-compliant path overrides)

T-asvs-session:
  V7.2.4: 30-min idle timeout ✓ — PASS
  V7.3.1: back-channel logout ✓ — PASS
  T-asvs-session verdict: PASS

T-oauth-oidc:
  V10.2.1: exact URIs ✓; V10.4.6: PKCE S256 ✓; V10.2.6: state ✓; V10.3: revocation ✓
  T-oauth-oidc verdict: PASS

Overall verdict: REPROCESS (authentication territory REPROCESS overrides)
```

**Reason:** Argon2id for user accounts satisfies V6.2.5. scrypt N=1024 for service accounts does not — service account credentials are still credentials subject to V6.2.5. Session and OAuth controls all pass. One REPROCESS territory overrides the overall verdict.

**Phrasing-B:** User accounts use Argon2id m=65536 — correct. Service accounts use scrypt N=1024 for performance. Session and OAuth all correct. Does the compliant user account hashing compensate for the service account scrypt N=1024, or does service account hashing need to meet V6.2.5 independently?

---

## Pair[CB4-3.4-380]

**A:** Combined PKCE and code review assessment. PKCE S256: code_verifier generated from `SecureRandom.getInstanceStrong()` producing 32 bytes, base64url-encoded (43 chars). SHA256 computed and compared at token exchange. All engineers have 40-hour security training annually. Code reviews required. New: a junior security engineer has been assigned as Security Champion but has no specific security code review checklist — they review PRs using general security knowledge without a structured checklist.

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  V10.4.6: PKCE S256:
    code_verifier: 32-byte CSPRNG, base64url-encoded → 43 chars ✓ (meets §4.1 minimum)
    SHA256 computed; compared to stored code_challenge ✓ (verification correct)
    S256 method enforced ✓
  T-oauth-oidc verdict: PASS

T-ssdf-process:
  PW.3.2 (security-focused code review):
    Security Champion designated ✓ — designated reviewer exists
    Security checklist: absent ✗
    Assessment:
      A designated Security Champion without a structured checklist is better
      than no designated reviewer, but does not fully satisfy PW.3.2.
      PW.3.2 requires structured security examination — the checklist is what
      makes the review systematic rather than dependent on individual memory.
      A new/junior Security Champion without a checklist has high variance per PR.
    Finding: CONCERN — Security Champion exists (positive); structured checklist
      absent (gap); security review is semi-structured, not fully systematic
  T-ssdf-process verdict: CONCERN

Overall verdict: CONCERN (CONCERN overrides PASS)
```

**Reason:** PKCE is fully and correctly implemented (PASS). The Security Champion designation is a positive step — better than no designated reviewer. But without a structured checklist, the review quality depends on the Champion's knowledge and memory per PR rather than systematic coverage. CONCERN: partial PW.3.2 satisfaction. CONCERN overrides PASS → overall CONCERN.

**Phrasing-B:** PKCE S256 fully correct: 32-byte CSPRNG verifier, SHA256 comparison. We designated a junior security engineer as Security Champion for PR reviews. No formal checklist — they use general security knowledge. PKCE passes; does the no-checklist Security Champion partially or fully satisfy PW.3.2?

---

## Pair[CB4-3.4-381]

**A:** Algorithm configuration and OIDC audience review. JWT algorithm allowlist: `["RS256"]`. At token issuance, the `aud` claim is set to `["api.example.com", "admin.example.com"]` — the same token is reused for both the API service and the admin service. Both services verify the signature and expiry. The API service checks `aud` contains `"api.example.com"`. The admin service does NOT check the `aud` claim — it accepts any valid RS256-signed token.

**B:**
```
Arbiter: ASVS-V9-Token + ASVS-V10-OAuth + RFC-JWT

T-asvs-token:
  V9.1.1 (algorithm allowlist): PASS — RS256 allowlist correctly configured
  V10.5.1 (aud claim validation):
    API service: aud validated ✓ — PASS
    Admin service: aud NOT validated ✗
      - The admin service accepts any valid RS256-signed token regardless of aud
      - A token issued for the API service (aud: ["api.example.com", "admin.example.com"])
        is also valid at the admin service without aud check
      - More critically: a token issued for any other audience in the ecosystem
        that is RS256-signed by the same key will be accepted by the admin service
      - Admin operations are high-privilege; accepting tokens without aud verification
        is especially high-risk here
    V10.5.1 verdict for admin service: REPROCESS

T-rfc-jwt:
  RFC 7519 §4.1.3 (aud claim): REPROCESS for admin service
  When aud is present and the application is the intended audience, it MUST verify.

T-asvs-token verdict: REPROCESS (admin service aud gap)
Overall verdict: REPROCESS
```

**Reason:** V9.1.1 allowlist correctly configured. V10.5.1 is violated at the admin service: aud claim is present in tokens but not validated, meaning any RS256-signed token from the same key is accepted regardless of intended audience. This is especially high-risk at an admin service. One REPROCESS overrides all PASSes.

**Phrasing-B:** RS256 allowlist. API service validates aud claim. Admin service only validates signature and expiry — no aud check. Tokens include both services in aud. Is the API service aud validation enough, or does the admin service aud gap create a REPROCESS?

---

## Pair[CB4-3.4-382]

**A:** Session and OAuth combined — all-PASS scenario. Application in an enterprise SSO environment. OAuth: exact redirect URIs for all clients; PKCE S256 with 32-byte CSPRNG verifier (43 chars); state parameter validated; access and refresh tokens both revoked on logout. Session: 30-minute idle timeout; on logout, application session cleared immediately + OIDC back-channel logout call to IdP terminating the SSO session; user must re-authenticate at IdP for any federated service. Algorithm: RS256 allowlist; aud claim validated against service identifier. Argon2id m=65536, t=3, p=4. Vault for all secrets. Security Champion + OWASP checklist. Privilege grants bounded by caller role.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session + ASVS-V9-Token + ASVS-V6-Auth +
         SSDF-PW + ASVS-V8-AuthZ + CWE-Authz

T-oauth-oidc:
  V10.2.1 ✓; V10.4.6 ✓; V10.2.6 ✓; V10.3 ✓ → PASS

T-asvs-session:
  V7.2.4 (30-min idle) ✓
  V7.3.1 (back-channel SSO logout) ✓ → PASS

T-asvs-token:
  V9.1.1 (RS256 allowlist) ✓
  V10.5.1 (aud claim validated) ✓ → PASS

T-asvs-authentication:
  V6.2.5 (Argon2id m=65536/t=3/p=4) ✓ → PASS

T-ssdf-process:
  PW.1 (Vault) ✓
  PW.3.2 (Security Champion + OWASP checklist) ✓ → PASS

T-asvs-authorization:
  V8.2.1 (aud claim validated — authorization transparency via JWT claims) ✓
  V8.1.2 (deny-by-default; privilege grants bounded) ✓ → PASS

T-cwe-authz:
  CWE-269: bounded grants ✓ → PASS

All territories: PASS
Overall verdict: PASS
```

**Reason:** Comprehensive implementation of all controls across all territories. SSO back-channel logout, CSPRNG verifier length, aud validation, bounded privilege grants — all patterns from recent training pairs — implemented correctly. All-PASS verdict.

**Phrasing-B:** Enterprise SSO with exact URIs, PKCE S256 CSPRNG verifier, state, token revocation, back-channel logout, RS256 allowlist, aud validated, Argon2id, Vault, Security Champion, bounded privilege grants. Does this comprehensive implementation yield an all-PASS across all territories?

---

## Pair[CB4-3.4-383]

**A:** Code review and authentication combined. Security Champion reviews all PRs with OWASP checklist and merge authority. SAST tool (Semgrep) integrated in CI/CD — runs on all PRs. Password hashing: MD5 used for all user passwords (legacy system). The team states: "Our code review process is mature — the Security Champion and SAST tool together provide comprehensive coverage. V6.2.5 is about the hashing algorithm, and MD5 is what the legacy database uses."

**B:**
```
Arbiter: SSDF-PW + ASVS-V6-Auth

T-ssdf-process:
  PW.3.2 (security-focused code review): PASS
    Security Champion + OWASP checklist + merge authority ✓
    PW.4 (SAST): PASS — Semgrep integrated in CI/CD ✓
  T-ssdf-process verdict: PASS

T-asvs-authentication:
  V6.2.5 (memory-hard password hashing): REPROCESS
    MD5 is not a memory-hard algorithm.
    MD5 is a cryptographic hash function designed for speed, not password hashing.
    MD5 provides no memory-hardness, no work factor, and no iteration count.
    A GPU can compute billions of MD5 hashes per second — password database
    compromise leads to near-immediate cracking.
    "Legacy system" does not create a V6.2.5 exception — the requirement applies
    regardless of the age of the codebase.
    Team argument ("SAST + Champion = comprehensive"): rejected for V6.2.5
      Code review quality is independent of the algorithm compliance requirement.
      A well-reviewed codebase using MD5 for passwords still fails V6.2.5.
  T-asvs-authentication verdict: REPROCESS

Overall verdict: REPROCESS (REPROCESS overrides SSDF PASS)
```

**Reason:** Security Champion + SAST satisfies PW.3.2 and PW.4 (PASS). MD5 for password hashing fails V6.2.5 regardless of process maturity — MD5 is not a memory-hard algorithm. Legacy status is not an exception. Code review quality is independent of algorithm compliance. One REPROCESS overrides all PASSes.

**Phrasing-B:** Mature code review: Security Champion, OWASP checklist, Semgrep SAST, merge authority. Password hashing uses MD5 (legacy). Process controls are excellent. Does the mature code review process compensate for MD5 password hashing, or does MD5 fail V6.2.5 independently?

---

## Pair[CB4-3.4-384]

**A:** JWT DEBUG logging graduated severity review. Three different scenarios submitted for comparative assessment:
(A) JWT logged at DEBUG; production at INFO — DEBUG suppressed in prod.
(B) JWT logged at DEBUG; staging environment at DEBUG with production-equivalent data.
(C) JWT logged at INFO directly (not DEBUG) in production code — always logged in production.

**B:**
```
Arbiter: ASVS-V9-Token

Comparative severity assessment:

Scenario A — JWT at DEBUG; prod at INFO:
  V9.3.1: CONCERN
  Production is currently protected by log level configuration.
  Risk is latent: log level change or misconfiguration triggers exposure.
  Finding: CONCERN — meaningful mitigation exists; latent risk present.

Scenario B — JWT at DEBUG; staging at DEBUG with production data:
  V9.3.1: REPROCESS
  The credential exposure is active in the staging environment.
  Staging has production-equivalent data (real users' JWTs).
  Staging has broader access (developers, QA, contractors) than production.
  Finding: REPROCESS — active credential exposure, not latent risk.
  Distinguishing factor: production data in staging elevates from CONCERN to REPROCESS.

Scenario C — JWT logged at INFO directly in production code:
  V9.3.1: REPROCESS
  JWTs are actively logged in every production request that validates a token.
  Any principal with production log access can retrieve current, valid JWTs.
  No log level configuration can suppress this — it fires at INFO always.
  Finding: REPROCESS — direct, active, unrestricted production credential exposure.

Severity gradient:
  CONCERN: latent (code exists; suppressed in prod; risk if level changes)
  REPROCESS: active in staging with prod data
  REPROCESS: active in production (always fires)

The distinction between Scenario A (CONCERN) and Scenario B/C (REPROCESS)
is the presence of active credential exposure vs. latent configuration risk.

Overall verdicts: A=CONCERN, B=REPROCESS, C=REPROCESS
```

**Reason:** The severity gradient for JWT logging depends on whether the exposure is latent or active. DEBUG disabled in production = CONCERN (latent). DEBUG enabled in staging with production data = REPROCESS (active, lower-trust environment, real credentials). INFO level in production = REPROCESS (active, unrestricted). This pair teaches the comparative graduation explicitly.

**Phrasing-B:** Three scenarios: (A) JWT at DEBUG, prod at INFO — suppressed; (B) JWT at DEBUG, staging has prod data; (C) JWT at INFO directly in production code. Is scenario A a CONCERN and B/C both REPROCESS? What's the distinguishing principle?

---

## Pair[CB4-3.4-385]

**A:** Multi-source review — refresh token and session combined with code review gap. OAuth token revocation: access tokens revoked on logout; refresh tokens NOT revoked (90-day expiry). Session: 30-minute idle timeout; application session cleared on logout; OIDC back-channel logout initiated. Code review: Security Champion + OWASP checklist (PW.3.2 satisfied). The team acknowledges the refresh token gap and states: "We plan to implement refresh token revocation next quarter."

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session + SSDF-PW

T-oauth-oidc:
  V10.2.1, V10.4.6, V10.2.6: not described but not the focus — not evaluated
  V10.3 (token revocation on logout):
    Access token revoked ✓
    Refresh token NOT revoked ✗ — 90-day expiry survives logout
    V10.3 verdict: REPROCESS
    Finding: 90-day refresh token valid post-logout enables continued token
    acquisition after user has terminated their session

T-asvs-session:
  V7.2.4 (30-min idle): PASS ✓
  V7.3.1 (back-channel logout): PASS ✓ — application session + SSO session terminated
  Session invalidation is correctly implemented.
  The refresh token survival undermines the effective security outcome
  but V7.3.1 is satisfied by the logout mechanism itself (captured under V10.3).

T-ssdf-process:
  PW.3.2: Security Champion + OWASP checklist ✓ — PASS

"We plan to implement next quarter" argument:
  A planned future remediation does not affect the current state assessment.
  The current state has a REPROCESS finding. The plan is noted but does not
  change the verdict.

T-oauth-oidc verdict: REPROCESS
T-asvs-session verdict: PASS
T-ssdf-process verdict: PASS
Overall verdict: REPROCESS
```

**Reason:** V7.3.1 session invalidation is correctly implemented (PASS). V10.3 is not — refresh tokens with 90-day expiry survive logout, enabling continued token acquisition after logout. The planned future fix does not affect the current assessment. PW.3.2 PASS. One REPROCESS overrides all other PASSes.

**Phrasing-B:** Back-channel SSO logout, 30-min idle, Security Champion + checklist all pass. Access tokens revoked on logout. Refresh tokens have 90-day expiry and survive logout — we plan to fix this next quarter. Does the planned fix affect the current assessment, or is refresh token survival a REPROCESS today?

---

## Pair[CB4-3.4-386]

**A:** Comprehensive review — all patterns from ext14 correctly implemented plus two new elements. All prior controls: exact redirect URIs; CSPRNG PKCE verifier (43+ chars); state validated; token revocation; back-channel SSO logout; Argon2id m=65536; /me/permissions; RS256 allowlist; Vault for all secrets; Security Champion + OWASP checklist; privilege grants bounded; aud claim validated. New elements: (1) privilege grants are logged in an append-only audit log with user ID, granted role, grantor ID, and timestamp; (2) token logging reviewed and no JWT or token value appears in any log level.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session + ASVS-V9-Token + ASVS-V6-Auth +
         SSDF-PW + ASVS-V8-AuthZ + CWE-Authz + RFC-JWT

All prior controls: PASS (verified per prior assessments)

New element 1 — Privilege grant audit log:
  V8.3.2 (audit log of sensitive operations): PASS
    Role grants logged with user ID, role name, grantor ID, timestamp in
    append-only log — satisfies audit trail requirements for privilege operations
  CWE-Authz additional depth: audit trail provides detection layer alongside
    the authorization bound check — defense-in-depth ✓

New element 2 — Token logging review:
  V9.3.1: PASS
    No JWT or token value appears in any log at any level ✓
    The logging review confirms the gap identified in prior pair (V9.3.1 CONCERN
    for header logging on rejection) would not be present here — no token
    data in any log.

All territories: PASS
Overall verdict: PASS
```

**Reason:** All prior controls correctly implemented (PASS). Two new elements add defense-in-depth: (a) privilege grant audit log satisfies V8.3.2 and provides detection alongside the V8.1.2 authorization bound; (b) token logging review confirms no token value in logs at any level, satisfying V9.3.1. Comprehensive PASS.

**Phrasing-B:** All prior controls correct. Added: privilege grants logged in append-only audit log with full attribution. Token logging reviewed — no JWT value appears in any log at any level. Do the audit log and clean token logging contribute positively, and does this implementation yield an all-PASS?

---

## File Summary

| Pair ID | Description | Stratum | Arbiters | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-377 | OAuth PASS + CWE-269 privilege escalation → REPROCESS overall | B/C | V10-OAuth + V8-AuthZ + CWE-Authz + SSDF-PW | REPROCESS |
| CB4-3.4-378 | Vault (PASS) + K8s Secret no etcd (CONCERN) + RS256 allowlist PASS → CONCERN | B/C | SSDF-PW + V9-Token | CONCERN |
| CB4-3.4-379 | Argon2id PASS + scrypt N=1024 service accounts REPROCESS + session/OAuth PASS → REPROCESS | C | V6-Auth + V7-Session + V10-OAuth | REPROCESS |
| CB4-3.4-380 | PKCE PASS + Security Champion without checklist → CONCERN overall | B/C | V10-OAuth + SSDF-PW | CONCERN |
| CB4-3.4-381 | RS256 allowlist PASS + API aud PASS + admin service no aud → REPROCESS | C | V9-Token + V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-382 | All 7-territory comprehensive correct implementation → PASS | B | Multiple | PASS |
| CB4-3.4-383 | PW.3.2+PW.4 PASS + MD5 passwords REPROCESS → REPROCESS overall | C/D | SSDF-PW + V6-Auth | REPROCESS |
| CB4-3.4-384 | JWT logging severity gradient: DEBUG/prod=CONCERN; DEBUG/staging-prod-data=REPROCESS; INFO/prod=REPROCESS | D | V9-Token | Mixed |
| CB4-3.4-385 | Session PASS + refresh token no revocation (V10.3) REPROCESS + planned fix irrelevant → REPROCESS | B/C | V10-OAuth + V7-Session + SSDF-PW | REPROCESS |
| CB4-3.4-386 | All prior controls + audit log + clean token logging → comprehensive PASS | B | Multiple | PASS |
