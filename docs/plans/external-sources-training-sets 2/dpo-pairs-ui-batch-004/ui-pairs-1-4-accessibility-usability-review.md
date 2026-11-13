# UI Pairs — Sub-phase 1.4 Accessibility and Usability Review
# Batch: dpo-pairs-ui-batch-004
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: accessibility constraints, usability risks, user-facing quality checks
# Pair count: 4

---

## Pair AP4-1.4-001 — accessibility usability review confused with manual QA acceptance

```
source_id: xiigen-ui-batch-004-AP4-1.4-001
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.4
rejected_sub_phase: 6.1
```

Prompt: `"perform an accessibility and usability review of the new checkout page during the design phase, before development starts"`

```
CHOSEN — 1.4 Accessibility and Usability Review:
A pre-development review of the design artefacts (Figma file, prototype,
spec). The output is a list of accessibility and usability constraints
that the implementation must satisfy.

Accessibility constraints (must hold by design):

Form labels:
  Every input has a visible label that is positioned above or to the
  left of the input. Placeholder-only labels are forbidden because
  they vanish on focus and disappear for screen readers when empty.
  The card-number input uses inputmode="numeric" + autocomplete="cc-number".
  Address fields use the standard autocomplete tokens (street-address,
  postal-code, country).

Error feedback:
  Errors appear inline beneath the relevant input within 100 ms of
  validation triggering. Errors are linked to the input via aria-describedby
  so screen readers announce them on focus.
  Error styling does not rely on color alone; an icon and text are
  always present.

Tab order:
  The Tab order matches visual reading order: top to bottom, left to
  right. The pay button is the last focusable element on the page.
  Focus is visible on every focusable element with a 3 px outline at
  color-primary-500.

Touch targets:
  Every actionable element is at least 44 × 44 logical pixels (Apple
  HIG and WCAG 2.5.5 minimum). The "Use saved card" radio buttons
  have a hit area larger than the visual circle.

Usability constraints (must hold by design):

Information density:
  The cart summary stays visible at all breakpoints — on desktop in
  a sticky right rail, on mobile collapsed but expandable from the
  page top.
  The total amount is the most visually prominent number on the page.

Form length:
  At most 7 fields visible at once. The address autocomplete reduces
  visible field count when the user accepts a suggestion.
  No card-number-confirmation field; the user types the number once.

Error recovery:
  When card validation fails, the cursor returns to the failed field
  and the entered value is preserved. A new card-number entry never
  loses the user's previously entered name and address.

Loading state:
  After Pay is pressed, the button shows a spinner and is disabled.
  The form remains visible (not hidden behind a fullscreen overlay)
  so the user knows what they submitted.
  If the request takes more than 5 seconds, a "still working" message
  appears beneath the button.

Output:
  A constraint list attached to the design ticket. The development
  ticket inherits the constraints; the implementation review (in 1.4
  again at the end of development) verifies each constraint is met.

REJECTED — 6.1 Manual QA and Acceptance Review:
The QA reviewer creates an acceptance test plan for the new checkout
page. After the page is implemented in staging, the reviewer walks
through happy paths and edge cases: place an order, place an order
with a saved card, place an order with an invalid card, place an
order from a tablet, place an order with the back button mid-flow.
The reviewer documents pass/fail and signs off if all paths pass.
[WRONG: the prompt says "during the design phase, before development
starts". 6.1 manual QA happens at release time on a built system; it
cannot run before development. 1.4 accessibility and usability review
happens during design and produces constraints that the development
team must follow. The two activities cover different points in time
and produce different artifacts (design constraints vs release sign-off).
Treating "before development" as "after development" misreads the
prompt's timing.]
```

---

## Pair AP4-1.4-002 — accessibility design constraint confused with interaction state design

```
source_id: xiigen-ui-batch-004-AP4-1.4-002
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.4
rejected_sub_phase: 1.3
```

Prompt: `"identify the accessibility risks in our toast-notification component as designed"`

```
CHOSEN — 1.4 Accessibility and Usability Review:
A risk register for the toast component. Each risk names the affected
user group, the failure mode, and the design constraint that mitigates it.

Risk 1 — Screen reader users miss toasts.
  Affected users: blind / low-vision users using JAWS, NVDA, VoiceOver.
  Failure mode: the toast appears visually but the screen reader does
   not announce it. The user does not know an action completed or
   failed.
  Constraint: the toast container is an aria-live region with role
   "status" for non-error toasts and role "alert" for error toasts.
   "alert" forces interruption; "status" announces politely.

Risk 2 — Auto-dismiss is too fast for users with cognitive disability,
low vision, or motor impairment.
  Affected users: users with cognitive disability, users using screen
   magnifiers (who must scan to find the toast before it disappears),
   users with motor impairment (who cannot quickly tap a CTA in the
   toast).
  Failure mode: the toast disappears before the user can read it or
   interact with its action.
  Constraint: minimum auto-dismiss is 8 seconds for any toast carrying
   text. Toasts with an action (Undo, View) do not auto-dismiss until
   the user moves focus elsewhere or hits 30 seconds.
  Constraint: the toast can be paused by hovering or focusing it; pause
   resumes when the focus leaves.

Risk 3 — Color-only error indication.
  Affected users: users with red-green colour-blindness, low contrast
   sensitivity, or in bright environments.
  Failure mode: an error toast looks like a success toast because the
   red/green difference is the only signal.
  Constraint: every toast carries an icon (check / warning / error /
   info) in addition to color. The icon is always shown.

Risk 4 — Toasts cover the action that triggered them.
  Affected users: anyone, but particularly users using a screen
   magnifier (where the toast may now be the only thing in view).
  Failure mode: the toast obscures the underlying action and the
   user loses context.
  Constraint: toasts appear in a corner that is consistent across the
   product and never covers the primary action area. Default position:
   bottom-right on desktop, full-width top on mobile.

Risk 5 — Focus is moved unexpectedly.
  Affected users: keyboard users.
  Failure mode: when the toast appears with an action button, focus
   jumps to the button, interrupting the user's typing or navigation.
  Constraint: focus does not move to the toast unless the toast is an
   "alert" requiring decision (rare; in that case, treat it as a
   modal, not a toast).

Each risk is logged with the constraint that mitigates it. The
implementation review (back in 1.4) verifies each constraint at
build time.

REJECTED — 1.3 Interaction and Navigation Model:
Define the toast interaction states. S1 — Hidden: no toast visible.
S2 — Appearing: toast slides in from bottom-right over 200 ms ease-out.
S3 — Visible: toast remains for 5 seconds. S4 — Dismissing: toast slides
out over 200 ms. Auto-dismiss to S1 after the timer expires. The user
can hover to pause the timer. The user can click an X icon to dismiss
manually. Multiple toasts stack vertically.
[WRONG: the prompt asks for the accessibility risks in the toast
component as designed. The interaction state diagram is 1.3's
responsibility and describes how the toast behaves; it does not
identify accessibility risks (screen-reader announcement, auto-dismiss
timing, color-only signalling, focus management). Returning the state
diagram answers a different question; the risk register is what 1.4
produces.]
```

---

## Pair AP4-1.4-003 — accessibility review confused with unit test authoring

```
source_id: xiigen-ui-batch-004-AP4-1.4-003
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.4
rejected_sub_phase: 5.1
```

Prompt: `"review the accessibility of the new modal dialog and identify the issues that need fixing before release"`

```
CHOSEN — 1.4 Accessibility and Usability Review:
Review the modal dialog against the WCAG 2.1 AA criteria and the
keyboard / screen-reader expected behaviours. Issues are recorded with
severity, the affected user group, and the required fix.

Issue 1 — Focus is not trapped (severity: high).
  Affected: keyboard users, screen reader users.
  Observed: when the modal is open, pressing Tab repeatedly eventually
   moves focus to elements behind the modal in the underlying page.
   Pressing Shift+Tab from the first focusable element in the modal
   moves focus to the URL bar / page elements behind.
  Required fix: focus must remain within the modal while it is open.
   The first and last focusable elements wrap to each other on Tab /
   Shift+Tab.

Issue 2 — Focus is not restored on close (severity: high).
  Affected: keyboard users.
  Observed: when the modal closes, focus moves to the body element
   (the top of the page).
  Required fix: when the modal closes, focus returns to the element
   that was focused immediately before the modal opened.

Issue 3 — Esc does not close the modal (severity: medium).
  Affected: keyboard users.
  Observed: only clicking the X icon or the Cancel button closes
   the modal.
  Required fix: pressing Esc closes the modal as if the user clicked
   Cancel. (For destructive-confirmation modals, Esc still closes —
   it is treated as cancel.)

Issue 4 — The modal is not announced to screen readers (severity: high).
  Affected: screen reader users.
  Observed: when the modal opens, JAWS continues reading the page
   behind the modal.
  Required fix: the modal container has role="dialog" + aria-modal="true",
   an accessible name (aria-labelledby pointing to the modal's heading),
   and an accessible description (aria-describedby pointing to the modal's
   primary text). When the modal opens, focus moves into the modal's
   heading and the screen reader announces the title and description.

Issue 5 — Backdrop click is the only way to close on touch (severity: low).
  Affected: motor-impairment users.
  Observed: on touch devices, the small X icon is hard to hit.
  Required fix: the X icon meets the 44 × 44 minimum touch target.
   Backdrop click also dismisses for non-destructive modals.

Issue 6 — Color contrast on the secondary button is 3.8:1 (severity: medium).
  Affected: low-vision users.
  Observed: the secondary button uses gray-500 text on a gray-50 background.
   Measured contrast 3.8:1; WCAG AA minimum is 4.5:1 for normal text.
  Required fix: switch to gray-700 on gray-50 (contrast 7.5:1) or use
   the larger text variant which has a 3:1 minimum.

Issue 7 — Modal width on mobile cuts off scrolling content (severity: low).
  Affected: all mobile users.
  Observed: when modal content exceeds the viewport, the page behind
   the modal also scrolls; the content within the modal is hard to
   scroll independently.
  Required fix: lock body scroll while the modal is open; scroll
   within the modal content area only.

Each issue is logged in the release-blocking ticket if severity is high
or medium. Severity-low issues are tracked but do not block release.

REJECTED — 5.1 Unit Test Design:
Write unit tests for the Modal component using @testing-library/react.
Test: rendering the modal makes its content visible. Test: clicking the
X button calls the onClose handler. Test: pressing Esc inside the modal
calls onClose. Test: focusable elements within the modal are reachable
by Tab. Test: the modal has role="dialog" and aria-modal="true". Test:
the trigger button receives focus after the modal is closed.
[WRONG: "review the accessibility of the modal and identify the issues
that need fixing" asks for an issue inventory with severities and required
fixes. Writing unit tests would verify some properties of the Modal
component but only those properties the test author thought to test;
issue 4 (announcement on open), issue 5 (touch target size), issue 6
(measured contrast ratio), and issue 7 (body scroll lock) are not
typically captured in unit tests because they involve runtime aria-live
behaviour, real-device touch input, contrast measurement, and full-page
scroll behaviour. Unit tests are supporting evidence; the accessibility
review is the inventory.]
```

---

## Pair AP4-1.4-004 — accessibility review confused with end-to-end testing

```
source_id: xiigen-ui-batch-004-AP4-1.4-004
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.4
rejected_sub_phase: 5.3
```

Prompt: `"identify the usability risks in the multi-step signup form and recommend mitigations before we ship"`

```
CHOSEN — 1.4 Accessibility and Usability Review:
A risk register over the user journey through the signup form. Each
risk names the failure mode, the affected user group, and the
recommended mitigation.

Risk 1 — Users abandon when the form looks long.
  Failure mode: a 7-field form on one screen produces high abandonment.
  Mitigation: the form is broken into three steps (account, profile,
   preferences) with a visible progress indicator. The first step is
   the lightest (email + password) so users complete it quickly and
   feel committed.

Risk 2 — Users do not realise the email confirmation is required.
  Failure mode: a user signs up, never sees a confirmation prompt,
   forgets to confirm, and is later surprised when they cannot sign in.
  Mitigation: the success-after-step-1 page clearly states "We sent a
   confirmation email to <address>" with a Resend link. The button
   to proceed in the app is disabled until the user clicks the link
   in the email; if they navigate around the app first, every page
   shows a "Confirm your email" banner.

Risk 3 — Password rules are not visible until validation fails.
  Failure mode: a user types a password, hits Continue, then sees
   "Password must contain a number". They edit and try again. They
   see "Password must contain a symbol". Frustrating.
  Mitigation: the password rules are shown above the password input
   from the moment the input becomes focused. As the user types,
   each rule shows a green check or a red dot in real time. No
   submit-and-fail loop.

Risk 4 — Form data is lost on browser back.
  Failure mode: a user on step 2 hits Back to review step 1, then
   Forward, and finds step 2's data gone.
  Mitigation: form data is preserved in component state across step
   navigation; form data is also persisted to localStorage keyed by
   the email address so a refresh recovers it.

Risk 5 — Users with non-Latin names fail validation.
  Failure mode: a name input rejects "ñ", "ä", apostrophes, hyphens,
   spaces. Users with names like "Sandra O'Connor" or "Anna Müller"
   cannot proceed.
  Mitigation: the name field accepts any Unicode letter, apostrophes,
   hyphens, and spaces. Validation rejects only digits and most
   punctuation. The field accepts at least 100 characters.

Risk 6 — Users on assistive devices lose context across steps.
  Failure mode: each step is a fresh page; the screen reader has to
   re-announce the page title and the form context every time, and
   users do not know where they are in the flow.
  Mitigation: each step is announced as "Step <n> of 3: <step name>"
   when it loads. The progress indicator is part of the announcement.

Risk 7 — Mobile users cannot easily reach the Continue button.
  Failure mode: on a small phone, after typing a long password, the
   keyboard covers the Continue button. The user has to dismiss the
   keyboard manually.
  Mitigation: the Continue button is sticky to the bottom of the
   viewport on mobile, above the keyboard. When the keyboard opens,
   the button repositions to remain visible.

Each risk has an owner and a target step in the build plan. Severities
high (1, 2, 3, 5) block release; medium (4, 6, 7) are tracked.

REJECTED — 5.3 End-to-End Scenario Testing:
Write Playwright tests for the signup flow. Cover: complete signup
happy path, password rules feedback, browser back-and-forward navigation
preserves state, name field accepts Unicode characters and apostrophes,
mobile viewport with on-screen keyboard, form validation with various
invalid inputs. Run the suite in CI on every release.
[WRONG: "identify the usability risks and recommend mitigations" asks
for a risk register the team can act on during design — what could go
wrong, who is affected, what to change. End-to-end tests verify some
of these mitigations after they have been implemented, but writing
tests does not produce a risk inventory. Several of the listed risks
(1 — abandonment due to length, 2 — users not realising confirmation
is required) are about user behaviour and judgment, which automated
tests cannot detect. The risk register is the artifact 1.4 produces;
end-to-end tests are downstream verification.]
```
