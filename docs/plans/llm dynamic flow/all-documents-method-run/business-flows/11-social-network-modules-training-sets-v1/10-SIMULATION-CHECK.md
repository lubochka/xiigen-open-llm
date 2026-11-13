# 10 - Simulation Check

## Source Being Simulated

`C:\Users\lubao\Documents\xiigen\business flows\11-social network modules.md`

The simulation checks whether these node sets can recreate the social-network module map and main flows without using the source document as runtime input.

## Test Prompt 1 - Full Social Network

Runtime user prompt:

"Plan a Facebook or LinkedIn style social network with signup, rich profiles, friend/follow graph, posting with media, reactions, comments, feed, messaging, notifications, search, groups, jobs or marketplace, ads, moderation, recommendations, analytics, admin tools, and infrastructure."

Expected behavior:

- Node-01 returns all major social product and platform domains.
- Node-02 writes exact domain statements for each label.
- Node-03 plans identity/profile, graph/privacy, content/media, engagement, feed, messaging, search, moderation, communities, monetization, recommendations, analytics, support, and infrastructure.
- Node-04 processes module and flow results.
- Node-05 applies concrete social-network behavior.
- Node-07 selects identity, graph, content, feed, engagement, messaging, search, and moderation reviews.
- Node-08 rejects private profile leaks, block failures, media readiness failures, privacy-filter failures, duplicate reactions, missing message request gate, search permission leaks, and moderation without audit.
- Node-09 returns one social-network architecture answer.

Pass condition:

The result must connect graph, privacy, feed, messaging, search, and moderation. Isolated modules are not enough.

## Test Prompt 2 - Publish Post With Media

Runtime user prompt:

"A user uploads a video, creates a friends-only post, and publishes it when the media is ready."

Expected behavior:

- Node-01 returns content/media, feed, notifications, privacy.
- Node-03 includes upload, processing, post creation, media readiness, audience rule, feed eligibility.
- Node-08 rejects publishing final video before transcoding and rejects public visibility for friends-only content.
- Node-09 explains safe post publication.

Pass condition:

Media readiness and audience rules must both be present.

## Test Prompt 3 - Home Feed

Runtime user prompt:

"Build the home feed request pipeline with candidates, privacy filters, ranking, ads, diversity, cursors, and impressions."

Expected behavior:

- Node-01 returns feed, privacy, recommendations, ads, analytics.
- Node-03 plans candidate generation, filtering, ranking, ad merge, serving, and measurement.
- Node-08 rejects blocked content and consecutive ad overload.
- Node-09 explains the feed pipeline.

Pass condition:

Filtering must happen before final feed serving.

## Test Prompt 4 - Messaging Unknown Sender

Runtime user prompt:

"An unconnected user sends a message with an attachment."

Expected behavior:

- Node-01 returns messaging, media, trust gating.
- Node-03 routes through message request gate and media safety.
- Node-08 rejects direct inbox delivery and attachment bypass.
- Node-09 explains trust-gated messaging.

Pass condition:

Unknown sender cannot enter normal inbox without gate.

## Test Prompt 5 - Report Content

Runtime user prompt:

"A user reports a post for phishing."

Expected behavior:

- Node-01 returns trust and safety, admin support, analytics.
- Node-03 creates case, classifies, hides/downranks or reviews, enforces, audits, and supports appeal.
- Node-08 rejects unsafe content remaining fully ranked and rejects enforcement with no audit.
- Node-09 explains moderation.

Pass condition:

The moderation result must include action, audit, and appeal path.

## Missing-Training Detector

If onboarding stops at account creation, add Node-03 onboarding pairs.

If graph changes do not affect feed/search/messaging/privacy, add Node-04 and Node-05 graph pairs.

If feed ignores privacy or blocks, add Node-03, Node-07, and Node-08 feed filter pairs.

If engagement duplicates reactions, add Node-05 and Node-08 idempotency pairs.

If messaging lacks trust gating, add Node-03 and Node-08 messaging pairs.

If search returns private or blocked content, add Node-07 and Node-08 search pairs.

If moderation has enforcement but no audit or appeal, add Node-08 moderation pairs.

