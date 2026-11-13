# OWASP-ASVS — Binding Failure Taxonomy
# Stage 1, Document 2 of 5
# Wave 1 — Sub-phase 3.4 (Security and Authorization Model)
# Batch: CB4 — Pair ID prefix: CB4-3.4-
# Source version: OWASP ASVS 5.0.0 (May 2025)
# Produced: 2026-05-05

---

## Purpose

This taxonomy classifies every gap that the executor may find between the oracle
expected trace and the actual orchestrator output. Each gap type has a name,
a definition, a detection test, and a required corrective action.

The taxonomy has three primary gap classes (structural, contract, judgment) and
five named failure modes specific to security-verification examination.

---

## Primary gap classes

### Structural gap

**Definition:** Content that the oracle requires to be present in the output is
entirely absent.

**Examples:**
- Node 8 emits a verdict with no ASVS requirement IDs named
- Node 9 produces a result string without any verdict code (PASS / CONCERN / REPROCESS)
- Node 3 produces a scheme plan with no steps

**Detection test:** Search the actual output for the required element. If it
cannot be found, the gap is structural.

**Corrective action (Stage 3):** Action 2 gap closure pass — write new pairs that
train the missing content into the specific node and role.

---

### Contract gap

**Definition:** Content is present in the output but is assigned to the wrong
node in the pipeline.

**Examples:**
- The ASVS 10.4.6 PKCE violation finding appears in Node 3 (scheme plan) instead of Node 8 (arbiter verdict)
- The final verdict appears in Node 6 (result translator) before Node 8 has run
- ASVS requirement IDs appear in Node 1 output (domain splitter) rather than being derived from the domain processor

**Detection test:** Identify which node produced the element. Compare to the
expected node for that element per the Expected Orchestration Trace. If the
node numbers differ, the gap is a contract gap.

**Corrective action:** Action 2 gap closure pass — write pairs that anchor the
content to the correct node. The pairs must show the wrong node receiving
the element as input and passing it through unchanged, while the correct node
produces it from scratch.

---

### Judgment gap

**Definition:** Content is present in the correct node but its meaning is
semantically wrong. The structure matches but the conclusion does not.

**Examples:**
- Node 8 emits CONCERN for S2 when REPROCESS is required (ASVS 10.4.6 plain method)
- Node 8 emits REPROCESS for S3 when CONCERN is required (ASVS 7.3.1 satisfied, internal standard deviation only)
- Node 8 emits PASS for S1 without naming individual ASVS requirement IDs (coverage illusion)
- Node 9 names the correct verdict code but the ASVS IDs cited are from the wrong chapter

**Detection test:** Verify the verdict code against the acceptance criteria.
Verify that every ASVS ID cited appears in the Source Binding for that scenario.
Verify that required ASVS IDs from the Source Binding appear in the output.
A verdict code mismatch is always a judgment gap.

**Corrective action:** Action 2 gap closure pass for the specific node and role.
If the judgment gap is in Node 8, write pairs that show the correct distinction
(e.g., ASVS violation vs internal standard deviation) using the exact scenario
context. Do not generalize — the pair must be specific enough that the arbiter
learns the distinction at the scenario level.

---

## Five named failure modes

### FM-1: COVERAGE_ILLUSION

**Description:** The orchestrator assigns a PASS verdict based on domain label
match without verifying individual ASVS requirement IDs.

**Identification:** Node 8 output contains PASS with only a chapter reference
(e.g., "V6 authentication requirements met") but no section and item number
references (e.g., "V6.2.1 satisfied: password minimum 15 chars confirmed").

**Present in scenarios:** S1 (the coverage illusion failure mode for the positive case)

**Classification:** Judgment gap in Node 8.

**Why dangerous:** A coverage illusion allows a non-compliant implementation to
receive a PASS verdict because the broad topic label matches. An implementation
that claims "password policy enforced" without specifying the minimum length
could satisfy V6.2.1 (8 chars) or fail it (7 chars) — the label alone cannot
distinguish these.

**Training correction:** Pairs that show Node 8 receiving a domain-processor
result with individual ASVS IDs present and emitting a verdict that names each
ID. Pairs where only a chapter label is present and the arbiter returns a
CONCERN asking for specific requirement evidence.

---

### FM-2: WRONG_SUB_PHASE_ASSIGNMENT

**Description:** Security controls are assigned to sub-phase 3.3 (API Contract)
instead of sub-phase 3.4 (Security and Authorization Model), or vice versa.

**Identification:** Node 1 or Node 2 output includes labels like `C-api-contract`
or `C-api-schema` for inputs that concern authentication or authorization
enforcement, rather than `G-security-verification` or `G-auth-model`.

**Present in scenarios:** Can appear in any of the three scenarios if Node 1
misclassifies the input domain.

**Classification:** Contract gap (content in wrong node assignment).

**Why dangerous:** Sub-phase 3.3 deals with API contract shape (endpoint
definitions, request/response schemas). Sub-phase 3.4 deals with who is
allowed to call those endpoints and under what conditions. Assigning ASVS
V8 authorization requirements to sub-phase 3.3 causes them to be trained
into the API contract model, not the security model. The API contract node
does not know how to enforce authorization.

**Training correction:** Pairs where the input explicitly names authentication
or authorization enforcement and Node 1 correctly produces `G-security-verification`
domain territories rather than API shape territories.

---

### FM-3: CLAIM_BOUNDARY_FAILURE

**Description:** The orchestrator cannot distinguish between:
(a) an unverified compliance claim (assertion without evidence) and a verified
    enforcement finding (evidence confirmed), or
(b) an ASVS violation (non-compliance with a normative requirement) and an
    internal-standard deviation (non-conformance with an organizational policy
    that does not correspond to an ASVS requirement).

**Identification (type a):** Node 8 emits PASS for a scenario where the input
claims compliance but provides no evidence. For example, input says "our system
is ASVS 10.4.6 compliant" without specifying how — Node 8 should return CONCERN
requesting evidence, not PASS accepting the claim.

**Identification (type b):** Node 8 emits REPROCESS for S3 because it detects
a deviation from the 30-minute internal standard, without checking whether
ASVS 7.3.1 is satisfied. The 20-minute timeout satisfies ASVS 7.3.1 — the
deviation is organizational, not ASVS.

**Present in scenarios:** S2 (type a — PKCE compliance claimed but plain method
used), S3 (type b — ASVS satisfied but internal standard deviation).

**Classification:** Judgment gap in Node 8.

**Why dangerous:** Type (a) failures allow unverified claims to pass the security
model without evidence. Type (b) failures cause the orchestrator to block
implementations that are actually ASVS-compliant, creating false positives that
erode trust in the security model and cause teams to bypass the orchestrator.

**Training correction (type a):** Pairs where Node 8 receives an input that
contains a claim without evidence and returns CONCERN with the specific evidence
it requires.

**Training correction (type b):** Pairs that show both the ASVS requirement
text and the internal standard side by side, and Node 8 correctly identifies
which verdict each maps to. The pair must show the ASVS check passing and the
internal standard deviation being flagged as CONCERN separately.

---

### FM-4: ARBITER_SELECTION_GAP

**Description:** Node 7 selects an arbiter that does not cover the most
important ASVS chapter for the input. The arbiter runs, produces a verdict,
but the verdict misses the critical compliance check because the relevant
arbiter was never selected.

**Identification:** Compare the arbiter role names in the Node 7 output against
the expected arbiter roles from the Source Binding for that scenario. If a
required arbiter (e.g., ASVS-V10-OAuth-Arbiter for S2) is absent from Node 7
output, this is an arbiter selection gap.

**Present in scenarios:** Can appear in S2 if Node 7 selects ASVS-V6 or ASVS-V9
arbiters but not ASVS-V10-OAuth-Arbiter for an OAuth/PKCE compliance input.

**Classification:** Contract gap in Node 7.

**Training correction:** Pairs where Node 7 input contains OAuth/PKCE terminology
and Node 7 output includes ASVS-V10-OAuth-Arbiter in the selected set.

---

### FM-5: FORMATTER_PREMATURE_RESOLUTION

**Description:** Node 9 (formatter) formats a final result that appears
resolved when Node 8 returned REPROCESS. The formatter should propagate
the REPROCESS state — it must not resolve it.

**Identification:** Node 8 output contains REPROCESS but Node 9 output contains
a human-readable result that describes the system as compliant or as having
"addressed" the issue. The REPROCESS state is lost at the formatter boundary.

**Present in scenarios:** S2 (the most dangerous failure for a REPROCESS case).

**Classification:** Contract gap in Node 9.

**Why dangerous:** A REPROCESS verdict is an instruction to the orchestrator to
send the work back for revision. If the formatter converts it to a resolved
output, the reprocess instruction is silently dropped. The team receives a
"resolved" output when the security control is missing.

**Training correction:** Pairs where Node 9 input contains REPROCESS and Node 9
output explicitly propagates REPROCESS with the ASVS violation named and an
action item for remediation. Node 9 MUST NOT write resolution language for
a REPROCESS input.

---

## Gap classification reference card

| FM code | Primary class | Node(s) affected | Scenario(s) |
|---------|--------------|------------------|-------------|
| FM-1 COVERAGE_ILLUSION | Judgment | Node 8 | S1 |
| FM-2 WRONG_SUB_PHASE_ASSIGNMENT | Contract | Node 1, Node 2 | S1, S2, S3 |
| FM-3 CLAIM_BOUNDARY_FAILURE | Judgment | Node 8 | S2, S3 |
| FM-4 ARBITER_SELECTION_GAP | Contract | Node 7 | S2 |
| FM-5 FORMATTER_PREMATURE_RESOLUTION | Contract | Node 9 | S2 |

---

## Severity ordering for gap closure prioritization

Stage 3 gap closure pairs are written in this order when multiple gaps are found:

1. FM-5 first: a premature resolution in Node 9 corrupts every downstream use of
   the result. It must be corrected before other gaps can be evaluated.
2. FM-3 second: a claim boundary failure in Node 8 produces systematically wrong
   verdicts. It is the deepest judgment gap.
3. FM-4 third: an arbiter selection gap in Node 7 prevents the critical arbiter
   from running at all. No Node 8 correction can fix a gap where Node 8 was never
   invoked.
4. FM-1 fourth: a coverage illusion produces false positive PASS verdicts. It is
   less immediately dangerous than FM-3 but affects every positive case.
5. FM-2 last: a wrong sub-phase assignment misroutes content but does not
   produce a wrong verdict for the current session — it affects future routing.
