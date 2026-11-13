# Spec Pairs — Sub-phase 2.3 Data and Validation Rules
# Batch: dpo-pairs-spec-batch-005
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: input data, validation policy, state transitions, failure behavior
# Pair count: 4

---

## Pair AP5-2.3-001 — validation rule specification confused with backend implementation

```
source_id: xiigen-spec-batch-005-AP5-2.3-001
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.3
rejected_sub_phase: 4.1
```

Prompt: `"specify the validation rules for the create-account form — what is required, what formats are accepted, and what happens on each failure"`

```
CHOSEN — 2.3 Data and Validation Rules:
A field-by-field validation policy. Each rule names what is checked, what
counts as a pass, and what the user sees on failure.

Field: email
  Required.
  Format: matches the published WHATWG email-input parsing rule. Reject
   addresses with no @ symbol, multiple @ symbols, or a domain without
   at least one dot.
  Length: maximum 254 characters (RFC 5321 envelope limit).
  Failure feedback (inline, beneath the field):
    Empty: "Email address is required".
    Bad format: "Enter a valid email address (e.g. name@example.com)".
    Too long: "Email address is too long" — does not include the
      maximum length to keep the message simple; field hints provide
      the limit.

Field: password
  Required.
  Length: minimum 12 characters, maximum 200.
  Composition: must contain ≥3 of: lowercase letter, uppercase letter,
   digit, symbol.
  Breach check: must not appear in the configured breach list (we use
   the k-anonymity API of HaveIBeenPwned with the 5-character-prefix
   query; no plaintext leaves the client).
  Failure feedback:
    Empty: "Password is required".
    Length too short: "Password must be at least 12 characters" with a
      live counter.
    Composition fail: list the missing classes — "Include at least one
      digit and one symbol".
    Breach match: "This password appears in known data breaches —
      please choose another".

Field: full name
  Required.
  Length: 1 to 100 characters.
  Allowed characters: any Unicode letter, mark, hyphen, apostrophe, space.
   Reject digits and most punctuation.
  Failure feedback:
    Empty: "Name is required".
    Forbidden character: "Name contains an unsupported character" — does
      not echo the user's name back to them in the error.

Field: country
  Required.
  Format: ISO 3166-1 alpha-2 code from a fixed list.
  Failure: "Choose a country from the list" (the field is a dropdown; this
   message is shown on form submit only).

Cross-field rule:
  When country = US, ZIP code is required and must be 5 or 9 digits.
  When country != US, postal code is optional; format is country-specific
   and not validated beyond non-empty.

State on validation failure:
  All entered values are preserved on validation-failure render.
  The first invalid field receives focus and is scrolled into view.
  The submit button stays enabled (do not block the user from re-trying;
   the failure is shown inline).

Server-side mirror:
  Every client-side rule has a server-side mirror. Server-side validation
  is the authority; client-side validation is a UX optimisation. A request
  that bypasses the client returns the same per-field validation errors
  via JSON.

REJECTED — 4.1 Backend Services:
Implement the validation in CreateAccountRequest with FluentValidation:
RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
RuleFor(x => x.Password).NotEmpty().MinimumLength(12).Must(BeStrongEnough);
RuleFor(x => x.FullName).NotEmpty().Matches(@"^[\p{L}\p{M}\s'-]+$");
RuleFor(x => x.Country).NotEmpty().Must(BeIso2Code);
Inject IBreachChecker. Add When(x => x.Country == "US") for the ZIP
rule. Add the validator to the DI container. Configure the API
controller to return ValidationProblemDetails with field-keyed errors
on validation failure.
[WRONG: "specify the validation rules" asks for the policy — what is
required, what formats are accepted, what the user sees on each failure.
The FluentValidation implementation is 4.1's work and follows from the
specification. The specification must exist before the implementation,
and several rules in the chosen response (the breach-check via k-anonymity,
the avoid-echoing-user-input rule, the cross-field US-ZIP rule, the
client-server mirroring rule) are policy decisions that an implementation
without them would silently miss.]
```

---

## Pair AP5-2.3-002 — state transition rules confused with data model design

```
source_id: xiigen-spec-batch-005-AP5-2.3-002
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.3
rejected_sub_phase: 3.2
```

Prompt: `"specify the state transitions and validation rules for the order lifecycle — what can become what, and under what conditions"`

```
CHOSEN — 2.3 Data and Validation Rules:
A state-transition specification with the conditions for each transition
and the failure behaviour when a transition is not permitted.

States:
  Draft, PendingPayment, Confirmed, InWarehouse, Shipped, Delivered,
  Cancelled, Returned, Refunded.

Permitted transitions (from → to, with the trigger condition):

  Draft → PendingPayment:
    Trigger: user submits the order with a valid payment method.
    Condition: cart is non-empty; all line items are still purchasable
     at the current price (no price change since last quote).
    On condition fail: validation error "Cart contents have changed —
     please review".

  PendingPayment → Confirmed:
    Trigger: payment provider confirms the charge succeeded.
    Condition: charge response includes a successful status and a
     non-zero amount equal to the order total.

  PendingPayment → Cancelled:
    Trigger: payment provider rejects the charge OR user cancels
     within 5 minutes.
    Condition: order is still in PendingPayment.

  Confirmed → InWarehouse:
    Trigger: warehouse system acknowledges receipt.
    Condition: all items reserved in inventory.
    On condition fail: order remains in Confirmed; alert is raised
     for the operations team after 4 hours.

  Confirmed → Cancelled:
    Trigger: user cancels via the self-service flow OR support cancels.
    Condition: no shipment has been created.
    On condition fail: "Cannot cancel — order is being prepared.
     Please request a return after delivery."

  InWarehouse → Shipped:
    Trigger: warehouse marks the order as shipped with a tracking number.
    Condition: tracking number is provided and the carrier is supported.

  Shipped → Delivered:
    Trigger: carrier webhook delivery confirmation OR manual mark from
     support.
    Condition: tracking shows delivered status.

  Shipped → Returned:
    Trigger: customer requests a return; carrier confirms receipt of
     the return shipment.
    Condition: order is within the 30-day return window.

  Delivered → Returned:
    Trigger: same as Shipped → Returned.

  Returned → Refunded:
    Trigger: warehouse confirms returned items are in receivable
     condition.
    Condition: all returned items pass the condition check.
    Partial: if some items pass and others do not, the order moves
     to PartiallyRefunded (a sub-state of Returned for the
     unreturned items).

Forbidden transitions (with explicit messages):

  Cancelled → anything: terminal state. A new order must be created.
  Refunded → anything: terminal state.
  Confirmed → Refunded: forbidden. Refund requires Returned first.
  Draft → Shipped: forbidden. Order must pass through PendingPayment
   and Confirmed.

Validation invariants:
  Every order has exactly one current state at any time. The state
  history is preserved as a timestamped log; querying "what state was
  the order in at time T" must always return a definite answer.

Idempotency:
  Each transition is idempotent within its trigger. A duplicate carrier
  webhook for "delivered" does not move Delivered → Delivered → ... .
  The state machine ignores transitions that would not change state.

REJECTED — 3.2 Data Model and Persistence Design:
The Order entity has fields: orderId, userId, total, currency, currentState
(enum), createdAtUtc, updatedAtUtc. A separate OrderStateHistory entity
records each transition with orderId, fromState, toState, transitionedAtUtc,
actor, reason. Index OrderStateHistory by orderId for fast lookup. Use
a composite primary key (orderId, transitionedAtUtc). The currentState
on Order is a denormalisation kept in sync with the latest history entry
via a trigger or application code.
[WRONG: "specify the state transitions and validation rules" asks for the
behavioural rules — what can become what, under what conditions, and
what happens when a transition is not permitted. The data model is 3.2's
responsibility and follows the rules; without the rules, the model has
no way to know which transitions need to be representable. The chosen
response specifies the rules; the model is downstream.]
```

---

## Pair AP5-2.3-003 — input validation specification confused with API contract design

```
source_id: xiigen-spec-batch-005-AP5-2.3-003
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.3
rejected_sub_phase: 3.3
```

Prompt: `"specify the validation rules for the search query input — accepted shapes, length limits, character handling, and what the user sees when input is rejected"`

```
CHOSEN — 2.3 Data and Validation Rules:
The validation policy for the search input. The rules apply across the
search bar, the URL parameter, and any API caller.

Allowed query content:
  Letters (any Unicode), digits, spaces.
  Punctuation: hyphens, apostrophes, periods (for abbreviations like
   "u.s."), question marks (treated as literal characters), commas.
  Operators: "+", "-", quotes (") for phrase search, parentheses for
   grouping. These have semantic meaning in the search syntax.

Length rules:
  Minimum 1 character that is not whitespace.
  Maximum 200 characters total. Length is measured in Unicode code points,
   not bytes (a query of 200 emoji is rejected because it exceeds the
   index's per-token budget; 200 ASCII characters is the floor).

Forbidden characters:
  Control characters (U+0000 through U+001F except U+0020 space, U+0009
   tab, U+000A newline, U+000D carriage return).
  Backslash (\) — not used in our query syntax; reject to prevent
   accidental injection patterns.
  Null byte (U+0000) explicitly listed.

Whitespace handling:
  Leading and trailing whitespace are trimmed.
  Internal multi-whitespace is collapsed to single spaces.
  A query that becomes empty after trimming is rejected.

Failure feedback:

For the search bar (interactive):
  Empty after trim: do nothing — the search button is disabled.
  Length over 200: inline message "Search query is too long" and the
   button is disabled. The query input cannot accept additional
   characters beyond 200.
  Forbidden character: inline message "This character is not allowed
   in search". The character is highlighted; the user can edit and
   the message clears once the character is removed.

For URL parameter (?q=...):
  Length over 200: redirect to the search page with q truncated to 200
   and a message banner "Search query truncated".
  Forbidden character: redirect to the search page with q stripped of
   forbidden characters and the same banner.
  Empty: redirect to /search with no q (open search bar focused).

For API callers:
  Length over 200: 400 Bad Request with body
   { "errors": { "q": ["maximum length is 200 characters"] } }.
  Forbidden character: 400 Bad Request with body
   { "errors": { "q": ["contains forbidden character"] } }.
  Empty after trim: 400 Bad Request "q is required".

Server-side mirror:
  The interactive search bar uses these rules client-side for fast
  feedback. The server enforces the same rules independently. Discrepancy
  between client and server is a server-bug to investigate; the server
  is authoritative.

REJECTED — 3.3 API Contract Definition:
Define the GET /search endpoint contract.
Request:
  GET /search?q={string,1-200}&page={int,>=1}&pageSize={int,1-100}
  &sort={enum: relevance|date|price}
Response 200:
  {
    "results": [ { "id": string, "title": string, ... } ],
    "total": int,
    "page": int,
    "pageSize": int
  }
Response 400:
  {
    "errors": { "<field>": [ "<message>" ] }
  }
[WRONG: "specify the validation rules — accepted shapes, length limits,
character handling, what the user sees when input is rejected" asks for
the policy. The API contract is 3.3's work and is the published shape of
the endpoint; it follows the validation policy. The rejected response
has the contract shape correct but does not specify the rules — what is
forbidden, why, what message the user sees in different surfaces (UI vs
URL vs API). The contract is a different artifact, downstream of the
validation rules.]
```

---

## Pair AP5-2.3-004 — failure-behaviour specification confused with backend implementation

```
source_id: xiigen-spec-batch-005-AP5-2.3-004
source_license_state: first_party
pair_kind: dpo_spec
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 2.3
rejected_sub_phase: 4.1
```

Prompt: `"specify what happens when a tenant import file fails validation — partial-row failures, file-level failures, and what the user sees"`

```
CHOSEN — 2.3 Data and Validation Rules:
A failure-behaviour policy for the tenant import file. Each kind of
failure has a specified user-visible response and a specified system
state.

Per-row validation:
  Each row in the import file is validated independently against the
  per-row rules: required fields, formats, foreign-key existence, and
  cross-field consistency.

Row-level failure handling:
  A failed row does not abort the import. Failed rows are collected.
  After processing all rows, the import returns a result that names:
   - total_rows: count.
   - successful_rows: count.
   - failed_rows: array of { row_number, field, error }.
   - imported_ids: ids of the records that were created.
  The user sees a summary screen showing the counts and a downloadable
   CSV of the failed rows with the original data plus an "error"
   column.

File-level validation (rejects the whole import):
  - File is empty.
  - File is not a valid CSV / XLSX.
  - File is missing one or more required header columns.
  - File exceeds 10,000 rows.
  - File exceeds 50 MB.

File-level failure handling:
  No rows are imported. The user sees a single error message naming
   the file-level cause:
   "The file is missing the 'email' column".
   "The file has 12,000 rows; the maximum is 10,000. Split the file
    and try again."
  The user can re-upload immediately; no rate-limit applies to a
   file-level rejection because nothing was changed.

Partial-success state:
  When any rows succeeded, the system has a partial-import state. The
   user can:
   - Accept the partial import and download the failed-rows file.
   - Roll back the partial import; the system deletes the imported
     rows. Roll-back is available for 1 hour from the import.

Re-running the import after fixing failed rows:
  Each row's idempotency key is derived from the (tenant_id, external_id)
  combination. Rows imported on a previous run with the same key are
  not duplicated; they are updated with the new data.
  This means the user can fix the failed-rows CSV and re-upload it
   without producing duplicates.

Logging and audit:
  Every import attempt is logged with the user, timestamp, file name,
   total rows, successful rows, failed rows. The audit entry is
   visible in the tenant's audit log.

Concurrent imports:
  Only one import per tenant may be in progress at a time. A second
   import attempt while one is in progress is rejected with
   "An import is already in progress; please wait for it to complete".

REJECTED — 4.1 Backend Services:
Implement TenantImportService.ImportAsync(stream, tenantId). Read the
file with CsvHelper. For each row, validate using FluentValidation; on
failure, append to a List<ImportRowError>. Use a transaction per chunk
of 500 rows to keep transactions small. On file-level errors, throw an
ImportFileException. Use DbContext.Database.BeginTransactionAsync for
the row-level work. Return an ImportResult { TotalRows, SuccessfulRows,
FailedRows }.
[WRONG: "specify what happens when a tenant import file fails" asks for
the user-visible behaviour and the policy for the failure cases. The
chosen response covers row-level partial failure handling, file-level
rejection, partial-success state, idempotency on re-upload, audit, and
concurrency — none of which are decided by writing the implementation.
The implementation in 4.1 will encode these decisions, but only after
they exist as a specification. Returning the implementation skips the
policy choices.]
```
