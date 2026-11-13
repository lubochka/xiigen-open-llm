# Layer 0-B — Router Pairs (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: router

---

## C0B-004-001 — IMPLEMENT+UI routes to 4.1 + 1.5

A: ["IMPLEMENT", "UI"] | "Build the pagination component"
B: IMPLEMENT → sub-phase 4.1 (ImplementationFlow41); UI → sub-phase 1.5 (UIInteractionFlow15). Two flows.
Reason: "build" fires IMPLEMENT (4.1). "pagination component" fires UI (1.5). Independent routing.

---

## C0B-004-002 — IMPLEMENT+TEST+UI routes to three flows

A: ["IMPLEMENT", "TEST", "UI"] | "Build skeleton loader and write render tests"
B: IMPLEMENT → 4.1; TEST → 5.3 (UI/E2E context); UI → 1.5. Three flows.
Reason: Three predicates, three flows. TEST routes to 5.3 because of browser/render context.

---

## C0B-004-003 — IMPLEMENT+TEST+UI+QA routes to four flows

A: ["IMPLEMENT", "TEST", "UI", "QA"] | "Build tab component, write tests, fix styling, QA sign-off"
B: IMPLEMENT → 4.1; TEST → 5.3; UI → 1.5; QA → 7.3. Four flows. QA executes last.
Reason: Four predicates, four flows. TransitionType: feature_addition (IMPLEMENT present). QA always last.

---

## C0B-004-004 — TEST ambiguity: "validate UI" → TEST+UI

A: ["TEST", "UI"] | "Validate UI rendering at different viewport sizes"
B: TEST → 5.3 (responsive/browser context); UI → 1.5.
Reason: "validate" in UI/viewport context → TEST (5.3). "viewport sizes" → UI (1.5).

---

## C0B-004-005 — TransitionType: TEST+UI without IMPLEMENT → documentation

A: ["TEST", "UI"] | "Write tests for existing button interactions and document test coverage"
B: TEST → 5.3; UI → 1.5. TransitionType: documentation (no IMPLEMENT present).
Reason: TEST alone drives documentation TransitionType when IMPLEMENT is absent.
