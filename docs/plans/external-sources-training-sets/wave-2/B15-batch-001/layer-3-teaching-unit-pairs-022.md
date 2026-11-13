# Layer 3 — Teaching Unit Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-022-A — "refactor" always → TransitionType: refactoring

A: "Refactor the pricing engine." What TransitionType?
B: refactoring. "refactor" verb present → TransitionType: refactoring overrides feature_addition. This rule has no exceptions in B15.

## TU-022-B — "strategy pattern" does not fire ARCHITECTURE

A: "Refactor the pricing engine to use a strategy pattern." Does ARCHITECTURE fire?
B: No. "strategy pattern" is an implementation design pattern noun. ARCHITECTURE requires: architecture, architect, domain model, bounded context. "strategy classes" are code constructs — IMPLEMENT scope. Only ["IMPLEMENT"] fires (TransitionType: refactoring).

## TU-022-C — "refactor + write tests" → IMPLEMENT+TEST, TransitionType: refactoring

A: "Refactor the pricing engine and write regression tests." What predicates and TransitionType?
B: Predicates: ["IMPLEMENT", "TEST"]. TransitionType: refactoring. "refactor" → IMPLEMENT (TransitionType: refactoring); "write regression tests" → TEST. TransitionType: refactoring propagates alongside both predicates — it is the property of the ImplementationFlow41 execution, not of the TEST flow.

## TU-022-D — TransitionType: refactoring in factory passthrough

A: When TransitionType: refactoring is set, does it change which flow is selected?
B: No. TransitionType: refactoring is metadata that passes through the factory to flow execution. The factory still selects ImplementationFlow41 (or its candidate/fallback). The flow executes in refactoring mode — checking backward-compatibility and no-deletion constraints — but the same flow ID is selected.

## TU-022-E — Backward-compatibility required in refactoring

A: What does the arbiter check in refactoring mode?
B: The refactoring-safety-arbiter checks: (1) existing public API signatures preserved; (2) no business logic deleted; (3) callers are unmodified. If any existing caller would break, the arbiter returns FAIL. The arbiter does not check for new features — only that the refactoring is safe.
