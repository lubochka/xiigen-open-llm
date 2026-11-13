# Held-Out and Corruption Pairs (010)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H010-001 — Held-out: QA — "QA metrics"

A: "Review the QA metrics dashboard before the release decision."
  Expected: ["QA"]
  Held-out: "QA metrics" is a QA compound noun → fires QA predicate.

---

## H010-002 — Held-out: QA — negative sentence

A: "This PR was not reviewed by QA yet."
  Expected: ["QA"]
  Held-out: Negation does not suppress QA predicate. "QA" present → fires.

---

## H010-003 — Held-out: gap — "set up test environment"

A: "Set up the test environment for the performance tests."
  Expected: []
  Held-out: "set up" is operational gap. No test-writing verb and no QA abbreviation.

---

## C010-001 — Corruption v6-02: "test" keyword → TEST (wrong, no writing verb)

A: "Review the test coverage report in Sonar."
  Correct: []
  Corrupt: ["TEST"] ← v6-02 ("test" keyword incorrectly triggers TEST without writing verb)
  Teaching: TEST fires on test-writing verbs (write, create, add, implement, build tests). Reading/reviewing coverage reports is observational gap.

---

## C010-002 — Corruption: QA sub-phase 7.1 for feature-gate context (wrong)

A: "QA sign-off on the new user profile feature."
  Correct: ["QA"] → sub-phase 7.3 (QAGatingFlow73) — feature acceptance vocabulary
  Corrupt: → sub-phase 7.1 (ReleaseGateFlow71) ← wrong (no release/deployment vocabulary present)
  Teaching: Feature acceptance without release/deploy vocabulary → 7.3. 7.1 requires explicit release pipeline context.
