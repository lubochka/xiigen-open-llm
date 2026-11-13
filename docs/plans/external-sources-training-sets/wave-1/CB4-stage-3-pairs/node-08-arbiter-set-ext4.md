# Node 08 — Arbiter Units — Extension Set 4
# Stage 3 Action 3 — Coverage Extension (new domain arbiter completions)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-171 through CB4-3.4-178
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 356 instances (sequences 001–178)

---

## Coverage targets for this file

Arbiter-level evaluations for new domains introduced in the extension sets.
Domains: CWE-352 (CSRF), CWE-307 (brute force), ASVS V6.3 (MFA), ASVS V8.3 (admin access),
SSDF PO.1 (org training), SSDF PO.2 (component evaluation), RFC 8628 (device flow),
ASVS V10.3 (token revocation).

Each pair establishes the complete arbiter verdict logic for its domain.

---

## Pair CB4-3.4-171

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — CWE-352 CSRF arbiter: state-changing endpoint without CSRF protection)
**Source:** CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter: CWE-Authz-Arbiter
territory: T-cwe-authz
findings:
  - endpoint: "POST /api/account/password-change"
    status: "No CSRF token in request. Session cookie used for authentication with no SameSite attribute."
    impact: "Attacker can craft a cross-origin form that triggers password change for any authenticated user who visits a malicious page."
    team_claim: "Endpoint requires current password re-entry — CSRF cannot be exploited without knowing the current password"
pipeline_run_id: PR-5001
```

### B

```
arbiter_result:
  arbiter: CWE-Authz-Arbiter
  verdict: REPROCESS
  cwe_id: CWE-352
  severity: HIGH
  gap: "POST /api/account/password-change has no CSRF token and no SameSite cookie attribute. A cross-origin request from a malicious page can trigger password change without the user's intent."
  action_required: "Add CSRF token to the password-change form and validate it server-side on POST, OR set SameSite=Strict (or SameSite=Lax) on the session cookie. Password re-entry mitigates exploitation but does not satisfy CSRF protection requirements — the cross-origin request can still be triggered; whether it succeeds also depends on what the attacker knows."
  note: "REPROCESS: CSRF protection is a MUST-level control for state-changing endpoints. Password re-entry partially mitigates one exploitation path but does not prevent the malicious cross-origin request from being sent. CSRF token or SameSite is the required remediation."
```

### Reason

CWE-352 on a password-change endpoint. The team's password-re-entry argument is addressed: it partially mitigates exploitation but does not satisfy the CSRF protection requirement. The cross-origin request can still be triggered — whether it succeeds depends on whether the attacker knows the current password. CSRF protection must prevent the cross-origin request itself, not just make exploitation harder. REPROCESS because the control is absent.

### Phrasing B

```
CWE-Authz-Arbiter, run PR-5001.
POST /api/account/password-change: no CSRF token, no SameSite. Requires current password.
Team: "password re-entry makes CSRF exploitation impossible."
Verdict?
```

Expected: REPROCESS. CWE-352. Password re-entry partially mitigates but does not satisfy CSRF protection requirement. CSRF token or SameSite=Strict/Lax required.

---

## Pair CB4-3.4-172

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — CWE-307 with CAPTCHA: CAPTCHA as partial lockout control)
**Source:** CWE Top 25 2023
**Verdict:** CONCERN

### A

```
arbiter: CWE-Authn-Arbiter
territory: T-cwe-authn
findings:
  - endpoint: "POST /api/auth/login"
  - controls_present:
    - "CAPTCHA displayed after 5 consecutive failed attempts"
    - "no account lockout implemented"
    - "no IP-based rate limiting"
  - team_claim: "CAPTCHA after 5 failures is our brute-force mitigation"
pipeline_run_id: PR-5012
```

### B

```
arbiter_result:
  arbiter: CWE-Authn-Arbiter
  verdict: CONCERN
  cwe_id: CWE-307
  severity: MEDIUM
  gap: "CAPTCHA after 5 failures provides partial mitigation against automated brute-force but does not constitute account lockout. Automated tools can solve CAPTCHAs (third-party CAPTCHA solving services are widely available). No IP-based rate limiting or account lockout is present."
  action_recommended:
    - "Add account lockout (e.g., 10–20 failed attempts triggers temporary lockout with exponential backoff)."
    - "Add IP-based rate limiting as a complementary control."
    - "Retain CAPTCHA as a defence-in-depth layer — it is not a substitute for lockout."
  note: "CONCERN not REPROCESS: CAPTCHA is a real mitigation — it raises the cost of automated attacks. The gap is the absence of lockout, which is the more reliable control. Recommend adding lockout to reach PASS."
```

### Reason

CAPTCHA after 5 failures is a real partial mitigation — it raises the cost of automated brute force. The verdict is CONCERN (not REPROCESS) because a partial control is present. The gap is that CAPTCHA is not account lockout and can be bypassed by CAPTCHA-solving services. The action_recommended describes how to reach PASS: add lockout + rate limiting, retain CAPTCHA as defence-in-depth. This contrasts with pair 133 (no controls at all = REPROCESS) and pair 151's chain (no controls = REPROCESS).

### Phrasing B

```
CWE-Authn-Arbiter, run PR-5012.
Login endpoint: CAPTCHA after 5 failures. No lockout, no rate limiting.
Is CAPTCHA sufficient? What verdict?
```

Expected: CONCERN. CAPTCHA is partial mitigation (raises cost, not an absolute barrier). CWE-307. Action_recommended: add account lockout + rate limiting. Not REPROCESS (partial control present).

---

## Pair CB4-3.4-173

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — V6.3.2 mandatory MFA: admin-enforced MFA satisfies the requirement)
**Source:** OWASP ASVS 5.0.0
**Verdict:** PASS

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - requirement: V6.3.1
    status: "TOTP MFA available for all users"
  - requirement: V6.3.2
    status: "MFA enforced for all admin users (12 admin accounts); MFA is mandatory before admin session is established"
  - requirement: V6.3.3
    status: "not assessed in this submission (backup codes)"
pipeline_run_id: PR-5023
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: PASS
  satisfied: [V6.3.1, V6.3.2]
  notes:
    V6.3.1: "TOTP available for all users — requirement satisfied."
    V6.3.2: "MFA enforced for all admin users; mandatory before admin session — requirement satisfied."
    V6.3.3: "V6.3.3 (backup authentication code handling) was not included in this submission scope. Not assessed — not a gap for this run."
  scope_boundary: "V6.3.1 and V6.3.2 assessed. V6.3.3 not in scope for this submission."
```

### Reason

V6.3.1 (MFA available) and V6.3.2 (MFA mandatory for admins) are both satisfied. The arbiter notes that V6.3.3 was not in scope — it does not mark V6.3.3 as PASS or FAIL, it correctly states it was not assessed. This is important: out-of-scope sub-requirements are neither passed nor failed, they are noted as not assessed.

### Phrasing B

```
ASVS-V6-Auth-Arbiter, run PR-5023.
V6.3.1: TOTP available. V6.3.2: admin MFA enforced. V6.3.3: not submitted.
Verdict?
```

Expected: PASS on V6.3.1 + V6.3.2. V6.3.3 noted as not assessed (not in scope). No REPROCESS or CONCERN because no gap was presented.

---

## Pair CB4-3.4-174

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — V8.3.2 admin action logging; gap in audit trail)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V8-AuthZ-Arbiter
territory: T-asvs-authz
findings:
  - requirement: V8.3.1
    status: "admin role check enforced on /admin/* routes — satisfied"
  - requirement: V8.3.2
    status: "administrative actions (user deletion, permission change) performed via /admin/* but not logged; no audit trail for admin operations"
pipeline_run_id: PR-5034
```

### B

```
arbiter_result:
  arbiter: ASVS-V8-AuthZ-Arbiter
  verdict: CONCERN
  satisfied: [V8.3.1]
  gap_v8_3_2: "Administrative actions performed via /admin/* are not logged. V8.3.2 requires that administrative actions be logged for audit purposes."
  action_recommended: "Implement an audit log recording who performed each administrative action, what action was taken, on which resource, and at what time. Log entries should be tamper-evident (append-only store or cryptographic chain)."
  note: "CONCERN not REPROCESS: V8.3.1 (access control) is satisfied. V8.3.2 (audit trail) is absent — this is a logging/accountability gap, not an access control failure. Prioritize for the next sprint."
```

### Reason

V8.3.1 (access control for admin functions) is satisfied. V8.3.2 (audit logging for admin actions) is not. The verdict is CONCERN (not REPROCESS) because the access control itself is correct — admin actions require admin role. The audit trail gap is a accountability concern, not an authorization bypass. The action_recommended specifies the technical requirements for the audit log.

### Phrasing B

```
ASVS-V8-AuthZ-Arbiter, run PR-5034.
V8.3.1: admin role check on /admin/* — satisfied.
V8.3.2: admin actions not logged.
Verdict?
```

Expected: CONCERN. V8.3.1 PASS. V8.3.2 gap: no audit log for admin actions. Not REPROCESS (access control correct; accountability gap). Action_recommended: implement tamper-evident admin action log.

---

## Pair CB4-3.4-175

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — SSDF PO.1 training gap)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
arbiter: SSDF-PO-Arbiter
territory: T-ssdf-po1
findings:
  - practice: PO.1
    evidence_provided: "General secure coding training completed by all engineers (annual)"
    gap: "No role-specific security training for the authentication/authorization team. Authentication team implements JWT, OAuth, and session controls but has not received targeted training on those specific domains."
pipeline_run_id: PR-5045
```

### B

```
arbiter_result:
  arbiter: SSDF-PO-Arbiter
  verdict: CONCERN
  practice: PO.1
  gap: "General secure coding training is present and annually renewed. Role-specific training for the authentication team (JWT, OAuth, session security) is absent."
  action_recommended:
    - "Develop or source role-specific training covering JWT security (algorithm confusion, alg:none, claims validation), OAuth 2.0/OIDC implementation security, and session management for the authentication team."
    - "Target: role-specific training delivered within 60 days."
  note: "CONCERN not REPROCESS: general training is present (partial compliance). The gap is the absence of role-tailored content for a specialist team."
```

### Reason

SSDF PO.1 requires that organizational roles be equipped with appropriate security training. General training is present (partial credit). Role-specific training for the authentication team (a specialized team implementing JWT and OAuth) is absent. CONCERN because general training is in place — not REPROCESS. The 60-day target is a reasonable timeline for sourcing or developing specialist training.

### Phrasing B

```
SSDF-PO-Arbiter, PO.1, run PR-5045.
All engineers: annual general secure coding training.
Auth team: no JWT/OAuth-specific training.
Verdict?
```

Expected: CONCERN. General training present (partial). Role-specific gap for auth team. 60-day target for specialist training. Not REPROCESS (general training = partial control).

---

## Pair CB4-3.4-176

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — SSDF PO.3 supply chain: no software bill of materials)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
arbiter: SSDF-PO-Arbiter
territory: T-ssdf-po3
findings:
  - practice: PO.3
    gap: "No Software Bill of Materials (SBOM) maintained. Organization uses 47 third-party libraries in the authentication service. No formal inventory of component versions or known vulnerabilities."
    existing_control: "Snyk SCA scans run in CI pipeline — alerts on HIGH/CRITICAL CVEs in current branch"
pipeline_run_id: PR-5056
```

### B

```
arbiter_result:
  arbiter: SSDF-PO-Arbiter
  verdict: CONCERN
  practice: PO.3
  gap: "No SBOM maintained for the authentication service. 47 third-party components are used without a formal component inventory."
  existing_partial_control: "Snyk SCA in CI provides runtime CVE alerts for the current build. This covers active vulnerabilities in the checked-in code but does not provide a persistent, auditable component inventory."
  action_recommended:
    - "Generate and maintain an SBOM in SPDX or CycloneDX format for the authentication service."
    - "Update the SBOM automatically on each dependency change (integrate with CI pipeline)."
    - "The Snyk SCA integration satisfies PO.2 (component evaluation) but is not a substitute for PO.3 (component inventory/SBOM)."
  note: "CONCERN: Snyk provides reactive vulnerability detection; SBOM provides proactive inventory visibility. Both are needed for full PO.3 compliance."
```

### Reason

SSDF PO.3 requires maintaining a component inventory (SBOM). Snyk SCA satisfies PO.2 (evaluation of components for known vulnerabilities) but is not an SBOM — it is a scanner, not an inventory. The CONCERN verdict acknowledges that Snyk is a real and valuable control; the gap is the absence of a formal persistent inventory. This pair also reinforces the PO.2 vs PO.3 distinction.

### Phrasing B

```
SSDF-PO-Arbiter, PO.3, run PR-5056.
47 third-party libraries in auth service. No SBOM. Snyk SCA in CI.
Does Snyk satisfy PO.3?
```

Expected: CONCERN. Snyk satisfies PO.2 (vulnerability evaluation), not PO.3 (inventory/SBOM). SBOM in SPDX or CycloneDX format required. Not REPROCESS (Snyk = partial control).

---

## Pair CB4-3.4-177

**Node:** 08 — Arbiter Units
**Stratum:** A (domain breadth — RFC 8628 device flow: expired device_code; correct arbiter evaluation)
**Source:** IETF RFC 8628
**Verdict:** PASS

### A

```
arbiter: RFC-OAuth2-Arbiter
territory: T-rfc-device-flow
findings:
  - RFC: RFC-8628
  - "device_code and user_code returned by /oauth/device/authorize — confirmed"
  - "user_code: 9 characters (uppercase alpha + digits), displayed on device screen"
  - "device_code expiry: 10 minutes"
  - "polling interval: 5 seconds"
  - "expired device_code: server returns error:expired_token response per RFC 8628 §3.5"
pipeline_run_id: PR-5067
```

### B

```
arbiter_result:
  arbiter: RFC-OAuth2-Arbiter
  verdict: PASS
  rfc: RFC-8628
  satisfied:
    - "RFC8628-§3.2: device_code, user_code, verification_uri returned — satisfied"
    - "RFC8628-§3.3: polling interval 5s — satisfies minimum; device polls at stated interval"
    - "RFC8628-§3.2: device_code expiry 10 minutes — within reasonable duration"
    - "RFC8628-§3.5: expired device_code returns error:expired_token — required error response satisfied"
  scope_boundary: "RFC 8628 Device Authorization Grant normative requirements satisfied. OAuth token lifecycle (V10.3) not in scope for this arbiter."
```

### Reason

The device flow implementation satisfies all assessed RFC 8628 MUST requirements including the error response for expired device codes (§3.5). The scope_boundary notes that V10.3 (token revocation) is separate from this arbiter's scope. This establishes the complete arbiter evaluation for the device flow domain.

### Phrasing B

```
RFC-OAuth2-Arbiter (RFC 8628), run PR-5067.
device_code endpoint confirmed. user_code: 9 chars. Expiry: 10 min. Poll: 5s. Expired device_code: error:expired_token returned.
Verdict?
```

Expected: PASS. All RFC 8628 MUST requirements satisfied including expired device_code error response. Scope boundary notes V10.3 is separate.

---

## Pair CB4-3.4-178

**Node:** 08 — Arbiter Units
**Stratum:** B (team context — V10.3.2 access token revocation: revocation endpoint present but non-functional)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V10-OAuth-Arbiter
territory: T-asvs-token-revoke
findings:
  - requirement: V10.3.1
    status: "POST /oauth/revoke endpoint present — responds 200 to all requests"
  - requirement: V10.3.2
    status: "access tokens are NOT actually invalidated after revocation call; subsequent requests with revoked tokens still succeed"
  - team_note: "the endpoint exists and returns 200; our RFC 7009 implementation is in progress"
pipeline_run_id: PR-5078
```

### B

```
arbiter_result:
  arbiter: ASVS-V10-OAuth-Arbiter
  verdict: REPROCESS
  requirements_violated: [V10.3.2]
  gap: "The /oauth/revoke endpoint returns 200 but does not actually invalidate access tokens. Subsequent requests bearing 'revoked' tokens succeed. V10.3.2 requires that token revocation be effective — an endpoint that acknowledges revocation without enforcing it does not satisfy the requirement."
  action_required: "Implement actual token invalidation: after a revocation call, add the token to a revocation list (or invalidate the session record) and reject all subsequent requests bearing that token with 401."
  note: "V10.3.1 (endpoint presence): technically present but non-functional — a 200 response from a no-op endpoint does not constitute an implemented revocation endpoint. V10.3.1 is not satisfiable by a stub endpoint."
```

### Reason

The revocation endpoint returns 200 but does nothing. V10.3.2 requires effective revocation — tokens must actually be invalidated. A stub endpoint that acknowledges without enforcing is not a valid implementation of either V10.3.1 or V10.3.2. REPROCESS because the control is present in name but absent in function. The note clarifies that V10.3.1 cannot be satisfied by a stub endpoint.

### Phrasing B

```
ASVS-V10-OAuth-Arbiter, run PR-5078.
/oauth/revoke endpoint: returns 200. But revoked tokens still work.
Team: endpoint exists; implementation in progress.
Verdict?
```

Expected: REPROCESS. V10.3.2 violated — stub endpoint ≠ effective revocation. Endpoint returning 200 without invalidation does not satisfy V10.3.1 either. Action_required: implement actual token revocation with rejection of revoked tokens.

---

## File summary

| Seq | Stratum | Source | Verdict | Domain introduced at arbiter level |
|-----|---------|--------|---------|-----------------------------------|
| 171 | A | CWE | REPROCESS | CWE-352 CSRF on password-change; password-re-entry argument rejected |
| 172 | A | CWE | CONCERN | CWE-307 with CAPTCHA: partial control = CONCERN (not REPROCESS) |
| 173 | A | ASVS | PASS | V6.3.1+V6.3.2 satisfied; V6.3.3 not assessed (not failed) |
| 174 | A | ASVS | CONCERN | V8.3.2 admin action logging absent (V8.3.1 access control satisfied) |
| 175 | A | SSDF | CONCERN | SSDF PO.1 role-specific training absent (general training present) |
| 176 | A | SSDF | CONCERN | SSDF PO.3 SBOM absent (Snyk satisfies PO.2, not PO.3) |
| 177 | A | IETF | PASS | RFC 8628 device flow complete evaluation including expired device_code |
| 178 | B | ASVS | REPROCESS | V10.3.2 stub revocation endpoint — returns 200 but tokens still valid |

**Running total: 356 instances (sequences 001–178)**
Stratum breakdown: A ~31%, B ~24%, C ~29%, D ~16%
