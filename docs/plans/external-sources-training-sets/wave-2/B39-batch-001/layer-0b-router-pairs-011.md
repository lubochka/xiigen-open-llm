# Layer 0-B — Router Pairs (011)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-011-001 — UI+DOCUMENT: component documentation

A: ["UI", "DOCUMENT"] | "Document all button variants in the design system"
B: UI → 1.5; DOCUMENT → DocumentationFlow62. Two flows. TransitionType: documentation (no IMPLEMENT).

---

## C0B-011-002 — IMPLEMENT+UI+QA: no TEST in this path

A: ["IMPLEMENT", "UI", "QA"] | "Build the cookie consent banner and QA it"
B: IMPLEMENT → 4.1; UI → 1.5; QA → 7.3. Three flows. TransitionType: feature_addition. QA last.
Reason: No test-writing verb → TEST does not fire. Three predicates from three independent triggers.

---

## C0B-011-003 — TEST+QA without UI

A: ["TEST", "QA"] | "Write API tests for the product endpoint and QA sign-off"
B: TEST → 5.3 (API integration tests); QA → 7.3. Two flows. No UI vocabulary → UI does not fire.
Reason: API tests without UI vocabulary → TEST only (not UI). QA gate fires via abbreviation.

---

## C0B-011-004 — IMPLEMENT+TEST+UI: complete cycle without QA

A: ["IMPLEMENT", "TEST", "UI"] | "Build notification toast component and write render tests"
B: IMPLEMENT → 4.1; TEST → 5.3 (render/UI context); UI → 1.5. Three flows. TransitionType: feature_addition.

---

## C0B-011-005 — Gap: "performance profiling of UI"

A: [] | "Profile the UI rendering performance in Chrome DevTools"
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "profile" + "Chrome DevTools" → observational/operational gap. No test-writing verb or QA abbreviation.
