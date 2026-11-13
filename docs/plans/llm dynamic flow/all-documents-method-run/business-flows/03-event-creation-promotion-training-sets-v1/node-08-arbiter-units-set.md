# Node-08 - Arbiter Units Set

## Pair 8.1 - Validation arbiter: past date promoted

A: Arbiter: validation arbiter. Result: event date is in the past and event was published.

B: Flag it. The document says event date in the past is rejected, and stale drafts are archived without promotion.
fbx: Reprocess event validation. Reject past event date and stop promotion.

Pair 8.1 - Phrasing B

A: Past event was allowed to publish.

B: Flag. Send event date validation back for correction.

## Pair 8.2 - Matching integrity arbiter: wrong formula

A: Arbiter: matching integrity arbiter. Result: location weight is 0.50 and interest weight is 0.10.

B: Flag it. The document defines interest 0.35, location 0.25, time 0.20, price 0.10, social 0.10.
fbx: Reprocess event matching score with the documented weights.

Pair 8.2 - Phrasing B

A: Matching formula uses wrong weights.

B: Flag. Correct the audience score formula.

## Pair 8.3 - Feed placement arbiter

A: Arbiter: feed placement arbiter. Result: score 0.82 placed below fold.

B: Flag it. Score above 0.8 belongs in the top three feed positions.
fbx: Reprocess feed placement for this event and place it in top three positions.

Pair 8.3 - Phrasing B

A: Strong match is hidden low in the feed.

B: Flag. Move it to the documented high-score placement.

## Pair 8.4 - Notification rules arbiter

A: Arbiter: notification rules arbiter. Result: SMS sent for non-urgent event score 0.55.

B: Flag it. SMS is for urgent events only, score above 0.8, within 50km.
fbx: Reprocess SMS decision and remove SMS for this user.

Pair 8.4 - Phrasing B

A: SMS was sent without urgency and strong score.

B: Flag. Apply the SMS rule correctly.

## Pair 8.5 - Capacity arbiter

A: Arbiter: capacity arbiter. Result: capacity went from one remaining to negative one after two registrations.

B: Flag it. The document requires atomic counter behavior to prevent overselling.
fbx: Reprocess capacity registration with atomic decrement and waitlist loser.

Pair 8.5 - Phrasing B

A: Event oversold final seat.

B: Flag. Protect capacity and waitlist the losing registration.

## Pair 8.6 - Abuse arbiter

A: Arbiter: abuse arbiter. Result: organizer can directly message attendees outside platform channels.

B: Flag it. The document says organizers cannot directly message attendees outside platform channels.
fbx: Reprocess notification access. Force communication through platform-managed channels.

Pair 8.6 - Phrasing B

A: Organizer bypasses platform notification rules.

B: Flag. Route attendee communication through platform channels only.

## Pair 8.7 - Status arbiter

A: Arbiter: status arbiter. Result: organizer suspended but event remains promoted.

B: Flag it. Organizer suspension mid-promotion cancels pending notifications and marks event suspended.
fbx: Reprocess suspended-organizer handling. Stop promotion and mark event suspended.

Pair 8.7 - Phrasing B

A: Suspended organizer's event continues promotion.

B: Flag. Suspend event and stop campaign.

## Pair 8.8 - Pass case

A: Arbiters: matching, feed, notification, capacity. Result: score uses documented weights, score 0.805 is top three, urgent nearby user gets in-app/push/SMS, final seat is assigned once and second user waitlisted.

B: Pass.

Pair 8.8 - Phrasing B

A: Reviewed promotion result matches rules.

B: Pass.
