# Layer 0-B — Router Pairs (056)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-056-001 — PWAInstallTest: routing

A: "Write E2E tests for PWA installation — assert install button and verify install prompt modal." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (button, modal). AllSubPhaseIds: [5.3, 1.5].

---

## C0B-056-002 — PWAInstallTest: install prompt banner → 1.5

A: Does an "install prompt banner" route to 1.5?
B: Yes. Banner → 1.5.

---

## C0B-056-003 — PWAInstallTest: "configure manifest" gap

A: "Configure the web app manifest." Predicates []. Routing?
B: knowledge_edge_synthetic. No routing.

---

## C0B-056-004 — PWAInstallTest: [TEST, UI, QA → 7.1] before release

A: "Write PWA install tests, assert install button, and QA before release." AllSubPhaseIds?
B: [5.3, 1.5, 7.1]. QA → 7.1.

---

## C0B-056-005 — PWAInstallTest: IMPLEMENT → 4.1

A: "Implement PWA install capability and write E2E tests for install prompt modal." AllSubPhaseIds?
B: [4.1, 5.3, 1.5]. TransitionType: feature_addition.
