# Layer 0-C — Knowledge Edge Pairs (047) — BloomFilter: gap analysis for probabilistic data structure verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E047-001 — "calculate false-positive rate" does NOT fire IMPLEMENT

"Calculate the expected false-positive rate for a BloomFilter with 1000 elements." → CompiledPredicateIds: []
"calculate" → domain computation verb; zero predicates.
"false-positive rate" → domain probabilistic term; does not fire any predicate.

---

## E047-002 — "apply bit array updates" does NOT fire IMPLEMENT

"Apply the bit array updates from the hash functions during element insertion." → CompiledPredicateIds: []
"apply" (standalone) is explicitly NOT in the IMPLEMENT predicate lexicon.
Even though "apply bit array updates" describes a construction action, "apply" alone is not IMPLEMENT.
Only: write, create, build, develop, implement, add, extend, generate fire IMPLEMENT.

---

## E047-003 — "false-positive" domain term does NOT fire TEST

"Ensure the BloomFilter false-positive rate stays below 1%." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "false-positive" → domain statistical term; does not fire TEST.
Contrast: "assert the false-positive rate is below 1%" → ["TEST"] (because "assert" fires TEST).

---

## E047-004 — "probably-present" domain phrase does NOT fire TEST

"Confirm the BloomFilter returns probably-present for inserted elements." → CompiledPredicateIds: []
"confirm" → informal QA; zero predicates. "probably-present" → domain membership query term; zero predicates.
Teaching: domain probabilistic terms (probably-present, false-positive, false-negative) do not fire predicates.
