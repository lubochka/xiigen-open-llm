# OWASP-ASVS — Human Result Standards
# Stage 1, Document 4 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source version: OWASP ASVS 5.0.0 (May 2025)
# Produced: 2026-05-05

---

## Purpose

This document tells a human evaluator what to check in each Stage 2 output and
why. Every check is stated as a binary pass/fail with the exact text or
condition that constitutes a pass. "Looks correct" is not a check. Each check
names the ASVS requirement or oracle claim it maps to.

A human evaluator using this document to score the executor output does not
need to read the ASVS source or any other document in this set. The checks
here are complete and self-contained for scoring purposes.

---

## How to use this document

For each scenario, work through the checks in order. Mark each check as PASS
or FAIL. If a check fails, note the failure mode code from the Binding Failure
Taxonomy. Record the node where the failure appeared.

A scenario passes if all mandatory checks (marked M) pass. Advisory checks
(marked A) record quality information but do not determine the scenario pass/fail.

---

## Scenario S1 — JWT authentication chain, all controls satisfied

### S1 check list

**S1-M01** (mandatory): Node 8 final combined verdict must be PASS.
Any other verdict code (CONCERN, REPROCESS, ERROR) is a mandatory check failure.
Failure mode if CONCERN or REPROCESS: FM-3 (CLAIM_BOUNDARY_FAILURE) if the
system escalated an unambiguous positive to a concern. FM-1 (COVERAGE_ILLUSION)
if the system returned PASS but without naming individual ASVS IDs (see S1-M02).

**S1-M02** (mandatory): Node 8 output must name at least eight of the following
ten ASVS requirement IDs explicitly. Naming only the chapter number (e.g., "V6")
does not count. The section and item number must be present (e.g., "6.2.1").

Required IDs:
1. V6.2.1 (password minimum length)
2. V6.5.5 (OTP/TOTP lifetime)
3. V7.3.1 (session inactivity timeout)
4. V7.5.1 (re-authentication before credential changes)
5. V8.2.1 (function-level access restriction)
6. V8.2.2 (data-specific access restriction)
7. V9.1.1 (token signature validation)
8. V9.1.2 ('none' algorithm rejection)
9. V9.2.1 (token validity time span)
10. V9.2.3 (audience restriction)

If fewer than eight are named with section and item numbers: FAIL.
Failure mode: FM-1 (COVERAGE_ILLUSION).

**S1-M03** (mandatory): Node 9 formatter output must include the text "PASS"
or "PASS" as its verdict code in the first sentence or in a clearly labeled
verdict field. A formatter output that begins with a concern or warning without
first stating PASS fails this check.
Failure mode if absent: FM-5 (FORMATTER_PREMATURE_RESOLUTION) or structural gap.

**S1-M04** (mandatory): Node 9 output must not contain "REPROCESS" or
"CONCERN" as a primary verdict. Either of those appearing as the Node 9 verdict
is a mandatory check failure regardless of what Node 8 produced.
Failure mode: FM-5 (FORMATTER_PREMATURE_RESOLUTION) or judgment gap.

**S1-M05** (mandatory): The domain labels produced by Node 2 must include
G-security-verification for at least one territory. If G-security-verification
is absent from all Node 2 outputs: FAIL.
Failure mode: FM-2 (WRONG_SUB_PHASE_ASSIGNMENT).

**S1-A01** (advisory): Node 8 output includes a note about OTP and TOTP values
being at the maximum boundary (exactly at the allowed limit). This is advisory —
it improves output quality but does not affect the pass/fail verdict.

**S1-A02** (advisory): Node 9 output includes the risk analysis document
reference RA-AUTH-003 in the V7.3.1 confirmation. Advisory — confirms that
the specific evidence cited in the input was processed, not generalized.

**S1-A03** (advisory): Node 8 output names the arbiter roles that produced
the PASS verdicts (ASVS-V6-Auth-Arbiter, ASVS-V7-Session-Arbiter,
ASVS-V8-AuthZ-Arbiter, ASVS-V9-Token-Arbiter). Advisory — confirms arbiter
selection is transparent.

---

### S1 pass condition summary

All of S1-M01 through S1-M05 must pass. Failure on any mandatory check is
a scenario failure regardless of advisory results.

---

## Scenario S2 — OAuth 2.0 public client using 'plain' PKCE method

### S2 check list

**S2-M01** (mandatory): Node 8 final combined verdict must be REPROCESS.
Any other verdict code (PASS, CONCERN) is a mandatory check failure.
- If PASS: the orchestrator missed the ASVS 10.4.6 violation entirely.
  Failure mode: FM-3 (CLAIM_BOUNDARY_FAILURE — unverified claim accepted as
  verified enforcement finding). This is the most serious failure possible
  for S2.
- If CONCERN: the orchestrator downgraded a clear violation to a concern.
  ASVS 10.4.6 uses the word "rejects" — plain method usage is not a concern,
  it is a protocol violation. Failure mode: FM-3 (CLAIM_BOUNDARY_FAILURE —
  boundary between concern and block not correctly drawn).

**S2-M02** (mandatory): Node 8 output must name ASVS 10.4.6 explicitly with
section and item number. "ASVS V10" alone does not pass this check.
Failure mode if absent: FM-1 (COVERAGE_ILLUSION — the relevant ASVS ID was
not verified individually).

**S2-M03** (mandatory): Node 8 output must state the specific violation:
the 'plain' code_challenge_method is used and the authorization server accepts
it, where ASVS 10.4.6 requires the authorization server to reject 'plain'.
The output does not need to use these exact words, but the semantic content
must be present: (1) plain method used, (2) ASVS 10.4.6 requires rejection
of plain, (3) violation confirmed.
Failure mode if absent: structural gap in Node 8 output.

**S2-M04** (mandatory): Node 7 must have selected ASVS-V10-OAuth-Arbiter
(or an arbiter with equivalent OAuth/PKCE scope). If Node 7 output shows
only V6, V7, V8, or V9 arbiters, the PKCE check was never run.
Failure mode: FM-4 (ARBITER_SELECTION_GAP).

**S2-M05** (mandatory): Node 9 formatter output must begin with REPROCESS
or BLOCKED or an equivalent escalation signal. It must not produce resolved
or compliant language. Specifically: the words "compliant", "satisfies",
"passes", or "no action required" must not appear in the Node 9 output for S2.
Failure mode if absent: FM-5 (FORMATTER_PREMATURE_RESOLUTION).

**S2-M06** (mandatory): Node 9 output must include at least one specific
required action for remediation. "Fix the issue" is not an action. A passing
action is: "Configure the authorization server to reject code_challenge_method=plain"
or equivalent specificity.
Failure mode if absent: structural gap in Node 9.

**S2-A01** (advisory): Node 8 or Node 9 output references RFC 7636 Section
4.2 ("clients MUST use S256 when capable"). Advisory — confirms that the RFC
binding was applied alongside the ASVS requirement.

**S2-A02** (advisory): Node 8 output notes that the security review team's
previous assessment was incorrect. Advisory — confirms the orchestrator can
contradict a prior assessment when the evidence shows a violation.

**S2-A03** (advisory): Node 9 output names ASVS 10.4.4 as satisfied
(authorization code flow used, implicit and password grant types not present).
Advisory — confirms the orchestrator correctly separates satisfied requirements
from violated ones rather than treating all V10 requirements as failed.

---

### S2 pass condition summary

All of S2-M01 through S2-M06 must pass. S2 is the worst negative case — the
most dangerous failure mode is producing a PASS verdict for a clear PKCE
violation. A single mandatory check failure on S2-M01 is the highest severity
finding this experiment can produce.

---

## Scenario S3 — Session timeout ASVS-compliant, deviating from internal standard

### S3 check list

**S3-M01** (mandatory): Node 8 final combined verdict must be CONCERN.
Any other verdict code is a mandatory check failure.
- If REPROCESS: the orchestrator escalated an ASVS-compliant implementation
  to a reprocess because it could not distinguish ASVS compliance from internal
  standard conformance. Failure mode: FM-3 (CLAIM_BOUNDARY_FAILURE — type b,
  internal standard deviation treated as ASVS violation).
- If PASS: the orchestrator ignored the internal standard deviation entirely.
  This is less dangerous than REPROCESS but produces an incomplete result.
  Failure mode: structural gap in Node 8.

**S3-M02** (mandatory): Node 8 output must confirm that ASVS 7.3.1 is
satisfied. The confirmation must name ASVS 7.3.1 explicitly (section and item
number required). It must state that the 20-minute timeout with RA-AUTH-003
on file satisfies the requirement. A CONCERN verdict without this confirmation
fails this check — the output must distinguish between ASVS compliance
(confirmed) and internal standard deviation (flagged as concern).
Failure mode if absent: judgment gap in Node 8 (CONCERN issued but source not
correctly discriminated from violation).

**S3-M03** (mandatory): Node 8 output must identify the concern as an
internal standard deviation, not an ASVS violation. The output must make clear
that ESS-SESSION-001 deviation is the source of the concern, not ASVS 7.3.1.
Acceptable language: "organizational governance", "internal standard", "not an
ASVS violation", or equivalent.
Failure mode if absent: FM-3 (CLAIM_BOUNDARY_FAILURE — cannot distinguish
ASVS scope from organizational governance scope).

**S3-M04** (mandatory): Node 9 output must not contain REPROCESS as its
verdict. The formatter must propagate CONCERN with a clear statement that ASVS
compliance is confirmed.
Failure mode if REPROCESS appears: FM-5 (FORMATTER_PREMATURE_RESOLUTION) or
FM-3 (verdict escalation).

**S3-M05** (mandatory): Node 9 output must include at least one resolution
path for the internal standard deviation. A passing resolution path names
the action: either update the internal standard or align the implementation.
"Resolve the issue" is not a resolution path.
Failure mode if absent: structural gap in Node 9.

**S3-A01** (advisory): Node 8 or Node 9 output notes that a 20-minute timeout
is more restrictive than the 30-minute internal standard (shorter = more secure).
Advisory — confirms the orchestrator correctly evaluates the security direction
of the deviation.

**S3-A02** (advisory): Node 8 output notes that ASVS 7.3.2 (absolute session
lifetime) was not addressed in the input. Advisory — confirms the orchestrator
identifies incomplete coverage without blocking on it.

---

### S3 pass condition summary

All of S3-M01 through S3-M05 must pass. S3 is the borderline case — the most
common failure mode is escalating CONCERN to REPROCESS (FM-3 type b). This
failure mode is equally dangerous because it trains the orchestrator to block
ASVS-compliant implementations when an internal standard deviation exists.

---

## Aggregate scoring

| Scenario | Mandatory checks | Scenario passes if |
|----------|-----------------|-------------------|
| S1 | M01 through M05 | All 5 pass |
| S2 | M01 through M06 | All 6 pass |
| S3 | M01 through M05 | All 5 pass |

**Experiment passes if:** All three scenarios pass all mandatory checks.

**Experiment passes with gaps if:** All three scenarios pass S-M01 (verdict
code correct) but one or more advisory checks reveal quality gaps. The gaps
are catalogued and addressed in Stage 3 Action 2.

**Experiment fails if:** Any scenario fails its S-M01 verdict code check.
A wrong verdict code indicates the orchestrator has a fundamental judgment
failure that must be corrected before pair production begins.

---

## What the executor must record

For each scenario, the executor records the following in the Stage 2 evidence file:

1. The actual Node 8 verdict code (verbatim from output)
2. The actual ASVS IDs named in Node 8 output (list them)
3. The actual Node 9 verdict code or opening statement (verbatim first sentence)
4. For S2: the actual Node 7 arbiter selection list (verbatim)
5. For S2: whether the words "compliant" or "satisfies" appear in Node 9 output
6. For S3: whether Node 8 output contains both "ASVS 7.3.1" and language
   distinguishing ASVS compliance from internal standard

These six items are sufficient to score all mandatory checks for all three
scenarios. The executor records them verbatim — no paraphrasing.
