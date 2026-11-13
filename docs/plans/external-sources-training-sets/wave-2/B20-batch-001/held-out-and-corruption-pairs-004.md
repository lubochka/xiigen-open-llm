# Held-Out and Corruption Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: held_out_corruption

---

## H004-001 — Held-out: BDD + explicit "test" = predicates fire

Request: "Write BDD tests: Given the UserService has an active session, When login is called again, Then return the existing token."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "write" → IMPLEMENT; "BDD tests" → TEST via "tests"; Given/When/Then syntax is gap but "tests" present
  TransitionType: feature_addition

---

## H004-002 — Corruption v6-01: "testing strategy" fires only ARCHITECTURE (drops TEST)

Corrupt output: "define the testing strategy for the PaymentModule" → CompiledPredicateIds: ["ARCHITECTURE"]
Correct output: CompiledPredicateIds: ["ARCHITECTURE", "TEST"]
  Corruption type: v6-01 (predicate suppression — incorrectly dropping TEST from "testing strategy")
  Fix: "testing" fires TEST. "define" fires ARCHITECTURE. Both fire.

---

## H004-003 — Held-out: "code coverage" is gap; "test coverage" fires TEST

Request A: "Achieve 95% code coverage on the InvoiceService." → CompiledPredicateIds: []
Request B: "Increase test coverage to 95% on the InvoiceService." → CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence B: "increase" → IMPLEMENT; "test coverage" → TEST via "test"; "95%" threshold is gap

---

## H004-004 — Corruption v6-02: fallback to gap — "assert" suppressed by domain context

Corrupt output: "assert the PaymentService stores no plaintext" → CompiledPredicateIds: ["SECURITY"]
Correct output: CompiledPredicateIds: ["TEST", "SECURITY"]
  Corruption type: v6-02 (fallback suppression — TEST incorrectly dropped in security domain context)
  Fix: "assert" fires TEST regardless of domain. SECURITY fires from "plaintext" security-action context. Both fire.

---

## H004-005 — Held-out: "mutation testing" + "apply" = IMPLEMENT+TEST both fire

Request: "Apply mutation testing to the DiscountService before the sprint review."
  Correct: CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Evidence: "apply" → IMPLEMENT; "mutation testing" → TEST via "testing"; "sprint review" is gap
  TransitionType: feature_addition
