# Held-Out and Corruption Pairs (008)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H008-001 — Held-out: QA — "QA criteria"

A: "Define the QA criteria for the mobile checkout release."
  Expected: ["QA"]
  Held-out: "QA criteria" is a QA compound noun → fires QA predicate.

---

## H008-002 — Held-out: IMPLEMENT+TEST+QA — three-predicate

A: "Implement the loyalty points feature, write unit tests, and QA sign-off before the sprint ends."
  Expected: ["IMPLEMENT", "TEST", "QA"]
  Held-out: Three distinct clauses → three predicates. QA last. TransitionType: feature_addition (IMPLEMENT present).

---

## H008-003 — Held-out: gap — "production monitoring"

A: "Monitor the production error rate after the hotfix."
  Expected: []
  Held-out: Production monitoring without QA abbreviation → gap. Operational post-deployment activity.

---

## C008-001 — Corruption v6-01: TESTING invented

A: "QA testing must pass before the release."
  Correct: ["QA"]
  Corrupt: ["QA", "TESTING"] ← v6-01 (TESTING is not a valid predicate)
  Teaching: TESTING is not a predicate. TEST is the predicate. "QA testing" is one QA compound noun → one QA predicate.

---

## C008-002 — Corruption: QA approval → AUTHORIZATION (wrong)

A: "Get QA approval before merging."
  Correct: ["QA"]
  Corrupt: ["AUTHORIZATION"] ← wrong (approval context incorrectly triggers AUTHORIZATION)
  Teaching: AUTHORIZATION fires on access-control verbs (grant, revoke, authorize, permission). "QA approval" is a QA compound noun → QA predicate only.
