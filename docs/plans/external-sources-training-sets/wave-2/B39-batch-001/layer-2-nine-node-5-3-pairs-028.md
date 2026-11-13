# Layer 2 — Nine-Node Flow Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: LoginFlowTest | Node 01 A = SelectedLogicalTransition

---

## B39-nine-028 — LoginFlowTest nine-node trace

### Node 01 — DomainSplitterUnit
SelectedLogicalTransition = E2ETestingFlow53.
Domains: login form (email, password inputs), login button state, invalid-credentials error toast, locked-account state, successful redirect.

### Node 02 — DomainTranslatorUnit
Scenarios: (1) Valid credentials → redirect to dashboard. (2) Empty fields → login button disabled. (3) Invalid credentials → error toast appears. (4) Locked account → locked-account error message.

### Node 03 — SchemePlannerUnit
Framework: Playwright. Pattern: POM (Page Object Model). Test class: LoginFlow.spec.ts. Fixtures: test accounts (valid, invalid, locked) seeded via API.

### Node 04 — DomainProcessorUnit
4 test cases written. Assertions on button state (empty), toast content (invalid), locked message (locked account), redirect URL (success).

### Node 05 — ApplierUnit
Naming: loginFlow_invalidCredentials_errorToast. File: tests/e2e/loginFlow.spec.ts.

### Node 06 — ResultTranslatorUnit
Output: 4 test cases. Coverage: happy path + 3 error states. Gap: empty-fields validation not explicitly asserted for each field independently.

### Node 07 — ArbiterSelectorUnit
Coverage arbiter selected.

### Node 08 — ArbiterUnit
Verdict: PASS_WITH_WARNINGS. Per-field empty validation not individually asserted — suite validates button disabled state only.

### Node 09 — FormatterUnit
Output: loginFlow.spec.ts (4 tests). output_status: PASS_WITH_WARNINGS. TODO: add per-field empty validation assertions.
