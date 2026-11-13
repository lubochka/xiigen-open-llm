# Layer 0-D — Flow Factory Pairs
# Batch: B34-batch-001
# Scenario: B34 — Skills and guidance gap
# Contract: FlowFactoryUnit : ILogicalUnit<SubPhaseRouterResult, FlowInstanceDescriptor>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B34B1-WDA-factory-001 — DOCUMENT / 6.2 → deterministic_generic

```
source_id: xiigen-b34-factory-001
transition_type: documentation
phase: 6
sub_phase: 6.2
pair_kind: factory_selection
```

A:
  sub_phase: 6.2
  predicate: DOCUMENT
  compiled_text: "Document the onboarding guide and write a tutorial walkthrough for new developers."
  capability_gap_warnings: ["gap-skills-guidance-onboard-001"]

B:
  TrustState: deterministic_generic
  FlowId: "DocumentationFlow62"
  FlowLabel: "Documentation and Knowledge Base Flow — sub-phase 6.2"
  SelectionReason: "DOCUMENT predicate at 6.2 with documentation transition — deterministic_generic;
    gap warnings do not degrade trust state"
  NodeCount: 9
  CapabilityGapWarningPassthrough: true

---

## Pair B34B1-WDA-factory-002 — IMPLEMENT / 4.1 → deterministic_generic

```
source_id: xiigen-b34-factory-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: factory_selection
```

A:
  sub_phase: 4.1
  predicate: IMPLEMENT
  compiled_text: "Implement the developer training module and build the skill-tracking system."
  capability_gap_warnings: ["gap-skills-guidance-train-001"]

B:
  TrustState: deterministic_generic
  FlowId: "BackendServiceFlow41"
  FlowLabel: "Service and Logic Implementation Flow — sub-phase 4.1"
  SelectionReason: "IMPLEMENT predicate at 4.1 with feature_addition transition; high-density
    predicate match (implement/module/build/system) — deterministic_generic"
  NodeCount: 9
  CapabilityGapWarningPassthrough: true

---

## Pair B34B1-WDA-factory-003 — zero predicates → knowledge_edge_synthetic

```
source_id: xiigen-b34-factory-003
transition_type: unknown
phase: null
sub_phase: null
pair_kind: factory_selection
```

A:
  sub_phase: null
  predicate: null
  compiled_text: "Mentor the junior developers on clean code practices and guide them through the codebase."
  capability_gap_signal:
    gap_id: "gap-skills-guidance-mentor-001"
    DomainId: "software-development"

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  FlowLabel: "No flow instantiated — capability gap"
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; no nine-node flow runs;
    SCHEDULE_CAPABILITY_LEARNING emitted"
  NodeCount: 0
  CapabilityGapSignalForwarded: true

---

## Pair B34B1-WDA-factory-004 — DOCUMENT/IMPLEMENT multi-predicate → two flows

```
source_id: xiigen-b34-factory-004
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: factory_selection
```

A:
  sub_phase: [4.1, 6.2]
  predicate: ["IMPLEMENT", "DOCUMENT"]
  compiled_text: "Build the onboarding system and document the learning path for new engineers."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'build' + 'system' → IMPLEMENT → 4.1"
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
      routing_note: "'document' → DOCUMENT → 6.2"
  SelectionReason: "Two predicates → two independent flow instances; both deterministic_generic"
  CapabilityGapWarnings:
    - "gap-skills-guidance-onboard-001: 'onboarding', 'learning path' are gap vocabulary"

  Teaching: Multi-predicate requests produce one flow per predicate. The two flows
  run independently — IMPLEMENT routes to 4.1, DOCUMENT routes to 6.2. They are
  never collapsed into a single flow.
