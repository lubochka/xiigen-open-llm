# MANIFEST — B39-batch-001
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation
# Production date: 2026-05-05 (001–012 files); 2026-05-06 (013–024 extension)
# Total files: 201

---

## File Count by Layer

| Layer | Files |
|-------|-------|
| Layer 0-A (compiler pairs) | 24 (001–024) |
| Layer 0-B (router pairs) | 24 (001–024) |
| Layer 0-C (knowledge edge pairs) | 24 (001–024) |
| Layer 0-D (flow factory pairs) | 24 (001–024) |
| Layer 1 (transition orchestrator pairs) | 24 (001–024) |
| Layer 2 (nine-node 5.3/1.5 pairs) | 24 (001–024) |
| Layer 3 (teaching unit pairs) | 24 (001–024) |
| Held-out and corruption pairs | 24 (001–024) |
| Cross-layer synthesis | 8 (001–008) |
| MANIFEST | 1 |
| **Total** | **201** |

---

## E2ETestingFlow53 + UIInteractionFlow15 Nine-Node Flows (Layer 2)

| File | Target | Notable Tests | Verdict |
|------|--------|---------------|---------|
| 001 | Checkout form (original batch) | input validation, button, modal | PASS_WITH_WARNINGS |
| 002 | Registration form | required fields, email format, password strength | PASS |
| 003 | Confirmation modal | open/cancel/confirm/focus-trap/ESC | PASS_WITH_WARNINGS (backdrop inconsistency) |
| 004 | Product category dropdown | single/search/keyboard/multi-select | FAIL (Space multi-select, aria-selected) |
| 005 | Product listing responsive grid | 320px/768px/1440px breakpoints | PASS |
| 006 | Product image carousel | arrow nav/dots/auto-play/ARIA | PASS |
| 007 | Settings tab panel | click/arrow/ARIA/Space/Home-End keys | FAIL (Space key, Home/End) |
| 008 | File upload drag-and-drop | drop zone/validation/progress/keyboard | PASS |
| 009 | Dark mode toggle | theme switch/persistence/WCAG contrast | PASS_WITH_WARNINGS (button contrast 3.8:1) |
| 010 | Notification toast system | success/error/auto-dismiss/stacking/a11y | PASS |
| 011 | Search autocomplete | debounce/results/arrow/enter/escape | PASS_WITH_WARNINGS (p99 340ms) |
| 012 | Pagination | next/prev/direct-input/URL-sync/edge-cases | PASS |
| 013 | Cypress checkout cart flow | add-to-cart, quantity selector, proceed button | PASS |
| 014 | Playwright login form | valid/invalid/locked-out credential states | PASS_WITH_WARNINGS (empty fields gap) |
| 015 | Visual regression suite | button/card/modal baselines, QA gate | PASS_WITH_WARNINGS (secondary button pending) |
| 016 | BDD checkout scenarios | Gherkin scenarios, 3 payment methods | PASS_WITH_WARNINGS (bank transfer failure gap) |
| 017 | Snapshot test suite | button/input/dropdown components | PASS |
| 018 | Cross-browser compatibility | product listing page, Chrome/Firefox/webkit | PASS_WITH_WARNINGS (Firefox Arrow key gap) |
| 019 | Mobile responsive tests | 320px/375px, sticky footer, accordion | PASS |
| 020 | Accessibility audit tests | modal/form/nav, WCAG 2.1 AA, QA gate | PASS_WITH_WARNINGS (modal Tab-cycle gap) |
| 021 | Performance UI tests | page load, LCP, p95 timing, QA gate | PASS_WITH_WARNINGS (p95=2150ms > 2000ms SLA) |
| 022 | Form validation E2E | contact form, required/format/char-limit | PASS_WITH_WARNINGS (message required + char=500 gap) |
| 023 | Probationary rejected | AAA compliance claim + staging vocab = REJECTED | REJECTED |
| 024 | Complete nine-node rule summary | node roster, verdict codes, TransitionType | Reference |

---

## Key Teaching Points

1. **TEST fires on test-writing verbs** — write, create, add, assert, validate (in test context); NOT on run, execute, automate, check
2. **UI fires on component vocabulary** — button, modal, form, dropdown, checkbox, toggle, tab, carousel, tooltip, badge, chip, pagination, sidebar, header, footer, nav, breadcrumb, skeleton loader, accordion, popover, snackbar, screen, layout, responsive, accessibility, aria-label, aria-live, keyboard navigation, screen reader, WCAG; NOT on dashboard
3. **QA fires on abbreviation only** — "QA" fires; "quality assurance" spelled out → gap
4. **UI always routes to 1.5** — even in testing context. TEST routes to 5.3 for E2E/UI/browser tests
5. **TEST 5.1 vs 5.3** — "unit tests" + pure function → 5.1; "E2E/smoke/interaction/browser/visual regression/accessibility/performance" + UI vocabulary → 5.3
6. **QA 7.1 vs 7.3** — release/staging/deployment vocabulary → 7.1; feature acceptance/gate → 7.3 (default)
7. **Framework names are gap** — Playwright, Cypress, Selenium, TestCafe → gap on their own
8. **"automate" is gap** — not a TEST trigger. TEST requires test-writing verbs.
9. **QA always last** — QA gate validates all preceding flows; ordering constraint is absolute
10. **TransitionType** — IMPLEMENT → feature_addition; "refactor" → refactoring; TEST+UI (no IMPLEMENT) → documentation; QA alone → no contribution
11. **Deduplication** — multiple UI vocabulary triggers → one UI predicate; multiple TEST triggers → one TEST predicate
12. **v6 corruptions** — v6-01: invented predicates (PLAYWRIGHT, BDD, VISUAL_REGRESSION, MOBILE, PERFORMANCE, BROWSER, ACCESSIBILITY); v6-02: gap verb → predicate (run→TEST, check→TEST/QA, update→IMPLEMENT, configure→IMPLEMENT, profile→TEST, review→QA, resize→IMPLEMENT, describe→TEST); v6-03: UI routing collapsed into TEST routing

---

## Cross-Layer Synthesis Traces

| File | Scenario | Predicate Set |
|------|----------|---------------|
| 001 | Build modal + write tests + fix focus trap + QA | IMPLEMENT+TEST+UI+QA |
| 002 | Gap: "Automate UI tests with Cypress" | [] → GapHandler |
| 003 | Write responsive tests + fix breakpoints | TEST+UI |
| 004 | Build design system button + full cycle | IMPLEMENT+TEST+UI+DOCUMENT+QA |
| 005 | TEST alone: Cypress checkout flow (documentation TransitionType) | TEST+UI |
| 006 | TEST+UI+QA: visual regression full trace | TEST+UI+QA |
| 007 | IMPLEMENT+TEST+UI+QA: build harness + test + QA gate | IMPLEMENT+TEST+UI+QA |
| 008 | IMPLEMENT+UI+TEST+QA+DOCUMENT+ARCHITECTURE: six-predicate | IMPLEMENT+UI+TEST+QA+DOCUMENT+ARCHITECTURE |
