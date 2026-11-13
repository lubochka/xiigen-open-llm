# NIST-SSDF — Expected Orchestration Trace
# Stage 1, Document 3 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: NIST SP 800-218 SSDF v1.1 — PUBLIC_DOMAIN_US
# Produced: 2026-05-05

---

## Scenario S1 — SSDF process evidence complete (positive)

### S1 Input (to Node 1)

"Our team has the following NIST SSDF v1.1 process artifacts on file for the
authorization model implementation:
- TS-AUTHZ-001: Security requirements document for the authorization model,
  including all ASVS V8 control requirements and acceptance criteria.
- TM-AUTHZ-2026: Threat model documenting all authorization design decisions,
  risk mitigations, and residual risks accepted.
- DR-AUTHZ-2026-03: Design review record, conducted 2026-03-10 (two weeks
  before implementation sprint), showing all design findings resolved.
- TR-AUTHZ-2026-05: OWASP ZAP automated scan and manual penetration test
  report dated 2026-05-01, zero critical or high findings.
Does this implementation satisfy NIST SSDF v1.1 PO and PW practices for the
security and authorization model?"

### S1 Expected Node 8 verdict

PASS. All four required practice families evidenced with named artifacts:
- PO.1: TS-AUTHZ-001 (security requirements document) — evidenced
- PW.1: TM-AUTHZ-2026 (threat model with design decisions) — evidenced
- PW.2: DR-AUTHZ-2026-03 (design review record, pre-implementation) — evidenced
- PW.8: TR-AUTHZ-2026-05 (penetration test report, zero critical findings) — evidenced

### S1 Expected Node 9 output

VERDICT: PASS

NIST SSDF v1.1 process compliance confirmed for sub-phase 3.4 (Security and
Authorization Model):

- PO.1 satisfied: Security requirements document TS-AUTHZ-001 on file, includes
  ASVS V8 control requirements and acceptance criteria.
- PW.1 satisfied: Threat model TM-AUTHZ-2026 documents design decisions, risk
  mitigations, and accepted residual risks.
- PW.2 satisfied: Design review DR-AUTHZ-2026-03 conducted 2026-03-10,
  two weeks before implementation — pre-implementation timing confirmed.
  All findings resolved.
- PW.8 satisfied: Penetration test TR-AUTHZ-2026-05 dated 2026-05-01, zero
  critical or high findings.

All SSDF practices have named, traceable artifacts. Process compliance confirmed.

---

## Scenario S2 — SSDF compliance claimed without process artifacts (worst negative)

### S2 Input (to Node 1)

"Our security team has completed a comprehensive security review of the
authorization model. We ran OWASP ZAP scans that found no critical issues.
Our implementation passes all ASVS V8 runtime checks. Standard security
requirements have been applied throughout development. We believe our
implementation satisfies NIST SSDF compliance requirements for the authorization
model. Please confirm."

### S2 Expected Node 8 verdict

REPROCESS. SSDF practices require traceable design-phase artifacts. Runtime
test results and ASVS compliance confirm PW.8 (security testing) only. The
following required practices have no named artifacts in the input:

- PO.1: "Standard security requirements applied" is a claim, not evidence.
  Required: a named requirements document (e.g., "TS-AUTHZ-001 contains
  security requirements"). No document named.
- PW.1: No threat model or design decision document referenced.
  Required: a named artifact showing the design was evaluated against security
  requirements with risk mitigations documented.
- PW.2: OWASP ZAP scan and ASVS runtime checks are PW.8 evidence (security
  testing), not PW.2 evidence (design review). PW.2 requires a review of the
  design — a document showing the authorization model design was examined for
  security requirement compliance before or during implementation. Runtime
  testing cannot substitute for design review.

Error code: SSDF_DESIGN_EVIDENCE_MISSING. Three practices (PO.1, PW.1, PW.2)
lack traceable artifacts. PW.8 alone is insufficient for SSDF compliance.

### S2 Expected Node 9 output

VERDICT: REPROCESS — SSDF DESIGN EVIDENCE MISSING

NIST SSDF v1.1 compliance cannot be confirmed. The following required process
artifacts are absent:

Missing: PO.1 security requirements document. "Standard security requirements
applied" is a claim — not evidence. Required: a named document (e.g.,
TS-AUTHZ-001) containing the specific security requirements for the authorization
model.

Missing: PW.1 threat model or design decision record. Required: a named artifact
showing the authorization model design was evaluated against security requirements
and residual risks were documented.

Missing: PW.2 design review record. Note: the OWASP ZAP scan result and ASVS
compliance confirmation are PW.8 (security testing) evidence — they confirm
that the implemented code has no known vulnerabilities. They do NOT confirm
that the design was reviewed for security requirement compliance. PW.2 requires
a separate design-phase review artifact.

PW.8 is satisfied: runtime testing confirmed, zero critical findings.

Required actions: produce the three missing artifacts (requirements doc, threat
model, design review record) and resubmit.

---

## Scenario S3 — Design review conducted post-implementation (borderline)

### S3 Input (to Node 1)

"Our team conducted a design review of the authorization model as part of our
SSDF compliance effort. The review was conducted in week 8 of the project.
Implementation was completed in week 6. The review found two minor design
concerns that we documented but did not implement fixes for, since the code
was already shipped. The reviewer noted the concerns for the next iteration.
The review document (DR-AUTHZ-2026-04) is on file. Does this satisfy NIST SSDF
PW.2 for the authorization model?"

### S3 Expected Node 8 verdict

CONCERN. The PW.2 artifact exists (DR-AUTHZ-2026-04 on file, review completed,
findings documented). SSDF PW.2's task letter is satisfied — a design review
was conducted and recorded.

However, a concern is raised: SSDF PW.2's purpose is to review the design to
verify compliance and produce findings that can be actioned. A review conducted
after implementation (week 8, implementation complete by week 6) cannot prevent
design defects that were already built into the shipped code. Two findings were
documented but not addressed because the code was already shipped. This
indicates the review identified design security gaps that remain unresolved in
the current implementation.

The concern is two-layered:
1. Timing gap: the review could not influence the design (post-implementation)
2. Open findings: two design concerns documented but not resolved

This is CONCERN, not REPROCESS. The artifact exists. The practice was
technically performed. The timing and open findings are governance concerns.

### S3 Expected Node 9 output

VERDICT: CONCERN — PW.2 TIMING GAP AND OPEN FINDINGS

NIST SSDF v1.1 PW.2 status: CONCERN (not a blocking violation)

Confirmed: Design review DR-AUTHZ-2026-04 on file. Review conducted, findings
documented. PW.2 artifact requirement satisfied.

Concerns:
1. Post-implementation timing: review conducted in week 8, after implementation
   completed in week 6. SSDF PW.2 is designed to be a preventive practice —
   a review that cannot change the design has reduced preventive value.
2. Two open findings: design concerns documented but not resolved (deferred to
   next iteration). The authorization model as shipped contains two known design
   security concerns without mitigations in place.

Recommended actions:
(a) Address the two open design findings in the next development iteration.
    Update DR-AUTHZ-2026-04 with resolution status when complete.
(b) In future projects, schedule the PW.2 design review before implementation
    sprint begins to maximize its preventive value.

This item does not require reprocessing. SSDF PW.2 compliance is technically
met. The timing gap and open findings are forwarded as governance concerns.
