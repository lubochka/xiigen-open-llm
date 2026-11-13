# Layer 0-A — Compiler Pairs (024) — Complete predicate rule reference: "run" ≠ TEST boundary
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P024-001 — GAP: "run unit tests" does NOT fire TEST

A: "Run unit tests for the CartCalculationService and report the results."
  "run" — NOT in TEST predicate lexicon. TEST fires on: test (verb/noun/gerund), write tests, assert, verify, validate, spec, mock, stub, fixture, scenario, regression, harness, case.
  "unit tests" is the object of "run" — the predicate is assessed from the verb, not the object.
  "run" is an execution verb; "unit tests" as object does not elevate "run" into a TEST predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
  Critical boundary: "run tests" ≠ TEST. Running is execution; writing, asserting, verifying, speccing are testing.

---

## P024-002 — GAP: "run the test suite" does NOT fire TEST

A: "Run the test suite against the BillingService before deployment."
  "run" — NOT TEST. "test suite" is the object of "run". The predicate is parsed from the action verb.
  "run" fires no predicate. "before deployment" — temporal qualifier; no predicate.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P024-003 — TEST: "write tests" vs "run tests" minimal pair

A: "Write tests for the DiscountEngine." vs "Run tests for the DiscountEngine."
  "write tests" → CompiledPredicateIds: ["TEST", "IMPLEMENT"] — TEST via "tests"; IMPLEMENT via "write"
  "run tests" → CompiledPredicateIds: [] — "run" is execution, not test-writing; zero predicates → knowledge_edge
  This is the canonical minimal pair for the run ≠ TEST boundary.

---

## P024-004 — TEST: "execute tests" also does NOT fire TEST

A: "Execute the regression tests for the FraudScoreCalculator."
  "execute" — NOT in TEST predicate lexicon (same category as "run" — execution verbs).
  "regression tests" — "regression" alone is gap; compound with "tests" — but "execute" is the action verb and it does not fire TEST.
  CompiledPredicateIds: [] | SourceStatus: knowledge_edge
  Boundary: "execute" and "run" are execution verbs. Neither fires TEST. Only writing/asserting/verifying/speccing fires TEST.

---

## P024-005 — Full predicate rule reference for sub-phase 5.1 (Unit Tests) boundary

TEST fires on (exhaustive list for sub-phase 5.1):
  Verbs: test, verify, assert, validate, spec, mock (when test-adjacent), stub (when test-adjacent)
  Nouns/compounds: unit test(s), integration test(s), test suite, test case(s), test harness, fixture, regression test(s), test coverage, scenario (test context), write tests, create tests
  Morphological forms: test / tests / testing / tested / test-driven — all fire TEST

TEST does NOT fire on:
  run, execute, trigger, invoke, launch, start — these are execution verbs (zero predicate)
  check, confirm, ensure, make sure — these are informal QA language (zero predicate)
  testable (adjective alone) — zero predicate
  QA (abbreviation) — fires QA predicate, NOT TEST

TransitionType rules for TEST alone:
  TEST alone → documentation (TransitionType belongs to IMPLEMENT; TEST alone has no IMPLEMENT signal)
  TEST + IMPLEMENT → feature_addition
  TEST + REFACTOR → refactoring
