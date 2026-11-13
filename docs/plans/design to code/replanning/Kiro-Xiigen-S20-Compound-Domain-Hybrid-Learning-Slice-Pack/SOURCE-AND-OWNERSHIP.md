# S20 sources and ownership — current observations, limits and mandatory RAG descent

## P/K refs observed during authoring, refresh when executing
P=lubochka/xiigen-planning branch llm_plan3 was observed at b9a4c5f0e3bfdb44a9dc538fd1490d7dec055c28.
K=lubochka/llm_mvp_core branch codex/dpo-implementation was observed at 3c7f5f8c7809936e25579603c96ea6df8598fc40.
Both are **historical evidence**, not reset targets; publication itself advances P. Before EACH task re-read live AGENTS/CLAUDE, owners, affected file blobs, branch vs origin/main, preserve other writers.

D=P/llm mvp/design to code session; G=P/llm mvp/dpo legal build; F=P/llm mvp/llm dynamic flow; V=P/llm mvp/visual studio & visual studio code continue.

**Direct source scope during S20 authorship:** read actual current D original task/task prompt/goal/DPO algorithm (selected portions), WorkLeft-02/MECH-01/RESEARCH-03, RAG-MASTER and RAG-00, RAG-01/02/04 NDJSON (selected portions), current R20 FINDINGS, R22/R23 and S17/RP08–RP15 sections. Inspected G/active-codex-execution-plan-09-auto-dpo-loop.md and F/DECISIONS-LOCKED.md plus F/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md actual portions. **V actual primary ordinary endpoint not established**; RAG entry is INDEX_ONLY. Historical previous rounds thinking results (~481k chars) examined in relevant opening/R07 correction only; it is NOT a completed whole-file audit. K skills relevant sections inspected, not all 194 files.

## Source and code anchors directly inspected (13 Oct)
- K/00.Core/LogicalContracts.cs around 104: ILogicalUnit<TInput,TOutput> ExecuteAsync; does not alone prove many→many ordinary use.
- K/00.Core/LearningMachineContracts.cs around 202+: ILearningMachine TrainBatchAsync/EvaluateAsync, machine owns learner.
- K/00.Core/DynamicGraphContracts.cs around 21–85: typed queryable GraphNodeCandidate, OutputRefs, GraphEdgeKind many-to-many.
- K/21.AgentInfrastructure/Logical/PromptDomainClassifier.cs around 1–90: D-04 BUG/PLAN/ARCH/CODE_Q/REFACTOR are **developer-task intent**, not C# version, SQL dialect or language locale.
- K/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs around 202–250: _index.Search(query,_k), but returned best output is **hits[0]**, even when evidence carries other hits. Multi-neighbor **prediction** not proven there; RP02 owns generic fix.
- K/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs uses width 60 versus KnnLogicalUnit BPE/predicate coordinates, not automatically comparable and NOT LoRA.
- K/06.VectorSearch/HnswIndex.cs Search method and K/11.DPO/DpoTrainer.cs constructor/mutation admission portions.
- These are scoped **source observations**, not current claim that every alternative core class/adapter was audited or absent.

## Mandatory per-task, scoped, direct-first progression
D/original task.md; D/task prompt.md; D/goal of the round.md; D/Алгоритм создания ДПО.md; D/09.09.26WorkLeft-02-teaching-chinese-model.md; D/09.09.26WorkLeft-MECH-01-mechanism-validation.md; D/09.09.26WorkLeft-RESEARCH-03-dpo-algorithm-conformance.md
D/09.09.26WorkLeft-RAG-MASTER-INDEX.md → RAG-00 usage guide → affected RAG-01/02/03/04 .md AND .ndjson row → real G/F/V primary paragraph → live K full caller/method/consumer
Then affected Rxx/current results/FINDINGS/SOURCES/checkpoint; relevant RP01–RP15/S17 owner and current K code full method+upstream DI/caller+downstream ordinary consumer. For every claim record repo/ref/path/blob/line or section, read depth FULL/SCOPED/INDEX_ONLY/UNREAD, source-rights authority, evidence and actual effect. RAG and historical report cannot be promoted to code or independent measurement.

## Non-overlap table
| Owner | Preserved component | S20 adds, narrowly |
|---|---|---|
| RP01, RP05 | existing DPO trainer, Student/LoRA, feedback ledger and recovery | mixed-domain residual weakness exam only |
| RP02 | domain mapper, subject IR/token/vector, lawful multiple neighbors and adaptive kNN | simultaneous-law binding and cross-domain consumer |
| RP03 | typed many-to-many Unit and correcting Flow | composition constraint at real typed join |
| RP04 | async Orchestrator and higher-order DynamicGraph | legality of compound route, no rival scheduler |
| RP06, RP08 | DTC/IDE host; generic multi-domain transfer | one ordinary user compound join, matched law exam |
| RP07 | Builder candidate proposal | never human case selector |
| RP09, RP10 | independent experiment runner; PC state activation | joint-law rubric and negative-transfer signal |
| RP11–RP13 | counterfactual credit, causal repair and policy | compound path evidence, not another policy |
| RP14, RP15, S17 | law-version drift and state compatibility | simultaneous *different subject* law conjunction |
| Historical S00–S10 | training/integrity/host/native release | do not silently cancel or replace |
| R01–R25 | 10+5+5+5 independent research/replanning shards | historical baseline only, no fabricated PASS |

**Unresolved to executor:** exact compound ordinary consumer, current V host primary, actual law-specific semantic encoder, whether RP08 already implements the join, independently sourced law passports, current Student and resource grants. No implementation verdict or machine grant follows from this authoring receipt.
