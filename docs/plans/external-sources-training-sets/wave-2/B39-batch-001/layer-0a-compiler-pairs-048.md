# Layer 0-A — Compiler Pairs (048)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-048-001 — TEST+UI+QA three-predicate combo: canonical form

A: What is the canonical predicate firing pattern for a three-predicate TEST+UI+QA request?
B: TEST fires: one or more test-writing verbs (write, create, add [test noun], assert, validate, verify) present. UI fires: one or more UI lexicon words (button, form, modal, page, screen, input, badge, toggle, panel, dropdown, checkbox, component, toast, accordion, banner, etc.) present. QA fires: "QA" abbreviation present. Three independent triggers → three predicates. Routing: [5.3, 1.5, 7.3] (default) or [5.3, 1.5, 7.1] (release vocabulary).

---

## C0A-048-002 — TEST+UI+QA: order of predicates does not affect routing

A: "QA acceptance — write E2E tests — assert the admin panel sidebar." Does predicate order matter?
B: No. Predicates are compiled in any order. QA fires on "QA acceptance". TEST fires on "write E2E tests" + "assert". UI fires on "admin panel sidebar" → sidebar. Result: [TEST, UI, QA] regardless of which clause appears first. Routing: [5.3, 1.5, 7.3].

---

## C0A-048-003 — TEST+UI+QA: "quality assurance" spelled out does NOT fire QA

A: "Write E2E tests for the notification page and quality assurance sign-off before merge." Predicates?
B: TEST (write E2E tests). UI (notification page → page). QA: "quality assurance" spelled out → gap. QA does NOT fire. Result: [TEST, UI]. Not [TEST, UI, QA]. QA fires ONLY on the two-letter abbreviation "QA".

---

## C0A-048-004 — TEST+UI+QA: dense request with multiple clauses

A: "Add E2E test coverage for the dashboard navigation, verify the notification badge, assert the sidebar layout, and QA gate before sprint merge."
B: TEST (add, verify, assert → one TEST). UI (navigation, notification badge → badge, sidebar → sidebar, layout → layout — four triggers → one UI). QA (QA gate → 7.3). Three predicates: [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.3]. TransitionType: documentation.

---

## C0A-048-005 — TEST+UI+QA: TransitionType is always documentation without IMPLEMENT

A: Can [TEST, UI, QA] ever produce feature_addition?
B: No. [TEST, UI, QA] → documentation always when IMPLEMENT is absent. Adding QA does not change TransitionType. Only IMPLEMENT produces feature_addition. The three-predicate combo TEST+UI+QA is the most complex documentation-producing pattern in B39.
