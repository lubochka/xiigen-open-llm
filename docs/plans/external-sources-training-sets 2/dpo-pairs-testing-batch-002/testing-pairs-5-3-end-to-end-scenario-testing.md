# Testing Pairs — Sub-phase 5.3 End-to-End Scenario Testing
# Batch: dpo-pairs-testing-batch-002
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: end-to-end scenario testing, user journey automation, acceptance test execution, full-stack flow verification
# Pair count: 5

---

## Pair AP2-5.3-001 — automated user-journey test confused with manual checklist

```
source_id: xiigen-testing-batch-002-AP2-5.3-001
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.3
rejected_sub_phase: 6.1
```

Prompt: `"automate the new-user signup journey as an end-to-end test — from the marketing landing page through completed registration"`

```
CHOSEN — 5.3 End-to-End Scenario Testing:
Playwright-driven journey running against a deployed staging environment.
The journey is automated end-to-end including the email verification step.

Test setup:
  Headless browser with a recorded baseline trace.
  A test-mailbox service (Mailosaur or MailHog API) so the test can fetch
  the verification email programmatically.
  A test-tenant identifier per run so concurrent runs do not collide.

Test catalogue:

Journey steps (one test per persona):

  T1 — New paid-tier signup:
    Step 1 — navigate to /pricing.
    Step 2 — click "Start free trial" on the Professional plan.
    Step 3 — fill the signup form with first name, last name, work email
              (test-mailbox-generated address), password.
    Step 4 — submit and assert redirect to /verify-email.
    Step 5 — poll the test mailbox for an email with subject containing
              "verify your email" (timeout 60 seconds).
    Step 6 — extract the verification link from the email body, navigate
              to it, assert redirect to /onboarding.
    Step 7 — fill the company-details form (company name, employee count
              range, primary use case).
    Step 8 — submit and assert redirect to /dashboard.
    Step 9 — assert dashboard shows the user's first name and the
              company name.

  T2 — New free-tier signup: same shape, "Start for free" button on the
        free plan, no payment step, faster path through onboarding.

  T3 — Signup with an already-registered email: form submission shows
        a clear "this email is already registered" message and offers a
        login link. Reaches /signup/exists, not /verify-email.

Assertions per step:
  Page URL matches expected.
  Visible elements match expected text (case-sensitive on copy that the
  marketing team reviews).
  Screenshot at end of journey is compared to a baseline; differences over
  a threshold fail the test.

Run cadence:
  In CI on every release-candidate build, before the human acceptance
  review begins.

REJECTED — 6.1 Manual QA and Acceptance Review:
The QA reviewer creates a manual checklist for the signup journey and
performs each step by hand: 1. Open the pricing page in Chrome and verify
the layout. 2. Click Start free trial. 3. Fill the form with test data.
4. Wait for the email and check it arrived. 5. Click the verify link.
6. Complete onboarding. 7. Confirm the dashboard shows correctly. The
reviewer documents findings and signs off. The reviewer repeats this for
free-tier and for the already-registered case. The reviewer estimates
this takes 30 minutes per release.
[WRONG: "automate the new-user signup journey" asks for an automated test
that runs in CI on every build. A manual checklist is the gate at release
time (6.1's territory) but it cannot run on every commit and cannot catch
regressions early. The chosen sub-phase is 5.3 — automation. Manual review
and end-to-end automation serve different purposes and live in different
sub-phases. The prompt explicitly says "automate"; treating it as a manual
checklist exercise misreads the verb.]
```

---

## Pair AP2-5.3-002 — full-stack flow test confused with manual reviewer checklist

```
source_id: xiigen-testing-batch-002-AP2-5.3-002
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.3
rejected_sub_phase: 6.1
```

Prompt: `"automate end-to-end tests for the place-order flow — cart, checkout, payment, and order confirmation page — that we can run in CI on every PR"`

```
CHOSEN — 5.3 End-to-End Scenario Testing:
Playwright suite against a deployed PR-preview environment with Stripe in
test mode. The suite runs in CI on every pull request and gates the merge.

Test setup:
  Each PR gets a preview deployment with a fresh database snapshot.
  Stripe test cards are used for all payment scenarios (no real money).
  A baseline cart fixture is built per test (no fixture pollution across
  tests).

Test catalogue:

  T1 — Happy path with one item:
    Add one item to the cart from the product page. Open the cart drawer
    and assert quantity = 1 and the line total. Click Checkout.
    Fill the shipping form. Select the saved test card. Click Place Order.
    Assert redirect to /orders/<orderId>. Assert the order page shows
    the item, the shipping address, and the order total.

  T2 — Happy path with multiple items + a coupon:
    Add three items. Apply coupon code TEST10 (test-only). Verify the
    discount is applied and the total reflects it. Complete payment.
    Order page shows the discount line item.

  T3 — Declined card:
    Use the Stripe test card 4000000000000002 (generic decline). On Place
    Order, assert the page does not redirect; instead, an inline error
    message appears with the decline reason. The cart is not cleared.
    The user can retry with a different card.

  T4 — Inventory exhausted between cart-add and checkout:
    Set up the test such that an item in the cart is reduced to zero
    inventory by a parallel admin action. On Place Order, assert the
    error message names the item and the user is offered to remove it
    from the cart.

  T5 — Browser back during checkout:
    Begin checkout, fill shipping, click Continue. On the payment page,
    click browser Back. Assert the shipping form is preserved with the
    earlier values, not blanked.

Cross-browser:
  T1 runs in Chromium, Firefox, and WebKit. T2-T5 run only in Chromium
  to keep CI time bounded; the cross-browser regression is caught by
  T1.

Assertions:
  Network requests recorded and asserted on the order-creation request
  (verifies the payload shape).
  Database state: after T1, the orders table has one row with the expected
  status; this is asserted via a test-only API endpoint that the suite calls.

REJECTED — 6.1 Manual QA and Acceptance Review:
QA reviewer runs through the place-order flow manually before each release.
Reviewer checks: 1. Add an item, verify the cart updates. 2. Open checkout,
fill the form. 3. Use test card details. 4. Verify the order confirmation
page. 5. Verify the order appears in the admin orders list. Reviewer
repeats with declined cards, multiple items, and a coupon. Reviewer
estimates this takes 90 minutes per release. Reviewer documents findings
and signs off.
[WRONG: the prompt says "automate" and "run in CI on every PR" — that is
a 5.3 deliverable. Manual review on each release does not run on every PR,
cannot catch regressions before merge, and consumes 90 minutes per cycle.
Manual review remains valuable for release-time judgment (look-and-feel,
real-email rendering, judgment calls about edge cases) — but it is a
different sub-phase. Confusing the two produces neither the automated
regression coverage nor the human judgment.]
```

---

## Pair AP2-5.3-003 — end-to-end flow test confused with manual exploratory review

```
source_id: xiigen-testing-batch-002-AP2-5.3-003
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.3
rejected_sub_phase: 6.1
```

Prompt: `"automate the password-reset journey end-to-end — request, email link, reset, and login with the new password"`

```
CHOSEN — 5.3 End-to-End Scenario Testing:
Playwright suite using a real test mailbox.

Test setup:
  Test mailbox API for fetching emails programmatically.
  Test user accounts pre-seeded per test class.

Test catalogue:

  T1 — Happy path:
    Step 1 — navigate to /login, click "Forgot password".
    Step 2 — enter the test user's email, submit. Assert the page shows
              "If an account exists, a reset link has been sent" — the
              non-enumerable response.
    Step 3 — poll the test mailbox for an email matching the user's
              address with subject "Reset your password" (timeout 60s).
    Step 4 — extract the reset link from the email body. Assert the
              link domain is the application's domain (not a phishable
              external redirect).
    Step 5 — navigate to the reset link, fill the new-password form
              twice (entry and confirmation).
    Step 6 — submit; assert redirect to /login with a success banner.
    Step 7 — navigate to /login, fill the user's email and the new
              password, submit; assert redirect to /dashboard.

  T2 — Reset link expired:
    Trigger a password reset for the test user, but wait until the
    reset token has expired (the test environment uses a 1-minute
    expiry to keep this fast). Click the link; assert the page shows
    "this link has expired" with a Request a new link button.

  T3 — Reset link reused:
    Complete a successful reset using the link from T1. Then re-open the
    same link in a new browser tab; assert the page shows "this link is
    no longer valid" — single-use token enforcement.

  T4 — Old password no longer works after reset:
    After completing T1, attempt to log in with the user's previous
    password; assert the login fails with "incorrect password".

  T5 — Email enumeration prevention:
    Submit the forgot-password form with an email that does not exist
    in the system. Assert the response page is identical to the
    real-email case (same HTTP status, same response body, same render
    time within 200 ms).

REJECTED — 6.1 Manual QA and Acceptance Review:
The QA reviewer manually tests the password reset flow before each
release. Reviewer: 1. Goes to login, clicks Forgot password. 2. Enters
their test email. 3. Waits for the email. 4. Clicks the link. 5. Sets a
new password. 6. Logs in with the new password. 7. Confirms the old
password does not work. Reviewer repeats with an expired link by waiting
1 minute. Reviewer documents findings.
[WRONG: "automate end-to-end" is a 5.3 deliverable that runs in CI without
a human in the loop. A reviewer waiting 1 minute for a token to expire
cannot run on every commit and cannot test the email-enumeration response
parity (which depends on assertion-level timing comparison). The end-to-end
test catches all of these properties continuously; the manual review remains
the gate at release time for human-judgment concerns. The prompt asks for
the automation, not the manual checklist.]
```

---

## Pair AP2-5.3-004 — end-to-end test confused with integration-only test

```
source_id: xiigen-testing-batch-002-AP2-5.3-004
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.3
rejected_sub_phase: 5.2
```

Prompt: `"automate end-to-end testing for the multi-step onboarding wizard — must traverse the UI, persist progress, and complete to dashboard"`

```
CHOSEN — 5.3 End-to-End Scenario Testing:
Playwright suite that drives the actual wizard UI. The property under test
is the full user journey including the UI's progress-persistence behaviour
across browser refreshes.

Test setup:
  Playwright with persistent-context (same browser session across page
  loads within a test) and clean-context isolation (different sessions
  across tests).
  A clean test-tenant per test run.

Test catalogue:

  T1 — Complete the wizard end-to-end:
    Step 1 — sign in as a freshly-created tenant admin.
    Step 2 — assert redirect to /onboarding/step-1.
    Step 3 — fill the company-profile form (4 fields), click Next; assert
              redirect to step-2.
    Step 4 — fill the team-invites form (add 3 users), click Next; assert
              redirect to step-3.
    Step 5 — fill the integration-setup form (skip integrations), click
              Next; assert redirect to step-4.
    Step 6 — fill the preferences form, click Finish; assert redirect to
              /dashboard.
    Step 7 — assert dashboard shows the company name and the 3 invited
              users in the team list.

  T2 — Resume after browser close:
    Begin onboarding. Complete steps 1 and 2. Close the browser.
    Re-open the application as the same user. Assert redirect to
    /onboarding/step-3 (resume point), not back to step-1. Assert that
    the data entered in steps 1 and 2 is still present (verifiable by
    navigating Back to step-1 and checking the form values).

  T3 — Skip optional integrations:
    Begin onboarding. At step 3, click "Skip for now". Assert redirect
    to step-4 with no integration warnings on the dashboard.

  T4 — Validation on step transitions:
    Begin onboarding. On step 1, leave the required field blank, click
    Next. Assert the form does not submit and an error appears on the
    blank field.

  T5 — Wizard exit without completion:
    Begin onboarding. Navigate to /dashboard manually. Assert redirect
    back to onboarding (the user cannot reach the dashboard without
    completing onboarding).

REJECTED — 5.2 Integration Tests:
Test the onboarding API endpoints in isolation. POST /onboarding/step-1
with the company-profile body; assert 200 and that the persisted state has
moved to step-2. POST /onboarding/step-2 with the team-invites body; assert
the invites are queued in the database. POST /onboarding/step-3 with skip
flags; assert the integration state is empty. POST /onboarding/step-4 with
preferences; assert the user moves to active state. Use Testcontainers for
the database. Run as integration tests.
[WRONG: the prompt says "traverse the UI" and "persist progress" — these
are properties of the wizard's UI behaviour. The integration-test approach
verifies the API endpoints in isolation; it does not exercise the UI's
state-management, the page-redirect logic, or the resume-after-refresh
behaviour. End-to-end tests through the UI are the correct sub-phase;
integration tests on the endpoints alone may pass while the UI is broken.
Both kinds of tests have value, but they are different sub-phases with
different scopes.]
```

---

## Pair AP2-5.3-005 — end-to-end suite confused with integration-only suite

```
source_id: xiigen-testing-batch-002-AP2-5.3-005
source_license_state: first_party
pair_kind: dpo_test
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 5.3
rejected_sub_phase: 5.2
```

Prompt: `"automate end-to-end coverage for the search-and-purchase flow including search relevance, filter behavior, add-to-cart, and checkout"`

```
CHOSEN — 5.3 End-to-End Scenario Testing:
Playwright suite covering the full user journey from search to confirmed
purchase. The journey crosses the search service, the catalog service, the
cart service, the payment service, and the order service — but the test
exercises them through the actual UI.

Test setup:
  PR-preview environment with seeded catalog (1000 products).
  Stripe test mode for payment.
  A test user with a known account.

Test catalogue:

  T1 — Search → filter → add → buy:
    Step 1 — navigate to /, assert the search bar is focused on first
              keystroke.
    Step 2 — type "leather wallet" in the search bar; assert the result
              page loads with leather-wallet products at the top.
    Step 3 — apply the filter "color: brown" from the filter sidebar;
              assert the results refresh and only brown wallets remain.
    Step 4 — click the first result; assert the product detail page
              loads with the product images, price, and Add to Cart.
    Step 5 — click Add to Cart; assert the cart drawer opens with the
              item.
    Step 6 — click Checkout; assert redirect to /checkout.
    Step 7 — fill shipping (or use saved shipping); use the saved
              Stripe test card; click Place Order.
    Step 8 — assert redirect to /orders/<orderId>; assert the order
              shows the leather wallet, the brown variant, and the
              expected price.

  T2 — Search empty state:
    Search for "asdfqwerty" (a query with zero results). Assert the page
    shows the empty-results message with suggested searches; the cart
    icon is unchanged.

  T3 — Filter clear and reapply:
    Search and apply two filters; click Clear All; assert all results
    return; reapply one filter; assert the results match.

  T4 — Add multiple items from search:
    From the search results page, add three items to the cart by clicking
    the Add buttons on the result cards. Open the cart and assert all
    three are present with the correct quantities.

  T5 — Search relevance regression:
    The suite includes a relevance baseline: 10 known queries with
    expected top-3 results. The test asserts that the top-3 results
    (by displayed order) match the expected set. The relevance baseline
    is updated explicitly when the relevance team approves a change;
    silent drift fails the test.

Run cadence:
  Every PR; the relevance regression test is gated on the search-team
  approval check (a label on the PR).

REJECTED — 5.2 Integration Tests:
Write integration tests against the search API and the cart API. POST
/search?q=leather+wallet&color=brown; assert the response contains
leather-wallet products. POST /cart/add with the productId; assert the
cart endpoint returns 200. POST /checkout/place-order with the cart and
test-card details; assert the order is created. Use Testcontainers for the
database and real Stripe test mode for payment. Run as part of the
integration test suite.
[WRONG: the prompt asks for end-to-end coverage of the full flow — search,
filter behaviour in the UI, add-to-cart through the UI, checkout through
the UI. Integration tests against the APIs cover the back-end of each step
but do not cover the UI behaviour: filter sidebar interaction, cart drawer
open behaviour, checkout form rendering, network errors visible to the user.
A back-end integration test passes while a UI bug breaks the user journey.
End-to-end tests through Playwright catch the UI bugs that integration
tests miss; both are valuable but they live in different sub-phases.]
```
