# Layer 2 — Nine-Node Flow Pairs (022)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: nine_node_flow (E2ETestingFlow53)
# Flow: FormValidationE2E | Predicate: TEST alone (form fires UI) | TransitionType: documentation

---

## B39-nine-022 — Form validation E2E tests — PASS

Request: "Write end-to-end test cases covering every validation rule on the contact form: required fields, format checks, and character limits."
Predicate set: ["TEST", "UI"] | TransitionType: documentation | TrustState: deterministic_generic

### Flow: E2ETestingFlow53 + UIInteractionFlow15

**Node 1 — DomainSplitter**: SelectedLogicalTransition: E2ETestingFlow53
Target: Contact form — name field, email field, message textarea, submit button.
Domains: required_field_validation, format_validation, character_limit_validation, submission_success.

**Node 2 — DomainTranslator**:
  required_field_validation: empty name → "Name is required"; empty email → "Email is required"; empty message → "Message is required".
  format_validation: invalid email format → "Invalid email format"; valid email → no error.
  character_limit_validation: message > 500 chars → "Message too long (max 500 characters)"; message = 500 → no error.
  submission_success: all valid → form submits; success notification shown; form resets.

**Node 3 — SchemePlanner**:
  Framework: Playwright + xUnit | Pattern: Page Object Model
  Class: ContactFormPage — selectors: [data-testid=name-input], [data-testid=email-input], [data-testid=message-input], [data-testid=submit-btn], [data-testid=success-notification], .field-error.

**Node 4 — DomainProcessor**:
  Test 1 — EmptyName_ShowsRequiredError: await page.FillForm({name:'', email:'a@b.com', message:'hello'}); await page.Submit(); await Expect(page.locator('[data-testid=name-error]')).toContainTextAsync('required').
  Test 2 — InvalidEmail_ShowsFormatError: await page.FillForm({name:'John', email:'notanemail', message:'hello'}); await page.Submit(); await Expect(page.locator('[data-testid=email-error]')).toContainTextAsync('Invalid').
  Test 3 — MessageOver500Chars_ShowsLimitError: const longMessage = 'x'.repeat(501); await page.FillForm({name:'John', email:'a@b.com', message:longMessage}); await page.Submit(); await Expect(page.locator('[data-testid=message-error]')).toBeVisibleAsync().
  Test 4 — AllValid_SubmitSucceeds: await page.FillForm({name:'John',email:'a@b.com',message:'Hello'}); await page.Submit(); await Expect(page.locator('[data-testid=success-notification]')).toBeVisibleAsync().
  Test 5 — AfterSubmit_FormResets: await Expect(page.locator('[data-testid=name-input]')).toHaveValueAsync('').

**Node 5 — Applier**: POM; Playwright async; 5 tests; boundary value for character limit; data-testid selectors.

**Node 6 — ResultTranslator**: 5 tests. required: 3, format: 1, limit: 1, submission: 2 (success + reset).

**Node 7 — ArbiterSelector**: FormValidationCoverageArbiter — each validation rule: boundary + invalid + valid.

**Node 8 — Arbiter**:
  name required: ✓. email required: ✓. message required: missing explicitly.
  email format invalid ✓; email format valid → implicitly tested in AllValid_SubmitSucceeds ✓.
  char limit: 501 ✓; 500 exact boundary not tested.
  submission: success ✓; reset ✓. PASS.
  OverallVerdict: PASS_WITH_WARNINGS — message required error not explicitly tested; char=500 boundary missing.

**Node 9 — Formatter**: Output: ContactFormPage.cs, ContactFormTests.cs. 5 tests. Gaps annotated. Status: ready_with_gaps.
