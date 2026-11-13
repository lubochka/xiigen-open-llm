# Held-Out and Corruption Pairs (003)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H003-001 — Held-out: QA — "QA lead"

A: "The QA lead needs to review the performance test results."
  Expected: ["QA"]
  Held-out: "QA lead" role title → fires QA predicate.

---

## H003-002 — Held-out: TEST+QA — test writing + QA gate

A: "Write smoke tests for the checkout flow and get QA sign-off."
  Expected: ["TEST", "QA"]
  Held-out: "write smoke tests" → TEST (sub-phase 5.3); "QA sign-off" → QA (sub-phase 7.3). Two predicates.

---

## H003-003 — Held-out: gap — "approval" alone

A: "Get approval from the team before deploying."
  Expected: []
  Held-out: "approval" without QA abbreviation → gap. Generic approval is not QA.

---

## C003-001 — Corruption v6-02: deploy → QA (wrong)

A: "Deploy the service to the QA environment."
  Correct: ["QA"]
  Corrupt: ["QA", "IMPLEMENT"] ← v6-02 ("deploy" incorrectly mapped to IMPLEMENT)
  Teaching: "deploy" is operational gap. IMPLEMENT requires code-writing verbs (implement, build, create, write). Deploy ≠ implement.

---

## C003-002 — Corruption v6-03: TEST+QA collapsed to QA only

A: "Write acceptance tests and get QA sign-off."
  Correct: ["TEST", "QA"]
  Corrupt: ["QA"] ← v6-03 (TEST suppressed because QA appears to subsume it)
  Teaching: QA does not subsume TEST. Test-writing verbs independently fire TEST. Two flows must be instantiated.
