# Node 05 — Applier — Extension Set 1
# Stage 3 Action 3 — Coverage Extension (Stratum A domain breadth + Stratum B team context)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-135 through CB4-3.4-142
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 284 instances (sequences 001–142)

---

## Coverage targets for this file

Node 05 (Applier) takes domain processor findings and applies them to the specific
system context: deployment type, user population, risk profile, and team constraints.
The applied finding is what goes to Node 06 (Result Translator) for vocabulary preservation.

New contexts in this file:
  - SPA (single-page application) context for MFA enforcement gap
  - IoT / embedded device context for OAuth device flow
  - Financial services context for brute-force finding (CWE-307)
  - Multi-tenant SaaS context for IDOR / authorization gap
  - Startup context with limited engineering capacity (SSDF PO.2)
  - Healthcare context for session management (HIPAA adjacent)
  - Public API context for API key lifecycle
  - Developer portal context for token revocation

---

## Pair CB4-3.4-135

**Node:** 05 — Applier
**Stratum:** B (team context — SPA context for MFA optional finding)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
domain_processor_finding:
  requirement: V6.3.1
  verdict_signal: CONCERN
  gap: "MFA available (TOTP) but optional; not enforced for any user class including admins"

system_context:
  deployment: "Single-page application (React) with REST API backend"
  user_population: "Internal employees (300) + external business partners (50)"
  admin_users: 12
  product_type: "B2B SaaS platform"
  sensitivity: "Customer financial data, contract documents"
pipeline_run_id: PR-4876
```

### B

```
applied_finding:
  pipeline_run_id: PR-4876
  requirement: V6.3.1
  applied_context: "B2B SaaS platform handling financial data with 12 admin users and 350 total users (employees + external partners)."

  scope_specific_finding:
    gap: "MFA is available (TOTP) but optional for all users. For a platform handling financial data and contracts, V6.3.1 applies with heightened concern because: (1) admin users (12) have broad data access with no mandatory MFA; (2) external partner accounts represent a higher credential-compromise risk (managed outside the organization's identity controls)."
    action_recommended:
      - "Enforce MFA for all users with admin privileges (12 admin accounts)."
      - "Consider mandatory MFA for all external partner accounts given they are outside organizational identity management."
      - "Optional MFA for standard internal employees is acceptable as a starting point, but enforce for privileged and external users."
    priority: HIGH
    verdict: CONCERN
```

### Reason

The applier takes the V6.3.1 CONCERN finding and applies it to the specific context: financial data, 12 admins, 350 users including external partners. The context elevates the priority of the finding: admin accounts with broad data access and external partner accounts with higher credential-compromise risk should have mandatory MFA. The applier does not change the verdict (still CONCERN — not REPROCESS because MFA is available), but adds context-specific prioritization and targets the remediation at the highest-risk user classes.

### Phrasing B

```
V6.3.1 CONCERN (MFA optional) applied to: B2B SaaS, 12 admins, 350 users, external partners, financial data.
Node 05, run PR-4876. What is the applied finding with system-specific context?
```

Expected: CONCERN preserved. Context elevates priority for admin users and external partners. Action_recommended: mandatory MFA for admins and external partners; optional acceptable for standard internal users.

---

## Pair CB4-3.4-136

**Node:** 05 — Applier
**Stratum:** A (domain breadth — IoT/embedded device context for OAuth device flow PASS)
**Source:** IETF RFC 8628
**Verdict:** PASS

### A

```
domain_processor_finding:
  rfc: RFC-8628
  verdict_signal: PASS
  note: "Device Authorization Grant implementation satisfies RFC 8628 requirements."

system_context:
  deployment: "IoT smart thermostat; no browser or keyboard on device"
  authentication_flow: "device displays 8-character user_code; user enters code at account.example.com on separate device"
  device_type: "Embedded Linux, constrained UI"
  user_base: "Consumer residential users (~200,000 devices)"
pipeline_run_id: PR-4920
```

### B

```
applied_finding:
  pipeline_run_id: PR-4920
  rfc: RFC-8628
  applied_context: "IoT smart thermostat — constrained device without browser or keyboard. Consumer user base of ~200,000 devices."

  scope_specific_finding:
    verdict: PASS
    confirmation: "RFC 8628 Device Authorization Grant is the appropriate flow for a device without browser or keyboard capability. All RFC 8628 MUST requirements satisfied. The 8-character user_code is within the recommended format. The requirement for the user to complete authorization on a separate device (account.example.com) is the expected interaction model for device flow."
    scope_boundary: "RFC 8628 normative requirements satisfied for this deployment context. Note: the /oauth/revoke endpoint and token lifecycle management are separate from the device authorization flow and should be assessed under V10.3 if applicable."
    verdict: PASS
```

### Reason

The applier confirms that the IoT deployment context makes the Device Authorization Grant the correct flow choice (no browser/keyboard = device flow is required, not just acceptable). The scope_boundary note advises that token lifecycle (V10.3) is a separate assessment concern. For a consumer IoT product with 200,000 devices, the applier does not add additional concerns — the RFC-8628 implementation is straightforwardly correct for this context.

### Phrasing B

```
RFC 8628 PASS finding. System context: IoT thermostat, no browser/keyboard, 200,000 consumer devices.
Node 05, run PR-4920. Applied finding?
```

Expected: PASS preserved. Device flow is the correct choice for constrained device. Scope boundary notes token lifecycle is separate concern. No additional concerns for this context.

---

## Pair CB4-3.4-137

**Node:** 05 — Applier
**Stratum:** B (team context — financial services context for CWE-307 brute force REPROCESS)
**Source:** CWE Top 25 2023, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
domain_processor_finding:
  cwe_id: CWE-307
  severity: HIGH
  verdict_signal: REPROCESS
  gap: "No rate limiting or account lockout on login endpoint; unlimited attempts permitted"

system_context:
  deployment: "Online banking platform"
  user_population: "Retail banking customers (~500,000 accounts)"
  account_type: "Financial accounts with payment capabilities"
  regulatory_context: "PCI-DSS v4.0 Requirement 8.3.4 (account lockout after max attempts)"
pipeline_run_id: PR-4942
```

### B

```
applied_finding:
  pipeline_run_id: PR-4942
  cwe_id: CWE-307
  applied_context: "Online banking platform with 500,000 retail accounts holding payment capabilities. Regulatory context: PCI-DSS v4.0 Requirement 8.3.4."

  scope_specific_finding:
    verdict: REPROCESS
    gap: "No rate limiting or account lockout on the login endpoint for a platform with 500,000 financial accounts."
    elevated_risk: "Financial accounts with payment capabilities represent a high-value target for credential stuffing and brute-force attacks. The absence of lockout is a high-severity gap in this context — regulatory compliance (PCI-DSS v4.0 Req 8.3.4) independently requires lockout after a maximum number of failed attempts."
    action_required:
      - "Implement account lockout after a maximum of 10 consecutive failed login attempts (PCI-DSS v4.0 Req 8.3.4 specifies a maximum threshold; consult current PCI-DSS guidance for the exact value)."
      - "Implement progressive rate limiting per account and per IP."
      - "Add CAPTCHA or equivalent for suspicious request patterns."
    priority: CRITICAL
    regulatory_note: "PCI-DSS v4.0 Requirement 8.3.4 also mandates lockout. Compliance with this requirement satisfies both CWE-307 remediation and PCI-DSS scope for this control."
```

### Reason

The applier escalates priority to CRITICAL in the financial services context: 500,000 financial accounts with payment capabilities facing unlimited brute force = critical risk. The applier also notes the PCI-DSS regulatory context — this is not just a best-practice gap, it is a regulatory compliance requirement. The REPROCESS verdict is preserved; the context adds critical priority and regulatory context that helps the team prioritize and frame the remediation.

### Phrasing B

```
CWE-307 REPROCESS (no lockout) applied to: online banking, 500,000 accounts, payment capability, PCI-DSS scope.
Node 05, run PR-4942. Applied finding with financial services context?
```

Expected: REPROCESS preserved, priority elevated to CRITICAL. PCI-DSS v4.0 Req 8.3.4 noted as independent regulatory requirement for lockout. Action_required: account lockout, progressive rate limiting, CAPTCHA.

---

## Pair CB4-3.4-138

**Node:** 05 — Applier
**Stratum:** B (team context — multi-tenant SaaS for IDOR/authorization REPROCESS)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
domain_processor_finding:
  cwe_id: CWE-862
  severity: HIGH
  verdict_signal: REPROCESS
  gap: "No per-resource ownership check on /api/projects/{project_id}"

system_context:
  deployment: "Multi-tenant SaaS project management platform"
  tenant_isolation: "each tenant has their own project namespace"
  user_population: "10,000 tenants, average 50 users per tenant"
  data_sensitivity: "project plans, internal communications, budget data"
pipeline_run_id: PR-4967
```

### B

```
applied_finding:
  pipeline_run_id: PR-4967
  cwe_id: CWE-862
  applied_context: "Multi-tenant SaaS platform with 10,000 tenants; tenant data isolation is a core product requirement."

  scope_specific_finding:
    verdict: REPROCESS
    gap: "No per-resource ownership check on /api/projects/{project_id} — any authenticated user across all tenants can retrieve any project."
    cross_tenant_impact: "In a multi-tenant context, the CWE-862 gap is a cross-tenant data isolation failure: a user in Tenant A can access projects belonging to Tenant B. This is not only an authorization gap — it is a data isolation breach affecting all 10,000 tenants."
    action_required:
      - "Add tenant_id scoping to the project retrieval query: WHERE project_id = {id} AND tenant_id = {requesting_user.tenant_id}."
      - "Add user ownership or role check within the tenant: confirm requesting user has read access to the project within their tenant."
      - "Verify all endpoints using project_id have both tenant-scope and user-scope checks."
    priority: CRITICAL
    blast_radius: "All 10,000 tenant organizations; any project data is accessible to any authenticated user."
```

### Reason

In a multi-tenant SaaS context, an IDOR gap is a cross-tenant data isolation failure — not just a single-user authorization issue. The blast radius is all 10,000 tenants. The applier escalates to CRITICAL and adds the multi-tenancy context to the action_required: the fix must scope to tenant_id first, then user-level access within the tenant. The applier also notes the blast_radius explicitly so the team understands the full impact scope.

### Phrasing B

```
CWE-862 REPROCESS on /api/projects/{id}. System: multi-tenant SaaS, 10,000 tenants.
Any authenticated user can access any project across all tenants.
Node 05, run PR-4967. Applied finding?
```

Expected: REPROCESS preserved, priority CRITICAL, cross-tenant data isolation impact noted. Action_required: tenant_id scoping + user-level check. Blast radius: all 10,000 tenants.

---

## Pair CB4-3.4-139

**Node:** 05 — Applier
**Stratum:** B (team context — startup with limited engineering capacity; SSDF PW.2 CONCERN)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
domain_processor_finding:
  practice: PW.2
  verdict_signal: CONCERN
  gap: "Threat model last updated 10 months ago; approaching recommended annual review cycle"

system_context:
  team_size: 4 engineers
  company_stage: "seed-stage startup"
  product: "Developer API for payment processing"
  compliance_target: "SOC 2 Type II audit in 8 months"
pipeline_run_id: PR-4978
```

### B

```
applied_finding:
  pipeline_run_id: PR-4978
  practice: PW.2
  applied_context: "Seed-stage startup, 4-engineer team, payment processing API, SOC 2 audit in 8 months."

  scope_specific_finding:
    verdict: CONCERN
    gap: "Threat model 10 months old; annual review cycle approaching. For a payment processing API, the threat model should be current given the financial attack surface."
    action_recommended:
      - "Schedule threat model review within the next 2 months (before the 12-month mark)."
      - "Given the 4-engineer team, a focused threat model review (2–4 hours with the full team) is proportionate. Document the review date and any new threats identified."
      - "For the SOC 2 Type II audit in 8 months, current threat model documentation will be required as process evidence. A 10-month-old model reviewed in the next 2 months will satisfy the annual cycle for the audit window."
    priority: MEDIUM
    audit_context: "A reviewed threat model within the past 12 months is typically required for SOC 2 Type II. The 8-month audit timeline means the review should happen now to establish a documented review date within the audit period."
```

### Reason

The applier adds startup-specific context: a 4-engineer team cannot sustain the same documentation overhead as an enterprise team. The action_recommended is proportionate (a focused 2-4 hour review, not a multi-week engagement). The audit_context note is specific to the SOC 2 timeline — the team should understand that the review should happen now to establish documented evidence within the SOC 2 audit window. This is a CONCERN, not REPROCESS — the threat model exists, it just needs refreshing.

### Phrasing B

```
PW.2 CONCERN (threat model 10 months old). System: seed-stage startup, 4 engineers, SOC 2 in 8 months.
Node 05, run PR-4978. Applied finding with startup context?
```

Expected: CONCERN preserved. Review recommended within 2 months. Action proportionate to team size (2-4 hour focused review). SOC 2 audit context notes that current threat model documentation is required for the audit period.

---

## Pair CB4-3.4-140

**Node:** 05 — Applier
**Stratum:** B (team context — healthcare context for session management REPROCESS)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
domain_processor_finding:
  requirement: V7.3.1
  verdict_signal: REPROCESS
  gap: "Session not invalidated server-side on logout"

system_context:
  deployment: "Electronic health record (EHR) system"
  user_population: "Clinical staff (nurses, physicians) in hospital setting"
  session_usage: "Shared workstations common; multiple clinicians log into the same terminal during a shift"
  regulatory_context: "HIPAA requires appropriate safeguards for PHI (protected health information)"
pipeline_run_id: PR-4818
```

### B

```
applied_finding:
  pipeline_run_id: PR-4818
  requirement: V7.3.1
  applied_context: "EHR system with clinical staff using shared workstations in a hospital setting. HIPAA scope."

  scope_specific_finding:
    verdict: REPROCESS
    gap: "Session not invalidated server-side on logout. In a shared-workstation clinical environment, a clinician who logs out without server-side session invalidation leaves a valid session that the next user of that terminal could replay."
    elevated_risk: "On shared clinical workstations, session replay after a colleague's logout exposes PHI (patient records) under a different clinician's identity. This is both a security gap and a HIPAA access control violation (45 CFR §164.312(a)(2)(iii) — automatic logoff; §164.312(a)(1) — unique user identification)."
    action_required:
      - "Implement server-side session invalidation immediately on logout: the session record must be marked invalid so that any subsequent request bearing the old session token is rejected with 401."
      - "Implement automatic session timeout aligned with clinical workflow (typically 15-minute idle timeout for EHR systems)."
    priority: CRITICAL
    regulatory_note: "HIPAA 45 CFR §164.312(a)(2)(iii) requires automatic logoff procedures. Server-side session invalidation on logout is required independently of the ASVS V7.3.1 finding."
```

### Reason

The healthcare/shared-workstation context elevates V7.3.1 session invalidation from HIGH to CRITICAL. On shared clinical terminals, a session that survives logout can expose PHI under a different clinician's identity — this is a HIPAA violation, not just a security gap. The applier adds the regulatory context and escalates priority. The action_required adds the automatic idle timeout as a second recommended control (appropriate for clinical workflows where clinicians may step away without logging out).

### Phrasing B

```
V7.3.1 REPROCESS (session not server-invalidated on logout). System: EHR, shared hospital workstations, HIPAA scope.
Node 05, run PR-4818. Applied finding?
```

Expected: REPROCESS preserved, priority CRITICAL. Shared workstation context: session replay after logout = PHI exposure under wrong clinician identity. HIPAA 45 CFR §164.312 noted. Action_required: server-side session invalidation + idle timeout.

---

## Pair CB4-3.4-141

**Node:** 05 — Applier
**Stratum:** A (domain breadth — public API context for API key lifecycle CONCERN)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
domain_processor_finding:
  requirements: ["V6 key lifecycle"]
  verdict_signal: CONCERN
  gap: "API keys have no expiry and manual-only revocation"

system_context:
  deployment: "Public REST API for weather data"
  user_population: "Registered developers — 15,000 API key holders"
  key_issuance: "self-service via developer portal"
  api_tier: "free tier (rate limited) and paid tier (higher limits)"
pipeline_run_id: PR-4909
```

### B

```
applied_finding:
  pipeline_run_id: PR-4909
  requirements: ["V6 key lifecycle"]
  applied_context: "Public weather data API with 15,000 developer API key holders across free and paid tiers."

  scope_specific_finding:
    verdict: CONCERN
    gap: "15,000 API keys with no expiry and manual-only revocation. Keys issued to external developers remain valid indefinitely, including abandoned developer accounts, test keys from completed projects, and keys that may have been exposed in public code repositories."
    context_risk: "Public API keys are at elevated risk of accidental exposure (GitHub commits, blog posts, tutorials). Without expiry or self-service revocation, an exposed key remains exploitable until a support request is processed."
    action_recommended:
      - "Implement optional key expiry (configurable by developer: 30/90/365 days or no expiry). This allows developers who want non-expiring keys for automation to choose accordingly while encouraging periodic rotation."
      - "Add self-service key revocation and re-issuance in the developer portal."
      - "Add key scanning alert: notify developers when their key is detected in a public repository (GitHub secret scanning integration)."
    priority: MEDIUM
```

### Reason

The public API context with 15,000 external developers adds specific risk factors: accidental key exposure in public repositories, abandoned developer accounts, test keys from completed projects. The applier makes the gap more concrete with these context-specific examples. The action_recommended is calibrated to the public API developer context: optional expiry (not mandatory — automation use cases need non-expiring keys) and self-service revocation.

### Phrasing B

```
API key lifecycle CONCERN (no expiry, manual revocation). System: public weather API, 15,000 developer keys.
Node 05, run PR-4909. Applied finding?
```

Expected: CONCERN preserved. Context: 15,000 external developer keys at risk of public repository exposure. Action_recommended: optional expiry, self-service revocation in developer portal, GitHub secret scanning integration. Priority MEDIUM.

---

## Pair CB4-3.4-142

**Node:** 05 — Applier
**Stratum:** A (domain breadth — developer portal context for OAuth token revocation CONCERN)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
domain_processor_finding:
  requirements: [V10.3.1, V10.3.2, V10.3.3]
  verdict_signal: CONCERN
  gap: "V10.3.3: refresh tokens have no revocation mechanism; valid 7 days"

system_context:
  deployment: "OAuth 2.0 authorization server for a developer API platform"
  token_grant_type: "Authorization code + PKCE for third-party applications"
  third_party_apps: "2,000 registered third-party applications"
  refresh_token_usage: "long-lived access for mobile apps and server-side integrations"
pipeline_run_id: PR-4931
```

### B

```
applied_finding:
  pipeline_run_id: PR-4931
  requirements: [V10.3.1, V10.3.2, V10.3.3]
  applied_context: "OAuth authorization server serving 2,000 third-party applications with long-lived refresh tokens."

  scope_specific_finding:
    v10_3_1_v10_3_2: "PASS — revocation endpoint and access token revocation confirmed."
    v10_3_3:
      verdict: CONCERN
      gap: "Refresh tokens are non-revocable and valid for 7 days. For a platform with 2,000 third-party applications, a compromised or withdrawn third-party application cannot have its refresh tokens invalidated — the tokens remain active for up to 7 days after the app is de-authorized."
      action_recommended:
        - "Implement refresh token revocation: when a user revokes an application's access or an app is de-authorized, all associated refresh tokens must be immediately invalidated."
        - "Consider implementing refresh token rotation: issue a new refresh token with each use, invalidating the previous one. This limits the refresh token replay window to the interval between uses."
        - "Add token family tracking to detect refresh token theft (RFC 6819 §4.4.2-style token rotation)."
      priority: HIGH
      context_note: "Third-party application context: when a user withdraws consent from an application, non-revocable refresh tokens mean the application retains API access for up to 7 days post-de-authorization — a platform trust and data sovereignty concern."
```

### Reason

For an OAuth authorization server with 2,000 third-party applications, the non-revocable refresh token gap has a specific platform impact: de-authorized applications retain API access. The applier makes this concrete with the de-authorization scenario and recommends refresh token rotation as an additional mitigation. The context_note frames the issue as a platform trust concern — not just a security implementation gap.

### Phrasing B

```
V10.3 CONCERN (refresh tokens non-revocable, 7-day validity). System: OAuth server, 2,000 third-party apps.
Node 05, run PR-4931. Applied finding with third-party app context?
```

Expected: V10.3.1+V10.3.2 PASS. V10.3.3 CONCERN elevated to priority HIGH. De-authorization scenario: withdrawn apps retain access 7 days. Action_recommended: revocation on de-auth, refresh token rotation, token family tracking.

---

## File summary

| Seq | Stratum | Source | Verdict | System context applied |
|-----|---------|--------|---------|----------------------|
| 135 | B | ASVS | CONCERN | B2B SaaS: MFA optional — elevated priority for admins and external partners |
| 136 | A | IETF | PASS | IoT device: Device flow is the correct choice; constrained device context |
| 137 | B | CWE | REPROCESS | Financial services: CWE-307 + PCI-DSS Req 8.3.4 + CRITICAL priority |
| 138 | B | CWE+ASVS | REPROCESS | Multi-tenant SaaS: IDOR = cross-tenant isolation failure, 10,000 tenants |
| 139 | B | SSDF | CONCERN | Startup: PW.2 CONCERN with proportionate review + SOC 2 audit timing |
| 140 | B | ASVS | REPROCESS | EHR/healthcare: session invalidation + HIPAA + shared workstations |
| 141 | A | ASVS | CONCERN | Public API: key lifecycle CONCERN + 15,000 external developers |
| 142 | A | ASVS | CONCERN | OAuth server: refresh token revocation + de-authorization scenario |

**Running total: 284 instances (sequences 001–142)**
Stratum breakdown: A ~29%, B ~29%, C ~26%, D ~16%
