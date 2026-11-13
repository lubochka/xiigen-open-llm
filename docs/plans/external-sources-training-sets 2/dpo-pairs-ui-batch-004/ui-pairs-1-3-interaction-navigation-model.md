# UI Pairs — Sub-phase 1.3 Interaction and Navigation Model
# Batch: dpo-pairs-ui-batch-004
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: interaction states, navigation paths, controls, user task movement
# Pair count: 4

---

## Pair AP4-1.3-001 — interaction state design confused with token specification

```
source_id: xiigen-ui-batch-004-AP4-1.3-001
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.3
rejected_sub_phase: 1.1
```

Prompt: `"design the interaction states for the file-upload control — drag-over, dropping, uploading, success, and error"`

```
CHOSEN — 1.3 Interaction and Navigation Model:
A state diagram for the file-upload control. Each state names the entry
trigger, the visible feedback, and the exit triggers.

States:

S1 Idle:
  Entry: page loads, or after a successful upload, or after the user
   dismisses an error.
  Visible: dashed border, "Drag a file here or click to choose" message,
   small icon.
  Exits: file dragged over → S2; file selected via click → S4.

S2 DragOver:
  Entry: a draggable file enters the control's hit area.
  Visible: solid border (color-primary-500), background tint, message
   changes to "Drop to upload <filename>".
  The control accepts only the file types listed in the accept attribute;
   if the dragged file does not match, transition to S2-rejected
   (separate state).
  Exits: file drops → S3; user drags away without dropping → S1.

S2-rejected DragOverInvalid:
  Same visible as S2 but border is color-error-500 and the message reads
  "<filename> is not a supported file type". No drop action accepted.
  Exits: user drags away → S1.

S3 Dropping (transient, ≤ 100 ms):
  Entry: user releases the dragged file in the hit area.
  Visible: a brief flash of the success border color before transitioning
   to S4. This state exists to avoid a jarring jump from drag-over to
   upload.
  Exits: always transitions to S4 within 100 ms.

S4 Uploading:
  Entry: file has been accepted; upload starts.
  Visible: progress bar underneath the control, file name and size
   shown, a Cancel button.
  The progress bar shows determinate progress when the upload responds
   with progress events; otherwise it is indeterminate (oscillating).
  Exits: upload completes → S5; user clicks Cancel → S1; upload fails
   → S6.

S5 Success:
  Entry: upload completed.
  Visible: green checkmark, file name, "Replace" link to start over.
  Auto-dismiss to S1 is forbidden — the user must see confirmation.
  Exits: user clicks Replace → S1.

S6 Error:
  Entry: upload failed (network error, server-side validation failure,
   timeout).
  Visible: red icon, error message that names the cause when known
   ("File too large — maximum 10 MB", "Server unavailable, try again"),
   a Retry button.
  Exits: user clicks Retry → S4 (with the same file); user clicks
   Dismiss → S1.

Disallowed transitions:
  Direct S2 → S5 — uploads cannot succeed before S4 (the model rejects
   the implementation that "skips" S4 by uploading on drop).
  Direct S4 → S2 — the user cannot drag a new file while an upload
   is in progress; they must Cancel first.

Forbidden ambiguous states:
  No state where the control is partially active without visible
   feedback. Every state has a clear visible signature.

REJECTED — 1.1 Design System and Tokens:
Define the visual tokens for the file-upload control. Border-default:
1.5px dashed color-gray-300. Border-active: 2px solid color-primary-500.
Background-active: color-primary-50. Border-error: 2px solid
color-error-500. Background-error: color-error-50. Progress-bar-color:
color-primary-500. Progress-bar-height: 4px. Add to design-tokens
file under upload-control.*.
[WRONG: "design the interaction states" asks for the state diagram —
which states exist, what triggers each transition, what feedback is
visible in each, and which transitions are forbidden. Token definitions
are 1.1's responsibility and are referenced from the state diagram. The
state diagram is the missing artifact; tokens are the inputs to the
state diagram, not its output.]
```

---

## Pair AP4-1.3-002 — navigation flow confused with layout structure

```
source_id: xiigen-ui-batch-004-AP4-1.3-002
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.3
rejected_sub_phase: 1.2
```

Prompt: `"design the navigation flow for users reaching different parts of the application — top-level sections, deep links, and the back-button behavior"`

```
CHOSEN — 1.3 Interaction and Navigation Model:
A navigation graph plus rules for back, forward, deep-link, and
modal-vs-page transitions.

Top-level sections:
  Dashboard, Customers, Orders, Reports, Settings.
  Each is a top-level route (/dashboard, /customers, etc.) and appears
  in the persistent sidebar.

Within-section navigation:
  Each section has a default landing page. Sub-pages are reached via
  in-section navigation: tabs at the top of the content area, or
  contextual links inside content (e.g. clicking a customer in the
  list takes the user to /customers/<id>).

Back-button behaviour:
  Browser Back returns the user to the previous page in their history.
  Back must never cross section boundaries unintentionally; if a user
  arrives at a customer detail from Reports, browser Back returns
  them to Reports, not to Customers.
  In-app "Back to list" affordances are explicit: they are buttons
  that send the user to the parent page within the current section.

Deep-link behaviour:
  Every page has a stable URL. Deep links must work without prior
  navigation: opening /orders/<id> directly takes the user to that
  order's detail; opening /reports/sales?range=last-quarter applies
  the filter immediately.
  Deep-link state includes filters, pagination, and sort. Selection
  state (which row is highlighted in a list) is not part of the URL
  unless it is also in a panel that the URL describes.

Modal vs page:
  Decision rule: if the activity is a quick action that the user
  expects to return from to where they were (edit a single field,
  confirm a deletion), it is a modal. The URL does not change for
  modals.
  If the activity has its own deep-linkable state, it is a page,
  not a modal. Order detail is a page; "Confirm cancel" is a modal.
  Forbidden: a modal that the user might want to deep-link to or
  share (e.g. an "edit invoice" modal — that is a page, /invoices/
  <id>/edit).

Tab-vs-section change:
  Within a section, tabs are URL-encoded (?tab=customers) so deep
  links to a specific tab work.
  Tab change does not push a history entry; section change pushes
  a history entry. This means browser Back skips through tabs but
  stops at section boundaries.

Unsaved changes guard:
  Navigating away from a page with unsaved form input prompts
  "Discard changes?" with Cancel (stay) and Discard (proceed).
  Triggered for any navigation: link clicks, browser Back, browser
  Forward, sidebar clicks. Not triggered for explicit Save actions.

REJECTED — 1.2 Layout and Responsive Structure:
Define the layout of the navigation. The sidebar is 240 px wide on
desktop with the section list. On mobile, the sidebar collapses to a
bottom tab bar with the five top-level icons. The page header has a
breadcrumb on desktop and a single back-arrow on mobile. Tabs sit
under the page header, full width on desktop and scrollable on mobile.
Modals appear centred on desktop and as bottom sheets on mobile.
[WRONG: "design the navigation flow" asks for the navigation graph and
the interaction rules: how Back works, what is a modal vs page, how
deep links restore state, what triggers an unsaved-changes prompt.
Layout decisions about sidebar width and modal positioning are 1.2's
responsibility — they affect where the navigation chrome sits, but
they do not answer the question of how the user moves between pages
or what happens to their state along the way.]
```

---

## Pair AP4-1.3-003 — control behaviour confused with frontend implementation

```
source_id: xiigen-ui-batch-004-AP4-1.3-003
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.3
rejected_sub_phase: 4.2
```

Prompt: `"design the autocomplete control — when suggestions appear, how the user picks one, and how the keyboard behaves"`

```
CHOSEN — 1.3 Interaction and Navigation Model:
The autocomplete control's interaction model. Each behaviour names the
trigger, the response, and the constraints.

Suggestion appearance:
  Suggestions are not shown when the input is empty.
  Suggestions appear when the user has typed ≥ 2 characters AND there
  is at least one matching suggestion.
  Suggestions disappear when the user clears the input, picks a
  suggestion, or focuses outside the control.

Suggestion content:
  At most 8 suggestions visible at once. The remaining are reachable
  by typing more characters or by scrolling within the suggestion
  list.
  Each suggestion has a primary label (the matched text with the
  matching substring emphasised) and an optional secondary label
  (e.g. category, frequency).

Keyboard behaviour:
  Down arrow → focus the first suggestion (or the next when one is
   already focused).
  Up arrow → focus the previous suggestion; from the first suggestion,
   wrap to the input (do not exit to the page above).
  Enter on a focused suggestion → pick it. The input becomes the
   suggestion's primary label; the suggestions close.
  Enter on the input with no suggestion focused → submit the typed
   value (free-text fallback when free-text is allowed; otherwise
   no-op with a subtle shake feedback).
  Esc → close the suggestions and keep the typed input.
  Tab → close the suggestions and let focus move forward to the
   next focusable element. Do not pick the focused suggestion on
   Tab — that would cause unintended selections when the user is
   moving through the form.

Mouse behaviour:
  Hovering a suggestion does not focus it; clicking does. Hover only
   produces a visual highlight.
  Clicking a suggestion picks it.

Free-text fallback:
  Some autocomplete instances allow values not in the suggestion list
  (e.g. searching for any phrase). The fallback is opt-in per instance
  via an allowFreeText prop. When false, Enter on the input without
  a focused suggestion does nothing (with the shake feedback above).

Forbidden patterns:
  Auto-pick the first suggestion when the user blurs the input. This
  would commit a value the user did not intend.
  Auto-submit the input on every keystroke. The control proposes
  suggestions; the user picks.

REJECTED — 4.2 Frontend and Client Integration:
Implement the autocomplete control as a React component. Use Downshift
or Headless UI's Combobox primitive for accessibility-grade keyboard
handling. Debounce the suggestion fetch by 200 ms. Cancel in-flight
fetches when a new keystroke arrives using AbortController. Cache
recent results in a useRef-stored Map for instant repeat queries.
Render the suggestions list with React.memo to avoid re-render on
hover. Wire the picked value to the parent form via onChange.
[WRONG: "design the autocomplete control — when suggestions appear,
how the user picks one, how the keyboard behaves" asks for the
interaction model: triggers, transitions, forbidden patterns. The
React implementation with Downshift, AbortController, and useRef caching
is 4.2 frontend work that consumes the interaction model after it is
designed. The interaction model must exist first; the implementation
follows. Returning the implementation skips the design.]
```

---

## Pair AP4-1.3-004 — task-flow design confused with frontend implementation

```
source_id: xiigen-ui-batch-004-AP4-1.3-004
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.3
rejected_sub_phase: 4.2
```

Prompt: `"design how a user moves through the bulk-edit operation — selecting rows, picking the action, confirming, and seeing the result"`

```
CHOSEN — 1.3 Interaction and Navigation Model:
A four-step interaction model with clear feedback and a recovery path
at every step.

Step 1 — Selection:
  Trigger: user checks the row's checkbox or uses the header
   "Select all" checkbox.
  Feedback: a sticky action bar appears at the top of the table
   showing the count: "<n> rows selected".
  Constraint: bulk operations cannot run with zero rows; the action bar
   does not appear until the count is ≥ 1.
  Recovery: clicking "Clear" in the action bar deselects all rows
   and removes the bar.

Step 2 — Action picking:
  Trigger: user clicks the action menu in the action bar.
  Feedback: a menu opens with the available bulk actions: Update status,
   Assign owner, Delete, Export.
  Constraint: actions that do not apply to the current selection are
   disabled with a tooltip explaining why ("Cannot delete archived
   items; deselect them first").
  Recovery: clicking outside the menu closes it without selecting
   an action.

Step 3 — Confirmation:
  Trigger: user picks an action that requires a parameter (Update
   status, Assign owner) or a destructive action (Delete).
  Feedback: a confirmation modal appears showing: the action name, the
   parameter input (a status dropdown, an owner picker), the row count,
   and a representative sample of 3 rows so the user can verify the
   selection is correct.
  Destructive actions add an explicit "Type DELETE to confirm" pattern
   when the count is > 50 (high-impact safety).
  Recovery: Cancel closes the modal and returns the user to the
   selection state. The selection is preserved.

Step 4 — Result:
  Trigger: the user clicks Apply in the modal.
  Feedback: an inline progress bar in the action bar shows the
   completion percent. The result appears as a toast: "<n> rows
   updated" with an Undo link if the action is reversible.
  Undo behaviour: clicking Undo within 30 seconds rolls back the action.
   Beyond 30 seconds, Undo is no longer available — this is communicated
   in the toast.
  Constraint: the result must be visible while the user is still on
   the page. Auto-dismiss the toast after 8 seconds; never remove it
   mid-action.

Cross-cutting rule:
  At any step, the user can press Esc to cancel and return to selection
  state. Browser Back leaves the page and prompts an unsaved-changes
  guard if there are pending unconfirmed actions.

REJECTED — 4.2 Frontend and Client Integration:
Implement the bulk-edit flow in React. Use a useReducer for selection
state. Wire the action bar to a sticky positioning approach with
position: sticky. Implement the confirmation modal with Radix UI's
AlertDialog. Use React Query mutations for the bulk operations. On
success, invalidate the relevant query keys to refresh the table.
On failure, show a toast via the toast hook. Wire the Undo button
to the inverse mutation. Use Zustand for the toast queue.
[WRONG: "design how a user moves through the bulk-edit operation"
asks for the four-step interaction model: triggers, feedback, recovery
paths, and constraints. The React implementation with useReducer,
Radix, React Query, and Zustand is 4.2's frontend work that follows
the design. Returning code when an interaction model is asked for
skips the design and produces an implementation that does not have a
clear specification of "what is the user trying to do at each step".]
```
