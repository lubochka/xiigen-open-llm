# T34 Source Binding
# Phase 3 -> Phase 4 transition experiment
# Date: 2026-05-04

## Binding Statement

This experiment binds one upstream technical-design context to one downstream
implementation source packet.

Upstream Phase 3 source authority:

- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DYNAMIC-FLOW-HIGH-LEVEL-DESIGN-AMENDMENT-v1.md`
- `C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\DECISIONS-LOCKED.md`

Downstream Phase 4 source authority:

- `C:\Projects\llm_mvp_core\docs\plans\AUTONOMOUS-LEARNING-SESSION-PLAN-v1.0.md`

The upstream source is Phase 3 because it resolves the technical-design
problem: what the dynamic-flow architecture is, what decisions are locked,
which runtime topology rules are non-negotiable, and what the implementation
layer must preserve.

The downstream source is Phase 4 because it resolves an implementation
problem: which project, files, phases, tests, and gates should be built for
`16.AutonomousLearning`.

The transition tests whether decisions from the Phase 3 technical-design
context are preserved, acknowledged, or correctly marked out of scope in the
Phase 4 implementation plan.

## Upstream Context Packet

### Decisions Made

| ID | Decision | Source authority |
|---|---|---|
| DCTX-01 | The pipeline graph is data, not code; topology is stored in a registry and read at runtime. | Dynamic Flow Architecture Plan, Principle 2 |
| DCTX-02 | Units are self-similar and conform to a predicate-shaped external interface. | Dynamic Flow Architecture Plan, Principle 3 |
| DCTX-03 | Versioning and rollback exist by construction through append-only version chains and active pointers. | Dynamic Flow Architecture Plan, Principle 4 |
| DCTX-04 | Internal LLMs run first; external AI is allowed only through explicit single-use approval after a named knowledge edge. | D-14 and architecture Principle 5 qualification |
| DCTX-05 | Arbiters are the acceptance authority; KNN, priority, budget pressure, and external AI cannot override rejection. | D-15 and High-Level Amendment |
| DCTX-06 | Idle learning runs only under the IdleDetector gate and promotes a new adapter only if after-accuracy exceeds before-accuracy. | D-14 |

### Constraints Inherited

| ID | Constraint | Source authority |
|---|---|---|
| CSTR-01 | No external ML frameworks. | D-11 |
| CSTR-02 | StackDetector must run before normalization. | D-07 and AutonomousLearning hard gate list |
| CSTR-03 | StratifiedSplit is required for train/validation splits. | D-08 |
| CSTR-04 | `max_weight = total / (classes * rare)`; never use the failed x10 constant. | D-09 |
| CSTR-05 | Public service results use the project return envelope instead of uncontrolled exceptions. | D-01 / project DNA |
| CSTR-06 | External dependencies flow through interfaces. | D-02 / Fabric First |
| CSTR-07 | BaseService-style service inheritance is a project non-negotiable for relevant service classes. | Architecture plan and project DNA |

### Knowledge Edges

| ID | Knowledge edge | Required handling |
|---|---|---|
| KEDGE-01 | Cross-phase context packet mechanism is newly designed and not implemented in `16.AutonomousLearning`. | Downstream plan must mark it out of scope or state a handoff. |
| KEDGE-02 | Swappability judge is architecturally named but not trained. | Downstream plan must not pretend it is implemented. |
| KEDGE-03 | Unit registry topology runtime is broader than `16.AutonomousLearning`. | Downstream plan must stay scoped to learning pipeline implementation. |

### Boundary Markers

| ID | Boundary | Meaning |
|---|---|---|
| BND-01 | The downstream plan uses a fixed implementation phase sequence. | This may be valid build order, but must not be confused with rejected runtime topology. |
| BND-02 | The downstream plan predates D-14 and D-15 test hardening. | It may preserve the ideas partly but not name all later authority decisions. |

### Rejected Items

| ID | Rejected item | Source authority |
|---|---|---|
| REJ-01 | Static runtime pipeline as the architecture shape. | Dynamic Flow Architecture Plan |
| REJ-02 | External AI as ordinary control-path authority. | D-14 |
| REJ-03 | Any score or external answer overriding arbiter rejection. | D-15 |
| REJ-04 | Whole-subgraph replacement as the first version of flow modification. | Dynamic Flow Architecture Plan |

## Downstream Source Packet Summary

The Autonomous Learning session plan contains an eight-step autonomous learning
cycle:

1. IdleDetector gates whether training is safe.
2. DataSourceFinder collects .NET sources.
3. TrustScorer scores sources.
4. DatasetPreprocessor runs StackDetector before normalization.
5. ReplayBuffer keeps a 70/30 new/replay mix.
6. DatasetWeighter applies the D-09 weight formula.
7. IncrementalTrainer trains LoRA.
8. DomainModelRegistry saves the adapter and accuracy.

The plan preserves several technical constraints directly, including D-11,
D-07, D-08, D-09, IdleDetector gating, replay buffer protection, and
before/after accuracy measurement.

The transition experiment focuses on what the downstream source does not
fully preserve or acknowledge:

- BaseService inheritance appears as an architectural/project-DNA constraint
  but lacks an explicit enforcement check in the implementation plan.
- Cross-phase context packet work is outside the component scope but is not
  explicitly marked out of scope.
- The fixed phase sequence is build-time ordering and must not be mistaken for
  a reinstated static runtime pipeline.

