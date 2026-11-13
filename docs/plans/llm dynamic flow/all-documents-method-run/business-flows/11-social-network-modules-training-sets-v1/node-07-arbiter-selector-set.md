# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full collection of formal results R1...Rn plus flow context.
Runtime B is the selected review units.

## Pair 7.1 - Identity and profile

A: Formal results show account, session, profile fields, privacy settings, account recovery, deletion, or suspicious login.

B: Select identity-security arbiter, profile-privacy arbiter, recovery arbiter, and account-deletion arbiter.

Pair 7.1 - Phrasing B

A: Formal results include login, profile, or account controls.

B: Select authentication review, profile visibility review, and account lifecycle review.

## Pair 7.2 - Social graph

A: Formal results show friend, follow, block, muted, restricted, list, suggestion, or people-you-may-know result.

B: Select graph-edge arbiter, block-enforcement arbiter, suggestion-quality arbiter, and relationship-consistency arbiter.

Pair 7.2 - Phrasing B

A: Formal results include social relationship state.

B: Select graph-state review and relationship-impact review.

## Pair 7.3 - Content and media

A: Formal results show media upload, transcoding, post publish, draft, edit, scheduled post, or media placeholder.

B: Select media-readiness arbiter, audience-rule arbiter, publish-state arbiter, and CDN-delivery arbiter.

Pair 7.3 - Phrasing B

A: Formal results include user-generated content.

B: Select content-state review and media-safety review.

## Pair 7.4 - Feed and ads

A: Formal results show candidate generation, filters, ranking, diversity, ads, cursors, or impressions.

B: Select privacy-filter arbiter, block-filter arbiter, policy-removal arbiter, ranking arbiter, diversity arbiter, ad-spacing arbiter, and impression arbiter.

Pair 7.4 - Phrasing B

A: Formal results include home feed output.

B: Select feed-quality review and feed-safety review.

## Pair 7.5 - Engagement

A: Formal results show reaction, comment, mention, share, save, counter, notification, or ranking update.

B: Select idempotent-engagement arbiter, counter arbiter, mention-privacy arbiter, notification-rate arbiter, and ranking-signal arbiter.

Pair 7.5 - Phrasing B

A: Formal results include social interaction.

B: Select engagement-consistency review and notification review.

## Pair 7.6 - Messaging

A: Formal results show messages, attachments, fanout, read receipts, typing, presence, message request, or spam controls.

B: Select message-order arbiter, participant-fanout arbiter, attachment-safety arbiter, trust-gating arbiter, and spam arbiter.

Pair 7.6 - Phrasing B

A: Formal results include chat behavior.

B: Select messaging-delivery review and unknown-sender review.

## Pair 7.7 - Search

A: Formal results show people, post, company, group, job, hashtag, or typeahead search results.

B: Select permissions-filter arbiter, blocked-result arbiter, social-proximity arbiter, relevance arbiter, and typeahead arbiter.

Pair 7.7 - Phrasing B

A: Formal results include search results.

B: Select social-search review and visibility review.

## Pair 7.8 - Moderation and platform safety

A: Formal results show report case, classifier, hidden content, downrank, human review, enforcement, audit, appeal, or fraud dashboard.

B: Select moderation-action arbiter, human-review arbiter, enforcement-consistency arbiter, audit-log arbiter, appeal arbiter, and anomaly arbiter.

Pair 7.8 - Phrasing B

A: Formal results include trust and safety action.

B: Select safety review, audit review, and appeal review.

