# Layer 0-A — Compiler Pairs (034)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-034-001 — ProrationService: IMPLEMENT alone

A: "Build a ProrationService that calculates prorated charges on plan upgrades."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"build" → IMPLEMENT. "prorated charges" and "plan upgrades" are domain context.

---

## B15-compiler-034-002 — ProrationService: IMPLEMENT+ARCHITECTURE

A: "Build a ProrationService and architect the proration calculation model."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "architect" (verb) + "calculation model" → ARCHITECTURE.

---

## B15-compiler-034-003 — ProrationService: IMPLEMENT+API_CONTRACT

A: "Build a ProrationService and integrate with the subscription management system."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | feature_addition | DecisionCount: 2
"build" → IMPLEMENT. "integrate" → API_CONTRACT.

---

## B15-compiler-034-004 — ProrationService: IMPLEMENT+TEST+DOCUMENT

A: "Build a ProrationService, write unit tests for the proration formula, and document the calculation rules."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] | feature_addition | DecisionCount: 3
"build" → IMPLEMENT. "write unit tests" → TEST. "document" → DOCUMENT. Three predicates.

---

## B15-compiler-034-005 — IMPLEMENT only: "develop" verb

A: "Develop the ProrationService for mid-cycle plan changes."
B: Predicates: ["IMPLEMENT"] | feature_addition | DecisionCount: 1
"develop" → IMPLEMENT. No additional predicate triggers in request.
