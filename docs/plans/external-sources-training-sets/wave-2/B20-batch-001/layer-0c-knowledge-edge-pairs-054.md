# Layer 0-C — Knowledge Edge Pairs (054) — NumberFormatter: gap analysis for number-format domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E054-001 — "round the number" does NOT fire IMPLEMENT

"Round the number to two decimal places using the NumberFormatter precision setting." → CompiledPredicateIds: []
"round" → domain operation verb; zero predicates.
Number processing verbs: round, truncate, floor, ceil, format, normalise, pad — all zero predicates.

---

## E054-002 — "process numeric output" does NOT fire IMPLEMENT

"Process the numeric output through the NumberFormatter before sending to the UI layer." → CompiledPredicateIds: []
"process" → domain computation verb; zero predicates.
UI layer context does not change predicate firing: domain verbs remain zero predicates regardless of output target.

---

## E054-003 — "thousands separator" domain term does NOT fire TEST

"Ensure the NumberFormatter inserts thousands separators in the correct position." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "thousands separator" → domain format term; does not fire TEST.

---

## E054-004 — "decimal precision" domain term does NOT fire any predicate

"Configure the NumberFormatter to use two decimal places of precision." → CompiledPredicateIds: []
"configure" → domain operation verb; zero predicates. "decimal precision" → domain term; zero predicates.
Confirmed: "configure" is NOT in the IMPLEMENT lexicon. A common false-positive trap.
