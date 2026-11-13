# Layer 2 — Nine-Node Flow Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: nine_node_flow (E2ETestingFlow53)

---

## B39-nine-003 — Modal interaction tests — PASS_WITH_WARNINGS

Request: "Write interaction tests for the confirmation modal: open, cancel, confirm, and focus trap."

### Flow: E2ETestingFlow53 + UIInteractionFlow15

**Node 1**: Target: Confirmation modal | Features: open trigger, cancel action, confirm action, focus trap
**Node 2**: Test gates: modal open, cancel closes without action, confirm triggers action, focus trap keyboard navigation, ESC key closes
**Node 3**: Test 1 — Open: trigger button click → modal visible ✓
  Test 2 — Cancel: cancel button → modal closes, no action taken ✓
  Test 3 — Confirm: confirm button → action executed, modal closes ✓
  Test 4 — Focus trap: Tab key cycles within modal, does not escape to background ✓
  Test 5 — ESC key: ESC closes modal without action ✓
  Test 6 — Backdrop click: click outside modal → modal closes ✗ WARNING (behavior inconsistent: closes on desktop, does not close on mobile)
**Node 4**: 5/6 tests pass. Warning: backdrop click behavior inconsistent across devices.
**Node 5**: Arbiter: UIInteractionCoverageArbiter
**Node 6**: Coverage: open PASS, cancel PASS, confirm PASS, focus trap PASS, ESC PASS, backdrop WARNING
**Node 7**: PASS_WITH_WARNINGS — backdrop click inconsistency filed as UX ticket.
**Node 8**: Test report: modal-interaction-E2E.md | WARNING: UX-112 filed (backdrop click inconsistency)
**Node 9**: Verdict: PASS_WITH_WARNINGS — modal cleared with UX ticket for backdrop behavior.
