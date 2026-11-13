# Node Cross-Source — Extension Set 1
# Stage 3 Action 3 — Coverage Extension (new domains in multi-source scenarios)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-189 through CB4-3.4-198
# Instances: 20 (10 base pairs × 2 phrasings each)
# Running total after this file: 396 instances (sequences 001–198)

---

## Coverage targets for this file

Multi-source scenarios using the new domains introduced in the extension sets.
Key combinations:
  - V6.3 MFA + CWE-307 brute force (authentication depth: MFA is present but also needs lockout)
  - V8.3 admin access + SSDF PO.1 training (organizational + control: admins aren't trained)
  - CWE-352 CSRF + V7 session (cross-site + session management)
  - RFC 8628 device flow + V10.3 token revocation (two OAuth domains)
  - SSDF PO.3 SBOM + CWE-862 IDOR (supply chain + runtime gap)
  - V7 session fixation + V9 JWT token (session ID vs token confusion)
  - ASVS V8.1.2 deny-by-default + SSDF PW.2 (policy + evidence)
  - Client_credentials + V6.3 MFA (M2M context: MFA doesn't apply)
  - CWE-307 + CWE-862 combined (auth + authz gaps in same system)
  - Full new-domain all-PASS baseline

---

## Pair CB4-3.4-189

**Node:** 07 + 08 (focus)
**Stratum:** C (depth — V6.3 MFA present + CWE-307 brute force: two different gaps at the authentication layer)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
consolidated_findings:
  territories:
    - territory: T-asvs-mfa
      label: "asvs-authentication"
      findings: ["V6.3.1: TOTP MFA enforced for admin users", "V6.3.2: satisfied"]
      verdict_signal: PASS
    - territory: T-cwe-authn
      label: "cwe-authn"
      findings: ["login endpoint: no lockout, no rate limiting after 500 consecutive failures observed"]
      verdict_signal: REPROCESS
pipeline_run_id: PR-5201
```

### B

```
node_07_selection:
  - ASVS-V6-Auth-Arbiter (V6.3 MFA signals)
  - CWE-Authn-Arbiter (CWE-307 brute force signals)

node_08_verdicts:
  ASVS-V6-Auth-Arbiter:
    verdict: PASS
    satisfied: [V6.3.1, V6.3.2]
    note: "MFA enforced for admin users — V6 satisfied. MFA enforcement does not protect against brute force on the pre-MFA login step."

  CWE-Authn-Arbiter:
    verdict: REPROCESS
    cwe_id: CWE-307
    severity: HIGH
    gap: "No lockout or rate limiting on login endpoint. MFA adds a second factor but an attacker can still enumerate valid usernames and passwords at the first-factor login step before MFA is triggered."
    action_required: "Add account lockout and rate limiting on the first-factor login step."

overall_verdict: REPROCESS
```

### Reason

MFA and brute-force protection are complementary, not substitutes. The team has MFA enforced (V6.3 PASS) but no lockout. An attacker can brute-force the first factor (password) to discover valid credentials, then face the MFA step as a second barrier — but the first factor is still unprotected. REPROCESS because CWE-307 applies to the pre-MFA credential submission step. MFA PASS does not compensate for the absent lockout.

### Phrasing B

```
PR-5201. V6.3 MFA enforced for admins (PASS). Login endpoint: no lockout (CWE-307).
Does admin MFA enforcement satisfy the brute-force concern?
```

Expected: No. Two independent findings. ASVS-V6 PASS (MFA enforcement). CWE-Authn REPROCESS (no lockout on first-factor login). MFA ≠ brute-force protection. Overall: REPROCESS.

---

## Pair CB4-3.4-190

**Node:** 04 + 08 (focus)
**Stratum:** B (team context — V8.3 admin access + SSDF PO.1 training gap: admins have access but aren't trained)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
territories:
  - T-asvs-admin:
    domain: ASVS-V8
    findings:
      - "V8.3.1: admin role check on /admin/* — satisfied"
      - "V8.3.2: admin action audit log present — satisfied"
  - T-ssdf-po1:
    domain: SSDF-PO
    findings:
      - "PO.1: security training for admin users: general secure coding only"
      - "no role-specific training for admins on privilege management, audit log review, or incident response"
      - "12 admin users, all with broad data access"
pipeline_run_id: PR-5212
```

### B

```
node_08:
  ASVS-V8-AuthZ-Arbiter:
    verdict: PASS
    satisfied: [V8.3.1, V8.3.2]

  SSDF-PO-Arbiter:
    verdict: CONCERN
    practice: PO.1
    gap: "12 admin users have broad data access but have received only general secure coding training. No role-specific training on privilege management, recognizing insider threats, or reviewing audit logs for anomalies."
    action_recommended: "Provide admin-specific security training covering: privilege management, audit log review procedures, and incident response for privilege abuse. Target: completed within 60 days."

overall_verdict: CONCERN
note: "ASVS V8 technical controls are in place. The SSDF PO.1 gap means the humans operating those controls lack specialized training — the controls can be bypassed or misused without training awareness."
```

### Reason

ASVS V8 technical controls are satisfied (access control + audit log). The SSDF PO.1 gap is orthogonal: the humans who hold admin privileges lack specialized training. Technical controls alone are insufficient if the people operating them don't understand their security implications. Overall CONCERN (V8 PASS + PO.1 CONCERN). The note makes the relationship explicit: controls in place, operators undertrained.

### Phrasing B

```
PR-5212. ASVS V8.3.1+V8.3.2 both satisfied. SSDF PO.1: 12 admins with broad access, general training only.
Two arbiters. Verdicts?
```

Expected: ASVS-V8 PASS. SSDF-PO CONCERN. Overall CONCERN. Note: technical controls present; admin operator training absent.

---

## Pair CB4-3.4-191

**Node:** 01 + 08 (focus)
**Stratum:** C (depth — CWE-352 CSRF + V7 session: cross-site attack exploits the session)
**Source:** CWE Top 25 2023, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
submission:
  system: "PortalApp v3.5"
  claims:
    - "POST /api/account/settings: no CSRF token, no SameSite (V7-related)"
    - "V7.2.1: session tokens over TLS — confirmed"
    - "V7.3.1: session invalidated on logout — confirmed"
pipeline_run_id: PR-5223
```

### B

```
node_01:
  T-cwe-authz: [CWE-352, POST /api/account/settings, no CSRF, no SameSite]
  T-asvs-session: [V7.2.1, V7.3.1, session token, TLS, logout]

node_08:
  CWE-Authz-Arbiter:
    verdict: REPROCESS
    cwe_id: CWE-352
    severity: HIGH
    gap: "No CSRF token, no SameSite on session cookie. Account settings endpoint performs state-changing operations."
    action_required: "Add CSRF token or set SameSite=Strict on session cookie."

  ASVS-V7-Session-Arbiter:
    verdict: PASS
    satisfied: [V7.2.1, V7.3.1]
    note: "Session TLS and logout invalidation are separately satisfied. The CWE-352 finding relates to CSRF attack on the session, not to the session transport or invalidation mechanisms. V7 does not have a specific CSRF token requirement — CWE-352 covers it."

overall_verdict: REPROCESS
```

### Reason

Two territories: CWE-352 CSRF (REPROCESS) and ASVS V7 session (PASS). The ASVS V7 PASS covers session transport and invalidation. CWE-352 REPROCESS covers the CSRF attack vector that exploits the session cookie. The two findings are complementary: V7 controls are in place, but CSRF protection is absent — an attacker can exploit the valid session via cross-origin request. REPROCESS dominates.

### Phrasing B

```
PortalApp: session TLS and logout confirmed (V7). But no CSRF token on /api/account/settings, no SameSite.
Node 01 splits. What do the two arbiters return?
```

Expected: ASVS-V7 PASS (TLS + logout). CWE-Authz REPROCESS (CWE-352, no CSRF protection). Overall REPROCESS.

---

## Pair CB4-3.4-192

**Node:** 08 (focus)
**Stratum:** C (depth — RFC 8628 device flow PASS + V10.3 token revocation CONCERN, interaction)
**Source:** IETF RFC 8628, OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter_inputs:
  - arbiter: RFC-OAuth2-Arbiter
    territory: T-rfc-device-flow
    findings:
      - "RFC 8628: device_code, user_code, 5s poll, 10min expiry — all confirmed"
      - "error:expired_token on expired device_code — confirmed"
  - arbiter: ASVS-V10-OAuth-Arbiter
    territory: T-asvs-v10-3
    findings:
      - "V10.3.1: /oauth/revoke endpoint — present"
      - "V10.3.2: access tokens revocable — confirmed"
      - "V10.3.3: refresh tokens: issued by device flow; 7-day validity; no revocation"
pipeline_run_id: PR-5234
```

### B

```
arbiter_results:
  RFC-OAuth2-Arbiter:
    verdict: PASS
    satisfied: [RFC8628-§3.2, RFC8628-§3.3, RFC8628-§3.5]

  ASVS-V10-OAuth-Arbiter:
    verdict: CONCERN
    satisfied: [V10.3.1, V10.3.2]
    gap_V10_3_3: "Refresh tokens issued via device flow have no revocation mechanism; valid 7 days. If a TV/IoT device is lost or stolen, its refresh token cannot be invalidated."
    action_recommended: "Implement refresh token revocation. Provide a device management interface (e.g., account settings page) where users can revoke access for specific devices by invalidating associated refresh tokens."
    note: "Device context: the inability to revoke refresh tokens is especially concerning for IoT/TV devices which may be lost, sold, or shared."

overall_verdict: CONCERN
```

### Reason

RFC 8628 device flow PASS + V10.3.3 CONCERN. The device context (IoT/TV) adds specific risk to the refresh token revocation gap: devices may be sold, lost, or shared — without revocation, a device retains API access for up to 7 days post-loss. The action_recommended adds the device management interface as the practical remediation path for this context.

### Phrasing B

```
PR-5234. RFC 8628 device flow all confirmed. V10.3.1+V10.3.2 satisfied. V10.3.3: device flow refresh tokens non-revocable, 7-day.
IoT TV device context. Overall verdict?
```

Expected: RFC PASS. V10 CONCERN (V10.3.3). Overall CONCERN. Device loss/theft context noted. Action: device management interface for refresh token revocation.

---

## Pair CB4-3.4-193

**Node:** 07 + 08 (focus)
**Stratum:** C (depth — SSDF PO.3 SBOM gap + CWE-862 IDOR: supply chain + runtime combined)
**Source:** NIST SP 800-218 SSDF v1.1, CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
consolidated_findings:
  territories:
    - T-ssdf-po3:
      findings: ["No SBOM maintained. 47 third-party libraries in auth service. Snyk SCA present (satisfies PO.2)."]
      verdict_signal: CONCERN
    - T-cwe-authz:
      findings: ["GET /api/user-data/{user_id}: no ownership check; any authenticated user can retrieve any user's data"]
      verdict_signal: REPROCESS
pipeline_run_id: PR-5245
```

### B

```
node_07: SSDF-PO-Arbiter (PO.3 signal) + CWE-Authz-Arbiter (CWE-862 signal)

node_08:
  SSDF-PO-Arbiter:
    verdict: CONCERN
    practice: PO.3
    gap: "No SBOM for 47 third-party libraries. Snyk (PO.2) noted; SBOM (PO.3) absent."
    action_recommended: "Generate SBOM in CycloneDX format and maintain it."

  CWE-Authz-Arbiter:
    verdict: REPROCESS
    cwe_id: CWE-862
    severity: HIGH
    gap: "No ownership check on /api/user-data/{user_id}. Any authenticated user can access any user's data."
    action_required: "Add ownership check before returning user data."

overall_verdict: REPROCESS
note: "Two independent findings from different source families. PO.3 CONCERN (supply chain visibility) and CWE-862 REPROCESS (runtime authorization gap) are separate concerns. The REPROCESS dominates the overall verdict."
```

### Reason

SSDF PO.3 CONCERN (supply chain/SBOM gap) and CWE-862 REPROCESS (runtime authorization gap) are from different source families and address different aspects of security. REPROCESS dominates. The note makes explicit that these are independent findings — a team that fixes CWE-862 still needs to address PO.3.

### Phrasing B

```
PR-5245. SSDF PO.3: no SBOM (CONCERN). CWE-862: no ownership check on user data endpoint (REPROCESS).
Two arbiters. Overall verdict?
```

Expected: SSDF-PO CONCERN. CWE-Authz REPROCESS. Overall REPROCESS. Note: independent findings; both must be addressed.

---

## Pair CB4-3.4-194

**Node:** 01 + 04 (focus)
**Stratum:** D (adversarial — V7 session fixation + V9 JWT: team uses JWT as session token and argues V7 doesn't apply)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
submission:
  claims:
    - "Authentication: JWT stored in httpOnly cookie; used to track session state"
    - "JWT algorithm: RS256, exp=8h"
    - "session ID: not applicable — we use JWT, not session IDs"
    - "V9.1.1 and V9.2.1 cited as applicable requirements"
    - "V7 not applicable per team: 'we don't use session IDs'"
  sub_phase: "3.4"
```

### B

```
node_01:
  T-jwt-session: [JWT as session token, httpOnly cookie, session state, RS256, exp=8h]
  note: "JWT used as session mechanism → V7 is the governing chapter (session management), not V9 (self-contained tokens for delegation)"

node_04:
  T-jwt-session:
    V7.2.3: "session ID regenerated after login? JWT reissued after login? No — same JWT reused from authentication, no regeneration on login event"
    V7.2.1: "JWT transmitted over TLS (httpOnly cookie + HTTPS) — satisfied"
    V7.3.1: "session invalidated on logout? JWT invalidation on logout: not mentioned — requires investigation"
    V9.1.1: "RS256 — this is about the token mechanism; relevant but V7 governs the session aspect"
    V9.2.1: "8h expiry — for a session token, 8h is within session timeout norms; V7.2.x timeout requirements govern"
  verdict_signal: CONCERN/REPROCESS — V7.2.3 gap confirmed (no regeneration), V7.3.1 unclear

node_08:
  ASVS-V7-Session-Arbiter:
    verdict: REPROCESS
    requirements_violated: [V7.2.3]
    gap: "JWT used as session token must still satisfy V7 session management requirements. V7.2.3: same JWT reused from authentication — no session token regeneration on login. Session fixation vulnerability applies to JWT session tokens as well as traditional session IDs."
    action_required: "Issue a new JWT after successful authentication (do not reuse the authentication JWT as the session JWT). Implement server-side JWT revocation on logout."
    note: "Team's argument that V7 does not apply because 'we don't use session IDs' is incorrect. V7 governs session management for any mechanism used to maintain session state, including JWT-based sessions. V9 applies to non-session JWT delegation tokens."
```

### Reason

This pair specifically addresses the team's argument that V7 doesn't apply to JWT sessions. V7 governs session management regardless of the mechanism (session ID vs. JWT). A JWT used to maintain session state is a session token under V7. Session fixation (V7.2.3) applies: the same JWT from authentication is reused as the session token — equivalent to not regenerating a session ID after login. REPROCESS.

### Phrasing B

```
Team uses JWT (RS256, httpOnly cookie) as session token. Says V7 doesn't apply; cites V9.1.1+V9.2.1.
Same JWT reused after login — no reissuance.
Does V7 apply? What is the verdict?
```

Expected: V7 applies to JWT session tokens. V7.2.3 violated (JWT not reissued after login = session fixation). REPROCESS. Team's V7-exemption argument rejected. Note: V9 applies to delegation tokens, V7 governs session state maintenance.

---

## Pair CB4-3.4-195

**Node:** 08 (focus)
**Stratum:** B (team context — V8.1.2 default DENY confirmed: deny-by-default policy with allow-list)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** PASS

### A

```
arbiter_inputs:
  - arbiter: ASVS-V8-AuthZ-Arbiter
    findings:
      - "V8.1.1: per-resource ownership check on all user data endpoints — confirmed"
      - "V8.1.2: authorization policy is default DENY; all endpoint access requires explicit ALLOW grant in permission registry"
      - "V8.3.1: /admin/* requires admin role — confirmed"
  - arbiter: SSDF-PW-Arbiter
    findings:
      - "PW.2: threat model updated 2026-03-10 — 2 months ago; covers authorization flows"
      - "PW.8: code review mandatory for authorization-related changes"
pipeline_run_id: PR-5256
```

### B

```
arbiter_results:
  ASVS-V8-AuthZ-Arbiter:
    verdict: PASS
    satisfied: [V8.1.1, V8.1.2, V8.3.1]
    summary: "Ownership check, deny-by-default policy, and admin role enforcement all satisfied."

  SSDF-PW-Arbiter:
    verdict: PASS
    satisfied: [PW.2.1, PW.8.2]
    summary: "Threat model current; code review process for authorization changes documented."

overall_verdict: PASS
```

### Reason

All V8 requirements satisfied. SSDF PW.2 and PW.8 both satisfied. Importantly, V8.1.2 is confirmed satisfied (default DENY with explicit ALLOW registry) — this is the correct counterpart to pair 188 (default ALLOW with deny list = REPROCESS). The training establishes both the PASS case (correct policy) and the REPROCESS case (inverted policy) for V8.1.2.

### Phrasing B

```
PR-5256. ASVS V8.1.1+V8.1.2+V8.3.1: ownership, deny-by-default, admin role — all confirmed.
SSDF PW.2+PW.8 satisfied.
Verdict?
```

Expected: Both arbiters PASS. Overall PASS. V8.1.2 confirmation: default DENY + explicit ALLOW registry = correct policy (contrast with pair 188's deny-list REPROCESS).

---

## Pair CB4-3.4-196

**Node:** 07 + 08 (focus)
**Stratum:** D (adversarial — M2M client_credentials: MFA requirement does not apply)
**Source:** OWASP ASVS 5.0.0
**Verdict:** PASS (scoped)

### A

```
consolidated_findings:
  territories:
    - T-asvs-mfa:
      findings: ["V6.3.1: MFA not implemented; system is machine-to-machine API with no human users"]
      verdict_signal: "V6.3 not applicable — no human authentication"
    - T-oauth-oidc:
      findings: ["V10.2.1: client_credentials grant for M2M", "V10.4.6: PKCE not applicable (no auth code flow)"]
      verdict_signal: PASS (scoped)
pipeline_run_id: PR-5267
```

### B

```
node_07: ASVS-V6-Auth-Arbiter (MFA signal) + ASVS-V10-OAuth-Arbiter (V10 signal)

node_08:
  ASVS-V6-Auth-Arbiter:
    verdict: PASS
    note: "V6.3 (MFA) requires a second authentication factor for human user authentication. Machine-to-machine (M2M) service authentication does not involve human users — V6.3 MFA requirements are not applicable to client_credentials flows. Assessment scope: M2M service authentication only."

  ASVS-V10-OAuth-Arbiter:
    verdict: CONCERN
    note: "V10.2.1 (authorization code flow + PKCE) governs user-delegated authorization. client_credentials is a service-to-service grant type — V10.2.1 does not apply. Correct V10 requirements for M2M: V10.1.x (general OAuth requirements) apply. PKCE absence is correct for client_credentials."
    action_recommended: "Re-scope V10 assessment to the applicable requirements for client_credentials grant type. If user-delegated authorization is also needed in the future, implement authorization code + PKCE separately."

overall_verdict: CONCERN
note: "The submitted scope (V6.3 and V10.2.1) contains requirements that do not apply to the M2M use case. Verdicts are based on correct requirement scoping."
```

### Reason

MFA (V6.3) applies to human user authentication. M2M service authentication does not involve human users — V6.3 is not applicable. The CONCERN is from the V10.2.1 scoping issue (client_credentials ≠ V10.2.1's user-delegated flow), reinforcing pair 179. This pair establishes that citing inapplicable requirements produces a scoping CONCERN, not a REPROCESS.

### Phrasing B

```
M2M API service authentication: client_credentials grant. No human users.
Team cites V6.3 (MFA) and V10.2.1 (auth code flow).
Do these requirements apply?
```

Expected: V6.3 not applicable (M2M, no human users) = PASS on scoping. V10.2.1 not applicable to client_credentials = CONCERN on requirement scoping. Overall CONCERN (scoping issue). Not REPROCESS (controls are appropriate for M2M; citations are wrong).

---

## Pair CB4-3.4-197

**Node:** 08 (focus)
**Stratum:** C (depth — CWE-307 lockout + CWE-862 IDOR: two CWE vulnerabilities in one service)
**Source:** CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter_inputs:
  - arbiter: CWE-Authn-Arbiter
    territory: T-cwe-authn
    findings:
      - "POST /api/auth/login: 5 failed attempts triggers CAPTCHA (partial control)"
      - "no account lockout after CAPTCHA challenge"
  - arbiter: CWE-Authz-Arbiter
    territory: T-cwe-authz
    findings:
      - "GET /api/messages/{message_id}: no ownership check; any authenticated user can read any message"
      - "sequential integer message IDs; IDOR confirmed"
pipeline_run_id: PR-5278
```

### B

```
arbiter_results:
  CWE-Authn-Arbiter:
    verdict: CONCERN
    cwe_id: CWE-307
    severity: MEDIUM
    gap: "CAPTCHA after 5 failures — partial brute force mitigation. No account lockout."
    action_recommended: "Add account lockout after CAPTCHA bypass or persistent failures."

  CWE-Authz-Arbiter:
    verdict: REPROCESS
    cwe_id: CWE-862
    severity: HIGH
    gap: "No ownership check on /api/messages/{message_id}. Sequential IDs enable enumeration."
    action_required: "Add ownership check before returning message content."

overall_verdict: REPROCESS
note: "CWE-307 CONCERN (partial brute force mitigation) and CWE-862 REPROCESS (absent authorization check) are independent CWE findings. REPROCESS dominates. Both findings must be addressed."
```

### Reason

Two CWE findings: CWE-307 CONCERN (CAPTCHA present but incomplete) and CWE-862 REPROCESS (no authorization check). Both are in the CWE territory but different sub-arbiters (CWE-Authn vs CWE-Authz). REPROCESS dominates. This reinforces that two CWE findings can come from one service and produce different severity verdicts from different CWE arbiters.

### Phrasing B

```
PR-5278. CWE-307: CAPTCHA after 5 failures, no lockout. CWE-862: no ownership check on messages.
CWE-Authn and CWE-Authz arbiters. Verdicts?
```

Expected: CWE-Authn CONCERN (CWE-307, CAPTCHA = partial). CWE-Authz REPROCESS (CWE-862, no ownership check). Overall REPROCESS.

---

## Pair CB4-3.4-198

**Node:** 08 (focus)
**Stratum:** C (depth — full new-domain all-PASS baseline: V6.3+V7+V8+CWE-clean+SSDF-clean)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023, NIST SP 800-218 SSDF v1.1
**Verdict:** PASS

### A

```
arbiter_inputs:
  - ASVS-V6-Auth-Arbiter: [V6.2.3+V6.2.5+V6.3.1+V6.3.2 all satisfied]
  - ASVS-V7-Session-Arbiter: [V7.2.1+V7.2.3+V7.3.1 all satisfied — TLS, regeneration on login, logout invalidation]
  - ASVS-V8-AuthZ-Arbiter: [V8.1.1+V8.1.2+V8.3.1+V8.3.2 all satisfied — ownership, deny-by-default, admin role, audit log]
  - CWE-Authn-Arbiter: [no CWE-287 or CWE-307 pattern — MFA enforced + lockout present]
  - CWE-Authz-Arbiter: [no CWE-862 or CWE-269 or CWE-352 pattern — ownership checks, deny-by-default, CSRF tokens present]
  - SSDF-PO-Arbiter: [PO.1 role-specific training + PO.2 SCA + PO.3 SBOM all present]
  - SSDF-PW-Arbiter: [PW.1.1+PW.2.1+PW.4.1+PW.8.2 all satisfied]
pipeline_run_id: PR-5289
```

### B

```
arbiter_results: all seven arbiters return PASS

overall_verdict: PASS
arbiters_run: 7
arbiters_passed: 7
scope_boundary: "Authentication (V6.2+V6.3), session management (V7), authorization (V8), CWE authentication/authorization weakness patterns, SSDF PO and PW practices assessed. Token (V9), OAuth (V10), RFC normative not in scope for this run."
```

### Reason

Seven-arbiter all-PASS baseline for the extended domain set. This establishes what a fully compliant system looks like across ASVS V6/V7/V8, CWE-Authn/Authz, and SSDF PO/PW. The scope boundary is explicit: V9, V10, and RFC not in this run. All new domains (V6.3 MFA, V7.2.3 session fixation, V8.1.2 deny-by-default, V8.3.2 audit log, CWE-307 lockout, CWE-352 CSRF, SSDF PO.1/PO.3) must be satisfied for all-PASS.

### Phrasing B

```
PR-5289. Seven arbiters: V6 (V6.2+V6.3), V7 (TLS+regen+logout), V8 (ownership+deny-by-default+admin+audit), CWE-Authn (MFA+lockout), CWE-Authz (ownership+CSRF tokens), SSDF-PO (PO.1+PO.2+PO.3), SSDF-PW (PW.1+PW.2+PW.4+PW.8).
All confirmed. Final verdict?
```

Expected: All seven PASS. Overall PASS. Scope boundary excludes V9/V10/RFC (not in this run).

---

## File summary

| Seq | Stratum | Sources | Verdict | Key cross-source interaction |
|-----|---------|---------|---------|------------------------------|
| 189 | C | ASVS+CWE | REPROCESS | V6.3 MFA PASS + CWE-307 REPROCESS: MFA ≠ brute-force protection |
| 190 | B | ASVS+SSDF | CONCERN | V8.3 technical PASS + SSDF PO.1 CONCERN: controls work, operators undertrained |
| 191 | C | CWE+ASVS | REPROCESS | CWE-352 CSRF + V7.2.1/V7.3.1 PASS: CSRF exploits valid session |
| 192 | C | IETF+ASVS | CONCERN | RFC 8628 PASS + V10.3.3 CONCERN: device loss context for refresh token gap |
| 193 | C | SSDF+CWE | REPROCESS | SSDF PO.3 CONCERN + CWE-862 REPROCESS: supply chain + runtime combined |
| 194 | D | ASVS | REPROCESS | V7 applies to JWT sessions; V7.2.3 fixation gap; V7-exemption argument rejected |
| 195 | B | ASVS+SSDF | PASS | V8.1.2 deny-by-default confirmed + SSDF PW satisfied |
| 196 | D | ASVS | CONCERN | M2M client_credentials: V6.3 MFA and V10.2.1 not applicable; scoping CONCERN |
| 197 | C | CWE | REPROCESS | CWE-307 CONCERN + CWE-862 REPROCESS: two CWE findings, different arbiters |
| 198 | C | ASVS+CWE+SSDF | PASS | Seven-arbiter all-PASS baseline for extended domain set |

**Running total: 396 instances (sequences 001–198)**
Stratum breakdown: A ~28%, B ~24%, C ~32%, D ~16%
