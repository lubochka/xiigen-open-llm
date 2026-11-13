# Node 08 — Arbiter Units — Extension Set 7
# Stage 3 Action 3 — Coverage Extension (Stratum B team context for new domains)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-207 through CB4-3.4-214
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 428 instances (sequences 001–214)

---

## Coverage targets for this file

Stratum B team-context pairs for new domains introduced in the extension sets.
Each pair tests a realistic team scenario against a new domain's requirements.

---

## Pair CB4-3.4-207

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — "we enforce MFA for sensitive actions, not login" argument)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - requirement: V6.3.2
  - "MFA enforcement: TOTP required before performing high-value actions (password change, payment initiation)"
  - "login itself: password only; no MFA at login"
  - "admin users: MFA required for high-value actions; login is password only"
  - "team rationale: step-up authentication model; MFA is contextual, not at login"
pipeline_run_id: PR-5401
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: CONCERN
  requirements: [V6.3.2]
  gap: "V6.3.2 requires MFA enforcement for privileged users. The step-up MFA model applies MFA at action time but not at session establishment. A session established with password only is a lower-assurance session — an attacker who compromises the session cookie can observe activity and stage attacks before triggering the MFA step-up."
  action_recommended:
    - "For admin users: enforce MFA at login, not only at action time. Admin session should be established with full MFA."
    - "Step-up authentication (MFA on sensitive actions) is a valid supplementary control but should not replace login-time MFA for admin users."
  note: "CONCERN not REPROCESS: step-up MFA provides real protection at the action layer. The gap is that the session itself is established with password only, leaving a window between login and action where the session is lower-assurance. Recommend login-time MFA for admin users plus step-up for sensitive actions."
```

### Reason

Step-up MFA at action time is a real security control. However, for admin users, V6.3.2 expects MFA at session establishment (login), not only at action time. The CONCERN acknowledges the step-up model as partial credit while flagging the login-time gap.

### Phrasing B

```
V6.3.2. Admin users: step-up MFA on sensitive actions, but login is password only.
ASVS-V6-Auth-Arbiter, run PR-5401. Does step-up MFA satisfy V6.3.2?
```

Expected: CONCERN. Step-up MFA is partial. V6.3.2 expects login-time MFA for admins. Session established without MFA = lower-assurance window. Recommend login-time MFA + step-up.

---

## Pair CB4-3.4-208

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — "our users prefer long sessions; we've never had a session incident")
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V7-Session-Arbiter
territory: T-asvs-session
findings:
  - requirement: V7.2.4
  - "session idle timeout: none"
  - "session lifetime: indefinite (no maximum)"
  - "team note: 'we've been operating for 5 years with no reported session hijacking incidents; our users prefer staying logged in; adding a timeout would create friction'"
pipeline_run_id: PR-5412
```

### B

```
arbiter_result:
  arbiter: ASVS-V7-Session-Arbiter
  verdict: CONCERN
  requirements: [V7.2.4]
  gap: "No idle session timeout and no maximum session lifetime. V7.2.4 recommends session timeout to reduce the attack window from unattended or compromised sessions."
  action_recommended:
    - "Implement an idle timeout of 30–60 minutes for standard users, or a longer period with a 'keep alive' prompt."
    - "For sensitive operations (payment, password change), implement a lower-assurance session check (re-authentication or step-up)."
  note: "CONCERN not REPROCESS: V7.2.4 does not mandate an absolute timeout duration. The absence of any timeout is a security deviation. The 5-year incident history does not confirm security — it confirms no reported incidents. Unreported session hijacking is difficult to detect. User friction is a real UX concern; a 'keep alive' prompt every 60 minutes balances security and usability."
```

### Reason

The team's incident-history argument is addressed: absence of reported incidents ≠ no incidents (session hijacking is often silent). User friction is a real concern — the action_recommended provides a keep-alive prompt as a user-friendly timeout implementation. CONCERN not REPROCESS (V7.2.4 has no absolute MUST timeout duration).

### Phrasing B

```
V7.2.4. No idle timeout, indefinite sessions. Team: 5 years no incidents; users prefer staying logged in.
ASVS-V7-Session-Arbiter, run PR-5412.
```

Expected: CONCERN. No timeout = deviation from good practice. No reported incidents ≠ no incidents. Action_recommended: 30-60 min idle timeout with keep-alive prompt as UX-friendly implementation.

---

## Pair CB4-3.4-209

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — "we're a small team, SBOM tooling is too expensive")
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
arbiter: SSDF-PO-Arbiter
territory: T-ssdf-po3
findings:
  - practice: PO.3
  - "SBOM: not maintained"
  - "third-party libraries: 23 in production"
  - "team note: 'SBOM tooling requires paid licenses; as a 3-person team, we can't afford dedicated SBOM software'"
  - "existing control: manual dependency review in PR descriptions"
pipeline_run_id: PR-5423
```

### B

```
arbiter_result:
  arbiter: SSDF-PO-Arbiter
  verdict: CONCERN
  practice: PO.3
  gap: "No SBOM maintained for 23 production dependencies. Manual PR-description review is not an SBOM."
  action_recommended:
    - "Use free/open-source SBOM tooling: syft (open source) generates SBOM in SPDX or CycloneDX format from package.json, requirements.txt, or similar. No paid license required."
    - "Integrate syft into CI pipeline to auto-generate SBOM on each dependency change. Store the SBOM artefact in version control."
    - "Cost concern addressed: syft is free, open-source, and can be run in CI without additional licensing."
  note: "CONCERN not REPROCESS: manual PR review is a partial control. The cost barrier is addressable — SBOM generation tooling is available open-source. The gap is the absence of a persistent, machine-readable component inventory, not tool cost."
```

### Reason

The team's cost objection is addressed directly: open-source SBOM tooling (syft) is free and CI-integrable. CONCERN (not REPROCESS) because manual PR review is a partial control. The action_recommended removes the cost barrier.

### Phrasing B

```
SSDF-PO-Arbiter, PO.3, run PR-5423.
No SBOM. 23 production libs. Team: SBOM tooling is too expensive for 3-person team.
Verdict and response to cost concern?
```

Expected: CONCERN. Cost concern addressed: syft (open source, free) generates SBOM in CI. Action_recommended: syft in CI pipeline. Not REPROCESS (manual PR review = partial control).

---

## Pair CB4-3.4-210

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — SSDF PW.9: "we review manually but don't document it")
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-pw9
findings:
  - practice: PW.9
  - "security review before component integration: team states 'we informally check new dependencies'"
  - "documentation of review: not maintained; no artefact showing review was performed"
  - "informal check: senior engineer glances at dependency on npm before adding it"
pipeline_run_id: PR-5434
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: CONCERN
  practice: PW.9
  gap: "Informal manual review without documentation. PW.9 requires that the review be performed and that evidence of the review be available. A 'glance at npm' without a documented outcome is not verifiable."
  action_recommended:
    - "Implement a lightweight documentation step: for each new dependency added, record the reviewer, the check performed (CVE count, license, last commit), and the approval decision."
    - "Acceptable format: a PR comment template or a dependency-review checklist in the PR template that must be completed before merge."
  note: "CONCERN not REPROCESS: informal review is partial — it reflects security-minded behaviour but lacks documentation. Lightweight tooling (e.g., GitHub Dependency Review Action, which is free) can automate the evidence capture."
```

### Reason

Informal review without documentation is partial. PW.9 requires verifiable review — a reviewer's mental check without documentation cannot be audited. CONCERN (not REPROCESS) because the behavior exists; the gap is the absence of documentation. The action_recommended provides a lightweight solution (PR comment template or dependency review checklist).

### Phrasing B

```
SSDF-PW-Arbiter, PW.9, run PR-5434.
Team informally reviews new dependencies before adding them. No documentation.
Does undocumented informal review satisfy PW.9?
```

Expected: CONCERN. Informal review exists (partial). No documentation = not verifiable. Action_recommended: PR comment template or dependency checklist to capture review evidence. Not REPROCESS (review behavior is present).

---

## Pair CB4-3.4-211

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — CWE-352 CSRF: "we use SPA so CSRF doesn't apply")
**Source:** CWE Top 25 2023
**Verdict:** CONCERN

### A

```
arbiter: CWE-Authz-Arbiter
territory: T-cwe-authz
findings:
  - "frontend: React SPA; API calls via fetch() with Authorization: Bearer header"
  - "authentication: JWT Bearer token in Authorization header (not cookie)"
  - "team note: 'CSRF doesn't apply to SPAs — CSRF attacks require cookies; we use Bearer tokens'"
  - "some endpoints: also accept session cookie fallback for mobile browser compatibility"
pipeline_run_id: PR-5445
```

### B

```
arbiter_result:
  arbiter: CWE-Authz-Arbiter
  verdict: CONCERN
  cwe_id: "CWE-352"
  gap: "Primary SPA flow uses Authorization: Bearer header — CSRF attacks do not apply to requests carrying tokens in Authorization headers (fetch() with custom headers is blocked by CORS preflight for cross-origin requests). Team's claim is correct for the primary flow. However: the session cookie fallback for mobile browser compatibility means that some requests may authenticate via cookie. If those requests include state-changing operations, CWE-352 applies to the cookie-authenticated code path."
  action_recommended:
    - "Audit all endpoints that accept the session cookie fallback: identify which are state-changing."
    - "For state-changing endpoints accepting session cookies: add CSRF token or SameSite=Strict."
    - "Consider eliminating the cookie fallback and using Bearer token consistently across all clients."
  note: "CONCERN not REPROCESS: primary Bearer-token flow is correctly CSRF-resistant. The gap is the cookie-fallback code path for mobile browser compatibility."
```

### Reason

The team is mostly correct: SPA with Bearer token in Authorization header is CSRF-resistant (custom headers require CORS preflight). The CONCERN is the session cookie fallback — any endpoint that also accepts a cookie is potentially vulnerable to CSRF on the cookie-authenticated path. CONCERN (not REPROCESS) because the primary flow is correct; only the fallback path is at risk.

### Phrasing B

```
CWE-Authz-Arbiter, run PR-5445.
SPA: Bearer token in Authorization header (primary). Session cookie fallback for mobile browsers.
Team: "CSRF doesn't apply to SPAs with Bearer tokens."
Are they correct?
```

Expected: Mostly correct for primary Bearer flow. CONCERN: cookie fallback path may be CSRF-vulnerable for state-changing operations. Action_recommended: audit cookie fallback endpoints, add CSRF protection or eliminate fallback.

---

## Pair CB4-3.4-212

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — CWE-307: "our users are locked out too often; we increased the limit to 100 attempts")
**Source:** CWE Top 25 2023
**Verdict:** CONCERN

### A

```
arbiter: CWE-Authn-Arbiter
territory: T-cwe-authn
findings:
  - "account lockout: triggered after 100 consecutive failed login attempts"
  - "lockout duration: 15 minutes"
  - "team rationale: 'users complained about being locked out; we increased the limit from 5 to 100 attempts'"
  - "rate limiting: no IP-based rate limiting"
pipeline_run_id: PR-5456
```

### B

```
arbiter_result:
  arbiter: CWE-Authn-Arbiter
  verdict: CONCERN
  cwe_id: CWE-307
  severity: MEDIUM
  gap: "Account lockout after 100 attempts is an insufficient brute-force barrier. Common password lists (e.g., RockYou) contain millions of passwords; at 100 attempts per account, an attacker can test 100 passwords per account before triggering lockout. Without IP-based rate limiting, a distributed attack can cycle through many accounts, testing up to 100 passwords per account before any account is locked."
  action_recommended:
    - "Reduce lockout threshold to 10–20 attempts per account (industry standard)."
    - "Add IP-based rate limiting as a complementary control (caps total attempts from any source)."
    - "To reduce user friction: implement progressive lockout (5 attempts → 5-min lockout → reset; 15 attempts → 30-min lockout) rather than a hard limit."
  note: "CONCERN not REPROCESS: lockout exists (not absent) — it is at a very high threshold. User friction is a real UX concern; progressive lockout addresses both security and usability."
```

### Reason

Account lockout at 100 attempts exists but is set too high to be effective. 100 attempts per account is sufficient to test common passwords. CONCERN (not REPROCESS) because lockout exists — it is present but poorly calibrated. The action_recommended addresses user friction with a progressive lockout model.

### Phrasing B

```
CWE-Authn-Arbiter, run PR-5456.
Account lockout: triggers at 100 consecutive failures, 15-minute lockout.
Team increased limit from 5 to 100 due to user complaints.
Verdict?
```

Expected: CONCERN. Lockout exists (partial control) but threshold too high (100 attempts = sufficient for common password testing). Progressive lockout recommended for UX + security balance. Not REPROCESS (lockout mechanism present).

---

## Pair CB4-3.4-213

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — V6.2.4 password change: "we email a confirmation link instead")
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - requirement: V6.2.4
  - "password change: accepts new_password without current_password"
  - "compensating control: password change triggers confirmation email to registered address"
  - "team note: 'if an attacker changes the password, the real user gets an email and can react'"
pipeline_run_id: PR-5467
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: CONCERN
  requirements: [V6.2.4]
  gap: "V6.2.4 requires current password verification before accepting a new password. The email confirmation is a notification, not a verification — it informs the account owner after the fact, but the password has already been changed by the time the email is received."
  action_recommended: "Add current password verification as a pre-condition for the password change. The email confirmation can remain as a supplementary notification (defense-in-depth) but cannot substitute for the pre-change verification."
  note: "CONCERN not REPROCESS: the email notification is a partial compensating control — it enables rapid detection and account recovery. However, by the time the user reacts to the notification, the attacker may have already used the compromised account. V6.2.4 requires prevention, not only detection."
```

### Reason

Email confirmation after password change is a detection/notification control, not a prevention control. V6.2.4 requires current password verification (prevention). The email is a partial compensating control — it speeds recovery but does not prevent the attack. CONCERN (not REPROCESS) because the email notification is a real partial control.

### Phrasing B

```
V6.2.4. Password change: no current password required. Email confirmation sent after change.
ASVS-V6-Auth-Arbiter, run PR-5467. Does email confirmation satisfy V6.2.4?
```

Expected: CONCERN. Email = detection/notification, not prevention. V6.2.4 requires pre-change current password verification. Email remains as supplementary control but cannot substitute. Not REPROCESS (partial control present).

---

## Pair CB4-3.4-214

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — V10.5.1 ID token: "we added a version timestamp to aud instead of client_id")
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V10-OAuth-Arbiter
territory: T-oauth-oidc
findings:
  - requirement: V10.5.1
  - "ID token aud claim: 'platform-v2-2026' (a version timestamp added for deployment tracking)"
  - "client_id: 'web-app-prod'"
  - "team note: 'we added a versioned audience to track which platform version issued the token; the client validates the version string'"
pipeline_run_id: PR-5478
```

### B

```
arbiter_result:
  arbiter: ASVS-V10-OAuth-Arbiter
  verdict: REPROCESS
  requirements_violated: [V10.5.1]
  gap: "The aud claim contains 'platform-v2-2026' instead of the client_id ('web-app-prod'). V10.5.1 requires the aud claim to identify the intended recipient — specifically the client_id of the relying party. A version timestamp does not identify the intended recipient and cannot be used to bind the token to a specific client."
  action_required: "Set the aud claim to the client_id ('web-app-prod'). If version tracking is needed, add a custom claim (e.g., 'platform_version': 'v2-2026') separate from the aud claim. The aud claim's security function must not be repurposed for deployment metadata."
  note: "REPROCESS: the aud claim must contain the relying party identifier to prevent token relay attacks. Using aud for deployment tracking disables this security binding. Custom claims are the correct mechanism for deployment metadata."
```

### Reason

The team repurposed the aud claim for deployment version tracking, losing its security function (binding the token to the intended client). V10.5.1 is violated because the client_id is not in aud. REPROCESS because the client-binding security function is absent. The action_required provides the correct architecture: client_id in aud, version in a custom claim.

### Phrasing B

```
ASVS-V10-OAuth-Arbiter, V10.5.1, run PR-5478.
aud claim: 'platform-v2-2026'. Client_id: 'web-app-prod'.
Team: aud used for version tracking.
Verdict?
```

Expected: REPROCESS. aud must contain client_id, not version metadata. Token relay attack possible (aud doesn't identify recipient). Action_required: client_id in aud; custom claim for version.

---

## File summary

| Seq | Stratum | Source | Verdict | Team scenario |
|-----|---------|--------|---------|---------------|
| 207 | B | ASVS | CONCERN | Step-up MFA on actions ≠ login-time MFA for admin users (V6.3.2) |
| 208 | B | ASVS | CONCERN | 5-year no-incident history doesn't justify indefinite sessions (V7.2.4) |
| 209 | B | SSDF | CONCERN | SBOM too expensive: cost barrier removable with free open-source tooling |
| 210 | B | SSDF | CONCERN | Informal PW.9 review without documentation: partial control, lightweight fix |
| 211 | B | CWE | CONCERN | SPA Bearer tokens + cookie fallback: primary flow correct, fallback needs review |
| 212 | B | CWE | CONCERN | Lockout at 100 attempts (UX accommodation): too high threshold = CONCERN not REPROCESS |
| 213 | B | ASVS | CONCERN | Email confirmation after password change: detection ≠ prevention (V6.2.4) |
| 214 | B | ASVS | REPROCESS | aud claim used for version tracking: client_id must be in aud (V10.5.1) |

**Running total: 428 instances (sequences 001–214)**
Stratum breakdown: A ~30%, B ~26%, C ~28%, D ~16%
