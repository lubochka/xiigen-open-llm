# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full collection of formal results R1...Rn plus flow context.
Runtime B is the list of review units that must fire.

## Pair 7.1 - Capacity and reservation

A: Formal results show capacity check, reservation hold, expiry, waitlist, or capacity counter update.

B: Select capacity-lock arbiter, reservation-expiry arbiter, waitlist arbiter, and capacity-drift arbiter.

Pair 7.1 - Phrasing B

A: Formal results include available spots, reserved spots, sold tickets, or full-event handling.

B: Select oversell-prevention review, hold-timeout review, and counter-consistency review.

## Pair 7.2 - Payment

A: Formal results show paid event checkout, payment success, payment decline, timeout, retry, webhook, or refund.

B: Select payment-security arbiter, idempotency arbiter, reservation-release arbiter, double-charge arbiter, and refund-approval arbiter.

Pair 7.2 - Phrasing B

A: Formal results include money movement or paid-registration state.

B: Select checkout-state review, no-card-data review, duplicate-payment review, and payment-failure review.

## Pair 7.3 - Ticket

A: Formal results show ticket issuance, QR code, ticket delivery, ticket scan, ticket transfer, or ticket reuse.

B: Select ticket-uniqueness arbiter, qr-encryption arbiter, single-scan arbiter, transfer-verification arbiter, and delivery arbiter.

Pair 7.3 - Phrasing B

A: Formal results include secure ticket material.

B: Select QR-token review, ticket-state review, and scan-validity review.

## Pair 7.4 - Calendar and reminders

A: Formal results show calendar entry, local-time scheduling, reminder sequence, missed reminder, fallback delivery, quiet hours, or notification fatigue.

B: Select calendar-consent arbiter, timezone arbiter, reminder-schedule arbiter, missed-reminder arbiter, fallback-channel arbiter, and fatigue arbiter.

Pair 7.4 - Phrasing B

A: Formal results include preparation notifications before the event.

B: Select reminder-timing review, preference review, and recovery-delivery review.

## Pair 7.5 - Participant privacy

A: Formal results show participant list, anonymous attendance, organizer visibility, calendar data, location, or connection scores.

B: Select anonymous-attendance arbiter, participant-visibility arbiter, calendar-privacy arbiter, connection-score-privacy arbiter, and erasure-right arbiter.

Pair 7.5 - Phrasing B

A: Formal results include attendee identity or social discovery data.

B: Select attendee-privacy review, score-exposure review, and location-consent review.

## Pair 7.6 - Participant scoring

A: Formal results show event history, questionnaire, group, audience score components, composite score, or strength class.

B: Select scoring-component arbiter, equal-weight arbiter, raw-answer arbiter, and strength-classification arbiter.

Pair 7.6 - Phrasing B

A: Formal results include co-attendee relevance scores.

B: Select participant-score review, privacy-of-score-inputs review, and score-class review.

## Pair 7.7 - Feed evolution

A: Formal results show participant posts integrated, weight multipliers, event-day pinning, post-event decay, feed cap, or spacing rules.

B: Select feed-cap arbiter, same-participant-spacing arbiter, time-multiplier arbiter, event-day-priority arbiter, decay arbiter, and permanent-bonus arbiter.

Pair 7.7 - Phrasing B

A: Formal results include event-related feed positioning.

B: Select feed-balance review, time-evolution review, and post-event-decay review.

## Pair 7.8 - Analytics and operations

A: Formal results show conversion, revenue, engagement, social impact, event ROI, payment failure rates, capacity drift, reminder failure, or feed latency.

B: Select analytics-coverage arbiter, revenue-metrics arbiter, social-impact arbiter, performance-threshold arbiter, and dead-letter-retry arbiter.

Pair 7.8 - Phrasing B

A: Formal results include business metrics or system monitoring.

B: Select metrics-completeness review, alert-threshold review, and reliability review.

