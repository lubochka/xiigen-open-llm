# Layer 0-D — Flow Factory Pairs (024) — Complete Flow Factory Reference for 7.1
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-024-001 — QA alone → one flow, no TransitionType

A: ["QA"] | [ReleaseGateFlow71] | One flow | TransitionType: none (QA does not contribute)
  Teaching: QA alone never produces a TransitionType. Gate-only.

## B28-factory-024-002 — QA+IMPLEMENT → feature_addition driven by IMPLEMENT

A: ["QA", "IMPLEMENT"] | [ReleaseGateFlow71, ImplementationFlow41] | Two flows | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA does not override.

## B28-factory-024-003 — QA+DOCUMENT (no IMPLEMENT) → documentation

A: ["QA", "DOCUMENT"] | [ReleaseGateFlow71, DocumentationFlow62] | Two flows | documentation
  Teaching: Without IMPLEMENT, TransitionType: documentation.

## B28-factory-024-004 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge | not SCHEDULE_CAPABILITY_LEARNING | not GapHandler
  Teaching: Pure gap in release domain → knowledge_edge output only.

## B28-factory-024-005 — max predicate set for 7.1 context

A: ["QA", "ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "SECURITY"] | 6 flows | feature_addition
  Sequential: ARCHITECTURE → SECURITY → IMPLEMENT → TEST → DOCUMENT → QA (always last)
  Teaching: No cap on flow count. All predicates fire independent flows.
