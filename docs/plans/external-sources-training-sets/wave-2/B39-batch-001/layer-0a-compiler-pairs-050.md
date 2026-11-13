# Layer 0-A — Compiler Pairs (050)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-050-001 — CoreWebVitalsTest: predicate compilation

A: "Write E2E test cases to assert LCP (Largest Contentful Paint), FID (First Input Delay), and CLS (Cumulative Layout Shift) scores pass the budget on the product listing page."
B: TEST (write, assert). UI ("product listing page" → page). Result: [TEST, UI]. TransitionType: documentation. LCP/FID/CLS are performance metrics — they do not fire any predicate. Only the test-writing verb and UI vocabulary fire.

---

## C0A-050-002 — CoreWebVitalsTest: "measure LCP" is gap

A: "Measure the LCP score for the landing page." Predicates?
B: "measure" is gap. "landing page" → page → UI fires. Result: [UI]. TEST does not fire.

---

## C0A-050-003 — CoreWebVitalsTest: "validate LCP passes budget" fires TEST

A: "Validate the LCP score passes the 2.5s budget for the checkout page." Predicates?
B: TEST (validate → standalone assertion verb). UI (checkout page → page). Result: [TEST, UI].

---

## C0A-050-004 — CoreWebVitalsTest: performance arbiter selected at Node 07

A: When Core Web Vitals metrics are explicit (LCP, FID, CLS), which arbiter does Node 07 select?
B: Performance arbiter. Triggered by explicit metric vocabulary: LCP, FID, CLS, INP (Interaction to Next Paint), TTFB (Time to First Byte), performance budget, score. Coverage arbiter is default; performance arbiter is selected when timing/score/budget vocabulary is detected.

---

## C0A-050-005 — CoreWebVitalsTest: "automate Core Web Vitals measurement" is gap for TEST

A: "Automate Core Web Vitals measurement for the landing page." Predicates?
B: "automate" is gap. "landing page" → page → UI fires. Result: [UI]. TEST does not fire from "automate".
