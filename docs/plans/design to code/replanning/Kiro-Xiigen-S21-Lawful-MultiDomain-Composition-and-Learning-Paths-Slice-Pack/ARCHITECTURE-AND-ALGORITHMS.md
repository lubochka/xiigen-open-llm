# S21 architecture — typed lawful composition, not another framework

## The concrete capability
One query Q requires multiple subjects with ordered dependencies. Proposed only-as-needed envelope:
- CompositeRequest: original spans, D-04 taskIntent, project/tenant/rights, stage DAG, source-family and final typed target.
- StageScope: subjectDomain, stack, dialect/edition, translation direction, law evidence, schema, tokenizer/codebook/index versions, admissible effects and project-compatible Student LoRA.
- StageHandoff: producer, typed output association/OutputRef, input snapshot, target consumer, ordered dependency, law authority and content-safe provenance/hash.
- CompositeRouteCandidate: chosen lawful stage variants, real path/order, rejected routes and reasons, individual and total CPU/RAM/time, confidence/unknown, hard veto and ordinary consumer output.
These are **proposal names**, not claims that code already exports such records. 00B/01B must reuse native contracts and supply exact real integration evidence.

## Hybrid algorithm preserving the four architecture roles
1. **Deterministic law binding:** stack detection before normalization (D-07). Separate D-04 intent from subject C#/SQL/EN→RU, project, dialect and version. Check rights, schema, side effects, negation/units and dependent ordering. Wrong/unknown law -> abstain/clarify.
2. **Semantic coordinates + adaptive retrieval:** RP02/RP18 own domain-law typed tokenizer/vector and versioned projection, RP02 owns lawful multi-neighbor adaptive kNN and BM25+kNN (D-10). Different per-stage admissible candidate pools and functions may be optimal; exact/ANN comparison and top1 vs multi-neighbor must use their actual consumers. Legal negative warnings can be retrieved but not executed as positive actions.
3. **ILearningUnit many→many:** real typed outputs with source input, OutputRef scope and association ID; no silent first-candidate or unrelated multi-result selection.
4. **ILearningFlow:** corrects information passed to a **real next Unit**; relation state may learn preference but cannot invent legal authority. Freeze Units for causal Flow experiment.
5. **ILearningOrchestrator:** asynchronously explores bounded lawful combinations respecting DAG predecessors, side-effect/approval gates, cancellation and full branch cost; compare with deterministic topological routing.
6. **ILearningDynamicGraph:** higher-order patterns link permitted relationships across domains; preserve causal order for noncommutative paths and allow order-free projection only for truly commutative evidence merges. Owner is existing ILearningMachine<DynamicGraphDomain> where real state exists; don't call a static registry trained.
7. **Student and LoRA:** selected existing per-project (D-05) adapter improves response via real RP01 response-only DPO when justified, not by re-encoding arbitrary vectors. LoRA↔semantic projection is RP18's separately measured port.
8. **Final deterministic arbiter:** D-15 vetoes invalid output regardless of neighbor score, learned graph route or PC pressure. Feedback and replay use RP05/S19/RP10 owners.

## Suspected but UNPROVEN issue
In K/21.AgentInfrastructure/Logical/DynamicGraphEdgeFlow.cs, NodeOutputToEdgeInputAdapter includes UpstreamCandidateRefs and an order-free projection rule; LogicalUnitDynamicGraphNode and LogicalRegistryDynamicGraphProjector supply neighboring graph boundaries. A sorted candidate identity may be valid for commutative evidence, but would be unsafe if it were used to equate C#→SQL and SQL→C# dependent transformations. S21.00B must show an *actual* order-sensitive consumer and counterexample before code edits. Otherwise keep it and REUSE_NO_CHANGE.

## Example: three lawful alternatives
Q: update .NET C# request DTO, align SQL Server migration and write RU API docs.
- GOOD: preserve nullable property and SQL Server dialect, update DB only after verified DTO dependency and approval; RU correctly explains optional field.
- BAD: wrong-stage OutputRef or PostgreSQL LIMIT enters SQL Server query; RU looks fluent but repeats wrong schema.
- EDGE: SQL dialect missing, target schema absent, "must not cache" negated, two equally good lawful routes, output refs stale or partial cancellation.
Hard-law correctness is deterministic, but semantic case mapping, route ranking, unknown calibration and unseen composition may benefit from learning. No universal k or learned Graph superiority assumed.

## Novelty / owner test before every patch
If existing RP03/RP04 typed ordering works at ordinary consumer, S21 owes no graph patch. If RP02 already maps law-aware multi-neighbor correctly, reuse. If RP18 already transposes a particular vector with lawful proof, consume versioned port. S21 only repairs an evidenced **inter-subject stage handoff used in one ordinary composite request**. All alternatives: exact caller, schema mismatch, local patch owner and falsifier in 00B/01B.
