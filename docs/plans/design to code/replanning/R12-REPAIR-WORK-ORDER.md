# R12-REPAIR-WORK-ORDER — hybrid learning architecture synthesis

**Current authority:** user repair request, 13.10.2026.  
**Purpose:** repair R12 so it chooses a coherent deterministic+learned architecture for the whole learning core, not merely the nearest DPO code seam.

## Why this repair exists

Historical R12-CP1 produced a useful DPO fast-path candidate, but it was too narrow for the assigned architecture question. Its own `SOURCES.csv` relied mainly on R04–R09 summaries and did not directly read the original DPO algorithm, the original four-role task, the first-learning WorkLeft program, or the dynamic-flow primary decisions before declaring several designs selected.

Therefore old R12-CP1 is **historical bounded evidence**, not a complete hybrid-architecture decision. Keep its code-traced DPO findings, but re-run architecture synthesis under this work order.

## Non-negotiable role boundary

1. **CODEX using Astra 6 UltraCode performs the human role when selecting the learning case.**
2. **The external open Chinese model authors BOTH final DPO sides: chosen and rejected.**
3. **CODEX using Astra 6 UltraCode separately evaluates the exact final pair and may return CHOSEN/REJECTED/TIE/BOTH_BAD/UNJUDGEABLE according to the active rubric.**
4. XIIGEN Student is the system being trained.
5. Weak Student output may be context, but is not automatically the final rejected side.
6. Pair authorship, human preference selection, pair-quality assessment, optimizer admission, runtime arbiter and post-training evaluation are separate roles.

Do not reopen or blur this boundary.

## Required skill stack

Read the current versions of:

- `docs/skills/trainable-architecture-decision-SKILL.md`
- `docs/skills/phase-planning-SKILL.md`
- `docs/skills/plan-review-SKILL.md`
- `docs/skills/generated-code-review-SKILL.md`
- `docs/skills/prompt-patch-authoring-SKILL.md`
- `docs/skills/reference-inventory-and-source-map-SKILL.md`
- `docs/skills/human-readable-architecture-audit-SKILL.md`
- `docs/skills/output-readability-gate-SKILL.md`

Use their evidence-honesty, source-map, work-order completeness, human-readable architecture and deterministic-vs-learned authority rules. Do **not** import their old coverage-heavy acceptance patterns as learning-efficacy evidence.

## Required direct source set

Do not close R12 from sibling summaries alone. Read the relevant spans of the current files themselves:

### Original intent and DPO
- `original task.md`
- `task prompt.md`
- `goal of the round.md`
- `Алгоритм создания ДПО.md`
- `planning of testing.md`

### WorkLeft baseline
- `09.09.26WorkLeft-00-ИТОГ.md`
- `09.09.26WorkLeft-02-teaching-chinese-model.md`
- `09.09.26WorkLeft-PLAN-HUMAN-01.md`
- `09.09.26WorkLeft-DESIGN-ACTIONS-01.md`
- relevant `WorkLeft-RESEARCH-*`, `MECH-01`, `INTEGRATION-01`, and RAG routes identified by R01–R03.

### Hybrid dynamic-flow primary
- `llm mvp/llm dynamic flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md`
- `llm mvp/llm dynamic flow/DECISIONS-LOCKED.md`
- all newer RAG successors that supersede or narrow those decisions.

### Current research and code traces
- current R01–R10/R11 checkpoints that actually exist;
- R04–R09 only as attributed evidence;
- current code for every selected state owner/consumer before implementation is authorized.

A missing source is `RESULT_NOT_OBSERVED`, not permission to invent or narrow the architecture.

## Mandatory architecture model

Preserve these behavioral responsibilities:

- **ILearningUnit** — domain + input-vector schema + output schema; typed many→many production/association.
- **ILearningFlow** — learned/corrected relations between Units and input→output transformation.
- **ILearningOrchestrator** — asynchronous combinations, candidate ordering, cancellation, cost and result composition.
- **ILearningDynamicGraph** — higher-order relations among patterns and transferable compositions.

Map them onto the actual current `ILogical*` / `ILearningMachine` code by behavior. Do not invent four new trainers. Training ownership remains with the actual LearningMachine/trainer state that is updated.

---

# R12-A — primary-lineage and conflict reconciliation

## Prompt

Build an authority table from original task → WorkLeft/RAG successor → current plan → current code/research. For every consequential architecture rule mark `CURRENT`, `SUPERSEDED`, `HISTORICAL`, `CONFLICT`, or `UNKNOWN`.

At minimum resolve:

- four-role architecture;
- DPO authorship and CODEX human-role selection/assessment;
- one-pair/no-repeat/full-state rollback;
- historical `70/30 Replay Buffer` versus current new-lessons/no-hidden-replay policy;
- semantic compression;
- StackDetector-before-normalization;
- BM25+kNN fusion;
- positive/negative/warning memory and arbiter authority;
- shared base + project LoRA;
- domain labels versus domain families and mixed/OOD cases;
- current allowed consumer/recipient boundaries.

## Output

`R12-A-AUTHORITY-MATRIX` inside FINDINGS/DECISIONS, with exact source locator and downstream owner for each conflict.

## Good example

“D-15 arbiter authority remains valid; its old numeric confidence threshold is historical unless a current source re-authorizes it.”

## Bad example

“DECISIONS-LOCKED says replay, therefore replay is mandatory.”

## Edge cases

Newer source narrows only one domain; identical class name changed semantics; a WorkLeft rule was an experiment result rather than permanent product law; current plan contradicts historical locked text.

## Verification

No selected architecture decision may rely only on sibling Rxx prose when an assigned primary exists.

---

# R12-B — solved hybrid architecture, not a DPO-only fast path

## Prompt

For each of Unit, Flow, Orchestrator and DynamicGraph write one complete causal contract:

`input/domain -> deterministic admissibility/helpers -> learned authority/state -> objective/signal -> mutation -> version/storage/rollback -> ordinary consumer -> observable output -> falsifier`.

Then draw the connection between them. Identify which paths can be learned independently and which require another level as context only.

Distinguish:
- deterministic law/invariant;
- deterministic baseline/reference;
- learned selector/reranker/corrector;
- learned parameter state;
- learned memory;
- final arbiter.

The first implementation candidate may be narrow, but the architecture must show how it plugs into the full four-role system.

## Required output

A human-readable architecture map plus a technical owner table with actual/proposed class/method/state names and `IMPLEMENTED / PARTIAL / PROPOSED / UNKNOWN`.

## Good example

“Unit solver remains frozen; Flow learns which lawful output association corrects a mixed-domain input. Improvement is measured with fixed Units.”

## Bad example

“DPO checkpoint improved, therefore DynamicGraph learned.”

## Edge cases

Multiple lawful outputs; asynchronous branch cancellation; correct deterministic answer wins; learned selector chooses a worse solver; graph state exists but no ordinary consumer loads it.

## Verification

One concrete input must be mentally traced from request to final consumer without an unnamed owner, loss, state or bridge.

---

# R12-C — DPO creation, preference and update contract

## Prompt

Design the full DPO lesson lifecycle, preserving the project roles:

1. CODEX/Astra 6 UltraCode selects the lesson and permitted context.
2. External open Chinese model creates BOTH exact final sides.
3. CODEX/Astra 6 UltraCode evaluates the exact pair separately.
4. Invalid/tie/both-bad/unjudgeable pairs do not become forced preference.
5. The exact assessed bytes/context survive tokenizer admission; any changed/reduced pair requires new assessment.
6. The optimizer updates the intended state exactly once under the active no-repeat policy.
7. Accepted state must reach an ordinary consumer.
8. Post-update evaluation is independent of pair admission.

Explicitly reconcile the original DPO algorithm with current policy; do not relabel historical authorship.

For the existing DPO trainer, verify whether response-conditioned preference is the intended objective. If direct primary/code evidence supports it, keep the CP1 repair candidate:
`m = beta * [(log pi(y+|x)-log pi_ref(y+|x)) - (log pi(y-|x)-log pi_ref(y-|x))]`.
If the actual intended objective differs, name it and its owner instead of silently preserving CP1.

## Required outputs

- exact DPO state machine;
- pair passport fields;
- admission/rejection reasons;
- optimizer recipient;
- rollback/recovery;
- ordinary adoption seam;
- E-DPO experiment handoff.

## Functional examination

Bulk-to-bulk, new lessons only unless current authority explicitly resolves replay otherwise. Measure semantic quality, pair validity, update/no-update, retention, total time and full PC footprint. Green tests/coverage are not learning evidence.

---

# R12-D — domain representation + dynamic kNN family

## Prompt

Do not defer domain/vector/kNN design to “later” as one undifferentiated block. Define a **domain-parameterized family** that can be experimentally selected.

For every target domain family specify:

- domain mapping inputs and confidence/unknown behavior;
- hard laws/admissibility that similarity cannot override;
- raw/canonical/token/semantic-feature/vector/index representations;
- which representation is shared and which is domain-specific;
- distance/similarity feature meanings and scales;
- candidate-pool filtering;
- positive/negative/warning memory;
- aggregation/reranking consumer;
- dynamic `k` rule and bounds;
- abstain/clarify path;
- exact-search diagnostic reference;
- ANN compatibility assumptions.

Treat `k` as candidate budget, not as learning by itself.

Candidate family to examine, not an automatic final formula:

`score(q,n,d)=sum_j w_j(q,d) * phi_j(q,n)`

with explicit feature normalization, weight constraints and training signal. Compare against current first-hit baseline and BM25+kNN fusion. If query-dependent scoring violates HNSW assumptions, keep exact bounded reference and separate candidate generation from learned reranking rather than silently changing the metric.

## Required domain cases

At least:
- clear in-domain;
- near-domain hard negative;
- mixed-domain;
- unknown/OOD;
- same domain with different task intent;
- same semantics with different stack/language;
- cross-domain transferable composition.

## Functional examination

Measure separately:
- domain mapping accuracy;
- lawful candidate recall;
- ranking/aggregation utility;
- downstream solution quality;
- latency;
- memory;
- index/reindex cost;
- retention after new lessons.

Use oracle-domain copies to distinguish mapping failure from solver failure.

## Good example

“Legal domain filter excludes impossible candidates, then learned reranker orders the lawful pool.”

## Bad example

“Raise k from 5 to 20 and call it adaptive learning.”

---

# R12-E — LoRA, vector projection and PC continual learning

## Prompt

Resolve the relationship between LoRA, vectorization and token projection without conflating them.

For each proposed adapter/transformation name:
- source representation;
- target representation;
- matrix/parameter group;
- trainable versus frozen parts;
- loss/signal;
- version identity;
- ordinary consumer;
- rollback;
- memory/disk cost.

LoRA may adapt a projection/model matrix, but “LoRA” by itself is not a tokenizer or proof of semantic vector meaning. If the product needs learned transformation from solver result to domain vector/token space, design that as an explicit projection/adapter with its own objective and consumer, and state whether LoRA is the parameterization technique for that projection.

Design for PC operation:
- shared base;
- compact per-domain/per-project adapters only when measured useful;
- bounded indices and memories;
- incremental update;
- full-state snapshot/rollback;
- idle/background training only if current authority still permits it;
- user feedback enters as a candidate learning signal, never automatic truth.

## Functional examination

Report model+adapter+index+memory+optimizer+snapshot footprint, training wall time, inference latency, quality gain per accepted lesson, retention and growth from bulk to bulk.

---

# R12-F — interaction experiments and earliest training runway

## Prompt

Select the shortest path that both:
1. can produce a **real measured learning signal soon**, and
2. does not structurally bypass the hybrid architecture.

Prepare four controlled hypotheses, but do not require all four before first training:

- **E1 DPO/state adoption** — exact pair -> real update -> ordinary consumer.
- **E2 domain/vector/kNN** — fixed lawful pool; compare first-hit/static fusion against learned reranking/domain-aware representation.
- **E3 Flow/Orchestrator connection** — freeze Units; learn relation/path/order and measure total branch cost.
- **E4 DynamicGraph transfer** — known primitives, unseen compositions; separate selector/path learning from solver improvement.

Choose the earliest experiment by **causal bottleneck + expected information gain / implementation cost**, not merely “closest existing code”.

Explicitly state what can start in parallel:
- implementation of missing deterministic admission/consumer seam;
- preparation of domain datasets and DPO pairs;
- benchmark/panel construction;
- PC footprint instrumentation.

Do not place Windows/GitHub Actions on the critical path unless a concrete native capability under examination needs them.

## Result R12 must hand off

To R13:
- one filled first experiment plus E2–E4 queued with exact falsifiers.

To R16:
- exact live-plan corrections.

To R18:
- bounded implementation packet(s) for the first missing mechanisms.

To R21/R22/R23:
- domain curriculum, interaction experiments and PC continual-learning measurements.

## R12 acceptance

R12 is accepted only when:

- direct primary lineage is recorded;
- all four learning roles have solved causal contracts;
- DPO roles are exactly correct;
- domain mapping, vectorization, dynamic kNN and LoRA/projection are not merely deferred placeholders;
- deterministic and learned responsibilities are explicit;
- earliest training route is chosen by causal/information-value reasoning;
- every learned claim has an ordinary consumer and a future functional examination;
- tests/coverage/Windows are not used as learning-efficacy gates;
- unresolved empirical optimum is allowed, but unresolved owner/state/objective/consumer is not hidden;
- result is human-readable and source-bounded.

**No claim of learning improvement is allowed until the bulk experiment runs.**
