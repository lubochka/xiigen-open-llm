# Node-02 - Domain Translator Set

## Pair 2.1 - Submission and validation

A: Fragment: "Organizer creates an event with title, date, location, pricing, and capacity." Domain: event submission and validation.

B: Accept an event creation request only from an authenticated user with permission to create events. Validate title, description, date, location, pricing, and capacity. Reject past dates. If validation passes, the event can move to published status.

Pair 2.1 - Phrasing B

A: Fragment: "Publish a new event from the event form." Domain: event submission and validation.

B: Treat the form as an organizer submission. Check required fields and permission before the event becomes publishable.

## Pair 2.2 - Storage and status

A: Fragment: "Store the event and manage its status." Domain: event storage and status.

B: Persist the event record with host identity, dates, capacity, pricing, and status. The status machine allows draft, published, promoted, active, completed, archived, cancelled, and suspended states. Publish EventCreated only after the event record exists.

Pair 2.2 - Phrasing B

A: Fragment: "Create the event record and publish the creation event." Domain: event storage and status.

B: Write the event first, then announce creation. Do not promote an event that was not stored.

## Pair 2.3 - Search indexing

A: Fragment: "Index the event for discovery." Domain: search indexing.

B: Send the event to the search index with searchable fields such as title, description, category, tags, date, and location. Publish EventIndexed after indexing succeeds.

Pair 2.3 - Phrasing B

A: Fragment: "Make the event searchable." Domain: search indexing.

B: Create a searchable index entry for the event and announce that the index is ready.

## Pair 2.4 - Predictive analytics

A: Fragment: "Predict attendance and virality." Domain: predictive analytics.

B: Analyze the event to estimate expected attendance, virality score, and category trends. The analytics result feeds event service and recommendation logic.

Pair 2.4 - Phrasing B

A: Fragment: "Estimate campaign performance." Domain: predictive analytics.

B: Produce predictions that help judge how the event is likely to perform before promotion metrics arrive.

## Pair 2.5 - Audience matching

A: Fragment: "Score users for event relevance." Domain: audience matching.

B: Calculate match score with interest relevance 0.35, location proximity 0.25, time availability 0.20, price sensitivity 0.10, and social connections 0.10. Segment users into strong match at at least 0.75, medium at at least 0.50, and weak at at least 0.25.

Pair 2.5 - Phrasing B

A: Fragment: "Find the target audience." Domain: audience matching.

B: Use the documented five-factor score to rank users and produce audience tiers with reasons.

## Pair 2.6 - Feed placement

A: Fragment: "Place the event in users' feeds based on score." Domain: feed placement.

B: Inject the event into matched users' event feeds. Score above 0.8 goes to top three positions, above 0.6 goes to top ten, above 0.4 appears within view, and below 0.4 appears below the fold.

Pair 2.6 - Phrasing B

A: Fragment: "Rank the event in the feed." Domain: feed placement.

B: Convert match score into feed position using the documented placement rules.

## Pair 2.7 - Notifications

A: Fragment: "Send channel-appropriate notifications." Domain: notification delivery.

B: Send in-app notification to every matched user. Send push when enabled and score is above 0.6. Send email digest for score above 0.4. Send SMS only for urgent events with score above 0.8 and within 50 kilometers.

Pair 2.7 - Phrasing B

A: Fragment: "Notify matched users by tier." Domain: notification delivery.

B: Choose notification channel from match strength, urgency, distance, and user channel settings.

## Pair 2.8 - Capacity and pricing

A: Fragment: "Avoid overselling and handle price changes." Domain: payment, pricing, and capacity.

B: Use atomic capacity updates so two simultaneous registrations cannot take the same final seat. Paid events may use price tiers. Price changes after publication require re-notification to registered attendees. Event cancellation requires full refund within forty-eight hours.

Pair 2.8 - Phrasing B

A: Fragment: "Paid event has limited seats and changing price." Domain: payment, pricing, and capacity.

B: Protect seat count, pricing communication, and refund obligations.

## Pair 2.9 - Abuse prevention

A: Fragment: "Prevent event spam and fake events." Domain: security and abuse prevention.

B: Rate limit event creation to five events per day per user. Apply content moderation for event descriptions. Use approval workflow for paid events and organizer reputation scoring. Organizers cannot directly message attendees outside platform channels.

Pair 2.9 - Phrasing B

A: Fragment: "Keep event creation safe." Domain: security and abuse prevention.

B: Control spam, fake events, notification abuse, price manipulation, and capacity overflow.

## Pair 2.10 - Metrics

A: Fragment: "Report promotion campaign performance." Domain: promotion metrics.

B: Aggregate reach, impressions, click-through, conversions, return on investment, notification delivery status, event fill rate, and revenue per event.

Pair 2.10 - Phrasing B

A: Fragment: "Show organizer event campaign analytics." Domain: promotion metrics.

B: Summarize how many users were reached, how they engaged, and whether promotion converted.
