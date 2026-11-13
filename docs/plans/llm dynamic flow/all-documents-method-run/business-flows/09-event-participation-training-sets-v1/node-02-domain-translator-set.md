# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label.
Runtime B is the same request written in the language of that domain.

## Pair 2.1 - Availability and reservation

A: Fragment: "Check event capacity, reserve a temporary place, and release it if checkout expires." Domain: event availability and capacity hold.

B: The availability domain must check current capacity, create a five-minute reservation hold when a spot exists, prevent oversell through atomic capacity control, and release the hold if payment or registration does not complete in time.

Pair 2.1 - Phrasing B

A: Fragment: "Let a user register only if capacity is available and hold the spot for five minutes." Domain: event availability and capacity hold.

B: Event participation begins with a capacity decision: full events offer waitlist, available events create a temporary hold, and expired holds return the spot.

## Pair 2.2 - Payment

A: Fragment: "Process payment securely, handle declined cards, and avoid double charging." Domain: paid-event payment processing.

B: The payment domain creates a payment attempt for paid events, validates payment outcome, releases the reservation on decline or timeout, uses idempotency for repeated callbacks, and keeps card data outside the platform.

Pair 2.2 - Phrasing B

A: Fragment: "Take payment for the ticket and make repeated payment callbacks idempotent." Domain: paid-event payment processing.

B: Payment success is required before paid ticket issuance, while decline, timeout, and duplicate callback each have a defined safe outcome.

## Pair 2.3 - Ticketing

A: Fragment: "Issue a QR ticket after payment succeeds or free registration is confirmed." Domain: ticket issuance with QR code.

B: The ticketing domain creates one ticket with ticket identity, user, event, ticket number, type, seat when applicable, and encrypted QR payload, then sends it to the user through supported delivery channels.

Pair 2.3 - Phrasing B

A: Fragment: "Generate the event ticket and prevent duplicate scans." Domain: ticket security.

B: Ticket validation must treat the QR code as a single-use token and mark it used atomically on first scan.

## Pair 2.4 - Capacity update

A: Fragment: "Decrement available spots without overselling." Domain: atomic capacity update.

B: The capacity domain updates confirmed ticket counts and available capacity using atomic locking, refreshes the real-time capacity cache, and sends waitlist behavior when capacity is gone.

Pair 2.4 - Phrasing B

A: Fragment: "Two users try to buy the last ticket at the same time." Domain: capacity race handling.

B: The last spot must be awarded to the first successful locked update, while the losing attempt moves to waitlist or receives a full-event result.

## Pair 2.5 - Calendar and reminders

A: Fragment: "Add the event to the user's calendar and schedule reminders." Domain: calendar integration and local-time reminders.

B: The calendar domain creates a calendar entry with event title, time, location or link, and ticket reference. The reminder domain schedules week-before, day-before, hour-before, and final reminders in the user's local timezone, respecting quiet hours and notification preferences.

Pair 2.5 - Phrasing B

A: Fragment: "Use the attendee's timezone to schedule progressive event reminders." Domain: calendar integration and reminders.

B: Reminders are time-aware and progressive: they prepare the attendee before the event, intensify near start time, and fall back through other channels when delivery fails.

## Pair 2.6 - Participant scoring

A: Fragment: "Score attendee connections using event history, questionnaire similarity, group overlap, and audience match." Domain: attendee connection scoring.

B: The participant scoring domain compares each attendee pair using four equal components: shared event history, questionnaire similarity, group overlap, and audience or market match. The score is classified as strong above 0.7, medium from 0.4 to 0.7, and weak below 0.4.

Pair 2.6 - Phrasing B

A: Fragment: "For each co-attendee, calculate relevance using four equal matching components." Domain: attendee connection scoring.

B: Co-attendee relevance is not one generic match; it is built from four separate evidence types that must each remain visible.

## Pair 2.7 - Feed time evolution

A: Fragment: "Boost participant content at T-7 days, T-1 day, and on event day." Domain: time-evolving feed weight.

B: The feed-weight domain starts from base weight at registration, applies 1.5 times weight at seven days before, 2 times at one day before, 3 times on event day, and then decays after the event until returning to base plus a permanent co-attendee bonus.

Pair 2.7 - Phrasing B

A: Fragment: "Show co-attendee posts with increasing weight as the event approaches." Domain: time-evolving feed weight.

B: Participant content becomes more prominent as the event gets closer, pins to top on event day, and fades after the event.

## Pair 2.8 - Feed integration and diversity

A: Fragment: "Integrate co-attendee posts into feeds but keep participant content below forty percent and leave three-post spacing between same-participant content." Domain: participant content feed integration.

B: The feed integration domain inserts public participant posts according to time-adjusted weight and display format while enforcing a forty percent cap on event participant content and at least three posts between content from the same participant.

Pair 2.8 - Phrasing B

A: Fragment: "Show full cards for strong participant connections and compact views for weak ones." Domain: participant content feed integration.

B: Feed display changes by connection strength: strong connections can receive fuller participant cards, while weak connections receive compact or lower-priority presentation.

## Pair 2.9 - Privacy and security

A: Fragment: "Respect anonymous attendance, participant list visibility, calendar consent, private connection scores, secure payment, and secure QR tickets." Domain: event participation privacy and security.

B: The privacy and security domain prevents card data from touching platform servers, verifies payment callbacks, encrypts QR tickets, enforces single-scan validation, requires calendar consent, hides anonymous attendees from other participants, and never exposes connection scores directly to other users.

Pair 2.9 - Phrasing B

A: Fragment: "Allow anonymous attendance and do not expose connection scores." Domain: attendee privacy.

B: Participant discovery and social feed effects must obey attendee visibility settings and organizer participant-list configuration.

## Pair 2.10 - Analytics

A: Fragment: "Measure booking, payment, reminders, feed updates, social connections, revenue, and event ROI." Domain: event participation analytics.

B: The analytics domain records conversion, payment success, waitlist conversion, revenue, reminder delivery, participant post engagement, attendee connection requests, post-event retention, network effects, feed update speed, and event ROI.

Pair 2.10 - Phrasing B

A: Fragment: "Track the journey from booking through post-event relationship building." Domain: event participation analytics.

B: Measurement spans the full lifecycle: registration, payment, ticket, reminders, social discovery, event-day feed priority, post-event decay, and long-term engagement.

