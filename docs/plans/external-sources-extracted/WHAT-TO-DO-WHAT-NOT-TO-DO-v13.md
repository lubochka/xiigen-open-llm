# What To Do and What Not To Do
# Version: 13.0 — 2026-05-06
# Replaces v12. Updated to absorb:
#   - New failure pattern: producing a visual artefact when a plan document was asked for
#   - All v12 rules preserved

---

## v13 Changes from v12

- New failure pattern added: producing a visual artefact when a markdown plan was
  requested. Full incident documented in the output format rules section below.
- All v12 rules preserved.

## 2026-05-11 Learning-Set Run Rules

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

New failure pattern: using deterministic scripts to decide semantic document
authority. This is forbidden. Scripts may enumerate, split, validate, hash, and
train. They may not decide that a root user request, flow initiation document,
session-start prompt, basic prompt, or genesis prompt is low-authority without
model-led registry analysis.

Do:

- Read the full document and surrounding ZIP/directory context.
- Identify producer, consumer, lifecycle, canonicality, and DPO behavior.
- Protect orchestration, plans, and skills as the highest-precision domains.
- Use small standalone ablations before adding new batches to the baseline.
- Keep accepted and rejected training runs documented in `RUN-STATUS.json`.

Do not:

- Train rows where chosen and rejected are identical.
- Train rows with mojibake in prompt/a_side/chosen/rejected.
- Treat ledger/status/audit rows as DPO pairs.
- Mix external code-documentation rows into orchestration/plans/skills without
  standalone and combo measurement.
- Send user-intent/root prompt documents to review-required just because code
  cannot classify them.

## v12 Changes from v11

- Seven teaching documents updated to current versions: examination method v8,
  external sources pipeline v7, XIIGen export addendum v3, self-teaching guidance v2,
  orchestrator teaching plan v3, teaching measurement guidance v4, executor-prompt-register v2.
- All references updated. No behavioural rules changed from v11.
- All v11 rules preserved.

## v11 Changes from v10

- Layer 1 transition orchestrator locked out (A47/A48): building a separate
  LogicalTransitionOrchestrator forks the learning path. Not implemented, not needed.
- Context packet enrichment locked as correct design (A49): DomainClassifierUnit reads
  prior LogicalContextPacket from request metadata; sub-phase router re-weights by
  context evidence. UseContextCarryForward flag currently defaults to true but nothing
  reads it — this is the gap to fix.
- E24 rehabilitated (A50): per-segment-per-domain contract makes AttributedSegmentRouter
  safe. False E21 split produces mild branch redundancy, not garbled input. A46 superseded.
- Domain classification confirmed FIVE copies, not three (A51): InternalLlmGateway.cs
  (InternalModelDomain enum + inline Classify) and 35.ExperimentsV2 Infrastructure/
  DomainClassifier.cs (integer constants) were previously undiscovered.
- Experiment Infrastructure folder confirmed 8 standalone production-class copies:
  PromptSplitter, AttributedRouter, DatasetRouter, SemanticNormalizer,
  SentencePatternClassifier, FamilyCollapser, LogicExtractor, StopSignalDetector.
- What not to do section updated: do not build Layer 1 transition orchestrator.
- All v10 rules preserved.

## v10 Changes from v9

- Experiment gap rule added (A43): a class that exists in projects 12, 15, 16, 22,
  or 29 but has no reference from project 21 is NOT implemented in the execution
  path. The correct test for "implemented" is: referenced from project 21.
- All 27 positive experiments confirmed disconnected from project 21.
  Three-path architecture documented: Path A (prompt), Path B (training), Path C
  (result translation). All three paths have proven components waiting to be wired.
- Source duplication confirmed: DomainLabel enum exists three times with different
  value names; RouteClassifier exists in three projects; 13 pipeline classes
  duplicated between projects 12 and 29. Must consolidate before wiring (Phase 0).
- Domain enum canonical form locked (A44): Domain { CODE_Q, BUG, PLAN, ARCH,
  REFACTOR, UNKNOWN } from project 29. DomainLabel in projects 12 and 22 deprecated.
- E21 reclassified from IMPLEMENTED to PARTIAL (A45): Decomposer class exists in
  project 12 but PromptRoutingInputCompilerUnit never calls SplitPrompt.
- E24 locked DO NOT IMPLEMENT (A46): AttributedRouter showed -37% noise increase.
  E21 must be verified in production first; E24 only after E21 is confirmed stable.
- Experiment status section corrected to distinguish "class exists" from "connected
  to runtime path". All 27 positive experiments are in the PARTIAL/NOT CONNECTED
  or NOT BUILT category from project 21's perspective.
- Plan v4 reference added: PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md
  covers all 27 experiments in 8 phases (Phase 0 consolidation first).
- All v9 rules preserved.

## v9 Changes from v8

- Batch DPO confirmed working: A41 is complete. Commit 0c8d349. One DPO step
  per domain per row, all branches batched together. 20-row run improved from
  8,056ms to 7,280ms average. All domain labels visible in batch summaries.
- Wave-2 external batch completed: 291 usable rows taught across 5 domain
  batches in one pass. 16 sub-phases covered. BUG (184 rows) flagged for time,
  not logic — all capabilities registered.
- BUG chunking rule added (A42): BUG batches over 100 rows must be chunked
  at 20-30 rows per DPO step. One step on 184 examples is weaker than 6-9
  steps on 20-30 examples each.
- XIIGen export analyzed: 28 relevant sessions, 1,441 human messages,
  ~1,080 usable rows. Domain split: BUG 40%, PLAN 30%, ARCH 20%, CODE_Q 8%,
  REFACTOR 2%. Top session: 227 human messages → ~170 usable rows.
- Artifact review methodology added: three-layer examination rule.
- Floor updated to 1224.
- All v8 rules preserved.

- Domain architecture section added: the correct design is 5 domain adapters
  (CODE_Q, BUG, PLAN, ARCH, REFACTOR), not one model per sub-phase.
- Token-to-domain-to-flow connection documented as a first-class architectural idea.
- Continued-weight learning rule added: trainer must load prior checkpoint before
  each DPO step, not construct fresh model.
- Experiment gap section updated: E34V2 (BM25+KNN fusion), E12 (SIMD matrix),
  E10 (regex prep) are the remaining gaps that matter for performance.
- Branch cap rule added: maximum 8 branches per routing pass.
- Teaching performance rules updated from 20-row bulk measurements.
- Domain label flow rule added: domain label must appear in row summaries.
- All prior v7 rules preserved.

---

## THE ORCHESTRATION MISSION

The project is building an engine that orchestrates complex software development
work. A developer types a prompt. The engine determines what kind of transition is
being requested, selects the right flows from a registry, splits the work into
the correct development domains and sub-phases, sequences it with explicit
dependencies, carries decisions forward across phase transitions, verifies the
result through arbiters, and produces coherent human-readable guidance.

The engine is not a classifier. It is not a rule engine. It is an adaptive
orchestrator that selects, sequences, and learns. The proof it works is an
orchestration scenario — a real development challenge that the engine manages
end to end, detecting inconsistencies, honouring phase ordering, and producing
coherent multi-phase guidance.

---

## THE DOMAIN ARCHITECTURE — READ THIS EVERY SESSION

This is the most important architectural rule added in v8. It describes the
correct memory model for the teaching path.

### The token-to-domain-to-flow connection

Every prompt enters the system as raw text (tokens). The first meaningful
transformation classifies those tokens into a business domain — the area of
expertise that governs how the work should be done. That domain label then selects
the flow, the adapter, and the memory that should respond.

```text
raw prompt tokens
  → PromptDomainClassifier         (D-04: CODE_Q / BUG / PLAN / ARCH / REFACTOR)
  → domain label                   (the area of expertise)
  → sub-phase routing              (which specific work areas within the domain)
  → domain adapter + sub-phase flow (the memory + the execution path)
  → result
```

Domain classification is not optional. It is the first gate. Without it, the
adapter architecture in D-05 cannot work.

### The five business domains

D-04 defines five canonical domains. These are not groupings of sub-phases. They
are areas of expertise — the kind of thinking the prompt requires.

```text
CODE_Q   — understanding what code does, how it works, where to find something
BUG      — something is broken, something does not behave as expected
PLAN     — sequencing work, phases, transitions, orchestration design
ARCH     — design decisions, patterns, structural choices, best approach
REFACTOR — internal improvement without behavior change
```

Every prompt maps to exactly one domain. This mapping is the D-04 classifier.
E18 proved 95% accuracy on this classification.

### The domain adapter is the memory for that expertise area

D-05 says each domain gets one LoRA adapter. The base model is shared. The adapter
is what makes the model an expert in BUG work versus ARCH work.

When sub-phases 4.4 (code quality), 5.1 (unit tests), and 4.1 (backend) are all
touched in a BUG prompt, they all train the same BUG adapter. The adapter
accumulates learning from every row that was classified as BUG, regardless of
which specific sub-phases were involved. After 50 BUG rows the BUG adapter knows
what debugging work looks like. After 50 CODE_Q rows the CODE_Q adapter knows
what explanation work looks like.

The sub-phases tell the adapter which part of the work to focus on. The domain
tells the adapter which expertise lens to apply.

```text
Correct model after 20 rows:
  BUG adapter     — trained on all BUG-domain rows, many sub-phases
  CODE_Q adapter  — trained on all CODE_Q rows
  ARCH adapter    — trained on all ARCH rows
  PLAN adapter    — trained on all PLAN rows
  REFACTOR adapter — trained on all REFACTOR rows

Wrong model (pre-fix, before v8):
  sub-phase-2-1-row-0001 model  — trained on exactly one sentence
  sub-phase-4-4-row-0002 model  — trained on exactly one sentence
  sub-phase-4-4-row-0004 model  — trained on exactly one sentence
  ... 114 disconnected models after 20 rows
```

### What to do

**Classify domain before routing to sub-phases.**

`PromptDomainClassifier.Classify` runs inside `RunAsync` before `RouteAsync`.
The domain label flows through `LogicalCapabilityTrainingRequest` so the trainer
knows which adapter to update.

**Key the teaching cache on domain, not sub-phase.**

The warm cache holds at most 5 live models at once — one per domain. Multiple
branches in the same row that belong to the same domain share one model and train
it sequentially in that row. The model's weights accumulate all branch signal for
that domain in one row.

**Load prior domain checkpoint before each row's first DPO step for that domain.**

If the BUG adapter has been trained in prior rows, load it before training the
current row. The current row's DPO step adds to the accumulated weights. After
the row, save the updated checkpoint. The next row will load this updated version.

**Mix 70/30 replay into each DPO step (D-06).**

Before each DPO step for a domain, sample 30% of prior training texts for that
domain from the replay list. The batch = current example + sampled prior examples.
This prevents the BUG adapter from forgetting row-1 learning when row-20 runs.

### What not to do

**Do not create a new model per sub-phase.**

One tiny model per sub-phase creates 114 disconnected models after 20 rows. Each
model knows only the one sentence it was trained on. None of them share knowledge
about what BUG work looks like in general. This was the pre-fix architecture and
it is wrong.

**Do not skip the domain classification step.**

If domain classification is missing, every row trains the same generic adapter
regardless of whether the prompt was a debugging question or an architecture
question. The adapters become meaningless.

**Do not key the warm cache on sub-phase.**

Sub-phase-keyed caching produces one cache entry per sub-phase. A row with 8
branches of the same domain creates 8 separate cache entries and 8 separate DPO
steps on 8 separate models. Domain-keyed caching produces one entry and 8
sequential DPO steps on the same model.

**Do not ignore the domain label in row summaries.**

If the row summary JSON does not contain the domain label, you cannot audit which
rows went to which domain or verify that the domain classifier is working. The
domain label must be written to every row summary record.

---

## BRANCH CAP RULE

### What to do

**Cap routing output at 8 branches maximum.**

After the relevance floor filter in `MultiSubPhaseRouterUnit`, take only the
top-8 candidates when more than 8 survive. The cap prevents broad prompts from
expanding to 26 branches and taking 40 seconds. Broad prompts with weak signal
across many sub-phases should not train 26 domain adapter steps in one row.

**Expect accepted rows to have 1-6 branches.**

The 20-row measurements show accepted rows (under 10 seconds) have 1-6 branches.
Review rows (over 10 seconds) have 6-8 branches. A row with 8 branches at
accumulated step 20+ will approach the time budget. This is acceptable. A row
with 26 branches is not.

### What not to do

**Do not remove the relevance floor filter.**

The floor filter (`bestScore × 0.80`) is correct and needed. The cap applies
after the floor filter, not instead of it.


---

## LAYER 1 TRANSITION ORCHESTRATOR — DO NOT BUILD

### The decision (A47/A48)

`LogicalTransitionOrchestrator` does not exist in the codebase and must not be
built as a separate orchestrator with its own classification model.

The concern was that a developer starting from a spec and a developer starting
from code are in different contexts. But they are doing the same kind of work —
the domain is ARCH, PLAN, or BUG regardless of whether they have a prior spec.
Building a separate Layer 1 model would mean training two models on the same domain
knowledge under two different starting conditions. Double storage, double training
cost, no extra accuracy.

The 36 transitions registered by `DevelopmentLifecycleTopologySeeder` are sequential
execution edges — they tell the orchestrator that sub-phase 3.3 must run before 3.4.
This is execution ordering, not transition type classification. Do not conflate them.

### The correct design (A49)

Context enriches the existing classifier. It does not replace it with a new one.

```text
prior context available:
  DomainClassifierUnit reads LogicalContextPacket from request metadata
    prior domain matches → confidence boost +0.10
    prior domain differs → confidence reduction -0.05
  MultiSubPhaseRouterUnit re-weights sub-phase scores using context evidence
    completed sub-phases get -0.5 weight
    active knowledge edges in prior context get +1.0 weight

no prior context:
  classifier behaves exactly as today — stateless, prompt-only
  zero regression on any existing test
```

After each orchestrator run, the result is packagable as a `LogicalContextPacket`
for the next call. The developer or calling system passes it on the next call if
work is continuing. If not passed, stateless behaviour is preserved.

### What not to do

**Do not build LogicalTransitionOrchestrator.**

**Do not build SelectedLogicalTransition as a separate contract type.**

**Do not create a separate transition type vocabulary** (feature_addition, repair,
learning_cycle, topology_mutation) — these distinctions are encoded in the prior
context packet, not in a separate classification step.

---

## DOMAIN CLASSIFICATION — FIVE COPIES, CONSOLIDATION REQUIRED (A51)

Five separate domain classification implementations exist across the codebase.
Each has diverged keyword vocabulary, diverged enum names, and diverged priority rules.
A fix to the keyword set in one copy is invisible to the other four.

| Location | Enum | Domain names |
|---|---|---|
| `12.SemanticCompiler/Routing/RouteClassifier.cs` | `DomainLabel` | CODE_Q, BUG, PLAN, ARCH, REFACTOR |
| `22.AgentAlgorithms/Routing/RouteClassifier.cs` | `DomainLabel` | CodeQuestion, Bug, Architecture... |
| `29.SemanticCompiler/Routing/RouteClassifier.cs` | `Domain` | CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN |
| `21.AgentInfrastructure/Loop/InternalLlmGateway.cs` | `InternalModelDomain` | CodeQ, Bug, Plan, Arch, Generic |
| `35.ExperimentsV2/Infrastructure/DomainClassifier.cs` | integer constants | CODE_QUESTION=0, BUG_FIX=1... |

Additionally, the experiment Infrastructure folder contains 8 standalone production-class
copies that duplicate project 12 classes.

### What to do

Execute `PLAN-DOMAIN-CLASSIFIER-SINGLE-SOURCE-v1.md` after v4 Phase 0. This creates
`DomainClassifierUnit : ILogicalUnit` in project 23, registers it by role
`"domain_classifier"`, and replaces all five copies with calls to this one unit.

`InternalModelDomain` stays as the gateway's adapter key. Only the classification
logic moves. The gateway keeps a 5-line mapping switch from `Domain` to `InternalModelDomain`.

### What not to do

**Do not fix keyword vocabulary in one copy without updating all five copies.**

**Do not create a sixth copy** of domain classification logic anywhere in the codebase.

**Do not treat E24 as unsafe** any longer. A50 rehabilitates it. The original failure
was per-segment-per-dataset simultaneous routing. The new design is per-segment-per-domain.
These are completely different contracts. A false E21 split in the new design produces
mild branch redundancy — not garbled input to a simultaneous dataset channel.


---

## TEACHING PERFORMANCE RULES

### Measurements from 20-row bulk runs

Before domain adapter fix (114 models, no cap):
```text
Average elapsed:  9,222 ms
Max elapsed:     40,149 ms (rows with 26 branches)
Average RAM:      25.6 MB
Max RAM:         142 MB
Accepted rows:   16/20
```

After domain adapter fix + cap:
```text
Average elapsed:  8,056 ms
Max elapsed:     23,818 ms
Average RAM:       3.1 MB (domain models are shared, not multiplied)
Max RAM:          87.6 MB
Accepted rows:   14/20
Learned units:   76 (was 114 — correct, fewer disconnected models)
```

The RAM improvement (25.6 MB → 3.1 MB average) confirms domain-grouped caching
is working. The time improvement is real but partial — 6 rows still exceed 10s.

### The cost breakdown — hold this every session

DPO is 98.4% of all branch time. Build stack, save, wrap, and register together
are 1.6%. No amount of caching or I/O optimization will solve the slow-row
problem. The only path to faster rows is:

1. Fewer branches per row (cap already applied).
2. One DPO step per domain per row rather than one per branch (batching).
3. Hardware acceleration (GPU — not available on this machine).
4. SIMD matrix ops in TensorCore (E12 — not yet implemented).

### The batching proposal — next engineering improvement

Currently: for a row with 8 branches in the BUG domain, the trainer runs 8
sequential DPO steps, each on one training text. The domain model goes through
8 forward/backward passes with 8 optimizer updates.

Proposed: collect all 8 branch training texts into one batch. Run one DPO step
on the combined batch. One forward/backward pass, one optimizer update. The
model sees all 8 examples simultaneously. Expected cost reduction: 8x for
same-domain multi-branch rows.

This requires `LogicalDpoTrainingUnit` to accept a batch of (prompt, chosen,
rejected) triples rather than a single triple.

### What to do

**Track DPO step count per domain across rows.**

The step count confirms that continued-weight learning is working. After 20 rows,
the BUG domain adapter should show 30-50 accumulated DPO steps. If step count
resets to 1 each row, continued-weight loading is broken.

**Verify the domain label in every row summary before accepting the row.**

If `DomainLabel` is `null` or `"?"` in the row summary, the classification step
did not run or did not write its output. This means the trainer keyed on the
wrong bucket. Do not accept rows with missing domain labels.

### What not to do

**Do not treat 14/20 accepted rows as a problem.**

The 6 review rows were time-budget failures, not logic failures. All 20 rows
passed logically with 0 knowledge edges. Review rows are correct behavior —
they flag that a row was expensive, not that it was wrong.

**Do not optimize build stack, save, or wrap time.**

These together are 1.6% of total time. Optimizing them cannot produce meaningful
improvement. Focus on DPO step reduction (batching) or matrix math speed (E12).

---

## WAVE-2 EXTERNAL BATCH — confirmed results

The wave-2 batch ran 291 usable rows (from 458 parsed) across 5 domain batches
in a single teaching pass. One DPO step per domain regardless of how many rows
were in the batch.

```text
ARCH:     16 rows →  7.2s  → ACCEPT → 6 sub-phases, max_weight=2.67
BUG:     184 rows → 203.2s → REVIEW → 13 sub-phases, max_weight=14.6
CODE_Q:   16 rows → 13.7s  → ACCEPT → 4 sub-phases, max_weight=8.75
PLAN:     71 rows → 53.6s  → ACCEPT → 9 sub-phases, max_weight=10.4
REFACTOR:  4 rows →  3.1s  → ACCEPT → 3 sub-phases, max_weight=12.0
```

BUG was flagged resource review for time only — all 184 rows registered
capabilities successfully. D-09 max_weight formula ran correctly and
automatically across all five domains.

Sub-phases covered after wave-2: 16 of 31.
All 5 domain adapters are now active.

### BUG batch size is a problem

184 examples in one DPO step produces weaker gradient signal than the same
184 examples taught in 6-9 steps of 20-30 each. One large step sees all examples
simultaneously but the optimizer can only move in one average direction. Multiple
steps can refine the gradient across different example groups.

Rule (A42): all BUG batches exceeding 100 rows must be chunked at 20-30 rows
per DPO step. Apply this to the XIIGen export BUG rows (~432 rows → ~15-20 steps).

---

## XIIGEN EXPORT — ROW ESTIMATE AND PROCESSING ORDER

The Claude conversation export contains 35 conversations. 28 are XIIGen-relevant.
Total human messages: 1,441. Estimated usable after filtering (25% skip): 1,080 rows.

Domain distribution estimate (XIIGen is more ARCH/PLAN-heavy than wave-2):
BUG ~40% (432 rows), PLAN ~30% (324), ARCH ~20% (216), CODE_Q ~8% (86),
REFACTOR ~2% (21).

Teaching time estimate as batch: ~15-20 minutes total.
BUG (~432 rows at 20-30 per chunk): ~6-9 sub-batches, ~8 min total.
PLAN (~324 rows): ~4 min. ARCH (~216 rows): ~2 min. Others: under 1 min each.

Processing priority (most valuable first):
1. Architecture and security sessions — richest dependency graphs, needed for O1
2. Plan and repair sessions — correction events, scheme planner training
3. Testing and QA sessions — sub-phases 5.x, 6.x coverage
4. Review and release sessions — arbiter and formatter rows

Top session: "Xiigen flow 01 testing and architecture review" — 227 human
messages → ~170 usable rows. Process this first within priority group 1.

---

## EXPERIMENT IMPLEMENTATION STATUS — corrected facts (v10)

**The correct test for "implemented": referenced from project 21.**

A class that exists in projects 12, 15, 16, 22, or 29 but has no reference from
project 21 is NOT implemented in the execution path regardless of how complete the
class itself is. Project 21 references only projects 00, 01, 03, 05, 06, 10, 11.

### Implemented and connected to project 21

None. Zero of the 27 positive experiments are connected to the runtime execution
path in project 21.

### Proven and built — but NOT connected to project 21 (Path A: prompt processing)

| Experiment | Improvement | Class location |
|---|---|---|
| E01 AstTokenizer | +93% perplexity | Project 12 |
| E03 N-gram | +64% perplexity | Project 12 |
| E04 BM25 | +100% recall@3 | Project 22 |
| E05 Syntax filter | +30% accuracy | Project 12 |
| E08 SyntaxRoles | +56% accuracy | Project 22 |
| E09 KNN structural | +45% accuracy | Project 22 |
| E15 Semantic tokenizer | +10% perplexity | Project 12 |
| E16 Synonym collapse | +8% perplexity | Project 12 |
| E17 Logic predicates | +97% perplexity | Project 12 |
| E18 Domain classifier | +15%, 95% accuracy | Projects 12, 22, 29 |
| E19 ComplexityRouter | +18% accuracy | Partial — project 12 |
| E21 Prompt splitter | +143% boundary accuracy | Project 12 — PARTIAL, never called |
| E23 TemplateBeautifier | +833% exact match | Project 12 |
| E25 PatternClassifier | +155% translation accuracy | Project 12 |
| E26 ContextualDictionary | +150% disambiguation | Project 12 |
| E27 PatternReorderer | +250% exact match | Project 12 |
| E32 EpisodeDecomposer | +11% accuracy | Project 15 |
| E35 DependencyDetector | +30% accuracy | Project 22 |
| E38V2 SkillRetriever | +100%, 86% load reduction | Project 22 |
| E39 QualityOrchestrator | +20% check_f1 | Project 15 |

### Proven and built — but NOT connected to project 21 (Path B: training)

| Experiment | Improvement | Class location |
|---|---|---|
| E40V2 TrustScorer | +50% accuracy | Project 16 |
| E41 IdleDetector | +100% idle accuracy | Project 16 |
| E43 ReplayBuffer | +23% retention | Project 16 |

### Not built anywhere yet

| Experiment | Improvement | What is needed |
|---|---|---|
| E11 KvCache | +54% step speed | Tokenizer-step cache — project 09 partial |
| E34V2 BM25+KNN fusion | +8% hit@1 | HybridRanker class in project 22 |
| E49 OrchestratorExpander | +44% dep. accuracy | Project 17 does not exist |
| E50 DomainGraphClosure | +75% edge delta | Project 17 does not exist |
| E51B Teaching-runtime symmetry | teaching = runtime | TrainingVectorBuilder in project 23 (planned) |

### Locked: do not implement

E24 (AttributedRouter) — showed -37% noise increase. False-positive boundary
detections from E21 cause this to make results worse, not better. Must not be
implemented until E21 boundary precision is verified in production (A46).

### The wiring plan

`PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md` covers all 27 experiments
in 8 phases preceded by Phase 0 (source consolidation). Adding one reference from
project 21 to project 23 (the planned SemanticCompilerAdapter) would bring 13
proven improvements into the runtime path simultaneously.

Expected floor after plan v4 completes: 1257 (33 new tests).

---

## CODE REVIEW FACTS — current state

**Test suite: 1224/1224 passing. Zero failures.**
**Latest commit: 0c8d349 (phase 4: batch domain teaching)**

### What is fully built and working in project 21

- All adaptive orchestrator phases (0/A/B/C/D) complete.
- `DevelopmentLifecycleTopologySeeder` — 7 phases, 31 sub-phases, 36 transitions.
- `UniversalNineNodeDevelopmentFlow` — scaffold nodes, emits generic_node_requires_domain_learning warning.
- `DefaultLogicalCapabilityTrainer` — domain-keyed, continued-weight, warm cache,
  disk-backed replay manifests, batch DPO, branch cap.
- `PromptDomainClassifier` — D-04 keyword classifier, runs before routing.
  Note: uses its own keyword profiles, not RouteClassifier from project 12 or 29.
- Domain-keyed warm cache — at most 5 live models per session, one per domain.
- Branch cap — maximum 8 branches per routing pass.
- `SecurityVerification34FlowAdapter` — located in 35.ExperimentsV2/LogicalFlow/,
  not in 21.AgentInfrastructure/Logical/.

### What project 21 does NOT yet use (all proven, all disconnected)

Projects 12, 15, 16, 22, 29 are invisible to project 21. This means:
- The SemanticCompilerFacade translation pipeline (E25/E26/E27) is bypassed.
- The Decomposer multi-task splitter (E21) is bypassed.
- The RouteClassifier (E18, 95% accuracy) is not used by the orchestrator.
- The BM25 retrieval (E04), skill retriever (E38V2), quality orchestrator (E39)
  are all bypassed.
- Training uses raw strings; it should use 60-slot feature vectors (E51B).
- The ReplayBuffer (E43), TrustScorer (E40V2), IdleDetector (E41) are bypassed.

### The structural limitation — tiny model

Teaching model: hidden dim 16, 1 layer, 18,720 params. Architecture proven;
size is placeholder. Target: ModelConfig.Small (44M params).

---

## CANONICAL PATTERN AUTHORITY

`DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3-4.md` is the single authority for
`ILogicalUnit`, `ILogicalFlow`, and `ILogicalOrchestrator`. Every session reads
it first. Every planning document references it rather than redefining the interfaces.

The v3-3 pattern includes: the intended pipeline (all 27 experiments wired), the
current pipeline (what is actually in commit 0c8d349), the implementation gap summary,
Layer 1 gap (NOT needed — A47/A48), context carry-forward gap (A49), and the five-copy
domain classification finding (A51). Read all sections every session.

---

## TEACHING PLAN RULES — all v7 rules remain in force

A29: Use `Alpha = 2f, Rank = 4` — never `Scaling = 0.5f`.
A30: Register Candidate sentinel before each new training row.
A31: Divide CPU percent by `Environment.ProcessorCount`.
A32: Report working set as a delta, not a peak.
A33: Split artifact size into BaseCheckpointBytes, LoraAdapterBytes, DpoMetricBytes.
A34: Build per-role factories before teaching any row.
A35: Wrap verification runs with TracingFlowDecorator before reading node traces.
A36: XIIGen session documents and Claude export are first-priority training source.

---

## ORCHESTRATION ROUTING RULES

Layer 0-A input type is `LogicalOrchestratorRequest`.
Layer 0-B input type is `SubPhaseRouterSourceDocument`.
Node 01 input type is `SelectedLogicalTransition`. Never raw string.

Domain classification runs before sub-phase routing. Domain label flows through
the full request into the training contract.

Show all territories when a prompt spans multiple sub-phases. A prompt touching
JWT auth and API contracts must produce routing decisions for both 3.4 and 3.3.

---

## OUTPUT FORMAT RULES — DO NOT PRODUCE A VISUAL WHEN A PLAN WAS ASKED FOR

### The failure pattern

The request was: "We need an additional more detailed report — division by phases
in the teaching process, exact placement in learning, set preparation, domain and
flow tree, recognition, LoRA, beautifier — each sub-phase's performance."

The correct output is a markdown document (`.md`) with sections, tables, and
code-fence-delimited data. The request described an analysis deliverable with
named dimensions — this is a plan or report in markdown format.

The incorrect output that was produced: a multi-section interactive HTML widget
with colour-coded chips, animated bars, tabbed panels, a domain learning
progression chart, and an accumulation chart. The response invested heavily in
presentation and visual layout. When Luba pointed out the error, the reply was
simply: "You invested so much in beautiful presentation, when I needed a proper
plan with md format."

### Why this happened

The request mentioned "report" and "examining" and "sub-phase performance" —
words that can trigger a visual output instinct. The executor read those words
and produced something impressive-looking. The actual need was for a structured
markdown document that could be referenced, diffed, and linked from other
planning documents.

### The rule

**When asked for a plan, analysis, report, or guidance document: produce markdown.**

The signal that a plan is being asked for:
- "We need a report on X" — this means a document with sections, not a dashboard
- "We need to examine / analyse / track X" — markdown with tables
- "A detailed breakdown of X" — markdown with sub-sections
- Any request that mentions dimensions, phases, metrics, or categories
  to be reported on — markdown tables, code fences, plain prose

The signal that a visual is appropriate:
- "Show me a chart of X" — chart
- "I want to see a diagram" — diagram
- "Make a UI for X" — HTML/React artifact

**When in doubt: ask whether markdown or visual is preferred before producing either.**

A plan document that covers sub-phase performance, LoRA metrics, DPO costs, and
domain learning progression should look like this:

```markdown
## Sub-Phase Performance Summary — 20-row run

| Sub-phase | Appearances | Domains | Avg DPO step | Loss | Accuracy |
|---|---|---|---|---|---|
| 4.1 | 16 | BUG, PLAN | 3.4 | 0.694 | 40% |
| 4.4 | 12 | BUG, REFACTOR | 2.1 | 0.695 | 38% |
...

## Domain Learning Progression

### BUG adapter — step-by-step chain

| Step | Row | Sub-phase | DPO cost (ms) | Loss | Accuracy |
|---|---|---|---|---|---|
| 1 | 0001 | 4.1 | 254 | 0.693 | 0% |
| 2 | 0003 | 4.4 | 261 | 0.694 | 20% |
...
```

Not a tabbed panel with chips and animated bars. A markdown document.

### What not to do

**Do not produce an HTML visual when a markdown plan or report was requested.**

**Do not invest in visual presentation before confirming the output format.**

**Do not mistake "report" for "dashboard."** A report is a document. A dashboard
is a visual tool. They are different deliverables. The default for any planning
or analysis request in this project is a markdown document.

**Do not treat reading the data carefully as permission to choose the output format.**
Reading the data is always required before producing output. Choosing the correct
output format is a separate decision and is governed by the explicit or implied
request, not by what is technically impressive to produce from that data.

---

## ARTIFACT REVIEW METHODOLOGY

When a round artifact arrives, run three layers in this order. Never skip to judgment.

**Layer 1 — Structural verification.**
Check completeness before reading content. Does the artifact contain every file
it claimed to produce? Do counts match — if the manifest says 11 packets, does
the trace show 11? Is the test count from a live run, not memory?

**Layer 2 — Contract verification.**
Check each gate against actual content, not against the manifest's claim of pass.
Read the final human result yourself and check for forbidden terms. Count metrics
yourself. This catches sessions that ran their own gate checks incorrectly.

**Layer 3 — Judgment verification.**
Read for correctness that no gate check captures. At node-03: does the dependency
graph enforce dependencies, or only list them? At node-08: is the verdict type
correct — concern for a named gap, reprocess for a structural defect? Across the
full trace: does every constraint remain visible at node-09? Nothing disappears.

**What to produce after examination.**
The verdict. Nothing else unless explicitly asked. No executor brief, no next
round plan, no command sequence.

If the round passes: state it passes, name what was done correctly, name
non-blocking observations, stop.

If the round fails: name the finding, classify it (concern or blocking),
ask for direction. Do not produce the correction unless asked.

The distinction between mechanical and judgment examination: mechanical checking
verifies that the right words appear. Judgment verification checks that the right
relationships hold. A dependency list can look correct while the dependency graph
is wrong.

---

## LOCKED DECISIONS — full list

D-01: OperationResult<T> — no exceptions, no null returns.
D-04: Domain split: CODE_Q / BUG / PLAN / ARCH / REFACTOR. These are the
      canonical names. PromptDomainClassifier uses these five labels.
D-05: LoRA adapters per domain, 2MB each. One adapter per D-04 domain.
      Base model shared. Not one model per sub-phase.
D-06: 70/30 replay buffer — prevents catastrophic forgetting.
      Inline implementation in DefaultLogicalCapabilityTrainer is acceptable.
D-07: StackDetector runs before normalization — always.
D-09: max_weight = total / (classes × rare) — never a fixed multiplier.
D-11: No external ML frameworks — TensorCore only.
D-13: Tier promotion requires coverage verification, not only metric passage.
D-14: Internal LLM first; external AI needs explicit single-use approval.
D-15: Priority decides what to attempt. Arbiters decide what is accepted.

A11: Logical contracts in `21.AgentInfrastructure/Logical`.
A12: Intent classifier is an `ILogicalUnit`, not an `if` block.
A13: Topology mutation commands use a sealed discriminated record hierarchy.
A16: Old batches (CB1, CB2, CB3) are informal reference only.
A17: Pair ID format is [BATCH]-[SUB-PHASE]-[NODE]-[SEQ].
A18: Node 08 pass cases come before failure cases in Stage 3 production.
A19: Wave 2 is routing-first — Nodes 01, 02, 03, 07 across all 31 sub-phases.
A20: Node 01 A-sides must be real developer language, not specification language.
A21: Orchestration scenarios O1-O5 are the acceptance test for the engine.
A22: Code-level pairs deferred to Wave 4. Route correctly first.
A23: AdaptiveLogicalOrchestrator is the orchestration implementation. COMPLETE.
A24: Registry relocation (Phase 0) must precede all adaptive phases. COMPLETE.
A25: SubPhaseRouterUnit input is SubPhaseRouterSourceDocument. COMPLETE.
A26: Multi-branch orchestration requires SubPhaseRoutingDecision[]. COMPLETE.
A27: Flows must support unit-resolving construction before Phase C. COMPLETE.
A28: TopologyMutationFlow command dispatch minor violation; fix when topology
     becomes its own orchestrator.
A29: LoraConfig.Scaling is read-only. Use Alpha=2f, Rank=4. Never set Scaling.
A30: Register Candidate sentinel before each new training row.
A31: CPU percent formula divides by Environment.ProcessorCount.
A32: Working set is a delta (WorkingSetDeltaMb), not a peak.
A33: Artifact size splits into BaseCheckpointBytes, LoraAdapterBytes, DpoMetricBytes.
A34: Build per-role factories before teaching any role.
A35: Wrap verification runs with TracingFlowDecorator.
A36: XIIGen session documents and Claude export are first-priority training source.
A37: Domain classification (PromptDomainClassifier, D-04) runs before sub-phase
     routing in both RunAsync and RunWithCapabilityGenerationAsync. The domain
     label flows through the full orchestrator result and into the training request.
A38: Teaching cache keys on domain label, not sub-phase ID. At most 5 live models
     per session. Multiple same-domain branches in one row train the same model.
A39: Branch cap is 8 maximum after relevance floor filter. Applied in
     MultiSubPhaseRouterUnit before building routing decisions.
A40: Domain label must appear in every row summary JSON record. A row with
     domain=null or domain=? is not fully accepted until the label is confirmed.
A41: Batch DPO: one DPO step per domain per row, all branches batched
     together. COMPLETE as of commit 0c8d349. Not the next target — done.
A43: A class that exists in projects 12, 15, 16, 22, or 29 but has no reference
     from project 21 is NOT implemented in the execution path. The test for
     "implemented" is: referenced from project 21.
A44: Domain enum canonical form is Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR,
     UNKNOWN } from project 29. DomainLabel in projects 12 and 22 are deprecated.
A45: E21 (Decomposer) is PARTIAL, not IMPLEMENTED. PromptRoutingInputCompilerUnit
     never calls SplitPrompt. Must be wired — each segment compiles independently.
A46: E24 (AttributedRouter) must NOT be implemented until E21 boundary precision
     is verified in production. AttributedRouter worsens results when E21 splits
     incorrectly.

Floor for all executor sessions: 1224.
Target floor after plan v4: 1257.
Target floor after domain single-source plan: prior floor + 19.
