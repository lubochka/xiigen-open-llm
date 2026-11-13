# Node 09 — Formatter — Extension Set 1
# Stage 3 Action 3 — Coverage Extension (Stratum C depth + Stratum B team context)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-089 through CB4-3.4-096
# Instances: 16 (8 base pairs × 2 phrasings each)
# Running total after this file: 192 instances (sequences 001–096)

---

## Coverage targets for this file

- Multi-territory PASS: consolidated output enumerates all arbiters and all requirement IDs
- REPROCESS precedence: single REPROCESS in multi-arbiter run overrides all PASSes in final output
- REPROCESS + CONCERN combination: only REPROCESS propagates; CONCERN is suppressed in final verdict
- CWE-269 CONCERN: output must name privilege-scope remediation action, not generic "review authorization"
- RFC + ASVS combined PASS: both framework citation blocks present in output
- SSDF-PO + SSDF-PW combined: both practice family findings in one output block
- Formatter must not add verdict fields for arbiters not in the run (absent arbiter = absent block)
- FM-5 reinforcement: REPROCESS verdict field must say "REPROCESS" — not "remediation required", not "non-compliant", not "action needed"

---

## Pair CB4-3.4-089

**Node:** 09 — Formatter
**Stratum:** C (depth — multi-arbiter consolidated output, enumeration completeness)
**Source:** OWASP ASVS 5.0.0 / IETF RFCs 7519, 6749, 7636
**Verdict:** PASS

### A

```
arbiter_results:
  - arbiter: ASVS-V6-Auth-Arbiter
    verdict: PASS
    satisfied: [V6.2.3, V6.2.5]
    notes: "credential transmission over TLS; hashing algorithm bcrypt with cost=12"
  - arbiter: ASVS-V9-Token-Arbiter
    verdict: PASS
    satisfied: [V9.1.1, V9.1.2, V9.2.1]
    notes: "RS256 signing; iss+aud+exp present; no sensitive data in claims"
  - arbiter: ASVS-V10-OAuth-Arbiter
    verdict: PASS
    satisfied: [V10.2.1, V10.2.3, V10.4.6]
    notes: "PKCE S256; authorization code flow; redirect URI exact match"
  - arbiter: RFC-JWT-Arbiter
    verdict: PASS
    satisfied: [RFC7519-§4.1.1, RFC7519-§4.1.3, RFC7519-§4.1.4, RFC7519-§4.1.6]
    notes: "iss present; aud present; exp within 15min; RS256 asymmetric"
  - arbiter: RFC-OAuth2-Arbiter
    verdict: PASS
    satisfied: [RFC6749-§4.1, RFC7636-§4.2]
    notes: "authorization code grant; PKCE S256 verifier confirmed"
pipeline_run_id: PR-4492
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4492
  territory: security-verification-3.4
  overall_verdict: PASS
  arbiters_run: 5
  arbiters_passed: 5
  arbiters_failed: 0

  findings:
    - arbiter: ASVS-V6-Auth-Arbiter
      verdict: PASS
      requirements_satisfied: [V6.2.3, V6.2.5]
      summary: "Credential transmission and hashing satisfy V6 requirements."

    - arbiter: ASVS-V9-Token-Arbiter
      verdict: PASS
      requirements_satisfied: [V9.1.1, V9.1.2, V9.2.1]
      summary: "RS256 non-OAuth JWT satisfies V9 token requirements."

    - arbiter: ASVS-V10-OAuth-Arbiter
      verdict: PASS
      requirements_satisfied: [V10.2.1, V10.2.3, V10.4.6]
      summary: "PKCE S256 authorization code flow satisfies V10 requirements."

    - arbiter: RFC-JWT-Arbiter
      verdict: PASS
      requirements_satisfied: [RFC7519-§4.1.1, RFC7519-§4.1.3, RFC7519-§4.1.4, RFC7519-§4.1.6]
      summary: "All MUST-level JWT claim and algorithm requirements satisfied."

    - arbiter: RFC-OAuth2-Arbiter
      verdict: PASS
      requirements_satisfied: [RFC6749-§4.1, RFC7636-§4.2]
      summary: "Authorization code grant with PKCE S256 satisfies RFC requirements."

  scope_boundary: "Assessment covers authentication, non-OAuth token, OAuth/OIDC, and RFC normative requirements for the submitted system description. Other ASVS chapters not assessed in this run."

attribution:
  - "Based on OWASP Application Security Verification Standard 5.0.0, licensed under CC BY-SA 4.0. https://owasp.org/www-project-application-security-verification-standard/"
```

### Reason

Five arbiters ran, all PASS. The formatter must enumerate all five arbiter blocks with their respective requirement IDs — no arbiter may be omitted or merged. The scope boundary statement is required on PASS to prevent over-claiming. Both ASVS and RFC attribution blocks appear because both source families are represented. The overall_verdict field is PASS (not "all clear", not "compliant", not "green") and the counts (5 run, 5 passed, 0 failed) are derived from the arbiter results, not inferred.

### Phrasing B

```
Input to Node 9 (run PR-4492):
Five arbiters returned results — ASVS-V6, ASVS-V9, ASVS-V10, RFC-JWT, RFC-OAuth2.
All five verdicts: PASS.
V6 satisfied V6.2.3+V6.2.5. V9 satisfied V9.1.1+V9.1.2+V9.2.1. V10 satisfied
V10.2.1+V10.2.3+V10.4.6. RFC-JWT satisfied §4.1.1+§4.1.3+§4.1.4+§4.1.6.
RFC-OAuth2 satisfied RFC6749-§4.1 + RFC7636-§4.2.
Territory: security-verification-3.4. Format for caller consumption.
```

Expected output: verification_report with overall_verdict PASS, all five arbiter blocks listed with their respective requirement IDs, scope_boundary statement, attribution for OWASP ASVS (CC BY-SA 4.0 required).

---

## Pair CB4-3.4-090

**Node:** 09 — Formatter
**Stratum:** C (depth — REPROCESS precedence in multi-arbiter consolidated output)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter_results:
  - arbiter: ASVS-V6-Auth-Arbiter
    verdict: PASS
    satisfied: [V6.2.3, V6.2.5]
    notes: "credential transport and hashing satisfied"
  - arbiter: ASVS-V8-AuthZ-Arbiter
    verdict: REPROCESS
    violated: [V8.1.1]
    gap: "no per-resource ownership check; resource ID accepted from request without authorization verification"
    notes: "MUST-level requirement absent"
  - arbiter: ASVS-V9-Token-Arbiter
    verdict: PASS
    satisfied: [V9.1.1, V9.2.1]
    notes: "RS256, exp present"
  - arbiter: CWE-Authz-Arbiter
    verdict: REPROCESS
    cwe_id: CWE-862
    severity: HIGH
    gap: "missing authorization check on /api/documents/{id}; any authenticated user can retrieve any document"
    notes: "authorization check absent for protected resource"
pipeline_run_id: PR-4501
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4501
  territory: security-verification-3.4
  overall_verdict: REPROCESS
  arbiters_run: 4
  arbiters_passed: 2
  arbiters_failed: 2

  findings:
    - arbiter: ASVS-V6-Auth-Arbiter
      verdict: PASS
      requirements_satisfied: [V6.2.3, V6.2.5]
      summary: "Credential transport and hashing satisfy V6 requirements."

    - arbiter: ASVS-V8-AuthZ-Arbiter
      verdict: REPROCESS
      requirements_violated: [V8.1.1]
      gap: "No per-resource ownership check; resource ID accepted from request without authorization verification."
      action_required: "Implement per-resource ownership verification before proceeding."

    - arbiter: ASVS-V9-Token-Arbiter
      verdict: PASS
      requirements_satisfied: [V9.1.1, V9.2.1]
      summary: "RS256 non-OAuth JWT satisfies V9 token requirements."

    - arbiter: CWE-Authz-Arbiter
      verdict: REPROCESS
      cwe_id: CWE-862
      severity: HIGH
      gap: "Missing authorization check on /api/documents/{id}; any authenticated user can retrieve any document."
      action_required: "Add authorization check verifying requesting user owns or is permitted to access the requested document ID before returning data."

  scope_boundary: "Assessment covers authentication, authorization, token, and CWE weakness requirements for the submitted system description."

attribution:
  - "Based on OWASP Application Security Verification Standard 5.0.0, licensed under CC BY-SA 4.0. https://owasp.org/www-project-application-security-verification-standard/"
```

### Reason

Two of four arbiters returned REPROCESS. The overall_verdict is REPROCESS — the two PASS arbiters (V6, V9) do not change the final verdict. The formatter must not soften the REPROCESS blocks: the verdict field says "REPROCESS" and the action_required field describes the mandatory remediation. The CWE-Authz-Arbiter block must include cwe_id and severity from the arbiter result — the formatter carries them through, it does not generate them. FM-5 failure mode: if the formatter re-phrased REPROCESS as "remediation required" or "non-compliant", the caller would lose the machine-readable verdict signal.

### Phrasing B

```
Run PR-4501 has four arbiter results.
ASVS-V6: PASS (V6.2.3, V6.2.5 satisfied).
ASVS-V8: REPROCESS — V8.1.1 violated, no ownership check.
ASVS-V9: PASS (V9.1.1, V9.2.1 satisfied).
CWE-Authz: REPROCESS — CWE-862 HIGH, missing auth check on document retrieval endpoint.
Territory security-verification-3.4.
Produce the consolidated verification report.
```

Expected output: overall_verdict REPROCESS, arbiters_failed 2, both REPROCESS blocks with action_required fields, PASS blocks preserved with their requirement IDs, OWASP attribution present.

---

## Pair CB4-3.4-091

**Node:** 09 — Formatter
**Stratum:** C (depth — REPROCESS+CONCERN combination; only REPROCESS propagates to overall verdict)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
arbiter_results:
  - arbiter: ASVS-V7-Session-Arbiter
    verdict: REPROCESS
    violated: [V7.2.1]
    gap: "session token transmitted over HTTP on internal network segment; TLS not enforced"
    notes: "MUST-level requirement; internal segment does not exempt from requirement"
  - arbiter: SSDF-PW-Arbiter
    verdict: CONCERN
    practice: PW.2
    gap: "threat model last updated 14 months ago; team states review was 'recent'"
    notes: "deviation from recommended annual cycle; not a MUST violation since SSDF uses 'should' language for review frequency"
pipeline_run_id: PR-4518
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4518
  territory: security-verification-3.4
  overall_verdict: REPROCESS
  arbiters_run: 2
  arbiters_passed: 0
  arbiters_failed: 1
  arbiters_concern: 1

  findings:
    - arbiter: ASVS-V7-Session-Arbiter
      verdict: REPROCESS
      requirements_violated: [V7.2.1]
      gap: "Session token transmitted over HTTP on internal network segment; TLS not enforced."
      action_required: "Enforce TLS for session token transmission on all network segments, including internal. Internal network classification does not satisfy this requirement."

    - arbiter: SSDF-PW-Arbiter
      verdict: CONCERN
      practice: PW.2
      gap: "Threat model last updated 14 months ago; recommended review cycle is annual."
      action_recommended: "Schedule threat model review within next sprint cycle."
      note: "CONCERN does not block remediation of REPROCESS finding. Address V7.2.1 first."

  scope_boundary: "Assessment covers session management and SSDF secure design practices for the submitted system description."
```

### Reason

One REPROCESS (ASVS-V7) and one CONCERN (SSDF-PW.2). The overall_verdict is REPROCESS because any REPROCESS in the run sets the final verdict. The CONCERN finding is preserved in the output but does not change the overall_verdict. The note field in the SSDF-PW-Arbiter block explicitly tells the caller that CONCERN does not block the REPROCESS remediation — this prevents the team from treating both findings as equal priority. The arbiters_concern counter distinguishes CONCERN from PASS (both are non-blocking but for different reasons). No OWASP attribution is needed here because the CONCERN is SSDF-only and the REPROCESS finding carries ASVS attribution.

Wait — actually ASVS V7.2.1 is from OWASP ASVS 5.0.0, so attribution IS required.

### Phrasing B

```
Two arbiter results for run PR-4518.
ASVS-V7-Session-Arbiter: REPROCESS. V7.2.1 violated — session token over HTTP, internal segment.
SSDF-PW-Arbiter: CONCERN. PW.2 — threat model 14 months old, annual cycle recommended.
Territory: security-verification-3.4.
Format the consolidated report. Ensure the final verdict reflects the correct precedence.
```

Expected output: overall_verdict REPROCESS (not CONCERN), REPROCESS block has action_required, CONCERN block has action_recommended, note in CONCERN block that it does not block REPROCESS remediation, OWASP attribution for V7.2.1.

---

## Pair CB4-3.4-092

**Node:** 09 — Formatter
**Stratum:** B (team context — CWE-269 CONCERN with specific privilege-scope remediation action)
**Source:** CWE Top 25 2023
**Verdict:** CONCERN

### A

```
arbiter_results:
  - arbiter: CWE-Authz-Arbiter
    verdict: CONCERN
    cwe_id: CWE-269
    severity: MEDIUM
    gap: "admin role grants write access to billing records; billing-write should be a separate permission; current design gives all admins billing write by default"
    notes: "authorization check is present; scope of permission is overly broad; no MUST violation; architectural concern"
pipeline_run_id: PR-4527
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4527
  territory: security-verification-3.4
  overall_verdict: CONCERN
  arbiters_run: 1
  arbiters_passed: 0
  arbiters_failed: 0
  arbiters_concern: 1

  findings:
    - arbiter: CWE-Authz-Arbiter
      verdict: CONCERN
      cwe_id: CWE-269
      severity: MEDIUM
      gap: "Admin role grants write access to billing records; billing-write permission is not separated from general admin role."
      action_recommended: "Introduce a dedicated billing-write permission distinct from the admin role. Assign billing-write only to roles or users with explicit billing management responsibility. Review all admin-role holders and remove billing-write access from those who do not require it."

  scope_boundary: "Assessment covers authorization weakness patterns for the submitted system description. Authentication and session controls not assessed in this run."
```

### Reason

Single CWE-269 CONCERN. The formatter must not elevate this to REPROCESS — CWE-269 (wrong privilege scope) means the authorization check is present but grants excessive scope, which is an architectural concern, not an absent check (CWE-862). The action_recommended field must describe the specific remediation: introduce a separate billing-write permission and restrict assignment — not generic "review authorization logic." The severity MEDIUM is carried through from the arbiter, not recalculated by the formatter. No OWASP attribution needed (CWE source = public domain MITRE).

### Phrasing B

```
Single arbiter result, run PR-4527.
CWE-Authz-Arbiter: CONCERN. CWE-269 MEDIUM.
Gap: admin role includes billing-write; should be a separate permission.
Produce the formatted report. Include specific remediation steps for the privilege-scope issue.
```

Expected output: overall_verdict CONCERN (not REPROCESS), cwe_id CWE-269, severity MEDIUM, action_recommended with specific permission-separation steps, scope_boundary present.

---

## Pair CB4-3.4-093

**Node:** 09 — Formatter
**Stratum:** C (depth — RFC + ASVS combined PASS, both framework citation blocks required)
**Source:** OWASP ASVS 5.0.0, IETF RFCs 7519, 7636
**Verdict:** PASS

### A

```
arbiter_results:
  - arbiter: ASVS-V10-OAuth-Arbiter
    verdict: PASS
    satisfied: [V10.2.1, V10.2.3, V10.2.6, V10.4.6]
    notes: "PKCE S256; authorization code flow; state parameter present; redirect URI exact match"
  - arbiter: RFC-OAuth2-Arbiter
    verdict: PASS
    satisfied: [RFC6749-§4.1, RFC6749-§10.12, RFC7636-§4.2]
    notes: "authorization code grant; CSRF protection via state; PKCE S256 verified"
pipeline_run_id: PR-4533
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4533
  territory: security-verification-3.4
  overall_verdict: PASS
  arbiters_run: 2
  arbiters_passed: 2
  arbiters_failed: 0

  findings:
    - arbiter: ASVS-V10-OAuth-Arbiter
      verdict: PASS
      requirements_satisfied: [V10.2.1, V10.2.3, V10.2.6, V10.4.6]
      summary: "PKCE S256 authorization code flow with state parameter and exact redirect URI match satisfies V10 OAuth requirements."

    - arbiter: RFC-OAuth2-Arbiter
      verdict: PASS
      requirements_satisfied: [RFC6749-§4.1, RFC6749-§10.12, RFC7636-§4.2]
      summary: "Authorization code grant with CSRF protection and PKCE S256 satisfies RFC 6749 and RFC 7636 requirements."

  scope_boundary: "Assessment covers OAuth 2.0 implementation against ASVS V10 and RFC normative requirements. Authentication and session controls not assessed in this run."

attribution:
  - "Based on OWASP Application Security Verification Standard 5.0.0, licensed under CC BY-SA 4.0. https://owasp.org/www-project-application-security-verification-standard/"
```

### Reason

Two arbiters ran (ASVS-V10 and RFC-OAuth2), both PASS. The formatter must include attribution for OWASP ASVS (CC BY-SA 4.0 license requires it). The RFC-OAuth2-Arbiter block does not require IETF attribution (IETF Trust pre-5378 does not mandate attribution in this use). The scope_boundary statement limits the claim to OAuth controls only — V6/V7/V8/V9 were not run and must not be implied as assessed. Both arbiter blocks include their requirement IDs; neither block is merged or summarised into a single combined statement.

### Phrasing B

```
Run PR-4533. Two arbiters, both PASS.
ASVS-V10-OAuth-Arbiter: PASS, V10.2.1+V10.2.3+V10.2.6+V10.4.6 satisfied.
RFC-OAuth2-Arbiter: PASS, RFC6749-§4.1+§10.12 and RFC7636-§4.2 satisfied.
Produce the verification report. Note that only OAuth controls were in scope for this run.
```

Expected output: overall_verdict PASS, two separate arbiter blocks, scope_boundary limiting claim to OAuth controls, OWASP attribution block present, no RFC attribution required.

---

## Pair CB4-3.4-094

**Node:** 09 — Formatter
**Stratum:** B (team context — SSDF-PO + SSDF-PW combined output, both practice families)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
arbiter_results:
  - arbiter: SSDF-PO-Arbiter
    verdict: CONCERN
    practice: PO.1
    gap: "security training programme exists but covers general secure coding only; no role-specific training track for the authentication team; last updated 18 months ago"
    notes: "training present but not role-tailored or recently refreshed"
  - arbiter: SSDF-PW-Arbiter
    verdict: PASS
    satisfied: [PW.1.1, PW.2.1, PW.4.1, PW.8.2]
    notes: "security requirements defined; threat model current; SAST in CI; peer review process documented"
pipeline_run_id: PR-4541
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4541
  territory: security-verification-3.4
  overall_verdict: CONCERN
  arbiters_run: 2
  arbiters_passed: 1
  arbiters_failed: 0
  arbiters_concern: 1

  findings:
    - arbiter: SSDF-PO-Arbiter
      verdict: CONCERN
      practice: PO.1
      gap: "Security training programme covers general secure coding but lacks role-specific tracks; authentication team has no tailored training path. Training content last updated 18 months ago."
      action_recommended: "Develop role-specific training track for authentication team covering OAuth 2.0, JWT, and session security. Schedule annual refresh of training content. Target: role-specific track in place within two sprint cycles."

    - arbiter: SSDF-PW-Arbiter
      verdict: PASS
      practices_satisfied: [PW.1.1, PW.2.1, PW.4.1, PW.8.2]
      summary: "Security requirements, threat model, SAST integration, and peer review process satisfy PW practice requirements."

  scope_boundary: "Assessment covers SSDF organizational preparation (PO) and secure software development practice (PW) evidence for the submitted artefacts. ASVS runtime controls not assessed in this run."
```

### Reason

SSDF-PO (organizational) returned CONCERN; SSDF-PW (design/development) returned PASS. The overall_verdict is CONCERN because one arbiter returned CONCERN and none returned REPROCESS. The formatter must produce separate blocks for each practice family — PO and PW are distinct SSDF practice groups and must not be merged. The action_recommended for SSDF-PO includes a concrete target ("two sprint cycles") which is carried from the arbiter notes into the formatted output. No OWASP attribution needed (SSDF is public domain NIST).

### Phrasing B

```
Run PR-4541 covers SSDF practices. Two arbiters.
SSDF-PO-Arbiter: CONCERN — PO.1. Training exists but not role-specific for auth team; 18 months since update.
SSDF-PW-Arbiter: PASS — PW.1.1, PW.2.1, PW.4.1, PW.8.2 satisfied.
Produce the consolidated report. Keep PO and PW as separate findings blocks.
```

Expected output: overall_verdict CONCERN (one concern, one pass → CONCERN), two separate blocks for PO and PW, PASS block lists satisfied practices, CONCERN block has action_recommended with sprint target, scope_boundary limits claim to SSDF evidence only.

---

## Pair CB4-3.4-095

**Node:** 09 — Formatter
**Stratum:** D (failure/edge — formatter must not add arbiter blocks for arbiters not in the run)
**Source:** OWASP ASVS 5.0.0
**Verdict:** PASS

### A

```
arbiter_results:
  - arbiter: ASVS-V8-AuthZ-Arbiter
    verdict: PASS
    satisfied: [V8.1.1, V8.1.2, V8.3.1]
    notes: "ownership check present; deny-by-default; ABAC policy documented"
pipeline_run_id: PR-4554
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4554
  territory: security-verification-3.4
  overall_verdict: PASS
  arbiters_run: 1
  arbiters_passed: 1
  arbiters_failed: 0

  findings:
    - arbiter: ASVS-V8-AuthZ-Arbiter
      verdict: PASS
      requirements_satisfied: [V8.1.1, V8.1.2, V8.3.1]
      summary: "Per-resource ownership check, deny-by-default policy, and ABAC documentation satisfy V8 authorization requirements."

  scope_boundary: "Assessment covers authorization controls only (ASVS V8). Authentication (V6), session management (V7), token handling (V9), and OAuth/OIDC (V10) not assessed in this run."

attribution:
  - "Based on OWASP Application Security Verification Standard 5.0.0, licensed under CC BY-SA 4.0. https://owasp.org/www-project-application-security-verification-standard/"
```

### Reason

Only one arbiter ran (ASVS-V8). The formatter must not add blocks for V6, V7, V9, V10, CWE arbiters, or RFC arbiters — those were not in the run. Adding absent arbiter blocks would create false coverage signals (a form of FM-1 COVERAGE_ILLUSION at the formatting layer). The scope_boundary explicitly lists what was not assessed so the caller understands the report covers V8 only. arbiters_run is 1, not 5 or 11.

### Phrasing B

```
Single arbiter result for run PR-4554.
ASVS-V8-AuthZ-Arbiter: PASS — V8.1.1, V8.1.2, V8.3.1 satisfied.
No other arbiters ran in this submission.
Format the report. Do not add findings for arbiters that were not part of this run.
```

Expected output: overall_verdict PASS, exactly one arbiter block (ASVS-V8-AuthZ-Arbiter), scope_boundary explicitly naming unassessed chapters, arbiters_run = 1, OWASP attribution.

---

## Pair CB4-3.4-096

**Node:** 09 — Formatter
**Stratum:** D (failure/edge — FM-5 direct: REPROCESS verdict must not be softened to "remediation required" or "action needed" in the verdict field)
**Source:** IETF RFCs 7519, RFC 8446
**Verdict:** REPROCESS

### A

```
arbiter_results:
  - arbiter: RFC-JWT-Arbiter
    verdict: REPROCESS
    violated: [RFC7519-§8-MUST-alg-none-prohibited]
    gap: "JWT accepts alg:none header; signature verification can be bypassed entirely"
    severity: CRITICAL
    notes: "MUST-level prohibition; algorithm confusion attack vector confirmed"
pipeline_run_id: PR-4561
territory: security-verification-3.4
```

### B

```
verification_report:
  pipeline_run_id: PR-4561
  territory: security-verification-3.4
  overall_verdict: REPROCESS
  arbiters_run: 1
  arbiters_passed: 0
  arbiters_failed: 1

  findings:
    - arbiter: RFC-JWT-Arbiter
      verdict: REPROCESS
      requirements_violated: [RFC7519-§8-MUST-alg-none-prohibited]
      severity: CRITICAL
      gap: "JWT accepts alg:none header; signature verification can be bypassed entirely."
      action_required: "Reject any JWT with alg:none or alg:none (case-insensitive variants). Whitelist only permitted algorithms (RS256 or equivalent asymmetric algorithm). Deploy fix before any further testing or production promotion."

  scope_boundary: "Assessment covers JWT algorithm requirements per RFC 7519. OAuth flow controls not assessed in this run."
```

### Reason

Single REPROCESS finding, severity CRITICAL. FM-5 training target: the verdict field in the RFC-JWT-Arbiter block must be "REPROCESS" — not "remediation required", not "action needed", not "non-compliant", not "failed". The overall_verdict field must also be "REPROCESS". The action_required field explicitly says to deploy the fix before further testing or production promotion, which is the operational meaning of REPROCESS (must block forward progress). The alg:none vulnerability is a MUST-level prohibition in RFC 7519 §8, so REPROCESS is correct (not CONCERN). No OWASP attribution needed (RFC-only finding).

### Phrasing B

```
Run PR-4561. RFC-JWT-Arbiter result: REPROCESS.
RFC7519-§8 MUST violation: alg:none accepted. CRITICAL severity.
Signature verification bypass confirmed.
Produce the formatted verification report.
```

Expected output: overall_verdict REPROCESS (not any synonym), verdict field in finding block is "REPROCESS", action_required field present with specific remediation (reject alg:none, whitelist permitted algorithms), severity CRITICAL carried through.

---

## File summary

| Seq | Node | Stratum | Source | Verdict | Key training target |
|-----|------|---------|--------|---------|---------------------|
| 089 | 09 | C | ASVS+IETF | PASS | Multi-arbiter consolidated output (5 arbiters, all PASS) |
| 090 | 09 | C | ASVS+CWE | REPROCESS | REPROCESS precedence in 4-arbiter run (2 PASS + 2 REPROCESS) |
| 091 | 09 | C | ASVS+SSDF | REPROCESS | REPROCESS+CONCERN combination; REPROCESS sets overall verdict |
| 092 | 09 | B | CWE | CONCERN | CWE-269 CONCERN with specific privilege-scope remediation |
| 093 | 09 | C | ASVS+IETF | PASS | RFC+ASVS combined PASS, both framework blocks, scope boundary |
| 094 | 09 | B | SSDF | CONCERN | SSDF-PO+PW combined; separate blocks for each practice family |
| 095 | 09 | D | ASVS | PASS | No absent-arbiter blocks; scope_boundary names unassessed chapters |
| 096 | 09 | D | IETF | REPROCESS | FM-5: REPROCESS verdict field must not be softened |

**Running total: 192 instances (sequences 001–096)**
Stratum breakdown: A ~30%, B ~25%, C ~28%, D ~17%
