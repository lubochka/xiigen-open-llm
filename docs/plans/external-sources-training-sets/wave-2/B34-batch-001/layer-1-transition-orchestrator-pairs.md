# Layer 1 — Transition Orchestrator Pairs
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance gap
# Contract: TransitionOrchestratorUnit : ILogicalUnit<FlowInstanceDescriptor, TransitionClassification>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Pair B34B1-WDA-trans-001 — DOCUMENT → documentation transition

```
source_id: xiigen-b34-trans-001
transition_type: documentation
phase: 6
sub_phase: 6.2
pair_kind: transition_orchestrator
```

A:
  compiled_text: "Document the onboarding guide and write a tutorial walkthrough for new developers."
  compiled_predicate_ids: ["DOCUMENT"]
  flow_id: "DocumentationFlow62"

B:
  TransitionType: documentation
  TransitionEvidence:
    - "'document' + 'write' on a guide/tutorial target → documentation transition"
    - "No feature construction vocabulary (build/implement/create service)"
    - "Output is a document artifact, not a code artifact"
  PhaseId: 6
  SubPhaseId: 6.2
  TransitionConfidence: 0.92

---

## Pair B34B1-WDA-trans-002 — IMPLEMENT → feature_addition (new training system)

```
source_id: xiigen-b34-trans-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: transition_orchestrator
```

A:
  compiled_text: "Implement the developer training module and build the skill-tracking system."
  compiled_predicate_ids: ["IMPLEMENT"]
  flow_id: "BackendServiceFlow41"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement' + 'build' on named module/system → new construction = feature_addition"
    - "No refactoring (refactor/extract/restructure) or migration (migrate/seed) vocabulary"
    - "'training module' and 'skill-tracking system' are net-new systems"
  PhaseId: 4
  SubPhaseId: 4.1
  TransitionConfidence: 0.92
