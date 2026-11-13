# Layer 2 — Nine-Node Flow Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: UserRegistrationTest | Node 01 A = SelectedLogicalTransition

---

## B39-nine-027 — UserRegistrationTest nine-node trace

### Node 01 — DomainSplitterUnit
SelectedLogicalTransition = E2ETestingFlow53.
Domains: registration form (required fields, email format, password strength), submit button state, duplicate email error, success redirect.

### Node 02 — DomainTranslatorUnit
Scenarios: (1) All fields valid → submit → success redirect. (2) Missing required field → submit button disabled. (3) Invalid email format → inline error on email input. (4) Duplicate email → error modal.

### Node 03 — SchemePlannerUnit
Framework: Playwright. Pattern: POM (Page Object Model). Test class: UserRegistration.spec.ts. beforeEach: clear DB test user seeding via API.

### Node 04 — DomainProcessorUnit
4 test cases. API seeding creates/clears users before each test. Assertions on button state, inline errors, modal appearance, redirect URL.

### Node 05 — ApplierUnit
Naming: registration_duplicateEmail_errorModal. Async: await page.fill / await expect. File: tests/e2e/userRegistration.spec.ts.

### Node 06 — ResultTranslatorUnit
Output: 4 test cases, 12 assertions. Coverage: happy path + 3 validation error states.

### Node 07 — ArbiterSelectorUnit
Coverage arbiter selected.

### Node 08 — ArbiterUnit
Verdict: PASS. All required registration scenarios covered.

### Node 09 — FormatterUnit
Output: userRegistration.spec.ts (4 tests, 12 assertions). output_status: PASS.
