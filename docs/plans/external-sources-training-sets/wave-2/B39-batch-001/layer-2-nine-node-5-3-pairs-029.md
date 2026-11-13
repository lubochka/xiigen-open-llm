# Layer 2 — Nine-Node Flow Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: PasswordResetTest | Node 01 A = SelectedLogicalTransition

---

## B39-nine-029 — PasswordResetTest nine-node trace

### Node 01 — DomainSplitterUnit
SelectedLogicalTransition = E2ETestingFlow53.
Domains: reset request form (email input), reset link delivery, expired token modal, new password form (password + confirm-password), success screen.

### Node 02 — DomainTranslatorUnit
Scenarios: (1) Valid email → reset email sent state. (2) Expired reset link → expired token modal. (3) Valid new password → success screen. (4) Mismatched passwords → inline error.

### Node 03 — SchemePlannerUnit
Framework: Playwright. Pattern: POM (Page Object Model). Test class: PasswordReset.spec.ts. Fixtures: mock token generation via test API endpoint to avoid email delivery.

### Node 04 — DomainProcessorUnit
4 test cases. Mock token endpoint injects valid/expired tokens. Assertions on modal appearance, success screen URL, inline error content.

### Node 05 — ApplierUnit
Naming: passwordReset_expiredToken_modal. File: tests/e2e/passwordReset.spec.ts.

### Node 06 — ResultTranslatorUnit
Output: 4 test cases. Coverage: happy path (valid token) + expired token + mismatch error. No gap on required paths.

### Node 07 — ArbiterSelectorUnit
Coverage arbiter selected.

### Node 08 — ArbiterUnit
Verdict: PASS. All required password reset scenarios covered.

### Node 09 — FormatterUnit
Output: passwordReset.spec.ts (4 tests). output_status: PASS.
