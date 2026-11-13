# S23 current source routes, source depth and ownership

## Repo identity and exact inspected anchor (13 Oct 2026)
P = lubochka/xiigen-planning branch llm_plan3; D=P/llm mvp/design to code session; R=D/replanning; G=P/llm mvp/dpo legal build; F=P/llm mvp/llm dynamic flow; V=P/llm mvp/visual studio & visual studio code continue; K=lubochka/llm_mvp_core branch codex/dpo-implementation.
Observed before this pack: P HEAD 8ff9db877faec4e1ec99fe9f8e7ebc7a839e182c and K HEAD 3c7f5f8c7809936e25579603c96ea6df8598fc40. New P publication necessarily changes P HEAD; actual executors MUST repin both branches. Directory inventories were INDEX_ONLY; selected sources below were READ_SCOPED, not a claim of whole-project audit.

## Mandatory RAG → primary descent for EVERY future dispatched task
D/original task.md + task prompt.md + goal of the round.md + Алгоритм создания ДПО.md;
D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → D/09.09.26WorkLeft-RAG-00-usage-guide.md → selected RAG-01 (DPO legal), RAG-02 (DTC), RAG-03 (branches), RAG-04 (VS/VS Code/Continue) MD and NDJSON → REAL referenced G/F/V file + required paragraphs. Also D/WorkLeft-PROMPTS.md, TASK-PROMPTS-TRAINING.md, WorkLeft-02-teaching-chinese-model.md, WorkLeft-INTEGRATION-01, MECH-01, RESEARCH-03, PLAN-HUMAN-01, 03-decisions-register.md as relevant. Index-only ≠ law proof. Record READ_FULL/READ_SCOPED/INDEX_ONLY/NOT_READ and unread source/risk.
R/previous rounds thinking results.md, PARALLEL-R01-R03-JOIN.md, PARALLEL-R01-R25-CONTRACT.md, results/R01–R25 (only actual relevant findings, decisions, sources, logs); R23-PC-STATE-FEEDBACK-LIFECYCLE.md and R23-REPAIR-WORK-ORDERS.md §V2 are the nearest owners. R18 G1–G6, R22 C01–C11, R21 domain passports, R24 ordinary DTC/IDE, R25 independent review; S17/S19 existing packs, RP01/02/03/04/05/09/10 are references, not completed implementations.

## Directly inspected current K code spans, NOT performance proof
- K/14.ContinuousLearning/Feedback/FeedbackRouter.cs (blob a676058b1ffcaa19dd6385608ec8f1ef242b3822), first 140 lines: Accepted/Edited/Rejected and distinct generic UserCorrection authorship.
- K/14.ContinuousLearning/Pipeline/ContinuousLearningPipeline.cs (blob 24ea6a7a341e49b2756220bf0848113fc9b1597f), first 140: RunCycleAsync, cache, domain transaction and adapter owners.
- K/14.ContinuousLearning/Feedback/FileDpoPairStore.Transactions.cs (blob a46a003a986ab395b342f8fe33f91156b4f583c5), first 140: transaction/replay ledger.
- K/14.ContinuousLearning/Training/CodeDocumentationRepositoryOwnedDpoTrainingBaselineAttemptJournal.cs (blob c890034dfa3fee0e21f06d15d230fed228a22c8c), first 140: baseline marker explicitly not weights/optimizer/RNG recovery.
- K/14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs (blob 3835b1db3207f1bfd656956d0e635a53bee64ed7), first 140: model+dataset+runtime compatibility fingerprint.
- K/14.ContinuousLearning/Training/IncrementalTrainingSession.cs (blob 542d7dd8167af8f631dddae3cea3dc41575d33a2), first 140: distinct resumed parent vs explicitly authorized cold start.
- K/25.ModelServer/ContinuousLearning/CodeDocumentationOneLessonEndpoint.cs (blob 953cedf7128b3adc6ada1279dcb359a4fb9157a1), first 140: OneLesson preflight endpoint and owned grant catalog, NOT proof all ordinary requests use it.
- K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs (blob 2957816a88aa9cdcd3020bb81be56b5dfff165e0), first 140: BPE/HNSW training and k configuration, not proof actual multi-neighbor decision.
- F/DECISIONS-LOCKED.md was directly read: D-04/D-05/D-06/D-07/D-08/D-10/D-14/D-15 preserved.
Actual ordinary host, consumed trained state, recovery completeness, numbers, device and independent Astra review still UNBOUND / NOT_MEASURED. Never describe above READ_SCOPED evidence as end-to-end proof.

## S23 owner-by-seam table
S23.00A/00D source+host investigator; .00B/.02* K existing transactional Student/recovery owner; .00C/.01* local PC/operations owner; .03A/03B RP05+RP01 CODEX/Chinese/policy owners; .03C S19/RP05 retained bulks; .04A/.04B RP09/R22 independent exam; .04C K isolated failure verification; .04D RP10/R24 user-visible host; .05A exactly ONE found code gap owner; .05B independent R25-style Astra review. Multi-writer state must be serialized. No S23 task may claim old slice achieved a behavior without exact current receipt.
