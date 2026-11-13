# Cross-Layer Synthesis (003)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

## CLS-003 — IMPLEMENT+TEST+DOCUMENT+QA: Full feature cycle without API

Request: "Implement the order discount engine, write unit tests for all discount scenarios, document the discount rules, and QA before the feature ships."

### Layer 0-A (Compiler)
implement → IMPLEMENT | write unit tests → TEST (5.1) | document → DOCUMENT | QA → QA
Predicates: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]

### Layer 0-B (Router)
Four flows selected. QA sub-phase: 7.3 (feature gate — "before the feature ships").

### Layer 0-C (Knowledge Edge)
TEST → 5.1: "unit tests" + "discount scenarios" = calculator/algorithm context → pure function → 5.1.
QA → 7.3: "before feature ships" = feature acceptance gate, not staging release.

### Layer 0-D (Flow Factory)
Four factories. TransitionType: feature_addition.

### Layer 1 (Transition Orchestrator)
ImplementationFlow41 → UnitTestingFlow51 ‖ DocumentationFlow → QAGatingFlow73
TEST and DOCUMENT parallel; QA validates all three.

### Layer 2 (Flow Execution)
ImplementationFlow41: OrderDiscountEngine — tiered discounts, bulk discounts, promo codes. APPROVED.
UnitTestingFlow51: 12 unit tests — tiered/bulk/promo/edge cases — all pass. TEST_PASS.
DocumentationFlow: Discount rules spec — 3 discount types, eligibility matrix. APPROVED.
QAGatingFlow73: All gates cleared. QA_PASS.

### Layer 3 (Teaching)
Four predicates from four verbs. QA sub-phase determined by "before the feature ships" (7.3). TEST sub-phase from "unit tests" + algorithm (5.1). No API integration → no API_CONTRACT.

TransitionType: feature_addition | QA last: absolute
