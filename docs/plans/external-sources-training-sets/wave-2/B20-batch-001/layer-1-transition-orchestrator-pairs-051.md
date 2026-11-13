# Layer 1 — Transition Orchestrator Pairs (051) — "verify" in purpose clause fires TEST regardless
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T051-001 — "verify" in purpose clause fires TEST: HashMapImpl example

Request: "Insert and retrieve key-value pairs from the HashMapImpl to verify throughput."
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation
  Evidence: "verify" fires TEST even as a purpose clause ("to verify").
  "insert" and "retrieve" → domain operation verbs; do not fire IMPLEMENT.
  No construction verb → TransitionType: documentation.

---

## T051-002 — "verify" in purpose clause with construction verb → feature_addition

Request: "Implement the HashMapImpl and insert test keys to verify bucket distribution."
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition
  Evidence: "implement" → IMPLEMENT fires. "verify" → TEST fires (as purpose clause).
  Construction verb present → TransitionType: feature_addition.

---

## T051-003 — Confirmed: TEST fires from any syntactic position when "verify" is present

"Verify that..." (main clause) → TEST fires.
"...to verify that..." (purpose clause) → TEST fires.
"...in order to verify..." (infinitive purpose) → TEST fires.
"...by verifying that..." (gerund) → TEST fires.
All morphological and syntactic forms of "verify" fire TEST regardless of position in sentence.
