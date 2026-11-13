# Layer 2 — Nine-Node Flow Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: nine_node_flow (E2ETestingFlow53)

---

## B39-nine-009 — Dark mode toggle tests — PASS_WITH_WARNINGS

Request: "Write E2E tests for the dark mode toggle: theme switching, persistence across page reload, and color contrast compliance."

### Flow: E2ETestingFlow53 + UIInteractionFlow15

**Node 1**: Target: Dark mode toggle | Features: toggle switch, theme persistence (localStorage), color contrast compliance
**Node 2**: Test gates: toggle activates dark theme, toggle deactivates dark theme, theme persists after reload, dark mode color contrast ≥ 4.5:1 (WCAG AA), system preference respected
**Node 3**: Test 1 — Activate dark mode: toggle on → dark theme CSS class applied ✓
  Test 2 — Deactivate: toggle off → light theme restored ✓
  Test 3 — Persistence: toggle on → reload page → dark theme maintained ✓
  Test 4 — Color contrast (dark): primary text #E5E7EB on #1F2937 = 11.5:1 ✓ (WCAG AAA)
  Test 5 — Color contrast (dark mode buttons): button text #374151 on #6B7280 = 3.8:1 ✗ WARNING (below WCAG AA 4.5:1)
  Test 6 — System preference: prefers-color-scheme: dark → auto-applies dark mode ✓
**Node 4**: One color contrast failure in dark mode button variant. Non-critical component but below WCAG AA.
**Node 5**: Arbiter: UIInteractionCoverageArbiter
**Node 6**: Coverage: toggle-on PASS, toggle-off PASS, persistence PASS, text-contrast PASS, button-contrast WARNING, system-pref PASS
**Node 7**: PASS_WITH_WARNINGS — button contrast ratio 3.8:1 below WCAG AA threshold. Filed for next sprint.
**Node 8**: Test report: dark-mode-toggle-E2E.md | WARNING: A11Y-087 filed (dark mode button contrast)
**Node 9**: Verdict: PASS_WITH_WARNINGS — dark mode cleared with accessibility ticket for button contrast.
