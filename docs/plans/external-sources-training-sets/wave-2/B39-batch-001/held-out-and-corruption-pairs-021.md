# Held-Out and Corruption Pairs (021)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H021-001 — Held-out: TEST+UI+QA — performance tests + QA gate

A: "Write performance tests that assert the product listing page loads within 2 seconds and QA the p95 timing."
  Expected: ["TEST", "UI", "QA"]
  Held-out: "write performance tests" + "assert" → TEST; "product listing page" → UI (page); "QA the p95 timing" → QA.

---

## H021-002 — Held-out: UI only — "profile rendering pipeline layout"

A: "Profile the rendering pipeline to find layout thrashing causes."
  Expected: ["UI"]
  Held-out: "profile" → gap; "layout" → UI (layout). Only UI fires.

---

## H021-003 — Held-out: TEST+UI — verify LCP for hero image component

A: "Verify that the hero image component achieves an LCP score under 1.5s in the performance test suite."
  Expected: ["TEST", "UI"]
  Held-out: "verify" + "test suite" → TEST; "hero image component" → UI (component, visual). "LCP score" → gap.

---

## C021-001 — Corruption v6-01: PERFORMANCE predicate invented

A: "Write performance tests for the product listing page."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "PERFORMANCE"] ← v6-01 (PERFORMANCE invented as a separate predicate)
  Teaching: "performance" is a modifier for TEST. It does not create a PERFORMANCE predicate. "performance tests" fires TEST+UI (from "page").

---

## C021-002 — Corruption v6-02: "profile" → TEST (wrong)

A: "Profile the rendering pipeline to find layout thrashing causes."
  Correct: ["UI"]
  Corrupt: ["TEST", "UI"] ← v6-02 ("profile" incorrectly treated as a TEST verb — model sees profiling as testing)
  Teaching: "profile" is diagnostic/operational vocabulary. It does not fire TEST. TEST requires test-writing verbs: write, create, assert, validate, verify. "profile" is gap.
