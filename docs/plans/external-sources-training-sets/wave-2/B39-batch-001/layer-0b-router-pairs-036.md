# Layer 0-B — Router Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-036-001 — Complete routing reference: Round 2 TEST disambiguation

A: For all Round 2 topics (CheckoutFlow through OrderTracking), is TEST always routed to 5.3?
B: Yes — all Round 2 topics are E2E/browser/UI interaction flows. None qualify as unit test context (5.1). Signals that confirm 5.3: explicit E2E vocabulary, UI component vocabulary co-occurring with TEST, multi-step flow language (flow, scenario, interaction). 5.1 would require: "unit test", "pure function", service/repository logic without UI vocabulary.

---

## C0B-036-002 — Complete routing reference: UI always 1.5 in Round 2

A: Does UI ever route to anything other than 1.5 in Round 2?
B: No. UI → 1.5 (UIInteractionFlow15) is absolute. All UI vocabulary (button, input, form, modal, page, screen, badge, toggle, checkbox, breadcrumb, toast, skeleton, panel, dropdown, component, sidebar, banner, navigation) routes to 1.5 regardless of context — testing, building, or reviewing.

---

## C0B-036-003 — Complete routing reference: QA disambiguation in Round 2

A: Summarize all cases in Round 2 where QA routes to 7.1 instead of 7.3.
B: QA → 7.1 when the request contains: "before release", "before staging", "staging deployment", "pre-production deployment", "release cut", "production deploy". Examples from Round 2: "QA sign-off before release" (035), "QA before staging deployment" (026). Default 7.3 applies for: "QA gate before merge", "QA acceptance", "QA gate before sprint review", "QA sign-off" without release vocabulary.

---

## C0B-036-004 — Complete routing reference: IMPLEMENT adds 4.1

A: When IMPLEMENT fires in a Round 2 request, how does it change routing?
B: IMPLEMENT adds sub-phase 4.1 (BackendServiceFlow41) to the flow list. It also changes TransitionType from documentation to feature_addition. Execution order shifts: IMPLEMENT (4.1) runs first; TEST+UI run in parallel after IMPLEMENT; QA always runs last. IMPLEMENT does not suppress any other sub-phase — all predicates still route to their own sub-phases.

---

## C0B-036-005 — Complete routing reference: AllSubPhaseIds patterns

A: List the AllSubPhaseIds patterns for common Round 2 predicate combinations.
B:
  [TEST, UI] → [5.3, 1.5]
  [TEST, UI, QA] → [5.3, 1.5, 7.3] (default) or [5.3, 1.5, 7.1] (release vocabulary)
  [IMPLEMENT, TEST, UI] → [4.1, 5.3, 1.5]
  [IMPLEMENT, TEST, UI, QA] → [4.1, 5.3, 1.5, 7.3]
  [UI] only → [1.5]
  [TEST] only → [5.3]
  [] gap → knowledge_edge_synthetic
