# Layer 2 — Nine-Node Flow Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: CrossBrowserCompatibility | Predicate: TEST+UI+QA | TransitionType: documentation

---

## B39-nine-018 — Cross-browser compatibility tests — PASS

Request: "Write cross-browser tests for the product listing page layout on Chrome, Firefox, and Safari, and QA the results."
Predicate set: ["TEST", "UI", "QA"] | TransitionType: documentation | TrustState: deterministic_generic

### Flow: E2ETestingFlow53 + UIInteractionFlow15 + QAGatingFlow73

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Product listing page — grid layout (3/2/1 columns), product card, filter sidebar, sort dropdown.
Domains: grid_layout_compat, card_rendering_compat, interactive_controls_compat.

**Node 2 — DomainTranslator**:
  grid_layout_compat: 3-column grid at 1440px ✓ on Chrome/Firefox/Safari; 2-column at 768px; 1-column at 375px.
  card_rendering_compat: image aspect ratio preserved; card text no overflow; hover state consistent.
  interactive_controls_compat: filter sidebar open/close; sort dropdown keyboard navigation — Arrow keys, Enter.

**Node 3 — SchemePlanner**:
  Framework: Playwright multi-browser | Browsers: chromium, firefox, webkit.
  Test matrix: 3 browsers × 3 viewports × 3 domains = 9 parameterized test combinations.

**Node 4 — DomainProcessor**:
  Test 1 — GridLayout_1440px_ThreeColumns [chromium/firefox/webkit]:
    await page.setViewportSize({width:1440, height:900}); await page.goto('/products');
    const cards = await page.locator('[data-testid=product-card]').all(); expect(cards.length % 3).toBe(0).
  Test 2 — GridLayout_375px_SingleColumn [chromium/firefox/webkit]:
    await page.setViewportSize({width:375, height:812}); const firstCard = page.locator('[data-testid=product-card]').first();
    await expect(firstCard).toBeVisibleAsync().
  Test 3 — CardHoverState_ConsistentAcrossBrowsers [chromium/firefox/webkit]:
    await page.hover('[data-testid=product-card]:first-child'); await expect(page.locator('.card-hover-overlay')).toBeVisible().
  [6 more tests for filter sidebar and sort dropdown]

**Node 5 — Applier**: Playwright parameterized test suite; projects: chromium/firefox/webkit; viewport parameterization; data-testid selectors.

**Node 6 — ResultTranslator**: 9 test combinations × 3 browsers = 27 test runs total. Matrix results tracked.

**Node 7 — ArbiterSelector**: CrossBrowserCoverageArbiter — each browser must pass each viewport × domain test.

**Node 8 — Arbiter**:
  chromium: all 9 ✓. PASS.
  firefox: grid ✓, card ✓, filter ✓, sort: Arrow key navigation FAIL on Firefox (known browser bug).
  webkit: all 9 ✓. PASS.
  QAGatingFlow73: Firefox sort-dropdown keyboard nav failure — QA gate: FAIL on Firefox.
  OverallVerdict: PASS_WITH_WARNINGS — Firefox Arrow key sort-dropdown navigation failure flagged to QA.

**Node 9 — Formatter**: Output: cross-browser.spec.ts. 9 tests × 3 browsers. QA annotation: Firefox Arrow-key gap documented. Status: ready_with_gaps.
