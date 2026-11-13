# External Sources Orchestration Pipeline Plan
# Version: 7.0 — 2026-05-08
# Supersedes: EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v6.md

---

## 2026-05-11 Routing-Flow Correction

Stage 2.5 adaptive routing rows are now split into two levels:

1. Station rows for `PromptRoutingInputCompilerUnit` and `MultiSubPhaseRouterUnit`.
2. Acceptance rows for `OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>`.

Only the second level can promote a routing batch. Unit rows can prepare the
model, expose field-shape mistakes, and diagnose wrong predicates, but they do
not prove orchestration routing is learned.

Required routing acceptance path:

```text
LogicalOrchestratorRequest
-> SubPhaseRouterSourceDocument
-> SubPhaseRoutingDecision[]
-> knowledge-edge state
-> flow/factory resolution
-> final routing verdict
```

If the pipeline has station rows but no flow-level held-out inference, mark the
batch `PAIRSET_READY_ROUTING_FLOW_PENDING`, not `ACCEPT_BATCH`.

---

## What changed from v6

**DPO pair quality rules are now a first-class gate in every stage.** Each stage
now states which rejected type applies and what the Stage 9 corruption requirements
are. Stage 2.5 (adaptive routing rows) is unchanged from v6 but now includes
explicit pair quality requirements for each sub-stage.

**Priority reordered.** ARCH and PLAN domains are the primary accuracy deficit.
Code sub-phases (4.x) are performing adequately. Wave assignments and stage
priorities reflect this.

**Stage 9 expanded.** Three new corruption case types are required in every batch
targeting the adaptive orchestrator.

---

## 2026-05-11 Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

The pipeline now has an explicit Stage 0 safety requirement:

1. Inventory all files, subdirectories, and ZIP contents.
2. Use model-led type/canonicality adjudication before deterministic filtering.
3. Preserve root prompt/user-intent/session-start documents as possible source
   authorities.
4. Split output into clean-trainable, repair-needed, and evidence/ledger.
5. Train only small standalone ablations before combo promotion.

Observed failures that this pipeline must prevent:

```text
chosen == rejected
mojibake in prompt/chosen/rejected
unregistered external domains
weak compact rows with negative reward margin
code-doc rows mixed directly into orchestration/plans/skills baseline
user-intent documents demoted by filename or shape alone
```

The primary protected domains remain orchestration, plans, and skills. External
code-documentation is useful but must be isolated until it passes its own
measurement gate.

---

## Purpose

External and first-party sources are not collected to teach isolated facts. They
are collected to teach orchestration:

```
prompt compilation and routing
multi-sub-phase splitting
knowledge-edge detection
flow factory selection
prompt transition
expert split
phase and sub-phase routing
task sequencing
branch processing
arbiter selection
arbiter verdicts
human result formatting
model teaching and retraining
```

The code implementation is secondary. The first goal is that the adaptive
orchestrator can receive a real prompt, compile it into the router input, identify
all sub-phases the prompt touches, detect which sub-phases have no registered
flow, select the right units for those that do, and produce coherent multi-branch
guidance.

---

## Pipeline Overview

```
source registry
-> license and authority check
-> prompt and evidence extraction
-> DPO pair quality gate (applies to every stage below)
-> transition-orchestrator row production             [Stage 2]
-> adaptive routing infrastructure rows               [Stage 2.5]
   -> prompt routing input compiler rows              [Stage 2.5a]
   -> multi-sub-phase router rows                     [Stage 2.5b]
   -> knowledge-edge detection rows                   [Stage 2.5c]
   -> flow factory selection rows                     [Stage 2.5d]
-> Node 01 expert split rows                          [Stage 3]
-> Node 02 domain translation rows                    [Stage 4]
-> Node 03 scheme plan rows                           [Stage 5]
-> Nodes 04–06 branch execution rows                  [Stage 6]
-> Nodes 07–09 review and human result rows           [Stage 7]
-> teaching-unit rows                                 [Stage 8]
-> held-out and corruption rows                       [Stage 9]
-> manifest and batch zip
```

No downstream row is written from imagination. Each downstream A side comes from
the accepted B side of the previous station.

---

## DPO Pair Quality Gate — Applies to Every Stage

Before any row is accepted into a batch, it must pass this gate.

**Forbidden rejected template:** "Do not generate a capability for sub-phase X"
or any negation variant. This template shares tokens with chosen and gives the
model no signal.

**Three valid rejected types:**

Type A — Plausible adjacent sub-phase. The rejected addresses a different but
semantically close sub-phase with real technical content.

Type B — Correct sub-phases, wrong dependency order. Required for all PLAN
pairs where the prompt contains multiple tasks.

Type C — Wrong domain for the prompt's intent verb. Required for every domain
in the batch to teach domain boundaries.

**Every rejected must include a [WRONG: ...] annotation** naming the specific
confusion.

**One prompt per pair per batch.** A prompt must not be chosen in one pair and
rejected in another within the same batch.

**Batch minimums:**

| Type | Minimum fraction |
|---|---|
| Type A | 40% |
| Type B | 20% (required when prompt is multi-task) |
| Type C | 30% (required for every domain) |
| Stage 9 corruption rows | At least one of each kind |

---

## Stage 0 — Source Registration

Each source receives a record:

```
source_id
source_title
source_family
license_state              — MIT, Apache 2.0, CC BY 4.0, or first_party only
authority_type
supported_transition_types
supported_phase_sub_phase_task_types
supported_contract_signatures
supported_rejected_types   — which of A, B, C apply to pairs from this source
excluded_uses
attribution_requirement
```

`supported_contract_signatures` is mandatory. A source that cannot be mapped to
a contract signature cannot enter the learning set.

CC BY-SA and AGPL sources are not accepted. Share-alike is incompatible with a
closed training corpus.

---

## Stage 1 — Prompt and Evidence Extraction

Extract two things separately:

**Developer-language prompts** become A sides for transition rows and for prompt
routing compiler rows.

**Authority evidence** shapes B sides and arbiter verdicts.

Do not use specification text as the user prompt unless it is explicitly a
requirements document produced by a user or team.

**What makes a good PLAN prompt (from calibration run analysis):**
- Contains a named artifact: "Flow 10", "Phase 3", "Factory Interface"
- Contains dependency language: "before", "after", "once X is complete"
- Names specific sub-phases or components

**What makes a poor PLAN prompt (batch 30 pattern, accuracy 0.306):**
- Meta-commentary without technical content: "Option A is clearly the right path"
- Abstract statements: "it's grounded in the docs that already exist"
- No named components, no dependency language, no phase references

---

## Stage 2 — Transition Orchestrator Rows

Target contract:

```
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

Rows must show candidate array, selected candidate, rejected candidates,
confidence, A/B/X, evidence, and rejection reasons.

**DPO pair requirement:** Type A rejected — wrong transition type selected for
the prompt intent. The most common confusion is "repair" selected for an
additive prompt. [WRONG: repair applies when existing behavior is broken.
This prompt adds new capability that does not exist yet.]

---

## Stage 2.5 — Adaptive Routing Infrastructure Rows

Required for any batch where the target orchestrator is `AdaptiveLogicalOrchestrator`.
Once a scenario has been used in an adaptive routing batch, Stage 2.5 pairs must
exist for all subsequent batches using that scenario.

Stage 2.5a-c rows are internal station rows. Add Stage 2.5d before promotion:

### Stage 2.5d - Orchestration Routing Flow Rows

Target: `OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>`
or equivalent `AdaptiveLogicalOrchestrator` behavior.

B must include the compiled document, routing decisions, knowledge-edge state,
flow/factory resolution, warnings, and final routing verdict. Without Stage 2.5d
held-out inference, Stage 2.5 remains `PAIRSET_READY_ROUTING_FLOW_PENDING`.

### Stage 2.5a — Prompt Routing Input Compiler Rows

Target: `PromptRoutingInputCompilerUnit`

B uses only actual `SubPhaseRouterSourceDocument` fields. No `intent_signals`,
`domain_keywords`, `flags_summary`.

Confidence is 0.92 when predicates matched, 0.70 when none matched.

At least one no-predicate prompt per batch showing empty `CompiledPredicateIds`,
confidence 0.70, and the `no_high_signal_predicate` warning.

**DPO pair requirement:** Type A rejected — wrong-predicate-assignment. Authority
for correct predicate assignment comes from the eleven-predicate code-backed lexicon. The RFC
and CWE families provide concrete examples of ambiguous cases where secondary
vocabulary (e.g. "authentication") should not override the primary verb signal
(e.g. "test coverage").

### Stage 2.5b — Multi-Sub-Phase Router Rows

Target: `MultiSubPhaseRouterUnit`

A comes directly from the B of the corresponding compiler row.

B uses only actual `SubPhaseRoutingDecision` fields. No `dependency_order`,
`parallel_group`, `domain_label`, or `is_primary`.

Required cases per batch:
- One prompt producing one routing decision
- One prompt producing two or more routing decisions
- One prompt with no matched predicates producing a knowledge-edge decision
- One prompt routing to a sub-phase that exists in topology but has no registered flow

**DPO pair requirement:** Type A rejected — ambiguous-predicate-case. The rejected
routes only one sub-phase when the predicates support two. [WRONG: when multiple
predicates match, all corresponding sub-phases must be routed.]

### Stage 2.5c — Knowledge Edge Detection Rows

A is a `LogicalOrchestratorRequest` for a prompt whose router output includes
at least one sub-phase with no registered flow.

B is a `LogicalOrchestratorResult` where `OverallVerdict` is `"KNOWLEDGE_EDGE"`
and the gap branch has all six required metrics:

```
knowledge_edge, missing_sub_phase, missing_flow,
routed_phase_id, routing_confidence, knowledge_edge_reason
```

Every knowledge-edge row must be accompanied by at least one comparison row
where the same prompt routes to 3.4 (registered) and produces a PASS result.

**DPO pair requirement:** knowledge-edge-suppressed corruption. Chosen shows
KNOWLEDGE_EDGE with all six metrics. Rejected returns PASS, suppressing the
knowledge edge. This is the Stage 9 corruption case that must appear at least
once per batch.

### Stage 2.5d — Flow Factory Selection Rows

All four trust-state cases required per batch:

```
UseLearnedUnitWhenAvailable true  + Trusted learned unit       → learned selected
UseLearnedUnitWhenAvailable false + Trusted learned unit       → deterministic selected
UseLearnedUnitWhenAvailable true  + only Rejected learned unit → deterministic fallback
UseLearnedUnitWhenAvailable true  + Probationary + min Probationary → Probationary selected
```

**DPO pair requirement:** Type A rejected for the Rejected-unit case — the
rejected selects the Rejected unit instead of the deterministic fallback.
[WRONG: Rejected units are never selected regardless of the
UseLearnedUnitWhenAvailable flag.]

---

## Stage 3 — Node 01 Expert Split Rows

Target: `DomainSplitterUnit`

Rows split the selected transition into domain packets. One packet per territory
actually present in the transition. Do not add inferred territories.

Required fields per packet:

```
packet_id, domain_label, phase, sub_phase, task_type,
prompt_fragment, coverage_status, reason, handoff_marker, source_evidence
```

**DPO pair requirement:** Type A rejected — adds an ungrounded territory.
[WRONG: the domain splitter must not add territories absent from the selected
transition's A/B/X statement.]

---

## Stage 4 — Node 02 Domain Translation Rows

Target: `DomainTranslatorUnit`

One packet at a time. Array-node contract — do not group packets.

B is the formal domain statement including control type, authority source,
required evidence, conflict status, and knowledge-edge status when present.

---

## Stage 5 — Node 03 Scheme Planner Rows

Target: `SchemePlannerUnit`

Receives all translated statements together and produces ordered work branches.

**Priority target for PLAN accuracy.** Node 03 rows are the primary training
signal for dependency ordering. Every Node 03 pair for a multi-task prompt must
use a Type B rejected.

Rows must prove orchestration relationships:

```
dependencies, parallel groups, wait states,
recovery paths, context carry-forward, blocked and rejected branches
```

**DPO pair requirement:** Type B rejected — correct branches in wrong dependency
order, or branches with empty `depends_on` where a real dependency exists.
Use the dependency ordering reference table from the examination method document.

---

## Stage 6 — Nodes 04–06 Branch Execution Rows

Targets: `DomainProcessorUnit`, `ApplierUnit`, `ResultTranslatorUnit`

Nodes 04 and 05 are array-node positions — one branch at a time.
Node 06 receives the full set and may fold results but every status class
must remain visible.

The important artifact is the logical transformation:

```
current state A, target state B, transformation X,
accepted, blocked, rejected, knowledge edge, next action
```

---

## Stage 7 — Nodes 07–09 Review and Human Result Rows

Targets: `ArbiterSelectorUnit`, `ArbiterUnit`, `FormatterUnit`

Rows protect orchestration correctness by showing:

```
which arbiters fire and which do not
why each arbiter was selected or rejected
pass, concern, reprocess, or reject verdict
human-visible preservation of every hard edge
```

**DPO pair requirement for Node 08:** Type A rejected — wrong arbiter verdict.
RFC (public domain) and CWE (public domain) provide the authority for security
arbiter correctness. A prompt where the implementation satisfies a NIST MUST
requirement should produce a pass verdict; the rejected shows a concern verdict
with a fabricated reason. [WRONG: the implementation satisfies the stated control.
A concern verdict requires an identified gap.]

---

## Stage 8 — Teaching Unit Rows

Teaching rows are not optional. Training result rows must include:

```
teaching time, CPU time, RAM, disk read/write,
model artifact size, training count, held-out count, held-out accuracy
```

---

## Stage 9 — Held-Out and Corruption Rows

Every batch must include at minimum:

```
held-out prompts
rephrased prompts
wrong-transition prompts
dropped-constraint prompts
wrong-sub-phase prompts
missing-evidence prompts
arbiter contradiction prompts
wrong-predicate-assignment        ← compiler assigns wrong predicate set
ambiguous-predicate-case          ← router emits one decision when two predicates support two sub-phases
knowledge-edge-suppressed         ← PASS returned when KNOWLEDGE_EDGE required
```

**wrong-predicate-assignment:** The compiler B assigns predicates that do not
match the lexicon terms in the prompt. Authority for correct assignment: the
eleven-predicate code-backed lexicon. RFC and CWE entries provide natural examples of ambiguous
vocabulary.

```
Prompt: "test coverage shows 42% on the authentication module"
Correct:  ["TEST", "SECURITY"]
Corrupted: ["SECURITY", "AUTHORIZATION"]
[WRONG: "test coverage" triggers TEST. "authentication module" is the measured
object. TEST takes priority when the primary verb is coverage measurement.]
```

**ambiguous-predicate-case:** Router emits one decision when two predicates
support two sub-phases.

```
Prompt: "add authorization to the API endpoint"
Correct:  Routes to 3.4 AND 3.3
Corrupted: Routes only to 3.4
[WRONG: API_CONTRACT from "API endpoint" is present. Both sub-phases must route.]
```

**knowledge-edge-suppressed:** PASS returned for a prompt routing to an
unregistered sub-phase.

```
Prompt routes to 5.3 — no flow registered
Correct:  KNOWLEDGE_EDGE with all six metrics
Corrupted: PASS with no knowledge-edge markers
[WRONG: PASS fabricates a result the system cannot produce for this sub-phase.]
```

---

## Wave Priorities

### Wave 2 — Security and Compliance Review (sub-phase 6.3)

Primary source families: Wave 1 security corpus (registered-flow comparison),
CWE MITRE (public domain) for Type A rejected pairs, NIST SP 800-53 (public
domain) for arbiter authority.

DPO pair focus: Type A — 6.3 Security and Compliance Review confused with 3.4
Security and Authorization Model. Chosen: review finding that the implementation
does not satisfy the model. Rejected: redesign the auth model. [WRONG: the model
exists; the implementation is defective. Review scope, not design scope.]

Knowledge-edge detection: include Layer 0-C rows showing 6.3 as a gap sub-phase
until a flow for 6.3 is registered.

### Wave 3 — Architecture (sub-phases 3.1, 3.2, 3.5)

Primary source families: Architecture Decision Records (MIT), arc42 (MIT),
XIIGen ARCH sessions (first-party).

DPO pair focus: Type A pairs using the ARCH confusion table. Primary rejected
patterns: 3.1 confused with 4.1 (boundaries vs class body), 3.2 confused with
4.1 (model vs repository), 3.5 confused with 7.3 (signal design vs dashboard
configuration).

Knowledge-edge detection for 3.1: include Layer 0-C rows for prompts routing
jointly to 3.1 and 3.4 — the registered 3.4 branch provides the comparison.

### Wave 4 — Integration Testing (sub-phase 5.2)

Primary source families: Testcontainers (MIT), Microsoft Testing (MIT),
Playwright (Apache 2.0).

DPO pair focus: Type A — 5.2 Integration Tests confused with 5.1 Unit Tests.
Use Testcontainers documentation as authority for when a real container is
required (5.2) vs mock isolation sufficient (5.1).

Knowledge-edge detection for 5.2: include Layer 0-C rows showing 5.2 at
dependency order 2 relative to design sub-phases at order 1.

### Wave 5 — Cross-Phase PLAN Prompts

Primary source families: XIIGen sessions (first-party), orchestration pattern
documentation (Temporal, MIT; Dapr, Apache 2.0).

DPO pair focus: Type B — dependency ordering for prompts spanning two phases.
Node 03 SchemePlannerUnit rows are the primary vehicle. Use the dependency
ordering reference table to construct rejected responses.

---

## Batch Acceptance

A batch targeting the adaptive orchestrator is accepted only if it contains:

```
Stage 2.5a compiler rows (one per prompt in the batch)
Stage 2.5b router rows derived from compiler outputs
Stage 2.5c knowledge-edge rows with registered-flow comparison
Stage 2.5d factory selection rows (all four trust-state cases)
transition orchestrator rows
Node 01 split rows
Node 02 translation rows
Node 03 plan rows
Node 04–06 branch rows when upstream plan branches exist
Node 07–09 review rows when branch results exist
teaching unit rows when enough pair files exist to train
held-out and corruption rows including all three Stage 9 corruption kinds
manifest and zip
```

A batch is rejected if any pair contains the forbidden negation rejected template
or a rejected response with no [WRONG: ...] annotation.

---

## Last Updated

2026-05-08.
