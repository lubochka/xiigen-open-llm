# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of domain translations from Node-02.
Runtime B is a plan with ordered work, parallel work, wait states, recovery paths, and review points.

## Pair 3.1 - Full event participation plan

A: The translated domains say that the user wants to participate in an event with capacity hold, payment when required, ticket issuance, atomic capacity update, calendar entry, progressive reminders, participant identification, co-attendee scoring, feed integration, time-based feed boosts, post-event decay, security, and analytics.

B: Plan: first verify the user and event. Check capacity and either offer waitlist or create a five-minute reservation hold. If the event is paid, process payment during the hold; if the event is free, skip payment. For paid events, release the hold on decline or timeout and prevent duplicate payment handling. After payment success or free registration confirmation, issue one secure ticket. Update capacity atomically after ticket issuance. Add the event to the user's calendar with consent and schedule local-time reminders. Identify other participants according to visibility rules. For each participant pair, run four equal scoring branches: event history, questionnaire similarity, group overlap, and audience match. Combine scores, integrate participant posts into feeds with diversity caps, increase feed weight as the event approaches, pin content on event day, decay after the event, apply permanent co-attendee bonus, and record analytics. Review payment security, double charging, capacity drift, ticket QR security, reminder delivery, anonymous attendance, connection-score privacy, feed caps, and analytics completeness.

Pair 3.1 - Phrasing B

A: The translated descriptions require booking, payment, ticket, capacity, calendar, reminders, attendee matching, feed evolution, post-event behavior, and measurement.

B: Plan: the money-and-ticket path is strict and sequential. The social and reminder paths begin only after ticket issuance. Feed weight changes over time, not all at once. Review units run before final wording is formatted.

## Pair 3.2 - Free event

A: The translated domains say the event is free, payment is skipped, registration is confirmed, ticket is issued, capacity is updated, and reminders/social integration continue.

B: Plan: after capacity hold succeeds, bypass payment. Confirm registration directly. Issue the ticket and update capacity. Continue calendar, reminders, participant identification, scoring, feed integration, and analytics. Review that no payment attempt or payment failure branch appears for the free event.

Pair 3.2 - Phrasing B

A: The translated descriptions say no money is needed but all non-payment participation steps still apply.

B: Plan: free registration still needs capacity, ticketing, calendar, reminders, participant scoring, feed integration, privacy, and analytics.

## Pair 3.3 - Payment declined

A: The translated domains say the paid event payment is declined. The reservation must be released, the user can retry up to three times with another method, and no ticket is issued.

B: Plan: keep the reservation during the payment attempt. On decline, release the capacity hold, show the payment error, allow retry within the source-defined limit, and block ticket issuance and capacity decrement until a successful payment exists. Review payment state and ticket gate.

Pair 3.3 - Phrasing B

A: The translated descriptions say a failed payment cannot become a ticket.

B: Plan: declined payment stops paid registration at checkout, frees the held spot, and prevents ticket and social integration from starting.

## Pair 3.4 - Capacity race

A: The translated domains say two users try to buy the last ticket at the same time. Row-level locking makes one winner, and the other gets waitlist.

B: Plan: process both capacity attempts through atomic lock. The first successful lock gets the reservation and may continue to payment. The second receives event full or waitlist path. Ticketing and capacity update must not oversell. Review capacity counter drift and waitlist result.

Pair 3.4 - Phrasing B

A: The translated descriptions describe a last-seat race.

B: Plan: one spot can only be reserved once. Losing attempts move to waitlist or full-event messaging.

## Pair 3.5 - Calendar and missed reminder

A: The translated domains say the event is added to the user's calendar, reminders are scheduled in local time, and if reminder service is down, a catch-up job sends missed reminders with adjusted copy.

B: Plan: after ticket issuance, add calendar entry when the user consents. Schedule reminders at T-7 days, T-1 day, T-1 hour, and T-15 minutes in the user's timezone. If the reminder window is missed due to downtime, catch up every fifteen minutes and send immediately with copy that acknowledges the timing. Use SMS then email fallback when push fails if enabled. Review reminder delivery and fatigue rules.

Pair 3.5 - Phrasing B

A: The translated descriptions say reminders are time-sensitive but must recover from downtime.

B: Plan: reminders are scheduled before the event, respect local time and preferences, and have catch-up behavior rather than vanishing.

## Pair 3.6 - Anonymous attendance

A: The translated domains say the user opts out of participant identification and attends anonymously, visible only to organizer.

B: Plan: allow registration, payment, ticket, capacity, calendar, and reminders. Exclude the attendee from participant discovery shown to other attendees. Do not calculate or expose co-attendee feed scores for that attendee except where organizer-only visibility allows. Review anonymity and participant-list configuration.

Pair 3.6 - Phrasing B

A: The translated descriptions say participation can be private while ticketing still works.

B: Plan: anonymous attendance limits social discovery and feed integration but does not block the operational participation path.

## Pair 3.7 - Event-day feed priority

A: The translated domains say event day applies a 3 times multiplier, participant content is pinned to the top, and real-time mode is active.

B: Plan: when event day arrives, send event-day reminder, raise participant content weight to 3 times, pin qualifying event content at the top, enable real-time feed updates, keep event participant content below forty percent, and keep three-post spacing between same-participant content. Review feed caps and real-time update latency.

Pair 3.7 - Phrasing B

A: The translated descriptions say co-attendee content becomes most important during the event without taking over the whole feed.

B: Plan: event-day relevance is high, but diversity limits still hold.

## Pair 3.8 - Post-event decay

A: The translated domains say after event completion the feed multiplier decays, reaches base by T+7, and applies a permanent 0.05 co-attendee bonus.

B: Plan: after EventCompleted, begin decay. From T+1 to T+3 use the elevated but decreasing post-event weight. By T+7 return to base weight and add the permanent co-attendee bonus. Track engagement, posts, connection requests, retention, and event ROI. Review decay timing and permanent bonus application.

Pair 3.8 - Phrasing B

A: The translated descriptions say event social priority should fade but not disappear completely.

B: Plan: event-driven feed strength gradually returns to normal while preserving a small lasting connection signal for attendees.

