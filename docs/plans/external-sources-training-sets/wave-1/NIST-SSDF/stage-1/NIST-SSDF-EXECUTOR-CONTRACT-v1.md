# NIST-SSDF — Executor Contract
# Stage 1, Document 5 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: NIST SP 800-218 SSDF v1.1 — PUBLIC_DOMAIN_US
# Produced: 2026-05-05

---

## Section 1 — Pre-run checklist

Same as OWASP-ASVS Executor Contract Section 1. Baseline ≥ 1153 required
before any scenario runs.

---

## Section 2 — Run protocol

Same as OWASP-ASVS Executor Contract Section 2. Record verbatim output for
all nine nodes per scenario.

---

## Section 3 — Scenario inputs (verbatim)

### S1 Input

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

### S2 Input

"Our security team has completed a comprehensive security review of the
authorization model. We ran OWASP ZAP scans that found no critical issues.
Our implementation passes all ASVS V8 runtime checks. Standard security
requirements have been applied throughout development. We believe our
implementation satisfies NIST SSDF compliance requirements for the authorization
model. Please confirm."

### S3 Input

"Our team conducted a design review of the authorization model as part of our
SSDF compliance effort. The review was conducted in week 8 of the project.
Implementation was completed in week 6. The review found two minor design
concerns that we documented but did not implement fixes for, since the code
was already shipped. The reviewer noted the concerns for the next iteration.
The review document (DR-AUTHZ-2026-04) is on file. Does this satisfy NIST SSDF
PW.2 for the authorization model?"

---

## Section 4 — What the executor must not do

Same constraints as OWASP-ASVS Executor Contract Section 4. Additionally:

**Must not substitute ASVS verdicts for SSDF verdicts.** If the pipeline uses
ASVS compliance evidence to resolve an SSDF practice claim, record this as a
FM-SSDF-4 gap (SSDF/ASVS boundary confusion).

**Must not accept "we followed secure practices" as PW.2 evidence.** The design
review artifact must be specifically named and the review must be a design review,
not a security scan, code review, or runtime test.

---

## Section 5 — Evidence file location

`wave-1/NIST-SSDF/stage-2/NIST-SSDF-EXPERIMENT-EVIDENCE-v1.md`

Six sections, same structure as OWASP-ASVS evidence file.

---

## Section 6 — Source authority

| SSDF Practice | Task | Authority for this experiment |
|--------------|------|------------------------------|
| PO.1 | PO.1.1 | NIST SP 800-218, Practice PO.1: Define Security Requirements |
| PW.1 | PW.1.1 | NIST SP 800-218, Practice PW.1: Design Software to Meet Requirements |
| PW.2 | PW.2.1 | NIST SP 800-218, Practice PW.2: Review the Software Design |
| PW.8 | PW.8.1 | NIST SP 800-218, Practice PW.8: Test Executable Code |

---

## Section 7 — Pair production for Stage 3

**Pair ID format:** CB4-3.4-[SEQ] (continuing from OWASP-ASVS pairs)
**Source citation:** NIST SP 800-218 SSDF v1.1
**License:** US Government work — no copyright restriction
**Attribution text (recommended):** "NIST SP 800-218 Secure Software Development Framework v1.1"

**Primary training goal:** Node 8 arbiter ability to distinguish SSDF process
evidence (design artifacts) from ASVS runtime evidence (test results). This is
the key training contribution of NIST-SSDF to batch CB4.

**Node coverage priorities:**
1. Node 8 — SSDF-PW-Arbiter: S2 REPROCESS case (evidence type conflation)
2. Node 8 — SSDF-PW-Arbiter: S3 CONCERN case (timing gap)
3. Node 8 — SSDF-PO-Arbiter: S1 PASS case (requirements document evidence)
4. Node 7 — Arbiter selector: SSDF arbiter selection when process evidence vocabulary present

Node 8 pass case (S1) must be written before CONCERN or REPROCESS cases per
the batch CB4 Node 8 pass-case-first rule.
