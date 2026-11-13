# S32 — direct source map, run-time resolution and distinct owners
## Source snapshot observed while authoring this plan
Planning branch `lubochka/xiigen-planning@llm_plan3` was observed before publication at `d733d3b3140b9caf6443c8e2a04e5404edd21288`; Core `lubochka/llm_mvp_core@codex/dpo-implementation` at `d81a2f29a6f3a13e591567ae0653d1878a996472`. Future tasks MUST repin both, record blob SHA, read-depth and primary evidence. Author inspected directory inventories, scoped K contracts and selected S28/RP11/RP13/RP18/WorkLeft/RAG routes; **not** the entire 481k-character previous-rounds transcript, every WorkLeft/RAG primary or every K caller. No comprehensive current K runtime execution was performed.

P roots: D=`llm mvp/design to code session`; R=`D/replanning`; G=`llm mvp/dpo legal build`; F=`llm mvp/llm dynamic flow`; V=`llm mvp/visual studio & visual studio code continue`. The WorkLeft RAG index is NOT a primary semantic read. Traverse `D/09.09.26WorkLeft-RAG-MASTER-INDEX.md` → `D/09.09.26WorkLeft-RAG-00-usage-guide.md` → question-matched `RAG-01/02/03/04 .md + .ndjson` → actual G/F/V original, with distinct evidence/ref/line. Always read `D/original task.md`, `task prompt.md`, `goal of the round.md`, `Алгоритм создания ДПО.md`, `09.09.26WorkLeft-02-teaching-chinese-model.md`, `09.09.26WorkLeft-MECH-01-mechanism-validation.md`, `09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md`, `09.09.26WorkLeft-TASK-PROMPTS-TRAINING.md` insofar as task touches them. `R/previous rounds thinking results.md` has historical/status caveats, and `R/results/Rxx/*` has per-round direct evidence; neither is a permission to rewrite research results. Visit `PARALLEL-R01-R25-CONTRACT.md` and `R01-R25-TO-CODE-SLICES-STRATEGY.md` for ownership.

## Selected actual code anchors; no correctness or gain inferred
- `K/00.Core/DynamicGraphContracts.cs`: `GraphNodeCandidate(Confidence, Score, OutputRefs, ProvenanceRefs,...)`, typed `IQueryable` many→many contract and D-15 non-authority. **Raw Score/Confidence semantics not established**; trace concrete writer→reader.
- `K/00.Core/LearningMachineContracts.cs`: `LearningBatch<TDomain>`, `LearningBatchResult`, `LearningEvaluationSet<TDomain>`; machine owns Train; not automatically a calibrated result.
- `K/21.AgentInfrastructure/Logical/PromptDomainClassifier.cs`: CODE_Q/BUG/PLAN/ARCH/REFACTOR are D-04 *intent* labels, not guaranteed SUBJECT.
- `K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs`: source audit in S28 reports Search(query,k) then best=hits[0] in one consumer; must retrace actual ref before diagnosing as current. RP02 owns any fix.
- `K/15.OrchestratorModel/Logical/DynamicGraphLogicalOrchestratorRuntime.cs`, `15.OrchestratorModel/Logical/DynamicGraphLogicalOrchestratorCompositionExecutorAdapter.cs`: candidate real bounded async consumer, not presumed to combine scores.
- `K/11.DPO/DpoTrainer.cs`, `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`, `10.LoRA/LoraLinear.cs`: actual Student material/objective/recipient needs current scoped trace; DPO outcome not a probability of lawfulness.
- Source authority: `F/DECISIONS-LOCKED.md` and `F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md` via correct RAG primary; **reverify** before claiming current D-04..D-15. Project system instructions `K/AGENTS.md`, `K/CLAUDE.md` and task-matched skills.
- Existing S/RP plans `R/Kiro-Xiigen-RP02-...`, `RP03`, `RP04`, `RP09`, `RP10`, `RP11-Causal-Hybrid-Credit...`, `RP13-Causal-Hybrid-Policy...`, `RP18-Semantic-Transposition...`, `Kiro-Xiigen-S22-...`, `S23`, `S24`, `S25`, `S26`, `S27`, `S28`. Their implementation remains unproven by their README.

## Explicit owner assignment
**S32** = only absent *heterogeneous evidence calibration/qualification envelope→existing final decision/abstention consumer* seam; at most one adapter with actual writer grant.
**RP02/RP18** = subject mapping, law-sensitive vector/token/geometry, dynamic kNN, projection.
**RP03/04** = Unit/Flow associations and async Orchestrator/higher-order Graph route, scheduling.
**RP11** = online credit/reliability learning and cost model; **RP13** = when/where to train; **RP12** = root-cause repair; **S28** = actual lawful path hard-negative admission.
**RP01/RP05/S23** = authorized Chinese pair/Student Step, continual feedback, state/no-repeat/recovery.
**RP09** = blind evaluation and scientific study; **RP10/RP06** = promotion and cold ordinary host.
**S17/S19/S20/S21/S22/S26** = law drift, interference, composition, novelty and withdrawn evidence.

Return source ledger `repo,ref,path,blob_sha,lines_or_method,read_depth(READ_FULL/READ_SCOPED/INDEX_ONLY/NOT_READ),actual_what_proves,unknown`. A `tree` listing establishes existence, not implementation. Old SHA supports a source observation only, never latest runtime behavior.
