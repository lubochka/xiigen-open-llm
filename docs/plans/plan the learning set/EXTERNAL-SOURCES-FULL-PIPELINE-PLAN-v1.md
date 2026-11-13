# External Sources — Full Pipeline Plan
# From source selection through experiment, dataset compilation, and orchestrator learning tests
# Version: 1.0 — 2026-05-04
# Inputs: EXTERNAL-SOURCE-REGISTRY-v1.0, EXAMINATION-METHOD-FINAL,
#         HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT-v1,
#         HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT-v1,
#         EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1,
#         DEVELOPMENT-PHASES-AND-DOCUMENT-IDENTIFICATION-GUIDE-v1,
#         MULTI-PHASE-ORCHESTRATION-DESIGN-AND-EXPERIMENT-PLAN-v1,
#         ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1.0,
#         EXAMINATION-CONTINUATION-PLAN-v1.0

---

## What this plan governs

This plan covers the complete pipeline from an external source document in the
registry through to a verified dataset that the orchestrator can learn from and
be tested against. It replaces scattered guidance across multiple documents with
one authoritative sequence that names every step, every gate, every failure
pattern, every expected transition, and every test of the orchestrator's ability
to learn and adapt.

The pipeline has seven stages. Each stage has a defined input, a defined output,
a gate that must pass before the next stage begins, and a set of anti-patterns
that make the stage silently fail while appearing to succeed.

---

## Stage 0 — Source selection and license gate

### Input
The EXTERNAL-SOURCE-REGISTRY-v1.0 TRAINING_OK section.

### What happens
Before any document is examined, its source id must appear in Section 1 of the
registry with `licenseUseClass` of `TRAINING_OK`, `ATTRIBUTION_REQUIRED`,
`ATTRIBUTION_SHAREALIKE`, `PUBLIC_DOMAIN_US`, `FREE_COMMERCIAL_USE_WITH_NOTICE`,
or `IETF_TRUST_PROVISIONS`. Sources in `VERIFY_BEFORE_USE` require a full
license read before proceeding. Sources in `REMOVED` and the permanently
excluded categories are never entered regardless of content quality.

The priority order from the registry governs which sources are processed first:
1. Security and governance rows — OWASP-ASVS, OWASP-CHEAT-SHEETS, NIST-SSDF, CWE-TOP25, IETF-RFCS
2. Architecture and orchestration rows — KUBERNETES-KEPS, OPENTELEMETRY-SPEC, OPENAPI-SPEC, GOOGLE-AIP, MICROSOFT-LEARN-ARCH
3. Bug-fix and code-agent rows — SWE-BENCH, then DEFECTS4J/BUGSINPY/QUIXBUGS after license verification
4. Proposal and debate archive rows — RUST-RFCS, PYTHON-PEPS, GO-PROPOSALS, KAFKA-KIPS, CLOUDEVENTS-SPEC
5. Open-source architecture narrative rows — AOSA-VOL1, AOSA-VOL2, DOTNET-MICROSERVICES, SYSTEM-DESIGN-PRIMER
6. Stack-specific pattern rows — NESTJS-ADVANCED, REACT-ADVANCED, ANGULAR-STYLE, DOTNET-ASPNET-PERF

### Phase classification gate
Before examining a document, it must be assigned to a development phase and
sub-phase using the two-question method from the Development Phases Guide.

Question 1: What does this document define, decide, or constrain?
Question 2: Which phase of the development lifecycle produces this kind of output?

If the assignment is ambiguous after applying these two questions and the four
classification rules from the guide, the document is held and its ambiguity is
recorded. It is not examined until the ambiguity is resolved. Examining an
ambiguously classified document produces pairs that train the wrong node
behaviors because the node assignments inherit the classification error.

The KUBERNETES-KEPS documents, for example, classify primarily to sub-phases 3.5
(Infrastructure and deployment topology) and 4.1 (Backend services) — they
define production-readiness gates, rollout strategies, and migration procedures.
OWASP-ASVS classifies to sub-phase 3.4 (Security and authorization model).
SWE-BENCH instances classify individually — each issue-to-patch pair must be
classified to the sub-phase it belongs to before its node pairs can be written.

### Output
A confirmed source entry with: source id, license class, attribution requirement,
phase and sub-phase assignment, and the domain labels it contributes to (drawn
from the registry's `targetDomains` field).

### Gate
License is in TRAINING_OK class. Phase assignment is unambiguous. Attribution
annotation format is confirmed. Only then does Stage 1 begin.

### Anti-patterns
- Classifying by title alone without reading the content. A document titled
  "API Security Guidelines" might classify to sub-phase 3.4, 3.3, or 6.3
  depending on whether it defines authorization models, API contracts, or QA
  compliance checks.
- Treating `VERIFY_BEFORE_USE` as `TRAINING_OK` because the content looks useful.
  The license check is non-negotiable regardless of content quality.
- Skipping the phase classification and writing pairs without a node assignment.
  Node assignment is derived from phase classification. Unclassified source
  documents produce unassigned pairs that cannot be promoted.

---

## Stage 1 — Source document translation to experiment design

### Input
A confirmed source entry from Stage 0 with phase and sub-phase assignment.

### What happens
The six-step method from HOW-TO-TRANSLATE-A-DOCUMENT-INTO-AN-EXPERIMENT
is applied to the source document. The steps are:

**Step 1 — State the central question.** One question only, in the exact form:
"Given [source document] and [the domain it governs], does the document
correctly [claim A], [claim B], and [claim C] — or does it [failure mode 1],
[failure mode 2], and [failure mode 3]?"

Both the positive half and the negative half must be present. A question
without the negative half is not a question — it is a restatement of the
document's purpose.

**Step 2 — Read the document as a set of claims** and classify each claim by
type: enforcement, binding, coverage, sequencing, or honesty.

**Step 3 — Find the real failure modes.** For each claim, ask: what would have
to be true for this claim to appear satisfied while actually failing? The EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED document shows this in action: CB1-03-001's
parallel-step cross-reference contradiction is a failure mode where the Reason
field says "independent" but the transformational prompt body says "after Step 4."
The claim appears satisfied at the structure level while failing at the content level.

**Step 4 — Select the three scenarios.** One scenario from the positive half
(the document does what it claims), one from the worst negative case (the most
consequential failure mode), and one from the borderline case (the failure mode
that most resembles a pass). The borderline case is the most important for
training — it is the case that the orchestrator will most often get wrong.

**Step 5 — Write the oracle.** Before any executor session runs, the oracle
documents are produced: source binding (which specific sections of the document
each scenario exercises), expected trace (what the pipeline should produce for
each scenario), human result standards (what a human evaluating the result must
check and why), executor contract (what the executor must do and what it must
not do), failure taxonomy (how to classify each gap found).

**Step 6 — State the acceptance criteria.** The criteria must be measurable
without manual interpretation. "The orchestrator handles this correctly" is
not measurable. "Node 3 produces a plan with at least three steps, at least
one wait state, and at least one recovery path named by its triggering condition"
is measurable.

### Expected transitions between stages 1 and 2

A well-formed experiment design from Stage 1 tells Stage 2 exactly what to
verify. The oracle expected trace defines the B output that the trained pipeline
must produce. The acceptance criteria define the measurable properties of that
B. The failure taxonomy defines how to classify any gap between the oracle and
the actual output. Stage 2 cannot begin without all five oracle documents
present and the acceptance criteria stated at the measurement level.

### Output
Five oracle documents: source binding, expected trace, human result standards,
executor contract, failure taxonomy. Plus acceptance criteria stated in
measurable form.

### Gate
All five oracle documents produced. Acceptance criteria measurable. Failure
taxonomy has at least three distinct gap classifications. Central question has
both positive and negative halves.

### Anti-patterns

**Anti-pattern 1 — The oracle describes instead of shows.**
The expected trace says "Node 3 should produce a scheme plan." That is a
description. The oracle must show the actual plan — the actual steps, the actual
wait states, the actual recovery paths — for each of the three scenarios. If the
oracle only describes what the output should look like, the executor cannot
compare the actual output against it with any precision.

**Anti-pattern 2 — All three scenarios are positive cases.**
Selecting three scenarios that all represent the document working correctly
produces an experiment that can only confirm success. The worst negative case
and the borderline case must be present. An experiment with no negative scenario
is not a test — it is a demonstration.

**Anti-pattern 3 — The failure taxonomy has only one category.**
If every failure is classified as "gap," the experiment produces no actionable
information about which layer failed or what correction is needed. The taxonomy
must distinguish: structural gaps (content missing from the oracle), contract
gaps (content present but in the wrong node), and judgment gaps (content
structurally correct but semantically wrong).

**Anti-pattern 4 — The oracle is written after the executor runs.**
The oracle must exist before the executor runs. An oracle written after seeing
the output is not an oracle — it is a rationalisation of the output. The order
is always: oracle first, executor second.

---

## Stage 2 — Experiment execution and evidence collection

### Input
The five oracle documents and acceptance criteria from Stage 1.

### What happens
The executor runs each of the three scenarios through the current pipeline.
For each scenario, the executor records: the actual A input to each node, the
actual B output from each node, the confidence score where available, the
verdict emitted by each arbiter unit, and the final human result.

The executor does not interpret the results. The executor records them.
Interpretation is Stage 3.

The EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED document showed the correct pattern
for the five existing experiments: each experiment had a source binding that
named the exact pairs being examined, an expected trace that stated what the
correct output would be, and a pass/concern/reprocess verdict for each finding.

### Evidence file format

Every experiment execution produces one evidence file with six sections:

**Section 1 — Baseline.** The test suite count before the experiment ran.
This is the floor. If the count drops during or after execution, the experiment
is paused and the regression is investigated.

**Section 2 — Scenario traces.** For each of the three scenarios: the A input,
the B output from each node, the confidence score, the arbiter verdicts, and
the final human result. Verbatim. No summarisation.

**Section 3 — Comparison against oracle.** For each scenario, the expected
trace side by side with the actual trace. Differences are marked. The marking
uses the three gap classifications from the failure taxonomy.

**Section 4 — Verdict per scenario.** PASS, PASS_WITH_GAPS, or CONCERN for
each scenario. A scenario receives PASS only if the actual trace matches the
oracle on all acceptance criteria. PASS_WITH_GAPS if the trace matches on
structure but contains named gaps in content. CONCERN if the trace fails on
a measurable acceptance criterion.

**Section 5 — Gap catalogue.** Every gap found across all scenarios, each
classified by type (structural, contract, or judgment) and by the node where
it was found.

**Section 6 — Next action.** The specific Stage 3 work that the gap catalogue
requires. Named by action type: correction pass, gap closure pass, or coverage
extension pass.

### Output
One evidence file per experiment, structured as above.

### Gate
Baseline holds. All three scenarios have verbatim traces. Each gap has a
classification. The next action is named.

### Anti-patterns

**Anti-pattern 1 — Paraphrasing the output instead of quoting it.**
"The orchestrator produced a reasonable plan" is not evidence. The actual plan
text must appear in the evidence file. If the output is too long to quote in
full, quote the sections that the acceptance criteria measure against.

**Anti-pattern 2 — Conflating "the acceptance criteria check passed" with
"the oracle was met."**
The acceptance criteria are a subset of the oracle. A trace that passes the
acceptance criteria check can still contain judgment gaps not captured by the
criteria. The comparison against the full oracle must happen separately from
the acceptance criteria check.

**Anti-pattern 3 — Skipping the borderline scenario because it is ambiguous.**
The borderline scenario is the hardest to execute because the expected output
is not obviously right or wrong. It must be run. If the correct verdict for the
borderline scenario is uncertain, that uncertainty is itself a finding and must
appear in the gap catalogue as a judgment gap in the oracle itself.

---

## Stage 3 — Learning set production from experiment evidence

### Input
The evidence file from Stage 2 and the gap catalogue.

### What happens
The three actions from HOW-TO-STRUCTURE-A-LEARNING-SET-FROM-AN-EXPERIMENT are
applied in order. They must not run in parallel.

**Action 1 — Correction pass.**
Apply all CONCERN findings to existing pairs. Only existing content is modified.
No new pairs are written. A correction is a specific text change to a specific
field of a specific pair — named by pair ID, section within the pair, and the
exact text change. A correction that cannot be stated at this level of
specificity is not ready for the correction pass. It must wait for Stage 3's
gap closure pass.

The five pre-promotion checks run after every correction pass:
- Pair count unchanged (no pairs added or removed)
- All pair IDs present and unchanged
- All source citations present and unchanged
- All license annotations present and unchanged
- No new content added beyond the specific correction text

**Action 2 — Gap closure pass.**
Write new pairs for the gaps identified in the gap catalogue. The gap closure
pairs must observe all rules from the Examination Method Final:

A is a real runtime input. For Node 1, A is a prompt a real user would type.
For Node 2, A is one prompt fragment plus one domain label — not a document line.
For Nodes 3, 4, 5, A derives from the output of the previous node. No node
ever receives a raw document line as A at runtime.

B is the actual output, not a description of it. For Node 3, B is the actual
ordered plan with the actual steps, the actual wait states, the actual recovery
paths — not "a plan with steps and wait states." For Node 6, B is the actual
natural-language translation — not "a human-readable version of the formal result."

Every pair appears at least twice with different phrasing. Same situation,
different angle, same depth. Shorter rephrasing is not allowed — it loses content.

For Node 8 (arbiter units), every file that contains concern or reprocess cases
must also contain at least one pass case. A node with no pass training learns to
flag everything.

The array nodes (2, 4, 5, 6) produce one pair per element, not one pair for all
elements together. Writing one large pair where Node 2 translates four domain
labels simultaneously breaks the architectural contract — each array unit
receives exactly one element.

**Action 3 — Coverage extension pass.**
Write pairs for nodes that were proxied during the experiment's first pass —
nodes where no trained model output existed and human output was substituted.
Coverage extension pairs cannot be written before Action 2 is complete because
they chain from the gap closure pairs.

### Good examples of pair shape

**Good Node 3 pair (scheme planner):**

A: "The orchestrator must add OAuth 2.0 token validation to an existing REST API
that currently uses API key authentication. The API receives approximately 50,000
requests per hour. The API key validation is synchronous and occurs in a
middleware layer before routing. Three downstream services depend on the validated
request context."

B: "Step 1 (parallel-safe): Audit the existing API key validation middleware to
map the exact fields it writes to the request context. Observable output: a
named list of context fields with their types and the names of the downstream
services that read each field. Wait state: audit complete, context map confirmed
in writing before Step 2 begins.

Step 2 (parallel-safe): Provision the OAuth 2.0 authorization server and
register the API as a resource server. Obtain the JWKS endpoint URL and the
expected `aud` claim value for this API. Observable output: JWKS endpoint URL
confirmed reachable, test token issued and validated manually. Wait state: test
token validates successfully before Step 3 begins.

Step 3 (depends on Steps 1 and 2): Implement the JWT validation middleware. The
middleware must validate signature via JWKS, validate `exp`, `iss`, and `aud`
claims, extract the identity and scope claims and write them to the same context
fields identified in Step 1. Observable output: middleware validates a test
token and populates the exact fields from Step 1 with correct values. Recovery
path: if JWKS endpoint is unreachable, return 503 with Retry-After header; do
not fail open.

Step 4 (depends on Step 3): Deploy the new middleware in shadow mode alongside
the existing API key validation. Route 1% of production traffic through the new
path. Compare the context fields written by both paths for the same request.
Wait state: 24-hour shadow period with zero discrepancies in context fields.

Step 5 (depends on Step 4): Decommission API key validation after shadow period
passes. Remove the API key validation middleware. Observable output: API key
headers are rejected with 401."

Reason: "Step 3 depends on Steps 1 and 2 because it must write to the exact
same context fields that downstream services read — an assumption about field
names produces subtle breakage only visible when a downstream service misreads
the context. Steps 1 and 2 are parallel-safe because provisioning the auth
server does not require knowledge of the context fields and vice versa. The
shadow mode in Step 4 is required because the downstream services cannot be
changed simultaneously with the middleware — they inherit the context field names
and types, and a discrepancy would cause silent data corruption, not a visible
error."

**Good Node 8 pair (arbiter unit — pass case):**

A: "The result from Node 5 states: 'JWT validation middleware deployed to
production, JWKS endpoint reachable, test token validated successfully, shadow
period passed with zero discrepancies, API key validation middleware removed.'
Arbiter rule: the result must confirm that the authentication boundary is
closed — no path exists that bypasses JWT validation for authenticated
endpoints."

B: "PASS. The result confirms: JWKS endpoint reachable (signature validation
possible), shadow period passed with zero discrepancies (context field parity
confirmed), API key middleware removed (bypass path eliminated). The
authentication boundary is closed. No concern raised."

Reason: "A pass case is required because an arbiter trained only on concern and
reprocess cases learns to flag everything. This pass case confirms that the
arbiter fires correctly when the completion evidence is clean — it does not
produce false concerns on an unambiguous success."

**Bad examples (anti-patterns in pair shape):**

Anti-pattern: A is a document line.
"A: Raw source line: 'The API must validate all incoming requests using JWT tokens
signed by the authorization server.'"
This is wrong. No node receives a raw document line at runtime. The A must be
a user prompt (Node 1), a fragment plus label (Node 2), or a derived input from
a previous node (Nodes 3-9).

Anti-pattern: B describes instead of shows.
"B: A scheme plan that correctly sequences the OAuth migration with appropriate
wait states and recovery paths for each deployment phase."
This is wrong. The plan must appear as B. The description of what B would contain
is not B.

Anti-pattern: Rephrasing that loses content.
Original A: "I need to add OAuth 2.0 to my REST API that uses API keys."
Rephrasing A: "Add OAuth."
This is wrong. The rephrasing must preserve the same situation at the same depth.
"Can I migrate my REST API from API key auth to OAuth 2.0?" is a valid rephrasing.
"Add OAuth." is a truncation that loses context the node needs.

Anti-pattern: Parallel steps with cross-references.
"Step 3 (parallel with Step 2): Configure the JWT validation middleware.
Position it after the rate limiter (which will be added in Step 2)."
This is wrong. A step that references the output of a parallel step is not
parallel — it is sequential. The cross-reference reveals the dependency. Either
the steps are genuinely independent (remove the cross-reference) or they are
sequential (state the dependency in the plan).

### Output
Corrected pair files (Action 1), new gap closure pair files (Action 2), coverage
extension pair files (Action 3). Each file annotated with source id, license class,
attribution text, and the experiment ID whose gap catalogue produced it.

### Gate
All five pre-promotion checks pass on corrected files. New pairs have A as
real runtime input. New pairs have B as actual output not description. Every
pair appears at least twice. Node 8 files have at least one pass case. Array
nodes have one pair per element. Source citations and license annotations present.

---

## Stage 4 — Dataset compilation

### Input
The pair files from Stage 3.

### What happens
The batch registry is updated. Each promoted pair file receives four required
annotations before it enters the quota count:

- Source identifier in format `[BATCH]-[SUB-PHASE]-[SEQ]`
- License class
- Attribution required (yes/no; if yes, the required attribution text)
- Anonymisation applied (yes/no; if yes, the log of what was replaced)

A file without all four annotations does not count toward any quota.

The four-stratum stratified split (Stratum A domain breadth, Stratum B team
context, Stratum C depth level, Stratum D failure and edge case) is applied
to every batch of 120 documents before the 100/20 training/test split. The
split is not random — it is stratified to ensure each stratum contributes
proportionally to both the training set and the held-out test set.

The sub-phase-specific validity conditions from the Corpus Expansion Plan apply.
A document that passes the three general validity conditions (unambiguous primary
assignment, at least three verifiable claims, at least one known failure mode)
but fails its sub-phase-specific condition is held for revision, not promoted.
A Node 3 scheme plan pair where B does not include a named recovery path fails
the sub-phase 3.1 condition ("at least one rule stated as must or must not").

### Wave sequencing for external sources

The five-wave build sequence applies to external sources exactly as to internal
sources. External sources in the existing corpora (clean-batch-001 through
clean-batch-003) have already contributed to wave 1 sub-phases 3.1, 3.3, 3.4,
4.1, 4.3. New external source pairs enter the wave that matches their sub-phase
classification:

Wave 1 target sub-phases: 3.1 (architecture topology), 3.3 (API contract),
3.4 (security and authorization), 4.1 (backend services), 4.3 (integration
connectors). KUBERNETES-KEPS, OPENTELEMETRY-SPEC, OPENAPI-SPEC, and OWASP-ASVS
are the primary wave 1 sources.

Wave 2 target sub-phases: 2.1-2.4 (functional specification), 3.2 (data schema),
3.5 (infrastructure), 4.2 (frontend), 4.4 (data migration), 4.5 (documentation).

Wave 3 target sub-phases: 5.1-5.4 (testing), 6.1-6.4 (QA).

Wave 4 target sub-phases: 1.1-1.5 (design). The design sub-phases have the
least overlap with existing external sources and introduce the most new
predicate pattern vocabulary. They are built last to extend a working system
rather than starting from zero.

Wave 5 target sub-phases: 7.1-7.4 (QA automation), plus cross-phase transition
documents.

### Output
Updated batch registry with promoted pair files, source annotations, and stratum
assignments. Wave membership for each promoted file recorded.

### Gate
All four annotations present. Stratum assignment recorded. Sub-phase-specific
validity condition passes. Pair count in the batch registry matches the count
in the pair files.

---

## Stage 5 — Orchestrator learning test

### Purpose
Stages 1 through 4 produce a dataset. Stage 5 tests whether the orchestrator
can actually learn from that dataset and improve its behavior — not just whether
the dataset passes structural gates.

A dataset that passes all promotion gates but does not improve orchestrator
behavior has not closed the gap it was designed to close. Stage 5 is the only
stage that measures learning, not structure.

### Test structure per sub-phase

Each sub-phase's evaluation runs once after that sub-phase's wave is complete
and the relevant cells have been trained. The evaluation protocol is drawn
from the Corpus Expansion Plan and produces one result per sub-phase:

**Baseline measurement.** Before running test documents, record the full test
suite count. This is the floor. The evaluation protocol produces a report, not
new passing tests, and the floor must hold throughout.

**Category 1 — Correct flow selection and routing (4 test documents).**
Each document is clearly assignable to the target sub-phase. The orchestrator
must route it correctly. Pass condition: the sub-phase router unit produces
the correct domain label with `LogicalUnitResult.Confidence` above 0.70.
Zero tolerance on these — the orchestrator must correctly identify every
clearly-assigned document.

**Category 2 — Knowledge edge detection (4 test documents).**
Each document contains a claim that depends on knowledge outside the trained
corpus. The orchestrator must detect the edge and produce a learning gap packet.
Pass condition: a learning gap packet appears in `LogicalFlowResult.EvidenceIds`,
and `FinalHumanResult` does not contain invented content for the knowledge edge
domain. Zero tolerance for invented answers even when the edge is missed.

**Category 3 — Cross-sub-phase constraint detection (4 test documents).**
Each document contains a constraint that depends on a decision from a different
sub-phase. Pass condition: a boundary predicate appears in the result that
correctly identifies the cross-sub-phase dependency.

**Category 4 — Conflict detection (4 test documents).**
Each document contains at least one internal conflict. Pass condition: the
arbiter unit produces a reprocess verdict naming both conflicting claims, and
the formatter does not receive the conflicting result.

**Category 5 — Stratum D edge cases (4 test documents).**
Pass condition is specified per document in the test manifest.

**Passing threshold:** 17 of 20 documents passing. A sub-phase below this
threshold is not ready for cross-phase testing. The failing documents are
analyzed, the failure is attributed to the specific ILogicalUnit cell and role
that produced the wrong output, and that cell is retrained before re-evaluation.

### The learning signal test

This is distinct from the evaluation test above. The evaluation test measures
whether the trained orchestrator produces the correct output on held-out test
documents. The learning signal test measures whether the orchestrator can
improve a cell's behavior by processing new evidence it has not seen before —
without retraining on the full corpus.

The learning signal test has three steps:

**Step 1 — Establish a baseline failure.** Find a document in the held-out
test set that the current orchestrator handles incorrectly. The failure must
be clear, measurable, and attributable to a specific cell. This is the
baseline failure case.

**Step 2 — Process a corrective training pair.** Feed the orchestrator one
new training pair that directly addresses the baseline failure. The pair follows
all pair shape rules from Stage 3. It is not a batch — it is one pair.

**Step 3 — Re-run the baseline failure case.** Run the same document that
failed in Step 1 through the orchestrator after the new pair has been processed.
The pass condition is not that the orchestrator now gets a perfect score — it
is that the specific failure mode from Step 1 does not recur. If the specific
failure does not recur, the learning signal test passes. If the orchestrator
produces a different wrong output, the test records this as a different failure
mode and the cycle begins again.

### Cross-phase transition test

The cross-phase test runs after all sub-phases in a pair of adjacent phases
have passed their individual evaluations. It uses ten documents per phase
transition (six transitions × ten documents = sixty documents total). Each
document spans a phase boundary and probes the phase context compiler and the
transition gate.

The expected outcome for each document is stated in the test manifest as one
of: PASS, PASS_WITH_GAPS, or BLOCK. The orchestrator's `LogicalOrchestratorResult.OverallVerdict` must match the manifest verdict. A single
incorrect verdict on a cross-phase document is a protocol-level failure — it
means either the phase context compiler dropped a constraint or the transition
gate misclassified an artifact type.

### Expected results vs actual results format

Every orchestrator learning test produces a comparison table. Each row has
four fields:

| Test document id | Expected outcome | Actual outcome | Match |
|---|---|---|---|

The match field is PASS, PARTIAL, or FAIL. PARTIAL means the outcome category
matches (e.g. both are PASS_WITH_GAPS) but the specific gaps or concerns differ
from the manifest. A PARTIAL is not a failure but requires investigation before
the next wave begins — a different gap from the expected one means the
orchestrator is detecting something real that the oracle did not anticipate, or
is failing to detect something the oracle expected.

---

## Stage 6 — Adaptation and anti-regression

### Purpose
After the learning test in Stage 5, the orchestrator's behavior has changed.
Stage 6 ensures the change did not regress behavior on sub-phases that were
already passing.

### Wave regression check

After each wave evaluation, the regression check runs the evaluation for all
prior waves' sub-phases at their 17/20 threshold. If any sub-phase that was
passing before the new wave's cells were trained now falls below 17/20, the
regression is named as such and must be investigated before the next wave begins.

The most likely regression pattern is vocabulary interference: the new wave
introduces predicate patterns that shift the sub-phase router cell's behavior
on earlier sub-phases. The investigation identifies which pairs in the new
wave introduced the shift, and either revises the pairs (removing the
interfering vocabulary) or adds calibration pairs to the earlier sub-phase's
training set that distinguish it from the new vocabulary.

### Adaptation test pattern

The adaptation test verifies that the orchestrator can handle a methodology
variation — a company whose development process differs from the canonical
seven-phase lifecycle — without changing orchestrator code.

The test input is a methodology description. The test action is a series of
typed topology commands issued through `LogicalOrchestratorAssistant.RunTopologyCommandAsync`. The pass conditions are drawn from the WDA-05 acceptance gates:

- `topology_mutation_required` = true (the topology must change to accommodate the variation)
- `code_change_required` = false (no C# code is changed to support the variation)
- `merged_phase_preserves_all_sub_phase_signals` = true (if two phases are merged, their sub-phase predicate patterns are both detectable in the merged phase)
- `dropped_transition_count` = 0 (no transitions are silently lost)
- `safety_flags_preserved` = true (`UseArbiterAuthority` and `UseKnowledgeEdgeDetection` remain true throughout)

If any pass condition fails, the failing topology mutation command is recorded
with its exact error code and message. The gap is added to the learning gap
catalogue for that experiment, and new pairs are written in the next correction
pass cycle.

---

## Stage 7 — Registry update and next batch selection

### What happens
After Stages 1 through 6 complete for a batch of external source documents,
the batch registry is updated with the results:

- Pairs promoted and their wave assignment
- Cells trained and their resulting trust state (Candidate, Probationary, Trusted)
- Flows registered in the flow registry after cells reach Trusted
- Evaluation scores per sub-phase
- Regression check results
- Learning signal test results
- Adaptation test results
- Next batch recommendation from the priority order in Stage 0

### Next batch selection rule

The next batch is selected from the priority group that produced the highest
number of Category 2 failures (knowledge edge missed) in the Stage 5 evaluation.
Category 2 failures indicate that the corpus has insufficient coverage for the
knowledge edges that appear in real documents in that sub-phase. More pairs from
the relevant source in that priority group will close those edges.

If Category 2 failures are distributed evenly across sub-phases, the next batch
follows the wave sequence: complete Wave 1 before beginning Wave 2.

If a regression was found in Stage 6, the next batch is calibration pairs for
the regressed sub-phase, not new source documents. A regression takes priority
over forward progress.

---

## Complete pipeline summary

| Stage | Input | Gate | Output |
|---|---|---|---|
| 0 | External source registry | License class confirmed, phase assigned | Confirmed source entry |
| 1 | Confirmed source entry | Five oracle documents produced, criteria measurable | Experiment design |
| 2 | Experiment design | Baseline holds, verbatim traces recorded, gaps classified | Evidence file |
| 3 | Evidence file | Five pre-promotion checks pass, pair shape rules satisfied | Pair files |
| 4 | Pair files | Four annotations present, stratum assigned, validity passes | Updated batch registry |
| 5 | Batch registry | Evaluation runs, 17/20 per sub-phase, learning signal passes | Comparison table |
| 6 | Comparison table | No regression below 17/20 on prior waves | Adaptation test result |
| 7 | Adaptation test result | Next batch selected | Registry update |

Each stage has a gate. No stage begins until the prior gate passes. A stage that
passes its gate but fails Stage 5 requires returning to Stage 3 to write
corrective pairs — not returning to Stage 0 to select a new source. The source
and the experiment design are not wrong. The pairs are incomplete.
