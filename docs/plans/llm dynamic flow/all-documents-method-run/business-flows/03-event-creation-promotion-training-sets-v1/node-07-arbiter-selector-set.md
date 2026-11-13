# Node-07 - Arbiter Selector Set

## Pair 7.1 - Event creation

A: Results include event storage, published status, and EventCreated.

B: Select arbiters:
- validation arbiter: event fields and date must be valid.
- event sequence arbiter: EventCreated must follow storage.
- permission arbiter: organizer must be allowed to create events.

Pair 7.1 - Phrasing B

A: Event is newly published.

B: Select [validation arbiter, event sequence arbiter, permission arbiter].

## Pair 7.2 - Audience matching

A: Results include match scores, audience tiers, and match reasons.

B: Select arbiters:
- matching integrity arbiter: scoring weights and thresholds must match the document.
- privacy arbiter: match reasons must not expose sensitive interests.
- targeting arbiter: strong, medium, and weak segmentation must be correct.

Pair 7.2 - Phrasing B

A: Audience has been identified.

B: Select [matching integrity arbiter, privacy arbiter, targeting arbiter].

## Pair 7.3 - Feed placement

A: Results include feed insertion positions.

B: Select arbiters:
- feed placement arbiter: positions must match score thresholds.
- user experience arbiter: high-match users should see relevant events prominently.

Pair 7.3 - Phrasing B

A: Event was placed in feeds.

B: Select [feed placement arbiter, user experience arbiter].

## Pair 7.4 - Notifications

A: Results include in-app, push, email, SMS, urgency, distance, and channel preferences.

B: Select arbiters:
- notification rules arbiter: channel rules must match score, urgency, distance, and settings.
- abuse arbiter: organizers cannot directly message users outside platform channels.
- operations arbiter: burst delivery must preserve queued work.

Pair 7.4 - Phrasing B

A: Notifications were sent.

B: Select [notification rules arbiter, abuse arbiter, operations arbiter].

## Pair 7.5 - Paid event

A: Results include pricing tiers, payment processing, capacity, and refunds.

B: Select arbiters:
- payment arbiter: Stripe delegation and refund rules must be respected.
- capacity arbiter: registrations must not oversell.
- attendee communication arbiter: price changes require re-notification.

Pair 7.5 - Phrasing B

A: Paid limited-capacity event.

B: Select [payment arbiter, capacity arbiter, attendee communication arbiter].

## Pair 7.6 - Suspended organizer

A: Results include organizer suspension, cancelled notifications, and suspended event status.

B: Select arbiters:
- safety arbiter: promotion must stop.
- attendee communication arbiter: registered attendees must be notified.
- status arbiter: event status must be suspended.

Pair 7.6 - Phrasing B

A: Organizer was suspended during promotion.

B: Select [safety arbiter, attendee communication arbiter, status arbiter].

## Pair 7.7 - Campaign metrics

A: Results include reach, impressions, click-through, conversions, ROI, and revenue.

B: Select arbiters:
- metrics integrity arbiter: campaign metrics must match delivery and feed results.
- billing arbiter: paid event revenue must be consistent.

Pair 7.7 - Phrasing B

A: Promotion campaign completed.

B: Select [metrics integrity arbiter, billing arbiter].

## Pair 7.8 - Full event promotion

A: Results include event creation, indexing, analytics, matching, feed update, notifications, and campaign metrics.

B: Select arbiters:
- validation arbiter
- event sequence arbiter
- matching integrity arbiter
- feed placement arbiter
- notification rules arbiter
- capacity arbiter
- metrics integrity arbiter
- formatter consistency arbiter

Pair 7.8 - Phrasing B

A: Full event creation and promotion result group.

B: Select arbiters for validation, order, matching, feed, notification, capacity, metrics, and final consistency.
