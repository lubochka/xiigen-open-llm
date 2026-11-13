# Held-Out and Corruption Pairs (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H007-001 — Held-out: UI — "overflow" vocabulary

A: "The product name overflows its card container on small screens."
  Expected: ["UI"]
  Held-out: "overflows its card container" → UI (overflow + card component vocabulary).

---

## H007-002 — Held-out: TEST+UI — "assert carousel"

A: "Assert that the carousel advances to the next slide when the right arrow is pressed."
  Expected: ["TEST", "UI"]
  Held-out: "assert" → TEST (5.3, interaction context); "carousel", "right arrow" → UI.

---

## H007-003 — Held-out: gap — "browser compatibility"

A: "Test browser compatibility for the new modal on IE11."
  Expected: [] — "test browser compatibility" without test-writing verb → observational gap. "IE11" is browser context. Actually — is "test" as verb here a test-writing trigger? No — "test compatibility" is observational/operational. Gap.
  Held-out: "test compatibility" is observational (check, review). No test-writing verb (write, create, add, assert). Gap.

---

## C007-001 — Corruption v6-02: gap → UI ("check the UI")

A: "Check the UI before release."
  Correct: []
  Corrupt: ["UI"] ← v6-02 ("check the UI" incorrectly triggers UI predicate)
  Teaching: "check" is observational gap. UI predicate requires specific UI vocabulary (component names, states, layout). Generic "UI" without specific vocabulary is gap.

---

## C007-002 — Corruption v6-03: IMPLEMENT+TEST+UI+QA collapsed to IMPLEMENT+QA

A: ["IMPLEMENT", "TEST", "UI", "QA"]
  Correct: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
  Corrupt: [ImplementationFlow41, QAGatingFlow73] ← v6-03
  Teaching: TEST and UI predicates independently instantiate their own flows. No collapse to just IMPLEMENT+QA.
