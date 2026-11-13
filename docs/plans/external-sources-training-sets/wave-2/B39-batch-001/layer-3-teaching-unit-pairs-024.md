# Layer 3 — Teaching Unit Pairs (024)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU024-001 — Complete rule summary: TEST predicate

A: Summarize the TEST predicate firing rules for B39 in one statement.
B: TEST fires when a test-writing verb (write, create, add, assert, validate, verify) co-occurs with a test noun (tests, test cases, test suite, spec, coverage, scenarios, assertions) OR when a standalone test assertion verb (assert, validate, verify) is present. TEST does not fire on: run, execute, automate, configure, Cypress, Playwright, Selenium, "e2e" alone (descriptive use), "tests" with only a gap verb.

---

## TU024-002 — Complete rule summary: UI predicate

A: Summarize the UI predicate firing rules for B39 in one statement.
B: UI fires when any word from the UI component/layout/accessibility lexicon appears: button, input, form, modal, page, screen, layout, component, responsive, accessibility, aria, aria-label, aria-live, navigation, nav, dropdown, tooltip, pagination, badge, chip, accordion, popover, snackbar, toast, skeleton, header, footer, sidebar, breadcrumb, focus trap, drag-and-drop zone, checkbox, toggle, radio, select, visual, placeholder. UI does NOT fire on: dashboard, API, service, controller, database, module, pipeline, configuration.

---

## TU024-003 — Complete rule summary: QA predicate

A: Summarize the QA predicate firing rules for B39 in one statement.
B: QA fires ONLY when the two-letter abbreviation "QA" appears literally in the text. Any form: "QA sign-off", "QA gate", "QA review", "QA acceptance", "QA the [noun]", "get QA", "pass QA". The spelled-out form "quality assurance" and any synonyms (quality check, quality review) are gap. QA routes to 7.3 by default; routes to 7.1 when explicit staging/deployment/release-pipeline vocabulary is present.

---

## TU024-004 — Complete rule summary: deduplication

A: Summarize the deduplication rule for B39.
B: Each predicate fires at most once per request, regardless of how many vocabulary words trigger it. "button + input + form + modal" → four UI triggers → one UI predicate. "write tests + assert + validate" → three TEST triggers → one TEST predicate. Deduplication applies to each predicate independently; it never merges different predicates.

---

## TU024-005 — Complete rule summary: gap vocabulary impact

A: What happens when gap vocabulary is the only vocabulary in a request?
B: Zero predicates → knowledge_edge routing → GapHandler → SCHEDULE_CAPABILITY_LEARNING emitted to AutonomousLearningPipeline (subproject 16). No flow is instantiated. No TransitionType is assigned. The gap_id is recorded and the gap vocabulary set is logged for future learning.
