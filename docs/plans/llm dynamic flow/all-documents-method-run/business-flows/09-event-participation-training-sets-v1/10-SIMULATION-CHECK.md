# 10 - Simulation Check

## Source Being Simulated

`C:\Users\lubao\Documents\xiigen\business flows\09-event-participation.md`

The simulation checks whether these node sets can recreate event participation without using the source document as runtime input.

## Test Prompt 1 - Paid Event Registration

Runtime user prompt:

"A user registers for a paid event. Hold capacity for five minutes, process payment, issue a secure ticket, update capacity, add the event to the calendar, schedule reminders, score co-attendees, integrate their posts, and track analytics."

Expected behavior:

- Node-01 returns capacity hold, payment, ticketing, capacity update, calendar, reminders, participant scoring, feed evolution, privacy, and analytics.
- Node-02 writes exact domain statements for each label.
- Node-03 keeps capacity -> payment -> ticket -> capacity update in strict order, then schedules calendar/reminders and social/feed branches.
- Node-04 processes success, wait, fallback, and failure states.
- Node-05 applies reservation, payment, ticket, capacity, reminders, scoring, feed evolution, and analytics.
- Node-07 selects capacity, payment, ticket, reminder, privacy, scoring, feed, and analytics arbiters.
- Node-08 rejects double payment, oversell, readable QR, server-time reminders, anonymous attendee leaks, wrong scoring formula, feed takeover, and missing decay.
- Node-09 returns one answer explaining confirmed event participation.

Pass condition:

No ticket may be issued before payment success for a paid event, and capacity must not be decremented twice.

## Test Prompt 2 - Free Event

Runtime user prompt:

"Register me for a free event and send my ticket."

Expected behavior:

- Node-01 returns free registration, ticketing, capacity update.
- Node-03 skips only payment, not ticketing or capacity.
- Node-05 issues ticket and updates capacity.
- Node-09 says registration was confirmed without payment.

Pass condition:

The simulation must not create a payment attempt for the free event.

## Test Prompt 3 - Capacity Race

Runtime user prompt:

"Two users try to buy the last ticket at the same time."

Expected behavior:

- Node-01 returns capacity race and waitlist.
- Node-03 plans one lock winner and one waitlist result.
- Node-08 rejects two confirmed tickets for one spot.
- Node-09 explains full-event or waitlist handling for the losing user.

Pass condition:

Only one user receives the last ticket.

## Test Prompt 4 - Anonymous Attendance

Runtime user prompt:

"Let the user attend anonymously so other attendees cannot see them."

Expected behavior:

- Node-01 returns anonymous attendance and attendee privacy.
- Node-03 keeps registration, ticket, calendar, and reminders active, but limits participant discovery and feed scoring.
- Node-08 rejects attendee-facing participant list exposure.
- Node-09 explains private participation.

Pass condition:

The attendee must not appear in other attendees' participant discovery.

## Test Prompt 5 - Post-Event Feed Decay

Runtime user prompt:

"After the event ends, reduce participant feed priority back to normal by seven days later but keep a small co-attendee bonus."

Expected behavior:

- Node-01 returns post-event decay and permanent bonus.
- Node-03 plans decay from EventCompleted to T+7.
- Node-05 applies decay and 0.05 bonus.
- Node-08 rejects never-ending boost.
- Node-09 explains post-event tapering.

Pass condition:

The event boost must decay and must not remain pinned forever.

## Missing-Training Detector

If the simulation issues paid tickets before payment success, add Node-03, Node-05, and Node-08 payment-gate pairs.

If capacity can oversell, add Node-03, Node-04, and Node-08 capacity-race pairs.

If free events skip ticketing or capacity update, add Node-03 and Node-05 free-registration pairs.

If QR tickets are readable or reusable, add Node-02 and Node-08 ticket-security pairs.

If reminders ignore user timezone or missed reminders disappear, add Node-02, Node-03, and Node-08 reminder pairs.

If anonymous attendance appears in participant discovery, add Node-03 and Node-08 privacy pairs.

If participant scoring hides the four equal components, add Node-02 and Node-08 scoring pairs.

If event participant posts exceed forty percent of the feed or never decay, add Node-05 and Node-08 feed-evolution pairs.

