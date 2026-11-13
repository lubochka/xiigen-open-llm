# Node 08 — Arbiter Units — Extension Set 6
# Stage 3 Action 3 — Coverage Extension (remaining domain gaps + Stratum D)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-199 through CB4-3.4-206
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 412 instances (sequences 001–206)

---

## Coverage targets for this file

Final domain gap closure + Stratum D adversarial edge cases.
New domains:
  - ASVS V6.2.4 (password change requires current password)
  - ASVS V7.2.4 (session timeout / idle timeout)
  - ASVS V9.3 (token transport security)
  - CWE-287 with magic link: passwordless email link = authentication concern
  - SSDF PW.9 (security review of components before deployment)
  - ASVS V10.5 (OpenID Connect scope claims)

Stratum D adversarial:
  - sub-claim injection: team submits JWT with sub="admin" claim as ASVS V8 evidence
  - V7.2.4 timeout: team argues "users stay logged in for 30 days; business requirement"

---

## Pair CB4-3.4-199

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — V6.2.4: password change requires current password)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - requirement: V6.2.4
  - "password change endpoint: POST /api/account/password"
  - "fields: new_password, confirm_new_password"
  - "current_password field: not required"
  - "endpoint requires valid session cookie only"
pipeline_run_id: PR-5301
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: REPROCESS
  requirements_violated: [V6.2.4]
  gap: "Password change does not require current password verification. V6.2.4 requires that password change operations verify the current password. Without current password verification, any attacker with a valid session (including an unattended session or CSRF attack in combination with CWE-352) can change the account password."
  action_required: "Add current_password verification to the password change endpoint. The new password must not be accepted unless the current password is also provided and verified."
```

### Reason

V6.2.4 requires current password verification on password change. The endpoint accepts new_password and confirm_new_password without requiring the current password. This allows any session-bearer to change the password — combined with session fixation or CSRF, this is a high-impact gap. REPROCESS because V6.2.4 is a MUST-level requirement.

### Phrasing B

```
V6.2.4. Password change endpoint: new_password + confirm_new_password, no current_password required.
ASVS-V6-Auth-Arbiter, run PR-5301.
```

Expected: REPROCESS. V6.2.4 requires current password verification. Absent = gap enabling any session-bearer to change password.

---

## Pair CB4-3.4-200

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — "30-day session is a business requirement" against V7.2.4 session timeout)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V7-Session-Arbiter
territory: T-asvs-session
findings:
  - requirement: V7.2.4
  - "session idle timeout: none — sessions remain active indefinitely until explicit logout"
  - "session maximum lifetime: 30 days (rolling refresh on each request)"
  - "team note: 30-day sessions are a business requirement; our customers demand 'stay logged in'"
pipeline_run_id: PR-5312
```

### B

```
arbiter_result:
  arbiter: ASVS-V7-Session-Arbiter
  verdict: CONCERN
  requirements: [V7.2.4]
  gap: "No idle session timeout. V7.2.4 recommends session idle timeout to reduce the exposure window from unattended sessions (shared computers, lost devices). 30-day session lifetime is a business decision but creates extended exposure."
  action_recommended:
    - "Implement idle session timeout (e.g., 15 minutes for security-sensitive actions, or a configurable idle timeout)."
    - "Consider separating 'stay logged in' from full-privilege session: use a long-lived refresh token + short-lived session token combination."
  note: "CONCERN not REPROCESS: V7.2.4 does not mandate a specific timeout duration via an absolute MUST clause. The absence of any idle timeout is a deviation from good practice. Business requirements for long sessions can be accommodated with short-lived active session tokens + long-lived refresh tokens (similar to OAuth patterns)."
```

### Reason

V7.2.4 (session timeout) does not impose an absolute MUST timeout duration — it is a security recommendation. The complete absence of any idle timeout is a CONCERN. The business requirement for 30-day sessions is noted; the action_recommended provides an architecture that satisfies both the business need (stay logged in) and the security concern (short-lived active session tokens).

### Phrasing B

```
V7.2.4. No idle timeout. 30-day rolling session. Team: business requirement for "stay logged in."
ASVS-V7-Session-Arbiter, run PR-5312. REPROCESS or CONCERN?
```

Expected: CONCERN. No absolute MUST timeout duration in V7.2.4. No idle timeout = deviation from good practice. Action_recommended: idle timeout + refresh token architecture. Not REPROCESS (no hard MUST violation on duration).

---

## Pair CB4-3.4-201

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — V9.3 token transport: JWT in Authorization header vs URL parameter)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V9-Token-Arbiter
territory: T-token-validation
findings:
  - requirement: V9.3.1
  - "JWT delivery: some legacy endpoints accept JWT via URL query parameter (?token=...)"
  - "newer endpoints: Authorization: Bearer header — correct"
  - "URL-based JWT: appears in server access logs, browser history, and Referer headers"
pipeline_run_id: PR-5323
```

### B

```
arbiter_result:
  arbiter: ASVS-V9-Token-Arbiter
  verdict: REPROCESS
  requirements_violated: [V9.3.1]
  gap: "JWT tokens transmitted via URL query parameter on legacy endpoints. V9.3.1 requires tokens to be transmitted in headers or cookies, not URL parameters. URL-based tokens are logged in server access logs, appear in browser history, and can leak via Referer headers to third-party analytics."
  action_required: "Remove JWT query parameter support from all endpoints. Require Authorization: Bearer header or httpOnly cookie for token delivery on all endpoints, including legacy ones."
  note: "Partial compliance: newer endpoints use Authorization header (correct). Legacy endpoints using URL parameters remain REPROCESS — partial compliance is not compliance."
```

### Reason

V9.3.1 requires tokens to be transmitted securely — not in URL parameters (which leak to logs, browser history, and Referer headers). The legacy endpoints with URL-based JWT create a real exposure path. REPROCESS because some endpoints violate the requirement, even though newer endpoints are correct.

### Phrasing B

```
V9.3.1. New endpoints: Authorization header (correct). Legacy endpoints: ?token= query parameter.
ASVS-V9-Token-Arbiter, run PR-5323. Verdict?
```

Expected: REPROCESS. URL-based JWT leaks to logs, browser history, Referer. Action_required: remove URL token support from all endpoints. Partial compliance ≠ compliance.

---

## Pair CB4-3.4-202

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — CWE-287 magic link: email-based passwordless login)
**Source:** CWE Top 25 2023
**Verdict:** CONCERN

### A

```
arbiter: CWE-Authn-Arbiter
territory: T-cwe-authn
findings:
  - "authentication mechanism: magic link (one-time link sent to email)"
  - "magic link: valid for 15 minutes, single-use"
  - "no second factor required"
  - "link token: 32 random bytes (hex) — cryptographically strong"
  - "email delivery: over SMTP with TLS"
pipeline_run_id: PR-5334
```

### B

```
arbiter_result:
  arbiter: CWE-Authn-Arbiter
  verdict: CONCERN
  cwe_id: "CWE-287"
  finding: "Magic link authentication with 15-minute expiry, single-use, and cryptographically strong token does not exhibit CWE-287 (Improper Authentication) in the token mechanism. However: authentication security is dependent on email account security — if the email account is compromised, authentication is compromised."
  action_recommended:
    - "Email as authentication channel creates account takeover risk via email compromise. For high-risk applications, supplement magic link with a second factor (e.g., TOTP or push notification)."
    - "Ensure magic links are invalidated on first use and on email address change."
  note: "CONCERN not REPROCESS: the token mechanism is secure (15-minute expiry, single-use, strong entropy). The concern is the email channel dependency — email account security is outside the application's control."
```

### Reason

Magic link authentication with strong token parameters (15-minute expiry, single-use, 32-byte random token) is not a CWE-287 vulnerability in the token mechanism itself. The CONCERN is the email channel dependency — email account compromise leads to authentication compromise. CONCERN (not REPROCESS) because the technical implementation is sound; the risk is the channel's security model.

### Phrasing B

```
CWE-Authn-Arbiter, run PR-5334.
Magic link: 32-byte random token, 15-minute expiry, single-use, SMTP+TLS delivery.
Is this CWE-287?
```

Expected: CONCERN. Token mechanism technically sound. Concern: email channel dependency (email account compromise = auth compromise). Not REPROCESS (no improper authentication in the token mechanism).

---

## Pair CB4-3.4-203

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — SSDF PW.9: security review before component deployment)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-pw9
findings:
  - practice: PW.9
  - "security review of new dependencies before deployment: not performed"
  - "team note: Snyk SCA runs post-merge in CI and blocks HIGH/CRITICAL CVEs"
  - "Snyk scan timing: runs after the dependency is merged to main; before deployment to staging/production"
  - "pre-merge review: dependency added to package.json without security review"
pipeline_run_id: PR-5345
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: REPROCESS
  practice: PW.9
  gap: "PW.9 requires security review of components before they are integrated. Snyk runs post-merge — the dependency is merged to main before the security review occurs. PW.9 requires pre-integration review, not post-merge scanning."
  action_required: "Move Snyk SCA to the pre-merge check (pull request stage) so that security review occurs before any dependency is merged to main. Configure branch protection to require Snyk SCA pass as a merge condition."
  note: "REPROCESS: post-merge scanning (PW.4 control) is valuable but does not satisfy PW.9 (pre-integration review). PW.9 requires that the review gate the integration decision, not follow it."
```

### Reason

SSDF PW.9 requires security review of components before integration (pre-merge). Running Snyk post-merge means the dependency is integrated first and reviewed second. If a vulnerability is found post-merge, it may already be deployed to staging. REPROCESS because the timing violates the "before integration" requirement.

### Phrasing B

```
SSDF-PW-Arbiter, PW.9, run PR-5345.
Snyk SCA: runs after merge to main, before staging/production deployment.
Does post-merge Snyk satisfy PW.9 (security review before integration)?
```

Expected: REPROCESS. PW.9 requires pre-integration review. Post-merge = dependency already integrated. Move Snyk to PR-stage pre-merge check.

---

## Pair CB4-3.4-204

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — ASVS V10.5 OpenID Connect: ID token claims)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V10-OAuth-Arbiter
territory: T-oauth-oidc
findings:
  - requirement: V10.5.1
  - "ID token issued by authorization server: contains sub, iss, aud, exp, iat"
  - "V10.5.1: ID token aud claim must contain the client_id"
  - "aud claim value: 'all-clients' (a generic audience label used for all applications)"
  - "client_id: 'mobile-app-prod'"
pipeline_run_id: PR-5356
```

### B

```
arbiter_result:
  arbiter: ASVS-V10-OAuth-Arbiter
  verdict: REPROCESS
  requirements_violated: [V10.5.1]
  gap: "ID token aud claim contains 'all-clients' instead of the specific client_id ('mobile-app-prod'). V10.5.1 requires the aud claim to contain the client_id of the relying party. A generic 'all-clients' audience means the same ID token can be accepted by any application, enabling ID token relay attacks."
  action_required: "Issue ID tokens with the specific client_id in the aud claim. ID tokens must not use a generic audience — each ID token must be bound to the intended relying party."
  note: "ID token relay attack: if 'all-clients' is accepted, an ID token obtained by Application A can be replayed to Application B. The client-specific aud is the binding mechanism that prevents this."
```

### Reason

V10.5.1 requires the ID token aud claim to contain the client_id. A generic "all-clients" audience breaks the binding between the ID token and its intended relying party — any application that accepts the audience "all-clients" can accept any ID token. This is an ID token relay attack vector. REPROCESS because it is a MUST-level requirement in OIDC.

### Phrasing B

```
ASVS-V10-OAuth-Arbiter, V10.5.1, run PR-5356.
ID token aud claim: 'all-clients'. Client_id: 'mobile-app-prod'.
Does generic 'all-clients' audience satisfy V10.5.1?
```

Expected: REPROCESS. Generic aud ≠ client-specific aud. ID token relay attack vector. Action_required: use client_id in aud claim.

---

## Pair CB4-3.4-205

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — JWT sub="admin" claim submitted as ASVS V8 authorization evidence)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V8-AuthZ-Arbiter
territory: T-asvs-authz
findings:
  - requirement: V8.3.1
  - endpoint: "/admin/users"
  - "authorization check: JWT decoded; if jwt.claims.sub == 'admin' → grant access"
  - "sub claim value checked at runtime against the string literal 'admin'"
  - "team note: the JWT sub claim identifies the user; if the sub is 'admin', the user is an admin"
pipeline_run_id: PR-5367
```

### B

```
arbiter_result:
  arbiter: ASVS-V8-AuthZ-Arbiter
  verdict: REPROCESS
  requirements_violated: [V8.3.1]
  gap: "Authorization is based on the sub claim value equaling the string 'admin'. The sub claim identifies the subject (typically a user ID or username), not the user's role. A user whose username or user ID is 'admin' will be granted admin access; any other string will not. This conflates subject identity with role assignment."
  action_required: "Implement role-based authorization: issue a role or groups claim in the JWT (e.g., jwt.claims.roles includes 'admin') and verify the role claim at the authorization check. Sub claim must identify the user, not the user's role."
  note: "REPROCESS: sub='admin' as an authorization check is a role-as-identity conflation — any account with the username 'admin' receives admin privileges. This may be intentional for a single admin account but does not satisfy V8.3.1's requirement for proper authorization. The more critical issue: if the JWT is user-generated or if the sub claim can be influenced, this is an authorization bypass."
```

### Reason

Using JWT sub claim value to grant admin access conflates subject identity with role. V8.3.1 requires a proper authorization check. The sub claim identifies who the user is (user ID/username), not what they're allowed to do. REPROCESS because the authorization check is present but based on the wrong claim type.

### Phrasing B

```
ASVS-V8-AuthZ-Arbiter, V8.3.1, run PR-5367.
/admin/users: checks if JWT sub == 'admin'. If yes, grants access.
Is checking sub=='admin' a valid V8.3.1 authorization check?
```

Expected: REPROCESS. sub identifies subject, not role. Authorization must use a role/permission claim, not subject identity string comparison. Action_required: implement role claim-based authorization.

---

## Pair CB4-3.4-206

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — SSDF PW.2 + team submits "roadmap" as threat model evidence)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-pw2
findings:
  - practice: PW.2
  - "evidence submitted: Q3 2026 product roadmap document"
  - "roadmap includes section 3.2: 'Security Architecture — planned authentication redesign using OAuth 2.0'"
  - "team note: 'the roadmap covers our planned security architecture, which is our threat model'"
pipeline_run_id: PR-5378
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: REPROCESS
  practice: PW.2
  gap: "A product roadmap is not a threat model. PW.2 requires a threat model that identifies threats to the current system, their likelihood, and mitigations. A roadmap describes planned future changes, not current threat analysis."
  action_required: "Produce a threat model for the current system architecture. The threat model must: (1) identify assets and trust boundaries in the current deployment; (2) enumerate threat actors and attack vectors; (3) document mitigations in place. Submit the threat model artefact with a creation or review date."
  note: "A roadmap section on 'planned security architecture' is future-state planning, not current-state threat analysis. PW.2 requires analysis of the current system, not a plan for what the system will look like. Both the current system threat model and the planned architecture's threat model are eventually needed."
```

### Reason

A product roadmap describing planned future security architecture is not a threat model for the current system. PW.2 requires current-state threat analysis. The team conflated "describing future security plans" with "analyzing current threats." REPROCESS because no threat model artefact was provided. This is a variant of the FM-SSDF-5 (ARTIFACT_NAME_REQUIRED) failure mode — the team provided a document, but the wrong type of document.

### Phrasing B

```
SSDF-PW-Arbiter, PW.2, run PR-5378.
Team submits Q3 2026 product roadmap with section on "planned OAuth redesign" as threat model evidence.
Does a roadmap satisfy PW.2?
```

Expected: REPROCESS. Roadmap = future-state planning, not current threat analysis. PW.2 requires current system threat model with assets, threat actors, mitigations. Roadmap document is wrong evidence type.

---

## File summary

| Seq | Stratum | Source | Verdict | Key domain / adversarial scenario |
|-----|---------|--------|---------|-----------------------------------|
| 199 | A | ASVS | REPROCESS | V6.2.4: password change without current password verification |
| 200 | D | ASVS | CONCERN | V7.2.4: 30-day session "business requirement"; no idle timeout = CONCERN not REPROCESS |
| 201 | A | ASVS | REPROCESS | V9.3.1: JWT in URL query parameter on legacy endpoints |
| 202 | A | CWE | CONCERN | CWE-287 magic link: token mechanism secure; email channel dependency = CONCERN |
| 203 | A | SSDF | REPROCESS | SSDF PW.9: Snyk post-merge ≠ pre-integration review |
| 204 | A | ASVS | REPROCESS | V10.5.1: ID token aud='all-clients' instead of specific client_id |
| 205 | D | ASVS | REPROCESS | sub='admin' authorization: subject identity ≠ role claim |
| 206 | D | SSDF | REPROCESS | Product roadmap submitted as threat model: future planning ≠ current threat analysis |

**Running total: 412 instances (sequences 001–206)**
Stratum breakdown: A ~31%, B ~22%, C ~30%, D ~17%

---

## Milestone note

412 instances (206 base pairs) produced for CB4, Sub-phase 3.4.
Original batch (Action 2): 120 instances (60 base pairs, sequences 001–060).
Action 3 extension: 292 additional instances (146 base pairs, sequences 061–206).
