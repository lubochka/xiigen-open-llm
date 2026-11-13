# Node 04 — Domain Processor — Extension Set 1
# Stage 3 Action 3 — Coverage Extension (Stratum A domain breadth)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-127 through CB4-3.4-134
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 268 instances (sequences 001–134)

---

## Coverage targets for this file

Stratum A: domain breadth — new security domains not previously processed in Node 04.
New domains introduced:
  - Multi-factor authentication (MFA) — ASVS V6.3.x requirements
  - Administrative function access control — ASVS V8.3.x
  - CWE-352 (Cross-Site Request Forgery) — a state-changing operation without CSRF token
  - API key authentication — not JWT-based; different trust model
  - OAuth device authorization grant — RFC 8628 device flow
  - ASVS V10.3 (OAuth token introspection / revocation)
  - CWE-307 (Improper Restriction of Excessive Authentication Attempts)
  - SSDF PO.1 (organizational security policy) vs PO.2 (tool acquisition)

---

## Pair CB4-3.4-127

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — MFA requirement processing)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
territory: T-asvs-authentication
domain_label: "asvs-authentication"
input_claims:
  - "V6.3.1: system supports TOTP (time-based OTP) as second factor"
  - "TOTP implementation: Google Authenticator compatible, 30-second window"
  - "MFA is optional — users can enable it in account settings; it is not enforced for any user class"
sub_phase: "3.4"
pipeline_run_id: PR-4876
```

### B

```
domain_processor_output:
  territory: T-asvs-authentication
  pipeline_run_id: PR-4876

  finding:
    requirement: V6.3.1
    claim: "TOTP MFA available but optional."
    evaluation: "V6.3.1 requires that a second authentication factor be available. The system provides TOTP — this satisfies availability. However, ASVS requires MFA to be enforced for privileged access (V6.3.2 covers mandatory MFA for admin/privileged paths). If V6.3.2 is in scope, optional MFA for all users including admins does not satisfy the mandatory enforcement requirement."
    verdict_signal: CONCERN
    note: "CONCERN based on V6.3.1 scope: MFA is available (partial). If V6.3.2 (mandatory MFA for privileged users) is also in scope, resubmit with clarification of whether any user class has mandatory MFA enforcement. The gap is enforcement scope, not mechanism availability."
```

### Reason

ASVS V6.3.1 requires that a second factor be available. The system provides TOTP — that part is satisfied. The CONCERN arises because MFA is entirely optional with no enforcement for any user class, including administrators. If V6.3.2 (mandatory MFA for privileged paths) is in scope, this is a gap. Node 04 processes the claim and identifies the partial satisfaction: mechanism is present (satisfies V6.3.1) but enforcement is absent (may violate V6.3.2). CONCERN signals that the submission needs to clarify whether privileged-path MFA enforcement is required.

### Phrasing B

```
ASVS V6.3.1. TOTP MFA available but optional for all users.
Domain processor evaluates: does optional TOTP satisfy V6.3.1? What is the verdict signal?
Pipeline PR-4876.
```

Expected: CONCERN. TOTP available (V6.3.1 partial) but not enforced for any user class. V6.3.2 mandatory MFA for privileged paths may also apply. Note asks for clarification of enforcement scope.

---

## Pair CB4-3.4-128

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — administrative function access control)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
territory: T-asvs-authz
domain_label: "asvs-authorization"
input_claims:
  - "V8.3.1: administrative functions accessible via /admin/* routes"
  - "/admin/* routes require the user to be authenticated (valid session)"
  - "no additional role check beyond authentication is applied to /admin/* routes"
  - "team rationale: admin functions are behind our SSO provider, so only employees can log in"
sub_phase: "3.4"
pipeline_run_id: PR-4887
```

### B

```
domain_processor_output:
  territory: T-asvs-authz
  pipeline_run_id: PR-4887

  finding:
    requirement: V8.3.1
    claim: "Admin routes require authentication only; no role-based restriction."
    evaluation: "V8.3.1 requires that administrative functionality be accessible only to users with explicit administrative authorization. Authentication (confirming identity) is not authorization (confirming role permission). SSO ensuring only employees can log in does not restrict which authenticated employees can access admin functions. Any authenticated employee can access /admin/* routes."
    verdict_signal: REPROCESS
    note: "REPROCESS: authentication ≠ authorization. V8.3.1 requires a role check specifically for administrative access. The SSO perimeter controls who is an authenticated user; a separate role check (admin role, explicit permission grant) controls which users are authorized for administrative functions."
```

### Reason

V8.3.1 requires authorization checks on administrative functions — not just authentication. The team's SSO argument is a perimeter argument: SSO ensures only employees authenticate, but does not enforce that only designated administrators access admin functions. Any of the organization's employees can access /admin/* routes. This is the same ASVS-authentication vs ASVS-authorization distinction applied to the V8.3 context. REPROCESS (not CONCERN) because there is no authorization check at all — authentication is present, authorization is absent.

### Phrasing B

```
/admin/* routes: authentication required (SSO), but no role-based restriction.
V8.3.1. Team says SSO ensures only employees can log in — so admin functions are protected.
Node 04, run PR-4887. Verdict signal?
```

Expected: REPROCESS. Authentication ≠ authorization. SSO = employee perimeter, not admin role restriction. V8.3.1 requires explicit admin authorization check, not just authenticated access.

---

## Pair CB4-3.4-129

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — CWE-352 CSRF on state-changing operation)
**Source:** CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
territory: T-cwe-authz
domain_label: "cwe-authz"
input_claims:
  - "POST /api/account/delete — deletes the authenticated user's account"
  - "endpoint requires valid session cookie for authentication"
  - "no CSRF token required in the request"
  - "no SameSite cookie attribute set on session cookie"
  - "team note: account deletion requires re-entering password; CSRF attack would also need to know the password"
sub_phase: "3.4"
pipeline_run_id: PR-4898
```

### B

```
domain_processor_output:
  territory: T-cwe-authz
  pipeline_run_id: PR-4898

  finding:
    cwe_id: CWE-352
    claim: "State-changing endpoint (account deletion) without CSRF protection."
    evaluation: "CWE-352 (Cross-Site Request Forgery): POST /api/account/delete performs an irreversible state change. No CSRF token and no SameSite cookie attribute. The team's password re-entry argument addresses one exploitation path — but CSRF protection is required independently of password confirmation, since the request pattern (no CSRF token, no SameSite) can be exploited in browser contexts where a cross-origin request triggers the authenticated action."
    severity: HIGH
    verdict_signal: REPROCESS
    note: "Password confirmation mitigates one attack vector; it does not satisfy CSRF protection requirements. CSRF token or SameSite=Strict/Lax on the session cookie is the standard remediation."
```

### Reason

CWE-352 (CSRF) applies to state-changing operations where a cross-origin request can trigger an authenticated action. The account deletion endpoint performs an irreversible action. The password confirmation partially mitigates direct exploitation (an attacker needs to know the password to complete the action) but does not satisfy the CSRF protection requirement — CSRF protection must prevent the cross-origin request from being issued in the first place. Additionally, the absence of SameSite on the session cookie makes the session cookie available to cross-origin requests. REPROCESS because both CSRF token and SameSite attribute are absent.

### Phrasing B

```
POST /api/account/delete: no CSRF token, no SameSite cookie, session cookie only.
Team argues password re-entry protects against CSRF.
Node 04, cwe-authz territory, run PR-4898.
```

Expected: REPROCESS. CWE-352. No CSRF token + no SameSite = CSRF vulnerability. Password re-entry partially mitigates but does not satisfy CSRF protection requirement. Remediation: CSRF token or SameSite=Strict/Lax.

---

## Pair CB4-3.4-130

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — API key authentication; not JWT-based; different trust model)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
territory: T-asvs-authentication
domain_label: "asvs-authentication"
input_claims:
  - "authentication mechanism: API key (static secret) in Authorization header"
  - "API keys are 32 random hex characters generated at account creation"
  - "API key stored in database as bcrypt hash"
  - "no expiry mechanism on API keys — once issued, valid indefinitely"
  - "V6.2.3 (TLS): API key transmitted over HTTPS only — confirmed"
  - "no key rotation mechanism; keys can only be manually revoked by support team"
sub_phase: "3.4"
pipeline_run_id: PR-4909
```

### B

```
domain_processor_output:
  territory: T-asvs-authentication
  pipeline_run_id: PR-4909

  finding:
    requirements_evaluated: [V6.2.3, "V6 key lifecycle"]
    claim_v6_2_3: "TLS enforced — V6.2.3 satisfied."
    claim_key_lifecycle:
      evaluation: "API keys without expiry or automated rotation create a persistent credential risk. A compromised API key remains valid indefinitely unless manually revoked. ASVS V6 generally requires that credentials have defined lifecycle management. The absence of expiry and the manual-only revocation path are deviations from good practice."
      verdict_signal: CONCERN
    overall_verdict_signal: CONCERN
    note: "CONCERN on key lifecycle management. V6.2.3 satisfied. Recommended: implement key expiry (configurable, e.g., 90-day or 1-year default with renewal) and provide a self-service revocation mechanism for API key holders."
```

### Reason

API keys are a valid authentication mechanism but carry lifecycle management requirements. Keys without expiry and with manual-only revocation are a concern — not a MUST violation (ASVS does not mandate a specific expiry period for API keys in most sub-profiles) but a gap from best practice. V6.2.3 (TLS) is satisfied. The CONCERN covers key lifecycle: no expiry, manual revocation only, no rotation. Node 04 must process each claim independently and produce the appropriate verdict signal for each aspect of the territory.

### Phrasing B

```
API key authentication. 32-char random hex. Stored as bcrypt hash. TLS: HTTPS only.
No expiry, no rotation, manual-only revocation.
Node 04, asvs-authentication territory, V6 requirements. Run PR-4909.
```

Expected: V6.2.3 PASS (TLS). CONCERN on key lifecycle (no expiry, manual revocation). Overall CONCERN for the territory.

---

## Pair CB4-3.4-131

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — OAuth device authorization grant; RFC 8628)
**Source:** IETF RFC 8628, OWASP ASVS 5.0.0
**Verdict:** PASS

### A

```
territory: T-rfc-oauth2-normative
domain_label: "rfc-oauth2-normative"
input_claims:
  - "OAuth flow: Device Authorization Grant (RFC 8628)"
  - "device code endpoint returns device_code, user_code, verification_uri"
  - "polling interval: 5 seconds (per RFC 8628 §3.3 minimum)"
  - "device code expiry: 5 minutes"
  - "PKCE is not applicable — device flow uses polling model not redirect model"
  - "token endpoint requires client authentication; public client with client_id only"
sub_phase: "3.4"
pipeline_run_id: PR-4920
```

### B

```
domain_processor_output:
  territory: T-rfc-oauth2-normative
  pipeline_run_id: PR-4920

  finding:
    rfc: RFC-8628
    claim: "Device Authorization Grant implementation matches RFC 8628 requirements."
    evaluation:
      - "device_code, user_code, verification_uri returned: RFC 8628 §3.2 MUST satisfied"
      - "polling interval 5 seconds: RFC 8628 §3.3 minimum respected"
      - "device_code expiry 5 minutes: within RFC 8628 §3.2 reasonable duration guidance"
      - "PKCE not applicable: Device flow does not use redirect_uri and authorization code exchange — PKCE applies to authorization code flow only. Absence of PKCE is correct for device flow."
      - "public client with client_id: acceptable for device flow per RFC 8628 §3.1"
    verdict_signal: PASS
    note: "RFC 8628 Device Authorization Grant is a distinct OAuth 2.0 grant type from authorization code flow (RFC 6749 §4.1). PKCE absence is correct and expected. RFC-OAuth2-Arbiter should evaluate RFC 8628 compliance for this territory, not RFC 6749 §4.1."
```

### Reason

Device Authorization Grant (RFC 8628) is a separate grant type from the authorization code flow. PKCE applies to authorization code flow where a redirect_uri is used — it is explicitly not applicable to device flow. Node 04 must process the PKCE-absent claim correctly: PKCE absence on device flow is correct, not a gap. The five-second polling interval satisfies the RFC 8628 §3.3 requirement that clients poll no faster than the interval returned by the device code endpoint (5s is the minimum the server may specify). All claims are satisfied.

### Phrasing B

```
RFC 8628 Device Authorization Grant. device_code, user_code, verification_uri returned.
Polling 5s, device_code expiry 5min. No PKCE (device flow has no redirect).
Node 04, rfc-oauth2-normative territory. Run PR-4920.
```

Expected: PASS. All RFC 8628 MUST requirements satisfied. PKCE absence is correct for device flow (not a gap). Polling 5s satisfies minimum interval requirement.

---

## Pair CB4-3.4-132

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — OAuth token revocation; ASVS V10.3)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
territory: T-oauth-oidc
domain_label: "oauth-oidc"
input_claims:
  - "V10.3.1: token revocation endpoint present at /oauth/revoke"
  - "V10.3.2: access tokens can be revoked; system validates against revocation list on each request"
  - "V10.3.3: refresh tokens: no revocation mechanism; once issued, valid until expiry (7 days)"
sub_phase: "3.4"
pipeline_run_id: PR-4931
```

### B

```
domain_processor_output:
  territory: T-oauth-oidc
  pipeline_run_id: PR-4931

  finding_v10_3_1:
    requirement: V10.3.1
    evaluation: "Revocation endpoint present — satisfied."
    verdict_signal: PASS

  finding_v10_3_2:
    requirement: V10.3.2
    evaluation: "Access token revocation present with per-request validation — satisfied."
    verdict_signal: PASS

  finding_v10_3_3:
    requirement: V10.3.3
    evaluation: "Refresh tokens have no revocation mechanism; valid for 7 days after issuance with no way to invalidate before expiry. V10.3.3 requires refresh tokens to be revocable. A 7-day non-revocable refresh token represents an extended window where a compromised refresh token cannot be invalidated."
    verdict_signal: CONCERN
    note: "CONCERN not REPROCESS: access token revocation is implemented (reducing immediate exposure). The gap is specifically refresh token revocation. Refresh tokens with 7-day validity and no revocation path should be flagged for remediation. Recommended: extend the revocation mechanism to cover refresh tokens."

  overall_verdict_signal: CONCERN
```

### Reason

ASVS V10.3 has three sub-requirements. Two are satisfied (revocation endpoint, access token revocation). One is not (refresh token revocation absent). The domain processor must evaluate each sub-requirement independently and return the aggregate verdict signal. The finding for V10.3.3 is CONCERN (not REPROCESS) because access token revocation is implemented — the missing element is refresh token revocation, which extends the risk window but does not create an immediate catastrophic gap given that access tokens are revocable. Node 04 produces three separate findings, one per sub-requirement.

### Phrasing B

```
ASVS V10.3 token revocation. V10.3.1: endpoint present. V10.3.2: access token revocable.
V10.3.3: refresh tokens — no revocation, valid 7 days.
Node 04, oauth-oidc territory. Run PR-4931.
```

Expected: V10.3.1 PASS, V10.3.2 PASS, V10.3.3 CONCERN (no refresh token revocation). Overall territory signal: CONCERN.

---

## Pair CB4-3.4-133

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — CWE-307 brute force / account lockout)
**Source:** CWE Top 25 2023, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
territory: T-cwe-authn
domain_label: "cwe-authn"
input_claims:
  - "CWE-307 concern: login endpoint has no rate limiting or account lockout"
  - "POST /api/auth/login accepts unlimited credential attempts"
  - "no IP-based rate limiting"
  - "no account lockout after failed attempts"
  - "team note: we use bcrypt cost=12 which makes brute force very slow already"
sub_phase: "3.4"
pipeline_run_id: PR-4942
```

### B

```
domain_processor_output:
  territory: T-cwe-authn
  pipeline_run_id: PR-4942

  finding:
    cwe_id: CWE-307
    claim: "No rate limiting or account lockout on login endpoint."
    evaluation: "CWE-307 (Improper Restriction of Excessive Authentication Attempts): unlimited login attempts permitted. bcrypt cost=12 slows per-attempt computation to ~100ms — at that rate, a distributed attack across 1,000 nodes can attempt ~600,000 passwords per minute. The bcrypt cost does not prevent brute force; it raises the attacker's cost but does not make brute force infeasible at scale."
    severity: HIGH
    verdict_signal: REPROCESS
    note: "REPROCESS: bcrypt slows individual attempts but does not constitute rate limiting. Rate limiting (e.g., max 5 attempts per 15 minutes per account) and account lockout (with unlock mechanism) are independent controls from hashing algorithm choice. Both are needed."
```

### Reason

CWE-307 requires that excessive authentication attempts be restricted. bcrypt hashing makes each attempt more computationally expensive but does not cap the number of attempts per unit time — a distributed attacker can still attempt many passwords. The team conflates hashing cost with rate limiting. Rate limiting (per IP, per account) and account lockout are separate controls that operate at the network/application layer, not the cryptographic layer. REPROCESS because no attempt restriction exists at the application layer.

### Phrasing B

```
Login endpoint: no rate limit, no lockout. Team says bcrypt cost=12 makes brute force slow.
CWE-307, cwe-authn territory, run PR-4942.
Does bcrypt cost satisfy the rate limiting requirement?
```

Expected: REPROCESS. CWE-307. bcrypt raises per-attempt cost but does not cap attempt count. Distributed attack still feasible. Rate limiting and lockout are required independently of hashing algorithm.

---

## Pair CB4-3.4-134

**Node:** 04 — Domain Processor
**Stratum:** A (domain breadth — SSDF PO.2 tool acquisition vs PO.1 organizational policy)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** PASS

### A

```
territory: T-ssdf-process
domain_label: "ssdf-process-evidence"
input_claims:
  - "SSDF PO.2: organization evaluates and approves third-party software components before use"
  - "software composition analysis (SCA) tool: Snyk, integrated in CI pipeline"
  - "component approval process: Snyk license report reviewed in each PR; components with HIGH or CRITICAL CVEs blocked from merge"
  - "approved vendor list: maintained in engineering wiki, updated quarterly"
sub_phase: "3.4"
pipeline_run_id: PR-4953
```

### B

```
domain_processor_output:
  territory: T-ssdf-process
  pipeline_run_id: PR-4953

  finding:
    practice: PO.2
    claim: "Third-party component evaluation process documented and tooled."
    evaluation:
      - "SCA tool (Snyk) integrated in CI: automated evaluation present"
      - "CVE severity blocking (HIGH/CRITICAL): clear acceptance criteria"
      - "approved vendor list with quarterly review: documented governance"
      - "All three PO.2 elements present: tooling, policy, governance cadence"
    verdict_signal: PASS
    note: "PO.2 requires that third-party software be evaluated for security risk before use and that the process be documented. All three elements (tool, policy, governance) are present. PO.2 evidence is distinct from PW.4 (SAST on first-party code) — this correctly addresses third-party component risk."
```

### Reason

SSDF PO.2 concerns organizational preparation for third-party software evaluation. The team has: SCA tooling (Snyk in CI), a blocking policy (HIGH/CRITICAL CVE = merge blocked), and a governance cadence (quarterly vendor list review). All three elements constitute satisfying PO.2. The note distinguishes PO.2 from PW.4: PO.2 is about third-party components; PW.4 is about first-party code analysis. This distinction trains the pipeline to recognize PO vs PW evidence types at the domain processor level, building on the Node 7 selector training in ext1.

### Phrasing B

```
SSDF PO.2. Snyk SCA in CI, HIGH/CRITICAL CVE blocks merge, approved vendor list quarterly review.
Node 04, ssdf-process-evidence territory. Run PR-4953.
Does this evidence satisfy PO.2?
```

Expected: PASS. All three elements present: tooling (Snyk), blocking policy (HIGH/CRITICAL CVEs), governance cadence (quarterly review). Note distinguishes PO.2 (third-party) from PW.4 (first-party SAST).

---

## File summary

| Seq | Stratum | Source | Verdict | New domain introduced |
|-----|---------|--------|---------|----------------------|
| 127 | A | ASVS | CONCERN | MFA availability vs enforcement (V6.3.1 vs V6.3.2) |
| 128 | A | ASVS | REPROCESS | Administrative function access control — auth ≠ authz (V8.3.1) |
| 129 | A | CWE | REPROCESS | CWE-352 CSRF on state-changing operation |
| 130 | A | ASVS | CONCERN | API key lifecycle — no expiry, manual revocation only |
| 131 | A | IETF | PASS | OAuth Device Authorization Grant (RFC 8628); PKCE not applicable |
| 132 | A | ASVS | CONCERN | OAuth token revocation V10.3 — access tokens revocable, refresh tokens not |
| 133 | A | CWE | REPROCESS | CWE-307 brute force — bcrypt cost ≠ rate limiting |
| 134 | A | SSDF | PASS | SSDF PO.2 third-party component evaluation (vs PW.4 first-party) |

**Running total: 268 instances (sequences 001–134)**
Stratum breakdown: A ~30%, B ~27%, C ~26%, D ~17%
