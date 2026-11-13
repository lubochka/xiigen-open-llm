# Layer 0-C — Knowledge Edge Pairs (055) — UnitConverter: gap analysis for unit-conversion domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E055-001 — "convert metres to feet" does NOT fire IMPLEMENT

"Convert the measurement from metres to feet using the UnitConverter." → CompiledPredicateIds: []
"convert" → domain operation verb; zero predicates.
Unit conversion verbs: convert, transform, translate (measurement), scale — all zero predicates.

---

## E055-002 — "apply conversion factor" does NOT fire IMPLEMENT

"Apply the conversion factor from the UnitConverter lookup table to the input measurement." → CompiledPredicateIds: []
"apply" (standalone) → NOT in IMPLEMENT lexicon; zero predicates.
Confirmed: "apply conversion factor" is the same gap pattern as all other "apply (standalone)" cases.

---

## E055-003 — ISO (International Organisation for Standardisation) abbreviation does NOT fire any predicate

"Update the UnitConverter to support ISO (International Organisation for Standardisation) unit codes." → CompiledPredicateIds: []
"update" → NOT in IMPLEMENT lexicon (IMPLEMENT is: write, create, build, develop, implement, add, extend, generate).
ISO (International Organisation for Standardisation) → standards abbreviation; does not fire predicates.
Double gap: "update" and "ISO" both produce zero predicates.

---

## E055-004 — "incompatible unit types" domain term does NOT fire TEST

"Ensure the UnitConverter rejects incompatible unit type conversions." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "incompatible unit types" → domain term; does not fire TEST.
Contrast: "validate that the UnitConverter rejects incompatible unit types" → ["TEST"]
  "validate" fires TEST; "ensure" does not. Same subject matter, opposite outcomes.
