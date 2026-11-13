# Node-05 - Applier Set

## Purpose

Runtime A is one work piece, its transformation instruction, and the processed result from Node-04.
Runtime B is the concrete action and outcome.

## Pair 5.1 - Apply reservation hold

A: Work piece: capacity check and hold. Transformation instruction: do not accept payment or issue ticket without a live hold unless the event is free and direct registration is confirmed. Processed result: capacity is available and five-minute hold exists.

B: Action outcome: the user's spot is reserved temporarily for five minutes. The hold is not a ticket. If the hold expires, the spot is released and the user must retry or join waitlist.

Pair 5.1 - Phrasing B

A: Work piece: reserve spot. Transformation instruction: temporary capacity only. Processed result: hold exists.

B: Action outcome: capacity is protected during checkout without confirming attendance too early.

## Pair 5.2 - Apply payment result

A: Work piece: paid checkout. Transformation instruction: payment success gates ticket issuance; failure releases reservation. Processed result: payment success, decline, timeout, or duplicate callback.

B: Action outcome: on success, transaction is stored and ticketing may start. On decline or timeout, reservation is released and no ticket is issued. On duplicate callback, the original payment result is reused without a second charge.

Pair 5.2 - Phrasing B

A: Work piece: payment processing. Transformation instruction: no double charging and no ticket after failed payment. Processed result: payment state known.

B: Action outcome: only confirmed payment can produce a paid ticket.

## Pair 5.3 - Apply free registration

A: Work piece: free event registration. Transformation instruction: skip payment but keep ticket and capacity rules. Processed result: free registration confirmed.

B: Action outcome: the flow bypasses payment, issues the ticket, updates capacity, and continues calendar, reminders, participant scoring, feed integration, and analytics.

Pair 5.3 - Phrasing B

A: Work piece: no-price event. Transformation instruction: remove payment branch only. Processed result: registration valid.

B: Action outcome: the user becomes a confirmed attendee without any payment attempt.

## Pair 5.4 - Apply ticket and capacity update

A: Work piece: ticket issuance and capacity update. Transformation instruction: create ticket and decrement capacity exactly once. Processed result: ticket and atomic capacity outcome ready.

B: Action outcome: one secure QR ticket is created and delivered, and the available capacity counter is decremented under atomic control. Real-time capacity display is refreshed.

Pair 5.4 - Phrasing B

A: Work piece: confirmed attendance. Transformation instruction: ticket and capacity must agree. Processed result: ticket can be issued.

B: Action outcome: the user has a valid ticket and the event no longer treats that spot as available.

## Pair 5.5 - Apply calendar and reminders

A: Work piece: scheduling support. Transformation instruction: schedule in user's local timezone and recover missed reminders. Processed result: calendar consent and reminder preferences known.

B: Action outcome: the event is added to the user's calendar when consent allows, reminders are scheduled at the defined times, and missed reminders are sent by catch-up with adjusted wording.

Pair 5.5 - Phrasing B

A: Work piece: prepare attendee. Transformation instruction: respect preferences and quiet hours. Processed result: reminder plan ready.

B: Action outcome: the attendee receives useful preparation messages without ignoring notification settings.

## Pair 5.6 - Apply participant scoring

A: Work piece: participant identification and scoring. Transformation instruction: score only visible attendees and keep raw sensitive data hidden. Processed result: eligible participants and four component scores are ready.

B: Action outcome: co-attendee connection scores are calculated from event history, questionnaire similarity, group overlap, and audience match, classified into strong, medium, or weak, and stored for feed behavior without exposing the score to other users.

Pair 5.6 - Phrasing B

A: Work piece: social matching. Transformation instruction: visibility and privacy first. Processed result: participant scores ready.

B: Action outcome: the feed system receives allowed co-attendee relevance signals.

## Pair 5.7 - Apply feed time evolution

A: Work piece: participant feed integration. Transformation instruction: use time multiplier, diversity cap, spacing, and display format. Processed result: participant posts and weights are ready.

B: Action outcome: participant posts are inserted into feeds. At T-7 the weight rises to 1.5 times, at T-1 to 2 times, on event day to 3 times and pinned priority, then after the event it decays toward base with permanent 0.05 bonus.

Pair 5.7 - Phrasing B

A: Work piece: event social feed. Transformation instruction: time changes priority. Processed result: feed positions ready.

B: Action outcome: event-related social content grows more visible before and during the event and then gradually recedes.

## Pair 5.8 - Apply analytics

A: Work piece: event metrics and system monitoring. Transformation instruction: record conversion, revenue, engagement, social impact, and performance. Processed result: events and outcomes are available.

B: Action outcome: analytics records registration, payment, waitlist, ticket, reminder, feed, connection, revenue, retention, and system performance metrics for the event participation journey.

Pair 5.8 - Phrasing B

A: Work piece: measure outcomes. Transformation instruction: include business and reliability signals. Processed result: metrics ready.

B: Action outcome: the event can be evaluated for revenue, engagement, networking effect, and operational health.

