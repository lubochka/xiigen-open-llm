# RP11 — source map, current evidence and write ownership

## Repo/ref contract

- P = lubochka/xiigen-planning, branch llm_plan3, path llm mvp/design to code session.
- G = P/llm mvp/dpo legal build; F = P/llm mvp/llm dynamic flow; V = P/llm mvp/visual studio & visual studio code continue.
- K = lubochka/llm_mvp_core, branch codex/dpo-implementation, with current AGENTS.md, CLAUDE.md and docs/skills.
- At writing time (2026-10-13), scoped K HEAD read: 3c7f5f8c7809936e25579603c96ea6df8598fc40. **NOT a permitted reset/target pin for future execution.** Refresh HEAD and blob before any edit.
- Repo connector permits GitHub publication of *this planning pack*. It does NOT by itself grant model calls, GPU spending, product-code edits, unreviewed provider changes or release.

## Mandatory original scope recovery — actually visit, do not just list

Start P original task.md, task prompt.md, task prompt.txt, goal of the round.md, Алгоритм создания ДПО.md, Исполнение кода без внешней модели.md, Изменения в плане работы - цель.md, planning of testing.md, Claude session decisions.md, XIIGEN-human-slice-map-2026-09-24.md, Kiro-Xiigen-Original-Slice-Queue.md. Inspect 09.09.26WorkLeft-00-ИТОГ.md, -01-code-changes, -02-teaching-chinese-model, -COVERAGE-01, -MECH-01, -RESEARCH-01, -RESEARCH-03, -RAG-MASTER-INDEX, -RAG-00-usage-guide, -PROMPTS and relevant TASK-PROMPTS, plus any additional owner/trigger file discovered. Enumerate **all** current S00–S10 and extras, and the live P/G/F/V roots. File index ≠ primary traversal.

Use P/replanning/PARALLEL-R01-R03-JOIN.md, previous rounds thinking results.md (historical error corrections), results/R01…R25, R01-R25-TO-CODE-SLICES-STRATEGY.md, R11-REPAIR-WORK-ORDER.md/results/R11, R12 architecture candidate, R13 repaired experiment protocol, R17 evidence policy, R18 A–D first lesson work, R21 subject passports, R22 C00–C11, R23 PC & feedback, R24 ordinary, R25 exact-review. Rxx documentary status does not claim live K feature. R10 is a nine-way join of R01–R09, not universal block. Examine actual source IDs, not only FINDINGS summaries.

Mandatory G: active-codex-execution-01-authority-requirements-ledger.md, -04-dpo-legal-model-policy.md, -05-dpo-data-training-measurement-guide.md, -06-ilearningdynamicgraph-architecture-brief.md, relevant LeftToWorkDPO. Mandatory F: DECISIONS-LOCKED.md, ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md, DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md, DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md, E51-* scenarios. Mandatory V: LeftToWork-CONTINUE-* and current VS/VSCODE/Continue integration and RAG. Record actual visited path/ref/blob/span and any UNREAD routes in SOURCE-LEDGER result, do not declare global coverage.

## Directly sampled K code anchors — re-open before implementing

| Path / observed blob | Meaning / open seam |
|---|---|
| 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs / 2957816a88aa9cdcd3020bb81be56b5dfff165e0 | ExecuteAsync calls Search(query,_k) but returns bestExample.Output based on hits[0]. Other hits = evidence IDs. RP02 owns actual multi-neighbor repair. |
| 23.SemanticCompilerAdapter/TrainingVectorBuilder.cs / d1fc22d7823f377da497881c0e540218dd22c6a2 | 60-position predicate/domain/root representation, not interchangeable with BPE/predicate/HNSW or LoRA model tokens. |
| 18.CapabilityTraining/LlmIngredientLogicalUnits.cs / cb8744fcbbcff99821852e789dc92163976c0ad7 | BuildBatch admits raw DPO pair, then uses Compact(prompt/chosen/rejected) before Encode; 480-character semantic-loss risk. RP01/R18 repair owner. |
| 11.DPO/DpoTrainer.cs / 9c358978075db8c22fe0125eaea9e89e831d5635 | StepVector uses admitted batches, frozen reference, optimizer, not heldout quality proof; do not treat training metrics as examination. |
| 14.ContinuousLearning/Training/IncrementalTrainer.cs / 3c7e3dcae8cfd17fedc317a2ff3f6d62ac526ad2 | calls actual DPO executor and manages permits; state/optimizer/consumer gates are owned by RP01/RP05. |

**Do not infer current code from above beyond observed lines.** For ILearningUnit, ILearningFlow, ILearningOrchestrator, DynamicGraphMachine, runtime arbiter and ordinary consumer, first discover actual type, caller and identity via scoped K repo search; capture source→receiver→state→product response. If missing or method is honest_untrained, say that and use only a frozen instrumented compatible port, not pretend training.

## Locked hybrid invariants

F/DECISIONS-LOCKED D-01 OperationResult<T>, D-03 semantic compression, D-05 shared base/project LoRA, D-06 70/30 replay, D-07 StackDetector before normalization, D-08 stratified split, D-10 BM25+kNN, D-11 TensorCore, D-14 explicit competence/approval/idle and improvement-before-install, D-15 final arbiter rejection cannot be overruled by scores or budget. The old F D-04 CODE_Q/BUG/PLAN/ARCH/REFACTOR is one categorization axis, not seven subject domains; do not conflate domain/intent/stack/project/version.

Additional nonnegotiables: positive/negative/warning memory; semantic OrderedPatternIds/order; correct deterministic answer not auto rejected; real fan-out/fan-in cost; Student state ≠ memory; LoRA ≠ vector tokenizer/projection; no trained Graph claim without changed state and independent transfer exam.

## Specific RP11-owned delta versus neighbors

**Allowed after explicit slice-level permissions:** minimal versioned provenance event/credit schema; bounded local credit estimator; small, auditable CPU/PC-fit incremental credit-state store; read-only or approval-bound adapter that supplies credit to RP04 lawful route policy. If current K has equivalent function, REUSE_NO_CHANGE. This design does not authorize direct writes; executor confirms code rights and exact paths per task.

**Owned elsewhere (default READ-ONLY):** RP01 DPO optimizer/LoRA/objective, RP02 tokenizer/domain mapper/kNN, RP03 Unit/Flow training, RP04 core graph/orchestrator learning, RP05 feedback/train transactions, RP07 Builder, RP09 exam runner/rubrics, RP06 DTC/IDE host, all previous S00–S10. Shared interfaces changed only with owner coordination, exact contract and bounded authorization. RP10 PUBLISHED DRAFT exists at replanning/Kiro-Xiigen-RP10-PC-First-Learning-Pilot-and-Promotion-Slice-Pack and owns one PC-first pilot, full-compatible selected state promotion and cold activation. Read its current source, reuse genuine receipts and DO NOT build a second activation/promotion lifecycle. Its publication does not establish any product execution; code owners still require real current K evidence.

## Required evidence receipt for every source claim

source_id | repo/ref/HEAD | path + lines/blob | P/G/F/V or K | primary_or_RAG | read_depth | actual_assertion | ordinary_consumer | unresolved_conflict | owner | next_action. Mark UNREAD and UNKNOWN frankly. Source index names are navigation, not primary validation.