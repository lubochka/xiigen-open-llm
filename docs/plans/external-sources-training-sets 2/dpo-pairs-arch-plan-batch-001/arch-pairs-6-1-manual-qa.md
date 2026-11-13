# ARCH Pairs — Sub-phase 6.1 Manual QA and Acceptance Review
# Batch: dpo-pairs-arch-plan-batch-001
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: human acceptance criteria, exploratory validation, release-facing quality judgment
# Pair count: 5

---

## Pair AP1-6.1-001 — acceptance criteria confused with end-to-end test authoring

```
source_id: xiigen-arch-plan-batch-001-AP1-6.1-001
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.1
rejected_sub_phase: 5.3
```

Prompt: `"define what a human reviewer must verify before we ship the new returns flow to production"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
Six items the reviewer must verify by hand. Each names what to do, what to
expect, and what counts as a fail.

1. Initiate a return for an order over 30 days old.
   Expected: the UI clearly states "this order is past the return window"
   with a way to contact support.
   Fails if: the system silently accepts the return and produces a refund.
   Fails if: the message is generic ("an error occurred") instead of stating
   the return-window cause.

2. Initiate a return for a partial order — three of five items.
   Expected: the refund amount displayed matches the sum of the three item
   prices plus the proportional shipping cost.
   Fails if: the refund amount silently omits shipping or includes the full
   shipping cost. Reviewer must compute the expected amount independently
   and compare.

3. Receive the return-confirmation email in a real inbox (not a dev mailbox).
   Expected: the email arrives within 5 minutes, contains the return tracking
   number, and renders correctly on mobile (Gmail iOS app and Outlook
   Android).
   Fails if: any field is empty, any link is broken, or the email is in
   the spam folder.

4. Cancel a return in progress before the package is shipped back.
   Expected: the order returns to the "delivered" state and the refund is
   not initiated.
   Fails if: the order moves to an invalid state, or the refund is silently
   processed.

5. Attempt to initiate a return for another tenant's order.
   Expected: the response is a clear "order not found" — not a 500 error
   and not a "this order belongs to a different tenant" message that leaks
   tenant existence.
   Fails if: the response leaks information that cross-tenant access was
   attempted.

6. Submit a return reason containing emoji and Hebrew text.
   Expected: the support agent's view of the reason renders the emoji and
   right-to-left text correctly.
   Fails if: the reason appears as question marks, mojibake, or with
   broken text direction.

Each fail blocks the release. The reviewer signs the release artifact only
after all six are verified.

REJECTED — 5.3 End-to-End Scenario Testing:
Write Playwright tests for the returns flow. Test 1: initiate a return for
an old order and assert the error message. Test 2: initiate a partial return
and assert the refund amount calculation. Test 3: stub the email service
and assert it was called with the correct tracking number. Test 4: cancel a
return and assert the order state. Test 5: attempt cross-tenant access and
assert a 404 response. Add the suite to the CI pipeline and gate releases
on the suite passing.
[WRONG: "what a human reviewer must verify" asks for the acceptance checklist
that requires human judgment — real email delivery to a real inbox, mobile
rendering, emoji and Hebrew text rendering, the language of error messages.
Automation cannot verify that an email arrives in a real inbox or renders
correctly across mobile email clients. The Playwright suite is supporting
evidence for some checks; it is not the gate. Acceptance review and end-to-end
automation are different sub-phases with different responsibilities.]
```

---

## Pair AP1-6.1-002 — security sign-off confused with unit test authoring

```
source_id: xiigen-arch-plan-batch-001-AP1-6.1-002
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.1
rejected_sub_phase: 5.1
```

Prompt: `"the security team needs to sign off that the new admin endpoints are properly authorized before release"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
Security sign-off checklist. Reviewer reads code; reviewer does not read tests.

1. Open the controller files under /controllers/admin/. For each public action
   method, verify in source that an [Authorize(Roles="Admin")] attribute is
   present. Do not rely on a checklist generated from reflection — that
   checklist would miss any controller registered through a custom convention.

2. Open the route registration code (Startup.cs or Program.cs). Verify that
   no admin route is registered without the AuthorizationPolicy("Admin")
   filter. Read this file in full. Any admin route that bypasses the policy
   is a release blocker.

3. For each admin action, attempt the action with a non-admin token in a
   running test environment. Expected: 403 response, no leak of the resource
   shape in the response body.

4. Read the file admin-bypass-paths.json (the explicit allow-list of admin
   actions that do not require admin role). Confirm every entry has a
   documented justification (in the file's comments) and was reviewed in a
   prior security sign-off. New entries since the last sign-off block this
   release until reviewed.

5. Read the audit logging code. Confirm that every admin action emits an
   audit log entry with the actor's user id, timestamp, and action name.
   Verify by reading the actual log file from the running test environment
   after performing each admin action — not by reading test assertions.

6. Reviewer signs the release artifact with: the date, the reviewer name,
   and the commit hash that was reviewed. A new commit after sign-off
   invalidates the sign-off.

Sign-off cannot be delegated to automation. Tests can pass while a controller
is unreviewed, an entry was added to the bypass list, or audit logging was
silently disabled by configuration.

REJECTED — 5.1 Unit Test Design:
Write unit tests for the AdminController. For each action method, write a
test that mocks the IAuthorizationService and asserts that the
authorization check is invoked. Add a test for the role-based authorization
policy that asserts a non-admin role is rejected. Add a parameterized test
that runs every controller method with three role permutations (admin,
non-admin, anonymous) and asserts the expected outcome. Include the tests
in the standard CI gate.
[WRONG: security team sign-off is a human review of the actual implementation —
reading the controller files, the route registration, the bypass list, and
the audit logs in the running environment. Unit tests with mocked
authorization services prove the test doubles behave correctly; they do not
prove that the real code in production does. A test passing while the route
is registered without the policy filter, or while audit logging is disabled
by configuration, is the failure mode the human review catches. Tests are
supporting evidence; they are not the gate.]
```

---

## Pair AP1-6.1-003 — release-readiness review confused with end-to-end test authoring

```
source_id: xiigen-arch-plan-batch-001-AP1-6.1-003
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.1
rejected_sub_phase: 5.3
```

Prompt: `"what does the QA reviewer check before we promote the release candidate from staging to production?"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
Pre-promotion checklist. Reviewer performs each check on the staging
environment that holds the candidate build.

Functional smoke (1 hour budget):
  Sign in as a real test account. Complete the three highest-revenue user
  journeys end-to-end: place a paid order, request a refund, change account
  settings. Each must succeed without any visible error and within the
  performance budget (each step < 2 seconds).

Look-and-feel pass (30 min budget):
  Walk through the top 10 screens in three browsers (Chrome, Safari, Firefox)
  and on a real iPhone and Android. Note any visual regression: misaligned
  elements, broken images, missing icons, hover states that look wrong.
  Visual regressions block promotion only if they affect a top-10 screen.

Email rendering:
  Trigger one email of each type (welcome, order-confirmation, password-reset,
  receipt). Read each in Gmail web, Gmail mobile, and Outlook. Forbidden:
  raw template variables ({{firstName}} appearing instead of the name),
  broken layout in mobile clients.

Cross-tenant safety:
  Sign in as Tenant A. Attempt to access a Tenant B URL by editing the
  URL bar. Expected: 404 with no information about Tenant B's existence.

Rollback rehearsal:
  Confirm the rollback script is executable and the previous release artifact
  is still in the staging registry. Reviewer states what they would do if
  promotion fails — without executing the rollback.

Sign-off:
  Reviewer documents findings (which checks passed, which deferred, which
  failed). A failed functional smoke or cross-tenant safety check blocks
  promotion. A failed look-and-feel check is a follow-up ticket, not a block.

REJECTED — 5.3 End-to-End Scenario Testing:
Write a Playwright test suite that runs through the highest-revenue user
journeys: place a paid order, request a refund, change account settings.
Add a visual regression test using Playwright's screenshot comparison
against a baseline. Add an email content assertion using the Mailtrap API
to fetch and parse triggered emails. Add a cross-tenant access test that
asserts a 404 response when Tenant A tries to access a Tenant B URL.
Run the suite as the staging-to-production promotion gate.
[WRONG: pre-promotion review is the human judgment that the candidate
build is ready to ship. The reviewer checks rendering across real browsers
and real mobile devices, reads emails in real inboxes, walks through
unscripted exploratory paths, and judges look-and-feel against intent —
none of which automation captures. The Playwright suite is part of the
candidate build's continuous integration, not the human acceptance gate.]
```

---

## Pair AP1-6.1-004 — release decision confused with end-to-end test authoring

```
source_id: xiigen-arch-plan-batch-001-AP1-6.1-004
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.1
rejected_sub_phase: 5.3
```

Prompt: `"how do we decide whether the new search experience is good enough to roll out to all users?"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
The decision is a human judgment after a staged rollout. Three measurements
plus one qualitative review.

Stage 1 — internal dogfood (one week).
  Reviewer recruits five internal users with varied search needs. Each user
  performs one week of normal work using the new search exclusively.
  Reviewer collects: per-user failure stories (what they searched for that
  did not work), per-user satisfaction score (1-5), and per-user willingness
  to keep the new experience over the old.
  Acceptance threshold: average satisfaction >= 4.0 AND zero users rate
  willingness to switch as 1.

Stage 2 — 5% rollout (two weeks).
  Reviewer monitors three quantitative signals daily: search abandonment
  rate (user types a query, does not click any result), zero-result rate,
  and downstream conversion rate from search-driven traffic.
  Acceptance thresholds: abandonment rate must not increase by more than
  10% relative to baseline; zero-result rate must decrease (the new search
  is supposed to handle long-tail queries better); conversion rate must
  not decrease.

Stage 3 — 50% rollout (one week).
  Reviewer holds a 30-minute session with the customer success team to
  collect support-ticket signal: are users complaining about search?
  About what specifically?
  Acceptance: support tickets mentioning search must not exceed 1.5x the
  pre-rollout rate.

Stage 4 — full rollout decision.
  Reviewer assembles the data from Stages 1-3 plus the unstructured
  feedback. Reviewer decides: roll out fully, hold at 50%, or roll back.
  The decision is documented with the data and the reasoning. Roll-back
  decisions document what specifically must change before the next try.

This is judgment, not a passing-test count. A search that hits all
quantitative thresholds but generates a wave of support tickets about
"results feel less relevant" is not ready, even if every quantitative
metric is green.

REJECTED — 5.3 End-to-End Scenario Testing:
Write a comprehensive Playwright test suite for the new search. Cover the
top 100 search queries collected from production logs and assert that the
new search returns relevant results. Add visual regression tests for the
search UI. Add performance tests asserting search latency < 200ms p95.
Run the suite in CI; gate the rollout on the suite passing. Add a separate
test for the search filtering and pagination edge cases.
[WRONG: "good enough to roll out" is a release-judgment question. The
decision depends on user satisfaction, support-ticket signal, and downstream
conversion behavior — none of which are visible to a Playwright suite. A
search that passes 100 hand-curated query tests can still feel worse to
real users on long-tail queries the curator did not think of. The reviewer's
judgment over staged rollout data is the gate; the Playwright suite is part
of the candidate build's continuous integration, not the rollout decision.]
```

---

## Pair AP1-6.1-005 — accessibility acceptance confused with unit test authoring

```
source_id: xiigen-arch-plan-batch-001-AP1-6.1-005
source_license_state: first_party
pair_kind: dpo_arch
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 6.1
rejected_sub_phase: 5.1
```

Prompt: `"how does the QA reviewer validate that the new checkout flow is accessible before release?"`

```
CHOSEN — 6.1 Manual QA and Acceptance Review:
Three exercises performed by hand. Automation reports are not sufficient.

Screen-reader pass (45 minutes):
  Reviewer launches NVDA on Windows or VoiceOver on macOS and completes the
  full checkout flow with the screen turned off (or with eyes closed).
  Required: the screen reader announces every actionable element with a
  meaningful name; the flow can be completed without sighted assistance;
  errors are announced when they occur (not silently displayed in the visual
  layer only).
  Failures the reviewer must catch: a button announced only as "button"; a
  required field whose error message appears visually but is not announced;
  a focus trap inside a modal that the reviewer cannot exit by keyboard.

Keyboard-only pass (30 minutes):
  Reviewer disconnects the mouse and completes the full flow using only the
  keyboard. The Tab order must follow visual order. Every interactive element
  must be reachable. Focus must be visibly indicated at all times.
  Failures: a control that requires a click to activate, a focus indicator
  that disappears or is invisible against the page background, a Tab order
  that jumps between sections in a way that loses context.

Real-user reduced-vision check (20 minutes):
  Reviewer enables OS-level high-contrast mode and OS-level zoom to 200%.
  The flow must remain usable. Every error must remain visible. No critical
  text must be cut off by the zoomed viewport.
  Failures: a CTA button whose contrast ratio falls below 4.5:1 in
  high-contrast mode; an error toast that is partially clipped at 200% zoom.

Reviewer documents findings on a release artifact. A failure on any of these
three exercises blocks the release. The automated axe-core report is
attached as supporting evidence; the human exercises are the gate.

REJECTED — 5.1 Unit Test Design:
Write unit tests for each form component using @testing-library/react and
its accessibility queries. Use queryByRole and queryByLabelText to assert
that every interactive element has an accessible role and an accessible name.
Add a test that snapshots the ARIA-attribute structure of each form. Add a
test using axe-core that asserts zero violations on the rendered components.
Run the suite in CI as the accessibility gate.
[WRONG: accessibility acceptance is a human-experience review. Automated
axe-core checks catch missing aria-label and missing-alt-text and a few
contrast issues, but they cannot detect a focus trap, a misleading screen-reader
announcement, a broken Tab order, or zoom-clipped text. The review by an
actual screen-reader user, an actual keyboard user, and an actual high-zoom
user is the gate; the automated test suite is supporting evidence. Tests
passing while the keyboard user cannot escape the modal is the failure mode
the human exercise catches.]
```
