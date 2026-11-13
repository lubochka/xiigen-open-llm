# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of domain translations from Node-02.
Runtime B is a plan with social modules, flows, wait states, recovery paths, and review points.

## Pair 3.1 - Full social network module map

A: The translated domains say the platform needs identity, profile, graph, content and media, engagement, feed, messaging, notifications, search, communities, marketplace or jobs, ads, privacy, moderation, recommendations, analytics, admin support, and infrastructure.

B: Plan: define identity and profile foundations. Define graph relationships and privacy controls. Define content creation and media pipeline. Define engagement events and notification behavior. Define feed candidate generation, filtering, ranking, ads, serving, and impressions. Define messaging with trust gating. Define search with permissions filtering. Define communities, jobs, marketplace, and ads based on product emphasis. Define trust and safety, recommendations, analytics, support, and infrastructure as platform layers. Review privacy, block handling, media readiness, feed filtering, ad spacing, idempotent engagement, message request gating, search permissions, moderation enforcement, and audit logging.

Pair 3.1 - Phrasing B

A: The translated descriptions describe a Facebook/LinkedIn-class social product.

B: Plan: build the visible social features and the invisible platform safety/scaling systems together, because feed, search, messaging, moderation, and recommendations all depend on graph and privacy.

## Pair 3.2 - Onboarding

A: The translated domains say signup creates account and session, profile prompts collect basics, graph suggests connections, recommendations seed follows or groups, and feed shows starter content.

B: Plan: create account and session. Collect profile basics. Generate connection, topic, group, or follow suggestions. Seed cold-start feed with popular and interest-based content. Review identity, profile visibility, suggestion quality, and cold-start feed diversity.

Pair 3.2 - Phrasing B

A: The translated descriptions say onboarding ends with a usable feed.

B: Plan: account alone is not enough; profile, suggestions, and starter feed complete onboarding.

## Pair 3.3 - Publish post with media

A: The translated domains say media upload goes to storage, media is transcoded, post references media, media readiness controls publishability, feed eligibility starts after publish, and notifications may go to close connections or followers.

B: Plan: upload media through controlled upload path. Process media into usable variants. Create post with text and media references. If media is not ready, use placeholder or wait according to rule. Publish the post when allowed. Emit publication event. Make it feed eligible for permitted audience and notify only where rules allow. Review media readiness, audience rules, and notification limits.

Pair 3.3 - Phrasing B

A: The translated descriptions say content and media are linked but not identical.

B: Plan: the post can only display correctly when media processing and privacy rules are resolved.

## Pair 3.4 - Home feed

A: The translated domains say home feed request generates candidates from graph, groups, pages, interests and recency, filters privacy and blocks, ranks by predicted value, merges ads, serves cursors, and records impressions.

B: Plan: collect candidates. Remove blocked, muted, private, duplicate, or policy-removed content. Rank by predicted value, relationship strength, freshness, and product rules. Add eligible ads through spacing constraints. Diversify the feed. Return items and cursors. Record impressions for analytics and ad measurement. Review filter order, privacy, ad spacing, ranking explainability, and analytics.

Pair 3.4 - Phrasing B

A: The translated descriptions say the feed is a pipeline, not a raw list.

B: Plan: candidate generation, filtering, ranking, ad merge, serving, and measurement must all be present.

## Pair 3.5 - Engagement

A: The translated domains say reaction or comment writes idempotently, updates counters, emits event, notifies author and mentions, and updates feed ranking signals.

B: Plan: accept reaction or comment. Make repeated same reaction idempotent. Update counters and social proof. Emit engagement event. Notify author and mentioned users according to preferences. Feed ranking consumes the signal and may resurface the post. Review idempotency, notification spam, mention privacy, and ranking signal update.

Pair 3.5 - Phrasing B

A: The translated descriptions say engagement changes both the conversation and the feed.

B: Plan: engagement is written once, counted correctly, messaged carefully, and used as ranking evidence.

## Pair 3.6 - Messaging

A: The translated domains say sender posts message, persistence assigns sequence, fanout sends to participants, attachments use media, real-time push delivers, and unconnected senders go through message request gate.

B: Plan: validate sender and conversation. If unconnected, route through message request gate. Persist message and sequence. Process attachments through media pipeline. Fanout to participants through real-time and push channels. Record read receipts, typing, and presence where enabled. Review spam, unknown sender gating, attachment safety, delivery, and chat search indexing.

Pair 3.6 - Phrasing B

A: The translated descriptions say chat must be real-time and safe.

B: Plan: message storage, participant fanout, media, presence, receipts, and trust gates work together.

## Pair 3.7 - Search

A: The translated domains say search retrieves people, posts, pages, companies, groups, jobs, hashtags, and topics; applies permissions; ranks by text, proximity, and recency; and returns typeahead suggestions.

B: Plan: understand query. Retrieve candidates from search index. Apply privacy, block, and visibility filters. Rank by relevance, social proximity, recency, and result type. Return results and typeahead suggestions. Review permission filtering and blocked-user exclusion.

Pair 3.7 - Phrasing B

A: The translated descriptions say search must be social-aware.

B: Plan: search results are not valid until visibility and social context have been applied.

## Pair 3.8 - Report and moderation

A: The translated domains say user reports content, trust and safety creates case, automated classifiers may hide or downrank, human review handles borderline cases, enforcement removes/warns/suspends/restricts, audit log and appeal flow follow.

B: Plan: create moderation case from report. Run automated classification. If violation is clear, apply immediate action. If borderline, route to human review. Apply enforcement action. Record audit trail. Allow appeal. Review classifier action, human review handoff, enforcement consistency, audit completeness, and appeal availability.

Pair 3.8 - Phrasing B

A: The translated descriptions say moderation is both automated and reviewable.

B: Plan: harmful content can be acted on quickly, but decisions remain traceable and appealable.

