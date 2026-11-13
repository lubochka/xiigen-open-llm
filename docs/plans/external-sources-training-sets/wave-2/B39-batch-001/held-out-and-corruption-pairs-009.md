# Held-Out and Corruption Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H009-001 — Held-out: TEST — "write smoke tests"

A: "Write smoke tests for the homepage render on all three supported browsers."
  Expected: ["TEST", "UI"]
  Held-out: "write smoke tests" → TEST (5.3, smoke/E2E); "homepage render" → UI (render vocabulary in browser context).

---

## H009-002 — Held-out: TEST+UI+QA — "three-predicate accessibility"

A: "Write WCAG tests for the nav menu, fix focus ring visibility, and QA sign-off."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write WCAG tests" → TEST; "focus ring visibility", "nav menu" → UI; "QA sign-off" → QA. Three predicates.

---

## H009-003 — Held-out: gap — "Figma design review"

A: "Review the Figma designs for the new checkout flow."
  Expected: []
  Held-out: "review Figma designs" → design review gap. No implementation, test-writing, or QA abbreviation.

---

## C009-001 — Corruption: TEST 5.1 for UI/E2E context (wrong)

A: "Write E2E tests for the form validation."
  Correct: ["TEST"] → sub-phase 5.3 (E2ETestingFlow53) — E2E keyword
  Corrupt: → sub-phase 5.1 (UnitTestingFlow51) ← wrong (E2E tests are system-level → 5.3)
  Teaching: "E2E" keyword disambiguates to 5.3. E2E tests are never unit tests. UI/browser context → 5.3.

---

## C009-002 — Corruption v6-02: gap → TEST ("test browser compatibility")

A: "Test browser compatibility for the new navbar."
  Correct: []
  Corrupt: ["TEST"] ← v6-02 ("test" as noun incorrectly triggers TEST predicate)
  Teaching: "test browser compatibility" is observational. TEST requires test-writing verb (write, create, add). "test" as a bare operational verb in compatibility-checking context → gap.
