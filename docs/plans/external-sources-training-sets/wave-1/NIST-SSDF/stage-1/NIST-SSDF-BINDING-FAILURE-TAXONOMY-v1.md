# NIST-SSDF — Binding Failure Taxonomy
# Stage 1, Document 2 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: NIST SP 800-218 SSDF v1.1 — PUBLIC_DOMAIN_US
# Produced: 2026-05-05

---

## Primary gap classes

Same three primary classes as the OWASP-ASVS taxonomy (structural, contract,
judgment) apply. This document defines the five failure modes specific to
SSDF-based security examination. See OWASP-ASVS-BINDING-FAILURE-TAXONOMY-v1.md
for the primary class definitions.

---

## Five named failure modes

### FM-SSDF-1: EVIDENCE_TYPE_CONFLATION

**Description:** The orchestrator accepts runtime test results (PW.8 evidence)
as proof of design-phase practices (PW.1, PW.2, PO.1, PO.4). This is the
most common SSDF failure in practice.

**Detection:** Node 8 output contains PASS for PW.2 when the input provides
only a penetration test report, security scan results, or ASVS runtime compliance
confirmation. PW.2 requires a design review record — a document that shows the
design was examined for security requirement compliance. Test results are PW.8
evidence and do not satisfy PW.2.

**Classification:** Judgment gap in Node 8.

**Training correction:** Pairs that show Node 8 receiving an input with only
runtime evidence for PW.2 and returning CONCERN or REPROCESS naming the
missing design review artifact.

---

### FM-SSDF-2: PRACTICE_FAMILY_CONFUSION

**Description:** The orchestrator assigns PW family evidence (build artifacts)
to PO family requirements (organizational preparation), or vice versa.

**Detection:** A security requirements document cited as PW.1 evidence instead
of PO.1 evidence, or a threat model cited as PW.8 evidence instead of PW.1
evidence. The practice family letter and number must both match the claimed
practice.

**Classification:** Contract gap in Node 4 or Node 8.

**Training correction:** Pairs that show each SSDF artifact type (requirements
doc, threat model, design review, code review, test report) mapped to the
correct practice ID.

---

### FM-SSDF-3: TIMING_GAP_IGNORED

**Description:** The orchestrator does not consider WHEN a practice was
performed relative to the development stage it was designed to affect.

**Detection:** Node 8 returns PASS for PW.2 when the input states the design
review was conducted after implementation. SSDF PW.2's value is in preventing
design defects — a post-implementation review cannot prevent defects already
built in. The review artifact exists (task letter satisfied) but the timing
gap is unaddressed (task intent not satisfied).

**Classification:** Judgment gap in Node 8.

**Present in scenarios:** S3.

**Training correction:** Pairs where Node 8 receives a PW.2 artifact with a
stated timeline (review conducted in week N of a project that began in week 1,
with implementation completing in week M where M < N) and correctly identifies
the timing gap as a CONCERN without escalating to REPROCESS.

---

### FM-SSDF-4: SSDF_ASVS_BOUNDARY_CONFUSION

**Description:** The orchestrator cannot distinguish SSDF process compliance
from ASVS runtime compliance. Both are required for sub-phase 3.4 but they
measure different things at different layers.

**Detection (type a):** Node 8 returns PASS for SSDF requirements when the
input contains only ASVS runtime compliance evidence (e.g., "our JWT validation
passes all ASVS 9.x checks" used as evidence for SSDF PW.1 or PW.2).

**Detection (type b):** Node 7 selects only the ASVS-V9-Token-Arbiter or
similar runtime arbiter when the input is about SSDF design process evidence.
The SSDF-PW-Arbiter must be selected for SSDF evidence.

**Classification:** Judgment gap in Node 8 (type a), contract gap in Node 7 (type b).

**Training correction (type a):** Pairs where Node 8 clearly maps ASVS
compliance evidence to ASVS practices and SSDF design evidence to SSDF
practices — and returns CONCERN when ASVS evidence is offered for an SSDF claim.

**Training correction (type b):** Pairs where Node 7 input contains SSDF
practice language (PW.2, PO.4, design review) and Node 7 output includes
SSDF-PW-Arbiter or SSDF-PO-Arbiter.

---

### FM-SSDF-5: ARTIFACT_NAME_REQUIRED

**Description:** The orchestrator accepts a practice claim without a named
artifact. SSDF practices require traceable evidence — the evidence must be
nameable (document title, test report ID, review record reference). A claim
without a named artifact is not evidence.

**Detection:** Node 8 returns PASS for PO.1 when the input says "security
requirements are documented" without naming the requirements document. The
phrase "security requirements are documented" is a claim. "TS-AUTHZ-001 contains
security requirements for the authorization model" is evidence.

**Classification:** Judgment gap in Node 8.

**Present in scenarios:** S2 (where all three failing practices have claims
but no named artifacts).

**Training correction:** Pairs where Node 8 receives a claim without an artifact
name and returns CONCERN requesting the artifact reference.

---

## Severity ordering

1. FM-SSDF-1 first (evidence type conflation is the most common and enables
   false PASS verdicts)
2. FM-SSDF-4 second (SSDF/ASVS boundary confusion prevents correct arbiter
   selection)
3. FM-SSDF-5 third (artifact name requirement ensures traceability)
4. FM-SSDF-2 fourth (practice family confusion is correctible with naming pairs)
5. FM-SSDF-3 last (timing gap is the most subtle and affects only edge cases)
