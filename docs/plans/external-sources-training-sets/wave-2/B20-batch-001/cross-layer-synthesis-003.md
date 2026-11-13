# Cross-Layer Synthesis (003)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## S003 — End-to-end trace: gap request "make sure the service behaves correctly after the refactor"

### Layer 0-A (Compiler)
Input: "Make sure the OrderService behaves correctly after the refactor."
  "make sure" → gap | "behaves correctly" → gap | "refactor" → IMPLEMENT
  CompiledPredicateIds: ["IMPLEMENT"]
  Note: "refactor" fires IMPLEMENT even when used as a noun ("after the refactor" = after the refactoring action). Context: noun use of "refactor" still fires IMPLEMENT.
  Teaching: "the refactor" (noun) is borderline — TransitionType would be refactoring.

### Alternate version
Input B: "Make sure the OrderService behaves correctly."
  "make sure" → gap | "behaves correctly" → gap
  CompiledPredicateIds: []
  GapHandler: Emit SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"

### Layer 0-B (Router) — version B
  Zero predicates → GapHandler path | No routing decision

### Layer 0-D (Factory) — version B
  GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING) | No flow created

### Layer 3 (Teaching)
  Lesson A: Noun form "the refactor" → IMPLEMENT fires (borderline rule — noun use of action verb)
  Lesson B: "make sure X behaves correctly" → pure gap; zero predicates regardless of domain

### Cross-layer invariant
  The gap path is a complete execution path: compiler identifies zero predicates → factory emits learning signal → no nine-node flow created. This is correct behavior, not an error state.
