# Held-Out and Corruption Pairs (006)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H006-001 — Held-out: QA — "(QA)" parenthetical

A: "Get testing (QA) sign-off before the production cut."
  Expected: ["QA"]
  Held-out: "(QA)" in parentheses is an abbreviation occurrence → fires QA predicate.

---

## H006-002 — Held-out: QA — "QA block"

A: "The release is under a QA block — critical bug found in checkout."
  Expected: ["QA"]
  Held-out: "QA block" is a QA compound noun → fires QA predicate.

---

## H006-003 — Held-out: gap — "continuous testing"

A: "Set up continuous testing for the microservices."
  Expected: []
  Held-out: "continuous testing" without test-writing verb → gap. "set up" is operational.

---

## C006-001 — Corruption v6-01: CONTINUOUS_INTEGRATION invented

A: "Run QA tests in the continuous integration pipeline."
  Correct: ["QA"]
  Corrupt: ["QA", "CONTINUOUS_INTEGRATION"] ← v6-01 (CONTINUOUS_INTEGRATION is not a valid predicate)
  Teaching: No CI/CD predicate exists. QA fires via "QA tests". CI/CD context is gap.

---

## C006-002 — Corruption: sub-phase 7.3 for release pipeline (wrong)

A: "QA the staging deployment before the production release."
  Correct: ["QA"] → sub-phase 7.1 (ReleaseGateFlow71) — release/staging vocabulary
  Corrupt: → sub-phase 7.3 (QAGatingFlow73) ← wrong (deployment/staging vocabulary → 7.1)
  Teaching: "staging deployment", "production release" → release pipeline vocabulary → 7.1. Feature acceptance vocabulary → 7.3.
