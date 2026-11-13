# Layer 3 — Teaching Unit Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU036-001 — Complete TEST+UI reference: UI lexicon consolidation

A: List every UI vocabulary word that appeared in Round 2 (025–035) that fired UI.
B: button, form, page, modal, input, screen, badge, toggle, panel, checkbox, dropdown, component, accordion, toast — all confirmed in Round 2 files. Additional Round 1 confirmed: tab, carousel, sidebar, header, footer, breadcrumb, skeleton, navigation/nav, layout, responsive, accessibility, aria, aria-label, WCAG. All fire UI on first appearance per request.

---

## TU036-002 — Complete TEST+UI reference: gap verb consolidation

A: List every gap verb confirmed in Round 2 (025–035) that did NOT fire TEST.
B: run, check, execute, automate, scan, configure, track, monitor, describe — all confirmed gap in Round 2. Additional from Round 1: profile, review, resize, trigger, schedule, enable. These verbs never fire any predicate regardless of context.

---

## TU036-003 — Complete TEST+UI reference: TEST assertion verbs

A: List all standalone TEST assertion verbs confirmed in B39.
B: assert, validate, verify — three standalone assertion verbs that fire TEST in any context. Additional TEST triggers via test-writing verb + test noun: write tests, create test cases, add test coverage, add E2E tests, add specs, add scenarios.

---

## TU036-004 — Complete TEST+UI reference: TransitionType summary

A: When does TEST alone produce feature_addition?
B: Never. TEST alone → documentation. TEST+UI → documentation. TEST+UI+QA → documentation. Only IMPLEMENT produces feature_addition, and it overrides all other predicates. No combination of TEST, UI, or QA without IMPLEMENT ever produces feature_addition.

---

## TU036-005 — Complete TEST+UI reference: "add" disambiguation rule

A: State the complete "add" disambiguation rule for B39.
B: "add" + test-artifact noun → TEST. Test-artifact nouns: tests, test cases, E2E tests, test coverage, assertions, specs, scenarios, test suite. "add" + feature/product noun → IMPLEMENT. Feature/product nouns: button, component, page, feature, endpoint, service, module, integration, field. "add" + neither → gap. This rule applies in all 60 topic files in B39.
