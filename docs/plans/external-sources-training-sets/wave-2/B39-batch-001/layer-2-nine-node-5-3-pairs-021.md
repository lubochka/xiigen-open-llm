# Layer 2 — Nine-Node Flow Pairs (021)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: PerformanceUiTests | Predicate: TEST+QA | TransitionType: documentation

---

## B39-nine-021 — Performance UI tests — PASS_WITH_WARNINGS

Request: "Write performance tests that assert the product listing page loads within 2 seconds and QA the p95 timing."
Predicate set: ["TEST", "UI", "QA"] | TransitionType: documentation | TrustState: deterministic_generic

### Flow: E2ETestingFlow53 + UIInteractionFlow15 + QAGatingFlow73

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Product listing page — initial load time, LCP (Largest Contentful Paint) for hero image, TBT (Total Blocking Time), p95 load time.
Domains: page_load_time, lcp_assertion, p95_timing.

**Node 2 — DomainTranslator**:
  page_load_time: DOMContentLoaded event ≤ 2000ms; all product card images loaded ≤ 2000ms.
  lcp_assertion: LCP element (first product image) renders ≤ 1500ms.
  p95_timing: 95th percentile page load across 20 repeated test runs ≤ 2000ms.

**Node 3 — SchemePlanner**:
  Framework: Playwright performance API + custom timing harness.
  Metrics: performance.timing.domContentLoadedEventEnd - performance.timing.navigationStart.
  Repeat: 20 runs for p95 calculation; collect array; sort; take index 19 (0-indexed).

**Node 4 — DomainProcessor**:
  Test 1 — PageLoad_Under2Seconds: await page.goto('/products', {waitUntil:'networkidle'}); const timing = await page.evaluate(() => performance.timing.domContentLoadedEventEnd - performance.timing.navigationStart); expect(timing).toBeLessThan(2000).
  Test 2 — LCP_Under1500ms: await page.goto('/products'); const lcp = await page.evaluate(() => new Promise(resolve => { new PerformanceObserver(list => resolve(list.getEntries().slice(-1)[0].startTime)).observe({type:'largest-contentful-paint',buffered:true}); })); expect(lcp).toBeLessThan(1500).
  Test 3 — P95_Under2000ms (20 runs): run page.goto('/products') 20 times; collect timings; sort; const p95 = timings[18]; expect(p95).toBeLessThan(2000).

**Node 5 — Applier**: Playwright performance API; 20-run sampling for p95; three assertions.

**Node 6 — ResultTranslator**: 3 tests. 20-run p95 measurement. LCP assertion. DOMContentLoaded assertion.

**Node 7 — ArbiterSelector**: PerformanceTimingArbiter — SLA: DOMContentLoaded ≤ 2000ms; LCP ≤ 1500ms; p95 ≤ 2000ms.

**Node 8 — Arbiter**:
  DOMContentLoaded: mean 1200ms ✓ (< 2000ms). PASS.
  LCP: mean 980ms ✓ (< 1500ms). PASS.
  p95: 2150ms ✗ (> 2000ms threshold). FAIL.
  QAGatingFlow73: QA gate: p95 exceeds SLA. Flag for performance investigation.
  OverallVerdict: PASS_WITH_WARNINGS — p95 timing 2150ms exceeds 2000ms SLA.

**Node 9 — Formatter**: Output: performance.spec.ts. 3 tests. QA flag: p95=2150ms > 2000ms. Recommendation: investigate network waterfall for blocking resources. Status: ready_with_gaps.
