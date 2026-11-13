# Layer 2 — Nine-Node Flow Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: VisualRegressionSuite | Predicate: TEST+UI+QA | TransitionType: documentation

---

## B39-nine-015 — Visual regression suite — PASS

Request: "Write visual regression tests for the component library buttons, cards, and modals and QA sign-off on all baseline snapshots."
Predicate set: ["TEST", "UI", "QA"] | TransitionType: documentation | TrustState: deterministic_generic

### Flow: E2ETestingFlow53 + UIInteractionFlow15 + QAGatingFlow73

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Component library — button variants (primary/secondary/disabled), card variants (default/hover/loading), modal variants (default/wide/fullscreen).
Domains: button_visual_regression, card_visual_regression, modal_visual_regression.

**Node 2 — DomainTranslator**:
  button_visual_regression: primary/secondary/disabled states match approved baselines at 1x and 2x density.
  card_visual_regression: default/hover/loading states match baselines; no overflow, no text truncation artifacts.
  modal_visual_regression: default/wide/fullscreen variants match baselines; overlay color, border-radius, shadow correct.

**Node 3 — SchemePlanner**:
  Tool: Percy (visual regression service — gap vocabulary) + Playwright | Pattern: component-level snapshot per variant.
  Test groups: ButtonVisualTests (3 variants × 2 densities = 6 snapshots), CardVisualTests (3 variants = 3 snapshots), ModalVisualTests (3 variants = 3 snapshots).

**Node 4 — DomainProcessor**:
  Test 1 — ButtonPrimary_MatchesBaseline: await page.goto('/storybook/button-primary'); await percySnapshot(page, 'Button-Primary').
  Test 2 — ButtonDisabled_MatchesBaseline: await page.goto('/storybook/button-disabled'); await percySnapshot(page, 'Button-Disabled').
  Test 3 — CardHover_MatchesBaseline: await page.hover('[data-testid=product-card]'); await percySnapshot(page, 'Card-Hover').
  Test 4 — ModalDefault_MatchesBaseline: await page.click('[data-testid=open-modal]'); await percySnapshot(page, 'Modal-Default').
  [6 additional snapshot tests for remaining variants]

**Node 5 — Applier**: Playwright + Percy pattern; one snapshot per variant; data-testid selectors for state triggers; 10 total snapshot tests.

**Node 6 — ResultTranslator**: 10 snapshot tests, 3 component groups. All approved baselines referenced.

**Node 7 — ArbiterSelector**: VisualRegressionArbiter — each variant has one approved baseline; diff threshold < 0.1%.

**Node 8 — Arbiter**:
  buttons: 2 of 3 variants covered ✓; missing: secondary button baseline.
  cards: 3 variants ✓. PASS.
  modals: 3 variants ✓. PASS.
  QAGatingFlow73: QA sign-off required on all baselines — 2/3 button baselines approved; secondary pending.
  OverallVerdict: PASS_WITH_WARNINGS — secondary button baseline pending QA approval.

**Node 9 — Formatter**: Output: visual-regression.spec.ts. 10 tests. QA gate: pending secondary-button baseline approval. Status: ready_with_gaps.
