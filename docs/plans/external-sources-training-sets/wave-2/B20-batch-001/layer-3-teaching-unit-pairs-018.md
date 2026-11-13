# Layer 3 — Teaching Unit Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Teach-018-001 — "spec" fires TEST for loyalty tier multiplier

A: "Spec the loyalty tier multiplier values: gold=3x, silver=2x, standard=1x." What fires?
B: "spec" fires TEST → 5.1. No IMPLEMENT verb present. Result: ["TEST"] — TransitionType: documentation.
   "multiplier values" — domain specification — no additional predicate.

---

## Teach-018-002 — "QA loyalty flow" fires QA, NOT TEST

A: "QA the loyalty points accrual flow for gold-tier customers." What fires?
B: QA fires on the literal abbreviation "QA" → sub-phase 7.3. TEST does NOT fire because "test" is
   absent from the sentence. "accrual flow" — domain term — no predicate. Result: ["QA"] only.
   QA and TEST are entirely separate predicates. "QA" routes to 7.3 (Quality Assurance); "test" routes to 5.1.

---

## Teach-018-003 — "deduplication rule": multiple TEST triggers → one TEST predicate

A: "Write unit tests for the LoyaltyPointsService, verify the accrual logic, and validate the expiry rules." What fires?
B: "write" → IMPLEMENT. "unit tests" → TEST. "verify" → TEST (duplicate). "validate" → TEST (duplicate).
   Deduplication rule: multiple TEST-firing terms in one request → ONE TEST predicate (not three).
   Result: ["TEST", "IMPLEMENT"] — two predicates total. TEST appears once regardless of how many
   TEST-firing words are in the request.

---

## Teach-018-004 — "testable" adjective vs "test" verb boundary

A: "Make the LoyaltyPointsService testable by extracting the points calculator into a separate class." What fires?
B: "testable" is an adjective — does NOT fire TEST. Only verb/noun/gerund forms fire TEST.
   "extracting" fires IMPLEMENT (refactoring action: extract). Result: ["IMPLEMENT"] — TransitionType: refactoring.
   "Make X testable" = refactoring request. "Write tests for X" = TEST+IMPLEMENT request. They are different.
