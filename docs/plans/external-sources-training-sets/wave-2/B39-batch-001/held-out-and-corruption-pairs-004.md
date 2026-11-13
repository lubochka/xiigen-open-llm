# Held-Out and Corruption Pairs (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H004-001 — Held-out: UI — "badge" on icon

A: "The notification badge count does not update after marking messages as read."
  Expected: ["UI"]
  Held-out: "notification badge count" → UI (badge component vocabulary).

---

## H004-002 — Held-out: TEST+UI — "add assertions for checkbox"

A: "Add assertions for the checkbox checked/indeterminate states."
  Expected: ["TEST", "UI"]
  Held-out: "add assertions" → TEST; "checkbox checked/indeterminate states" → UI.

---

## H004-003 — Held-out: gap — "configure test timeouts"

A: "Configure Playwright test timeouts to 30 seconds."
  Expected: []
  Held-out: "configure" is operational gap; "Playwright" is framework name → gap.

---

## C004-001 — Corruption v6-01: UITest invented (merged predicate)

A: ["TEST", "UI"]
  Correct: [E2ETestingFlow53, UIInteractionFlow15] — two separate flows
  Corrupt: [UITestFlow] ← v6-01 (UITestFlow is a fabricated merged flow, not valid)
  Teaching: TEST and UI are independent predicates. No merged UITestFlow exists in the flow registry.

---

## C004-002 — Corruption: "fix" → IMPLEMENT (wrong)

A: "Fix the button focus ring color in high-contrast mode."
  Correct: ["UI"]
  Corrupt: ["IMPLEMENT", "UI"] ← wrong ("fix" incorrectly triggers IMPLEMENT)
  Teaching: "fix" is not in the IMPLEMENT lexicon. IMPLEMENT requires build/implement/create/develop/write verbs. "fix" fires UI only.
