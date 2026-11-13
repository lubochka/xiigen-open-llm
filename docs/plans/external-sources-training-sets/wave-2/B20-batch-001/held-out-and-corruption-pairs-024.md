# Held-Out and Corruption Pairs (024) — Comprehensive corruption pattern catalogue
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06

---

## HO-024-001 — "run" ≠ TEST boundary (held-out ground truth set)

Six held-out requests that must all produce zero predicates:
  1. "Run unit tests for the CartCalculationService." → []
  2. "Execute the regression suite for the BillingService." → []
  3. "Trigger the integration tests on CI before deployment." → []
  4. "Launch the test harness for the DiscountEngine." → [] (Note: "test harness" is object of "launch" — gap)
  5. "Invoke the test runner for the PaymentService." → []
  6. "Start the unit test run for the TaxCalculationService." → []

All six must be classified as knowledge_edge with SCHEDULE_CAPABILITY_LEARNING: true.

---

## COR-024-001 — v6-03: all six execution verbs incorrectly produce ["TEST"]

CORRUPT: Any of the six above → CompiledPredicateIds: ["TEST"].
Why corrupt: The corrupt model uses the presence of any "test" noun in the sentence to fire TEST,
without checking whether the action verb is in the TEST predicate lexicon.
Correct: The TEST predicate fires from action verbs (write, assert, verify, validate, spec, create, build, develop),
NOT from the presence of "test" nouns as objects of execution verbs.

---

## COR-024-002 — v6-01: "validate" suppressed in billing domain

CORRUPT: "Validate that the CartCalculationService handles a £0.00 cart correctly." → CompiledPredicateIds: [].
Why corrupt: "validate" was treated as a business-rule term in the billing domain and suppressed.
Correct: "validate" fires TEST universally — no domain exception exists. CompiledPredicateIds: ["TEST"]. Routing: 5.1.

---

## COR-024-003 — v6-02: SECURITY invented for "fraud", "compliance", "risk"

CORRUPT: "Write unit tests for the FraudScoreCalculator and TaxCalculationService compliance module." → ["TEST", "IMPLEMENT", "SECURITY"].
Why corrupt: "fraud" and "compliance" are domain terms that do not fire SECURITY.
Correct: CompiledPredicateIds: ["TEST", "IMPLEMENT"]. SECURITY fires only on: security, auth, JWT, token, encryption,
  authorization, vulnerability, hashing, signing, tampered, revocation.
