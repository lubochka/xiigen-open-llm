# Node-04 - Domain Processor Set

## Pair 4.1 - Process event validation

A: b1: Validate and store event. Context: organizer has permission; title, date, location, price, and capacity are present; date is future.

B: Processed result: event is valid for publication. Status may move to published after storage. EventCreated may be published only after the stored event exists.

Pair 4.1 - Phrasing B

A: Event form is complete and valid.

B: Accept the event for storage and publication.

## Pair 4.2 - Process past event date

A: b1: Validate event. Context: date is in the past.

B: Processed result: validation rejected. Do not publish, promote, index, match, or notify. If this is a stale draft, archive without promotion.

Pair 4.2 - Phrasing B

A: Event date already passed.

B: Reject creation or archive draft; no promotion path opens.

## Pair 4.3 - Process matching score

A: b4: Match target audience. Context: interest 0.9, location 0.8, time 0.7, price 0.6, social 0.5.

B: Processed result: score is 0.805. Calculation: 0.9*0.35 + 0.8*0.25 + 0.7*0.20 + 0.6*0.10 + 0.5*0.10. Tier is strong match because score is at least 0.75.

Pair 4.3 - Phrasing B

A: Candidate user has high interest and close location.

B: Score is 0.805 and user belongs to strong audience tier.

## Pair 4.4 - Process feed placement

A: b5: Place event in feeds. Context: match score 0.805.

B: Processed result: event belongs in top three feed positions because score is greater than 0.8.

Pair 4.4 - Phrasing B

A: Feed placement receives strong score above 0.8.

B: Put the event in top three positions.

## Pair 4.5 - Process notification channels

A: b6: Deliver notifications. Context: score 0.805, urgent event, user within 50km, push enabled.

B: Processed result: send in-app, push, and SMS. Email digest may also apply if campaign rules include digest, but immediate urgency is covered by push and SMS.

Pair 4.5 - Phrasing B

A: Strong nearby urgent match with push enabled.

B: Send in-app, push, and SMS.

## Pair 4.6 - Process capacity race

A: b2: Protect capacity. Context: one seat remains and two users register simultaneously.

B: Processed result: one registration succeeds through atomic decrement. The other registration does not get the seat and receives waitlist position.

Pair 4.6 - Phrasing B

A: Two users try to take the final seat.

B: Use atomic capacity protection; one gets the seat, one goes to waitlist.

## Pair 4.7 - Process notification overload

A: b6: Delivery queue. Context: more than ten thousand notifications.

B: Processed result: queue notifications by priority. Strong matches first within five minutes. Other matches within one hour. Do not drop lower-priority notifications.

Pair 4.7 - Phrasing B

A: Notification service is overwhelmed.

B: Queue and prioritize by match tier.

## Pair 4.8 - Process suspended organizer

A: b6: Continue promotion. Context: organizer account suspended mid-promotion.

B: Processed result: promotion must stop. Pending notifications cancelled. Event status becomes suspended. Registered attendees must be informed.

Pair 4.8 - Phrasing B

A: Organizer becomes suspended while campaign is running.

B: Stop campaign and mark event suspended.
