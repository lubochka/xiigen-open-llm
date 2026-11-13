# Held-Out and Corruption Pairs (016)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H016-001 — Held-out: TEST alone — BDD scenarios checkout (no UI vocabulary)

A: "Write BDD scenarios in Gherkin for the checkout flow: Given-When-Then for each payment method."
  Expected: ["TEST"]
  Held-out: "write BDD scenarios" → TEST; "checkout flow" → no UI vocabulary fires; "payment method" → no UI vocabulary. TEST alone.

---

## H016-002 — Held-out: gap — "describe BDD workflow"

A: "Describe the BDD workflow for the checkout acceptance criteria."
  Expected: []
  Held-out: "describe" → gap; "BDD workflow" → gap; "acceptance criteria" → gap. Zero predicates.

---

## H016-003 — Held-out: TEST+UI — Cucumber scenario for modal focus

A: "Create Gherkin scenarios that assert the confirmation modal opens, traps focus, and closes on ESC."
  Expected: ["TEST", "UI"]
  Held-out: "create scenarios" + "assert" → TEST; "confirmation modal" + "focus" → UI. "Gherkin" + "Cucumber" → gap.

---

## C016-001 — Corruption v6-02: "describe" → TEST (wrong)

A: "Describe the BDD acceptance scenarios for the checkout."
  Correct: []
  Corrupt: ["TEST"] ← v6-02 ("describe" incorrectly treated as a test-writing verb; "scenarios" picked up and TEST fired)
  Teaching: "describe" is gap. Even when co-occurring with "scenarios", the test-writing verb must be present. "describe scenarios" ≠ "write scenarios".

---

## C016-002 — Corruption v6-01: BDD predicate invented

A: "Write BDD scenarios for the checkout flow."
  Correct: ["TEST"]
  Corrupt: ["TEST", "BDD"] ← v6-01 (BDD invented as a predicate)
  Teaching: BDD (Behaviour-Driven Development) is a methodology name, not a predicate. It is gap vocabulary. There is no BDD predicate.
