# Node 08 — Arbiter Units — Extension Set 3
# Stage 3 Action 3 — Coverage Extension (Stratum D adversarial / edge cases)
# Batch: CB4 — Sub-phase 3.4 (Security and Authorization Model)
# Sequences: CB4-3.4-117 through CB4-3.4-126
# Instances: 20 (10 base pairs × 2 phrasings each)
# Running total after this file: 252 instances (sequences 001–126)

---

## Coverage targets for this file

Stratum D: inputs that test the pipeline's discrimination against plausible-but-wrong answers.
Primary adversarial scenarios:
  - Evidence submitted for a different requirement than the gap (requirement substitution)
  - Partial evidence presented as full evidence (evidence completeness)
  - JWT without exp claim — team argues "exp is optional per RFC 7519"
  - ASVS V9 vs V7 confusion: team cites V9 for a session token (not a JWT for auth delegation)
  - SSDF policy document vs implementation artefact
  - CWE-862 vs CWE-269 argued by team: "the check exists, it's just checking the wrong thing"
  - Team re-frames REPROCESS gap as CONCERN by adding a post-hoc risk statement
  - RFC MAY vs MUST threshold: team uses RFC permissive language to argue CONCERN should be PASS
  - Requirement ID cited that does not exist in ASVS 5.0.0 (wrong version / fabricated ID)
  - SSDF PW.4 vs PW.8: team submits scan results for the wrong practice

---

## Pair CB4-3.4-117

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — team submits evidence for a related but different requirement than the gap)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V9-Token-Arbiter
territory: T-token-validation
findings:
  - requirement: V9.1.1
    gap_confirmed: "JWT signing algorithm is HS256 — symmetric key"
  - requirement: V9.2.1
    evidence_submitted: "exp claim set to 15 minutes — confirmed"
    team_note: "we have satisfied V9.2.1 (short expiry) which demonstrates our token security posture is strong"
pipeline_run_id: PR-4762
```

### B

```
arbiter_result:
  arbiter: ASVS-V9-Token-Arbiter
  verdict: REPROCESS
  requirements_violated: [V9.1.1]
  gap: "JWT signed with HS256 (symmetric shared key). V9.1.1 requires an asymmetric signing algorithm (e.g., RS256, ES256). Symmetric key signing creates a cross-service forgery risk."
  action_required: "Migrate JWT signing to RS256 or equivalent asymmetric algorithm."
  note: "V9.2.1 (exp=15min) is separately satisfied and noted. Compliance with V9.2.1 does not satisfy V9.1.1. Each requirement is assessed independently; satisfying one does not compensate for failing another."
```

### Reason

The team submitted evidence for V9.2.1 (short expiry) as if it addressed the V9.1.1 gap (asymmetric signing). These are independent requirements. The gap in V9.1.1 is HS256 symmetric key — satisfying V9.2.1 does not compensate for this. The arbiter must evaluate V9.1.1 and V9.2.1 separately and return REPROCESS because V9.1.1 is violated. The note acknowledges V9.2.1 as separately satisfied to be precise, but makes explicit that requirement-substitution arguments are not accepted.

### Phrasing B

```
PR-4762. V9.1.1 gap confirmed: HS256 symmetric key.
Team responds: "V9.2.1 is satisfied (exp=15min) which shows strong token security."
ASVS-V9-Token-Arbiter. What verdict, and does V9.2.1 compliance address the V9.1.1 gap?
```

Expected: REPROCESS on V9.1.1. V9.2.1 separately satisfied but irrelevant to V9.1.1 gap. Each requirement assessed independently.

---

## Pair CB4-3.4-118

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — JWT without exp claim; team argues exp is OPTIONAL per RFC 7519)
**Source:** IETF RFC 7519, OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter_inputs:
  - arbiter: RFC-JWT-Arbiter
    findings:
      - "JWT tokens have no exp claim"
      - "RFC 7519 §4.1.4 states: exp claim is OPTIONAL"
      - "team argument: since exp is OPTIONAL in RFC 7519, omitting it is RFC-compliant"
  - arbiter: ASVS-V9-Token-Arbiter
    findings:
      - "V9.2.1: JWT has no exp claim — tokens do not expire"
pipeline_run_id: PR-4774
```

### B

```
arbiter_results:
  - arbiter: RFC-JWT-Arbiter
    verdict: PASS
    note: "RFC 7519 §4.1.4 classifies exp as OPTIONAL. Absence of exp does not violate RFC 7519. RFC-JWT-Arbiter verdict: PASS. Note: the RFC permits absence of exp; ASVS V9.2.1 does not."

  - arbiter: ASVS-V9-Token-Arbiter
    verdict: REPROCESS
    requirements_violated: [V9.2.1]
    gap: "JWT tokens have no exp claim. V9.2.1 requires tokens to carry a short expiry. Tokens without exp do not expire — any compromised or replayed token remains valid indefinitely."
    action_required: "Add exp claim to all issued JWTs. Set expiry to a short duration appropriate to the access grant (recommended: ≤15 minutes for access tokens, ≤24 hours for refresh tokens)."
```

### Reason

RFC 7519 §4.1.4 does classify exp as OPTIONAL — so the RFC-JWT-Arbiter correctly returns PASS (the RFC MUST clauses are not violated). However, ASVS V9.2.1 imposes an additional requirement: tokens must carry a short expiry. ASVS is an application security standard that goes beyond RFC normative requirements in some areas. The RFC PASS does not override the ASVS REPROCESS. Two arbiters, two independent verdicts. The overall verdict (Node 09) will be REPROCESS because at least one arbiter returned REPROCESS.

This pair demonstrates that RFC-compliant ≠ ASVS-compliant: standards have different coverage levels.

### Phrasing B

```
PR-4774. JWTs issued without exp claim.
Team argues: RFC 7519 §4.1.4 says exp is OPTIONAL, so omitting it is compliant.
RFC-JWT-Arbiter and ASVS-V9-Token-Arbiter both run.
What does each return?
```

Expected: RFC-JWT-Arbiter PASS (exp is OPTIONAL in RFC 7519, MUST clauses not violated). ASVS-V9-Token-Arbiter REPROCESS (V9.2.1 requires short expiry; absent exp = non-expiring tokens). RFC PASS does not override ASVS REPROCESS. Overall: REPROCESS.

---

## Pair CB4-3.4-119

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — session token misrouted to V9 by team; correct arbiter is V7)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN (misrouting symptom at the arbiter layer)

### A

```
arbiter: ASVS-V9-Token-Arbiter
territory: T-token-validation
findings:
  - "system uses JWTs as session tokens — token is created on login and stored in httpOnly cookie"
  - "token contains: user_id, session_id, role, exp=8h"
  - "team cites V9.1.1 (RS256) and V9.2.1 (exp) as the applicable requirements"
  - "RS256 signing confirmed"
  - "token is used to maintain session state between HTTP requests — not used for OAuth delegation or API authorization across services"
pipeline_run_id: PR-4785
```

### B

```
arbiter_result:
  arbiter: ASVS-V9-Token-Arbiter
  verdict: CONCERN
  note: "This territory was routed to ASVS-V9-Token-Arbiter, but the token description matches a session token (used to maintain session state between requests, stored in httpOnly cookie, not used for OAuth delegation). Session tokens are governed by ASVS V7 (Session Management), not V9 (Self-contained Tokens). V9 applies to non-session JWTs used for API authorization or cross-service delegation."
  partial_finding:
    v9_perspective: "If assessed under V9: RS256 confirmed (V9.1.1 satisfied); exp=8h may exceed short-expiry expectation for V9.2.1 (recommend ≤15min for API access tokens)."
    v7_redirect: "Route this territory to ASVS-V7-Session-Arbiter for complete assessment. Key V7 requirements: V7.2.1 (session tokens over TLS), V7.3.1 (session invalidated on logout), V7.2.3 (session fixation prevention)."
  verdict_signal: CONCERN
```

### Reason

The arbiter identified that the token is being assessed under the wrong chapter. The token is used as a session token (login → session → httpOnly cookie), which belongs under ASVS V7 (Session Management). ASVS V9 governs self-contained tokens used for delegation and API authorization (not session maintenance). The arbiter returns CONCERN (routing concern) rather than attempting a full V9 evaluation on a token that is fundamentally a session mechanism. The partial_finding block provides guidance on both V7 requirements (redirect path) and the V9 assessment for completeness, so the caller can understand both what should be done and what the V9 evaluation found incidentally.

### Phrasing B

```
PR-4785. JWT used as session token — stored in httpOnly cookie, created on login, contains session_id.
Team cited V9.1.1 and V9.2.1. RS256 confirmed, exp=8h.
ASVS-V9-Token-Arbiter receives this territory.
What should the arbiter return?
```

Expected: CONCERN with routing note. Session token → V7 (not V9). Partial V9 finding included (V9.1.1 satisfied; V9.2.1 concern on 8h expiry). Direct to ASVS-V7-Session-Arbiter for complete assessment.

---

## Pair CB4-3.4-120

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — SSDF policy document submitted instead of implementation artefact)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** REPROCESS

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-process
findings:
  - practice: PW.4
    evidence: "Secure Development Policy v2.3 — section 4.2 states: 'All code must undergo SAST scanning before merge to main'"
    team_note: "our policy mandates SAST scanning; PW.4 requires that we check for security weaknesses in software, which our policy covers"
  - sast_scan_results: "not provided"
pipeline_run_id: PR-4796
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: REPROCESS
  practice: PW.4
  gap: "Policy document (Secure Development Policy v2.3 §4.2) states SAST scanning is required. No SAST scan results, CI pipeline configuration, or tool output provided. A policy that mandates SAST scanning is not evidence that SAST scanning has been performed."
  action_required: "Provide SAST scan results or CI pipeline configuration confirming SAST tooling is integrated and executing. Acceptable evidence: scan report from current sprint, CI configuration file showing SAST step, or tool dashboard export showing recent scan history."
  note: "SSDF PW.4 requires that checks for weaknesses are actually performed, not that a policy requires them to be performed. Policy → artefact is the evidence chain: policy alone stops halfway."
```

### Reason

A policy document is organizational intent, not implementation evidence. SSDF PW.4 requires that checks for security weaknesses are actually performed (verified by scan results or tool integration evidence). The team's policy mandates SAST — that is useful context but not satisfying evidence. The arbiter must return REPROCESS because no implementation artefact was provided. This is FM-SSDF-5 (ARTIFACT_NAME_REQUIRED) applied to the PW.4 practice: the practice requires artefact evidence, not policy statements.

### Phrasing B

```
SSDF-PW-Arbiter, PW.4, run PR-4796.
Team provides: Secure Development Policy v2.3 §4.2 saying SAST is mandatory.
No scan results or CI config provided.
Does a policy document satisfy PW.4?
```

Expected: REPROCESS. Policy states intent; PW.4 requires implementation evidence (scan results, CI config, tool output). ARTIFACT_NAME_REQUIRED applies. Acceptable evidence forms listed.

---

## Pair CB4-3.4-121

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — team argues CWE-862 is actually CWE-269 to avoid REPROCESS)
**Source:** CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter: CWE-Authz-Arbiter
territory: T-cwe-authz
findings:
  - endpoint: "/api/customers/{customer_id}/payment-methods"
    status: "no authorization check — any authenticated user can list payment methods for any customer_id"
    team_note: "we do have an authorization check in the customer profile module; this endpoint is part of the same module. The check exists, it's just not on this specific endpoint path. We believe this is CWE-269 (improper privilege management) not CWE-862 (missing check) because the overall authorization design is correct."
pipeline_run_id: PR-4807
```

### B

```
arbiter_result:
  arbiter: CWE-Authz-Arbiter
  verdict: REPROCESS
  cwe_id: CWE-862
  severity: HIGH
  gap: "No authorization check on /api/customers/{customer_id}/payment-methods. The absence of a check on this specific endpoint is CWE-862 (Missing Authorization), not CWE-269 (Improper Privilege Management). CWE-269 applies when an authorization check exists but grants excessive or incorrect scope. CWE-862 applies when the check is absent."
  action_required: "Add authorization check to /api/customers/{customer_id}/payment-methods verifying the requesting user is the customer_id owner or an authorized representative before returning payment method data."
  note: "The presence of authorization checks on other endpoints in the same module does not create an authorization check on this endpoint. CWE classification is determined by the presence or absence of a check on the specific resource, not by the general design intent."
```

### Reason

The team's argument attempts to reclassify CWE-862 as CWE-269 to reduce the finding to CONCERN. The distinction: CWE-862 = check absent; CWE-269 = check present but wrong scope. On this specific endpoint (/api/customers/{id}/payment-methods), there is no check. That is CWE-862, not CWE-269. The presence of checks elsewhere in the module does not create a check on this endpoint. This is FM-CWE-2 (CWE_CATEGORY_CONFUSION) — the team is using the wrong CWE classification to argue for a lower severity verdict. REPROCESS stands.

### Phrasing B

```
Endpoint /api/customers/{id}/payment-methods: no authorization check. Any authenticated user can access.
Team argues: checks exist elsewhere in the module; this is CWE-269 not CWE-862 because design intent is correct.
CWE-Authz-Arbiter, run PR-4807. Which CWE applies, and what verdict?
```

Expected: REPROCESS, CWE-862. Check is absent on this endpoint = CWE-862. CWE-269 requires a check to be present but incorrect. Team's reclassification argument rejected. Checks on other endpoints are irrelevant.

---

## Pair CB4-3.4-122

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — team adds post-hoc risk statement to re-frame REPROCESS as CONCERN)
**Source:** OWASP ASVS 5.0.0
**Verdict:** REPROCESS

### A

```
arbiter: ASVS-V7-Session-Arbiter
territory: T-asvs-session
findings:
  - requirement: V7.3.1
    status: "session token remains valid after logout — server-side session not invalidated"
    team_note: "we acknowledge the session is not server-side invalidated on logout. However, we have a formal risk statement (Risk ID: R-2026-044) approved by our security team acknowledging this as a low-risk deviation given our 2-hour session expiry. We believe this should be classified as CONCERN not REPROCESS."
pipeline_run_id: PR-4818
```

### B

```
arbiter_result:
  arbiter: ASVS-V7-Session-Arbiter
  verdict: REPROCESS
  requirements_violated: [V7.3.1]
  gap: "Session token not invalidated server-side on logout. V7.3.1 is a MUST-level requirement. A session that survives logout can be replayed by an attacker who captured the token before logout (e.g., from browser history, network intercept, or shared device)."
  action_required: "Implement server-side session invalidation on logout. The session server record must be marked invalid when the user logs out, such that subsequent requests bearing the same session token are rejected."
  note: "A risk statement classifying this as low-risk does not change the requirement level. V7.3.1 is MUST-level in ASVS 5.0.0. A team risk statement reclassifying a MUST violation as acceptable is an organizational decision — it does not reclassify the technical finding from REPROCESS to CONCERN. REPROCESS means the implementation does not satisfy the requirement; the risk statement is noted but does not change the technical verdict."
```

### Reason

The team added a risk statement to reframe a MUST violation as "low risk" and argue for CONCERN. This is the same pattern as the risk acceptance in pair 115, but applied here to push a REPROCESS to CONCERN (not just to dismiss the finding entirely). ASVS V7.3.1 is MUST-level. The risk statement is an organizational governance document. It does not change the technical requirement level. REPROCESS stands. A 2-hour expiry reduces the window for token replay but does not eliminate it — a captured token can still be replayed within the expiry window.

### Phrasing B

```
V7.3.1: session not server-side invalidated on logout. Token stays valid after logout.
Team provides risk statement R-2026-044 classifying this as low-risk. Argues for CONCERN not REPROCESS.
ASVS-V7-Session-Arbiter, run PR-4818.
```

Expected: REPROCESS. V7.3.1 is MUST-level; risk statement doesn't change requirement classification. Session survives logout = REPROCESS regardless of expiry duration.

---

## Pair CB4-3.4-123

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — team uses RFC MAY language to argue CONCERN should be PASS)
**Source:** IETF RFC 7519
**Verdict:** CONCERN (correctly evaluated; the MAY verdict is correct, not elevated to PASS)

### A

```
arbiter: RFC-JWT-Arbiter
territory: T-rfc-jwt-normative
findings:
  - "JWT tokens use a 30-minute expiry (exp=30min)"
  - "RFC 7519 §4.1.4 states: 'The processing of the exp claim requires that the current date/time MUST be before the expiration date/time listed in the exp claim'"
  - "RFC 7519 §8 states: 'Implementations SHOULD validate the alg header parameter and SHOULD reject JWTs that use algorithms the implementation does not support'"
  - "There is no RFC 7519 MUST clause specifying a maximum exp duration"
  - "team argument: since RFC 7519 has no MUST maximum exp, the 30-minute expiry fully satisfies all RFC MUST requirements"
  - "best practice: access tokens with 30-minute expiry are longer than the typically recommended 15-minute window"
pipeline_run_id: PR-4829
```

### B

```
arbiter_result:
  arbiter: RFC-JWT-Arbiter
  verdict: PASS
  satisfied: [RFC7519-§4.1.4]
  note: "RFC 7519 does not define a MUST maximum expiry duration. A 30-minute expiry satisfies all RFC 7519 MUST-level requirements. Team's RFC compliance argument is correct for this arbiter. Note: ASVS V9.2.1 recommends shorter expiry for access tokens (typically ≤15min); if ASVS-V9-Token-Arbiter was included in this run, V9.2.1 would warrant a CONCERN. RFC-JWT-Arbiter scope is limited to RFC normative compliance."
```

### Reason

This pair trains a nuanced case: the RFC-JWT-Arbiter returns PASS because the team is right that RFC 7519 has no MUST maximum expiry duration. The team's RFC-compliance argument is technically correct for the RFC. However, the note clarifies that ASVS V9.2.1 would raise a CONCERN about the 30-minute duration — that is a different arbiter's jurisdiction. The key training point: the RFC-JWT-Arbiter evaluates RFC MUST requirements only; it must not apply ASVS requirements, and it must not inflate RFC-compliant behavior to CONCERN just because ASVS has stricter recommendations. FM-RFC-1 (RFC_SURFACE_CLAIM) in the other direction: an arbiter that returns CONCERN where the RFC imposes no MUST requirement is over-reaching.

### Phrasing B

```
JWT exp=30 minutes. RFC 7519 §4.1.4: MUST validate exp — satisfied.
RFC 7519 has no MUST maximum exp duration.
Team argues RFC compliance is fully satisfied. RFC-JWT-Arbiter, run PR-4829.
Correct verdict?
```

Expected: PASS (RFC-JWT-Arbiter). RFC 7519 imposes no MUST maximum; exp present and validated = RFC MUST satisfied. Note: ASVS V9.2.1 has a stricter recommendation — if that arbiter runs, 30min may produce a CONCERN; that is outside the RFC-JWT-Arbiter's scope.

---

## Pair CB4-3.4-124

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — team cites requirement ID from ASVS 4.0.3, not 5.0.0)
**Source:** OWASP ASVS 5.0.0
**Verdict:** CONCERN

### A

```
arbiter: ASVS-V6-Auth-Arbiter
territory: T-asvs-authentication
findings:
  - "team cites V2.1.1 as the applicable ASVS requirement for password length"
  - "V2.1.1 states: 'Verify that user set passwords are at least 12 characters in length'"
  - "implementation: minimum password length enforced at 12 characters"
  - "note: V2.1.1 does not exist in ASVS 5.0.0; V2 was Authentication in ASVS 4.0.3; in ASVS 5.0.0 Authentication is V6"
pipeline_run_id: PR-4840
```

### B

```
arbiter_result:
  arbiter: ASVS-V6-Auth-Arbiter
  verdict: CONCERN
  requirements: ["V6 — authentication, password handling"]
  gap: "Requirement ID V2.1.1 does not exist in ASVS 5.0.0. This ID is from ASVS 4.0.3 (V2=Authentication in that version). In ASVS 5.0.0, authentication requirements are under V6. The closest equivalent is V6.1.x (password requirements)."
  action_recommended: "Resubmit with the correct ASVS 5.0.0 requirement ID. The password implementation (12-character minimum) appears to satisfy the intent of the requirement, but the submission uses an outdated version's ID. Confirm the applicable ASVS 5.0.0 requirement ID and resubmit."
  note: "CONCERN not REPROCESS: the implementation evidence (12-character minimum) is plausibly correct; the gap is the version mismatch in the requirement reference. This pipeline operates on ASVS 5.0.0 (released May 2025). Version 4.0.3 IDs are not valid in this pipeline's assessment scope."
```

### Reason

The team cited a requirement ID from ASVS 4.0.3. That ID does not exist in ASVS 5.0.0. This is a version confusion problem — ASVS 5.0.0 changed chapter numbering significantly (V2 Auth in 4.0.3 → V6 Auth in 5.0.0). The arbiter returns CONCERN (not PASS) because the requirement reference is invalid. It also does not return REPROCESS because the implementation evidence appears plausible — the team just cited the wrong version's ID. The note explains the version change context.

### Phrasing B

```
Team submits ASVS V2.1.1 (password ≥12 chars). Implementation: 12-char minimum enforced.
Note: V2.1.1 is an ASVS 4.0.3 ID; pipeline uses ASVS 5.0.0.
ASVS-V6-Auth-Arbiter, run PR-4840.
```

Expected: CONCERN. V2.1.1 does not exist in ASVS 5.0.0. Correct chapter is V6. Implementation appears plausible; gap is the incorrect version ID. Not REPROCESS (implementation not confirmed wrong); not PASS (reference is invalid).

---

## Pair CB4-3.4-125

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — SSDF PW.4 scan results submitted for wrong practice; team conflates PW.4 and PW.8)
**Source:** NIST SP 800-218 SSDF v1.1
**Verdict:** CONCERN

### A

```
arbiter: SSDF-PW-Arbiter
territory: T-ssdf-process
findings:
  - practice: PW.8
    gap_identified: "no peer review or code review process documented"
    evidence_submitted: "SonarQube SAST scan report from 2026-04-28"
    team_note: "our SAST tool reviews all code before merge — this covers PW.8 (review the software for security issues)"
pipeline_run_id: PR-4851
```

### B

```
arbiter_result:
  arbiter: SSDF-PW-Arbiter
  verdict: CONCERN
  practice: PW.8
  gap: "SAST scanning (PW.4 — automated analysis) was submitted as evidence for PW.8 (human review). PW.8 requires that software be reviewed by people, not only by automated tools. PW.4 and PW.8 are distinct practices: PW.4 covers automated checking (SAST, DAST, SCA); PW.8 covers human review (code review, security review, penetration testing)."
  action_recommended: "Document a human review process for security-relevant code changes (code review, security-focused peer review, or periodic manual review). The SonarQube SAST report satisfies PW.4 and is noted as such — submit it separately under PW.4 evidence."
  note: "CONCERN not REPROCESS: the team has automated security analysis in place (PW.4 satisfied by SAST). The gap is the absence of human review (PW.8). Automated tools do not substitute for human judgment on security design decisions."
```

### Reason

SSDF PW.4 and PW.8 are distinct practices: PW.4 = automated analysis (SAST, DAST, composition analysis); PW.8 = human review (code review, security-focused manual review). The team submitted PW.4 evidence (SAST scan) for a PW.8 gap. This is FM-SSDF-2 (PRACTICE_FAMILY_CONFUSION) at the sub-practice level. CONCERN (not REPROCESS) because SAST is a real security control — the team has partial coverage; the human review element is missing. The note acknowledges the SAST evidence as satisfying PW.4 (the arbiter should not ignore it).

### Phrasing B

```
SSDF-PW-Arbiter, PW.8 gap: no human code review documented.
Team submits SonarQube SAST report from 2026-04-28 as PW.8 evidence.
Run PR-4851. Does SAST evidence satisfy PW.8?
```

Expected: CONCERN. SAST = PW.4 (automated analysis), not PW.8 (human review). PW.8 gap remains. SAST report noted as satisfying PW.4. Not REPROCESS because automated security control is present; not PASS because human review element absent.

---

## Pair CB4-3.4-126

**Node:** 08 — Arbiter Units
**Stratum:** D (adversarial — team presents PASS-worded evidence for a finding where the gap has not been resolved)
**Source:** OWASP ASVS 5.0.0, CWE Top 25 2023
**Verdict:** REPROCESS

### A

```
arbiter: CWE-Authz-Arbiter
territory: T-cwe-authz
findings:
  - previous_finding: "IDOR on GET /api/files/{file_id} — CWE-862"
  - resubmission_evidence:
    - "authorization module updated (PR #2847 merged 2026-05-03)"
    - "PR description: 'adds file ownership check to the file retrieval endpoint'"
    - "no test output provided"
    - "no configuration showing the check is active in production"
    - "team note: 'the PR is merged; the fix is done'"
pipeline_run_id: PR-4862
```

### B

```
arbiter_result:
  arbiter: CWE-Authz-Arbiter
  verdict: REPROCESS
  cwe_id: CWE-862
  severity: HIGH
  gap: "A merged pull request (PR #2847) with a description claiming to add an ownership check does not confirm the check is implemented correctly or is active in production. Evidence required: test output showing the ownership check rejects unauthorized access, or production deployment confirmation."
  action_required: "Provide one or more of: (1) test output showing a request by a user who does not own file_id is rejected with 403; (2) production deployment confirmation log showing PR #2847 is deployed; (3) code review with the specific ownership check implementation visible."
  note: "PR merge confirms code was merged to the main branch; it does not confirm production deployment, correct implementation, or test coverage. A PR description is not a test result."
```

### Reason

A merged PR with a description that says it fixes the issue is not evidence that the issue is fixed. The arbiter must require implementation evidence: test output, production deployment confirmation, or code review showing the specific check. This tests the pipeline's ability to distinguish between "work was done" (PR merged) and "the control is in place" (verified implementation). REPROCESS continues until actual implementation evidence is provided. PR descriptions are developer intent, not verification.

### Phrasing B

```
Previous CWE-862 finding on /api/files/{id}.
Team resubmits: PR #2847 merged 2026-05-03 with description "adds file ownership check."
No test output, no production deployment log, no code shown.
CWE-Authz-Arbiter, run PR-4862. Verdict?
```

Expected: REPROCESS. PR merge ≠ implementation evidence. Required: test output showing 403 on unauthorized access, or production deployment confirmation, or code review of the specific check. PR description is not a test result.

---

## File summary

| Seq | Stratum | Source | Verdict | Adversarial scenario trained against |
|-----|---------|--------|---------|--------------------------------------|
| 117 | D | ASVS | REPROCESS | Requirement substitution: V9.2.1 evidence doesn't satisfy V9.1.1 gap |
| 118 | D | RFC+ASVS | RFC:PASS / ASVS:REPROCESS | RFC MAY vs ASVS MUST: exp optional in RFC 7519, mandatory in V9.2.1 |
| 119 | D | ASVS | CONCERN | Misrouting: session JWT → V7 (not V9); arbiter identifies routing error |
| 120 | D | SSDF | REPROCESS | Policy vs artefact: SAST policy ≠ SAST scan results (PW.4) |
| 121 | D | CWE | REPROCESS | CWE reclassification: absent check = CWE-862 (team argued CWE-269) |
| 122 | D | ASVS | REPROCESS | Risk reframing: risk statement does not convert MUST violation to CONCERN |
| 123 | D | RFC | PASS | RFC MAY evaluated correctly: no MUST max exp → RFC PASS (not CONCERN) |
| 124 | D | ASVS | CONCERN | Wrong ASVS version: V2.1.1 (4.0.3) does not exist in 5.0.0 |
| 125 | D | SSDF | CONCERN | Practice confusion: SAST (PW.4) submitted for human review gap (PW.8) |
| 126 | D | CWE | REPROCESS | PR description ≠ implementation evidence; requires test output or deploy log |

**Running total: 252 instances (sequences 001–126)**
Stratum breakdown: A ~26%, B ~28%, C ~27%, D ~19%
