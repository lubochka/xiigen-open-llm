# Layer 3 — Teaching Unit Pairs (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU004-001 — QA as imperative verb

A: "QA this before we deploy to staging."
B: ["QA"] — "QA" used as imperative verb → QA predicate.
Reason: "QA this", "QA it", "QA the build" — QA functioning as verb fires QA predicate.
CompiledPredicateIds: ["QA"]

---

## TU004-002 — QA as subject

A: "QA must approve all production deployments."
B: ["QA"] — "QA" as sentence subject → QA predicate.
Reason: QA in subject position (QA must, QA requires, QA is required) fires QA predicate.
CompiledPredicateIds: ["QA"]

---

## TU004-003 — QA in parenthetical

A: "Get testing (QA) approval before the release."
B: ["QA"] — "(QA)" parenthetical → QA predicate fires.
Reason: QA in parentheses is abbreviation occurrence → fires QA predicate.
CompiledPredicateIds: ["QA"]

---

## TU004-004 — QA in negative sentence still fires

A: "This was not reviewed by QA."
B: ["QA"] — negation does not suppress QA predicate. "QA" present → fires.
Reason: QA predicate is fired by presence of abbreviation, not by positive sentiment.
CompiledPredicateIds: ["QA"]

---

## TU004-005 — QA in question fires

A: "Has QA reviewed the login flow?"
B: ["QA"] — interrogative does not suppress QA. Presence → fires.
Reason: Questions containing QA abbreviation fire QA predicate normally.
CompiledPredicateIds: ["QA"]
