# Layer 0-C — Knowledge Edge Pairs (024) — Complete "run" ≠ TEST knowledge edge reference
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E024-001 — Canonical "run ≠ TEST" summary: six execution verbs, all producing zero predicates

The following execution verbs NEVER fire TEST, regardless of the noun object:

| Verb | Example | Result |
|------|---------|--------|
| run | "Run unit tests for the CartService" | [] — zero predicates |
| execute | "Execute the regression suite for BillingService" | [] — zero predicates |
| trigger | "Trigger the integration tests on CI (Continuous Integration)" | [] — zero predicates |
| launch | "Launch the test harness for DiscountEngine" | [] — zero predicates |
| invoke | "Invoke the test runner for PaymentService" | [] — zero predicates |
| start | "Start the unit test run for TaxCalculationService" | [] — zero predicates |

All six produce knowledge_edge with SCHEDULE_CAPABILITY_LEARNING: true.

---

## E024-002 — Contrast table: TEST-firing verbs vs execution verbs (same noun object)

Noun object: "unit tests for the CartCalculationService"

| Action Verb | CompiledPredicateIds | TransitionType | SubPhase |
|-------------|---------------------|----------------|----------|
| Write | ["TEST", "IMPLEMENT"] | feature_addition | 5.1 + 4.1 |
| Create | ["TEST", "IMPLEMENT"] | feature_addition | 5.1 + 4.1 |
| Build | ["TEST", "IMPLEMENT"] | feature_addition | 5.1 + 4.1 |
| Develop | ["TEST", "IMPLEMENT"] | feature_addition | 5.1 + 4.1 |
| Assert (result via "tests") | ["TEST"] | documentation | 5.1 |
| Verify (result via "tests") | ["TEST"] | documentation | 5.1 |
| Spec | ["TEST"] | documentation | 5.1 |
| Run | [] | unknown | null — knowledge_edge |
| Execute | [] | unknown | null — knowledge_edge |
| Trigger | [] | unknown | null — knowledge_edge |

---

## E024-003 — Sub-phase boundary: "run tests" after code change is a CI (Continuous Integration) action, not a 5.1 action

A: "Run all unit tests to confirm nothing is broken after the billing refactor."
B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Note: CI execution actions have no routing destination in the XIIGen (XIIGEN) phase model.
  The phase model covers design, implementation, and test-writing. Execution/CI operations are outside scope.
  Gap vocabulary: ["run all", "confirm nothing is broken", "after the refactor"]
