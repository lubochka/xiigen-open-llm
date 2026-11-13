# Layer 3 — Teaching Unit Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-022-001 — "locale" formatting fires no predicate; "validate locale" fires TEST

A: "The PriceFormatter must support German locale formatting." vs "Validate that the PriceFormatter formats German locale correctly." What fires?
B: "must support" → requirement modal → zero predicates → knowledge_edge.
   "Validate that..." → "validate" fires TEST (universal) → ["TEST"]. TransitionType: documentation.
   Teaching: "must support" is a requirement. "validate" is a test action. Requirements fire nothing; actions fire predicates.

---

## Teach-022-002 — "rounding mode" domain term; "spec rounding" fires TEST

A: "Spec the PriceFormatter rounding mode: always half-up, two decimal places." What fires?
B: "spec" fires TEST → 5.1. "rounding mode" — domain term — no additional predicate. Result: ["TEST"] — documentation.

---

## Teach-022-003 — "run unit tests" does NOT fire TEST (PriceFormatter instance)

A: "Run unit tests for the PriceFormatter to check the German locale fix." What fires?
B: Zero predicates. "run" — execution verb — NOT TEST. "check" — informal inspection — NOT TEST.
   "unit tests" — object of "run" — cannot promote "run" to TEST. Zero predicates → knowledge_edge.
   Correction: "Write unit tests for the PriceFormatter verifying the German locale output." → ["TEST", "IMPLEMENT"].

---

## Teach-022-004 — "free display" feature flag: "assert" fires TEST; "enable" fires IMPLEMENT

A: "Assert that the PriceFormatter shows 'Free' when the free-display flag is enabled." What fires?
B: "assert" → TEST. "free-display flag" — feature flag — no predicate. "enabled" — domain state — not IMPLEMENT.
   Result: ["TEST"] — TransitionType: documentation.
   Contrast: "Enable the free-display flag in PriceFormatter." → IMPLEMENT fires on "enable". No TEST.
