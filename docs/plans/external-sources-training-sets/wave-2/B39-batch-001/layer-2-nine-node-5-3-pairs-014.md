# Layer 2 — Nine-Node Flow Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: PlaywrightLoginFlow | Predicate: TEST+UI | TransitionType: documentation

---

## B39-nine-014 — Playwright login form E2E tests — PASS

Request: "Write Playwright test cases for the login form: valid credentials, invalid password, and locked-out account states."
Predicate set: ["TEST", "UI"] | TransitionType: documentation | TrustState: deterministic_generic

### Flow: E2ETestingFlow53 + UIInteractionFlow15

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Login form — email input, password input, submit button, error message, locked-out banner.
Domains: valid_login_path, invalid_password_path, locked_account_path.

**Node 2 — DomainTranslator**:
  valid_login_path: correct email + password → redirected to /dashboard; session cookie set.
  invalid_password_path: correct email + wrong password → "Invalid credentials" error visible.
  locked_account_path: correct email + locked account → "Account locked" banner visible; password input disabled.

**Node 3 — SchemePlanner**:
  Framework: Playwright + xUnit | Pattern: Page Object Model (POM)
  Class: LoginPage — selectors: [data-testid=email-input], [data-testid=password-input], [data-testid=submit-btn], [data-testid=error-message], [data-testid=locked-banner].

**Node 4 — DomainProcessor**:
  Test 1 — ValidCredentials_RedirectsToDashboard: await page.FillEmail('user@example.com'); await page.FillPassword('correct'); await page.ClickSubmit(); await Expect(page).ToHaveURLAsync('/dashboard').
  Test 2 — InvalidPassword_ShowsErrorMessage: await page.FillEmail('user@example.com'); await page.FillPassword('wrong'); await page.ClickSubmit(); await Expect(page.ErrorMessage).ToBeVisibleAsync().
  Test 3 — LockedAccount_ShowsBanner: await page.FillEmail('locked@example.com'); await page.FillPassword('any'); await page.ClickSubmit(); await Expect(page.LockedBanner).ToBeVisibleAsync(); await Expect(page.PasswordInput).ToBeDisabledAsync().

**Node 5 — Applier**: POM applied; Playwright async/await pattern; [Fact] async Task; data-testid selectors; 3 files.

**Node 6 — ResultTranslator**: 3 tests covering 3 login states. LoginPage.cs (POM), LoginTests.cs (3 tests).

**Node 7 — ArbiterSelector**: UIInteractionCoverageArbiter — valid path + error path + edge-case state per element.

**Node 8 — Arbiter**:
  email_input: used in all 3 paths ✓.
  password_input: valid ✓, invalid ✓, disabled (locked) ✓. PASS.
  submit_button: click action in all 3 paths ✓. PASS.
  error_message: shown on invalid password ✓; missing: empty fields test.
  locked_banner: shown on locked account ✓. PASS.
  OverallVerdict: PASS_WITH_WARNINGS — empty fields submission not tested.

**Node 9 — Formatter**: Output: LoginPage.cs, LoginTests.cs. 3 tests. Gap annotation: // TODO: Add EmptyFields_ShowsRequiredError test. Status: ready_with_gaps.
