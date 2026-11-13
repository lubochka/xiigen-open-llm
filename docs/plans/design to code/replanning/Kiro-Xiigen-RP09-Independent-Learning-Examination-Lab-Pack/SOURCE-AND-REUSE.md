# RP09 source routes and actual owner-bound baseline

## Namespaces, mandatory scoped traversal
P = lubochka/xiigen-planning @ llm_plan3; D = P/llm mvp/design to code session; K = lubochka/llm_mvp_core @ codex/dpo-implementation; G = P/llm mvp/dpo legal build; F = P/llm mvp/llm dynamic flow; V = P/llm mvp/visual studio & visual studio code continue.

For every RP09 task: refresh actual branch SHA and exact relevant full primary source; consult D/original task.md, D/Алгоритм создания ДПО.md, D/*WorkLeft* (especially MECH-01, RAG-MASTER-INDEX, RESEARCH-03, TRAINING, PLAN-HUMAN, RAG-00 usage), RAG NDJSON→real primary, D/replanning/previous rounds thinking results.md, PARALLEL-R01-R03-JOIN.md and D/replanning/results/R01–R25 own FINDINGS/SOURCES/DECISIONS. A visited RAG index without visited target = INDEX_ONLY. Read G/F/V primary for the actual domain. Preserve R01 IN_PROGRESS, R03 primary traversal OPEN, R07 REPAIR_REQUIRED and other live statuses if still current. Each card lists a *minimum* targeted subset, not permission to skip its actual owner/caller.

## Core observations read on 13 October 2026 (source assertions, not runtime outcomes)
- K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs at blob 2957816a88aa9cdcd3020bb81be56b5dfff165e0: Train clamps a fixed safeK; kNN index BPE/HNSW, and R06/R25 independently report a first-hit consumer in ExecuteAsync. Refresh full method and callers: a retrieved hit list alone is NOT multiple-neighbor usage.
- K/18.CapabilityTraining/LlmIngredientLogicalUnits.cs blob cb8744fcbbcff99821852e789dc92163976c0ad7: BuildBatch calls Compact on prompt, chosen, rejected before Encode; admission hash on raw data does not itself prove pre-optimizer semantic preservation. Exact response-only masks remain an examination question.
- K/11.DPO/DpoTrainer.cs blob 9c358978075db8c22fe0125eaea9e89e831d5635: StepVector calculates policy/reference rewards and gradients; training-set loss/reward/accuracy are *not* heldout ability.
- K/00.Core/LogicalContracts.cs blob fbba6f97f226d96d0d36ed08d6befa8338e86434: real LogicalUnitRequest<TInput>, LogicalUnitResult<TOutput> and ILogicalUnit<TInput,TOutput>. Do not claim an unverified compiled IQueryable-many-to-many API.
- K/43.SmartSkillsDomain/XiigenExtensionApiService.cs blob a59cdcdd830c4acb70c30ba82626155ee322a4df: partial/no-semantic-quality boundary; transport success is not adoption. Current registration and real installed host not established from this read.
- K/25.ModelServer/DesignConversion/DesignConversionModelServerLocalCallerAdapter.cs blob eaa5e75546740b3aa4ed37b78ba6e882eeb368a0: local in-repo adapter describes *no external caller wiring*. A local call is not Figma/Canva deployment.

**Important:** Some R23 text refers to ContinuousLearning/Feedback/FeedbackRouter.cs; direct fetch at that exact path on today's K branch returned NOT_FOUND. Executor must resolve actual path via repository search and owning projects and record relocation/absence, never invent a file or declare its code current.

## Evidence-to-owner matrix
| Subject | Research/process anchor | Implementation owner (inspect actual code) | RP09 only |
|---|---|---|---|
| E1 Chinese chosen/rejected, CODEX selection/judgment, Student DPO | R05/R18, RP01; WorkLeft-02, original DPO | K existing 18.CapabilityTraining / 11.DPO / repository state | Examine exact admitted pair, masks, P/C0/T, no-repeat |
| Domain/IR/tokenizer, adaptive kNN | R04/R06/R08/R22 C01–C06, RP02 | K 21.AgentInfrastructure, 06.VectorSearch, 17.DynamicDomains + consumer | Causal law-distinction and state/use trace |
| ILearningUnit / ILearningFlow | R12/R22 C07, RP03 owner if found | K LogicalContracts & Flow call graph | Attribute typed association and correction effect |
| Async Orchestrator / DynamicGraph | R06/R12/R22 C08–C09, RP04 | K orchestrator/graph actual consumer | Enumerated lawful path/cost and unseen motif effects |
| Feedback, PC state, retention | R23/RP05 | existing K state/feedback owners | Independent bulk/cost/retention observation |
| Ordinary DTC/IDE | R24/RP06/RP08, original S08/S09 | K ModelServer/Extension + external actual hosts | Cold accepted version and ordinary user outcome |
| Scientific experiment specification | R13/R17/R22/R25, RP02/05/08 existing experiment recipes | REUSE actual executable evaluator/harness first | Minimal instrumented runner only if absent |

## Source receipt / missing evidence
At every meaningful experiment decision produce one row:
source_root | repo/ref/blob | primary_path/span | RAG_locator | WorkLeft_clause | current_producer→consumer | evidence_depth (INDEX_ONLY/PRIMARY/CODE/CALL/ACTUAL_RUN) | reuser_owner | missing_seam | task_id | owner/permission | falsifier.
Do not use a file listing, historical self-report, query-hit or valid JSON as direct runtime evidence. Unknown refs are OPEN_SOURCE, not a reason to block unrelated scoped work.