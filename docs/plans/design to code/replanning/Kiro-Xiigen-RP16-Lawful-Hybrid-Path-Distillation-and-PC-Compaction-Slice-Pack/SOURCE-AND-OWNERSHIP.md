# RP16 — sources, actual observed code anchors, owners

P=lubochka/xiigen-planning@llm_plan3; D=P/llm mvp/design to code session; R=D/replanning; G=P/llm mvp/dpo legal build; F=P/llm mvp/llm dynamic flow; V=P/llm mvp/visual studio & visual studio code continue; K=lubochka/llm_mvp_core@codex/dpo-implementation.

Read exact relevant D/original task.md, task prompt.md, goal of the round.md, Алгоритм создания ДПО.md; D/09.09.26WorkLeft-PROMPTS.md, RESEARCH-01-training-quality-speed-accuracy-program.md, RESEARCH-03-dpo-algorithm-conformance.md, other scoped 09.09.26WorkLeft*; D/09.09.26WorkLeft-RAG-00-usage-guide.md → D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → applicable RAG .md/.ndjson **row** → actual G/F/V primary source (NOT just index). Read R/previous rounds thinking results.md, R/PARALLEL-R01-R03-JOIN.md with current PARALLEL25 override; scoped results/R04,R05,R06,R08,R13,R18,R22,R23,R24,R25 FINDINGS/SOURCES/DECISIONS and related original S00–S10 obligations. Read relevant RP01–RP14 packages; re-check later RP13/RP15 live state before any product changes.

K candidate code (NOT proven end-to-end): 00.Core/LogicalContracts.cs; 00.Core/LearningMachineContracts.cs; 21.AgentInfrastructure/Logical/{AdaptiveLogicalOrchestrator.cs,BoundedCombinationGraphCandidateRunner.cs,DynamicGraphEdgeFlow.cs,LogicalRegistryDynamicGraphProjector.cs,LogicalUnitDynamicGraphNode.cs,KnnLogicalUnit.cs}; 06.VectorSearch/IVectorIndex.cs; 11.DPO/DpoTrainer.cs; 14.ContinuousLearning/Training/DpoTrainingExecution.cs. Locate fresh full relevant methods, their *real upstream callers and ordinary downstream consumers*. Record root/branch/HEAD/path/blob/line/read-depth; if source cannot be opened mark NOT_VISITED or INDEX_ONLY, never pretend full RAG traversal.

## Direct scoped code anchors (NOT complete code/consumer audit)

| K file | Observed blob on 13.10.2026 | Why read / limits |
|---|---|---|
| 00.Core/LogicalContracts.cs | fbba6f97f226d96d0d36ed08d6befa8338e86434 | ILogicalUnit request/result, typed flags; must trace actual callers |
| 00.Core/LearningMachineContracts.cs | 7f605d49762e3fc0c1b4d4ca490596bc9668cdd5 | Train owned by ILearningMachine, not unit/flow |
| 21.AgentInfrastructure/Logical/KnnLogicalUnit.cs | 2957816a88aa9cdcd3020bb81be56b5dfff165e0 | trained BPE+HNSW bridge, actual multi-hit relevance unproven |
| 21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs | cca8e27b703559338aefa42f14e63d2b8c520d18 | >220kB potential Graph controller; full body/consumer not verified here |
| 21.AgentInfrastructure/Logical/BoundedCombinationGraphCandidateRunner.cs | 8099c2c68d3977f61ddf15ada77b89451f4a499a | bounded Graph candidates, branch cost/selection candidate |
| 21.AgentInfrastructure/Logical/DynamicGraphEdgeFlow.cs | cd699caf28994fd220b092280f97bde64ccb3ec0 | typed edge refs and fan-in, not proof of RP16 macro |
| 21.AgentInfrastructure/Logical/LogicalRegistryDynamicGraphProjector.cs | 3fbc558144809d649551312b902cc9b6374afbaa | registry projection, must check reuse before new logic |
| 11.DPO/DpoTrainer.cs | 9c358978075db8c22fe0125eaea9e89e831d5635 | existing trainer, not owned RP16 |
| 14.ContinuousLearning/Training/DpoTrainingExecution.cs | ab5bb45ca15a86a206952f2516a32119c67efde9 | existing runtime executor; no DPO run claimed |

These are **selected scoped GitHub observations** made to plan the package, NOT full traversal of enormous sources, proof of absence of macro/ordinary user use, or current live SHA for a later editor. Every work packet demands fresh repo HEAD/path/blob/lines, applicable **full target method + callers + consumers**, sources and read depth. Large indexed `WorkLeft-RESEARCH-01` (>500k chars) has only scoped excerpts reviewed in this authoring session.

| Owner | Reuse, do not rebuild | RP16's unique action |
|---|---|---|
| RP01/R05/R18 | Student DPO/LoRA, exact pair/objective, trainer and feedback | Keep separate, optionally isolate causal interaction |
| RP02 | multi-axis mapping, lawful semantic token/vector, adaptive kNN/multi-hit | hard macro eligibility adapter only |
| RP03 | typed many→many Unit, learning Flow correction | preserve actual typed associations/OutputRefs |
| RP04 | async Orchestrator and higher-order Graph/routing | conditional new bounded compact-path candidate in existing consumer |
| RP06, historical S08–S10 | DTC/IDE and native release | handoff selected normal local user evidence |
| RP07 | Builder and learner curriculum | admitted weak signal, not duplicate lesson author |
| RP08 | hybrid cross-domain transfer | measure macro negative transfer |
| RP09 | independent blinded functional examiner and resource ledger | reuse actual runner; no new evaluator |
| RP10 | compatible full-state promotion/cold selected pointer | child macro state/cold check, not another pointer |
| RP11 | learned causal route credit/cost policy | supply new lawful candidate, not another policy learner |
| RP12 | causal error diagnosis/minimal repair selector | use isolated failure, not redo all analysis |
| RP14 | law drift and version rebinding | invalidate old macro by lawful version authority |
| R01–R25 | parallel research/planning results | read-only references, no rewrite |

Source receipt columns: `root,branch,HEAD,path,blob,lines,original_requirement,RAG_row,primary_target_visited,read_depth,actual_method,caller,ordinary_consumer,learned_owner,missing_fact`. RAG index/filename alone = INDEX_ONLY; no sourced fact without visited target. R01–R25 each keep exclusive results/Rxx writers.