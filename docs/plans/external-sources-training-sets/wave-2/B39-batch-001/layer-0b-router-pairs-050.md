# Layer 0-B — Router Pairs (050)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-050-001 — CoreWebVitalsTest: routing

A: "Write E2E tests to assert LCP, FID, and CLS scores on the product listing page." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-050-002 — CoreWebVitalsTest: "measure LCP" [UI] only

A: "Measure the LCP score for the landing page." Predicates [UI]. AllSubPhaseIds?
B: AllSubPhaseIds: [1.5]. No TEST (measure is gap). UI → 1.5 (landing page → page).

---

## C0B-050-003 — CoreWebVitalsTest: metric vocabulary does not change routing

A: Do LCP, FID, CLS metrics change routing away from 5.3 or 1.5?
B: No. LCP/FID/CLS are measurement metrics — they have no routing impact. TEST → 5.3, UI → 1.5 remain. Metric vocabulary only affects Node 07 arbiter selection inside E2ETestingFlow53.

---

## C0B-050-004 — CoreWebVitalsTest: [TEST, UI, QA → 7.1]

A: "Write CWV (Core Web Vitals) tests for the checkout page and QA sign-off before production." AllSubPhaseIds?
B: [5.3, 1.5, 7.1]. QA → 7.1 (before production).

---

## C0B-050-005 — CoreWebVitalsTest: "automate CWV measurement" → [UI] only

A: "Automate Core Web Vitals measurement for the landing page." AllSubPhaseIds?
B: AllSubPhaseIds: [1.5]. "automate" is gap for TEST. UI → 1.5 (landing page → page). Only 1.5 instantiates.
