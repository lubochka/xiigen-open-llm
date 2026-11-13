# NIST-SSDF — Source Binding
# Stage 1, Document 1 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: NIST SP 800-218 Secure Software Development Framework v1.1
# License: US Government work — PUBLIC_DOMAIN_US
# Attribution: Recommended — "NIST SP 800-218 SSDF v1.1"
# Produced: 2026-05-05

---

## What this document is

This document binds the three NIST-SSDF experiment scenarios to the specific
SSDF practice families, practices, and tasks they exercise. NIST-SSDF is a
supplementary source to OWASP-ASVS in batch CB4 — it covers process-level
security compliance for the authorization model, while OWASP-ASVS covers
runtime control verification. The key training distinction this source provides:
SSDF compliance is about how the security model was built, not whether its
runtime controls are correct.

---

## Source summary

NIST SP 800-218 defines four practice families:

| Family | Name | Relevance to sub-phase 3.4 |
|--------|------|---------------------------|
| PO | Prepare the Organization | PO.1 (security requirements), PO.4 (security check criteria) |
| PW | Produce Well-Secured Software | PW.1 (design meets security requirements), PW.2 (design review), PW.7 (code review), PW.8 (security testing) |
| PS | Protect the Software | PS.1 (code access control), PS.2 (release integrity) |
| RV | Respond to Vulnerabilities | RV.1 (identify vulnerabilities), RV.2 (remediate) |

Primary practices for sub-phase 3.4 experiments:
- **PO.1**: Define security requirements for the authorization model
- **PW.1**: Design the authorization model to meet those requirements and mitigate identified risks
- **PW.2**: Review the authorization model design to verify compliance with security requirements
- **PW.8**: Test the authorization model executable code to identify vulnerabilities

---

## Requirement binding per scenario

### Scenario S1 — SSDF process evidence for authorization model (positive)

| SSDF Practice | Task | Relevance |
|--------------|------|-----------|
| PO.1 | PO.1.1: Document security requirements | Input claims requirements documented in TS-AUTHZ-001 |
| PO.4 | PO.4.1: Define criteria for security checks | Input claims acceptance criteria defined and linked to requirements |
| PW.1 | PW.1.1: Design meets requirements; risks mitigated | Input claims threat model (TM-AUTHZ-2026) documents design decisions |
| PW.2 | PW.2.1: Review design for security compliance | Input claims peer design review conducted and findings resolved |
| PW.8 | PW.8.1: Test for vulnerabilities | Input claims OWASP ZAP and manual penetration test results on file |

**S1 binding rationale:** All five practice-task pairs above are directly
evidenced in the input. The arbiter must verify each practice individually
— not by confirming that "security was done" (coverage illusion) but by
checking that each practice has a named artifact (requirement doc, threat
model, design review record, test result) as evidence.

---

### Scenario S2 — SSDF compliance claimed without process evidence (worst negative)

| SSDF Practice | Expected | Actual in input | Gap |
|--------------|---------|-----------------|-----|
| PW.1 | Design document with risk mitigations | "We designed it securely" (claim only) | No artifact — PW.1 not evidenced |
| PW.2 | Design review record with findings | Penetration test report | Penetration test is PW.8 evidence, not PW.2 evidence |
| PO.1 | Security requirements document | "Standard security requirements apply" | No named document — PO.1 not evidenced |

**S2 binding rationale:** The most consequential SSDF failure mode in practice:
teams submit runtime test results (PW.8) as evidence for process practices
(PW.1, PW.2) that require design-phase artifacts. A penetration test that passes
cannot retroactively prove that PW.2 design review was conducted. These are
distinct practices with distinct required artifacts. Conflating them is the
SSDF-specific COVERAGE_ILLUSION failure.

---

### Scenario S3 — Design review conducted post-implementation (borderline)

| SSDF Practice | Question |
|--------------|---------|
| PW.2 | Does a post-implementation design review satisfy PW.2, which expects the review to inform the design? |

**S3 binding rationale:** NIST SSDF PW.2 says "Review the Software Design to
Verify Compliance with Security Requirements." It does not explicitly require
the review to precede implementation. However, a review that cannot change the
design because code is already written has no preventive value — it can only
confirm or document, not correct. The orchestrator must distinguish between:
a review that satisfied the SSDF task letter (a review occurred) and a review
that satisfied the SSDF task intent (the review could have influenced the design).
The correct verdict is CONCERN — the review artifact exists (task technically
satisfied), but the timing creates a gap in SSDF intent.

---

## Key training distinction

SSDF provides PROCESS compliance evidence. ASVS provides RUNTIME control compliance
evidence. The orchestrator must not accept ASVS runtime evidence as SSDF process
evidence or vice versa. This is the primary training goal for NIST-SSDF pairs
in batch CB4.

Correct mapping:
- "We ran a penetration test and it passed" → PW.8 evidence only
- "We conducted a design review and documented findings" → PW.2 evidence only
- "We documented all authentication requirements in TS-AUTHZ-001" → PO.1 evidence only
- "Our JWT validation passes ASVS 9.1.1 checks" → ASVS evidence, not SSDF evidence

---

## Arbiter role names

| Arbiter role name | SSDF family | Triggered by |
|-------------------|-------------|-------------|
| `SSDF-PO-Arbiter` | PO | Authorization model requirement evidence |
| `SSDF-PW-Arbiter` | PW | Authorization model design and build evidence |
| `SSDF-RV-Arbiter` | RV | Vulnerability identification and remediation |

---

## Acceptance criteria

**AC-1:** For S1, Node 8 must emit PASS. The result must name at least four
of the five SSDF practice IDs (PO.1, PO.4, PW.1, PW.2, PW.8) with both
family and practice number. "SSDF requirements met" without practice IDs fails.

**AC-2:** For S2, Node 8 must emit REPROCESS or CONCERN with error identifying
that the penetration test result does not satisfy PW.2 (design review). A PASS
verdict for S2 is a mandatory failure regardless of other output.

**AC-3:** For S3, Node 8 must emit CONCERN (not REPROCESS). The concern must
acknowledge that the PW.2 artifact exists but flag the timing gap. A REPROCESS
verdict for S3 is a failure — the task was technically performed.

**AC-4:** For all scenarios, Node 2 domain labels must include G-security-verification.
Labels limited to C-process or C-compliance without G-security-verification
indicate wrong sub-phase routing (FM-2).
