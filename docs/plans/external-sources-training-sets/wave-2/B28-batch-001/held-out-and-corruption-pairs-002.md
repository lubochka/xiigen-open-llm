# Held-Out and Corruption Pairs (002)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H002-001 — Held-out: QA — QA checklist

A: "Complete the QA checklist before merging the feature branch."
  Expected: ["QA"]
  Held-out: "QA checklist" is a QA compound noun → fires QA predicate.

---

## H002-002 — Held-out: gap — "quality assurance" spelled out

A: "Run quality assurance on the new checkout flow."
  Expected: []
  Held-out: "quality assurance" spelled out → gap. QA predicate fires ONLY on "QA" abbreviation.

---

## H002-003 — Held-out: QA — "QA this"

A: "QA this build before we open the staging branch to testers."
  Expected: ["QA"]
  Held-out: "QA" as imperative verb → fires QA predicate. Sentence structure is irrelevant.

---

## C002-001 — Corruption v6-01: QA_AUTOMATION invented

A: "Set up QA automation for regression testing."
  Correct: ["QA"]
  Corrupt: ["QA", "QA_AUTOMATION"] ← v6-01 (QA_AUTOMATION is not a valid predicate)
  Teaching: QA_AUTOMATION is not a predicate. "QA automation" fires QA only via the abbreviation gate.

---

## C002-002 — Corruption v6-02: gap suppressed to QA

A: "Run the regression suite automatically on every commit."
  Correct: []
  Corrupt: ["QA"] ← v6-02 (automation context incorrectly mapped to QA)
  Teaching: Automated test execution without QA abbreviation → gap. CI/CD automation is not QA.
