# Corpus Expansion Plan — Translated to Logical Unit Terms
# 100 training documents and 20 test documents per sub-phase
# Version: 2.0 — 2026-05-04
# Supersedes: CORPUS-EXPANSION-PLAN-v1.md (node-01 through node-09 terminology)

---

## What Changed and Why

Version 1.0 described the corpus in terms of nine pipeline nodes — node-01
through node-09. These nodes are the internal teaching scaffold of the dynamic
flow. They are not the public-facing contract of the system. The public-facing
contract is now `ILogicalUnit`, `ILogicalFlow`, and `ILogicalOrchestrator`.

This version retranslates every node reference into its logical term. The
corpus content does not change. The documents, strata, validity conditions,
source tiers, and test categories are identical. Only the naming of what the
corpus is training changes.

The translation table is:

| Node term (v1.0) | Logical term (v2.0) |
|---|---|
| node-01 domain splitter | `ILogicalUnit` with role `sub-phase-router` |
| node-02 domain translator | `ILogicalUnit` with role `domain-translator` |
| node-03 scheme planner | `ILogicalUnit` with role `scheme-planner` |
| node-04 domain processor | `ILogicalUnit` with role `domain-processor` (array) |
| node-05 applier | `ILogicalUnit` with role `applier` (array) |
| node-06 result translator | `ILogicalUnit` with role `result-translator` (array) |
| node-07 arbiter selector | `ILogicalUnit` with role `arbiter-selector` |
| node-08 arbiter units | `ILogicalUnit` with role `arbiter-unit` |
| node-09 formatter | `ILogicalUnit` with role `formatter` |
| the full nine-node pipeline | `ILogicalFlow` for the relevant phase/sub-phase |
| the orchestrator assistant | `ILogicalOrchestrator` |

---

## Purpose and Scope

This plan defines how to build a corpus of 100 source documents per sub-phase
across the full seven-phase development lifecycle, plus 20 held-out test
documents per sub-phase for orchestrator evaluation. It covers twenty-seven
sub-phases, giving a total corpus of 2,700 training documents and 540 held-out
test documents, plus 60 cross-phase transition documents.

The corpus serves two purposes. First, it provides the training material for
the `ILogicalUnit` cells that fill each role in each phase domain — the
sub-phase router, the domain translator, the scheme planner, the phase context
compiler, the transition verification unit, and the formatters and arbiters
that review and present results. Second, it provides the evaluation set that
proves the `ILogicalOrchestrator` handles real documents correctly across the
full lifecycle, not just the controlled T34 and E53 scenarios.

---

## Part 1 — Document Quotas and Split Rules

Each sub-phase receives exactly 100 training documents and 20 held-out test
documents. The split uses stratified assignment (D-08), not random assignment.

Before splitting, the 120 documents for each sub-phase are grouped into four
strata. Each stratum contributes proportionally to both sets.

**Stratum A — domain breadth.** Documents from different business verticals
(e-commerce, healthcare, fintech, developer tools, consumer social).

**Stratum B — team context.** Documents that differ by team size and
organizational context (startup, mid-size product team, regulated enterprise,
open-source project).

**Stratum C — depth level.** Documents at different levels of completeness —
a one-page sketch, a detailed specification, a comprehensive reference document.

**Stratum D — failure and edge case.** Documents with incomplete information,
ambiguous requirements, conflicting decisions, or boundary conditions. These
probe the knowledge edge and rejected-item detection behavior of the
`ILogicalUnit` cells responsible for arbiter judgment and scheme planning.

The 100 training documents contain at minimum 25 from each stratum. The 20
test documents contain at minimum 5 from each stratum.

### Sub-phase quota table

| Phase | Sub-phase | Training | Test |
|---|---|---:|---:|
| 1 Design | 1.1 Design system and tokens | 100 | 20 |
| 1 Design | 1.2 Screen and layout structure | 100 | 20 |
| 1 Design | 1.3 Interaction model and state transitions | 100 | 20 |
| 1 Design | 1.4 Accessibility and compliance | 100 | 20 |
| 1 Design | 1.5 Motion and animation | 100 | 20 |
| 2 Functional spec | 2.1 User stories and acceptance criteria | 100 | 20 |
| 2 Functional spec | 2.2 Business rules | 100 | 20 |
| 2 Functional spec | 2.3 Integration point specification | 100 | 20 |
| 2 Functional spec | 2.4 Data model boundaries | 100 | 20 |
| 3 Technical design | 3.1 Architecture and component topology | 100 | 20 |
| 3 Technical design | 3.2 Data schema and migration design | 100 | 20 |
| 3 Technical design | 3.3 API contract definition | 100 | 20 |
| 3 Technical design | 3.4 Security and authorization model | 100 | 20 |
| 3 Technical design | 3.5 Infrastructure and deployment topology | 100 | 20 |
| 4 Implementation | 4.1 Backend services | 100 | 20 |
| 4 Implementation | 4.2 Frontend and client rendering | 100 | 20 |
| 4 Implementation | 4.3 Integration and provider connectors | 100 | 20 |
| 4 Implementation | 4.4 Data migration execution | 100 | 20 |
| 4 Implementation | 4.5 Documentation | 100 | 20 |
| 5 Testing | 5.1 Unit and contract tests | 100 | 20 |
| 5 Testing | 5.2 Integration tests | 100 | 20 |
| 5 Testing | 5.3 End-to-end scenario tests | 100 | 20 |
| 5 Testing | 5.4 Performance and load tests | 100 | 20 |
| 6 QA | 6.1 Functional review | 100 | 20 |
| 6 QA | 6.2 Regression coverage verification | 100 | 20 |
| 6 QA | 6.3 Security and compliance review | 100 | 20 |
| 6 QA | 6.4 Accessibility audit | 100 | 20 |
| 7 QA automation | 7.1 Test scenario automation | 100 | 20 |
| 7 QA automation | 7.2 Regression suite maintenance | 100 | 20 |
| 7 QA automation | 7.3 CI/CD gate configuration | 100 | 20 |
| 7 QA automation | 7.4 Coverage and reporting | 100 | 20 |
| **Total** | | **2,700** | **540** |

---

## Part 2 — What the Corpus Trains

### The logical units that need training per sub-phase

Each sub-phase requires a set of `ILogicalUnit` cells. A cell is a specific
trained instance of a role registered in the logical unit registry with a role,
a domain tag, a version, and a trust state. Building the corpus for a sub-phase
means producing training pairs that teach each of these cells.

**The sub-phase router cell** (`ILogicalUnit` with role `sub-phase-router`).
This cell receives the compiled predicate representation of a source document
and emits the sub-phase domain label with a confidence score. It is the first
decision point inside the `ILogicalFlow` for each phase. A sub-phase router
trained on sub-phase 3.1 documents must correctly distinguish a document about
architecture and component topology from one about data schema design. The
training pairs for this cell have A as the compiled predicate input and B as
the sub-phase domain label with confidence.

**The domain translator cell** (`ILogicalUnit` with role `domain-translator`).
This cell receives one predicate fragment paired with a domain label and emits
the formal statement in that domain's vocabulary. It operates as an array —
one cell instance per fragment. The training pairs for this cell have A as one
fragment plus one domain label and B as the formal domain statement. For
sub-phase 3.1, the B output uses architecture domain vocabulary: component
names, dependency directions, rule statements as must or must not.

**The scheme planner cell** (`ILogicalUnit` with role `scheme-planner`). This
cell receives all formal domain statements together and emits an ordered
examination plan with dependencies, parallel groups, and wait states. The
training pairs for this cell have A as the complete set of formal statements
and B as the plan with step order, dependencies, and the transformational
prompt for each step.

**The domain processor cell** (`ILogicalUnit` with role `domain-processor`,
array). This cell receives one sub-task and its domain context and emits all
success, failure, and wait-state outcomes for that sub-task. One cell instance
per sub-task. The training pairs have A as a single sub-task with context and
B as the complete outcome set.

**The applier cell** (`ILogicalUnit` with role `applier`, array). This cell
receives one sub-task, its transformational prompt, and the processed outcome
and emits the concrete action taken and its observable result. One cell
instance per sub-task. The training pairs have A as the triple of sub-task,
prompt, and outcome and B as the concrete applied result.

**The result translator cell** (`ILogicalUnit` with role `result-translator`,
array). This cell receives one formal result and emits the human-readable
fragment. One cell instance per result. The training pairs have A as the
formal result and B as the natural-language fragment.

**The arbiter selector cell** (`ILogicalUnit` with role `arbiter-selector`).
This cell receives the full collection of formal results and the flow context
and emits the names of the arbiter unit cells that must review the results.
The training pairs have A as the results collection plus context and B as the
list of arbiter unit roles that must fire.

**The arbiter unit cells** (`ILogicalUnit` with role `arbiter-unit`). Each
cell handles one review responsibility. It receives one result or result group
and emits pass, concern, or reprocess. The training pairs have A as the review
responsibility plus result and B as the verdict. The pass case must appear in
every arbiter unit file — a cell trained only on concern and reprocess cases
will flag everything.

**The formatter cell** (`ILogicalUnit` with role `formatter`). This cell
receives all approved translated results and the audience type and emits one
combined human-readable output shaped for that audience. The training pairs
have A as the approved results plus audience and B as the formatted output.

### What a training pair is in logical terms

A training pair for any `ILogicalUnit` cell has the same structure as the
existing clean-batch pairs: an A input in the exact shape the cell receives, a
B output in the exact shape the cell must produce, a Reason field explaining
why this B is correct for this A, and a Phrasing B companion that rephrases A
while producing the same B.

The cell's `ILogicalUnit<TInput, TOutput>` interface defines the A and B shapes.
A pair that does not match the registered input and output types for its cell
cannot be promoted — it would train the wrong behavior for the wrong interface.

### What makes a document valid for training

A source document is valid for a sub-phase's quota when it satisfies three
conditions. These conditions are unchanged from v1.0 because they are
properties of the document, not of the pipeline terminology.

**Condition 1 — Primary sub-phase assignment is unambiguous.** The document
belongs to exactly one sub-phase by the two-question classification method.

**Condition 2 — At least three verifiable claims.** The document makes
specific claims that can be confirmed or denied — not vague intentions.

**Condition 3 — At least one known failure mode.** You can describe what a
correct-looking but actually incorrect output from the relevant `ILogicalUnit`
cells would look like. A document with no failure modes produces training pairs
that teach nothing about where the cells can go wrong.

The sub-phase-specific validity signals from v1.0 are unchanged and apply
here — token values for sub-phase 1.1, state machine definitions for 1.3,
must/must-not rules for 3.1, gate conditions for 3.5, and so on. These signals
are what makes a document teach useful predicate patterns to the relevant cells.

---

## Part 3 — Source Authority Rules

Source authority rules are unchanged from v1.0. Three source tiers apply:

**Tier 1 — Real project documents.** Highest value. Anonymized before use.
Training OK after anonymization and validity check.

**Tier 2 — Public open-source documents.** Current primary source. Apache 2.0
and MIT are training OK. CC-BY 4.0 requires attribution. PSF 2.0 is training OK.

**Tier 3 — Synthetic documents.** Gap filler only. Must be authored against a
real regulatory or technical reference. Training OK only after domain expert
review confirms the synthetic content correctly represents the cited authority.

Every document in the corpus carries four annotations before entering the quota
count: source identifier, license class, attribution requirement, and
anonymization status. A document without all four annotations does not count.

---

## Part 4 — The Held-Out Test Set

### What each test document proves about the ILogicalOrchestrator

The twenty test documents per sub-phase probe five behaviors of the
`ILogicalOrchestrator`. The orchestrator is the thing being evaluated — it
selects the correct `ILogicalFlow` for the request, runs it, and produces the
result. The five categories test different aspects of that end-to-end behavior.

**Category 1 — Correct flow selection and routing (4 documents per sub-phase).**

The document is clearly assignable to the target sub-phase. The
`ILogicalOrchestrator` must select the correct `ILogicalFlow` for that
sub-phase, and the `ILogicalUnit` with role `sub-phase-router` inside that flow
must emit the correct sub-phase domain label.

Pass condition: the sub-phase router unit produces the correct domain label
with `LogicalUnitResult.Confidence` above 0.70. The orchestrator verdict
reflects the correct sub-phase.

Failure classification if this fails: wrong flow selected, or correct flow
selected but sub-phase router unit produced wrong label. These are different
failure types — the first is an `ILogicalOrchestrator` failure, the second is
an `ILogicalUnit` failure in the sub-phase router role.

**Category 2 — Knowledge edge detection (4 documents per sub-phase).**

The document contains at least one claim that depends on knowledge outside the
trained corpus. The `ILogicalUnit` cells responsible for scheme planning and
domain processing must detect the edge and produce a learning gap packet. The
formatter cell must not invent an answer for the gap domain.

Pass condition: a learning gap packet appears in `LogicalFlowResult.EvidenceIds`,
and the `FinalHumanResult` does not contain invented content for the knowledge
edge domain.

Failure classification if this fails: knowledge edge missed (the scheme planner
or domain processor cell did not produce the gap packet), or knowledge edge
detected but formatter cell invented content despite the gap.

**Category 3 — Cross-sub-phase constraint detection (4 documents per sub-phase).**

The document contains a constraint that depends on a decision from a different
sub-phase. The scheme planner and domain processor cells must detect the
dependency and name it as a boundary in their output. The applier cell must
not treat the unresolved assumption as a resolved constraint.

Pass condition: the `LogicalFlowResult` contains a boundary marker for the
cross-sub-phase dependency. The arbiter selector cell must fire the boundary
arbiter for this result group.

Failure classification: constraint not detected (scheme planner or domain
processor cell missed it), or detected but not preserved to the arbiter
selector cell.

**Category 4 — Conflict detection (4 documents per sub-phase).**

The document contains at least one internal conflict — two claims that cannot
both be true. The arbiter unit cells must detect the conflict and produce a
reprocess verdict naming both conflicting predicates. The formatter cell must
not receive the conflicting result.

Pass condition: the arbiter unit cell produces a reprocess verdict naming both
conflicting claims. The `LogicalFlowResult.OverallVerdict` reflects the
conflict. The formatter cell does not produce output for the conflicting item.

Failure classification: conflict not detected (arbiter unit cell missed it),
or detected but the formatter cell received the result anyway (arbiter authority
not enforced — safety flag violation).

**Category 5 — Stratum D edge cases (4 documents per sub-phase).**

Each document represents a genuine edge case. Pass condition is specified per
document in the test manifest because edge cases by definition do not have
uniform pass rules. These require manual review of `FinalHumanResult` against
the manifest's expected behavior.

### Test manifest entry per document

Every test document has a manifest entry with five fields:

Document ID — the unique identifier in the corpus.

Test category — one of the five categories above.

Expected `ILogicalOrchestrator` behavior — what the orchestrator must produce,
stated in terms of `LogicalOrchestratorResult` fields: which `OverallVerdict`,
which `FinalHumanResult` content, which `Metrics` keys and values, which
`EvidenceIds` must be present.

Pass condition — the exact measurable condition. Not "the orchestrator should
handle this correctly" — for example: `Metrics["knowledge_edge_count"] >= 1`
and `FinalHumanResult` does not contain `[domain]` in invented form.

Failure classification — which `ILogicalUnit` cell failed, in which role and
domain tag, and what type of failure it is.

---

## Part 5 — The Evaluation Protocol

### Running the ILogicalOrchestrator against the test set

The evaluation runs once per completed wave. It does not run during corpus
construction. The full test suite count is the floor — the evaluation protocol
produces a report, not new passing tests, and the floor must hold throughout.

For each of the twenty test documents per sub-phase:

**Step 1.** Call `LogicalOrchestratorAssistant.RunAsync` with the document
content as the user request and the sub-phase scenario id as the scenario.

**Step 2.** Inspect `LogicalOrchestratorResult`:
- `SelectedFlowId` — confirms the correct `ILogicalFlow` was selected.
- `OverallVerdict` — confirms the correct verdict class.
- `FinalHumanResult` — used for categories 2 and 5 where content matters.
- `Metrics` — checked against the pass condition from the manifest.
- `AppliedFlags` — confirms safety flags remain true regardless of caller flags.

**Step 3.** Record pass or fail per document with the failure classification
from the manifest if the document fails.

**Step 4.** After all twenty documents, produce the sub-phase evaluation report.

### Passing thresholds

Category 1: 4/4. The `ILogicalOrchestrator` must select the correct flow for
every clearly-routed document. Zero tolerance.

Category 2: at least 3/4. One missed knowledge edge is acceptable if the
signal was weak. Zero tolerance for invented answers even when the edge is
missed — that is an arbiter authority failure.

Category 3: at least 3/4. One missed cross-sub-phase constraint is acceptable
if the dependency was implicit.

Category 4: at least 3/4. One missed conflict is acceptable if the conflict
was between distant sections. Zero tolerance for a conflict that reached the
formatter cell — that is a `UseArbiterAuthority` safety flag failure regardless
of detection status.

Category 5: at least 3/4.

Overall minimum: 17/20 per sub-phase. A sub-phase below this threshold is not
ready for cross-phase evaluation. The failing documents are analyzed, the
failure is attributed to the specific `ILogicalUnit` cell and role that
produced the wrong output, and that cell is retrained before re-evaluation.

### Cross-phase evaluation

After all twenty-seven sub-phases pass, the cross-phase evaluation runs. It
uses sixty documents — ten per phase transition across the six canonical
transitions. Each document probes the `ILogicalOrchestrator` across a flow
boundary: one `ILogicalFlow` for the upstream phase and one for the downstream
phase, connected through the phase context compiler unit and the transition
verification unit.

The transition verification unit is itself an `ILogicalUnit` with role
`transition-gate`. It receives the upstream phase context packet and the
downstream source packet and emits a verdict: PASS, PASS WITH GAPS, or BLOCK.
The cross-phase evaluation confirms this unit produces the correct verdict for
all ten documents at each transition.

The `LogicalOrchestratorResult` for a cross-phase document must contain:
- `OverallVerdict` matching the oracle (PASS, PASS WITH GAPS, or BLOCK).
- `Metrics["concern_count"]` and `Metrics["block_count"]` matching the oracle.
- `FinalHumanResult` naming every gap if the verdict is PASS WITH GAPS.

A single incorrect verdict on a cross-phase document is a protocol-level
failure. It means either the phase context compiler cell dropped a constraint
or the transition gate cell misclassified an artifact type. Both require
investigation before the overall corpus evaluation is declared complete.

---

## Part 6 — Build Sequence and ILogicalFlow Registration

### Build sequence

The five-wave build sequence is unchanged from v1.0. Wave 1 builds sub-phases
3.1, 3.3, 3.4, 4.1, 4.3 first because they have the highest overlap with
the existing corpus. Wave 4 builds the design phase last because its vocabulary
is furthest from the current corpus. Wave 5 builds QA automation and the
cross-phase transition documents last because they depend on all prior waves.

### What happens at each wave in logical terms

After each wave's corpus is complete, the `ILogicalUnit` cells for that wave's
sub-phase roles are trained and registered in the logical unit registry with
trust state `Probationary`. They become `Trusted` after the wave's evaluation
passes at 17/20 per sub-phase.

After all cells for a sub-phase's roles are `Trusted`, a new `ILogicalFlow` is
registered in the logical flow registry for that sub-phase. The flow topology
connects the trained cells in the correct execution order: sub-phase router
first, domain translator array, scheme planner, domain processor array, applier
array, result translator array, arbiter selector, arbiter units, formatter.

The `ILogicalOrchestrator` can then route requests for that sub-phase to the
registered flow. Before the flow is registered, the orchestrator correctly
returns a failure for unknown scenarios — the existing Phase 28 behavior. After
registration, it returns the flow result.

This means the orchestrator assistant grows its capability one sub-phase at a
time. At wave 1 completion, it handles five sub-phase scenarios. At wave 5
completion, it handles all twenty-seven sub-phases plus the six cross-phase
transitions.

### Registration rule

A flow for sub-phase N is not registered until all `ILogicalUnit` cells it
depends on are `Trusted`. A cell is `Trusted` only after its sub-phase's
evaluation passes at 17/20. This ensures the orchestrator never routes a live
request to a flow backed by `Probationary` or `Candidate` cells.

If a wave regression drops a cell back below the conformance threshold, that
cell's trust state is reverted to `Probationary` and the flow that depends on
it is suspended. The `ILogicalOrchestrator` returns a clean failure for that
scenario until the cell is retrained and re-evaluated.

---

## Part 7 — What Each Batch Produces in Logical Terms

Every wave is divided into batches of approximately fifty source documents.
Each batch follows the same production sequence as the existing clean batches.

Each batch produces:

A source manifest with source identifier, license class, anonymization status,
stratum assignment, and validity check result per document.

Training pair files organized by `ILogicalUnit` role rather than by node
number. The file for the sub-phase router cell is named
`[sub-phase]-sub-phase-router-pairs.md`. The file for the domain translator
cell is named `[sub-phase]-domain-translator-pairs.md`. Array roles (domain
processor, applier, result translator) produce one pair per scenario element,
not one pair per scenario.

A conformance suite entry per cell specifying the input-output pairs that any
future cell claiming the same role and domain tag must reproduce within
tolerance before it can be registered as `Candidate`.

An updated batch registry entry in the corpus index.

A coverage ledger update.

No batch is promoted until the five pre-promotion checks pass: license
confirmed, validity conditions met, pairs correctly shaped for the cell's
`ILogicalUnit<TInput, TOutput>` interface, conformance suite entry written,
and phrasing B companion present for every original pair.

---

## Part 8 — Evaluation Cadence

Unchanged from v1.0. Evaluation runs once per completed wave. The floor holds
throughout. Wave regressions are investigated before the next wave starts. The
cross-phase evaluation runs at wave 5 after all sub-phase evaluations complete.

The final corpus declaration requires: all twenty-seven sub-phases at 17/20 or
above, all `ILogicalUnit` cells at `Trusted` state, all sub-phase `ILogicalFlow`
instances registered and active, the `ILogicalOrchestrator` routing correctly
across all scenarios, and the cross-phase evaluation passing all sixty transition
documents at the correct verdicts.
