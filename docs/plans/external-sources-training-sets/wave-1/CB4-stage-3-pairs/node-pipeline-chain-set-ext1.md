# Node Pipeline Chain — Extension Set 1
# Stage 3 Action 3 — Coverage Extension (Stratum C full pipeline traces)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-151 through CB4-3.4-158
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 316 instances (sequences 001–158)

---

## Coverage targets for this file

Full 9-node pipeline traces for scenarios introduced in the extension sets.
Each pair shows all nine node outputs for one complete submission.
Chains in this file:
  151: CWE-307 brute force + V6 auth (multi-territory, REPROCESS)
  152: V8.3 admin access + V6.3 MFA (ASVS multi-chapter, two CONCERNs)
  153: SSDF PO.2 + PW.2 (two SSDF practices, PASS + CONCERN)
  154: OAuth device flow + V10.3 refresh token (IETF + ASVS, CONCERN)
  155: API key lifecycle + CWE-352 CSRF (CONCERN + REPROCESS)
  156: Adversarial chain: team uses prior-audit + wrong version ID (REPROCESS)
  157: Partial migration (HS256 legacy) + RFC-JWT (REPROCESS from RFC, Stratum D)
  158: All-PASS baseline (V6+V7+V8+SSDF PO.2) — four-domain pass

---

## Pair CB4-3.4-151

**Stratum:** C (full chain — CWE-307 + V6 auth, multi-territory REPROCESS)
**Source:** CWE Top 25 2023, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A (raw submission)

```
submission:
  system: "FinancePortal v4.2"
  sub_phase: "3.4"
  claims:
    - "bcrypt cost=12 for password hashing (V6.2.5)"
    - "TLS 1.3 enforced for all credential transmission (V6.2.3)"
    - "POST /api/auth/login: no rate limiting, no account lockout"
    - "team note: bcrypt slowness provides sufficient protection against brute force"
```

### B (full 9-node trace)

```
node_01_domain_splitter:
  territories:
    - T-asvs-auth: [V6.2.5, V6.2.3, bcrypt, TLS]
    - T-cwe-authn: [CWE-307, login endpoint, no rate limiting, no lockout]

node_02_domain_translator:
  - T-asvs-auth → label: "asvs-authentication", domain: ASVS-V6
  - T-cwe-authn → label: "cwe-authn", domain: CWE-Authentication

node_03_scheme_planner:
  plan_type: parallel_with_sync
  branches: [B1: T-asvs-auth, B2: T-cwe-authn]
  sync_at: node_07

node_04_domain_processor:
  T-asvs-auth:
    V6.2.5: bcrypt cost=12 — PASS
    V6.2.3: TLS 1.3 — PASS
    verdict_signal: PASS
  T-cwe-authn:
    CWE-307: no lockout, no rate limit; bcrypt slowness ≠ rate limiting — REPROCESS
    verdict_signal: REPROCESS

node_05_applier:
  T-asvs-auth: V6.2.3+V6.2.5 satisfied; scope boundary: authentication controls only
  T-cwe-authn: financial portal, 500,000 accounts; CRITICAL — add lockout + rate limit

node_06_result_translator:
  T-asvs-auth: [V6.2.3, V6.2.5, TLS, bcrypt] → PASS
  T-cwe-authn: [CWE-307, brute force, lockout, rate limiting] → REPROCESS

node_07_arbiter_selector:
  - ASVS-V6-Auth-Arbiter (V6.2.3, V6.2.5 signals)
  - CWE-Authn-Arbiter (CWE-307 signals)

node_08_arbiter_units:
  ASVS-V6-Auth-Arbiter:
    verdict: PASS
    satisfied: [V6.2.3, V6.2.5]
  CWE-Authn-Arbiter:
    verdict: REPROCESS
    cwe_id: CWE-307
    severity: HIGH
    gap: "No rate limiting or lockout; bcrypt does not cap attempt count"
    action_required: "Implement account lockout and rate limiting on /api/auth/login"

node_09_formatter:
  overall_verdict: REPROCESS
  arbiters_run: 2
  arbiters_passed: 1
  arbiters_failed: 1
  findings:
    - arbiter: ASVS-V6-Auth-Arbiter
      verdict: PASS
      satisfied: [V6.2.3, V6.2.5]
    - arbiter: CWE-Authn-Arbiter
      verdict: REPROCESS
      cwe_id: CWE-307
      severity: HIGH
      action_required: "Implement account lockout and rate limiting"
  attribution: "OWASP ASVS 5.0.0 CC BY-SA 4.0"
```

### Reason

Two territories: V6 authentication (PASS) and CWE-307 brute force (REPROCESS). The parallel plan ensures both are evaluated independently. ASVS-V6 PASS does not mask the CWE-307 REPROCESS — they are different territories evaluated by different arbiters. Overall REPROCESS because CWE-Authn-Arbiter returns REPROCESS. The bcrypt-as-rate-limit argument is explicitly rejected at Node 04.

### Phrasing B

```
FinancePortal v4.2: bcrypt cost=12 + TLS for authentication (satisfied). But login endpoint has no lockout or rate limiting.
Team argues bcrypt slowness is sufficient brute-force protection.
Trace the full 9-node pipeline. What does each node output, and what is the final verdict?
```

Expected: Node 01 splits into V6-auth (PASS) and CWE-307 (REPROCESS) territories. Node 04 rejects bcrypt-as-rate-limit. Node 08 CWE-Authn-Arbiter: REPROCESS. Overall: REPROCESS.

---

## Pair CB4-3.4-152

**Stratum:** C (full chain — V8.3 admin access + V6.3 MFA optional, two CONCERNs)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
submission:
  system: "EnterpriseHR v2.1"
  sub_phase: "3.4"
  claims:
    - "TOTP MFA available but optional for all users (V6.3.1)"
    - "/admin/* routes: session authentication required; no additional role check"
    - "V8.3.1 in scope"
    - "12 admin users exist; all employees have session access to /admin/*"
```

### B (full 9-node trace)

```
node_01_domain_splitter:
  territories:
    - T-asvs-mfa: [V6.3.1, TOTP, MFA, optional]
    - T-asvs-admin: [V8.3.1, /admin/*, role check absent]

node_02_domain_translator:
  - T-asvs-mfa → label: "asvs-authentication", domain: ASVS-V6
  - T-asvs-admin → label: "asvs-authorization", domain: ASVS-V8

node_03_scheme_planner:
  plan_type: parallel_with_sync
  branches: [B1: T-asvs-mfa, B2: T-asvs-admin]
  sync_at: node_07

node_04_domain_processor:
  T-asvs-mfa:
    V6.3.1: TOTP available — partial; enforcement absent for admin users — CONCERN
  T-asvs-admin:
    V8.3.1: authentication present, role-based authorization absent — REPROCESS

node_05_applier:
  T-asvs-mfa: B2B SaaS, financial data; HIGH priority — enforce MFA for 12 admins
  T-asvs-admin: all 350 employees can access /admin/*; HIGH priority

node_06_result_translator:
  T-asvs-mfa: [V6.3.1, TOTP, MFA, enforcement] → CONCERN
  T-asvs-admin: [V8.3.1, admin role, administrative access] → REPROCESS

node_07_arbiter_selector:
  - ASVS-V6-Auth-Arbiter (V6.3.1 MFA signals)
  - ASVS-V8-AuthZ-Arbiter (V8.3.1 admin authorization signals)

node_08_arbiter_units:
  ASVS-V6-Auth-Arbiter:
    verdict: CONCERN
    requirement: V6.3.1
    gap: "TOTP available but not enforced for admin users"
    action_recommended: "Enforce MFA for admin accounts"
  ASVS-V8-AuthZ-Arbiter:
    verdict: REPROCESS
    requirements_violated: [V8.3.1]
    gap: "No role check on /admin/*; any authenticated user can access administrative functions"
    action_required: "Add admin role check before /admin/* route processing"

node_09_formatter:
  overall_verdict: REPROCESS
  arbiters_run: 2
  arbiters_passed: 0
  arbiters_failed: 1
  arbiters_concern: 1
  findings:
    - arbiter: ASVS-V6-Auth-Arbiter
      verdict: CONCERN
      gap: "TOTP available, not enforced for admins"
      action_recommended: "Enforce MFA for admin accounts"
    - arbiter: ASVS-V8-AuthZ-Arbiter
      verdict: REPROCESS
      requirements_violated: [V8.3.1]
      action_required: "Add admin role check on /admin/* routes"
  note: "CONCERN does not block REPROCESS remediation. Address V8.3.1 first."
  attribution: "OWASP ASVS 5.0.0 CC BY-SA 4.0"
```

### Reason

Two ASVS territories: V6.3 (MFA, CONCERN) and V8.3 (admin authorization, REPROCESS). V8.3.1 REPROCESS overrides the CONCERN for the overall verdict. The formatter note directs the team to address V8.3.1 first — the REPROCESS takes priority. Two ASVS chapters produce two separate arbiters (V6-Auth and V8-AuthZ), reinforcing the rule that ASVS chapters are separate arbiters.

### Phrasing B

```
EnterpriseHR: TOTP available but optional, and /admin/* requires session but no role check.
V6.3.1 and V8.3.1 both in scope.
Full 9-node trace. What are the two arbiter verdicts and the overall verdict?
```

Expected: V6-Auth: CONCERN (MFA optional). V8-AuthZ: REPROCESS (no admin role check). Overall: REPROCESS (REPROCESS overrides CONCERN).

---

## Pair CB4-3.4-153

**Stratum:** C (full chain — SSDF PO.2 + PW.2, one PASS one CONCERN)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
submission:
  system: "PaymentAPI v1.9"
  sub_phase: "3.4"
  claims:
    - "PO.2: Snyk SCA in CI; HIGH/CRITICAL CVE blocks merge; vendor list quarterly"
    - "PW.2: threat model last reviewed 10 months ago; artefact threat-model-v4.pdf dated 2025-07-15"
```

### B (full 9-node trace)

```
node_01_domain_splitter:
  territories:
    - T-ssdf-po2: [PO.2, SCA, Snyk, vendor list, third-party]
    - T-ssdf-pw2: [PW.2, threat model, 10 months, threat-model-v4.pdf]

node_02_domain_translator:
  - T-ssdf-po2 → label: "ssdf-process-evidence", sub-practice: PO.2
  - T-ssdf-pw2 → label: "ssdf-process-evidence", sub-practice: PW.2

node_03_scheme_planner:
  plan_type: parallel_with_sync
  branches: [B1: T-ssdf-po2, B2: T-ssdf-pw2]
  sync_at: node_07

node_04_domain_processor:
  T-ssdf-po2:
    PO.2: tooling + policy + governance all present — PASS
  T-ssdf-pw2:
    PW.2: threat model dated 2025-07-15 = 9.6 months old (today: 2026-05-05); approaching annual cycle — CONCERN (not yet exceeded; renewal recommended)

node_05_applier:
  T-ssdf-po2: payment processing API; PO.2 satisfied; no additional context concerns
  T-ssdf-pw2: payment API, SOC 2 in 8 months; schedule refresh within 2 months to maintain annual cycle

node_06_result_translator:
  T-ssdf-po2: [PO.2, SCA, Snyk, vendor review] → PASS
  T-ssdf-pw2: [PW.2, threat model, annual cycle, 10 months] → CONCERN

node_07_arbiter_selector:
  - SSDF-PO-Arbiter (PO.2 signals)
  - SSDF-PW-Arbiter (PW.2 signals)

node_08_arbiter_units:
  SSDF-PO-Arbiter:
    verdict: PASS
    satisfied: [PO.2.1]
    summary: "SCA tooling, blocking policy, and quarterly vendor review satisfy PO.2"
  SSDF-PW-Arbiter:
    verdict: CONCERN
    practice: PW.2
    gap: "Threat model 10 months old; annual review cycle approaching"
    action_recommended: "Schedule threat model review within 2 months"

node_09_formatter:
  overall_verdict: CONCERN
  arbiters_run: 2
  arbiters_passed: 1
  arbiters_concern: 1
  findings:
    - arbiter: SSDF-PO-Arbiter
      verdict: PASS
      practices_satisfied: [PO.2.1]
    - arbiter: SSDF-PW-Arbiter
      verdict: CONCERN
      gap: "Threat model approaching annual review cycle"
      action_recommended: "Schedule review within 2 months"
  scope_boundary: "SSDF PO and PW practices assessed for submitted artefacts. ASVS runtime controls not in scope."
```

### Reason

Two SSDF territories producing two arbiters (SSDF-PO and SSDF-PW). Both are SSDF but different practice families — they route to separate arbiters. PO.2 PASS and PW.2 CONCERN. Overall CONCERN (one concern, no REPROCESS). The scope boundary explicitly excludes ASVS from the report.

### Phrasing B

```
PaymentAPI: PO.2 satisfied (Snyk, quarterly vendor review). PW.2: threat model 10 months old.
Full pipeline trace. SSDF-PO and SSDF-PW arbiters.
```

Expected: PO.2 → SSDF-PO-Arbiter PASS. PW.2 → SSDF-PW-Arbiter CONCERN. Overall: CONCERN.

---

## Pair CB4-3.4-154

**Stratum:** C (full chain — OAuth device flow + V10.3 refresh token revocation)
**Source:** IETF RFC 8628, OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
submission:
  system: "SmartTV App v3.0"
  sub_phase: "3.4"
  claims:
    - "OAuth Device Authorization Grant (RFC 8628): device_code, user_code, 5s poll, 5min expiry"
    - "V10.3.1: revocation endpoint present at /oauth/revoke"
    - "V10.3.2: access tokens revocable"
    - "V10.3.3: refresh tokens — no revocation mechanism, 7-day validity"
```

### B (full 9-node trace)

```
node_01_domain_splitter:
  territories:
    - T-rfc-device-flow: [RFC 8628, device_code, user_code, poll interval]
    - T-asvs-token-revoke: [V10.3.1, V10.3.2, V10.3.3, refresh token, revocation]

node_02_domain_translator:
  - T-rfc-device-flow → label: "rfc-oauth2-normative", domain: RFC-OAuth2 (RFC 8628)
  - T-asvs-token-revoke → label: "oauth-oidc", domain: ASVS-V10

node_03_scheme_planner:
  plan_type: parallel_with_sync
  branches: [B1: T-rfc-device-flow, B2: T-asvs-token-revoke]
  sync_at: node_07

node_04_domain_processor:
  T-rfc-device-flow:
    RFC8628: all MUST requirements satisfied — PASS
    PKCE absent: correct (device flow has no redirect_uri)
  T-asvs-token-revoke:
    V10.3.1: endpoint present — PASS
    V10.3.2: access token revocation — PASS
    V10.3.3: refresh token no revocation — CONCERN

node_05_applier:
  T-rfc-device-flow: SmartTV, constrained device; device flow is the correct choice
  T-asvs-token-revoke: 2,000 third-party apps; de-authorized apps retain access 7 days

node_06_result_translator:
  T-rfc-device-flow: [RFC8628, device_code, user_code, poll interval] → PASS
  T-asvs-token-revoke: [V10.3.3, refresh token, revocation, 7-day] → CONCERN

node_07_arbiter_selector:
  - RFC-OAuth2-Arbiter (RFC 8628 device flow signals)
  - ASVS-V10-OAuth-Arbiter (V10.3.x signals)

node_08_arbiter_units:
  RFC-OAuth2-Arbiter:
    verdict: PASS
    satisfied: [RFC8628-§3.2, RFC8628-§3.3]
    summary: "Device Authorization Grant satisfies RFC 8628 requirements"
  ASVS-V10-OAuth-Arbiter:
    verdict: CONCERN
    requirements: [V10.3.1, V10.3.2, V10.3.3]
    gap: "V10.3.3: refresh tokens non-revocable, 7-day validity"
    action_recommended: "Implement refresh token revocation and consider token rotation"

node_09_formatter:
  overall_verdict: CONCERN
  arbiters_run: 2
  arbiters_passed: 1
  arbiters_concern: 1
  findings:
    - arbiter: RFC-OAuth2-Arbiter
      verdict: PASS
      satisfied: [RFC8628-§3.2, RFC8628-§3.3]
    - arbiter: ASVS-V10-OAuth-Arbiter
      verdict: CONCERN
      gap: "V10.3.3 refresh token revocation absent"
      action_recommended: "Implement refresh token revocation + rotation"
  attribution: "OWASP ASVS 5.0.0 CC BY-SA 4.0"
```

### Reason

Two territories: RFC 8628 device flow (PASS) and ASVS V10.3 refresh token (CONCERN). RFC PASS does not compensate for ASVS CONCERN. Overall: CONCERN. Device flow PKCE-absent is explicitly noted as correct (not a gap).

### Phrasing B

```
SmartTV OAuth: RFC 8628 device flow (all MUST satisfied). ASVS V10.3: access tokens revocable, refresh tokens not.
Full pipeline trace.
```

Expected: RFC-OAuth2-Arbiter PASS. ASVS-V10-OAuth-Arbiter CONCERN (V10.3.3). Overall CONCERN.

---

## Pair CB4-3.4-155

**Stratum:** C (full chain — API key lifecycle CONCERN + CWE-352 CSRF REPROCESS)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
submission:
  system: "WeatherAPI v2.5"
  sub_phase: "3.4"
  claims:
    - "API key auth: 32-char random hex, bcrypt stored, HTTPS only; no expiry, manual revocation"
    - "POST /api/account/settings: no CSRF token, no SameSite on session cookie"
    - "the /settings endpoint changes email and password"
```

### B (abbreviated chain — key nodes)

```
node_01: [T-asvs-auth: API key lifecycle] + [T-cwe-authz: POST settings, no CSRF]
node_02: asvs-authentication (V6) + cwe-authz (CWE-352)
node_04: API key CONCERN (no expiry) + CWE-352 REPROCESS (state-changing, no CSRF)
node_07: ASVS-V6-Auth-Arbiter + CWE-Authz-Arbiter
node_08:
  ASVS-V6: CONCERN (key lifecycle)
  CWE-Authz: REPROCESS, CWE-352, HIGH, POST /settings changes email+password without CSRF
node_09:
  overall_verdict: REPROCESS
  V6-Auth: CONCERN (key lifecycle action_recommended)
  CWE-Authz: REPROCESS, CWE-352 HIGH, action_required: add CSRF token or SameSite=Strict
```

### Reason

CONCERN (API key) + REPROCESS (CWE-352) → overall REPROCESS. Password and email change endpoint without CSRF protection is high severity. The API key concern is noted but does not change the final verdict. CWE-352 REPROCESS takes precedence.

### Phrasing B

```
WeatherAPI: API key auth (no expiry) and POST /settings (email+password change, no CSRF).
Abbreviated chain. What are the two arbiter verdicts and the overall?
```

Expected: V6-Auth CONCERN (no expiry). CWE-Authz REPROCESS CWE-352 (CSRF on account settings). Overall: REPROCESS.

---

## Pair CB4-3.4-156

**Stratum:** D (full chain — adversarial: prior-audit argument + wrong ASVS version ID)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
submission:
  system: "LegacyApp v7.1"
  sub_phase: "3.4"
  claims:
    - "PW.2: SOC 2 audit report 2024-01-20 confirmed threat model was adequate"
    - "V2.1.1 (ASVS): password minimum 12 characters enforced"
    - "team note: we passed SOC 2 Type I in January 2024; all controls confirmed"
    - "current date: 2026-05-05"
```

### B (abbreviated chain — key decision points)

```
node_01: [T-ssdf-pw2: audit report 2024] + [T-asvs-auth: V2.1.1 password]
node_04:
  T-ssdf-pw2: audit 2024-01-20 = 27 months ago; no current artefact → REPROCESS
  T-asvs-auth: V2.1.1 does not exist in ASVS 5.0.0 (4.0.3 ID); → CONCERN (version mismatch)
node_07: SSDF-PW-Arbiter + ASVS-V6-Auth-Arbiter
node_08:
  SSDF-PW-Arbiter: REPROCESS — 27-month gap, audit report ≠ current threat model
  ASVS-V6-Auth-Arbiter: CONCERN — V2.1.1 is ASVS 4.0.3 ID; pipeline uses 5.0.0; resubmit with V6.x ID
node_09:
  overall_verdict: REPROCESS
  SSDF-PW: REPROCESS (prior audit does not certify current state)
  ASVS-V6: CONCERN (wrong version ID; implementation likely correct but reference invalid)
```

### Reason

Two adversarial scenarios in one submission: prior-audit argument (SSDF REPROCESS) and wrong ASVS version ID (CONCERN). Overall REPROCESS because SSDF-PW-Arbiter returns REPROCESS. The V2.1.1 version mismatch produces CONCERN (not REPROCESS) because the implementation evidence appears plausible — just incorrectly referenced.

### Phrasing B

```
LegacyApp: SOC 2 Type I audit January 2024 cited for PW.2. V2.1.1 (ASVS) cited for password length.
Today 2026-05-05.
Full pipeline: what does SSDF-PW-Arbiter return, and what does ASVS-V6-Auth-Arbiter return?
```

Expected: SSDF-PW REPROCESS (2024 audit = 27 months ago; no current artefact). ASVS-V6 CONCERN (V2.1.1 is 4.0.3 ID; 5.0.0 uses V6.x). Overall: REPROCESS.

---

## Pair CB4-3.4-157

**Stratum:** D (full chain — partial HS256 migration; RFC REPROCESS even with partial RS256)
**Source:** IETF RFC 7519, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
submission:
  system: "MicroserviceMesh v2.0"
  sub_phase: "3.4"
  claims:
    - "V9.1.1: RS256 on all new /api/v2/* endpoints"
    - "V9.2.1: exp=15min on all endpoints"
    - "RFC7519-§4.1.6: RS256 on v2 endpoints"
    - "legacy /api/v1/* endpoints: HS256, migration to RS256 scheduled Q3 2026"
    - "v1 endpoints: <5% of traffic"
```

### B (abbreviated chain)

```
node_01: [T-asvs-token: V9.1.1 partial + V9.2.1] + [T-rfc-jwt: RFC7519 partial]
node_04:
  V9.1.1: v2 RS256 PASS; v1 HS256 still active → REPROCESS (partial ≠ compliant)
  V9.2.1: exp=15min all endpoints — PASS
  RFC7519-§4.1.6: v2 satisfies; v1 HS256 active → REPROCESS
node_08:
  ASVS-V9-Token-Arbiter: REPROCESS — V9.1.1 violated on /api/v1/*
  RFC-JWT-Arbiter: REPROCESS — HS256 active on v1; <5% traffic irrelevant
node_09:
  overall_verdict: REPROCESS
  V9.2.1: PASS noted
  V9.1.1 + RFC7519-§4.1.6: REPROCESS — complete migration required before resubmission
  note: "Traffic percentage does not reduce cryptographic risk. Migration schedule is future intent, not current compliance."
```

### Reason

This Stratum D chain reinforces pair 107's training: partial migration = REPROCESS. Both ASVS-V9-Token-Arbiter and RFC-JWT-Arbiter independently return REPROCESS for the HS256 v1 endpoints. V9.2.1 PASS is noted but does not change the overall verdict.

### Phrasing B

```
v2 endpoints: RS256 (V9.1.1 + RFC7519 satisfied). v1 endpoints: HS256, <5% traffic, migration Q3 2026.
Full chain. ASVS-V9 and RFC-JWT arbiters both run. What verdict?
```

Expected: Both REPROCESS (HS256 on any active endpoint). V9.2.1 PASS noted. Overall REPROCESS.

---

## Pair CB4-3.4-158

**Stratum:** C (full chain — all-PASS baseline: V6+V7+V8+SSDF PO.2 four-domain pass)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** PASS

### A

```
submission:
  system: "EnterpriseERP v5.0"
  sub_phase: "3.4"
  claims:
    - "V6.2.3: TLS 1.3 — credential transmission confirmed"
    - "V6.2.5: bcrypt cost=12 — confirmed"
    - "V6.3.1: TOTP MFA enforced for all admin users"
    - "V7.2.1: session tokens over TLS only"
    - "V7.3.1: session invalidated on logout (server-side)"
    - "V8.1.1: per-resource ownership check on all protected resources"
    - "V8.3.1: admin role required for all /admin/* routes"
    - "SSDF PO.2: Snyk SCA, quarterly vendor review"
```

### B (abbreviated chain)

```
node_01: [T-asvs-auth: V6.2.3, V6.2.5, V6.3.1] + [T-asvs-session: V7.2.1, V7.3.1] + [T-asvs-authz: V8.1.1, V8.3.1] + [T-ssdf-po2: PO.2]
node_03: four-branch parallel plan, sync at node_07
node_04: all territories — all findings PASS
node_07: ASVS-V6-Auth, ASVS-V7-Session, ASVS-V8-AuthZ, SSDF-PO arbiters selected
node_08: all four arbiters return PASS
node_09:
  overall_verdict: PASS
  arbiters_run: 4
  arbiters_passed: 4
  findings: all PASS with requirement lists
  scope_boundary: "Authentication (V6), session (V7), authorization (V8), SSDF PO.2 assessed. Token (V9), OAuth (V10), CWE, RFC not in scope for this submission."
  attribution: "OWASP ASVS 5.0.0 CC BY-SA 4.0"
```

### Reason

Four-territory all-PASS baseline covering V6, V7, V8, and SSDF PO.2. The scope_boundary explicitly names what was assessed and what was not. MFA (V6.3.1) is enforced for admin users in this submission (contrast with pair 152 where it was optional). All four arbiters return PASS. Overall PASS.

### Phrasing B

```
EnterpriseERP: V6.2.3+V6.2.5+V6.3.1 (MFA enforced for admins), V7.2.1+V7.3.1, V8.1.1+V8.3.1, SSDF PO.2.
All controls confirmed. Full pipeline trace.
```

Expected: Four territories, four arbiters (V6, V7, V8, SSDF-PO), all PASS. Overall PASS. Scope boundary notes V9/V10/CWE/RFC not in scope.

---

## File summary

| Seq | Stratum | Sources | Verdict | Chain type |
|-----|---------|---------|---------|-----------|
| 151 | C | ASVS+CWE | REPROCESS | CWE-307 + V6 auth — bcrypt ≠ rate limiting |
| 152 | C | ASVS | REPROCESS | V8.3 admin + V6.3 MFA — REPROCESS overrides CONCERN |
| 153 | C | SSDF | CONCERN | SSDF PO.2 PASS + PW.2 CONCERN — separate practice arbiters |
| 154 | C | IETF+ASVS | CONCERN | RFC 8628 device flow PASS + V10.3 refresh token CONCERN |
| 155 | C | ASVS+CWE | REPROCESS | API key CONCERN + CWE-352 REPROCESS — REPROCESS dominates |
| 156 | D | ASVS+SSDF | REPROCESS | Prior-audit + wrong version ID — two adversarial arguments |
| 157 | D | IETF+ASVS | REPROCESS | Partial HS256 migration — traffic % irrelevant to cryptographic risk |
| 158 | C | ASVS+SSDF | PASS | All-PASS baseline: V6+V7+V8+SSDF PO.2, four territories |

**Running total: 316 instances (sequences 001–158)**
Stratum breakdown: A ~27%, B ~25%, C ~32%, D ~16%
