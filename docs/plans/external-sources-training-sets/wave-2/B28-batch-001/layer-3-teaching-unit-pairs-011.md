# Layer 3 — Teaching Unit Pairs (011)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU011-001 — QA owner fires QA

A: "Who is the QA owner for the payment module?"
B: ["QA"] — "QA owner" → QA compound noun → QA predicate. Interrogative fires QA.
Reason: Questions containing QA abbreviation fire QA predicate. Role identifier "QA owner" is a QA compound noun.
CompiledPredicateIds: ["QA"]

---

## TU011-002 — QA review fires QA

A: "Schedule a QA review for the refactored authentication service."
B: ["QA"] — "QA review" → QA predicate.
Reason: QA review is a QA compound noun. "schedule" is operational but QA predicate fires via abbreviation.
CompiledPredicateIds: ["QA"]

---

## TU011-003 — QA testing fires QA (not double-predicate)

A: "Complete QA testing on the new dashboard."
B: ["QA"] — "QA testing" fires QA only. "testing" does not additionally fire TEST here.
Reason: "QA testing" is a QA compound noun. "testing" inside a QA compound noun does not separately fire TEST predicate.
CompiledPredicateIds: ["QA"]

---

## TU011-004 — Contrast: "write tests AND get QA sign-off" = TEST+QA

A: "Write tests for the dashboard and get QA sign-off."
B: ["TEST", "QA"] — "write tests" fires TEST independently; "QA sign-off" fires QA independently.
Reason: Two independent clauses each with their own trigger. Contrast with TU011-003 where "testing" is inside QA compound noun.
CompiledPredicateIds: ["TEST", "QA"]

---

## TU011-005 — QA harness fires QA

A: "Build the QA harness for automated acceptance testing."
B: ["QA"] — "QA harness" → QA compound noun → QA predicate.
Reason: QA harness, QA framework, QA suite → QA compound nouns. Note: "build" fires IMPLEMENT? No — "build the QA harness" context is automation tooling, not product feature. Without explicit feature implementation context, QA alone fires.
CompiledPredicateIds: ["QA"]
