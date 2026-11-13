# Layer 0-D — Flow Factory Pairs (036) — Complete Flow Factory Reference (Round 2)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-036-001 — Round 2 gate predicates and flow mapping

Test gates → TEST → TestingFlow | Security/compliance gates → SECURITY → SecurityFlow6x
  QA always adds ReleaseGateFlow71 | IMPLEMENT always adds ImplementationFlow41 + feature_addition
  Teaching: Each predicate maps to exactly one flow. No overlap between gate categories.

## B28-factory-036-002 — QA alone → one flow, no TransitionType

A: ["QA"] | [ReleaseGateFlow71] | no TransitionType
  Teaching: QA alone is gate-only. Always true in Round 2 context.

## B28-factory-036-003 — Round 2 maximum predicate set

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT", "QA"] | Six flows | feature_addition
  Sequential: ARCHITECTURE → parallel(IMPLEMENT ‖ SECURITY) → TEST → DOCUMENT → QA
  Teaching: No cap on flow count. All predicates fire independently.

## B28-factory-036-004 — IMPLEMENT+QA: feature_addition (IMPLEMENT wins over QA gate)

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT always drives feature_addition. QA is gate-only.

## B28-factory-036-005 — pure gap (all Round 2 gap terms) → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge | not SCHEDULE_CAPABILITY_LEARNING | not GapHandler
  Teaching: Any of: trigger, configure, enable, monitor, push, merge, commit, schedule → pure gap → knowledge_edge.
