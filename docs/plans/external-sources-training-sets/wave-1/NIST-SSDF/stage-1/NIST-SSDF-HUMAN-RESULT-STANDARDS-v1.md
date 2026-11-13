# NIST-SSDF — Human Result Standards
# Stage 1, Document 4 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source: NIST SP 800-218 SSDF v1.1 — PUBLIC_DOMAIN_US
# Produced: 2026-05-05

---

## Scenario S1 — SSDF process evidence complete

**S1-M01** (mandatory): Node 8 final verdict must be PASS. Any other verdict
is a mandatory failure.
Failure mode: FM-SSDF-1 (evidence type conflation) if the input provides
design artifacts but the arbiter still returns CONCERN or REPROCESS.

**S1-M02** (mandatory): Node 8 output must name at least three of the four
SSDF practice IDs (PO.1, PW.1, PW.2, PW.8) explicitly with family letter
and practice number. "SSDF compliance met" without IDs is coverage illusion.
Failure mode: FM-SSDF-5 (artifact name required).

**S1-M03** (mandatory): Node 9 output must begin with PASS or equivalent
passing verdict. The formatter must not convert a PASS to a CONCERN.

**S1-A01** (advisory): Node 8 output notes that DR-AUTHZ-2026-03 was conducted
before implementation (timing confirmed). This is advisory but important for
future S3 comparison.

**S1 passes if:** S1-M01, S1-M02, S1-M03 all pass.

---

## Scenario S2 — SSDF compliance claimed without process artifacts

**S2-M01** (mandatory): Node 8 final verdict must be REPROCESS or CONCERN.
PASS is a mandatory failure regardless of other output.
Most serious failure: PASS verdict where PO.1, PW.1, PW.2 are all unconfirmed.
Failure mode: FM-SSDF-1 (evidence type conflation — ASVS/ZAP results accepted
as SSDF evidence).

**S2-M02** (mandatory): Node 8 output must identify at least one of PO.1,
PW.1, or PW.2 as missing evidence. The output must name the specific practice
and what artifact type is required (not just "evidence missing").
Failure mode if absent: structural gap in Node 8.

**S2-M03** (mandatory): Node 8 output must state that OWASP ZAP results or
ASVS compliance alone do not satisfy PW.2. This is the core SSDF training
signal — runtime testing evidence ≠ design review evidence.
Failure mode if absent: FM-SSDF-1 (evidence type conflation).

**S2-M04** (mandatory): Node 9 output must not contain the words "confirmed"
or "satisfied" as a primary verdict for SSDF compliance. It must propagate the
REPROCESS/CONCERN state.
Failure mode: FM-SSDF-4 (SSDF/ASVS boundary confusion in formatter).

**S2-A01** (advisory): Node 8 correctly identifies PW.8 as satisfied (the ZAP
scan and ASVS checks do evidence PW.8 security testing). Advisory — confirms
the orchestrator separates what IS evidenced from what is NOT evidenced.

**S2 passes if:** S2-M01 through S2-M04 all pass.

---

## Scenario S3 — Post-implementation design review

**S3-M01** (mandatory): Node 8 final verdict must be CONCERN. PASS or REPROCESS
are both mandatory failures.
- If PASS: FM-SSDF-3 (timing gap ignored) — review accepted without noting
  post-implementation timing.
- If REPROCESS: FM-SSDF-3 (timing gap over-escalated) — post-implementation
  review escalated to blocking when the artifact technically satisfies PW.2.

**S3-M02** (mandatory): Node 8 output must acknowledge that the PW.2 artifact
exists (DR-AUTHZ-2026-04 on file, review conducted, findings documented).
The concern must not deny that PW.2 was performed.
Failure mode if absent: judgment gap — the orchestrator blocks on timing without
acknowledging the artifact.

**S3-M03** (mandatory): Node 8 output must identify the timing gap (review
week 8, implementation completed week 6) as the source of the concern.
The concern must name the timing, not just say "design review timing suboptimal."
Failure mode if absent: FM-SSDF-3 (timing gap present but not diagnosed).

**S3-M04** (mandatory): Node 8 output must identify the two open findings as
a second concern. The open findings are a separate concern from the timing gap
— both must appear.
Failure mode if absent: structural gap in Node 8.

**S3-M05** (mandatory): Node 9 output must not contain REPROCESS as verdict.
CONCERN must be propagated with resolution paths.

**S3 passes if:** S3-M01 through S3-M05 all pass.

---

## Aggregate scoring

| Scenario | Mandatory checks | Passes if |
|----------|-----------------|-----------|
| S1 | M01–M03 | All 3 pass |
| S2 | M01–M04 | All 4 pass |
| S3 | M01–M05 | All 5 pass |

The most critical check across all three scenarios is S2-M03 — the statement
that runtime test results (ZAP, ASVS) do not satisfy the PW.2 design review
requirement. This is the primary training distinction that NIST-SSDF pairs
contribute to batch CB4.
