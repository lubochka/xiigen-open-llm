# Layer 3 — Teaching Unit Pairs (009)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU009-001 — QA always executes last in sequential flow

A: "Implement the feature, write tests, document the API, and get QA sign-off."
B: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"] — execution order: IMPLEMENT → TEST → DOCUMENT → QA.
Reason: QA gate always last. It validates the complete work package including implementation, tests, and documentation.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]

---

## TU009-002 — QA does not drive TransitionType

A: "Write integration tests and get QA approval."
B: ["TEST", "QA"] → TransitionType: feature_addition (from TEST, not QA)
Reason: QA never drives TransitionType. TEST drives feature_addition here. QA only determines gate flow.
CompiledPredicateIds: ["TEST", "QA"]

---

## TU009-003 — QA+ARCHITECTURE ordering

A: "Design the new service topology and get QA sign-off on the architecture."
B: ["ARCHITECTURE", "QA"] — execution: ARCHITECTURE → QA. TransitionType: documentation.
Reason: ARCHITECTURE drives documentation TransitionType (no IMPLEMENT). QA validates architecture output last.
CompiledPredicateIds: ["ARCHITECTURE", "QA"]

---

## TU009-004 — QA FAIL is a valid terminal state

A: "QA failed the checkout release — two critical bugs found."
B: ["QA"] — FAIL outcome is valid. QA flow outputs FAIL verdict when gates are not met.
Reason: QA gate is a binary decision node. PASS, PASS_WITH_WARNINGS, and FAIL are all valid outputs.
CompiledPredicateIds: ["QA"]

---

## TU009-005 — QA predicate deduplication

A: "Have the QA team run QA testing and deliver the QA report."
B: ["QA"] — three QA occurrences → one QA predicate (set, not multiset).
Reason: Multiple QA triggers collapse to single predicate. One QAGatingFlow73 instance is created.
CompiledPredicateIds: ["QA"]
