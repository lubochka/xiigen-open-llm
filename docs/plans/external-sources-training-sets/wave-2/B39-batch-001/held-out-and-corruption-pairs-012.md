# Held-Out and Corruption Pairs (012)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H012-001 — Held-out: UI — "snackbar"

A: "The error snackbar overlaps the bottom navigation bar on mobile."
  Expected: ["UI"]
  Held-out: "error snackbar" + "bottom navigation bar" → UI (component + layout vocabulary).

---

## H012-002 — Held-out: IMPLEMENT+TEST+UI+QA — five-predicate with DOCUMENT

A: "Build the onboarding wizard, write step navigation tests, fix progress bar styling, document the component API, and QA sign-off."
  Expected: ["IMPLEMENT", "TEST", "UI", "DOCUMENT", "QA"]
  Held-out: Five predicates, five flows, QA last. TransitionType: feature_addition.

---

## H012-003 — Held-out: gap — "Cypress update"

A: "Update Cypress from 12.x to 13.x."
  Expected: []
  Held-out: "update Cypress" → dependency/version management gap. Framework name + operational verb → gap.

---

## C012-001 — Corruption v6-03: five-predicate collapsed to three

A: ["IMPLEMENT", "TEST", "UI", "DOCUMENT", "QA"]
  Correct: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, DocumentationFlow62, QAGatingFlow73]
  Corrupt: [ImplementationFlow41, UIInteractionFlow15, QAGatingFlow73] ← v6-03 (TEST and DOCUMENT suppressed)
  Teaching: No cap on flow count. All five predicates independently instantiate flows. v6-03 illegally collapses.

---

## C012-002 — Corruption: UI+QA TransitionType = feature_addition (wrong)

A: ["UI", "QA"] | "Fix button styling and QA sign-off"
  Correct: TransitionType: documentation (no IMPLEMENT)
  Corrupt: TransitionType: feature_addition ← wrong (QA and UI alone do not produce feature_addition)
  Teaching: TransitionType feature_addition requires IMPLEMENT predicate. UI+QA without IMPLEMENT → documentation.
