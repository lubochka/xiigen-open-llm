# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility and the formal result or result group to examine.
Runtime B is pass, concern, or reprocess instruction.

## Pair 8.1 - Capacity oversell

A: Review responsibility: capacity-lock arbiter. Formal result: two users both receive confirmed tickets for the last available spot.

B: Reprocess. The source requires atomic capacity control so the first lock wins and the second receives waitlist or full-event result. Send the capacity-update fragment back.

Pair 8.1 - Phrasing B

A: Review responsibility: capacity-drift review. Formal result: sold plus available does not equal total capacity.

B: Reprocess. Capacity counter drift is a critical failure. Send the capacity fragment back and require reconciliation before completion is claimed.

## Pair 8.2 - Payment safety

A: Review responsibility: double-charge arbiter. Formal result: duplicate payment webhook creates two tickets and two charges.

B: Reprocess. Payment callbacks require idempotency. Send the payment fragment back and require the second callback to return already processed.

Pair 8.2 - Phrasing B

A: Review responsibility: reservation-release review. Formal result: payment declines but the reservation remains held.

B: Reprocess. Declined payment must release the reservation and block ticket issuance. Send the payment-failure fragment back.

## Pair 8.3 - Ticket security

A: Review responsibility: qr-encryption arbiter. Formal result: ticket QR contains readable ticketId, userId, and eventId.

B: Reprocess. The source requires encrypted QR payload. Send the ticket fragment back and require encrypted single-use ticket content.

Pair 8.3 - Phrasing B

A: Review responsibility: single-scan review. Formal result: the same QR code validates twice.

B: Reprocess. Ticket must be marked used atomically on first scan. Send the ticket-scan fragment back.

## Pair 8.4 - Reminder correctness

A: Review responsibility: timezone arbiter. Formal result: reminders scheduled in server timezone instead of user timezone.

B: Reprocess. The source requires reminders calculated in the user's local timezone. Send the reminder fragment back.

Pair 8.4 - Phrasing B

A: Review responsibility: missed-reminder review. Formal result: reminder service downtime causes the T-1 hour reminder to disappear.

B: Reprocess. Missed reminders require catch-up every fifteen minutes and adjusted copy. Send the reminder-recovery fragment back.

## Pair 8.5 - Anonymous attendee privacy

A: Review responsibility: anonymous-attendance arbiter. Formal result: anonymous attendee appears in participant list for other attendees.

B: Reprocess. Anonymous attendance is visible to organizer only. Send the participant-identification fragment back and remove the attendee from attendee-facing discovery.

Pair 8.5 - Phrasing B

A: Review responsibility: connection-score-privacy review. Formal result: a user's connection score is displayed to another participant.

B: Reprocess. The source says connection scores are never exposed to other users. Send the social-feed fragment back and show only feed effects or permitted display wording.

## Pair 8.6 - Scoring formula

A: Review responsibility: equal-weight arbiter. Formal result: participant score uses event history at seventy percent and ignores questionnaire, group, and audience match.

B: Reprocess. The source defines four equal components at twenty-five percent each. Send the participant-scoring fragment back.

Pair 8.6 - Phrasing B

A: Review responsibility: raw-answer review. Formal result: questionnaire answers are exposed while calculating similarity.

B: Reprocess. Similarity should be used without exposing raw answers. Send the questionnaire-score fragment back.

## Pair 8.7 - Feed takeover

A: Review responsibility: feed-cap arbiter. Formal result: event participant content becomes seventy percent of the user's feed.

B: Reprocess. The source caps event participant content at forty percent of any user's feed. Send the feed-integration fragment back.

Pair 8.7 - Phrasing B

A: Review responsibility: same-participant-spacing review. Formal result: five posts from the same participant appear consecutively.

B: Reprocess. The source requires minimum three-post spacing between same-participant content. Send the feed-position fragment back.

## Pair 8.8 - Time evolution

A: Review responsibility: time-multiplier arbiter. Formal result: event-day multiplier remains at base weight.

B: Reprocess. Event day requires 3 times multiplier, top pinning, and real-time mode. Send the weight-evolution fragment back.

Pair 8.8 - Phrasing B

A: Review responsibility: decay review. Formal result: post-event feed boost never decays.

B: Reprocess. The source requires decay back to base by T+7 with permanent 0.05 bonus. Send the post-event-weight fragment back.

