# Layer 0-C — Knowledge Edge Pairs (053) — CurrencyFormatter: gap analysis for currency-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E053-001 — "format currency value" does NOT fire IMPLEMENT

"Format the currency value to two decimal places using the CurrencyFormatter." → CompiledPredicateIds: []
"format" → domain operation verb; zero predicates.
Currency formatting verbs: format, display, render, localise, present — all zero predicates.

---

## E053-002 — Currency abbreviations do NOT fire any predicate

"Display the amount in GBP (Great British Pound) using the CurrencyFormatter." → CompiledPredicateIds: []
"display" → domain operation verb; zero predicates.
GBP (Great British Pound), EUR (Euro), USD (US Dollar), JPY (Japanese Yen) — all domain abbreviations; zero predicates.

---

## E053-003 — "evaluate output format" does NOT fire TEST

"Evaluate the output format produced by the CurrencyFormatter for accuracy." → CompiledPredicateIds: []
"evaluate" → domain computation verb; NOT in the TEST predicate lexicon.
Critical boundary: "evaluate" (domain computation) ≠ "validate" (TEST lexicon). Same sentence structure, opposite outcomes.

---

## E053-004 — "negative amounts" domain boundary: TEST fires from "validate" not from subject matter

"Validate that the CurrencyFormatter displays negative amounts with a leading minus sign." → ["TEST"]
  "validate" → TEST fires. "negative amounts" → domain value type; does not independently fire predicates.
"Display negative amounts correctly in the CurrencyFormatter." → []
  "display" → domain operation verb; zero predicates.
Teaching: amount sign type (negative, positive, zero) does not fire predicates; the action verb does.
