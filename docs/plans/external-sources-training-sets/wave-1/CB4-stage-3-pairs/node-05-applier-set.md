# Node 5 — Applier Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-051 through CB4-3.4-055
# Base pairs: 5 × 2 phrasings = 10 pair instances
# Node role: applies domain processor finding to system context; produces contextualised result
# Production date: 2026-05-05

---

## Coverage map

| Pair | Domain processor input | Applied result | Source | Stratum |
|------|----------------------|----------------|--------|---------|
| 051 | T-auth PASS finding | PASS confirmed for production controls | OWASP-ASVS | A |
| 052 | T-oauth-oidc REPROCESS | REPROCESS contextualised for SPA deployment | OWASP-ASVS | D |
| 053 | T-session CONCERN | CONCERN contextualised — ASVS vs. internal standard | OWASP-ASVS | D |
| 054 | T-cwe-authz REPROCESS | REPROCESS contextualised for specific API endpoint | CWE-TOP25 | D |
| 055 | T-process-evidence CONCERN | CONCERN contextualised — PW.2 gap with open findings | NIST-SSDF | D |

---

## Pairs

---

### Pair CB4-3.4-051 — T-auth PASS applied to production authentication system (APPLY)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Domain processor result for T-auth: PASS — V6.2.1 satisfied (password 15 chars at
write time), V6.5.5 satisfied (MFA for admin), V6.2.3 satisfied (bcrypt cost 12),
V6.5.2 satisfied (lockout after 10 failures). Apply finding to system context: backend
authentication service in production. Confirm applicability of each control to the
production system and produce applied result.

B: APPLIED_RESULT: T-auth. PASS — confirmed applicable to production system.
  V6.2.1: password length enforced at write time — applies to all user account creation
    and password change operations in the production system.
  V6.5.5: MFA for admin operations — applies at the admin operations entry point; all
    admin-privileged API calls require MFA-validated session.
  V6.2.3: bcrypt cost 12 in production credential store — confirmed for stored credentials,
    not cached in memory or transmitted.
  V6.5.2: lockout after 10 failures — applied at the authentication service level,
    not only at the gateway.
Scope boundary: T-auth finding applies to the backend authentication service as submitted.
It does not extend to unsubmitted services (e.g., partner integrations, internal tooling).
Confidence: HIGH — all four requirements have specific, verifiable production evidence.

Reason: The Applier confirms that each domain processor finding maps to an actual
production system control, not a policy document or intended behavior. Scope boundaries
prevent the PASS from being interpreted as system-wide when only the backend auth service
was submitted.

---

### Pair CB4-3.4-051 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: T-auth domain processor result: PASS, V6.2.1/V6.5.5/V6.2.3/V6.5.2 satisfied.
Context: backend authentication service, production. Apply and confirm.

B: APPLIED_RESULT: T-auth. PASS — confirmed applicable to production system.
  V6.2.1: password length enforced at write time — applies to all user account creation
    and password change operations in the production system.
  V6.5.5: MFA for admin operations — applies at the admin operations entry point; all
    admin-privileged API calls require MFA-validated session.
  V6.2.3: bcrypt cost 12 in production credential store — confirmed for stored credentials,
    not cached in memory or transmitted.
  V6.5.2: lockout after 10 failures — applied at the authentication service level,
    not only at the gateway.
Scope boundary: T-auth finding applies to the backend authentication service as submitted.
It does not extend to unsubmitted services (e.g., partner integrations, internal tooling).
Confidence: HIGH — all four requirements have specific, verifiable production evidence.

---

### Pair CB4-3.4-052 — T-oauth-oidc REPROCESS applied to SPA context (APPLY)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Domain processor result for T-oauth-oidc: REPROCESS — ASVS_10_4_6_PLAIN_METHOD_REJECTED.
code_challenge_method='plain' transmits unhashed verifier. System context: single-page
application (SPA) using OAuth 2.0. Apply finding to SPA deployment context and produce
contextualised result.

B: APPLIED_RESULT: T-oauth-oidc. REPROCESS — contextualised for SPA deployment.
  Finding in context: the SPA sends code_challenge_method='plain' in its authorization
    requests. In a SPA (public client), the authorization request is visible in the
    browser redirect — the plain code_challenge is exposed in the browser URL bar and
    HTTP logs.
  Impact in SPA context: an attacker who intercepts the authorization request
    (e.g., via malicious browser extension, shared device logs, or HTTP proxy) can
    extract the code_challenge and derive the verifier directly, then exchange the
    intercepted authorization code for tokens.
  S256 in SPA context: S256 is supported by all modern authorization servers and all
    modern browser OAuth libraries. There is no technical infeasibility argument for
    plain in a SPA context.
  Remediation scoped to SPA: update the OAuth client library configuration to use
    S256; update the authorization server to reject plain for this client type.
Confidence: HIGH — plain method violation is deterministic; SPA context increases impact.

Reason: The Applier adds the deployment context that makes the impact concrete. A plain
method violation in a SPA is more exposed than in a server-side client because the
authorization URL is browser-visible. This context informs the remediation scope.

---

### Pair CB4-3.4-052 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: T-oauth-oidc REPROCESS: plain method, ASVS_10_4_6_PLAIN_METHOD_REJECTED. Context:
SPA (public client). Apply to SPA context.

B: APPLIED_RESULT: T-oauth-oidc. REPROCESS — contextualised for SPA deployment.
  Finding in context: the SPA sends code_challenge_method='plain' in its authorization
    requests. In a SPA (public client), the authorization request is visible in the
    browser redirect — the plain code_challenge is exposed in the browser URL bar and
    HTTP logs.
  Impact in SPA context: an attacker who intercepts the authorization request
    (e.g., via malicious browser extension, shared device logs, or HTTP proxy) can
    extract the code_challenge and derive the verifier directly, then exchange the
    intercepted authorization code for tokens.
  S256 in SPA context: S256 is supported by all modern authorization servers and all
    modern browser OAuth libraries. There is no technical infeasibility argument for
    plain in a SPA context.
  Remediation scoped to SPA: update the OAuth client library configuration to use
    S256; update the authorization server to reject plain for this client type.
Confidence: HIGH — plain method violation is deterministic; SPA context increases impact.

---

### Pair CB4-3.4-053 — T-session CONCERN applied to internal-standard context (APPLY)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: Domain processor result for T-session: CONCERN — ASVS 7.3.1 satisfied (20-minute
timeout with RA-AUTH-003), internal standard requires 15 minutes, deviation acknowledged.
Apply finding to context: team has risk analysis on file, designated approver not yet
confirmed. Produce contextualised result.

B: APPLIED_RESULT: T-session. CONCERN — contextualised for internal standard deviation.
  ASVS compliance state: CONFIRMED. V7.3.1 is satisfied. The 20-minute idle timeout
    with documented risk analysis RA-AUTH-003 meets the ASVS requirement.
  Internal standard deviation in context: the team's policy document requires 15 minutes
    maximum. The 5-minute deviation (20 vs. 15) is risk-accepted in RA-AUTH-003 but
    RA-AUTH-003 does not yet carry a named approver/reviewer signature.
  Blocking condition for CONCERN clearance: RA-AUTH-003 requires approver/reviewer
    sign-off to be treated as formally accepted. Without sign-off, the risk acceptance
    is informal.
  Concern severity: LOW — ASVS is satisfied; only internal governance step is missing.
    The security posture is not degraded relative to the ASVS baseline.
Resolution: either (a) obtain approver sign-off on RA-AUTH-003 within standard governance
  cycle, or (b) reduce timeout to 15 minutes to remove deviation entirely.
Confidence: HIGH — ASVS satisfaction is clear; internal standard gap is precisely scoped.

Reason: The Applier distinguishes the ASVS compliance state (CONFIRMED) from the internal
governance gap. The CONCERN rating reflects the governance gap, not a security defect.
The severity is LOW because the underlying security control is present and appropriate.

---

### Pair CB4-3.4-053 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: D

A: T-session CONCERN: V7.3.1 satisfied, 20-min timeout with RA-AUTH-003, internal
standard 15-min, no approver signature yet. Apply to context.

B: APPLIED_RESULT: T-session. CONCERN — contextualised for internal standard deviation.
  ASVS compliance state: CONFIRMED. V7.3.1 is satisfied. The 20-minute idle timeout
    with documented risk analysis RA-AUTH-003 meets the ASVS requirement.
  Internal standard deviation in context: the team's policy document requires 15 minutes
    maximum. The 5-minute deviation (20 vs. 15) is risk-accepted in RA-AUTH-003 but
    RA-AUTH-003 does not yet carry a named approver/reviewer signature.
  Blocking condition for CONCERN clearance: RA-AUTH-003 requires approver/reviewer
    sign-off to be treated as formally accepted. Without sign-off, the risk acceptance
    is informal.
  Concern severity: LOW — ASVS is satisfied; only internal governance step is missing.
    The security posture is not degraded relative to the ASVS baseline.
Resolution: either (a) obtain approver sign-off on RA-AUTH-003 within standard governance
  cycle, or (b) reduce timeout to 15 minutes to remove deviation entirely.
Confidence: HIGH — ASVS satisfaction is clear; internal standard gap is precisely scoped.

---

### Pair CB4-3.4-054 — T-cwe-authz REPROCESS applied to specific API endpoint (APPLY)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Domain processor result for T-cwe-authz: REPROCESS — CWE_862_IDOR_CONFIRMED.
GET /api/v1/users/{userId}/profile, JWT required, no sub-to-userId check, any
authenticated user unrestricted. Apply to system context: user profile service in
production, 50,000 active user accounts. Produce contextualised result.

B: APPLIED_RESULT: T-cwe-authz. REPROCESS — contextualised for user profile service.
  Finding in context: any of the 50,000 active user accounts can enumerate and read
    any other user's profile by iterating userId values. The only constraint is a valid
    JWT — which any authenticated user holds.
  Data at risk: user profile data at GET /api/v1/users/{userId}/profile — the content
    of this endpoint determines privacy impact. If profile data includes email, phone,
    or sensitive attributes, this is a privacy exposure at scale.
  Attack complexity: LOW. No privilege escalation required; any authenticated user
    can execute this. No brute-force or token forgery needed.
  Related endpoints to check: other endpoints using userId or similar direct-object
    reference patterns (e.g., /api/v1/users/{userId}/settings, /api/v1/orders/{orderId})
    should be reviewed for the same ownership check gap.
Confidence: HIGH — ownership check absence is deterministic from submitted code description.
Remediation priority: HIGH (confirmed authorization weakness, low attack complexity).

Reason: The Applier adds population size, data-at-risk assessment, and attack complexity
to give the REPROCESS actionable context beyond the CWE classification alone.

---

### Pair CB4-3.4-054 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: T-cwe-authz REPROCESS: CWE_862_IDOR_CONFIRMED, profile endpoint, JWT only, no
ownership check. Context: 50,000 active users. Apply.

B: APPLIED_RESULT: T-cwe-authz. REPROCESS — contextualised for user profile service.
  Finding in context: any of the 50,000 active user accounts can enumerate and read
    any other user's profile by iterating userId values. The only constraint is a valid
    JWT — which any authenticated user holds.
  Data at risk: user profile data at GET /api/v1/users/{userId}/profile — the content
    of this endpoint determines privacy impact. If profile data includes email, phone,
    or sensitive attributes, this is a privacy exposure at scale.
  Attack complexity: LOW. No privilege escalation required; any authenticated user
    can execute this. No brute-force or token forgery needed.
  Related endpoints to check: other endpoints using userId or similar direct-object
    reference patterns (e.g., /api/v1/users/{userId}/settings, /api/v1/orders/{orderId})
    should be reviewed for the same ownership check gap.
Confidence: HIGH — ownership check absence is deterministic from submitted code description.
Remediation priority: HIGH (confirmed authorization weakness, low attack complexity).

---

### Pair CB4-3.4-055 — T-process-evidence CONCERN applied to development timeline (APPLY)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: Domain processor result for T-process-evidence: CONCERN — PW.2 timing gap
(DR-AUTHZ-2026-04 at week 8, implementation completed week 6), two unresolved findings
(token expiry in s2s flow, rate limiting). Apply to context: authorization module, sprint
cycle now at week 10, team planning next sprint. Produce contextualised result.

B: APPLIED_RESULT: T-process-evidence. CONCERN — contextualised for sprint 10 context.
  SSDF PW compliance state:
    PW.1: SATISFIED — no action needed.
    PW.2: TIMING GAP (post-implementation design review) + 2 unresolved findings.
      Current week: 10. The gap occurred at week 8 and cannot be retroactively resolved
      by conducting a new pre-implementation review — implementation is complete.
      Remediation path: produce supplemental design review document DR-AUTHZ-2026-05
      that formally closes the two open findings from DR-AUTHZ-2026-04.
      This does not resolve the timing gap (which is historical) but does satisfy the
      spirit of PW.2 by ensuring findings are formally addressed.
    PW.8: SATISFIED — no action needed.
  Sprint 10 action items:
    1. Implement fix for Finding 1 (token expiry in s2s flow) — scheduled in sprint 10.
    2. Implement fix for Finding 2 (rate limiting on token endpoint) — scope sprint 10.
    3. Produce DR-AUTHZ-2026-05 after both fixes are verified.
    4. Re-submit with updated artifact set.
Confidence: HIGH — timing violation and open findings are from submitted documents.

Reason: The Applier translates the SSDF finding into actionable sprint-level items and
clarifies that the timing gap cannot be retroactively resolved — only its impact (open
findings) can be remediated. This prevents the team from misinterpreting the resolution
path as requiring a redo of past history.

---

### Pair CB4-3.4-055 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: D

A: T-process-evidence CONCERN: PW.2 timing gap (wk8 > wk6), 2 open findings (token
expiry s2s, rate limiting). Context: currently week 10, team planning sprint. Apply.

B: APPLIED_RESULT: T-process-evidence. CONCERN — contextualised for sprint 10 context.
  SSDF PW compliance state:
    PW.1: SATISFIED — no action needed.
    PW.2: TIMING GAP (post-implementation design review) + 2 unresolved findings.
      Current week: 10. The gap occurred at week 8 and cannot be retroactively resolved
      by conducting a new pre-implementation review — implementation is complete.
      Remediation path: produce supplemental design review document DR-AUTHZ-2026-05
      that formally closes the two open findings from DR-AUTHZ-2026-04.
      This does not resolve the timing gap (which is historical) but does satisfy the
      spirit of PW.2 by ensuring findings are formally addressed.
    PW.8: SATISFIED — no action needed.
  Sprint 10 action items:
    1. Implement fix for Finding 1 (token expiry in s2s flow) — scheduled in sprint 10.
    2. Implement fix for Finding 2 (rate limiting on token endpoint) — scope sprint 10.
    3. Produce DR-AUTHZ-2026-05 after both fixes are verified.
    4. Re-submit with updated artifact set.
Confidence: HIGH — timing violation and open findings are from submitted documents.

---

## Node 5 summary

Pairs produced: CB4-3.4-051 through CB4-3.4-055 (5 base × 2 phrasings = 10 instances)

Application rules trained:
1. PASS — confirm control applies to production system; add scope boundary
2. REPROCESS — contextualise impact for deployment type (SPA vs. server-side, user population)
3. CONCERN — distinguish ASVS compliance state from internal governance gap; severity rating
4. REPROCESS (CWE) — add attack complexity and data-at-risk assessment
5. CONCERN (SSDF) — clarify what can/cannot be retroactively remediated; produce actionable items

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-055 (110 instances)
