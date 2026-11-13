# Node Cross-Source — Multi-Source Submission Set
# Stage 3 Action 3 — Coverage Extension (Stratum C depth + Stratum B team context)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-097 through CB4-3.4-106
# Instances: 20 (10 base pairs × 2 phrasings each)
# Running total after this file: 212 instances (sequences 001–106)

---

## Coverage targets for this file

These pairs test the pipeline's correct behaviour when a single submission contains
evidence, vocabulary, or claims from more than one source family simultaneously.
The primary failure risks are:
  - Node 1 merging distinct territories into one (under-splitting)
  - Node 2 applying the wrong territory label to a mixed-vocabulary item
  - Node 3 producing a linear plan when a parallel plan is needed
  - Node 7 selecting only one source family's arbiters when two are needed
  - Node 8 letting one family's PASS mask the other family's REPROCESS

All pairs in this file: Stratum C unless marked B.

---

## Pair CB4-3.4-097

**Node:** 01 → 07 → 08 (focused chain, key nodes)
**Stratum:** C (multi-source split + arbiter selection: ASVS + CWE combined)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023
**Verdict:** REPROCESS

### A (Node 01 input)

```
submission:
  type: security_verification_request
  system: "ResourceAPI v3.1"
  claims:
    - "JWT tokens are signed with RS256 per V9.1.1"
    - "token expiry set to 15 minutes per V9.2.1"
    - "GET /api/orders/{order_id} retrieves order details"
    - "endpoint requires valid JWT authentication"
  concern:
    - "no check verifying the requesting user owns the order_id in the request path"
    - "any authenticated user can retrieve any order by guessing the order ID"
  sub_phase: "3.4"
```

### B (Node 01 output)

```
territories_identified:
  - id: T-token-validation
    label: "token-validation"
    source_signal: "V9.1.1, V9.2.1, RS256, JWT"
    domain: ASVS-V9
  - id: T-cwe-authz
    label: "cwe-authz"
    source_signal: "no ownership check on order_id, any authenticated user can retrieve any order"
    domain: CWE-Authorization
routing:
  - territory: T-token-validation → Node 2 (domain translation)
  - territory: T-cwe-authz → Node 2 (domain translation)
```

### Reason (Node 01)

The submission contains two distinct topics: a JWT token implementation claim (ASVS V9 vocabulary) and a missing authorization check (CWE-authz vocabulary — IDOR pattern). Node 01 must split these into two territories. Merging them into a single "authentication" territory would prevent the CWE-862 authorization gap from reaching a CWE-Authz-Arbiter. The JWT token claims are V9 (non-OAuth RS256), not V10 — there is no OAuth vocabulary in the submission.

### B (Node 07 input — after Node 2/3/4/5/6)

```
processed_territories:
  - territory: T-token-validation
    domain_label: "token-validation"
    findings: ["RS256 signing confirmed", "exp=15min confirmed"]
    verdict_signal: PASS
  - territory: T-cwe-authz
    domain_label: "cwe-authz"
    findings: ["ownership check absent on /api/orders/{order_id}", "IDOR pattern confirmed"]
    verdict_signal: REPROCESS
vocabulary_signals: ["V9.1.1", "V9.2.1", "RS256", "CWE-862", "IDOR", "ownership check absent"]
```

### B (Node 07 output)

```
arbiter_selection:
  - ASVS-V9-Token-Arbiter
    reason: "V9.1.1, V9.2.1, RS256 — non-OAuth JWT token controls"
  - CWE-Authz-Arbiter
    reason: "ownership check absent, IDOR pattern — CWE-862 authorization gap"
selection_count: 2
```

### Reason (Node 07)

Two arbiters required — one per territory. The ASVS-V9-Token-Arbiter handles the token implementation. The CWE-Authz-Arbiter handles the IDOR pattern. FM-4 (ARBITER_SELECTION_GAP) failure: selecting only ASVS-V9 would miss the authorization gap entirely, and the token PASS would mask the IDOR REPROCESS. Selecting CWE-Authn-Arbiter instead of CWE-Authz-Arbiter would be incorrect (authentication weakness ≠ authorization gap).

### Phrasing B

```
ResourceAPI v3.1 submission for sub-phase 3.4.
Team claims RS256 JWT with V9.1.1 + V9.2.1 compliance.
Team also reports: GET /api/orders/{order_id} has no check that the requesting user
owns the order — any authenticated user can retrieve any order ID.
What territories does Node 01 identify, and which arbiters does Node 07 select?
```

Expected: Node 01 splits into T-token-validation (ASVS-V9) and T-cwe-authz (CWE-authz). Node 07 selects ASVS-V9-Token-Arbiter + CWE-Authz-Arbiter. Final verdict after Node 08: REPROCESS (CWE-862 missing ownership check overrides V9 PASS).

---

## Pair CB4-3.4-098

**Node:** 01 → 02 (split + translation: SSDF + ASVS evidence in same submission)
**Stratum:** C (evidence-type routing: SSDF process evidence must not be labelled as ASVS runtime evidence)
**Source:** NIST SP 800-218 SSDF v1.1, OWASP ASVS 5.0.0
**Verdict:** CONCERN (SSDF) + PASS (ASVS) → overall CONCERN

### A

```
submission:
  type: security_verification_request
  system: "AuthService v2.0"
  claims:
    - "threat model updated Q1 2026 — covers authentication flows and session token handling"
    - "SAST tooling integrated in CI pipeline; scan results from last sprint attached"
    - "bcrypt password hashing with cost=12 implemented per V6.2.5"
    - "TLS 1.3 enforced for all credential transmission per V6.2.3"
  evidence_type_notes:
    - "items 1–2 are design/process artefacts (threat model, SAST reports)"
    - "items 3–4 are runtime implementation claims"
  sub_phase: "3.4"
```

### B

```
territories_identified:
  - id: T-ssdf-process
    label: "ssdf-process-evidence"
    source_signal: "threat model, SAST results, design artefacts, Q1 2026 update"
    domain: SSDF-PW
    items: [1, 2]
  - id: T-asvs-auth
    label: "asvs-authentication"
    source_signal: "V6.2.5, V6.2.3, bcrypt, TLS 1.3, credential transmission"
    domain: ASVS-V6
    items: [3, 4]
routing:
  - territory: T-ssdf-process → SSDF-PW-Arbiter path
  - territory: T-asvs-auth → ASVS-V6-Auth-Arbiter path
```

### Reason

The submission mixes two distinct evidence types: process artefacts (threat model, SAST reports → SSDF territory) and runtime implementation claims (bcrypt, TLS → ASVS territory). Node 01 must split these into separate territories. FM-SSDF-1 (EVIDENCE_TYPE_CONFLATION) failure: applying a single ASVS territory to all four items would route the threat model and SAST reports to ASVS-V6-Auth-Arbiter, which does not evaluate process evidence — those items would either be ignored or incorrectly assessed as PASS. The SSDF-PW-Arbiter and ASVS-V6-Auth-Arbiter are independent and parallel; neither overrides the other.

### Phrasing B

```
AuthService v2.0 mixes two types of evidence in one submission:
Design artefacts: threat model (Q1 2026) and SAST scan results.
Implementation claims: bcrypt cost=12 (V6.2.5) and TLS 1.3 (V6.2.3).
How does Node 01 split this, and what territory labels does Node 02 assign?
```

Expected: Node 01 splits into T-ssdf-process (items 1-2) and T-asvs-auth (items 3-4). Node 02 labels them ssdf-process-evidence and asvs-authentication respectively. The SSDF territory routes to SSDF-PW-Arbiter; the ASVS territory routes to ASVS-V6-Auth-Arbiter. These arbiters run independently.

---

## Pair CB4-3.4-099

**Node:** 03 (Scheme Planner — multi-source parallel plan with sync point)
**Stratum:** C (depth — plan structure for four-territory multi-source submission)
**Source:** OWASP ASVS 5.0.0, NIST SSDF, CWE Top 25 2023, IETF RFCs
**Verdict:** Plan (no verdict at Node 03)

### A

```
translated_territories:
  - territory: T-asvs-auth
    label: "asvs-authentication"
    domain: ASVS-V6
  - territory: T-ssdf-process
    label: "ssdf-process-evidence"
    domain: SSDF-PW
  - territory: T-cwe-authz
    label: "cwe-authz"
    domain: CWE-Authorization
  - territory: T-rfc-jwt
    label: "rfc-jwt-normative"
    domain: RFC-JWT
submission_id: SUB-8821
sub_phase: "3.4"
```

### B

```
execution_plan:
  submission_id: SUB-8821
  plan_type: parallel_with_sync
  parallel_branches:
    - branch_id: B1
      territories: [T-asvs-auth]
      nodes: [4, 5, 6]
      arbiter: ASVS-V6-Auth-Arbiter
    - branch_id: B2
      territories: [T-ssdf-process]
      nodes: [4, 5, 6]
      arbiter: SSDF-PW-Arbiter
    - branch_id: B3
      territories: [T-cwe-authz]
      nodes: [4, 5, 6]
      arbiter: CWE-Authz-Arbiter
    - branch_id: B4
      territories: [T-rfc-jwt]
      nodes: [4, 5, 6]
      arbiter: RFC-JWT-Arbiter
  sync_point:
    after: all_branches_complete
    node: 7
    purpose: "consolidate all branch results for arbiter selection and multi-arbiter evaluation"
  continuation: [8, 9]
```

### Reason

Four territories from four different source families require four parallel branches, each processing independently through Nodes 4-5-6 before converging at Node 7. A linear plan would force sequential evaluation and potentially apply one source family's routing logic to another family's territory — incorrect. The sync point at Node 7 is essential: Node 7 must see all four branch results simultaneously to make a complete arbiter selection. Node 8 then receives all four arbiter assignments and all four verdicts. Node 9 consolidates into a single output.

### Phrasing B

```
Node 03 receives four translated territories for submission SUB-8821:
T-asvs-auth (ASVS-V6), T-ssdf-process (SSDF-PW), T-cwe-authz (CWE-authz), T-rfc-jwt (RFC-JWT).
What is the correct execution plan structure?
```

Expected: parallel_with_sync plan with four branches, each containing one territory and running independently through Nodes 4-5-6, syncing at Node 7 before Nodes 8 and 9. A linear plan is incorrect; a plan with fewer than four branches means some territory is merged incorrectly.

---

## Pair CB4-3.4-100

**Node:** 08 (Arbiter Units — multi-source PASS + REPROCESS; REPROCESS from SSDF does not override ASVS PASS)
**Stratum:** C (depth — independence of arbiter verdicts across source families)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
arbiter_inputs:
  - arbiter: ASVS-V6-Auth-Arbiter
    territory: T-asvs-auth
    findings:
      - requirement: V6.2.3
        status: "TLS 1.3 enforced for credential transmission — satisfied"
      - requirement: V6.2.5
        status: "bcrypt cost=12 — satisfied"
  - arbiter: SSDF-PW-Arbiter
    territory: T-ssdf-process
    findings:
      - practice: PW.2
        status: "threat model review: last updated 22 months ago — gap"
        evidence: "team states 'reviewed in 2024' but no artefact with date provided"
pipeline_run_id: PR-4577
```

### B

```
arbiter_results:
  - arbiter: ASVS-V6-Auth-Arbiter
    verdict: PASS
    satisfied: [V6.2.3, V6.2.5]
    notes: "TLS and bcrypt requirements satisfied independently of SSDF findings."

  - arbiter: SSDF-PW-Arbiter
    verdict: REPROCESS
    practice: PW.2
    gap: "Threat model not updated within recommended cycle; 22 months since last review. Artefact with update date required."
    action_required: "Provide dated threat model artefact confirming review within the past 12 months before re-submission."
```

### Reason

ASVS-V6-Auth-Arbiter and SSDF-PW-Arbiter are independent: they evaluate different evidence types (runtime controls vs. process artefacts). The ASVS-V6 PASS stands regardless of the SSDF finding. The SSDF-PW REPROCESS stands regardless of the ASVS finding. Neither arbiter's verdict is influenced by the other. The SSDF-PW finding is REPROCESS (not CONCERN) because the team provided no dated artefact at all — FM-SSDF-5 (ARTIFACT_NAME_REQUIRED): "reviewed in 2024" is not verifiable evidence. The overall_verdict at Node 9 will be REPROCESS because at least one arbiter returned REPROCESS.

### Phrasing B

```
Two arbiters for PR-4577.
ASVS-V6: V6.2.3 and V6.2.5 satisfied (TLS 1.3, bcrypt cost=12).
SSDF-PW-Arbiter: PW.2 gap — threat model 22 months old, team says "reviewed in 2024" with no dated artefact.
What verdict does each arbiter return, and why are they independent?
```

Expected: ASVS-V6 PASS (controls satisfied), SSDF-PW REPROCESS (no dated artefact = FM-SSDF-5, 22-month gap = FM-SSDF-3). Verdicts are independent because they assess different evidence types. ASVS PASS does not compensate for SSDF REPROCESS.

---

## Pair CB4-3.4-101

**Node:** 02 (Domain Translator — RFC vocabulary must not be labelled as ASVS)
**Stratum:** C (depth — RFC normative claim routed incorrectly to ASVS territory)
**Source:** IETF RFCs 7519, OWASP ASVS 5.0.0
**Verdict:** REPROCESS (routing failure if misapplied)

### A

```
territory_input:
  raw_content: "JWT tokens are validated per RFC 7519 §4.1.6 — only RS256 permitted; alg:none and HS256 rejected at the validation layer"
  possible_labels:
    - "token-validation"
    - "rfc-jwt-normative"
    - "asvs-authentication"
  sub_phase: "3.4"
```

### B

```
territory_translation:
  assigned_label: "rfc-jwt-normative"
  assigned_domain: RFC-JWT
  rationale: "RFC 7519 §4.1.6 is cited explicitly; the claim references a normative RFC section, not an ASVS requirement ID. Token validation against RFC requirements routes to RFC-JWT-Arbiter, not ASVS-V9-Token-Arbiter."
  excluded_labels:
    - label: "asvs-authentication"
      reason: "No ASVS requirement ID cited (V6.x, V9.x). Authentication vocabulary absent. RFC section citation governs."
    - label: "token-validation"
      reason: "Generic label; insufficient precision when RFC section is named explicitly. rfc-jwt-normative is the correct specific label."
```

### Reason

When the team cites an RFC section number explicitly (RFC 7519 §4.1.6), the territory must be labelled rfc-jwt-normative and routed to RFC-JWT-Arbiter. Labelling it "asvs-authentication" or "token-validation" would route it to ASVS-V9-Token-Arbiter or ASVS-V6-Auth-Arbiter, which do not evaluate RFC normative compliance. The RFC-JWT-Arbiter checks whether the RFC MUST-level requirements are satisfied; ASVS arbiters check ASVS requirement IDs. The algorithm restriction claim (RS256 only, alg:none and HS256 rejected) maps to RFC 7519 §8 prohibition, which also belongs in the RFC-JWT territory, not an ASVS territory.

### Phrasing B

```
Node 02 receives a territory item: "JWT validated per RFC 7519 §4.1.6; RS256 only; alg:none and HS256 rejected."
Three candidate labels: token-validation, rfc-jwt-normative, asvs-authentication.
Which label is correct, and which arbiters does this territory route to?
```

Expected: label = rfc-jwt-normative, domain = RFC-JWT, arbiter = RFC-JWT-Arbiter. Excluded: asvs-authentication (no ASVS ID cited), token-validation (insufficient precision). If the team had cited V9.1.1 alongside RFC 7519 §4.1.6, two territories would be created — one per source family.

---

## Pair CB4-3.4-102

**Node:** 07 (Arbiter Selector — maximum arbiter selection: all four source families present)
**Stratum:** C (depth — correct enumeration when all four source families contribute vocabulary)
**Source:** OWASP ASVS 5.0.0, NIST SSDF, CWE Top 25 2023, IETF RFCs
**Verdict:** Selection (no verdict at Node 07)

### A

```
consolidated_findings:
  vocabulary_signals:
    asvs: ["V6.2.3", "V6.2.5", "V7.2.1", "V9.1.1", "V10.4.6"]
    ssdf: ["PW.1.1", "PW.2.1", "PW.8.2"]
    cwe: ["CWE-287", "CWE-862"]
    rfc: ["RFC7519-§4.1.6", "RFC6749-§4.1", "RFC7636-§4.2"]
  territories_present:
    - "asvs-authentication" (V6)
    - "asvs-session" (V7)
    - "token-validation" (V9)
    - "oauth-oidc" (V10)
    - "ssdf-process-evidence" (PW)
    - "cwe-authn" (CWE-287)
    - "cwe-authz" (CWE-862)
    - "rfc-jwt-normative" (RFC7519)
    - "rfc-oauth2-normative" (RFC6749+RFC7636)
pipeline_run_id: PR-4589
```

### B

```
arbiter_selection:
  - ASVS-V6-Auth-Arbiter
    reason: "V6.2.3, V6.2.5 — authentication controls"
  - ASVS-V7-Session-Arbiter
    reason: "V7.2.1 — session management"
  - ASVS-V9-Token-Arbiter
    reason: "V9.1.1 — non-OAuth JWT token"
  - ASVS-V10-OAuth-Arbiter
    reason: "V10.4.6 — OAuth PKCE"
  - SSDF-PW-Arbiter
    reason: "PW.1.1, PW.2.1, PW.8.2 — secure software development practices"
  - CWE-Authn-Arbiter
    reason: "CWE-287 — authentication weakness"
  - CWE-Authz-Arbiter
    reason: "CWE-862 — authorization gap"
  - RFC-JWT-Arbiter
    reason: "RFC7519-§4.1.6 — JWT normative requirements"
  - RFC-OAuth2-Arbiter
    reason: "RFC6749-§4.1, RFC7636-§4.2 — OAuth normative requirements"
selection_count: 9
```

### Reason

All four source families contribute vocabulary. All nine possible arbiters are selected. The key rule: no arbiter is omitted if its source family's vocabulary is present. Selecting 8 of 9 (e.g., omitting SSDF-PW-Arbiter) would create a structural gap where SSDF process evidence goes unevaluated. Selecting arbiters not in the input (e.g., SSDF-PO-Arbiter) is also incorrect — no PO vocabulary was present. The ASVS split across V6/V7/V9/V10 is correct: each chapter is a separate arbiter, not a single "ASVS-Arbiter". V10 is separate from V9 because OAuth vocabulary (V10.4.6, RFC6749) is present.

### Phrasing B

```
Submission PR-4589 contains vocabulary from all four source families:
ASVS V6, V7, V9, V10; SSDF PW; CWE-287 + CWE-862; RFC7519 + RFC6749 + RFC7636.
How many arbiters does Node 07 select, and which are they?
```

Expected: 9 arbiters — ASVS-V6-Auth, ASVS-V7-Session, ASVS-V9-Token, ASVS-V10-OAuth, SSDF-PW, CWE-Authn, CWE-Authz, RFC-JWT, RFC-OAuth2. All nine must be named. SSDF-PO is not selected (no PO vocabulary). Selection count = 9.

---

## Pair CB4-3.4-103

**Node:** 08 (Arbiter Units — ASVS PASS does not compensate for CWE REPROCESS when they assess different gaps)
**Stratum:** B (team context — team argues ASVS V8 PASS means CWE-862 does not apply)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter_inputs:
  - arbiter: ASVS-V8-AuthZ-Arbiter
    territory: T-asvs-authz
    findings:
      - requirement: V8.1.1
        status: "team claims: role-based access control implemented; users can only see their own order history"
        team_note: "we believe V8.1.1 is satisfied because RBAC is in place"
  - arbiter: CWE-Authz-Arbiter
    territory: T-cwe-authz
    findings:
      - "endpoint GET /api/admin/reports/{report_id} accessible by any authenticated user"
      - "report_id is a sequential integer; any user can enumerate all reports"
      - "RBAC check exists on the route but checks user.is_authenticated only, not user.is_admin"
pipeline_run_id: PR-4601
team_argument: "ASVS V8.1.1 is satisfied, so there is no authorization issue"
```

### B

```
arbiter_results:
  - arbiter: ASVS-V8-AuthZ-Arbiter
    verdict: PASS
    satisfied: [V8.1.1]
    notes: "RBAC implementation satisfies V8.1.1 for the order history endpoint described. Assessment limited to the evidence provided for that endpoint."

  - arbiter: CWE-Authz-Arbiter
    verdict: REPROCESS
    cwe_id: CWE-862
    severity: HIGH
    gap: "GET /api/admin/reports/{report_id} checks user.is_authenticated but not user.is_admin. Any authenticated user can access administrative reports. RBAC check is present but incorrect — it checks authentication state, not authorization role."
    action_required: "Add role check (user.is_admin or equivalent) to the /api/admin/reports/{id} endpoint before the report data is returned."
    note: "ASVS V8.1.1 PASS for order history does not exempt the admin reports endpoint from authorization requirements. Each endpoint is assessed independently."
```

### Reason

The team's argument — "V8.1.1 is satisfied, so there is no authorization issue" — is a category error. ASVS V8.1.1 was evaluated for the order history endpoint and passed. The CWE-862 finding concerns a different endpoint (/api/admin/reports/{id}). PASS on one endpoint does not create immunity for another endpoint. The CWE-Authz-Arbiter finding is REPROCESS (not CONCERN) because the authorization check is present but checks authentication state only — this is CWE-862 (absent effective check), not CWE-269 (check present, scope wrong). The note in the CWE block explicitly refutes the team's cross-arbiter argument.

### Phrasing B

```
Team submits two territories for PR-4601.
ASVS-V8: RBAC in place for order history endpoint; team claims V8.1.1 satisfied.
CWE-Authz: /api/admin/reports/{id} — any authenticated user can access; check only verifies
user.is_authenticated, not user.is_admin.
Team argues: "V8.1.1 PASS means no authorization issue exists."
What does each arbiter return, and how does the note field address the team argument?
```

Expected: ASVS-V8 PASS (order history endpoint evidence accepted), CWE-Authz REPROCESS CWE-862 HIGH (admin reports endpoint has absent effective authorization check). Note in CWE block refutes cross-arbiter argument: PASS on one endpoint does not exempt another. Overall verdict: REPROCESS.

---

## Pair CB4-3.4-104

**Node:** 06 (Result Translator — vocabulary preservation for cross-source routing)
**Stratum:** C (depth — translator must preserve source-family vocabulary markers for Node 7)
**Source:** OWASP ASVS 5.0.0, IETF RFC 7519
**Verdict:** Routing (no final verdict at Node 06)

### A

```
domain_processor_output:
  territory: T-jwt-implementation
  findings:
    - "token signing algorithm: RS256"
    - "issuer claim (iss) present: api.example.com"
    - "audience claim (aud) present: mobile-client"
    - "expiry (exp): 15 minutes from issuance"
    - "ASVS V9.1.1: RS256 satisfies asymmetric signing requirement"
    - "ASVS V9.2.1: exp present and within permitted range"
    - "RFC 7519 §4.1.6: RS256 satisfies alg MUST clause"
    - "RFC 7519 §4.1.4: exp present and validates correctly"
pipeline_run_id: PR-4614
```

### B

```
translated_result:
  territory: T-jwt-implementation
  pipeline_run_id: PR-4614
  vocabulary_preserved:
    asvs_signals: ["V9.1.1", "V9.2.1", "RS256", "asymmetric signing"]
    rfc_signals: ["RFC7519-§4.1.6", "RFC7519-§4.1.4", "exp", "alg"]
  finding_summary: "RS256 JWT with iss, aud, and 15-minute exp satisfies both ASVS V9 token requirements and RFC 7519 MUST-level normative requirements."
  routing_signals:
    - "ASVS-V9-territory confirmed: V9.1.1, V9.2.1 satisfied"
    - "RFC-JWT-territory confirmed: RFC7519-§4.1.6, RFC7519-§4.1.4 satisfied"
  verdict_signal: PASS
```

### Reason

Node 06 must preserve both ASVS-V9 vocabulary (V9.1.1, V9.2.1) and RFC-JWT vocabulary (RFC7519-§4.1.6, §4.1.4) in its output. If the translator strips RFC section references and returns only ASVS IDs, Node 7 will select only ASVS-V9-Token-Arbiter and miss RFC-JWT-Arbiter. If the translator strips ASVS IDs and returns only RFC references, Node 7 will miss the ASVS coverage. Both signal sets must be present and labelled in the routing_signals. The finding_summary may condense the prose, but the vocabulary_preserved block must contain the original source signals.

### Phrasing B

```
Node 06 receives findings that reference both ASVS V9 requirement IDs (V9.1.1, V9.2.1)
and RFC 7519 section numbers (§4.1.6, §4.1.4) for the same JWT token claims.
What must the translated result preserve, and why does stripping either signal set cause a downstream failure?
```

Expected: translated_result must preserve both ASVS-V9 signals and RFC-JWT signals in vocabulary_preserved. Stripping ASVS signals → Node 7 misses ASVS-V9-Token-Arbiter. Stripping RFC signals → Node 7 misses RFC-JWT-Arbiter. Either omission creates an incomplete arbiter selection (FM-4).

---

## Pair CB4-3.4-105

**Node:** 04 (Domain Processor — team cites multiple standards; processor must map each claim to its authoritative standard)
**Stratum:** B (team context — team conflates ASVS and SSDF requirements for the same artefact)
**Source:** OWASP ASVS 5.0.0, NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
territory: T-threat-model-claim
domain_label: "ssdf-process-evidence"
team_input:
  claim: "We have a threat model that covers authentication and session flows"
  supporting_evidence: "threat-model-v3.pdf, last updated 2025-11-10"
  team_note: "we believe this satisfies both ASVS V6 and SSDF PW.2 requirements"
pipeline_run_id: PR-4622
```

### B

```
domain_processor_output:
  territory: T-threat-model-claim
  pipeline_run_id: PR-4622

  finding_1:
    standard: SSDF-PW.2
    claim: "Threat model updated 2025-11-10 — approximately 6 months ago."
    evaluation: "Within recommended annual review cycle. PW.2 practice evidence present."
    verdict_signal: PASS

  finding_2:
    standard: ASVS-V6
    claim: "Threat model document is not ASVS V6 runtime evidence."
    evaluation: "ASVS V6 requirements (V6.2.3, V6.2.5) require runtime implementation claims, not design artefacts. The threat model cannot satisfy V6 requirements. The team's claim that this satisfies ASVS V6 is not supportable."
    verdict_signal: CONCERN
    note: "CONCERN not REPROCESS because the team did not claim to have satisfied a specific ASVS V6 requirement with incorrect evidence — they conflated the standard categories. Separate ASVS V6 evidence (TLS configuration, hashing implementation) would be needed to assess V6 compliance."
```

### Reason

The threat model is SSDF-PW.2 evidence (design-phase artefact) and satisfies that practice. It is not ASVS-V6 runtime evidence. Node 04 must separate the two claims: (1) threat model satisfies SSDF PW.2 (PASS — recent enough), (2) the team's claim that it also satisfies ASVS V6 is incorrect — ASVS V6 requires runtime evidence. This is FM-SSDF-1 (EVIDENCE_TYPE_CONFLATION) at the team level, not the pipeline level. The verdict_signal for finding_2 is CONCERN (not REPROCESS) because the team expressed confusion about standard applicability, but did not submit falsified or absent required evidence — they simply need to submit ASVS V6 runtime claims separately.

### Phrasing B

```
Team submits: "threat-model-v3.pdf (updated 2025-11-10) satisfies both ASVS V6 and SSDF PW.2."
The territory is labelled ssdf-process-evidence.
How does Node 04 evaluate the two standards separately?
```

Expected: SSDF PW.2 = PASS (threat model present, updated 6 months ago, within annual cycle). ASVS V6 = CONCERN (threat model is not runtime implementation evidence; team needs to submit separate ASVS V6 claims). CONCERN not REPROCESS because no specific V6 requirement was falsely claimed as satisfied.

---

## Pair CB4-3.4-106

**Node:** 08 (Arbiter Units — full four-source PASS scenario; all arbiters return PASS)
**Stratum:** C (depth — baseline multi-source PASS, no gaps, all requirements satisfied)
**Source:** OWASP ASVS 5.0.0, NIST SSDF, CWE Top 25 2023, IETF RFCs
**Verdict:** PASS

### A

```
arbiter_inputs:
  - arbiter: ASVS-V6-Auth-Arbiter
    findings: ["V6.2.3: TLS 1.3 confirmed", "V6.2.5: bcrypt cost=12 confirmed"]
  - arbiter: ASVS-V7-Session-Arbiter
    findings: ["V7.2.1: session tokens over TLS only", "V7.3.1: session invalidated on logout"]
  - arbiter: ASVS-V9-Token-Arbiter
    findings: ["V9.1.1: RS256 signing", "V9.2.1: exp=15min"]
  - arbiter: SSDF-PW-Arbiter
    findings: ["PW.1.1: security requirements documented", "PW.2.1: threat model Q1 2026", "PW.8.2: peer review mandatory"]
  - arbiter: CWE-Authn-Arbiter
    findings: ["no CWE-287 pattern detected", "authentication mechanism RS256+TLS is sound"]
  - arbiter: CWE-Authz-Arbiter
    findings: ["no CWE-862 pattern detected", "ownership check present on all protected endpoints"]
  - arbiter: RFC-JWT-Arbiter
    findings: ["RFC7519-§4.1.1 iss present", "RFC7519-§4.1.3 aud present", "RFC7519-§4.1.4 exp valid", "RFC7519-§4.1.6 RS256 — alg:none prohibited"]
pipeline_run_id: PR-4635
```

### B

```
arbiter_results:
  - arbiter: ASVS-V6-Auth-Arbiter
    verdict: PASS
    satisfied: [V6.2.3, V6.2.5]

  - arbiter: ASVS-V7-Session-Arbiter
    verdict: PASS
    satisfied: [V7.2.1, V7.3.1]

  - arbiter: ASVS-V9-Token-Arbiter
    verdict: PASS
    satisfied: [V9.1.1, V9.2.1]

  - arbiter: SSDF-PW-Arbiter
    verdict: PASS
    satisfied: [PW.1.1, PW.2.1, PW.8.2]

  - arbiter: CWE-Authn-Arbiter
    verdict: PASS
    finding: "No CWE-287 (improper authentication) pattern identified in submitted evidence."

  - arbiter: CWE-Authz-Arbiter
    verdict: PASS
    finding: "No CWE-862 (missing authorization check) pattern identified; ownership checks confirmed on protected endpoints."

  - arbiter: RFC-JWT-Arbiter
    verdict: PASS
    satisfied: [RFC7519-§4.1.1, RFC7519-§4.1.3, RFC7519-§4.1.4, RFC7519-§4.1.6]
```

### Reason

All seven arbiters return PASS. Each arbiter evaluates its own territory independently; no arbiter's PASS result needs justification from another arbiter's finding. The CWE arbiter PASS blocks include a finding field (not satisfied/violated) because CWE arbiters assess for weakness absence, not requirement satisfaction — the finding states what was not found. The SSDF-PW-Arbiter PASS lists satisfied practices with their task IDs. Node 09 will produce an overall_verdict PASS with seven arbiter blocks and a scope_boundary statement.

### Phrasing B

```
Run PR-4635: seven arbiters, all findings positive.
ASVS V6, V7, V9 all satisfied. SSDF PW practices met. No CWE-287 or CWE-862 patterns.
RFC 7519 MUST-level claims all confirmed.
Produce the arbiter results block for all seven arbiters.
```

Expected: seven PASS blocks. ASVS and SSDF blocks use satisfied/practices_satisfied with requirement IDs. CWE blocks use finding field describing absence of weakness pattern. RFC block uses satisfied with section numbers. No REPROCESS or CONCERN entries.

---

## File summary

| Seq | Primary nodes | Stratum | Sources | Verdict | Key training target |
|-----|--------------|---------|---------|---------|---------------------|
| 097 | 01, 07, 08 | C | ASVS+CWE | REPROCESS | Split: V9 token ≠ CWE-862 authz gap; both arbiters required |
| 098 | 01, 02 | C | SSDF+ASVS | CONCERN+PASS | Evidence-type split: SSDF process vs ASVS runtime in same submission |
| 099 | 03 | C | All four | Plan only | Four-territory parallel plan with sync point at Node 7 |
| 100 | 08 | C | ASVS+SSDF | REPROCESS | Arbiter independence: ASVS PASS does not compensate SSDF REPROCESS |
| 101 | 02 | C | IETF+ASVS | Routing | RFC section citation → rfc-jwt-normative label, not asvs-authentication |
| 102 | 07 | C | All four | Selection | All-source maximum: 9 arbiters selected from four-family vocabulary |
| 103 | 08 | B | ASVS+CWE | REPROCESS | Cross-endpoint: ASVS V8 PASS on order history ≠ CWE-862 PASS on admin reports |
| 104 | 06 | C | ASVS+IETF | Routing | Vocabulary preservation: both ASVS-V9 and RFC-JWT signals must survive Node 06 |
| 105 | 04 | B | ASVS+SSDF | CONCERN | Evidence-type conflation: threat model satisfies SSDF not ASVS V6 |
| 106 | 08 | C | ASVS+SSDF+CWE+IETF | PASS | Full four-source PASS baseline; seven arbiters all PASS |

**Running total: 212 instances (sequences 001–106)**
Stratum breakdown: A ~28%, B ~24%, C ~31%, D ~17%
