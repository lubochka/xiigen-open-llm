# Layer 0-D — Flow Factory Pairs (060) — Complete Flow Factory Reference (Round 3)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-060-001 — Round 3 predicate → flow mapping summary

IMPLEMENT → ImplementationFlow41 | TEST → TestingFlow | DOCUMENT → DocumentationFlow62
  SECURITY → SecurityFlow6x | ARCHITECTURE → ArchitectureFlow31 | QA → ReleaseGateFlow71 (always last)
  Teaching: Each predicate maps to exactly one flow. No overlap.

## B28-factory-060-002 — TransitionType resolution rules (complete)

IMPLEMENT present → feature_addition | No IMPLEMENT + other predicates → documentation | QA alone → no TransitionType | Gap alone → knowledge_edge
  Teaching: IMPLEMENT is the only driver of feature_addition. Complete TransitionType reference.

## B28-factory-060-003 — Maximum Round 3 factory output

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT", "QA"] | [ArchitectureFlow31, ImplementationFlow41, TestingFlow, SecurityFlow6x, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Six flows maximum in Round 3. All predicates fire independently. No cap.

## B28-factory-060-004 — QA gate position rule in factory output

QA → ReleaseGateFlow71 always occupies last position in flow list
  [ArchitectureFlow31, ImplementationFlow41, TestingFlow, SecurityFlow6x, DocumentationFlow62, ReleaseGateFlow71]
  Teaching: ReleaseGateFlow71 is always the final entry in the flow list regardless of predicate count.

## B28-factory-060-005 — Gap produces GapFlow → knowledge_edge: not SCHEDULE_CAPABILITY_LEARNING, not GapHandler

Round 3 gap vocabulary → GapFlow → knowledge_edge only
  Teaching: Pure gap → GapFlow. Not SCHEDULE_CAPABILITY_LEARNING. Not GapHandler. This is an invariant for all rounds.
