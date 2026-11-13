# Layer 3 — Teaching Unit Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU009-001 — "drag and drop" fires UI

A: "The drag-and-drop zone does not highlight on Safari drag-over."
→ ["UI"] — "drag-and-drop zone", "drag-over" → UI (interaction pattern vocabulary).

---

## TU009-002 — "focus trap" fires UI (accessibility)

A: "The modal focus trap is not implemented — Tab key escapes to the background."
→ ["UI"] — "modal focus trap" → UI (focus management/accessibility vocabulary).

---

## TU009-003 — TEST+UI: writing accessibility tests

A: "Write WCAG compliance tests for the form controls: focus ring, label association, ARIA roles."
→ ["TEST", "UI"] — "write WCAG compliance tests" → TEST; "focus ring, label association, ARIA roles" → UI.
Teaching: Accessibility testing → TEST+UI. Both predicates from one accessibility-focused request.

---

## TU009-004 — "card" fires UI when used as component

A: "The product card hover animation shows a 2px jitter on Firefox."
→ ["UI"] — "product card" + "hover animation" → UI. "Firefox" is browser context, not gap.
Teaching: "card" as UI component (product card, user card, card component) → UI. Not to be confused with credit card data.

---

## TU009-005 — QA sub-phase 7.1 for smoke test + release pipeline

A: "Write smoke tests and QA the staging build before the production release."
→ ["TEST", "QA"] — TEST → 5.3 (smoke test); QA → 7.1 (staging/production release vocabulary).
Teaching: Release pipeline vocabulary ("staging build", "production release") routes QA to 7.1 (ReleaseGateFlow71), not 7.3.
