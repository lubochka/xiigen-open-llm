# Held-Out and Corruption Pairs (004)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H004-001 — Held-out: QA — "QA report"

A: "Generate the QA report for the sprint review."
  Expected: ["QA"]
  Held-out: "QA report" is a QA compound noun → fires QA predicate.

---

## H004-002 — Held-out: IMPLEMENT+QA

A: "Build the cart abandonment feature and QA it before launch."
  Expected: ["IMPLEMENT", "QA"]
  Held-out: "build the cart abandonment feature" → IMPLEMENT; "QA it" → QA as imperative verb. TransitionType: feature_addition.

---

## H004-003 — Held-out: gap — "SLA compliance"

A: "Ensure SLA compliance for the API endpoints."
  Expected: []
  Held-out: SLA, SLO, compliance monitoring → operational gap. No predicate fires.

---

## C004-001 — Corruption v6-01: DEVOPS invented

A: "Set up the CI/CD pipeline to run QA tests automatically."
  Correct: ["QA"]
  Corrupt: ["QA", "DEVOPS"] ← v6-01 (DEVOPS is not a valid predicate)
  Teaching: DEVOPS is not a predicate. CI/CD context is gap; QA fires only via "QA" abbreviation in "QA tests".

---

## C004-002 — Corruption: QA → TransitionType: feature_addition (wrong)

A: ["QA"] only | "QA sign-off on the release."
  Correct: QA does not set TransitionType (it is determined by IMPLEMENT, TEST, DOCUMENT, etc.)
  Corrupt: TransitionType: feature_addition ← wrong (QA alone never drives TransitionType)
  Teaching: QA is a gate predicate, not a TransitionType driver. TransitionType must come from another predicate.
