# Layer 0-B — Router Pairs (021)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-021-001 — TEST+UI+QA → 5.3 + 1.5 + 7.3: performance UI test + QA gate

A: ["TEST", "UI", "QA"] | "Write performance tests that assert the product listing page loads within 2s and QA the p95 timing"
B: TEST → 5.3; UI → 1.5; QA → 7.3. Three flows.
Reason: "performance tests" → 5.3 (E2E/UI performance testing); "product listing page" → UI → 1.5; "QA the timing" → 7.3.

---

## C0B-021-002 — TEST+UI → 5.3 + 1.5: assert time-to-interactive for modal

A: ["TEST", "UI"] | "Assert that the search results modal renders fully within 500ms of the trigger"
B: TEST → 5.3; UI → 1.5. "assert" → 5.3; "search results modal" → UI → 1.5.

---

## C0B-021-003 — UI alone → 1.5: profile rendering pipeline

A: ["UI"] | "Profile the rendering pipeline to find layout thrashing causes"
B: UI → 1.5. "layout" → UI → 1.5. "profile" is gap. No TEST or IMPLEMENT fires.

---

## C0B-021-004 — TEST+UI → 5.3 + 1.5: verify LCP (Largest Contentful Paint) for hero image

A: ["TEST", "UI"] | "Verify that the hero image component achieves an LCP score under 1.5s in the performance test suite"
B: TEST → 5.3; UI → 1.5. "verify" + "test suite" → 5.3; "hero image component" → UI → 1.5.

---

## C0B-021-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: full perf regression + QA

A: ["TEST", "UI", "QA"] | "Write performance regression test cases for every page component and QA sign-off on timing baselines"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
AllSubPhaseIds: [5.3, 1.5, 7.3]
