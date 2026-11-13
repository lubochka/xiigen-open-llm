> **ACTIVE RP05 V4 SOURCE-OF-TRUTH** — [inspect 6 code-backed findings and 7 independent domain laws](RP05-V4-CODE-PATH-AND-SCIENCE-REPAIR-2026-10-14.md) before relying on historic source table below. Examined K branch core source (re-pin live K HEAD when executing): prepared `CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.ExecuteCapturedPreparedAsync` checks `request.DpoStepCount != 1`; generic `DpoTrainingExecutionRequest` supports configured steps but producer compatibility unverified. `FeedbackRouter` EDITED produces user-authored correction and model response — not governed final Chinese BOTH-side DPO. `FileDpoPairStore.SelectAtomicTrainingUnits` can reserve atomic CriticUnit+ImproverUnit two-row lesson. `KnnLogicalUnit` selects hits[0] despite Search(query,k); real law-aware aggregation belongs to RP02. `ILearningMachine` is distinct from historic design roles of Unit/Flow/Orch/DynamicGraph. Trace WorkLeft-RAG-MASTER-INDEX→RAG-00→relevant MD/NDJSON source.file→actual primary→code complete file+interface/callers→ordinary consumer. Past scoped reads are NOT full-code or full-RAG audits and no actual model effects have been verified. V4 overrides contradictory V3 assumptions below.

> **RP05 V3 current architecture link:** [14 task-specific work orders and four-role causal contrasts](RP05-V3-LEARNING-FIRST-EXECUTABLE-TASK-REPAIR-2026-10-14.md). Distinguish (i) CODEX human choice, (ii) external OPEN Chinese author of BOTH DPO final sides, (iii) separate CODEX exact-pair judge, (iv) Student/ILearningMachine train owner, (v) independent outcome evaluator. FeedbackRouter generic user-chosen/model-rejected and KnnLogicalUnit Search(k)→hits[0] are existing **candidates for traced investigation**, not functioning governed DPO or domain-law adaptive kNN. Domain vector law features precede nearest ranking; LoRA != semantic tokenizer/projector. `DpoStepCount` is a bounded optimizer-count parameter, not a count of unique admitted pairs. All historic snapshots below require fresh K/P HEAD/whole file/callers/consumer read for a code change. 

> **V2 SOURCE CORRECTION:** Code inspection at K branch found fresh DpoTrainingExecution.cs blob 987abd2c and CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs blob fde669af, superseding older blob notes below. Generic FeedbackRouter UserCorrection/sentinel is NOT Chinese-authored two-sided governed DPO. KnnLogicalUnit hits[0] is NOT adequate domain law-aware adaptive kNN. Source inspections are scoped, not all RAG/WorkLeft; before implementation re-read full current edited file, interfaces, callers and ordinary consumers. [F1–F5](RP05-V2-FAST-LEARNING-IMPLEMENTATION-AND-EXAMS-2026-10-14.md).

# RP05 — source authority, actual code evidence and hybrid causal owner map

## Namespace and provenance (read on 2026-10-13)
P = lubochka/xiigen-planning@llm_plan3; K = lubochka/llm_mvp_core@codex/dpo-implementation. Under P, D = llm mvp/design to code session; G = llm mvp/dpo legal build; F = llm mvp/llm dynamic flow; V = llm mvp/visual studio & visual studio code continue. A D RAG link into G/F/V must be resolved relative to that named root; do not pretend a D-only result covers all projects. Snapshot P HEAD e1541142c420ab923aae62ec181210cb867ec915 and K HEAD 3c7f5f8c7809936e25579603c96ea6df8598fc40 were observed while researching; **refresh on every execution**, not immutable task refs.

Directly read selected source content: D original task.md, D/09.09.26WorkLeft-RAG-00-usage-guide.md, D/...-RAG-MASTER-INDEX.md opening, D/...-RAG-04-vs-vscode-continue.md, F/DECISIONS-LOCKED.md, G/active-codex-execution-plan-09-auto-dpo-loop.md openings; replanning RP01 docs/tasks, R23/R24/R25 and targeted previous rounds. This is a scoped read, **not** full WorkLeft 1MB audit, all four NDJSON traversals, all 25 raw code investigations or complete G/F/V source books. Unread must be traversed by the owning task, never asserted visited.

## Required source traversal for RP05 work orders

| Route | Start and necessary primary follow-through | Owner/limit |
|---|---|---|
| Original decisions | D/original task.md → D/goal of the round.md → D/task prompt.md → D/Алгоритм создания ДПО.md; compare locked versus historical instructions | RP05.00A, .04C |
| WorkLeft library | D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → D/09.09.26WorkLeft-RAG-00-usage-guide.md → RAG-01/02/03/04 MD+NDJSON → source.file with root; D/WorkLeft-02; INTEGRATION-01; MECH-01; RESEARCH-03; TASK-PROMPTS-TRAINING; CODE changes; verify with REPORT/DECISIONS | RP05.00A, .01A, .02B |
| DPO / Graph | G/active-codex-execution-plan-07-dpo-builder.md; G/active-codex-execution-plan-09-auto-dpo-loop.md; G active guide / ILEARNINGDYNAMICGRAPH plan, then current K code | RP05.02A, .04C |
| Domain/algorithm | F/DECISIONS-LOCKED.md D-01..D-15; F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md; F/DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md; current R21/R22/R23 experiments | RP05.04B, .04C |
| DTC | D/Обучение создания HTML CSS - 1st level.md → WorkLeft-RAG-02 NDJSON → source → K/25.ModelServer/DesignConversion and K/44.DesignConversionDomain plus external original client repos | RP05.05A |
| IDE | D/WorkLeft-RAG-04 NDJSON → V/LeftToWork-CONTINUE-M1/M2/M4, V/PLAN-VS-VSCODE-CONTINUE-v1.md → live host SDK/caller; unknown installed status explicit | RP05.05B |
| Existing research | Current results/R01..R25 and PARALLEL-R01-R25-CONTRACT, R10 nine-source join, R18/R22/R23/R24/R25, RP01 scope correction, old Kiro S00–S10 | RP05.00A and task-specific deltas |

RAG is a search index, not evidence of currently executed logic; record visited IDs/source.file/root/primary SHA/lines/currentness, unresolved, and claim origin. Legacy F D-06 “70/30 Replay Buffer”, compressed 20×, 2MB adapter and domain 95% statements are historical claim/hypothesis, **not new consent to replay an applied S04 DPO pair or measured whole-PC footprint**. Current S04 no-repeat and actual R22 experiment override inference from historical counts.

## Real core method anchors — scoped direct read, not exhaustive audit

| K path, observed blob | Concrete mechanism found | RP05 question / falsifier |
|---|---|---|
| 14.ContinuousLearning/Feedback/FeedbackRouter.cs @ a676058b | Route(FeedbackEvent); Accepted→positive example; Edited→UserCorrection chosen; Rejected→no-preferred sentinel | Generic evidence is not automatically Chinese BOTH-final S04 pair. Trace caller and policy |
| 14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs @ 24ea6a7a | RunCycleAsync indexing/cache/pair reservations/IncrementalTrainer/registry | Which user/host triggers it and which actual State/ordinary caller loads adapter? No automatic RP05 integration by same name |
| 14.ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs @ a46a003a | PrepareTransactionCore and replay ledger / external consumption fields | Durable at-most-once? crash window after optimizer must be reconciled |
| 14.ContinuousLearning/Training/DpoTrainingExecution.cs @ ab5bb45c | StableDpoTrainingExecutor request binds policy/reference, LoRA, optimizer/RNG restore | Real trainable keys and reference freeze, not just changed hash |
| 26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedDpoTrainingPublicationCoordinator.cs @ 141be581 | Repository-owned CodeDocumentation trial/checkpoint publication with SingleNewPairRequired / AppliedPairAlready and attempt-recovery paths | End-to-end accepted-parent→next cold user response and post-update independent exam must be shown |
| 00.Core/LearningMachineContracts.cs @ 7f605d49 | ILearningMachine train owner; LearningBatch and LearningTrainingItem; heldout carriers | Do not make new trainers in each Unit/Flow/Graph |
| 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs @ 2957816a | fixed trained BPE+HNSW logical Unit and manifest K | First-hit versus evidence and per-domain k/metric/semantic vector compatibility need separate causal study |
| 25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs @ ae6c337c | typed DesignTree/OriginalTree/Html/Css and budget / deployment-readiness carrier | API shape != deployed cross-repo client |
| 44.DesignConversionDomain/DesignConversionImprovementLoopCoordinator.cs @ 68f8270c | bounded attempts, measurement gate, transaction context | Source-to-render-to-user outcome unverified |

Before any product edit read **full current** file, implementations, interfaces, all callers/consumers and impacted policy/native checks. RP05 publication does not assert such an exhaustive code trace happened.

## Four separate causal roles (no new competing runtime architecture)

| Role | Fixed deterministic contracts | Mutable state only if source-proven | Ordinary observation / falsifier |
|---|---|---|---|
| ILearningUnit | domain/type/vector coordinate/version, per-input stable many→many OutputRefs, safety | installed representation/ranker/adapter | unseen-family requested-type output with provenance; lost association on reordered/fan-in invalid |
| ILearningFlow | source/target ports, OutputRef identity, correction legality and dependency | learned relation/correction weights | freeze Units, vary Flow; next Unit becomes better/cheaper; unchanged relation consumer disproves effect |
| ILearningOrchestrator | lawful DAG, privilege, cancellation, bounded async fanout | route/branch/order/stop policy if actually installed | compare lawful paths under same pool, count ALL attempted branches; early wrong path cannot win by latency |
| ILearningDynamicGraph | typed ordered higher-order patterns, graph version/authority, arbiter | installed higher-order relation/composition state | freeze lower layers; transfer unseen compositions; static edge registry not learned |

LoRA A/B parameter updates are distinct from tokenizer, semantic token projection, vector codebook, index metric and HNSW. Require explicit LoRA-parameter→representation/output score→typed consumer crosswalk. Domain map UNKNOWN/MIXED must support abstention; exact retrieval validates altered metric before ANN.
