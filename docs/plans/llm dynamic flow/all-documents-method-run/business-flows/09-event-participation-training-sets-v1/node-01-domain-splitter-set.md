# Node-01 - Domain Splitter Set

## Purpose

Runtime A is a human prompt about participating in an event.
Runtime B is the array of domains the prompt touches.

## Pair 1.1 - Register with capacity hold

A: "Let a user register for an event only if capacity is available, and hold the spot for five minutes while payment finishes."

B: [event availability and capacity hold, paid-event payment processing]

Pair 1.1 - Phrasing B

A: "Check event capacity, reserve a temporary place, and release it if checkout expires."

B: [event availability and capacity hold, payment timeout handling]

## Pair 1.2 - Free event registration

A: "For a free event, skip payment and issue the ticket after registration is confirmed."

B: [free-event direct registration, ticket issuance with QR code, atomic capacity update]

Pair 1.2 - Phrasing B

A: "If the event has no price, register the attendee directly and create the ticket."

B: [free-event direct registration, ticket issuance with QR code, atomic capacity update]

## Pair 1.3 - Paid event payment

A: "For a paid event, process payment securely, handle declined cards, and avoid double charging."

B: [paid-event payment processing, payment decline handling, double payment handling]

Pair 1.3 - Phrasing B

A: "Take payment for the ticket, release the reservation if payment fails, and make repeated payment callbacks idempotent."

B: [paid-event payment processing, payment decline handling, double payment handling]

## Pair 1.4 - Ticket and capacity

A: "After payment succeeds, issue a QR ticket and update remaining event capacity atomically."

B: [ticket issuance with QR code, atomic capacity update, ticket security]

Pair 1.4 - Phrasing B

A: "Generate the event ticket, prevent duplicate scans, and decrement available spots without overselling."

B: [ticket issuance with QR code, atomic capacity update, ticket security, capacity race handling]

## Pair 1.5 - Calendar and reminders

A: "Add the event to the user's calendar and schedule reminders at seven days, one day, one hour, and fifteen minutes before the event."

B: [calendar integration and local-time reminders]

Pair 1.5 - Phrasing B

A: "Use the attendee's timezone to schedule progressive event reminders."

B: [calendar integration and local-time reminders, missed reminder handling]

## Pair 1.6 - Attendee matching

A: "Identify other event participants and score attendee connections using event history, questionnaire similarity, group overlap, and audience match."

B: [participant identification, attendee connection scoring]

Pair 1.6 - Phrasing B

A: "For each co-attendee, calculate how relevant they are using four equal matching components."

B: [participant identification, attendee connection scoring]

## Pair 1.7 - Feed integration before event

A: "Show co-attendee posts in the user's feed with increasing weight as the event approaches."

B: [participant content feed integration, time-evolving feed weight]

Pair 1.7 - Phrasing B

A: "Boost participant content at T-7 days, T-1 day, and on event day."

B: [participant content feed integration, time-evolving feed weight, event-day real-time priority]

## Pair 1.8 - Post-event decay

A: "After the event, gradually reduce the event feed boost and keep a small permanent connection bonus for co-attendees."

B: [post-event decay and permanent co-attendee bonus, conversion and engagement analytics]

Pair 1.8 - Phrasing B

A: "Let event-related feed priority fade back to base by seven days after the event, with a lasting co-attendee bump."

B: [post-event decay and permanent co-attendee bonus, social impact analytics]

## Pair 1.9 - Privacy and anonymous attendance

A: "Allow a user to attend anonymously so only the organizer can see them, and do not expose connection scores to other users."

B: [payment, ticket, calendar, and attendee privacy security, participant identification]

Pair 1.9 - Phrasing B

A: "Respect anonymous attendance, participant list visibility, calendar consent, and private connection scores."

B: [payment, ticket, calendar, and attendee privacy security, participant identification]

## Pair 1.10 - System reliability and analytics

A: "Track registration conversion, payment success, reminder delivery, feed update speed, connection requests, revenue, and event ROI."

B: [conversion, revenue, engagement, social impact, and system-performance analytics]

Pair 1.10 - Phrasing B

A: "Measure the full event participation journey from booking through post-event relationship building."

B: [conversion, revenue, engagement, social impact, and system-performance analytics]

