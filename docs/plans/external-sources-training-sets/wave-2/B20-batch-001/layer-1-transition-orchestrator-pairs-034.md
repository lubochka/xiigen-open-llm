# Layer 1 — Transition Orchestrator Pairs (034) — HashMapImpl: TransitionType with "test double" boundary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T034-001 — TEST alone via "test double" → documentation for HashMapImpl

Request: "Use a test double to simulate hash collision scenarios in the HashMapImpl unit tests."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "test double" → TEST (compound). "use" → context verb; does not fire IMPLEMENT.

---

## T034-002 — TEST+IMPLEMENT → feature_addition for HashMapImpl

Request: "Add the HashMapImpl class and write unit tests for collision resolution."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "add" and "write" → IMPLEMENT construction verbs → feature_addition.

---

## T034-003 — TEST alone via "verify" → documentation for HashMapImpl

Request: "Insert and retrieve key-value pairs from the HashMapImpl to verify throughput."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "verify" → TEST fires even when primary action verbs (insert, retrieve) are domain-only.
  "insert" and "retrieve" → domain operation verbs; do not fire IMPLEMENT.
  TransitionType: documentation (no construction verb).
