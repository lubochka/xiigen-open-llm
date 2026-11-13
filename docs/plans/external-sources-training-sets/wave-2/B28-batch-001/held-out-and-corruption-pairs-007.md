# Held-Out and Corruption Pairs (007)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H007-001 — Held-out: QA — "QA environment"

A: "Push the build to the QA environment."
  Expected: ["QA"]
  Held-out: "QA environment" is a QA compound noun → fires QA predicate. "push" alone is operational gap.

---

## H007-002 — Held-out: QA+TEST — "run smoke tests and QA"

A: "Run smoke tests and QA the release candidate."
  Expected: ["TEST", "QA"]
  Held-out: "run smoke tests" → TEST (sub-phase 5.3); "QA the release candidate" → QA (sub-phase 7.1 release vocabulary).

---

## H007-003 — Held-out: gap — "testing complete"

A: "Testing is complete — ready to ship."
  Expected: []
  Held-out: "testing complete" without QA abbreviation or test-writing verb → gap.

---

## C007-001 — Corruption v6-02: sprint review → QA (wrong)

A: "Present the completed features at the sprint review."
  Correct: []
  Corrupt: ["QA"] ← v6-02 (sprint review incorrectly mapped to QA)
  Teaching: Sprint ceremonies (review, retrospective, planning) without QA abbreviation → gap. Sprint review ≠ QA gate.

---

## C007-002 — Corruption: QA last order violated

A: ["IMPLEMENT", "TEST", "QA"] — flow execution order corrupted
  Correct: IMPLEMENT → TEST → QA (QA always last)
  Corrupt: QA → TEST → IMPLEMENT ← wrong (QA executed first)
  Teaching: QA gate always executes last. It validates the complete output of all preceding flows.
