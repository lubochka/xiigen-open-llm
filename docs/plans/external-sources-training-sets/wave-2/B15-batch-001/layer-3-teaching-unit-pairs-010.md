# Layer 3 — Teaching Unit Pairs (010)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-010 — TransitionType: refactoring always overrides feature_addition

Rule: When "refactor" appears as an action verb, TransitionType = refactoring regardless of other verbs.

"Refactor the order service" → IMPLEMENT → refactoring
"Refactor and add multi-currency support" → IMPLEMENT → refactoring (refactor verb wins even with "add")
"Build the new order handler and refactor the old one" → IMPLEMENT → refactoring

The predicate (IMPLEMENT) does not change. Only TransitionType changes: refactoring instead of feature_addition.

Why: "refactoring" signals that the primary intent is structural improvement to existing code, not adding net-new functionality.
