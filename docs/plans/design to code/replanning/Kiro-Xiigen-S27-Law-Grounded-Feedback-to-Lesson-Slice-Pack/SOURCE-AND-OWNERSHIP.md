# S27 — Mandatory source routes, direct observed K code and owner map

P=lubochka/xiigen-planning@llm_plan3; D=P/llm mvp/design to code session; R=D/replanning; G=P/llm mvp/dpo legal build; F=P/llm mvp/llm dynamic flow; V=P/llm mvp/visual studio & visual studio code continue; K=lubochka/llm_mvp_core@codex/dpo-implementation. Pin current remote HEADs and each relevant blob on every worker invocation. A historical name/branch is not a verified present caller.

## Start wide, descend to task-relevant primaries

Read D original task.md, task prompt.md, goal of the round.md, Алгоритм создания ДПО.md, 09.09.26WorkLeft-00-ИТОГ.md, WorkLeft-02-teaching-chinese-model.md, WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md, WorkLeft-MECH-01-mechanism-validation.md, WorkLeft-PROMPTS.md, WorkLeft-TASK-PROMPTS-TRAINING.md and other actual WorkLeft* inventory. Follow WorkLeft-RAG-MASTER-INDEX.md → WorkLeft-RAG-00-usage-guide.md → task-relevant RAG-01/02/03/04 in BOTH MD and NDJSON → actual referenced G/F/V primary files. In F read DECISIONS-LOCKED.md D-04 (intent != subject), D-05 (project LoRA), D-06 (historical replay conflict), D-07 (StackDetector before normalization), D-08 (source stratification), D-10 (BM25+kNN), D-14 (training cadence) and D-15 (arbiter). Follow V/RAG-03 translation, RAG-06 DynamicFlow, and G/active-codex-execution-plan-09-auto-dpo-loop.md. Do not claim a whole-recursive RAG crawl without visited primary receipts.

Reuse R/previous rounds thinking results.md with R07 status correction; PARALLEL-R01-R03-JOIN.md (historical name; PARALLEL-R01-R25-CONTRACT governs 25 starts and R10 nine inputs); R01-R25-TO-CODE-SLICES-STRATEGY.md; relevant R/results/R05/R06/R07/R08/R12/R13/R18/R21/R22/R23/R24/R25. R12 A–F is design only, R13 needs repair and separate Astra R25 remains not performed. Existing RP01/02/03/04/05/09/10/12/13/18 and S19–S23 are **implementation owners**, not already executed algorithms.

## Inspected K anchors (scoped, not full code review)

- K/14.ContinuousLearning/Feedback/FeedbackRouter.cs blob a676058b1ffcaa19dd6385608ec8f1ef242b3822 (read portions 1–32 and 117–248): Accepted adds positive example; Edited builds generic pair UserCorrection vs ModelResponse with per-side authorship and governance; Rejected uses no-preferred sentinel. This is a candidate mismatch with Chinese-both requirement, **not proof of bypass**, because other admission validation exists.
- K/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs blob 24ea6a7a341e49b2756220bf0848113fc9b1597f, scoped inventory: transactional pending DPO data → IncrementalTrainer → registry; not evidence ordinary host invokes it.
- K/18.CapabilityTraining/LlmIngredientLogicalUnits.cs blob b3951e7893cb83d6b5230360c764da04ee4b3b1b, scoped inspection: prepared DPO path exists, full pair/loss/response-only correctness belongs RP01/R18.
- K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs blob 2957816a88aa9cdcd3020bb81be56b5dfff165e0 scoped: index searches neighbors, ExecuteAsync uses first hit for output; RP02 owns actual multi-hit solution.

Further paths only **routes, resolve anew**: K/00.Core/LogicalContracts.cs, 01.Tokenizer/BpeTokenizer.cs, 06.VectorSearch/{IVectorIndex,HnswIndex,BruteForceIndex}.cs, 23.SemanticCompilerAdapter/TrainingVectorBuilder.cs, 25.ModelServer/ContinuousLearning/CodeDocumentationOneLessonEndpoint.cs, normal feedback origin/caller/DI and actual cold model loader. Before changing K read whole file, interface, all upstream callers, downstream consumers and affected tests. A source path disappearing must be reported as NEEDS_SOURCE, never invented.

## Every actual work receipt

Record source root/ref/sha/line, READ_FULL|READ_SCOPED|INDEX_ONLY|NOT_READ, domain law authority + project, code producer→input→state→consumer, owner, original requirement, current vs proposed, one falsifier and **unvisited routes**. RAG index-only is not source-proven. S27 may only own an actual proved missing feedback-evidence transfer edge. Other gaps go as exact packet to RP owners; no Rxx sibling file rewrite.
