# Node-03 - Scheme Planner Set

## Pair 3.1 - Full event promotion path

A: Domain translations describe event submission, storage, indexing, analytics, matching, feed injection, notifications, and campaign metrics.

B:
b1: Validate and store the event.
fb1: Check organizer permission and event fields, then write the event before publishing EventCreated.

b2: Index event for search.
fb2: Create a searchable index entry and publish EventIndexed.

b3: Analyze event.
fb3: Predict attendance, virality, and category trends.

b4: Match target audience.
fb4: Apply five-factor scoring and segment into strong, medium, and weak tiers.

b5: Place event in feeds.
fb5: Use score thresholds to choose feed position.

b6: Deliver notifications.
fb6: Use channel rules for in-app, push, email, and SMS.

b7: Aggregate promotion metrics.
fb7: Collect reach, impressions, click-through, conversions, and revenue outcomes.

Pair 3.1 - Phrasing B

A: "Plan the whole event creation and promotion lifecycle."

B: Store and announce the event, index it, analyze it, match users, update feeds, notify users, and measure the campaign.

## Pair 3.2 - Paid physical event

A: Domain translations describe paid physical event with early bird pricing and limited capacity.

B:
b1: Validate paid event fields.
fb1: Require valid price tiers, location, date, capacity, and organizer permission.

b2: Apply paid-event approval.
fb2: Use approval workflow before promotion if required.

b3: Match with location and price sensitivity.
fb3: Weight location and price factors according to the scoring formula.

b4: Protect capacity.
fb4: Use atomic decrement for registrations and waitlist loser when one seat remains.

b5: Notify price-sensitive audience.
fb5: Send time-sensitive notifications for early bird deadlines.

Pair 3.2 - Phrasing B

A: "Plan a paid local event with limited seats."

B: Validate price and capacity, approve if needed, match by location and price, protect seats, and notify around pricing deadlines.

## Pair 3.3 - Free virtual event

A: Domain translations describe a free virtual event with unlimited capacity.

B:
b1: Validate virtual event details.
fb1: Require virtual link and event time; no payment processing.

b2: Broaden matching.
fb2: Do not apply price sensitivity filter as a limiting factor.

b3: Notify with virtual access.
fb3: Include virtual access information in appropriate notification content.

b4: Track virtual campaign metrics.
fb4: Measure reach, impressions, click-through, and conversions without capacity fill constraints.

Pair 3.3 - Phrasing B

A: "Plan promotion for a free online event."

B: Validate virtual details, match broadly, include the link in messaging, and track campaign engagement.

## Pair 3.4 - Event edit after publication

A: Domain translations describe organizer changing date, location, or price after publication.

B:
b1: Validate edit.
fb1: Confirm new date, location, or price is valid.

b2: Recalculate matching and feed placement.
fb2: New event parameters can change audience relevance and feed position.

b3: Re-notify registered attendees.
fb3: Date, location, and price changes must be communicated to attendees.

b4: Preserve audit trail.
fb4: Keep evidence of what changed and who was notified.

Pair 3.4 - Phrasing B

A: "Plan what happens when an event changes after it is already published."

B: Validate changes, rerun affected matching and feed logic, notify registered users, and keep traceability.

## Pair 3.5 - Notification burst

A: Domain translations describe more than ten thousand notifications for a popular event.

B:
b1: Segment delivery queue.
fb1: Strong matches are highest priority.

b2: Apply service timing.
fb2: Strong matches should be delivered within five minutes; others within one hour.

b3: Preserve lower-priority notifications.
fb3: Queue instead of dropping delayed notifications.

b4: Report delivery status.
fb4: Include by-channel totals and failures in promotion metrics.

Pair 3.5 - Phrasing B

A: "Plan notification delivery when the campaign is very large."

B: Prioritize, queue, preserve, and report delivery.

## Pair 3.6 - Suspended organizer

A: Domain translations describe organizer account suspended mid-promotion.

B:
b1: Stop pending promotion.
fb1: Cancel pending notifications immediately.

b2: Mark event suspended.
fb2: Move event status to suspended.

b3: Notify registered attendees.
fb3: Tell attendees the event status changed.

b4: Prevent further promotion.
fb4: Do not continue feed boosts or campaign delivery.

Pair 3.6 - Phrasing B

A: "Plan event handling when the organizer is suspended."

B: Stop pending delivery, suspend the event, inform attendees, and halt promotion.

## Pair 3.7 - Zero audience

A: Domain translations describe matching produces zero audience.

B:
b1: Detect zero matched audience.
fb1: Recognize that no target segment was found.

b2: Apply fallback audience.
fb2: Show to same region or same industry with low confidence.

b3: Keep confidence visible internally.
fb3: Do not pretend fallback matches are strong matches.

b4: Continue campaign measurement.
fb4: Track fallback performance separately.

Pair 3.7 - Phrasing B

A: "Plan promotion for a very niche event with no exact audience."

B: Use regional or industry fallback at low confidence and measure it separately.

## Pair 3.8 - Past date

A: Domain translations describe event date is already in the past.

B:
b1: Reject published creation.
fb1: Do not publish or promote an event dated in the past.

b2: Auto-archive stale draft.
fb2: If a draft passes into the past, archive it without promotion.

b3: Explain correction.
fb3: Ask organizer to choose a valid future date.

Pair 3.8 - Phrasing B

A: "Plan validation when an event is scheduled in the past."

B: Reject promotion, archive stale drafts, and ask for a future date.
