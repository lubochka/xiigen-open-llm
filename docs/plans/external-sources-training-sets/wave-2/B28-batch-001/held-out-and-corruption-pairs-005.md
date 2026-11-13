# Held-Out and Corruption Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H005-001 — Held-out: QA — "QA is required"

A: "QA is required before this feature can be merged."
  Expected: ["QA"]
  Held-out: "QA" as sentence subject + passive voice → QA predicate fires.

---

## H005-002 — Held-out: QA+SECURITY — security change + QA gate

A: "Patch the authentication vulnerability and QA the security controls."
  Expected: ["SECURITY", "QA"]
  Held-out: "patch authentication vulnerability" → SECURITY; "QA the security controls" → QA. TransitionType: from SECURITY context.

---

## H005-003 — Held-out: gap — "rollback the deployment"

A: "Rollback the deployment to the previous version."
  Expected: []
  Held-out: Rollback is operational gap. No predicate fires.

---

## C005-001 — Corruption v6-02: gap → TEST (wrong)

A: "Monitor the test results in the CI dashboard."
  Correct: []
  Corrupt: ["TEST"] ← v6-02 (monitoring test results incorrectly triggers TEST)
  Teaching: "monitor test results" is observational/operational gap. TEST fires on test-writing verbs (write, create, add, implement tests). Reading dashboards ≠ writing tests.

---

## C005-002 — Corruption v6-03: IMPLEMENT+TEST+QA collapsed to IMPLEMENT only

A: "Implement the refund flow, write tests, and get QA sign-off."
  Correct: ["IMPLEMENT", "TEST", "QA"]
  Corrupt: ["IMPLEMENT"] ← v6-03 (TEST and QA suppressed)
  Teaching: No cap on predicate count. Three independent clauses → three predicates. All three flows must be instantiated.
