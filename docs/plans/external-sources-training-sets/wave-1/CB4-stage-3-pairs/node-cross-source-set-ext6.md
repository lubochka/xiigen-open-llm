# CB4 Stage 3 — Node Cross-Source Set Extension 6
# Sequences CB4-3.4-351 through CB4-3.4-360
# Stratum: B/C/D — multi-source combinations, cross-territory interactions
# Instances: 20 (10 base pairs × 2 phrasings)
# Running total after this file: 720 instances

---

## Pair[CB4-3.4-351]

**A:** Combined OAuth and session review. Application registers exact redirect URIs for all OAuth clients. Server-side session invalidation on logout clears the application session and initiates OIDC back-channel logout to terminate the SSO session. PKCE S256 enforced. State parameter validated. However: refresh tokens are long-lived (90-day expiry) and are not revoked when the user logs out — the application only revokes the access token and clears the session cookie on logout.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session

T-oauth-oidc assessment:
  V10.2.1: PASS — exact redirect URIs registered for all clients
  V10.4.6: PASS — PKCE S256 enforced
  V10.2.6: PASS — state parameter validated
  V10.3 (token revocation on logout): REPROCESS
    Refresh tokens not revoked on logout. A 90-day refresh token that survives
    logout allows the holder to obtain new access tokens long after the user
    has terminated their session. This directly undermines the session
    invalidation provided by V7.3.1.
    Evidence gap: refresh tokens with 90-day expiry remain valid post-logout.
  T-oauth-oidc verdict: REPROCESS (one sub-requirement REPROCESS overrides PASS verdicts)

T-asvs-session assessment:
  V7.3.1: Application session cleared + SSO back-channel logout initiated ✓
  However: refresh token survival undermines the effective logout
  V7.3.1 verdict: PASS for session invalidation mechanism itself
  Note: the refresh token gap is captured under V10.3

T-asvs-session verdict: PASS
Overall verdict: REPROCESS (REPROCESS in any territory overrides all other verdicts)
```

**Reason:** V10.3 requires token revocation on logout — including refresh tokens. A 90-day refresh token that survives logout enables continued access token acquisition after the user has "logged out." The session invalidation (V7.3.1) is correctly implemented (back-channel SSO logout); the gap is specifically the refresh token lifecycle under V10.3. One REPROCESS in any territory sets the overall verdict.

**Phrasing-B:** Exact redirect URIs, PKCE S256, state validated, SSO back-channel logout all correctly implemented. But refresh tokens have a 90-day expiry and we don't revoke them on logout — only access tokens are revoked. Does the refresh token survival make this a REPROCESS despite the correct session invalidation?

---

## Pair[CB4-3.4-352]

**A:** Combined code review and PKCE assessment. Security review of a mobile application. PKCE S256 implemented — code_challenge sent in authorization request; code_verifier validated by computing SHA256 and comparing to stored code_challenge at token exchange. All engineers complete 40-hour security training annually. PRs require one peer review. No security checklist on the PR template. No Security Champion role. The team relies on security-trained engineers to catch security issues during review.

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc assessment:
  V10.4.6 (PKCE S256): PASS
    - code_challenge included in authorization request ✓
    - code_verifier SHA256 computed and compared to stored challenge at exchange ✓
    - S256 method correctly implemented end-to-end
  T-oauth-oidc verdict: PASS

T-ssdf-process assessment:
  PW.3.2 (security-focused code review): CONCERN
    - 40-hour annual security training: positive foundation; knowledge exists
    - Peer review required: review practice exists
    - No security checklist: security examination not structured; depends on
      individual reviewer memory per PR
    - No Security Champion: no designated security review responsibility
    - Security training informs judgment but does not substitute for structured
      security review per PW.3.2
    Finding: CONCERN — review practice exists; training exists; structure absent
  T-ssdf-process verdict: CONCERN

Overall verdict: CONCERN (CONCERN overrides PASS; REPROCESS would override both)
```

**Reason:** PKCE S256 is fully and correctly implemented (PASS). The code review gap is structural — training provides knowledge but not systematic application per PR. PW.3.2 requires structured security examination during review. CONCERN overrides PASS in overall verdict. This pair teaches that a PASS in one territory and CONCERN in another yields CONCERN overall.

**Phrasing-B:** Mobile app, PKCE S256 fully implemented including SHA256 hash comparison at token exchange. Security-trained engineers do peer review on all PRs. No security checklist, no Security Champion. PKCE is solid — does the code review gap create a CONCERN overall despite the correct PKCE implementation?

---

## Pair[CB4-3.4-353]

**A:** Combined algorithm and credential management review. JWT service uses an allowlist of `["RS256"]` — no other algorithm accepted. JWT signing key is a 2048-bit RSA private key stored in a Kubernetes Secret (base64-encoded in etcd; etcd encryption not configured). The Kubernetes Secret is mounted into the JWT service pod as an environment variable `JWT_SIGNING_KEY`. No external secrets manager (Vault, etc.) is used.

**B:**
```
Arbiter: ASVS-V9-Token + SSDF-PW

T-asvs-token assessment:
  V9.1.1 (algorithm allowlist): PASS
    Allowlist ["RS256"] — single algorithm, explicitly enumerated.
    No blocklist; no "accept any not blocked" logic. Correct implementation.
  T-asvs-token verdict: PASS

T-ssdf-process assessment:
  PW.1 (credential management — JWT signing key): CONCERN
    - JWT signing key is in a Kubernetes Secret: correct K8s resource type
    - However: Kubernetes Secret without etcd encryption is base64-encoded in etcd
    - base64 is encoding, not encryption; etcd access or broad RBAC grants
      reveal the key in plaintext
    - The JWT signing key is a high-value secret (compromise → ability to forge
      tokens for any user); elevated protection warranted
    - Finding: CONCERN — correct K8s primitive used; etcd encryption absent
    - Note: for a signing key (vs. a database password), the risk of etcd-accessible
      plaintext is elevated — any RBAC principal with Secret read access can
      exfiltrate a key that undermines the entire authentication model
  T-ssdf-process verdict: CONCERN

Overall verdict: CONCERN (CONCERN overrides PASS)
```

**Reason:** RS256 allowlist correctly satisfies V9.1.1 (PASS). The PW.1 concern is that the JWT signing key — a critical secret whose compromise allows token forgery — is stored in a Kubernetes Secret without etcd encryption. The gap is the same as any K8s Secret without encryption, but elevated because of the key's criticality. CONCERN overrides PASS → overall CONCERN.

**Phrasing-B:** RS256 allowlist only — correct JWT algorithm configuration. JWT signing key is in a Kubernetes Secret mounted as an env var. No Vault or other secrets manager. Is the RS256 allowlist enough, or does the signing key storage in an unencrypted K8s Secret create a concern?

---

## Pair[CB4-3.4-354]

**A:** Multi-territory review of a payment processing API. OAuth: exact redirect URIs; PKCE S256; state validated. Session: logout terminates app session and calls IdP back-channel logout; refresh tokens revoked on logout. Authentication: Argon2id m=65536, t=3, p=4. Authorization: /me/permissions endpoint with full permission set; deny-by-default. Token: RS256 allowlist; aud claim verified. Credential management: HashiCorp Vault. Code review: Security Champion + OWASP checklist. One gap: the application logs the full JWT in debug logs at the INFO level in production for troubleshooting purposes.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session + ASVS-V6-Auth + ASVS-V8-AuthZ +
         ASVS-V9-Token + SSDF-PW + CWE-Authz

T-oauth-oidc: PASS (V10.2.1, V10.4.6, V10.2.6, V10.3 — all satisfied)
T-asvs-session: PASS (V7.3.1 — back-channel logout + refresh revocation)
T-asvs-authentication: PASS (V6.2.5 — Argon2id correctly configured)
T-asvs-authorization: PASS (V8.2.1, V8.1.2 — /me/permissions, deny-by-default)
T-asvs-token:
  V9.1.1: PASS — RS256 allowlist
  V10.5.1: PASS — aud claim verified
  JWT logging in production: REPROCESS
    Full JWT logged at INFO level in production logs:
    (1) JWT contains user identity, roles, and session claims — logging full token
        exposes these to anyone with log access
    (2) If the JWT is not expired and is stolen from logs, it can be replayed
    (3) Production logs at INFO level are typically retained and broadly accessible
        to operations staff — this expands the token's attack surface significantly
    V9.3.1 or general token hygiene: sensitive tokens must not be logged in plaintext
    Finding: REPROCESS — production logging of full JWT undermines token security

T-ssdf-process: PASS (PW.1, PW.3.2)

T-asvs-token verdict: REPROCESS
Overall verdict: REPROCESS (one REPROCESS in any territory overrides all PASSes)
```

**Reason:** All controls are correctly implemented except for full JWT logging in production. Production INFO-level logging of the complete JWT exposes a replayable credential to anyone with log access. This is REPROCESS because an active session JWT in logs is directly exploitable — not a configuration gap but a credential exposure. One REPROCESS overrides the comprehensive PASSes across all other territories.

**Phrasing-B:** Payment API: all OAuth, session, auth, authz, token, credential management, and code review controls correctly implemented. But we log the full JWT at INFO level in production for debugging. Every other territory passes — does the JWT logging create a REPROCESS?

---

## Pair[CB4-3.4-355]

**A:** SSO environment logout and token revocation. Enterprise SSO federation. Application logout: (1) clears application session cookie; (2) calls OIDC back-channel logout to terminate SSO session at the IdP; (3) access token revoked; (4) refresh token revoked. After logout: user must re-authenticate at the IdP to access any federated service. One issue: the application does not validate the `aud` (audience) claim on incoming JWTs — the JWT library validates signature and expiry only.

**B:**
```
Arbiter: ASVS-V7-Session + ASVS-V10-OAuth + RFC-JWT

T-asvs-session:
  V7.3.1: PASS — application session cleared; OIDC back-channel logout initiated;
    SSO session at IdP terminated; user must re-authenticate post-logout. Correct
    and complete logout implementation in SSO context.

T-oauth-oidc / T-asvs-token:
  V10.3 (token revocation): PASS — access and refresh tokens revoked on logout
  V10.5.1 (aud claim validation): REPROCESS
    JWT library validates signature and expiry but not aud claim.
    - The aud (audience) claim identifies the intended recipient of the token.
    - Failing to validate aud means this service will accept JWTs issued for
      other services in the federation if they have valid signatures.
    - In an SSO federation, multiple services share an IdP; tokens issued for
      Service A will have a valid signature verifiable by Service B, but aud
      should restrict acceptance to the intended audience.
    - Without aud validation, a JWT stolen from Service A can be replayed at
      Service B (confused deputy / token cross-service misuse).
    Evidence gap: aud claim not validated.

T-rfc-jwt: REPROCESS (aud validation required per RFC 7519 §4.1.3)

T-asvs-session verdict: PASS
T-oauth-oidc verdict: REPROCESS (V10.5.1 overrides V10.3 PASS)
Overall verdict: REPROCESS
```

**Reason:** The logout implementation is complete and correct (PASS for V7.3.1 and V10.3). The gap is aud claim validation — in a federated environment, skipping aud validation means tokens issued for one service will be accepted by any other service in the federation. This is REPROCESS because token cross-service misuse is directly exploitable. REPROCESS overrides all PASSes.

**Phrasing-B:** Complete logout: app session cleared, OIDC back-channel logout, access and refresh tokens revoked. But we don't validate the aud claim — our JWT library only checks signature and expiry. Is the missing aud validation a REPROCESS despite the correct logout?

---

## Pair[CB4-3.4-356]

**A:** Password hashing and credential management combined review. User passwords hashed with Argon2id m=65536, t=3, p=4. API key hashing uses scrypt N=1024, r=8, p=1 (different hashing path for API key comparison). Application secrets (database passwords, third-party API keys) stored in HashiCorp Vault. No hardcoded credentials in source code.

**B:**
```
Arbiter: ASVS-V6-Auth + SSDF-PW

T-asvs-authentication:
  V6.2.5 (password hashing — user passwords):
    Argon2id m=65536, t=3, p=4: PASS
    m=65536 (64 MB) ≥ OWASP minimum m=47104 (46 MB) ✓
    t=3 iterations ≥ OWASP minimum t=1 ✓
    p=4 parallelism — acceptable ✓
    Verdict for user passwords: PASS

  V6.2.5 (API key hashing — scrypt N=1024):
    API keys are also subject to V6.2.5 if used as a credential verification
    mechanism (API key = bearer credential requiring secure storage).
    scrypt N=1024: REPROCESS
    N=1024 is 32× below OWASP minimum N=32768 for scrypt.
    Memory per hash: ~1 MB vs recommended ~32 MB.
    Memory-hard property near-nullified at N=1024.
    The API key hashing path does not satisfy V6.2.5.

  T-asvs-authentication verdict: REPROCESS (one sub-path REPROCESS overrides
    user password PASS)

T-ssdf-process:
  PW.1: HashiCorp Vault for all secrets; no hardcoded credentials: PASS

Overall verdict: REPROCESS (REPROCESS in asvs-authentication overrides PW.1 PASS)
```

**Reason:** User password hashing correctly satisfies V6.2.5 (Argon2id at minimum parameters). API key hashing with scrypt N=1024 does not — N=1024 is 32× below the OWASP minimum, defeating the memory-hard property. Both code paths are subject to V6.2.5 if API keys are bearer credentials. One non-compliant hashing path makes the territory REPROCESS. Vault satisfies PW.1 (PASS). REPROCESS overrides PASS.

**Phrasing-B:** User passwords: Argon2id m=65536 — correctly configured. API keys: scrypt N=1024 — optimized for response time. Application secrets in Vault. Does the correct Argon2id for user passwords compensate for the scrypt N=1024 on API keys, or does the API key path fail V6.2.5 independently?

---

## Pair[CB4-3.4-357]

**A:** Authorization model review. Application implements role-based access control (RBAC). Deny-by-default enforced: no operation proceeds without an explicit grant. /me/permissions endpoint returns the user's full permission set. Users can see all their granted permissions before attempting operations. One gap: the access control logic is partially enforced in the frontend (React component `PermissionGate` hides UI elements for unauthorized users) and partially enforced in the backend API. Not all backend endpoints verify authorization independently — some rely on the frontend to have hidden the UI control before the request was made.

**B:**
```
Arbiter: ASVS-V8-AuthZ + CWE-Authz

T-asvs-authorization:
  V8.2.1: PASS — /me/permissions endpoint provides full permission set proactively
  V8.1.2 (deny-by-default):
    Frontend enforcement: insufficient — frontend hides UI elements but does
    not constitute server-side authorization
    Backend enforcement: partial — not all endpoints verify authorization independently
    CWE-602 (client-side enforcement of server-side security): directly applicable
    A user who bypasses the frontend (e.g., direct API calls, browser dev tools,
    curl) can reach backend endpoints that do not independently verify authorization.
    V8.1.2 requires deny-by-default at the enforcement point — the server.
    Frontend-only or frontend-primary authorization is not server-side enforcement.
    V8.1.2 verdict: REPROCESS
      Evidence gap: backend endpoints that rely on frontend to have hidden UI
        controls do not independently verify authorization server-side

T-cwe-authz:
  CWE-285 (improper authorization): REPROCESS
    Backend endpoints without independent server-side authorization checks
    are directly bypassable. This is a concrete CWE-285 (improper authorization)
    or CWE-602 (client-side enforcement of server-side security) finding.

T-asvs-authorization verdict: REPROCESS
Overall verdict: REPROCESS
```

**Reason:** V8.2.1 is correctly satisfied (/me/permissions). V8.1.2 is not — deny-by-default must be enforced at the server. Backend endpoints that rely on the frontend having hidden the UI control are not independently authorized. A user who bypasses the frontend can reach those endpoints without authorization checks. Frontend authorization (hiding UI elements) is UX, not access control.

**Phrasing-B:** RBAC with deny-by-default and /me/permissions endpoint correctly implemented. Frontend PermissionGate hides UI elements. Some backend endpoints rely on the frontend to prevent unauthorized access rather than checking authorization independently. Is frontend-enforced authorization sufficient for V8.1.2?

---

## Pair[CB4-3.4-358]

**A:** Two-territory review: OAuth callback and security process. OAuth redirect URI exact registration for all clients; PKCE S256; state validation. Code review process: dedicated Security Champion who reviews all PRs using the OWASP Application Security Verification Standard code review guidance. Security Champion has authority to block merges on security findings. Annual threat model review conducted by the security team. No SAST (static analysis security testing) tool integrated in CI/CD.

**B:**
```
Arbiter: ASVS-V10-OAuth + SSDF-PW

T-oauth-oidc:
  V10.2.1: PASS — exact redirect URIs
  V10.4.6: PASS — PKCE S256 enforced
  V10.2.6: PASS — state validated
  T-oauth-oidc verdict: PASS

T-ssdf-process:
  PW.3.2 (security-focused code review): PASS
    Security Champion reviews all PRs using OWASP ASVS guidance;
    authority to block merges; this satisfies PW.3.2's requirement for
    security-focused code review — designated reviewer, structured checklist,
    merge authority.

  PW.4 (SAST tools): NOT EVALUATED
    PW.4 is a separate SSDF practice from PW.3.2. Absence of SAST is a
    PW.4 concern, not a PW.3.2 concern. This submission does not ask about
    PW.4 specifically; the Security Champion review satisfies PW.3.2.
    Note: SAST absence would be a PW.4 gap if evaluated; PW.3.2 is PASS.

  T-ssdf-process verdict: PASS (PW.3.2 satisfied; PW.4 not in scope of submission)

Overall verdict: PASS
```

**Reason:** All OAuth controls correctly implemented (PASS). Security Champion with OWASP guidance and merge authority satisfies PW.3.2 (PASS). The absence of SAST is a PW.4 concern, not a PW.3.2 concern — these are independent SSDF practices. The submission does not ask about PW.4; PW.3.2 is satisfied. Overall PASS.

**Phrasing-B:** Exact redirect URIs, PKCE S256, state validated — all OAuth correct. Security Champion reviews every PR with OWASP guidance and can block merges. No SAST tool in CI/CD. Does the Security Champion satisfy PW.3.2 even without SAST? Are these independent?

---

## Pair[CB4-3.4-359]

**A:** Three-territory review. OAuth: exact redirect URIs; PKCE S256 enforced and fully validated (challenge + verifier hash comparison); state parameter validated; token revocation on logout. Session: logout terminates application session and initiates OIDC back-channel logout. Authentication: scrypt N=1024, r=8, p=1 for user passwords. Token: RS256 allowlist; aud claim validated. Credential management: HashiCorp Vault; no hardcoded credentials.

**B:**
```
Arbiter: ASVS-V10-OAuth + ASVS-V7-Session + ASVS-V6-Auth + ASVS-V9-Token + SSDF-PW

T-oauth-oidc:
  V10.2.1: PASS
  V10.4.6: PASS (challenge + verifier hash comparison — complete implementation)
  V10.2.6: PASS
  V10.3: PASS (token revocation on logout)
  T-oauth-oidc verdict: PASS

T-asvs-session:
  V7.3.1: PASS (app session + SSO back-channel logout)
  T-asvs-session verdict: PASS

T-asvs-authentication:
  V6.2.5: scrypt N=1024 → REPROCESS
    N=1024 is 32× below OWASP minimum N=32768 for scrypt.
    Memory per hash ~1 MB vs recommended ~32 MB.
    Memory-hard property near-nullified.
  T-asvs-authentication verdict: REPROCESS

T-asvs-token:
  V9.1.1: PASS (RS256 allowlist)
  V10.5.1: PASS (aud claim validated)
  T-asvs-token verdict: PASS

T-ssdf-process:
  PW.1: PASS (HashiCorp Vault; no hardcoded credentials)
  T-ssdf-process verdict: PASS

Overall verdict: REPROCESS (scrypt N=1024 REPROCESS in asvs-authentication
overrides all territory PASSes)
```

**Reason:** OAuth, session, token, and credential management are all correctly implemented (PASS). The single gap is scrypt N=1024 — an order-of-magnitude misconfiguration of the memory-hard algorithm for password hashing. REPROCESS in one territory overrides all other PASSes regardless of the number of passing territories. This pair demonstrates that one implementation gap can set the overall verdict.

**Phrasing-B:** OAuth fully correct, SSO back-channel logout, RS256 allowlist with aud validation, Vault for secrets — all correct. User passwords hashed with scrypt N=1024 for performance. Does the scrypt N=1024 create an overall REPROCESS even when all other five territories pass?

---

## Pair[CB4-3.4-360]

**A:** Comprehensive five-territory review — all gaps remediated from previous assessment. Prior assessment identified: (1) scrypt N=1024 for passwords (now migrated to Argon2id m=65536, t=3, p=4); (2) Kubernetes Secret without etcd encryption for JWT signing key (now migrated to HashiCorp Vault); (3) 403-only permission disclosure (now /me/permissions endpoint implemented); (4) blocklist algorithm configuration (now RS256 allowlist only); (5) no security checklist in PRs (now Security Champion + OWASP checklist added). All five gaps remediated. Current state assessed.

**B:**
```
Arbiter: ASVS-V6-Auth + SSDF-PW + ASVS-V8-AuthZ + ASVS-V9-Token + SSDF-PW

T-asvs-authentication:
  V6.2.5: Argon2id m=65536, t=3, p=4 → PASS
  Prior gap (scrypt N=1024) remediated.

T-ssdf-process (credential management):
  PW.1: HashiCorp Vault for JWT signing key → PASS
  Prior gap (K8s Secret without etcd encryption) remediated.

T-asvs-authorization:
  V8.2.1: /me/permissions endpoint implemented → PASS
  Prior gap (403-only disclosure) remediated.

T-asvs-token:
  V9.1.1: RS256 allowlist only → PASS
  Prior gap (blocklist approach) remediated.

T-ssdf-process (code review):
  PW.3.2: Security Champion + OWASP checklist → PASS
  Prior gap (no security structure) remediated.

All five remediated controls: PASS
Overall verdict: PASS

Note: reassessment after remediation correctly yields PASS for all previously
flagged controls. A prior REPROCESS or CONCERN finding that has been remediated
does not carry forward — current state is assessed on its own merits.
```

**Reason:** Post-remediation assessment. All five previously identified gaps are resolved: Argon2id replaces scrypt N=1024; Vault replaces K8s Secret; /me/permissions replaces 403-only disclosure; RS256 allowlist replaces blocklist; Security Champion + checklist replaces unstructured review. Each remediated control is assessed on its current state and passes. Prior findings do not persist after remediation.

**Phrasing-B:** We remediated all five gaps from our previous assessment: Argon2id instead of scrypt N=1024; Vault instead of K8s Secret; /me/permissions endpoint; RS256 allowlist; Security Champion with OWASP checklist. Does remediation of all five previously flagged items yield an overall PASS on reassessment?

---

## File Summary

| Pair ID | Description | Stratum | Arbiters | Verdict |
|---------|-------------|---------|---------|---------|
| CB4-3.4-351 | V10.3 refresh token not revoked on logout + V7.3.1 PASS → REPROCESS | B/C | ASVS-V10-OAuth + V7-Session | REPROCESS |
| CB4-3.4-352 | V10.4.6 PKCE PASS + PW.3.2 no checklist/champion → CONCERN overall | B/C | ASVS-V10-OAuth + SSDF-PW | CONCERN |
| CB4-3.4-353 | V9.1.1 RS256 allowlist PASS + PW.1 K8s Secret no etcd encryption → CONCERN | B/C | ASVS-V9-Token + SSDF-PW | CONCERN |
| CB4-3.4-354 | All 7 territories PASS except full JWT in production debug logs → REPROCESS | C/D | Multiple | REPROCESS |
| CB4-3.4-355 | V7.3.1+V10.3 logout PASS + V10.5.1 aud not validated → REPROCESS | B/C | V7-Session + V10-OAuth + RFC-JWT | REPROCESS |
| CB4-3.4-356 | Argon2id PASS + scrypt N=1024 for API keys REPROCESS + Vault PASS → REPROCESS | C | ASVS-V6-Auth + SSDF-PW | REPROCESS |
| CB4-3.4-357 | V8.2.1 PASS + V8.1.2 frontend-only authz → REPROCESS + CWE-285 | C | ASVS-V8-AuthZ + CWE-Authz | REPROCESS |
| CB4-3.4-358 | OAuth PASS + PW.3.2 Security Champion PASS + no SAST (PW.4 not in scope) → PASS | B | ASVS-V10-OAuth + SSDF-PW | PASS |
| CB4-3.4-359 | OAuth+session+token+vault PASS + scrypt N=1024 → REPROCESS overall | C | Multiple | REPROCESS |
| CB4-3.4-360 | Post-remediation reassessment: all 5 prior gaps resolved → PASS | B | Multiple | PASS |
