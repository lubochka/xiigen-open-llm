# Layer 1 — Transition Orchestrator Pairs
# Batch: B14-batch-001
# Scenario: B14 — DevOps cloud infrastructure, sub-phase 3.5
# Contract: TransitionOrchestratorUnit : ILogicalUnit<FlowInstanceDescriptor, TransitionClassification>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Pair B14B1-WDA-trans-001 — ARCHITECTURE feature_addition (new infra design)

```
source_id: xiigen-b14-trans-001
transition_type: feature_addition
phase: 3
sub_phase: 3.5
pair_kind: transition_orchestrator
```

A:
  compiled_text: "Design the cloud infrastructure architecture for the microservices deployment layer."
  compiled_predicate_ids: ["ARCHITECTURE"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'design' the architecture — creating a new architectural design artifact = feature_addition"
    - "Cloud infrastructure architecture is net-new design work"
    - "No refactoring (restructure existing arch) or documentation-only signals"
  PhaseId: 3
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

---

## Pair B14B1-WDA-trans-002 — ARCHITECTURE + DOCUMENT: documentation transition

```
source_id: xiigen-b14-trans-002
transition_type: documentation
phase: 3
sub_phase: 3.5
pair_kind: transition_orchestrator
```

A:
  compiled_text: "Document the cloud architecture blueprint and diagram the infrastructure components and their dependencies."
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  flow_id: "InfrastructureArchitectureFlow35"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'document' — primary action is documentation = documentation transition"
    - "'diagram' — producing a diagram artifact = documentation"
    - "Both verbs (document, diagram) produce documentary artifacts from existing architectural decisions"
    - "No net-new feature construction signals (build, implement, create service)"
  PhaseId: 3
  SubPhaseId: 3.5
  TransitionConfidence: 0.92

  Note: When the primary action verbs produce documentary artifacts (document, diagram,
  write up) the TransitionType is documentation even if the ARCHITECTURE predicate fires.
  Compare to compiler-001 where "design" → feature_addition (creating a design) vs
  compiler-003 where "document + diagram" → documentation (recording an existing design).
