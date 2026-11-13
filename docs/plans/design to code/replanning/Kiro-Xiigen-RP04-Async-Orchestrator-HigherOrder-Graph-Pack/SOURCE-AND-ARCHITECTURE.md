# RP04 — sources, actual seams, aliases and architecture authority

## 1. Snapshot and evidence status
Discovery pins observed 2026-10-13: P=xiigen-planning, branch llm_plan3, HEAD e1541142c420ab923aae62ec181210cb867ec915; K=llm_mvp_core, branch codex/dpo-implementation, HEAD 3c7f5f8c7809936e25579603c96ea6df8598fc40. A later executor **must re-read branch HEAD and affected complete files**, reconcile against current main, and record file blob SHAs. Paths below were discovered from direct GitHub directory/selected file reads. These are not proofs that every WorkLeft line, RAG entry or K caller has been audited.

Aliases: D="llm mvp/design to code session"; R=D/replanning; G="llm mvp/dpo legal build"; F="llm mvp/llm dynamic flow"; V="llm mvp/visual studio & visual studio code continue"; K=llm_mvp_core.

## 2. Original requirements and source routes
| Lane | Required primary reads / priority | RP04 use |
|---|---|---|
| D original | D/original task.md; D/Алгоритм создания ДПО.md; D/task prompt.md; D/original task.md human goals; D/09.09.26WorkLeft-03-luba-decisions.md | Many→many Unit, Flow corrections, async Orchestrator, multi-order DynamicGraph, learner + Builder distinction |
| WorkLeft | D/09.09.26WorkLeft-00-ИТОГ.md; -01-code-changes.md; -02-teaching-chinese-model.md; -03-decisions-register.md; -MECH-01-mechanism-validation.md; -RESEARCH-01-training-quality-speed-accuracy-program.md; -RESEARCH-03-dpo-algorithm-conformance.md; -RAG-00-usage-guide.md; -RAG-MASTER-INDEX.md | Baseline and unresolved contractual findings. The large files require separate bounded deep owner reads, not blanket "fully reviewed" |
| G graph and DPO | G/active-codex-execution-original-task.md; -ILEARNINGDYNAMICGRAPH-PLAN-v1.md; -plan-01-node.md; -plan-02-edge.md; -plan-03-orchestrator.md; -plan-04-dynamicgraph-machine.md; -plan-06-async-combinations.md; -plan-07-dpo-builder.md; -plan-08-legal-teacher-gate.md; -plan-09-auto-dpo-loop.md; -plan-10-training-lifecycle.md; -plan-12-no-hidden-determinism.md | Historical designed L2/L3/L4/L5 pattern/async contracts, including multi-combination DPO provenance. Read sections against current code |
| F dynamic flow | F/DECISIONS-LOCKED.md (especially D-04, D-09, D-10, D-13–D-15); F/DYNAMIC-FLOW-LOCKED-DECISION-D13-AUTHORITY-v1.md; F/E53-HIGH-LEVEL-ORCHESTRATION-PROOF-PLAN-v1.md; F/E53-SCENARIO-001-SOURCE-PACKET.md; F/E53-SCENARIO-001-EXPECTED-TRACE.md | Law/arbiter/priority/kNN boundaries and difficult async no-stack scenario; historical claimed metrics are NOT RP04 measures |
| V IDE | V/RAG-02-orchestrator-extension.md; V/RAG-06-llm-dynamic-flow.md; V/RAG-08-orchestrator-experiment.md; V/PLAN-VS-VSCODE-CONTINUE-v1.md | Native ordinary user consumer/host restrictions and no training runtime in constrained plugin binaries |
| Current R findings | R/results/R01–R25/FINDINGS.md, R22-CAUSAL-EXPERIMENT-CATALOG.md, R23-PC-STATE-FEEDBACK-LIFECYCLE.md, R24-CAUSAL-REPAIR-2026-10-13.md, R25-ASTRA-REVIEW-PACKET.md; R/previous rounds thinking results.md; R/PARALLEL-R01-R03-JOIN.md | Evidence with actual status; reconcile per owner, never promote documentary synthesis to code/training |
| Prior product slice | R/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/README.md, tasks/RP01.04C.md, tasks/RP01.04D.md and GOVERNANCE-AND-REVIEW.md | 04C/04D are READ_ONLY_FORWARD_HANDOFF and not RP01 E1 executable. RP04 owns 04D E3/E4 implementation work, not historical task re-execution |

**Visit ledger rule:** record repo/branch/HEAD/path/blob, source kind original vs RAG pointer vs code, actual section or line range, scope finding, successor, contradictory source and owner. For any claim, resolve RAG → earliest applicable primary → current K implementation/caller. Deduplicate visited spans; use query-bounded reads by task instead of demanding every gigabyte before first runnable scheduler.

## 3. Current K code anchors (directly inspected at discovery, revalidate)
| File | Current evidence | Do not assume |
|---|---|---|
| K/00.Core/LogicalContracts.cs | LogicalUnitRequest<TInput>, LogicalUnitResult<TOutput>, ILogicalUnit, LogicalFlowRequest/Result, LogicalContextPacket, LogicalOrchestratorRequest/Result, BranchResults, ILogicalOrchestrator | not automatically IQueryable all types, not proof of learned route state |
| K/00.Core/DynamicGraphContracts.cs | IDynamicGraphNode<TInput,TOutput> with IQueryable<GraphNodeCandidate> output, GraphNodeCandidate OutputRefs/PathOrderRef, typed edge handoff | OutputRefs already resolve to demanded payload at every caller; verify resolver |
| K/00.Core/LearningMachineContracts.cs | ILearningMachine<TDomain> TrainBatchAsync/EvaluateAsync/ExportAsync/ImportAsync/GetOrchestratorAsync | no permission to add separate E3/E4 trainers just because four functional roles |
| K/15.OrchestratorModel/Logical/DynamicGraphMachine.cs | RuntimeMode "honest_untrained", explicit DYNAMIC_GRAPH_*_NOT_ACTIVATED failures for Train/Evaluate/Export/Import; GetOrchestratorAsync factory path works | NOT a trained graph. This is the exact E4 gap |
| K/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs | RunAsync, RunWithTopologySnapshotAsync, RunWithInvocationNodeOutputsAsync, BranchResults/contexts, knowledge-edge retrial | not evidence that branch policy is already learned; must inspect all branches/callers |
| K/15.OrchestratorModel/Logical/DynamicGraphLogicalOrchestratorRuntime.cs | runtime/topology/preload operations; actual creation requires lawful projection/snapshot | topology application itself is not learning |
| K/15.OrchestratorModel/Logical/DynamicGraphCandidateInvocationOrchestrator.cs | one-shot invocation bridge, axis sweep candidates, runner failures, correlation/cleanliness checks | do not reinterpret one async run as many independent combinations |
| K/15.OrchestratorModel/Logical/DynamicGraphLogicalOrchestratorCompositionExecutorAdapter.cs | composition sessions, checkpoint and selected target execution interfaces | do not change owned checkpoint/authority without full source/context and review |
| K/19.DynamicGraphComposition.Contracts/Logical/ | bootstrap profile, execution sessions, RunAsync/RunOnCurrentTopologyAsync contracts | no silent new public API |
| K/17.DynamicDomains/DynamicDomainGraph.cs | deterministic dependency closure, OrchestratorExpander | not learned higher-order state |
| K/15.OrchestratorModel/OrchestratorFacade.cs | legacy classify→decompose→sort→Task.WhenAll per level→merge pipeline | legacy fallback must not be silently replaced by graph model |
| K/18.CapabilityTraining, K/14.ContinuousLearning, K/10.LoRA, K/11.DPO | downstream/upstream integration candidates | optional integration only after direct current actual recipient/consumer trace |

**True current design question:** which existing callable path is the smallest canonical ownership seam for E3 route decision and E4 learned higher-order state? It may already be a trained edge authority unit. Examine that rather than invent a duplicate graph trainer. Exact candidate/relationship state owner, ordinary host caller and training admission must be source-proven.

## 4. Functional roles and actual proof boundaries
- ILearningUnit: domain/input-vector/typed output structure; many inputs → IQueryable of many real typed results; preserve input→output association, OutputRef materialization, coordinate version.
- ILearningFlow: typed relation/port/OutputRef corrections between Units; freeze Unit parameters to prove Flow intervention.
- ILearningOrchestrator: valid dependency/order/permissions/budget are deterministic; ranking or early-stop over permitted async branch combinations may be learned; include all launched/cancelled costs.
- ILearningDynamicGraph: type/order/authority of candidate higher-order pattern legalities are deterministic; preference for compositional relations may be learned; freeze lower Units/Flows/E3 to test unseen composition transfer.

A graph pattern is not an edge-only alias; a route scheduler is not the final arbiter. D-15: priority chooses trials, kNN retrieves arbiter history, arbiters accept/reject, external AI advises only if approved. Never override a rejection by score or speed.

## 5. Original design decisions that cannot disappear
D-04 CODE_Q/BUG/PLAN/ARCH/REFACTOR is a task-intent taxonomy, not a replacement for domain passports; no full enum rewrite. D-07 stack detector before normalization, D-08 stratified experiment splits, D-09 dynamic class max_weight rather than constant multiplier, D-10 BM25+kNN historical baseline, D-11 internal TensorCore primitive, D-12 canonical enum spelling, D-13 promotion with density/edge risk, D-14 external calls granted one-at-a-time and idle learning only where applicable, D-15 non-override arbiter. These historical decisions require version/scoped legal review on conflict with newer user instructions, not silent cancellation.

No change here claims an empirical speedup, trained policy, PC-fit, model call, native delivery or independent review.
