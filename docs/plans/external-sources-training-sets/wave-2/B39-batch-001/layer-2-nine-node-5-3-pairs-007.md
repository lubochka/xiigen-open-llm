# Layer 2 — Nine-Node Flow Pairs (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: nine_node_flow (E2ETestingFlow53)

---

## B39-nine-007 — Tab panel keyboard navigation tests — FAIL

Request: "Write keyboard navigation tests for the settings tab panel: Tab/arrow key navigation, panel content display, and ARIA attributes."

### Flow: E2ETestingFlow53 + UIInteractionFlow15

**Node 1**: Target: Settings tab panel | Features: tab selection, keyboard navigation, ARIA tab role attributes
**Node 2**: Test gates: click tab → panel shows, arrow key navigation between tabs, Tab key moves to panel content, ARIA role/aria-selected attributes, Enter/Space activates tab
**Node 3**: Test 1 — Click tab: click "Security" tab → security panel shown ✓
  Test 2 — Arrow navigation: right arrow → next tab activated ✓
  Test 3 — Tab key to content: Tab from tab bar → focus moves to first interactive element in panel ✓
  Test 4 — ARIA role: tabs have role="tab", panels have role="tabpanel" ✓
  Test 5 — aria-selected: active tab has aria-selected="true" ✓
  Test 6 — Enter/Space: Enter activates tab ✓; Space activates tab ✗ FAIL (Space scrolls page instead)
  Test 7 — Home/End keys: Home → first tab, End → last tab ✗ FAIL (keys not implemented)
**Node 4**: Two critical keyboard gaps: Space key behavior (scrolls instead of activating), Home/End not implemented.
**Node 5**: Arbiter: UIInteractionCoverageArbiter
**Node 6**: Coverage: click PASS, arrow PASS, Tab-to-content PASS, ARIA PASS, aria-selected PASS, Space FAIL, Home/End FAIL
**Node 7**: FAIL — two keyboard accessibility failures block merge.
**Node 8**: Test report: tab-panel-keyboard-E2E.md | BUG-521 (Space key), BUG-522 (Home/End keys)
**Node 9**: Verdict: FAIL — tab panel blocked. Fix BUG-521 and BUG-522 before re-test.
