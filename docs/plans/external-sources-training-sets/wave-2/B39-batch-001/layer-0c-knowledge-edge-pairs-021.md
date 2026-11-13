# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E021-001 — "performance tests" fires TEST in E2E context; "page" fires UI

A: "Write performance tests that assert the product listing page loads within 2 seconds."
B: ["TEST", "UI"] — "write performance tests" + "assert" → TEST; "product listing page" → UI (page). Performance testing is categorized under TEST predicate.
Edge: "performance tests" is a TEST compound in E2E context. "page" reliably fires UI.

---

## E021-002 — "time-to-interactive" is gap vocabulary but "modal" fires UI

A: "Assert that the search results modal renders within 500ms."
B: ["TEST", "UI"] — "assert" → TEST; "search results modal" → UI (modal). "time-to-interactive" → gap (performance metric term, not predicate vocabulary). "500ms" → gap.
Edge: Performance metric terms (time-to-interactive, FCP, LCP, CLS) are gap vocabulary by themselves. The predicate fires on test verb and component vocabulary.

---

## E021-003 — "profile" is gap; "layout" fires UI alone

A: "Profile the rendering pipeline to find layout thrashing causes."
B: ["UI"] — "profile" → gap; "rendering pipeline" → gap; "layout" → UI (layout fires). "thrashing" → gap.
Edge: "profile" is consistently gap. UI fires on "layout" even with gap surrounding verbs.

---

## E021-004 — "LCP (Largest Contentful Paint)" is gap; "component" fires UI

A: "Verify that the hero image component achieves an LCP score under 1.5s."
B: ["TEST", "UI"] — "verify" → TEST; "hero image component" → UI (component, visual). "LCP score" → gap (performance metric abbreviation — even with expansion, it is gap).
Edge: Web performance metrics (LCP, FCP, CLS, TTI (Time to Interactive)) are all gap vocabulary — they do not fire any predicate. Predicates fire on test verbs and UI component names.

---

## E021-005 — "performance regression test cases" fires TEST; "page component" fires UI

A: "Write performance regression test cases for every page component."
B: ["TEST", "UI"] — "write performance regression test cases" → TEST (test cases compound); "page component" → UI (page, component).
Edge: "performance regression test cases" is a valid TEST compound. Multiple words between "write" and "test cases" do not break the TEST trigger.
