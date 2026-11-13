# Node 3 — Scheme Planner Pairs
# Batch CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Wave 1 — Stage 3 training pairs
# Sequence range: CB4-3.4-041 through CB4-3.4-045
# Base pairs: 5 × 2 phrasings = 10 pair instances
# Node role: receives territory list; produces ordered verification scheme plan
# Production date: 2026-05-05

---

## Coverage map

| Pair | Territory input | Plan type | Source | Stratum |
|------|----------------|-----------|--------|---------|
| 041 | T-oauth-oidc (single) | Linear, 5 steps | OWASP-ASVS | B |
| 042 | T-auth + T-session + T-authz + T-token | Parallel branches, 7 steps | OWASP-ASVS | A |
| 043 | T-process-evidence (SSDF, dates required) | Sequential with pre-condition | NIST-SSDF | B |
| 044 | T-cwe-authz (CWE-862 candidate) | Linear with evaluation path branch | CWE-TOP25 | A |
| 045 | T-cwe-authz (conflation detected) | Disambiguation step added | CWE-TOP25 | D |

---

## Pairs

---

### Pair CB4-3.4-041 — Single-territory linear plan for OAuth/PKCE (PLAN)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Territory list received from Node 1/2: [T-oauth-oidc]. Domain labels: [G-security-verification,
oauth-oidc]. Claim signal: team assertion that code_challenge_method='plain' satisfies
ASVS 10.4.6. Sub-phase: 3.4. Produce verification scheme plan.

B: SCHEME_PLAN (linear, 5 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-oauth-oidc.
    Proceed to single-branch plan.
  Step 2 — Process T-oauth-oidc: Domain processor evaluates OAuth 2.0 claims against
    ASVS Chapter V10 requirements. Extract: authorization-code-flow implementation,
    PKCE parameters (code_challenge, code_challenge_method value), server configuration.
    Flag: code_challenge_method='plain' — evaluate against ASVS 10.4.6 S256 requirement.
  Step 3 — Translate result: Result translator converts domain processor output to
    human-readable summary for Node 7 input.
  Step 4 — Arbiter synthesis: Node 7 selects ASVS-V10-OAuth-Arbiter from oauth-oidc
    domain label. Node 8 runs ASVS-V10-OAuth-Arbiter on translated result.
  Step 5 — Format: Node 9 formats arbiter verdict (PASS / REPROCESS / CONCERN)
    for caller.
Recovery path: if Step 2 returns insufficient evidence for code_challenge_method evaluation,
Node 4 emits CLARIFICATION_REQUIRED with specific field list; Step 3 is skipped until
re-submission.

Reason: Single-territory input produces a linear plan. No parallel branches are needed.
The recovery path is defined at Step 2 for the case where the submitted information is
incomplete — the plan does not assume a specific verdict, only the evaluation path.

---

### Pair CB4-3.4-041 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: B

A: Territories: [T-oauth-oidc]. Domain label: [G-security-verification, oauth-oidc].
PKCE with plain method, ASVS 10.4.6 referenced. One territory. Plan required.

B: SCHEME_PLAN (linear, 5 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-oauth-oidc.
    Proceed to single-branch plan.
  Step 2 — Process T-oauth-oidc: Domain processor evaluates OAuth 2.0 claims against
    ASVS Chapter V10 requirements. Extract: authorization-code-flow implementation,
    PKCE parameters (code_challenge, code_challenge_method value), server configuration.
    Flag: code_challenge_method='plain' — evaluate against ASVS 10.4.6 S256 requirement.
  Step 3 — Translate result: Result translator converts domain processor output to
    human-readable summary for Node 7 input.
  Step 4 — Arbiter synthesis: Node 7 selects ASVS-V10-OAuth-Arbiter from oauth-oidc
    domain label. Node 8 runs ASVS-V10-OAuth-Arbiter on translated result.
  Step 5 — Format: Node 9 formats arbiter verdict (PASS / REPROCESS / CONCERN)
    for caller.
Recovery path: if Step 2 returns insufficient evidence for code_challenge_method evaluation,
Node 4 emits CLARIFICATION_REQUIRED with specific field list; Step 3 is skipped until
re-submission.

---

### Pair CB4-3.4-042 — Multi-territory parallel plan for comprehensive auth input (PLAN)
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Territory list: [T-auth, T-session, T-authz, T-token]. Domain labels:
[G-security-verification, authentication-controls, session-management, authorization-controls,
token-validation]. Sub-phase: 3.4. All four territories have independent vocabulary —
no ordering dependency between them. Produce verification scheme plan.

B: SCHEME_PLAN (parallel branches, 7 steps):
  Step 1 — Classify: security-verification-3.4. Four territories. Parallel plan.
  Steps 2a–2d (parallel) — Process territories independently:
    2a: Domain processor evaluates T-auth against ASVS V6 requirements.
    2b: Domain processor evaluates T-session against ASVS V7 requirements.
    2c: Domain processor evaluates T-authz against ASVS V8 requirements.
    2d: Domain processor evaluates T-token against ASVS V9 requirements.
  Steps 3a–3d (parallel, after respective 2x) — Translate results:
    3a–3d: Result translator converts each domain processor output independently.
  Step 4 — Sync point: all four translated results must be available before proceeding.
  Step 5 — Arbiter synthesis: Node 7 receives all four results, selects
    [ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter, ASVS-V8-AuthZ-Arbiter,
    ASVS-V9-Token-Arbiter]. Node 8 runs all four arbiters.
  Step 6 — Consolidate: if any arbiter returns REPROCESS, overall verdict is REPROCESS.
    If no REPROCESS and any CONCERN, overall includes CONCERN. If all PASS, overall PASS.
  Step 7 — Format: Node 9 formats consolidated verdict with all requirement IDs.
Recovery path: if any parallel branch returns CLARIFICATION_REQUIRED, that branch is
paused; remaining branches continue. Final verdict deferred until all branches complete.

Reason: Parallel territory processing is correct for independent vocabulary clusters.
The sync point at Step 4 prevents Node 7 from running before all territories are processed.
Step 6 consolidation rule prevents a REPROCESS condition in one territory from being
masked by PASS verdicts in other territories.

---

### Pair CB4-3.4-042 — Phrasing B
Source: OWASP ASVS 5.0.0
License: CC BY-SA 4.0
Stratum: A

A: Four territories: T-auth (V6), T-session (V7), T-authz (V8), T-token (V9). All
independent, no ordering dependency. G-security-verification label. Produce parallel
scheme plan.

B: SCHEME_PLAN (parallel branches, 7 steps):
  Step 1 — Classify: security-verification-3.4. Four territories. Parallel plan.
  Steps 2a–2d (parallel) — Process territories independently:
    2a: Domain processor evaluates T-auth against ASVS V6 requirements.
    2b: Domain processor evaluates T-session against ASVS V7 requirements.
    2c: Domain processor evaluates T-authz against ASVS V8 requirements.
    2d: Domain processor evaluates T-token against ASVS V9 requirements.
  Steps 3a–3d (parallel, after respective 2x) — Translate results:
    3a–3d: Result translator converts each domain processor output independently.
  Step 4 — Sync point: all four translated results must be available before proceeding.
  Step 5 — Arbiter synthesis: Node 7 receives all four results, selects
    [ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter, ASVS-V8-AuthZ-Arbiter,
    ASVS-V9-Token-Arbiter]. Node 8 runs all four arbiters.
  Step 6 — Consolidate: if any arbiter returns REPROCESS, overall verdict is REPROCESS.
    If no REPROCESS and any CONCERN, overall includes CONCERN. If all PASS, overall PASS.
  Step 7 — Format: Node 9 formats consolidated verdict with all requirement IDs.
Recovery path: if any parallel branch returns CLARIFICATION_REQUIRED, that branch is
paused; remaining branches continue. Final verdict deferred until all branches complete.

---

### Pair CB4-3.4-043 — SSDF process evidence plan with artifact timeline pre-condition (PLAN)
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: Territory list: [T-process-evidence]. Domain labels: [G-security-verification,
ssdf-process-evidence]. Artifact signals: security requirements (pre-design), threat model
(pre-implementation), design review (date to be checked against implementation date), test
results (post-implementation). SSDF PW.2 requires design review before implementation.
Timeline analysis required. Produce scheme plan.

B: SCHEME_PLAN (sequential with pre-condition, 6 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-process-evidence.
    Sequential plan required — artifact timeline must be evaluated before content.
  Step 2 — Timeline analysis (pre-condition): Domain processor extracts artifact dates.
    Check: design-review-date < implementation-completion-date (PW.2 pre-condition).
    If design-review-date >= implementation-completion-date: flag TIMING_GAP before
    proceeding to Step 3.
  Step 3 — Content evaluation: Domain processor evaluates each artifact against
    its PW practice: PW.1 (requirements pre-design), PW.2 (design review completeness),
    PW.8 (testing coverage). Carries TIMING_GAP flag from Step 2 if present.
  Step 4 — Translate result: result translator produces human-readable SSDF
    compliance summary with TIMING_GAP annotation if flagged.
  Step 5 — Arbiter synthesis: Node 7 selects SSDF-PW-Arbiter. Node 8 evaluates
    PW.1/PW.2/PW.8 with timing context.
  Step 6 — Format: Node 9 formats PASS / CONCERN verdict with PW practice breakdown.
Pre-condition rationale: SSDF PW.2 timing is a structural property of the evidence, not
an artifact content property. Timeline must be checked before content is evaluated to
avoid clearing PW.2 when only the artifact content is valid but the timing is violated.

---

### Pair CB4-3.4-043 — Phrasing B
Source: NIST SP 800-218 SSDF v1.1
License: Public domain (NIST)
Stratum: B

A: Territory: T-process-evidence. Artifacts: requirements (pre-design), threat model
(pre-impl), design review (date to verify), test results (post-impl). PW.2 requires
pre-implementation design review. Timeline check needed. Plan required.

B: SCHEME_PLAN (sequential with pre-condition, 6 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-process-evidence.
    Sequential plan required — artifact timeline must be evaluated before content.
  Step 2 — Timeline analysis (pre-condition): Domain processor extracts artifact dates.
    Check: design-review-date < implementation-completion-date (PW.2 pre-condition).
    If design-review-date >= implementation-completion-date: flag TIMING_GAP before
    proceeding to Step 3.
  Step 3 — Content evaluation: Domain processor evaluates each artifact against
    its PW practice: PW.1 (requirements pre-design), PW.2 (design review completeness),
    PW.8 (testing coverage). Carries TIMING_GAP flag from Step 2 if present.
  Step 4 — Translate result: result translator produces human-readable SSDF
    compliance summary with TIMING_GAP annotation if flagged.
  Step 5 — Arbiter synthesis: Node 7 selects SSDF-PW-Arbiter. Node 8 evaluates
    PW.1/PW.2/PW.8 with timing context.
  Step 6 — Format: Node 9 formats PASS / CONCERN verdict with PW practice breakdown.
Pre-condition rationale: SSDF PW.2 timing is a structural property of the evidence, not
an artifact content property. Timeline must be checked before content is evaluated to
avoid clearing PW.2 when only the artifact content is valid but the timing is violated.

---

### Pair CB4-3.4-044 — CWE authorization weakness plan with evaluation path branch (PLAN)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: Territory list: [T-cwe-authz]. Domain labels: [G-security-verification,
cwe-security-weakness]. CWE-862 candidate flagged by Node 1 (no ownership check described).
Sub-phase: 3.4. Produce scheme plan.

B: SCHEME_PLAN (linear with evaluation path branch, 5 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-cwe-authz.
    CWE-862 candidate signal from Node 1.
  Step 2 — Process T-cwe-authz: Domain processor evaluates endpoint design.
    Branch A (CWE-862 path): if resource ownership check is absent → confirm
      IDOR pattern, produce CWE_862_IDOR_CONFIRMED finding.
    Branch B (CWE-269 path): if resource ownership check is present but scope is
      too broad → produce CWE_269_PRIVILEGE_SCOPE_EXCEEDED finding.
    Branch C (no weakness): if ownership check is present and scope is correct →
      produce AUTHORIZATION_CONTROLS_ADEQUATE finding.
  Step 3 — Translate result: result translator summarizes finding for Node 7.
  Step 4 — Arbiter synthesis: Node 7 selects CWE-Authz-Arbiter with evaluation
    path signal (A, B, or C). Node 8 produces REPROCESS (A), CONCERN (B), or PASS (C).
  Step 5 — Format: Node 9 formats verdict with CWE ID and remediation.
Note: Branch A (REPROCESS) and Branch B (CONCERN) are determined in Step 2 —
the scheme plan defines all possible paths before execution.

---

### Pair CB4-3.4-044 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: A

A: Territories: [T-cwe-authz]. CWE-862 candidate flagged. Authorization weakness
analysis required. Produce plan.

B: SCHEME_PLAN (linear with evaluation path branch, 5 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-cwe-authz.
    CWE-862 candidate signal from Node 1.
  Step 2 — Process T-cwe-authz: Domain processor evaluates endpoint design.
    Branch A (CWE-862 path): if resource ownership check is absent → confirm
      IDOR pattern, produce CWE_862_IDOR_CONFIRMED finding.
    Branch B (CWE-269 path): if resource ownership check is present but scope is
      too broad → produce CWE_269_PRIVILEGE_SCOPE_EXCEEDED finding.
    Branch C (no weakness): if ownership check is present and scope is correct →
      produce AUTHORIZATION_CONTROLS_ADEQUATE finding.
  Step 3 — Translate result: result translator summarizes finding for Node 7.
  Step 4 — Arbiter synthesis: Node 7 selects CWE-Authz-Arbiter with evaluation
    path signal (A, B, or C). Node 8 produces REPROCESS (A), CONCERN (B), or PASS (C).
  Step 5 — Format: Node 9 formats verdict with CWE ID and remediation.
Note: Branch A (REPROCESS) and Branch B (CONCERN) are determined in Step 2 —
the scheme plan defines all possible paths before execution.

---

### Pair CB4-3.4-045 — CWE plan with authentication/authorization conflation disambiguation (PLAN)
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Territory list: [T-cwe-authz]. Node 1 signal: team submitted authentication evidence
("JWT required and verified") as the authorization control for a resource-scoped endpoint.
Conflation detected: authentication ≠ authorization. Domain labels: [G-security-verification,
cwe-security-weakness]. Produce scheme plan.

B: SCHEME_PLAN (linear with disambiguation step, 6 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-cwe-authz.
    Conflation signal: authentication evidence offered as authorization evidence.
  Step 2 — Disambiguate: Domain processor records the conflation explicitly.
    Authentication claim: JWT required and signature/expiry verified — this establishes
    the caller's identity.
    Authorization claim: none provided — no ownership check, RBAC, or scope evaluation
    described. Conflation confirmed: AUTHENTICATION_AS_AUTHORIZATION.
  Step 3 — Process T-cwe-authz: evaluate whether a resource ownership check exists.
    With disambiguation from Step 2: no ownership check was submitted. CWE-862 path active.
  Step 4 — Translate result: result translator produces finding with conflation annotation.
  Step 5 — Arbiter synthesis: Node 7 selects CWE-Authz-Arbiter with Branch A signal
    (CWE-862, absent check). Node 8 produces REPROCESS.
  Step 6 — Format: Node 9 formats REPROCESS including explanation of conflation:
    "JWT authentication verifies identity; it does not authorize access to a specific
    resource. A separate resource ownership check is required."

Reason: The disambiguation step is required because the team's conflation could mask
the absence of an authorization check — the domain processor might incorrectly evaluate
the JWT claim as satisfying the authorization requirement. Step 2 makes the conflation
explicit before Step 3 evaluates authorization controls.

---

### Pair CB4-3.4-045 — Phrasing B
Source: CWE Top 25 2023
License: Public domain (MITRE)
Stratum: D

A: Territory: T-cwe-authz. Team submitted JWT auth claim as authorization control.
Conflation: AUTHENTICATION_AS_AUTHORIZATION detected by Node 1. No ownership check
described. Plan with disambiguation required.

B: SCHEME_PLAN (linear with disambiguation step, 6 steps):
  Step 1 — Classify: security-verification-3.4. Single territory T-cwe-authz.
    Conflation signal: authentication evidence offered as authorization evidence.
  Step 2 — Disambiguate: Domain processor records the conflation explicitly.
    Authentication claim: JWT required and signature/expiry verified — this establishes
    the caller's identity.
    Authorization claim: none provided — no ownership check, RBAC, or scope evaluation
    described. Conflation confirmed: AUTHENTICATION_AS_AUTHORIZATION.
  Step 3 — Process T-cwe-authz: evaluate whether a resource ownership check exists.
    With disambiguation from Step 2: no ownership check was submitted. CWE-862 path active.
  Step 4 — Translate result: result translator produces finding with conflation annotation.
  Step 5 — Arbiter synthesis: Node 7 selects CWE-Authz-Arbiter with Branch A signal
    (CWE-862, absent check). Node 8 produces REPROCESS.
  Step 6 — Format: Node 9 formats REPROCESS including explanation of conflation:
    "JWT authentication verifies identity; it does not authorize access to a specific
    resource. A separate resource ownership check is required."

---

## Node 3 summary

Pairs produced: CB4-3.4-041 through CB4-3.4-045 (5 base × 2 phrasings = 10 instances)

Planning rules trained:
1. Single territory → linear plan (no parallel branches needed)
2. Multi-territory independent → parallel branches with sync point
3. SSDF evidence → sequential with timeline pre-condition (PW.2 timing before content)
4. CWE candidate → linear with evaluation path branch (A/B/C determined in Step 2)
5. Conflation detected → disambiguation step before authorization evaluation

Running sequence total after this file: CB4-3.4-001 through CB4-3.4-045 (90 instances)
