# Layer 0-A — Compiler Pairs (021)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-021-001 — TEST+QA: performance UI test + QA gate

A: "Write performance tests that assert the product listing page loads within 2 seconds and QA the p95 (95th-percentile) timing."
B: ["TEST", "QA"] — "write performance tests" + "assert" → TEST; "product listing page" → UI (page); "QA the p95 timing" → QA. "page" fires UI.
Correction: CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-021-002 — TEST+UI: assert time-to-interactive for modal

A: "Assert that the search results modal renders fully within 500ms (milliseconds) of the trigger."
B: ["TEST", "UI"] — "assert" → TEST; "search results modal" → UI (modal). Performance timing assertion with UI vocabulary.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-021-003 — gap: "profile the rendering pipeline"

A: "Profile the rendering pipeline to find layout thrashing causes."
B: ["UI"] — "profile" → gap (not test-writing); "rendering pipeline" → gap; "layout thrashing" → "layout" fires UI. No test-writing verb → no TEST.
CompiledPredicateIds: ["UI"]

---

## C0A-021-004 — TEST+UI: verify LCP (Largest Contentful Paint) for hero image

A: "Verify that the hero image component achieves an LCP score under 1.5 seconds in the performance test suite."
B: ["TEST", "UI"] — "verify" → TEST; "hero image component" → UI (component, visual); "performance test suite" → TEST (test suite compound).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-021-005 — TEST+UI+QA: full perf regression + QA sign-off

A: "Write performance regression test cases for every page component and QA sign-off on the timing baselines."
B: ["TEST", "UI", "QA"] — "write regression test cases" → TEST; "page component" → UI (page, component); "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
