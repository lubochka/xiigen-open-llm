# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label.
Runtime B is the same request in the language of that domain.

## Pair 2.1 - Identity and profile

A: Fragment: "Register, complete profile basics, get suggested follows or groups, and see an initial feed." Domain: identity and profile onboarding.

B: The onboarding domain creates an account and session, collects profile basics such as headline, work history, interests, or life events, seeds graph or topic suggestions, and provides cold-start feed content from popular and interest-based sources.

Pair 2.1 - Phrasing B

A: Fragment: "Signup creates usable feed." Domain: onboarding.

B: A new social account becomes useful only when identity, profile, recommendations, graph suggestions, and feed seeding are connected.

## Pair 2.2 - Social graph

A: Fragment: "Friend, follow, block, muted, and restricted relationships." Domain: social graph.

B: The graph domain stores relationship edges with types, state, and trust meaning, then powers feed audience selection, suggestions, search proximity, messaging trust gates, and privacy filtering.

Pair 2.2 - Phrasing B

A: Fragment: "Support friend requests, follows, blocks, lists, and people-you-may-know." Domain: social graph.

B: The social graph is both a relationship store and a source of candidate generation, ranking features, and access decisions.

## Pair 2.3 - Content and media

A: Fragment: "Upload media, transcode it, create a post, wait for media readiness, publish, and make it feed eligible." Domain: content creation and media pipeline.

B: The content domain creates posts with text, images, video, links, documents, stories, or reels. The media domain stores uploads, transcodes, creates thumbnails, and emits readiness so posts can publish with final media or placeholder behavior.

Pair 2.3 - Phrasing B

A: Fragment: "Posts with media must be processed before final display." Domain: content and media.

B: Post publication depends on media state, audience rules, and feed eligibility.

## Pair 2.4 - Feed

A: Fragment: "Generate candidates, filter privacy and blocks, rank, diversify, inject ads, and record impressions." Domain: feed.

B: The feed domain gathers candidates from graph edges, groups, pages, companies, interests, and recency windows; filters privacy, blocks, mutes, policy removals, and duplicates; ranks by predicted value, relationship strength, and freshness; diversifies content; inserts eligible ads with spacing; serves cursors; and records impressions.

Pair 2.4 - Phrasing B

A: Fragment: "Build the home feed money flow." Domain: feed.

B: Home feed is a pipeline: candidate generation, filtering, ranking, ad merge, serving, and analytics.

## Pair 2.5 - Engagement

A: Fragment: "Reactions, comments, replies, mentions, reposts, saves, and view tracking." Domain: engagement.

B: Engagement must write reactions and comments idempotently, update counters and social proof, emit events, notify authors and mentioned users, and update feed ranking signals.

Pair 2.5 - Phrasing B

A: Fragment: "Notify the author and update ranking signals when someone reacts." Domain: engagement.

B: Engagement is both user-visible interaction and feedback input for ranking and analytics.

## Pair 2.6 - Messaging

A: Fragment: "One-to-one and group chat with attachments, read receipts, typing, presence, fanout, push, and message requests." Domain: messaging and trust gating.

B: The messaging domain persists messages, assigns order, sends fanout to participants, delivers real-time updates, handles attachments through media, records presence and receipts, indexes chat search, and gates unconnected senders through message requests with spam controls.

Pair 2.6 - Phrasing B

A: Fragment: "Gate spam from unknown senders." Domain: messaging trust.

B: Trust gating prevents unconnected or suspicious senders from reaching inboxes as normal conversations.

## Pair 2.7 - Search

A: Fragment: "Search people, posts, companies, groups, jobs, hashtags, and topics with permissions filtering." Domain: search and discovery.

B: Search retrieves candidates from an index, understands query intent, applies visibility and block filters, ranks by text relevance, social proximity, recency, and type, and returns results plus typeahead suggestions.

Pair 2.7 - Phrasing B

A: Fragment: "Search must obey visibility, blocks, social distance, and recency." Domain: search.

B: Search is not only text matching; it must enforce access rules and social context.

## Pair 2.8 - Moderation

A: Fragment: "Report content, automated safety checks, hide or downrank, human review, enforcement, audit, appeal." Domain: trust and safety.

B: The moderation domain creates a case from reports, runs automated classifiers, applies immediate actions for clear violations, routes borderline cases to human review, enforces remove, warn, suspend, or restrict-reach actions, logs decisions, and supports appeals.

Pair 2.8 - Phrasing B

A: Fragment: "Moderation protects content creation and media at scale." Domain: trust and safety.

B: Safety must be active before harmful content spreads and auditable after action is taken.

## Pair 2.9 - Communities, jobs, marketplace, ads

A: Fragment: "Groups, pages, companies, events, jobs, marketplace, campaigns, targeting, auctions, and brand safety." Domain: communities and monetization.

B: Community and monetization domains provide group/page/company membership and roles, events, marketplace or jobs surfaces, ad inventory, campaign targeting, budget pacing, auction decisions, measurement, attribution, and brand safety controls.

Pair 2.9 - Phrasing B

A: Fragment: "Facebook emphasizes groups and marketplace; LinkedIn emphasizes profiles, companies, jobs, hiring." Domain: social product emphasis.

B: Module emphasis changes by social-network style even when shared infrastructure remains the same.

## Pair 2.10 - Infrastructure and analytics

A: Fragment: "Rate limiting, gateway, caching, queues, object storage, CDN, observability, fraud dashboards, experiments, and admin support." Domain: platform safety and scale.

B: The platform domain provides gateway and rate limits, caching for feeds and profiles, queues and streams for events, object storage and CDN for media, logs, traces, metrics, support consoles, moderation tools, legal holds, audit logs, experiments, and fraud or anomaly dashboards.

Pair 2.10 - Phrasing B

A: Fragment: "Keep social networking safe, scalable, observable, and supportable." Domain: infrastructure and analytics.

B: Social products need invisible systems for reliability, moderation, support, measurement, and experimentation.

