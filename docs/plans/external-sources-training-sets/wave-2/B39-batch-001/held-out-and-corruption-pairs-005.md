# Held-Out and Corruption Pairs (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H005-001 — Held-out: UI — "dark mode" theme

A: "Add dark mode support for the settings page."
  Expected: ["IMPLEMENT", "UI"] — "add" (feature context) → IMPLEMENT; "dark mode" → UI.
  Held-out: "add" in feature-addition context fires IMPLEMENT. "dark mode" fires UI.

---

## H005-002 — Held-out: TEST — "write visual regression tests"

A: "Write visual regression tests for the header component at all breakpoints."
  Expected: ["TEST", "UI"]
  Held-out: "write visual regression tests" → TEST (5.3, E2E/visual context); "header component", "breakpoints" → UI.

---

## H005-003 — Held-out: gap — "CSS variables update"

A: "Update the CSS color variables in the design token file."
  Expected: ["UI"]
  Held-out: "CSS color variables" → UI predicate fires on styling vocabulary. This is not gap — "CSS variables" is UI vocabulary.

---

## C005-001 — Corruption v6-02: gap → UI (wrong — dashboard)

A: "Update the analytics dashboard."
  Correct: []
  Corrupt: ["UI"] ← v6-02 ("dashboard" incorrectly triggers UI)
  Teaching: "dashboard" is confirmed gap. Not in UI lexicon. Only sidebar, header, footer, nav etc. are UI.

---

## C005-002 — Corruption: QA before TEST in ordering

A: ["TEST", "UI", "QA"] — execution order corrupted
  Correct: E2ETestingFlow53 + UIInteractionFlow15 → QAGatingFlow73 (QA last)
  Corrupt: QAGatingFlow73 → E2ETestingFlow53 → UIInteractionFlow15 ← wrong (QA executed first)
  Teaching: QA always executes last. It validates the complete output of all preceding flows.
