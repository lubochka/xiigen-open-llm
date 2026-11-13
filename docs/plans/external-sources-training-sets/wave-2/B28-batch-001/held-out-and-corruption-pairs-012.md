# Held-Out and Corruption Pairs (012)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H012-001 — Held-out: QA — "QA analyst"

A: "Assign a QA analyst to validate the new onboarding flow."
  Expected: ["QA"]
  Held-out: "QA analyst" is a QA role title → fires QA predicate.

---

## H012-002 — Held-out: IMPLEMENT+TEST+DOCUMENT+QA — four-predicate

A: "Implement the two-factor authentication, write tests, document the setup guide, and QA sign-off."
  Expected: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]
  Held-out: Four independent clauses → four predicates. QA last. TransitionType: feature_addition (IMPLEMENT present).

---

## H012-003 — Held-out: gap — "sprint planning"

A: "Plan the QA activities for next sprint."
  Expected: ["QA"]
  Held-out: "QA activities" contains QA abbreviation → fires QA predicate. "plan" alone would be gap, but "QA activities" is a QA compound noun that triggers QA.

---

## C012-001 — Corruption v6-03: four-predicate collapsed to two

A: ["IMPLEMENT", "TEST", "DOCUMENT", "QA"]
  Correct: [ImplementationFlow41, UnitTestingFlow51/E2EFlow53, DocumentationFlow62, QAGatingFlow73]
  Corrupt: [ImplementationFlow41, QAGatingFlow73] only ← v6-03 (TEST and DOCUMENT suppressed)
  Teaching: No cap on flow count. All four predicates independently fire their respective flows. v6-03 illegally collapses.

---

## C012-002 — Corruption: "qa" lowercase → gap (wrong)

A: "Get qa sign-off before the release."
  Correct: ["QA"]
  Corrupt: [] ← wrong (lowercase "qa" treated as gap)
  Teaching: Abbreviation gate is case-insensitive. "qa", "Qa", "QA" all fire QA predicate equally.
