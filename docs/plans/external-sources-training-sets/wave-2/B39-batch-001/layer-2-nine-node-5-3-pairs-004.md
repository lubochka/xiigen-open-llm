# Layer 2 — Nine-Node Flow Pairs (004)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: nine_node_flow (E2ETestingFlow53)

---

## B39-nine-004 — Dropdown component tests — FAIL

Request: "Write E2E tests for the product category dropdown: selection, search filter, keyboard navigation, and multi-select."

### Flow: E2ETestingFlow53 + UIInteractionFlow15

**Node 1**: Target: Product category dropdown | Features: single selection, search filter, keyboard navigation, multi-select
**Node 2**: Test gates: single selection, search filter narrows options, keyboard navigation (arrow keys), multi-select with checkboxes, clear selection
**Node 3**: Test 1 — Single selection: click item → item selected, dropdown closes ✓
  Test 2 — Search filter: type "elec" → shows "Electronics" and "Electrical" only ✓
  Test 3 — Keyboard navigation: arrow down/up moves highlight, Enter selects ✓
  Test 4 — Multi-select: checkbox click selects multiple, count badge updates ✓
  Test 5 — Clear selection: clear button → all selections removed ✓
  Test 6 — Keyboard multi-select: Space key to check/uncheck in keyboard mode ✗ FAIL
  Test 7 — Screen reader: aria-selected updates on selection ✗ FAIL (aria-selected not set)
**Node 4**: Critical: keyboard Space multi-select broken; aria-selected not implemented. Two FAIL gates.
**Node 5**: Arbiter: UIInteractionCoverageArbiter
**Node 6**: Coverage: single PASS, search PASS, arrow-nav PASS, multi-select PASS, clear PASS, keyboard-multi FAIL, a11y FAIL
**Node 7**: FAIL — two critical accessibility and keyboard gaps block merge.
**Node 8**: Test report: dropdown-E2E.md | BUG-445 (keyboard Space multi-select), BUG-446 (aria-selected missing)
**Node 9**: Verdict: FAIL — dropdown component blocked. Fix BUG-445 and BUG-446 before re-test.
