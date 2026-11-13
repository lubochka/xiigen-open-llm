# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full collection of formal results R1...Rn plus the flow context.
Runtime B is the list of review units that must fire before formatting.

## Pair 7.1 - Visibility-sensitive result

A: Formal results show a post with visibility set to private or connections-only, plus feed placement results for recipients beyond the allowed audience.

B: Select privacy-enforcement arbiter, feed-placement arbiter, and recipient-eligibility arbiter.

Pair 7.1 - Phrasing B

A: Formal results include restricted visibility and a distribution result that may reach users outside that restriction.

B: Select privacy-enforcement review, allowed-recipient review, and feed-distribution review.

## Pair 7.2 - Harmful or spam-like content

A: Formal results show content moderation flags, spam risk, profanity risk, or sanitized post content before distribution.

B: Select content-safety arbiter, spam-rate arbiter, and sanitized-content arbiter.

Pair 7.2 - Phrasing B

A: Formal results include unsafe-content signals or repeated posting that may exceed the allowed rate.

B: Select moderation review, rate-limit review, and feed-release review.

## Pair 7.3 - Ranking formula result

A: Formal results show composite scores and feed tiers for recipients.

B: Select ranking-weight arbiter, tier-threshold arbiter, and duplicate-recipient arbiter.

Pair 7.3 - Phrasing B

A: Formal results include recipient scores built from match, friend, group, activity, recency, and engagement factors.

B: Select scoring-model review, score-factor-completeness review, and tier-assignment review.

## Pair 7.4 - Three matching branches

A: Formal results show business matches, friend connections, and group connections feeding a join point.

B: Select parallel-branch-completion arbiter, join-point arbiter, and missing-branch fallback arbiter.

Pair 7.4 - Phrasing B

A: Formal results depend on three independent audience paths that must be merged before ranking.

B: Select audience-branch review, ranking-wait review, and fallback-readiness review.

## Pair 7.5 - Feed diversity rules

A: Formal results show feed insertion and reordering for top feed positions.

B: Select author-diversity arbiter, topic-diversity arbiter, and reorder-consistency arbiter.

Pair 7.5 - Phrasing B

A: Formal results include top-ten feed positions and post ordering after insertion.

B: Select top-feed-quality review, author-limit review, and topic-mix review.

## Pair 7.6 - High-volume distribution

A: Formal results show an audience size above fifty thousand, progressive batching, feed update rate limits, and backlog metrics.

B: Select batching arbiter, throughput arbiter, retry-safety arbiter, and partial-completion arbiter.

Pair 7.6 - Phrasing B

A: Formal results show a popular-author post distributed in many batches.

B: Select high-volume distribution review, feed-backlog review, and resumable-progress review.

## Pair 7.7 - Edit or delete after distribution

A: Formal results show a post edited after feed placement or deleted after appearing in feeds.

B: Select duplicate-injection arbiter for edits, removal-completeness arbiter for deletes, and cache-delay arbiter when deleted content may remain briefly visible.

Pair 7.7 - Phrasing B

A: Formal results include a content change after distribution has already happened.

B: Select change-handling review, existing-feed-entry review, and deleted-post cleanup review.

## Pair 7.8 - Completion metrics

A: Formal results claim PostDistributionCompleted while analytics metrics are missing or incomplete.

B: Select completion-state arbiter, analytics-required-fields arbiter, and distribution-metrics arbiter.

Pair 7.8 - Phrasing B

A: Formal results say the flow is complete but do not show reach, impressions, timing, tier breakdown, or engagement evidence.

B: Select completion-validity review and analytics-evidence review.

