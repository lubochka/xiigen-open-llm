# Layer 2 — Nine-Node Flow Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: nine_node_flow (E2ETestingFlow53)

---

## B39-nine-006 — Carousel keyboard navigation tests — PASS

Request: "Write keyboard navigation tests for the product image carousel: arrow navigation, dot indicators, and auto-play pause."

### Flow: E2ETestingFlow53 + UIInteractionFlow15

**Node 1**: Target: Product image carousel | Features: arrow navigation, dot indicators, auto-play pause on focus
**Node 2**: Test gates: right/left arrow navigation, dot indicator selection, auto-play pauses on keyboard focus, ARIA live region announces slide change
**Node 3**: Test 1 — Right arrow: press → next slide displayed, dot indicator updates ✓
  Test 2 — Left arrow: press → previous slide (wraps at start) ✓
  Test 3 — Dot indicator: click dot → jump to slide, URL fragment updates ✓
  Test 4 — Auto-play pause: Tab focus on carousel → auto-play stops ✓
  Test 5 — Auto-play resume: Tab focus leaves → auto-play resumes ✓
  Test 6 — ARIA live region: slide change announced to screen readers ✓
  Test 7 — Touch swipe: swipe left/right changes slide on mobile ✓
**Node 4**: All 7 tests pass. Full keyboard + touch + accessibility coverage.
**Node 5**: Arbiter: UIInteractionCoverageArbiter
**Node 6**: Coverage: arrow-nav PASS, dots PASS, auto-play-pause PASS, auto-play-resume PASS, ARIA PASS, touch PASS
**Node 7**: PASS — carousel keyboard and accessibility tests complete.
**Node 8**: Test report: carousel-keyboard-E2E.md | 7 tests, all green
**Node 9**: Verdict: PASS — carousel cleared for merge.
