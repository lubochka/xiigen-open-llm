# Layer 0-B — Router Pairs
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance gap
# Contract: PromptRoutingInputRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRouterResult>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B34B1-WDA-router-001 — DOCUMENT → 6.2

```
source_id: xiigen-b34-router-001
transition_type: documentation
phase: 6
sub_phase: 6.2
pair_kind: router
```

A:
  compiled_predicate_ids: ["DOCUMENT"]
  compiled_text: "Document the onboarding guide and write a tutorial walkthrough for new developers."

B:
  RoutingDecisions:
    - predicate: DOCUMENT
      sub_phase: 6.2
      sub_phase_label: "Documentation and Knowledge Base"
      confidence: 0.92
      routing_note: "'document' + 'write' → DOCUMENT → 6.2; tutorial/onboarding are gap"

  TransitionType: documentation
  PhaseId: 6
  SubPhaseId: 6.2
  CapabilityGapWarnings:
    - "gap-skills-guidance-onboard-001: 'onboarding guide', 'tutorial', 'walkthrough' are gap vocabulary"

---

## Pair B34B1-WDA-router-002 — IMPLEMENT → 4.1

```
source_id: xiigen-b34-router-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: router
```

A:
  compiled_predicate_ids: ["IMPLEMENT"]
  compiled_text: "Implement the developer training module and build the skill-tracking system."

B:
  RoutingDecisions:
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'implement' + 'module' + 'build' + 'system' → 4.1; training/skill-tracking are gap"

  TransitionType: feature_addition
  PhaseId: 4
  SubPhaseId: 4.1
  CapabilityGapWarnings:
    - "gap-skills-guidance-train-001: 'training module', 'skill-tracking' are gap vocabulary"

---

## Pair B34B1-WDA-router-003 — zero predicates → knowledge_edge (no routing)

```
source_id: xiigen-b34-router-003
transition_type: unknown
phase: null
sub_phase: null
pair_kind: router
```

A:
  compiled_predicate_ids: []
  compiled_text: "Mentor the junior developers on clean code practices and guide them through the codebase."

B:
  RoutingDecisions: []
  TransitionType: unknown
  PhaseId: null
  SubPhaseId: null
  Confidence: 0.70
  CapabilityGapSignal:
    gap_id: "gap-skills-guidance-mentor-001"
    DomainId: "software-development"
    SCHEDULE_CAPABILITY_LEARNING: true

  Teaching: No routing decision is made when CompiledPredicateIds is empty.
  The router receives a zero-predicate document and emits the gap signal forward.
  No sub-phase is assigned. No flow is instantiated.
