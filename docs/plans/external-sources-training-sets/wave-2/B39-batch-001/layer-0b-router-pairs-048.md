# Layer 0-B — Router Pairs (048)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-048-001 — TEST+UI+QA three-predicate: complete routing reference

A: What is the complete routing for the three-predicate combo [TEST, UI, QA]?
B: TEST → 5.3 (E2ETestingFlow53). UI → 1.5 (UIInteractionFlow15). QA → 7.3 (QAGatingFlow73) default; → 7.1 (ReleaseGateFlow71) when release/staging vocabulary is present. AllSubPhaseIds: [5.3, 1.5, 7.3] or [5.3, 1.5, 7.1]. Three flows. QA always last.

---

## C0B-048-002 — TEST+UI+QA: "quality assurance" does not route to 7.3

A: "Write tests, assert the panel, and quality assurance before merge." Does QA → 7.3?
B: No. QA predicate did not fire — "quality assurance" spelled out is gap. Only [TEST, UI] fired. AllSubPhaseIds: [5.3, 1.5]. No QA flow.

---

## C0B-048-003 — TEST+UI+QA: dense request routing

A: "Add E2E coverage for dashboard navigation, verify notification badge, assert sidebar layout, and QA gate before sprint merge." AllSubPhaseIds?
B: TEST → 5.3. UI → 1.5 (navigation, badge, sidebar, layout — all → 1.5). QA → 7.3 (before sprint merge). AllSubPhaseIds: [5.3, 1.5, 7.3].

---

## C0B-048-004 — TEST+UI+QA: can QA route to both 7.1 and 7.3 in the same request?

A: If a request mentions "QA before merge and QA before release", does both 7.1 and 7.3 instantiate?
B: No. QA fires once (deduplication). One QA predicate → one routing decision. When conflicting signals are present, release vocabulary takes precedence → 7.1 overrides 7.3. Only one QA flow instantiates.

---

## C0B-048-005 — TEST+UI+QA: IMPLEMENT added to three-predicate combo

A: [IMPLEMENT, TEST, UI, QA]. AllSubPhaseIds and execution order?
B: AllSubPhaseIds: [4.1, 5.3, 1.5, 7.3] or [4.1, 5.3, 1.5, 7.1]. Order: BackendServiceFlow41 (4.1) → E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5) parallel → QAGatingFlow73 (7.3) or ReleaseGateFlow71 (7.1) last. TransitionType: feature_addition.
