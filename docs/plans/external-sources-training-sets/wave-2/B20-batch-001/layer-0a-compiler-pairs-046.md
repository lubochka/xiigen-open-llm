# Layer 0-A — Compiler Pairs (046) — HashMapImpl: TEST fires on test-infrastructure vocabulary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P046-001 — TEST+IMPLEMENT: "add HashMapImpl and write unit tests"

A: "Add the HashMapImpl class and write unit tests for collision resolution using chaining."
  "add" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  "chaining" — collision-resolution domain term; does not fire any predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P046-002 — TEST: "assert" fires TEST on hash collision handling

A: "Assert that the HashMapImpl handles key collisions without data loss."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P046-003 — GAP: "compute the hash" fires zero predicates

A: "Compute the hash of the key to locate the correct bucket in the HashMap."
  "compute" → domain computation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P046-004 — TEST: "test double" fires TEST for HashMapImpl dependency

A: "Use a test double to simulate hash collision scenarios in the HashMapImpl unit tests."
  "test double" → TEST (compound). "use" is context verb; "test double" provides the TEST signal.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P046-005 — GAP: "insert and retrieve key-value pairs" fires zero predicates

A: "Insert and retrieve key-value pairs from the HashMapImpl to verify throughput."
  "insert" and "retrieve" → domain operation verbs; neither fires TEST or IMPLEMENT.
  "verify" IS in the TEST lexicon — this is a boundary case.
  Correct: "verify" → TEST fires. "insert", "retrieve" → zero predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1
