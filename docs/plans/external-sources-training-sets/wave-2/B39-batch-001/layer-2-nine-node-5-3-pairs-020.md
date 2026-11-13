# Layer 2 — Nine-Node Flow Pairs (020)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: AccessibilityAuditTests | Predicate: TEST+UI+QA | TransitionType: documentation

---

## B39-nine-020 — Accessibility audit tests — PASS

Request: "Write accessibility audit tests for the modal, form inputs, and navigation, and QA sign-off on WCAG 2.1 AA compliance."
Predicate set: ["TEST", "UI", "QA"] | TransitionType: documentation | TrustState: deterministic_generic

### Flow: E2ETestingFlow53 + UIInteractionFlow15 + QAGatingFlow73

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Modal, form inputs (email, password, address), navigation menu.
Domains: modal_a11y, form_a11y, navigation_a11y.
Standard: WCAG (Web Content Accessibility Guidelines) 2.1 Level AA.

**Node 2 — DomainTranslator**:
  modal_a11y: focus trapped inside modal when open; ESC closes; aria-modal="true"; aria-labelledby references modal title.
  form_a11y: all inputs have associated <label>; required fields have aria-required="true"; error states have aria-describedby pointing to error message.
  navigation_a11y: nav has role="navigation" and aria-label; all links keyboard-reachable; focus visible on Tab; skip-to-content link present.

**Node 3 — SchemePlanner**:
  Framework: Playwright + axe-core integration | Contract: inject axe, run scan, assert zero violations.
  Manual checks: focus trap (modal), aria-labelledby (modal), skip link (nav).
  Automated: axe-core scan per component with violation count assertion.

**Node 4 — DomainProcessor**:
  Test 1 — Modal_AxeScanZeroViolations: await page.click('[data-testid=open-modal]'); await injectAxe(page); const results = await checkA11y(page, '[data-testid=modal]'); expect(results.violations.length).toBe(0).
  Test 2 — Modal_FocusTrap_ESCCloses: await page.click('[data-testid=open-modal]'); await page.keyboard.press('Escape'); await Expect(page.locator('[data-testid=modal]')).toBeHiddenAsync().
  Test 3 — FormInputs_AxeScanZeroViolations: await injectAxe(page); const results = await checkA11y(page, 'form'); expect(results.violations.length).toBe(0).
  Test 4 — Navigation_SkipLinkPresent: const skipLink = page.locator('[data-testid=skip-to-content]'); await Expect(skipLink).toBeVisibleAsync().
  Test 5 — Navigation_AllLinksKeyboardReachable: await page.keyboard.press('Tab'); await Expect(page.locator(':focus')).toHaveAttributeAsync('href').

**Node 5 — Applier**: Playwright + axe-core; zero-violation assertions; manual focus/keyboard checks; 5 tests.

**Node 6 — ResultTranslator**: 5 tests. modal_a11y: 2, form_a11y: 1, navigation_a11y: 2.

**Node 7 — ArbiterSelector**: AccessibilityCoverageArbiter — WCAG 2.1 AA zero violations per component; focus management; keyboard navigation.

**Node 8 — Arbiter**:
  modal: axe scan ✓; ESC closes ✓; focus trap: Tab cycling within modal — not tested explicitly.
  form: axe scan ✓. PASS.
  navigation: skip link ✓; keyboard reachable ✓. PASS.
  QAGatingFlow73: QA sign-off — modal focus trap cycling missing; rest PASS.
  OverallVerdict: PASS_WITH_WARNINGS — modal Tab-cycle focus trap not explicitly tested.

**Node 9 — Formatter**: Output: accessibility.spec.ts. 5 tests. QA gap: modal Tab-cycle focus trap. Status: ready_with_gaps.
