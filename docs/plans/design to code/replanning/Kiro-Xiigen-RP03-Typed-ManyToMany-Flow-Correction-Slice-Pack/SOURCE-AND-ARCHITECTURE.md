> **RP03 V7 IS THE ONLY ACTIVE DISPATCH:** [current dispatcher](RP03-V7-LEARNING-FIRST-DISPATCH-2026-10-15.md); [source/guard-cost task repair](RP03-V7-TARGETED-LEARNING-AND-GUARD-COST-REPAIR-2026-10-15.md). Older active labels below are historical evidence, not an instruction queue.

> **HISTORICAL V6.2 REFERENCE ONLY · superseded by V7 (15 October 2026).** Preserve its source background; do not dispatch tasks/v6.

> **HISTORICAL V6.1 REFERENCE ONLY · superseded by V7.** Do not treat old single-task guidance as an active executor contract.

<!-- RP03-ACTUAL-CODE-LEAN-SCIENCE-INTEGRATION-20261014 -->
> **RP03 CURRENT CODE-FIRST EXECUTION:** [real K seam / fastest Flow learning](RP03-ACTUAL-FLOW-LEARNING-FAST-LANE-2026-10-14.md) · [all 15 tasks minimum probes & domain exams](RP03-MINIMAL-TEST-AND-EXAM-MATRIX-2026-10-14.md). Confirmed source contrast: GraphNodeCandidate.OutputRefs are references, SourceIds are evidence identities, and sorted CandidateId hash is NOT typed payload. DynamicGraphEdgeFlow handoff is NOT a real downstream invocation; ILearningMachine TrainBatchAsync is DPO, not distinct Flow optimizer. Two parallel lanes prevent delaying first Flow weight update until complete typed/host integration. Planning/dispatch only: no executed weights, Teacher, Astra review or empirical measurements are implied.

# RP03 primary sources and actual K architecture

## Original/WorkLeft/RAG/round chain
P=xiigen-planning@llm_plan3; D=llm mvp/design to code session; R=D/replanning; G=llm mvp/dpo legal build; F=llm mvp/llm dynamic flow; V=llm mvp/visual studio & visual studio code continue; K=llm_mvp_core@codex/dpo-implementation. Original D/original task.md and D/Алгоритм создания ДПО.md define ILearningUnit(domain, vector/output structure, IQueryable many→many), ILearningFlow(unit relationships and corrections), ILearningOrchestrator(async input/output), ILearningDynamicGraph(higher-order patterns). These names are required behavioral roles, not instruction to duplicate existing ILogical APIs.

Read relevant bounded spans of D/task prompt.md; goal of the round.md; 09.09.26WorkLeft-00-ИТОГ.md; -01-code-changes.md; -02-teaching-chinese-model.md; -03-decisions-register.md; -MECH-01-mechanism-validation.md; -RAG-00-usage-guide.md; -RAG-MASTER-INDEX.md; -RAG-01..04.md + ndjson. FOLLOW RAG pointer→original primary G plans -01-node, -02-edge, -06-async-combinations, -07-dpo-builder, -08-legal-teacher-gate, -09-auto-dpo-loop, -12-no-hidden-determinism; F/DECISIONS-LOCKED and ARBITER-KNN-AUTHORITY-AMENDMENT, V IDE ordinary consumer RAG. Do not claim entire huge WorkLeft files read merely from listing.

Evidence: R/results/R04/FINDINGS.md F06 = real queryable/adapter, F07 = CandidateId hash is not payload resolution, F08 = association risks. R06 = four roles and frozen Unit Flow experiment. R12 design candidate, R13 experimental repair open, R21 domains, R22-CAUSAL-EXPERIMENT-CATALOG.md C07 = separate Unit vs Flow factors, RP01/tasks/RP01.04C.md READ_ONLY_FORWARD_HANDOFF. R/previous rounds thinking results.md and PARALLEL-R01-R03-JOIN.md (live PARALLEL-R01-R25-CONTRACT override) retain earlier obligations; R10 joins nine R01–R09 research inputs. The old Rxx work is attributed, not a current runtime pass.

## Current code anchors directly inspected on 13 Oct 2026 (re-pin current blobs)
| Current K file | Blob SHA | Observed vs not-yet-proven |
|---|---|---|
| 00.Core/LogicalContracts.cs | fbba6f97f226d96d0d36ed08d6befa8338e86434 | ILogicalUnit<TInput,TOutput> ExecuteAsync returns LogicalUnitResult<TOutput>; ILogicalFlow<TInput> ExecuteAsync returns LogicalFlowResult with optional EdgeTargetHandoff; no Train on either |
| 00.Core/DynamicGraphContracts.cs | 987c29396fab838db0eecd8060837d5e5e33bce4 | IDynamicGraphNode<TInput,TOutput> returns IQueryable<GraphNodeCandidate>, OutputRefs/OutputType/SourceIds, GraphEdgeKind five variants, IDynamicGraphEdge has L2 AuthorityUnitId |
| 00.Core/LearningMachineContracts.cs | 7f605d49762e3fc0c1b4d4ca490596bc9668cdd5 | A-LM-02: TrainBatchAsync on ILearningMachine, not Unit/Flow |
| 21.AgentInfrastructure/Logical/LogicalUnitDynamicGraphNode.cs | 781ba5afe61bd3dfa34049133fd4f334a710daa0 | Executes one Unit then snapshots projected GraphNodeCandidate[] as queryable; does not itself resolve OutputRefs |
| 21.AgentInfrastructure/Logical/DeterministicDynamicGraphNodeOutputProjector.cs | eced7aa0b4b917d603f31905fab2b5f09ef5971e | Projects hashes of candidate IDs, not semantic typed payload |
| 21.AgentInfrastructure/Logical/DeterministicDynamicGraphEdgeAuthorityInputMaterializer.cs | b61c5cd88dea9b818aa2cb55aa48d5000a1f788e | Edge input materialization, no actual typed payload execution |
| 21.AgentInfrastructure/Logical/DynamicGraphEdgeFlow.cs | cd699caf28994fd220b092280f97bde64ccb3ec0 | Deterministic L3 fan-in/ReturnLoop and learned-target handoff; distinct policy update not proven |
| 21.AgentInfrastructure/Logical/DynamicGraphEdgeAuthorityExecutionPipeline.cs | 70fcdcfbc824d9c0c71f66773136bb6c79fc580f | Learned L2 authority execution seam; confirm actual mutable recipient and cold serving |
| 21.AgentInfrastructure/Logical/DeterministicGraphFanInMerger.cs | 14adc1c9aef460200b140ae85ca0d799676952cc | Deterministic F0 baseline |
| 21.AgentInfrastructure/Logical/InMemoryLogicalUnitRegistry.cs; InMemoryLogicalFlowRegistry.cs | 8df425d2f11691d0e3ecf6167b22fcb0d06d2b41; cacefc348c2c61a1db1e8f3ac95dd50f18739148 | In-memory registration ≠ durable learned state |
| 21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs | cca8e27b703559338aefa42f14e63d2b8c520d18 | Ordinary L4 RunAsync, scheduler policy RP04 |
| 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs | 2957816a88aa9cdcd3020bb81be56b5dfff165e0 | Existing BPE/HNSW unit, retrieval improvements RP02 |

## Required next source proof
Before any product patch, search ALL constructors and callers of GraphNodeCandidate, read all OutputRefs producers and actual readers/resolvers, all next ILogicalUnit / DynamicGraphEdgeFlow / 15.OrchestratorModel / 50.ProductHost ordinary consumer paths and 18.CapabilityTraining actual relation-state owner. Trace each request_id→input_id→CandidateId→OutputRef→resolved TOutput schema/version/tenant/provenance→corrected next Unit TInput→user result. If unknown, report NO_CONSUMER or NO_RECIPIENT, not a fabricated code defect or fix. Pin current HEAD/blob/lines and callers before editing. Only claim runtime/training measurements when truly executed.
