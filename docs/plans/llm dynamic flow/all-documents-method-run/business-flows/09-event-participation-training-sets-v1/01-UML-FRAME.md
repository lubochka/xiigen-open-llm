# 01 - UML Frame

## Active Nodes

1. Domain splitter: receives a human prompt and emits domain labels.
2. Domain translator array: translates one prompt fragment for one domain.
3. Scheme planner: plans ordered pieces, parallel pieces, wait states, recovery paths, and reviews.
4. Domain processor array: processes one planned piece.
5. Applier array: applies one processed piece and emits an action result.
6. Result translator array: translates one formal result while checking consistency with all results.
7. Arbiter selector: chooses review units from formal results before formatting.
8. Arbiter units: pass, flag, or send fragments back for reprocessing.
9. Formatter: combines approved translated fragments into one answer.

## Array Nodes

Four nodes are arrays:

- Node 02 runs once per domain.
- Node 04 runs once per planned work piece.
- Node 05 runs once per planned work piece.
- Node 06 runs once per formal result.

This matters for Flow-09 because the source contains several independent branches: payment, ticketing, calendar/reminders, participant matching, feed integration, and time evolution. Some branches are sequential because money, tickets, and capacity must stay consistent. Other branches can run after ticket issuance and continue in the background.

## Main Edges

### Edge 1 - Prompt to Domain Splitter

What moves: a human request about event registration, ticketing, reminders, attendee matching, feed integration, or post-event analytics.

What the target receives: raw user wording.

What the target produces: all domain labels touched by the prompt.

What breaks if missing: the flow may skip payment, ticket, capacity, reminder, or social-feed work.

### Edge 2 - Domain Splitter to Domain Translator Array

What moves: one domain label plus its prompt fragment.

What the target receives: a single labeled fragment.

What the target produces: precise domain wording.

What breaks if missing: the planner may confuse paid registration, free registration, ticket issuance, participant matching, and feed evolution.

### Edge 3 - Domain Translator Array to Scheme Planner

What moves: all domain wordings.

What the target receives: the full domain set.

What the target produces: a sequence with capacity hold, payment or free registration, ticketing, capacity update, calendar, reminders, participant identification, scoring, feed integration, time evolution, post-event decay, recovery, and reviews.

What breaks if missing: the plan may issue tickets before payment, decrement capacity twice, or merge attendee feeds before registration is confirmed.

### Edge 4 - Scheme Planner to Domain Processor Array

What moves: one planned work piece.

What the target receives: one piece and context.

What the target produces: success, wait, fallback, and failure outcomes for that piece.

What breaks if missing: appliers cannot handle reservation expiry, payment decline, double webhook, capacity race, missed reminder, or anonymous participation.

### Edge 5 - Domain Processor Array to Applier Array

What moves: processed result and transformation instruction.

What the target receives: action-ready result.

What the target produces: concrete outcome.

What breaks if missing: holds, payments, tickets, reminders, feed adjustments, and analytics remain only planned.

### Edge 6 - Applier Array to Result Translator Array

What moves: formal results.

What the target receives: one formal result and all formal results for consistency.

What the target produces: readable wording.

What breaks if missing: the user receives internal state instead of an understandable participation result.

### Edge 7 - Applier Array to Arbiter Selector

What moves: formal result collection before final formatting.

What the target receives: all results and context.

What the target produces: selected review units.

What breaks if missing: payment, capacity, QR, privacy, reminder, feed-cap, and analytics checks may not run.

### Edge 8 - Arbiter Selector to Arbiter Units

What moves: review responsibilities.

What the target receives: a rule and result group.

What the target produces: pass, concern, or reprocess instruction.

What breaks if missing: violations are never examined.

### Edge 9 - Arbiter Units to Reprocessing

What moves: a rejected fragment and reason.

What the target receives: exact fragment needing correction.

What the target produces: corrected routing.

What breaks if missing: failures can be identified but not repaired.

### Edge 10 - Approved Results to Formatter

What moves: approved translated fragments.

What the target receives: reviewed, readable fragments only.

What the target produces: one final answer.

What breaks if missing: rejected payment, ticket, privacy, or feed results may appear in the final response.

