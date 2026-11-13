# Layer 2 — Nine-Node Flow Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: Complete TEST+UI reference | Node 01 A = SelectedLogicalTransition

---

## B39-nine-036 — Complete nine-node round-2 reference

### Node 01 Rule (all Round 2 flows)
SelectedLogicalTransition = E2ETestingFlow53 for all Round 2 topics (CheckoutFlow through OrderTracking). This is non-negotiable. Any request that fires TEST in E2E/browser context → Node 1 outputs E2ETestingFlow53.

### Verdict Distribution (025–035)
| File | Topic | Verdict |
|------|-------|---------|
| 025 | CheckoutFlowTest | PASS |
| 026 | PaymentFlowTest | PASS_WITH_WARNINGS (bank transfer mock gap) |
| 027 | UserRegistrationTest | PASS |
| 028 | LoginFlowTest | PASS_WITH_WARNINGS (per-field empty validation) |
| 029 | PasswordResetTest | PASS |
| 030 | ProfileUpdateTest | PASS |
| 031 | SearchFlowTest | PASS |
| 032 | FilterFlowTest | PASS |
| 033 | CartFlowTest | PASS |
| 034 | WishlistFlowTest | PASS |
| 035 | OrderTrackingTest | PASS |

### Node 07 Arbiter Selection Rule
Default arbiter for all E2E/browser flow tests: coverage arbiter. Visual arbiter: only when "visual regression" or "screenshot baseline" is explicit. Performance arbiter: only when "LCP (Largest Contentful Paint)", "p95", "load time" metrics are explicit. Accessibility arbiter: only when "WCAG (Web Content Accessibility Guidelines)", "ARIA (Accessible Rich Internet Applications)", "screen reader" is explicit.

### Node 08 PASS_WITH_WARNINGS Pattern
All Round 2 PASS_WITH_WARNINGS verdicts share a pattern: one edge-case or error-state scenario is identified by Node 06 but not fully covered by the available fixtures or mocks. The suite ships with a TODO annotation rather than blocking.

### TransitionType at Layer 2 (all Round 2)
All Round 2 flows with [TEST, UI] produce TransitionType: documentation at Layer 2. This is consistent across all 11 topic files (025–035).
