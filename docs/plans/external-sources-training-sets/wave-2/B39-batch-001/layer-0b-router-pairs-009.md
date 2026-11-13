# Layer 0-B — Router Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-009-001 — IMPLEMENT+UI: build responsive layout

A: ["IMPLEMENT", "UI"] | "Implement responsive grid layout for the product listing"
B: IMPLEMENT → 4.1; UI → 1.5. Two flows. TransitionType: feature_addition.
Reason: "implement" → IMPLEMENT (4.1). "responsive grid layout" → UI (1.5).

---

## C0B-009-002 — TEST routes 5.3 for "E2E" disambiguation keyword

A: ["TEST"] | "Write E2E tests for the user login and redirect flow"
B: Sub-phase 5.3 → E2ETestingFlow53
Reason: "E2E" is explicit sub-phase 5.3 disambiguator.

---

## C0B-009-003 — UI fires on "theme" vocabulary → 1.5

A: ["UI"] | "Update the global color theme to match the new brand palette"
B: Sub-phase 1.5 → UIInteractionFlow15
Reason: "global color theme", "brand palette" → styling/theming vocabulary → UI (1.5).

---

## C0B-009-004 — QA: "QA the theme update" → 7.3

A: ["UI", "QA"] | "Apply new theme colors and QA the visual output"
B: UI → 1.5; QA → 7.3. Two flows. QA last.
Reason: "Apply new theme colors" → UI (1.5). "QA the visual output" → QA (7.3, feature context).

---

## C0B-009-005 — Gap: "check design mockups"

A: [] | "Check the Figma mockups against the implemented UI"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "check Figma mockups" is observational/design review gap. No test-writing verb, no QA abbreviation, no implementation verb.
