# Spec Pairs — Sub-phase 2.2 User Stories and Acceptance Criteria
# Batch: dpo-pairs-spec-batch-005
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: story slices, acceptance gates, observable behavior requirements
# Pair count: 4

---

## Pair AP5-2.2-001 — user stories confused with capability definition

```
source_id: xiigen-spec-batch-005-AP5-2.2-001
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.2
rejected_sub_phase: 2.1
```

Prompt: `"write the user stories with acceptance criteria for the new self-service password reset feature"`

```
CHOSEN — 2.2 User Stories and Acceptance Criteria:
Story slices framed as user-observable behaviours, with acceptance criteria
that name the observable signal of completion.

Story 2.2-PR-1: Request a password reset
  As a user who forgot my password
  I want to request a reset link by entering my email
  So that I can regain access without contacting support.

  Acceptance criteria:
  AC-1.1 — On the login page, a "Forgot password" link is visible and
   clearly labeled, reachable in one click from the email/password form.
  AC-1.2 — Clicking the link opens a form with a single email input and
   a "Send reset link" button.
  AC-1.3 — Submitting the form returns to a confirmation page that says
   "If an account exists for this email, a reset link has been sent."
   (this exact message — non-enumerable response).
  AC-1.4 — The confirmation page is shown for any submitted email,
   regardless of whether the email exists in the system. Response time
   does not differ between existing and non-existing emails by more
   than 200 ms.
  AC-1.5 — A reset link arrives in the user's inbox within 60 seconds
   when the email exists. Real inbox, not a mocked test mailbox.

Story 2.2-PR-2: Use the reset link
  As a user who received a reset link
  I want to set a new password via the link
  So that I can sign back in.

  Acceptance criteria:
  AC-2.1 — The link is valid for exactly 60 minutes from issue.
  AC-2.2 — Clicking the link opens a "Set new password" page with a
   password input and a "Confirm new password" input.
  AC-2.3 — The new password must satisfy the password rules; rule
   compliance is shown in real time as the user types.
  AC-2.4 — On submit, the user is taken to the login page with a
   success message and asked to sign in with the new password.
  AC-2.5 — A used reset link cannot be used a second time. The second
   click shows "This link is no longer valid".

Story 2.2-PR-3: Recover from an expired link
  As a user whose reset link has expired
  I want to request a new one without contacting support
  So that I am not blocked.

  Acceptance criteria:
  AC-3.1 — Clicking an expired link shows "This link has expired" with
   a "Send a new link" button.
  AC-3.2 — Clicking "Send a new link" returns to story 2.2-PR-1.

Story 2.2-PR-4: Confirm the old password no longer works
  As a user who completed a password reset
  I want my old password to be invalidated
  So that anyone with the old password cannot sign in.

  Acceptance criteria:
  AC-4.1 — After a successful reset, attempting to log in with the
   previous password returns the standard incorrect-password error.

Out-of-scope:
  Account recovery via security questions, MFA backup codes, or
  customer-support intervention — those are separate stories.

REJECTED — 2.1 Business Capability Definition:
The self-service password reset capability lets users regain access to
their account without contacting support. In scope: email-based reset
for users with a verified email. Out of scope: phone-based recovery,
identity verification through a customer support agent. Actors:
end-user. Business value: reduces support load by an estimated 35% on
password-related tickets and improves perceived service quality. Expected
outcome: 90% of password-recovery cases are resolved via self-service
within 90 days post-launch; mean time to recovery drops from 4 hours
(support-handled) to 5 minutes (self-service).
[WRONG: "write the user stories with acceptance criteria" asks for the
story slices and the observable acceptance signals. The capability
definition is 2.1's responsibility and was the input to writing these
stories. Returning the capability definition when stories were asked for
skips the slicing step that turns the capability into actionable
specifications.]
```

---

## Pair AP5-2.2-002 — acceptance criteria confused with end-to-end test design

```
source_id: xiigen-spec-batch-005-AP5-2.2-002
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.2
rejected_sub_phase: 5.3
```

Prompt: `"write the acceptance criteria for the order-cancellation feature so the team knows when each story is done"`

```
CHOSEN — 2.2 User Stories and Acceptance Criteria:
The acceptance criteria for each story slice. Each criterion names an
observable behaviour and the condition under which it passes.

Story 2.2-OC-1: Cancel an unshipped order

  AC-1.1 — On the order detail page for an unshipped order, a "Cancel
   order" button is visible.
  AC-1.2 — On the order detail page for a shipped order, the "Cancel
   order" button is NOT visible. (Cancellation after shipping is a
   different flow — a return — handled by separate stories.)
  AC-1.3 — Clicking "Cancel order" opens a confirmation dialog with
   the order summary and the refund amount.
  AC-1.4 — The displayed refund amount equals the sum of items + the
   shipping cost (when the order has not been picked from inventory)
   or item-only (when partial inventory was reserved and is being
   restocked).
  AC-1.5 — On confirming, the order status changes to "Cancelled"
   immediately in the user's view.
  AC-1.6 — A cancellation email is sent to the user's email address
   within 5 minutes containing the cancellation summary and the
   refund timeline.
  AC-1.7 — The refund is initiated against the original payment method.
   Refund timing depends on the payment method: 1-3 business days for
   credit cards (per provider's standard), within 1 hour for wallet
   payments.
  AC-1.8 — The cancelled order remains visible in the user's order
   history with the cancelled status. The order is never silently
   removed.

Story 2.2-OC-2: Attempt to cancel a shipped order

  AC-2.1 — On a shipped order's detail page, in place of "Cancel
   order", a "Request return" button is visible.
  AC-2.2 — A direct API call to /orders/<shippedId>/cancel returns
   400 with a clear "Cannot cancel a shipped order" message;
   the URL is not exposed in the UI but the API is hardened.

Story 2.2-OC-3: Inventory restock after cancellation

  AC-3.1 — When an order is cancelled, the items return to available
   inventory within 1 minute.
  AC-3.2 — A second customer attempting to buy the same item right
   after cancellation can complete the purchase.

Story 2.2-OC-4: Cancellation visible to the support team

  AC-4.1 — A cancelled order appears in the support agent's order
   search with a clear cancelled marker.
  AC-4.2 — The cancellation reason (if the user provided one) is
   visible to the support agent.

Acceptance criteria are observable: each one names what someone (user
or agent or API client) will see. The criteria do not specify how the
behaviour is implemented or how it is tested.

REJECTED — 5.3 End-to-End Scenario Testing:
Write Playwright tests that drive the order-cancellation flow:
- Test 1: log in as a user with an unshipped order; navigate to the
  order detail page; click Cancel; assert the dialog appears with
  the correct refund amount; click Confirm; assert the order status
  is "Cancelled" and the email arrives in Mailtrap within 30s.
- Test 2: log in as a user with a shipped order; navigate to the
  order detail page; assert "Cancel order" is not visible and
  "Request return" is.
- Test 3: cancel an order from a freshly-seeded test database;
  query the inventory API after 1 minute; assert the inventory
  count increased.
- Test 4: as a support agent, search for the cancelled order;
  assert it appears with the cancelled marker.
[WRONG: "write the acceptance criteria so the team knows when each story
is done" asks for the observable behaviours that define done. End-to-end
tests are an implementation of those criteria — they verify some of them
automatically — but the criteria are the specification, not the tests.
Several criteria (AC-1.7 about refund timing depending on payment method,
AC-1.8 about the order remaining visible forever) are easier stated as
observable behaviour than as automated test scenarios. The criteria come
first; tests follow.]
```

---

## Pair AP5-2.2-003 — story acceptance confused with manual QA review

```
source_id: xiigen-spec-batch-005-AP5-2.2-003
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.2
rejected_sub_phase: 6.1
```

Prompt: `"specify the acceptance criteria for the new feature so we can hand the spec to engineering"`

```
CHOSEN — 2.2 User Stories and Acceptance Criteria:
A story-by-story breakdown of the feature with explicit acceptance criteria
that engineering can build against. The criteria are the contract between
product and engineering for "this story is done".

Feature: Saved searches.

Story 2.2-SS-1: Save a search

  As a user who frequently runs the same search
  I want to save it for quick access later
  So that I do not retype the same query.

  AC-1.1 — On the search results page, a "Save this search" button
   is visible when at least one filter or query is set.
  AC-1.2 — Clicking the button opens a name-the-search prompt with a
   suggested name based on the query and filters.
  AC-1.3 — The name input has a 60-character limit. Names longer than
   that are blocked with an inline error.
  AC-1.4 — Saving creates an entry in the user's saved-searches list.
  AC-1.5 — Each user has a maximum of 50 saved searches; attempting
   to save a 51st prompts the user to delete an existing one first.

Story 2.2-SS-2: Run a saved search

  AC-2.1 — A "Saved searches" entry in the sidebar lists all saved
   searches by name.
  AC-2.2 — Clicking a saved search runs it and displays results.
  AC-2.3 — The displayed results reflect the current data (the saved
   search is a query, not a snapshot).
  AC-2.4 — Saved-search names are visible in the URL when running
   them: /search?saved=<name>.

Story 2.2-SS-3: Edit or delete a saved search

  AC-3.1 — A saved search can be renamed in place from the saved-
   searches list.
  AC-3.2 — A saved search can be deleted; deletion is confirmed via
   a single-click confirmation, not a typed-name confirmation.
  AC-3.3 — Saved searches the user owns are visible only to that user;
   no shared saved searches in this version.

Story 2.2-SS-4: Saved search updates with new filters

  AC-4.1 — On the search results page, when running a saved search,
   adding or changing filters does NOT modify the saved version.
  AC-4.2 — A "Save changes" button appears when filters differ from
   the saved version. Clicking it updates the saved search.
  AC-4.3 — A "Save as new" button is also available, creating a new
   saved search with the current filters.

Acceptance criteria are observable behaviours of the system. They define
"done"; they do not define how to verify done.

REJECTED — 6.1 Manual QA and Acceptance Review:
QA reviewer creates a manual checklist and runs through it on the
staging build before release:
- 1. Save a search; verify the name field, the suggested name, the
  60-character limit. Document any issue.
- 2. Run the saved search; verify the URL parameter, the results
  freshness, and the sidebar listing.
- 3. Edit the saved search by renaming and by changing filters;
  verify both code paths.
- 4. Delete a saved search; verify the confirmation flow.
- 5. Try to save the 51st search; verify the user is prompted to
  delete an existing one.
The reviewer documents pass/fail and signs off on the release if all
checks pass.
[WRONG: "specify the acceptance criteria so we can hand the spec to
engineering" asks for the engineering-facing specification — the
observable behaviours that engineering will implement. The QA review
checklist comes later and verifies those behaviours after engineering
has built them. Returning a QA checklist when the spec was asked for
hands engineering a verification artifact instead of a build artifact.]
```

---

## Pair AP5-2.2-004 — acceptance criteria confused with unit-test specification

```
source_id: xiigen-spec-batch-005-AP5-2.2-004
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.2
rejected_sub_phase: 5.1
```

Prompt: `"write the user stories and acceptance criteria for the new approval-workflow feature"`

```
CHOSEN — 2.2 User Stories and Acceptance Criteria:
Stories framed as observable user behaviour.

Story 2.2-AW-1: Submit a request for approval

  As a user who needs approval for an action
  I want to submit a request to my manager
  So that I can proceed once approved.

  AC-1.1 — The request form requires a title, a description, an action
   type (from a fixed list), and an attachment (optional).
  AC-1.2 — On submit, the request enters "Pending" state and is visible
   in the user's request list.
  AC-1.3 — The user's manager receives a notification within 1 minute
   (email + in-app).
  AC-1.4 — A request can be edited by the submitter only while it is
   in "Pending" state and only when no approver action has been
   recorded.

Story 2.2-AW-2: Approve or reject a request

  As a manager
  I want to review pending requests assigned to me
  So that I can approve or reject them with a reason.

  AC-2.1 — The manager's "Pending requests" view lists all requests
   assigned to them, sorted by submission time (oldest first by default).
  AC-2.2 — Clicking a request shows its full content and an Approve /
   Reject panel with a required "Reason" field for Reject (optional
   for Approve).
  AC-2.3 — On Approve, the request enters "Approved" state and the
   submitter receives a notification with the approval reason
   (when provided).
  AC-2.4 — On Reject, the request enters "Rejected" state and the
   submitter receives a notification with the required rejection reason.
  AC-2.5 — The approve/reject decision is immutable — a reviewer cannot
   "un-approve" a request. A new request must be submitted instead.

Story 2.2-AW-3: Track request status

  As a submitter
  I want to see the status of my requests
  So that I know what to do next.

  AC-3.1 — The submitter's "My requests" view shows each request with
   its current state, the submission date, the reviewer name, and the
   decision date (when applicable).
  AC-3.2 — Filter and search are supported on the list (by state, by
   action type, by date range).

Story 2.2-AW-4: Re-assign a pending request

  As an admin
  I want to re-assign a pending request to a different approver
  So that workflows are not blocked when an approver is unavailable.

  AC-4.1 — The admin role can re-assign any pending request.
  AC-4.2 — Re-assignment is logged in the request's history.
  AC-4.3 — The new approver receives a notification within 1 minute.

REJECTED — 5.1 Unit Test Design:
Write unit tests for the ApprovalService:
- Test: SubmitRequest with valid fields creates a request in Pending state;
  assert the state, the submitter, and the timestamp.
- Test: SubmitRequest with missing title throws ValidationException
  with "title required".
- Test: ApproveRequest by the assigned manager moves the state to Approved.
- Test: ApproveRequest by a non-manager throws UnauthorizedException.
- Test: RejectRequest without a reason throws ValidationException.
- Test: ReassignRequest by an admin succeeds; assert the new assignee.
- Test: ReassignRequest by a non-admin throws UnauthorizedException.
[WRONG: "write the user stories and acceptance criteria" asks for the
specification — the user-observable behaviour that the team is committing
to deliver. Unit-test specifications are 5.1's work and verify selected
internal behaviours of the implementation. Several criteria from the
chosen response (AC-1.4 edit-while-pending, AC-2.5 immutable decisions,
AC-4.2 re-assignment logged in history) are about the cross-flow user
experience, not about isolated method behaviour. The user stories are
the specification; the unit tests are part of the implementation's
verification later.]
```
