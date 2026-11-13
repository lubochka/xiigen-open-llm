# Held-Out and Corruption Pairs (023)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H023-001 — Held-out: TEST+UI+QA — three-predicate dense signal

A: "Write e2e test cases for the accessibility-compliant modal, validate every ARIA role, and QA the test suite."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write e2e test cases" + "validate" → TEST (deduplicated); "modal" + "ARIA role" → UI (deduplicated); "QA the test suite" → QA.

---

## H023-002 — Held-out: TEST+UI+QA — "update" is gap (no IMPLEMENT)

A: "Create test coverage for checkout button states, update the responsive layout, and QA sign-off."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "create test coverage" → TEST; "checkout button" + "responsive layout" → UI; "QA sign-off" → QA. "update" → gap.

---

## H023-003 — Held-out: TEST+UI+QA — "release acceptance" → QA → 7.3 (not 7.1)

A: "Verify the mobile screen layout for the checkout flow and QA the results for release acceptance."
  Expected: ["TEST", "UI", "QA"] routed to [5.3, 1.5, 7.3]
  Held-out: "verify" → TEST → 5.3; "screen" + "layout" → UI → 1.5; "QA the results" → QA → 7.3 (not 7.1; no staging/deploy vocabulary).

---

## C023-001 — Corruption v6-02: "update" → IMPLEMENT (three-predicate case)

A: "Create test coverage for checkout button states, update the responsive layout, and QA sign-off."
  Correct: ["TEST", "UI", "QA"]
  Corrupt: ["TEST", "UI", "QA", "IMPLEMENT"] ← v6-02 ("update" incorrectly fires IMPLEMENT)
  Teaching: "update" is gap for IMPLEMENT in B39. Adding "update" to the sentence should not change the predicate set. TransitionType remains documentation (not feature_addition).

---

## C023-002 — Corruption v6-02: "release" → QA → 7.1 (wrong)

A: "QA the results for release acceptance before shipping."
  Correct: QA → 7.3 (feature acceptance; no staging/deploy vocabulary)
  Corrupt: QA → 7.1 ← v6-02 ("release" alone shifts QA to 7.1)
  Teaching: "release" alone in "release acceptance" does not trigger 7.1 routing. 7.1 requires: "staging deployment", "pre-release pipeline", "release cut", "production cutover". "Release acceptance" = feature acceptance = 7.3.
