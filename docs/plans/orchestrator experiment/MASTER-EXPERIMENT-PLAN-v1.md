# Master Experiment Plan
# Complete execution map for all fourteen experiments across four categories
# Version: 1.0 — 2026-05-04

---

## Current State

The test suite is at 997/997 after Round C. Every experiment plan that
references 985 as the floor is stale by twelve tests — any executor opening
one of these plans must mentally substitute 997 for 985 before starting. The
plans are otherwise correct. The baseline update is the first action of every
Round A executor session.

Fourteen experiments exist across four categories. None has started Round A.
All are approved to proceed to Round A upon the executor receiving this plan.

---

## The Four Categories

**Category 1 — E53 Fixture Replacement (E53 Rounds D through K).** Already
designed and contracted. Replace deterministic fixtures with trained units one
node at a time. The E53 oracle and twelve tests stay unchanged throughout.

**Category 2 — Domain and Governance Document Examination.** Four experiments
that test whether guidance documents, skill files, and planning documents
are correctly mapped to their domains and correctly bound to their arbiters.
These use the orchestration architecture as the examination engine.

**Category 3 — Training Pair Examination.** Five experiments derived from the
external batch training files. Each tests whether the B outputs in a specific
node's pair file are real content rather than descriptions of content, and
whether node boundaries are correctly observed.

**Category 4 — Architecture Document Examination.** One experiment testing
the artifact examination protocol itself — the meta-experiment that verifies
the examination layer above all others.

---

## Category 1 — E53 Fixture Replacement

These rounds run in strict dependency order. Each depends on the prior round
passing its twelve tests unchanged.

| Round | What changes | Gate |
|---|---|---|
| Round D (next) | Node-00 deterministic → trained logical compiler | 12/12 E53 tests, 997/997 full suite |
| Round E | Node-03 deterministic → trained scheme planner | Same |
| Round F | Node-08 deterministic → trained arbiter units | Same |
| Round G | Node-09 deterministic → trained formatter | Same |
| Round H | Node-02 deterministic → trained domain translator | Same |
| Round I | Nodes-04 and 05 deterministic → trained processor and applier | Same |
| Round J | Node-06 deterministic → trained result translator | Same |
| Round K | Node-07 deterministic → trained arbiter selector | Same |

**Batch grouping.** Rounds D through G form Batch 1. Rounds H through K
form Batch 2. Each batch is reviewed together using the batch protocol from
the Round D plan. The executor produces a `BATCH-ROUND-SUMMARY.md` for each
round and the architect produces one `BATCH-REVIEW-FINDINGS.md` per batch.

**What the batch review looks for.** The twelve metrics that should stay
stable across all rounds: eleven domain packets, zero lost edges, zero
dependency violations, zero invented biometric items, duplicate creation
false, zero forbidden stack terms, five of five human result sections, zero
live external calls, four learning gap packets, e2e orchestration success
true. Any metric that changes between rounds is a finding. A finding that
appears in Batch 1 but not Batch 2 means the earlier trained unit introduced
a variance that the later ones corrected. A finding that appears in both
batches means the architecture contract is exposed as variable across trained
units.

**Round D starts immediately.** The plan is already written. The executor
has the signal rules for the trained node-00 compiler. No additional design
is needed before Round D begins.

---

## Category 2 — Domain and Governance Document Examination

Four experiments. Each follows the same six-step translation method. Each
requires five oracle documents before any code runs. The oracle documents
are the most important output — producing them forces the gaps in the source
document to become visible before any execution begins.

### Experiment G1 — Guidance Document Domain Mapping

Source: `SESSION-START-PROMPT-v2_1.md`
Plan: `GUIDANCE-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1_0.md`
Central question: does the session start prompt correctly route all six
behavioral elements for an ARCHITECT session opening with uploaded documents?

**Three scenarios already designed:**
- Scenario 1: absorption directive fires before governance output
- Scenario 2: session type classification from keyword only, not document content
- Scenario 3: Hard Rule B correction signal detection and application

**Six abilities under test:** absorption order, session type classification,
governance-first load sequencing, verbatim goal anchoring, correction signal
detection, scope enforcement throughout.

**Round A executor actions:**
1. Update floor from 985 to 997 in the plan
2. Produce `GUIDANCE-MAPPING-DOMAIN-MAP-v1.md` — the formal vocabulary for
   each of the six behavioral domains
3. Produce `GUIDANCE-MAPPING-FAILURE-TAXONOMY-v1.md` — the three failure
   types (wrong classification, skipped step, governance-before-absorption)
   with arbiter verdict types
4. Produce `GUIDANCE-MAPPING-EXPECTED-TRACE-v1.md` — the oracle: expected
   output at every node boundary for all three scenarios
5. Produce `GUIDANCE-MAPPING-HUMAN-RESULT-STANDARDS-v1.md` — what a passing
   human result looks like for a reviewer who must decide whether the guidance
   was correctly applied
6. Produce `GUIDANCE-MAPPING-EXECUTOR-CONTRACT-v1.md` — exact Round B scope

Stop after Round A. Review before Round B.

**Critical oracle section:** node-03 scheme plan for Scenario 3 (correction
signal detection). The plan must show correction detection as a dependent step
that cannot run before the session type is confirmed, because an incorrect
session type classification changes which correction signals are valid.

---

### Experiment G2 — Skill Domain Mapping

Source: `SKILL-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1_0.md` (plan review skill set)
Plan: `SKILL-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1_0.md`
Central question: does the orchestrator correctly identify which planning
skills belong to a task, which are misbound, and which trigger conditions
were not detected?

**Three scenarios already designed:**
- Scenario S1: wrong skill loaded for the right layer (temporal skill for bid processing)
- Scenario S2: missing skill creates partial check output (goal delivery skill absent from plan review)
- Scenario S3: trigger condition present but not processed (materialization session type skill not loaded)

**Five abilities under test:** trigger rule accuracy, arbiter panel composition,
active prerequisite enforcement, scope gap identification, authority preservation
under custom arbiter substitution.

**Round A executor actions:** same five-document structure as G1, with
skill-specific naming:
1. Update floor to 997
2. `SKILL-MAPPING-DOMAIN-MAP-v1.md`
3. `SKILL-MAPPING-FAILURE-TAXONOMY-v1.md`
4. `SKILL-MAPPING-EXPECTED-TRACE-v1.md`
5. `SKILL-MAPPING-HUMAN-RESULT-STANDARDS-v1.md`
6. `SKILL-MAPPING-EXECUTOR-CONTRACT-v1.md`

Stop after Round A. Review before Round B.

**Critical oracle section:** node-03 scheme plan for Scenario S2. The plan
must show the goal delivery skill as a required dependency of the
definition-of-done check — not as an independent step. This is the same
two-state dependency pattern as b08 in E53.

---

### Experiment G3 — Planning Document Binding

Source: `CONTINUOUS-LEARNING-SESSION-PLAN-v1_0.md`
Plan: `PLANNING-DOCUMENT-BINDING-EXPERIMENT-PLAN-v1.md`
Central question: does the orchestrator identify that Principle 2 has no
enforcement mechanism, that the ICodeReader-to-ISessionStorage cast is a
conditional risk, and that the Phase 3 grep is weaker than the test it sits
next to?

**Three scenarios already designed:**
- Scenario D1: Principle enforcement gap (Principle 2 — BaseService — has no Gate B check)
- Scenario D2: Binding sequencing risk (ICodeReader cast not verified before use)
- Scenario D3: Gate B sufficiency gap (grep passes while formula is in comment only)

**Round A executor actions:**
1. Update floor to 997
2. `PLANNING-BINDING-DOMAIN-MAP-v1.md`
3. `PLANNING-BINDING-FAILURE-TAXONOMY-v1.md`
4. `PLANNING-BINDING-EXPECTED-TRACE-v1.md`
5. `PLANNING-BINDING-HUMAN-RESULT-STANDARDS-v1.md`
6. `PLANNING-BINDING-EXECUTOR-CONTRACT-v1.md`

Stop after Round A. Review before Round B.

**Critical oracle section:** the arbiter verdict types for the three scenarios
must be different. Scenario D1 produces a concern verdict (the principle is
not wrong, its enforcement is missing). Scenario D2 produces a concern verdict
(it is a risk, not a definite violation). Scenario D3 produces a reprocess
verdict (the gate cannot function correctly as written). Getting these verdict
types wrong in the oracle means the experiment cannot distinguish between
grades of finding.

---

### Experiment G4 — Planning Document Domain Mapping

Source: Semantic compiler session plan (referenced in `PLANNING-DOCUMENT-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1_0.md`)
Plan: `PLANNING-DOCUMENT-DOMAIN-MAPPING-EXPERIMENT-PLAN-v1_0.md`
Central question: are the four critical rules complete and correctly stated,
do the Gate B commands detect the violations they claim to prevent, does the
loading sequence establish all needed context, and do the test names cover the
right behaviors?

**Three scenarios already designed:**
- Scenario P1: Rule provenance verification (each rule traces to its experiment correctly)
- Scenario P2: Gate B detection accuracy (each gate fires correctly on violation present, passes on violation absent)
- Scenario P3: Loading sequence sufficiency (every rule dependency is loaded before the rule runs)

**Six abilities under test:** rule provenance verification, Gate B detection
accuracy, loading sequence sufficiency, test name behavioral coverage, gap
detection completeness, arbiter binding coverage.

**Round A executor actions:**
1. Update floor to 997
2. `PLANNING-MAPPING-ANALYSIS-TABLE-v1.md`
3. `PLANNING-MAPPING-GAP-REGISTRY-v1.md`
4. `PLANNING-MAPPING-ARBITER-BINDING-SPEC-v1.md`
5. `PLANNING-MAPPING-EVIDENCE-SCHEMA-v1.md`
6. `PLANNING-MAPPING-HUMAN-RESULT-CHECKLIST-v1.md`

Stop after Round A. Review before Round B.

**Note on document naming:** this experiment uses slightly different oracle
document names than the others — analysis table and gap registry instead of
domain map and failure taxonomy. The content is equivalent. The naming follows
what the plan specifies.

---

### Experiment G5 — Learning Set Translation Binding

Source: `DYNAMIC-FLOW-FIRST-FILE-LEARNING-SET-PLAN-v1.md`
Plan: `LEARNING-SET-TRANSLATION-BINDING-EXPERIMENT-PLAN-v1.md`
Central question: does the orchestrator find the coverage illusion in the
broad secondary target assignment, the self-edge teaching gap that is not
listed in the plan's known gaps, and the unmeasurable simulation criterion
in Simulation 6?

**Three scenarios already designed:**
- Scenario L1: broad secondary target dilution (lines 13-21, "all unit nodes" as secondary target)
- Scenario L2: self-edge teaching gap (edge 12, Domain applier → Domain applier, inferred rather than explicit support)
- Scenario L3: simulation pass criterion gap (Simulation 6 criterion is qualitative, not measurable)

**Round A executor actions:**
1. Update floor to 997
2. `LEARNING-TRANSLATION-DOMAIN-MAP-v1.md`
3. `LEARNING-TRANSLATION-FAILURE-TAXONOMY-v1.md`
4. `LEARNING-TRANSLATION-EXPECTED-TRACE-v1.md`
5. `LEARNING-TRANSLATION-HUMAN-RESULT-STANDARDS-v1.md`
6. `LEARNING-TRANSLATION-EXECUTOR-CONTRACT-v1.md`

Stop after Round A. Review before Round B.

**Critical oracle section:** the reprocess verdict for Scenario L3 must
contain the specific measurable criterion for the self-edge simulation. It
must name three phrases that must be absent from the reconstruction after
the self-edge row is removed: repeated application, partial results,
reprocessing passes. A reprocess verdict without this specificity is a
concern verdict in disguise.

---

### Experiment G6 — Skill Binding Examination

Source: Planning skill library
Plan: `SKILL-BINDING-EXPERIMENT-PLAN-v1.md`
Central question: given a planning task and skill library, does the orchestrator
identify domain mismatches, coverage gaps, and activation mismatches correctly
and classify each with the right failure vocabulary?

**Three scenarios already designed:**
- Scenario P1: domain mismatch (temporal behavior skill loaded for bid processing task)
- Scenario P2: coverage gap (goal delivery skill missing from plan review, causing partial check)
- Scenario P3: activation mismatch (materialization session type skill not triggered by keyword)

**Round A executor actions:**
1. Update floor to 997
2. `SKILL-BINDING-DOMAIN-MAP-v1.md`
3. `SKILL-BINDING-FAILURE-TAXONOMY-v1.md`
4. `SKILL-BINDING-EXPECTED-TRACE-v1.md`
5. `SKILL-BINDING-HUMAN-RESULT-STANDARDS-v1.md`
6. `SKILL-BINDING-EXECUTOR-CONTRACT-v1.md`

Stop after Round A. Review before Round B.

**Note on overlap with G2.** G2 (Skill Domain Mapping) and G6 (Skill Binding)
share subject matter — both test whether skills are correctly assigned to
tasks. The distinction: G2 uses the plan review skill set as the domain and
tests the orchestrator's routing logic. G6 uses the planning skill library as
the domain and tests the orchestrator's binding classification logic. They
can run in parallel — they do not depend on each other.

---

## Category 3 — Training Pair Examination

Five experiments from `EXTERNAL-BATCH-EXPERIMENTS-TRANSLATED-v1.md`. Each
tests a specific node's pair file for binding correctness and output quality.

These experiments share a structural property: the failure modes are about
content quality in the training pairs, not about orchestration logic. The
most common failure — B describes the output rather than being the output —
is visible only by reading the B fields carefully. The orchestrator's role
here is to catch this failure systematically rather than requiring human
reading of every pair.

### Experiment T1 — Scheme Planner Pair Examination (CB1-03)

Source: `clean-batch-001/node-03-scheme-planner-set.md`
Four pairs: OTel tail sampling (CB1-03-001), EF Core column rename (CB1-03-002),
Dapper local transaction (CB1-03-003), Redis shared cache (CB1-03-004)

Three failure modes:
- Parallel step dependency contradiction (CB1-03-001: Step 3 references Step 4 output, making them appear sequentially dependent while labeled parallel)
- Applier crossover (CB1-03-002 Phase 2: "update all three services to write to both columns" is an applied result, not a plan step)
- Hollow prerequisite (CB1-03-004 Step 1: the prerequisite question is already answered in the A input, making Step 1's wait state meaningless)

**Round A documents:**
1. `T1-DOMAIN-MAP-v1.md`
2. `T1-FAILURE-TAXONOMY-v1.md`
3. `T1-EXPECTED-TRACE-v1.md`
4. `T1-HUMAN-RESULT-STANDARDS-v1.md`
5. `T1-EXECUTOR-CONTRACT-v1.md`

---

### Experiment T2 — Domain Translator Pair Examination (CB2-02)

Source: `clean-batch-002/node-02-domain-translator-set.md`
Four pairs covering React scroll, Django N+1, BFF topology, Karpenter provisioning

Three failure modes:
- SQL shape crossover (CB2-02-002: the B output names the specific SQL query, which is node-04 applier territory, not node-02 translator territory)
- Solution name embedding (CB2-02-003: B names "Backend For Frontend" as the answer, removing the formal domain description and replacing it with a named pattern)
- Synthesized target (one phrasing B produces a target state that was not in the A input and cannot be derived from the fragment alone)

**Round A documents:** same five-document structure with T2 prefix.

---

### Experiment T3 — Domain Processor Pair Examination (CB2-04)

Source: `clean-batch-002/node-04-domain-processor-set.md`
Four pairs covering select_related, consistent-hash routing, EF Core migration, Redis CacheModule

Three failure modes:
- Silent failure signal (CB2-04-002 Failure state 2: "confirm load balancer supports header substring routing" is a pre-condition check, not a failure state of the sub-task)
- Non-self-contained wait state (CB2-04-003 Wait state: "confirm migration ID in production database" requires reading a production artifact that is not in the A input)
- Indistinct root causes (CB2-04-001 lists two failure states whose root causes are not distinguishable from the success state descriptions)

**Round A documents:** same five-document structure with T3 prefix.

---

### Experiment T4 — Arbiter Selector Pair Examination (CB2-07)

Source: `clean-batch-002/node-07-arbiter-selector-set.md`
Two pairs: Django N+1 fire case (CB2-07-001) and Dapper connection pass case (CB2-07-002)

Three failure modes:
- Implicit no-arbiter domain (CB2-07-001 correctly fires two arbiters and correctly states two do not fire — but the reason given for the non-firing arbiters references the domain labels rather than the formal results, which means the selector is checking domain labels instead of formal results)
- Scope completeness gap in the pass case (CB2-07-002 correctly states no arbiters fire, but the inline review reasoning does not explicitly confirm that all four available arbiters were evaluated)
- Inline review reference blurring (the pass case B writes an inline review explaining why no arbiter fires — which is correct — but the explanation contains vocabulary that belongs to the arbiter unit, not the selector)

**Round A documents:** same five-document structure with T4 prefix.

---

### Experiment T5 — Cascade Unblock Scheme Plan Examination (CB3-03)

Source: `clean-batch-003/node-03-scheme-planner-set.md`
One pair: Django N+1 cascade unblock plan

Three failure modes:
- Underspecified prerequisite (the plan's Step 1 says "verify Post.author is a ForeignKey" but does not specify what happens if it is not — the recovery path is absent)
- Single-branch recovery (the plan has one success path and one implicit failure that collapses to "use a different ORM method" without a specific recovery sequence)
- Untraceable target (the plan states the target as "1 database query, 0.38 second render time" but the A input does not specify the render time target — it was inferred from the CB2-04-001 success outcome, creating a cross-pair dependency the pair does not acknowledge)

**Round A documents:** same five-document structure with T5 prefix.

---

## Category 4 — Artifact Examination Protocol Examination

### Experiment M1 — Artifact Examination Protocol

Source: `artifact_reviewing.md`
Plan: `ARTIFACT-EXAMINATION-PROTOCOL-EXPERIMENT-PLAN-v1_0.md`
Central question: does the three-layer examination protocol correctly detect
structural gaps, contract violations, and judgment errors when a round artifact
arrives — or does the structural layer miss inventory gaps, the contract layer
accept manifest claims without reading actual content, and the judgment layer
collapse into mechanical checking?

This is the meta-experiment. It tests the examination layer that sits above
all other experiments. If this experiment passes, the examination protocol is
proven for the dynamic flow domain. If it fails, the failure classification
tells which examination layer is deficient and what the priority fix is.

**Three scenarios already designed:**
- Scenario E1: structural layer test — a synthetic round artifact with a file count mismatch, a packet count mismatch, and a metric key typo (exactly the `e2_e_orchestration_success` error caught in Round B)
- Scenario E2: contract layer test — a synthetic artifact where the manifest says all gates passed but one gate's required and actual values do not match
- Scenario E3: judgment layer test — a synthetic artifact where all gates pass and all counts are correct but node-03 lists b08 as a dependency of b09 without modeling b08's two states

**Round A executor actions:**
1. Update floor to 997
2. `M1-DOMAIN-MAP-v1.md`
3. `M1-FAILURE-TAXONOMY-v1.md`
4. `M1-EXPECTED-TRACE-v1.md` — the oracle for what correct examination looks like on each synthetic artifact
5. `M1-HUMAN-RESULT-STANDARDS-v1.md`
6. `M1-EXECUTOR-CONTRACT-v1.md`

Stop after Round A. Review before Round B.

**Critical dependency:** this experiment depends on having seen at least one
real round artifact (Round B, C, or D) before the synthetic artifacts are
designed. The Scenario E3 judgment test requires the b08 two-state pattern
as its example precisely because that pattern was identified and tested in
Round B. The synthetic artifact for Scenario E3 should reproduce a version
of the E53 Round B trace where the b08 modeling is absent, making all
contract gates pass while the judgment layer should fire.

---

## Execution Sequence and Parallelism

The fourteen experiments have the following dependency structure:

**Category 1 (E53 Rounds D-K)** runs sequentially in node dependency order.
Round D can start immediately. Each subsequent round waits for the prior one.

**Category 2 (G1 through G6)** experiments are independent of each other and
can run in parallel. They share no code and no oracle documents. The executor
can run all six Round A sessions simultaneously.

**Category 3 (T1 through T5)** experiments are independent of each other.
They can run in parallel. T1 and T5 share the node-03 subject matter but
test different pair files and have no oracle dependency between them.

**Category 4 (M1)** depends on at least one real E53 round having completed
(Round B or later) before its Scenario E3 synthetic artifact can be designed.
M1 Round A can start in parallel with Category 2 and 3, but its oracle must
reference a real artifact from Category 1 for Scenario E3.

**Recommended execution order for the first sprint:**
1. E53 Round D — start immediately, highest priority, unblocks Batch 1
2. G1 Round A, G2 Round A, G6 Round A — run in parallel, these share the
   most training material and their oracle documents inform each other
3. T1 Round A, T4 Round A — run in parallel, these cover the scheme planner
   and arbiter selector, which are the two nodes most likely to have crossover failures
4. M1 Round A — after Round D completes, so Scenario E3 has a real artifact to reference

---

## Updated Floor for All Plans

Every plan in Categories 2, 3, and 4 was written when the floor was 985.
The current floor is 997. Every executor opening a plan must apply this
substitution before starting Round A:

- Change `985/985` to `997/997` everywhere it appears as the test baseline
- Change `985 is the floor` to `997 is the floor`
- Change `at or above 985` to `at or above 997`

No other content changes are needed. The plans are otherwise correct.

---

## What a Completed Experiment Set Proves

When all fourteen experiments have completed Round B or later:

Category 1 proves that the dynamic flow architecture produces consistent
orchestration as deterministic fixtures are progressively replaced with
trained units. The batch review findings across Rounds D-K reveal whether
variance was introduced by training gaps or by architecture contracts that
were looser than the fixtures implied.

Category 2 proves that the orchestration architecture generalizes to document
examination — it can examine guidance documents, skill files, and planning
documents as structured inputs and produce actionable findings that a human
can apply before dispatching an executor.

Category 3 proves that the orchestration architecture can examine its own
training pairs for binding correctness, node boundary violations, and output
quality — closing the feedback loop between what the training set teaches and
what the nodes should produce.

Category 4 proves that the examination protocol itself is correctly specified
and correctly applied. This is the layer that validates all other layers.

Together, the fourteen experiments constitute a complete proof that the
dynamic flow architecture works not just for the registration domain it was
designed for, but for the full class of problems it was designed to handle:
complex multi-domain orchestration, document examination, training pair
validation, and meta-level protocol verification.
