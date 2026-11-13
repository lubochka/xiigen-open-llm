# Layer 3 — Teaching Unit Pairs (006)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU006-001 — QA+TEST disambiguation (QA is not TEST)

A: "Write unit tests and get QA sign-off."
B: ["TEST", "QA"] — "write unit tests" → TEST; "QA sign-off" → QA. Two distinct predicates.
Reason: TEST fires on test-writing verbs (write, create, add tests). QA fires on QA abbreviation. They are not synonyms.
CompiledPredicateIds: ["TEST", "QA"]

---

## TU006-002 — TEST alone does not fire QA

A: "Write integration tests for the payment service."
B: ["TEST"] — no QA abbreviation → TEST only.
Reason: Testing activities without QA abbreviation fire TEST, not QA. QA is the gate; TEST is the activity.
CompiledPredicateIds: ["TEST"]

---

## TU006-003 — QA is the gate, TEST is the activity

A: "Run the regression suite and get QA approval."
B: ["TEST", "QA"] — "run regression suite" → TEST; "QA approval" → QA.
Reason: TEST covers test execution; QA covers the formal gate/approval process. Both present → both predicates.
CompiledPredicateIds: ["TEST", "QA"]

---

## TU006-004 — QA+SECURITY multi-predicate

A: "Run penetration tests and have QA validate the security controls."
B: ["SECURITY", "TEST", "QA"] — "penetration tests" → SECURITY+TEST; "QA validate" → QA.
Reason: "penetration tests" co-fires SECURITY and TEST. QA validation adds QA. Three predicates.
CompiledPredicateIds: ["SECURITY", "TEST", "QA"]

---

## TU006-005 — QA environment is QA compound noun

A: "Deploy the build to the QA environment for testing."
B: ["QA"] — "QA environment" → QA predicate. "deploy" alone does not fire TEST.
Reason: "QA environment" is a QA compound noun. "deploy" is operational gap. "testing" alone does not fire TEST (no test-writing verb).
CompiledPredicateIds: ["QA"]
