# Layer 3 — Teaching Unit Pairs (021)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU021-001 — "performance tests" fires TEST; not a separate PERFORMANCE predicate

A: "Write performance tests for the product listing page." What predicates fire?
B: TEST fires on "write performance tests". UI fires on "product listing page" (page). There is no PERFORMANCE predicate — performance is a modifier for TEST, not a separate predicate.
Result: ["TEST", "UI"]. Teaching: "performance tests" is a TEST compound. "performance" does not create a new predicate. TEST absorbs the modifier.

---

## TU021-002 — "p95" is gap vocabulary; "page" fires UI

A: "Assert that the p95 load time of the product listing page is under 2 seconds." What fires?
B: TEST fires on "assert". UI fires on "product listing page" (page). "p95" → gap (performance metric abbreviation, even if expanded to 95th-percentile).
Result: ["TEST", "UI"]. Teaching: Performance metric terms (p95, p99, LCP, FCP, CLS, TTI) are all gap vocabulary.

---

## TU021-003 — "profile" is gap; "layout" fires UI

A: "Profile the rendering pipeline to find layout thrashing." What fires?
B: UI fires on "layout". "profile" → gap. "rendering pipeline" → gap. Only UI fires.
Result: ["UI"]. Teaching: "profile" is an operational/diagnostic verb — consistently gap in B39.

---

## TU021-004 — "LCP (Largest Contentful Paint)" is gap; "component" fires UI

A: "Verify that the hero image component achieves an LCP score under 1.5s." What fires?
B: TEST fires on "verify". UI fires on "hero image component" (component → UI; visual → UI). "LCP score" → gap.
Result: ["TEST", "UI"]. Teaching: Even abbreviated performance metrics (LCP, FCP, CLS) are gap. The predicate fires on "component" and "verify".

---

## TU021-005 — "performance regression test cases" is a valid TEST compound

A: "Write performance regression test cases for every page component." What fires?
B: TEST fires on "write performance regression test cases" (multi-word compound — "test cases" is the core TEST noun). UI fires on "page component" (page + component, two UI triggers).
Result: ["TEST", "UI"]. Teaching: Long multi-word compounds still fire TEST as long as "test cases", "test suite", "tests", "assertions", or "scenarios" is present as a core noun.
