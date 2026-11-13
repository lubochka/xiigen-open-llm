# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility plus the formal result or result group to examine.
Runtime B is pass, concern, or a reprocess instruction with the exact fragment to send back.

## Pair 8.1 - Privacy enforcement

A: Review responsibility: privacy-enforcement arbiter. Formal result: a private post appears in recipients' feeds outside the author.

B: Reprocess. The source rule says private posts must never appear in other users' feeds regardless of matching score. Send the feed-placement fragment back and require recipient eligibility to be recalculated with private visibility enforced.

Pair 8.1 - Phrasing B

A: Review responsibility: allowed-recipient review. Formal result: a connections-only post includes business-matched public recipients.

B: Reprocess. Connections-only visibility cannot use public business matching. Send the audience-matching fragment back and require business recipients to be removed unless a separate visibility rule allows them.

## Pair 8.2 - Content safety

A: Review responsibility: content-safety arbiter. Formal result: harmful content was flagged but feed distribution still proceeded as normal.

B: Reprocess. Content moderation cannot be a passive note when harmful content is flagged. Send the content-analysis and feed-release fragments back so the unsafe post is stopped, limited, or reviewed before distribution.

Pair 8.2 - Phrasing B

A: Review responsibility: moderation review. Formal result: profanity or spam detection appears, but no distribution gate is shown.

B: Concern. The result contains safety signals without a visible release decision. Require the flow to show whether the post passed moderation, was blocked, or was sent to review before formatting.

## Pair 8.3 - Ranking weights

A: Review responsibility: ranking-weight arbiter. Formal result: composite score uses match and engagement only, omitting friend, group, activity, and recency scores.

B: Reprocess. The source defines six ranking factors: match, friend, group, activity, recency, and engagement. Send the ranking fragment back and require all applicable factors to appear with the defined weights.

Pair 8.3 - Phrasing B

A: Review responsibility: score-factor-completeness review. Formal result: score factors do not include the group score.

B: Reprocess. Group score is a required fifteen percent factor when group information is available. Send the scoring fragment back and require the group component to be included or explicitly marked unavailable.

## Pair 8.4 - Join point completeness

A: Review responsibility: join-point arbiter. Formal result: ranking starts before business matches, friend connections, and group connections have each completed or timed out.

B: Reprocess. Ranking must wait for all three audience paths to complete or reach their fallback state. Send the ranking-start fragment back and require the missing branch outcomes to be represented.

Pair 8.4 - Phrasing B

A: Review responsibility: audience-branch review. Formal result: business and friend lists exist, but group matching is neither present nor marked unavailable.

B: Concern. The group branch is missing from the join point. The result must either include group recipients or state why the group branch is not available for this run.

## Pair 8.5 - Feed diversity

A: Review responsibility: author-diversity arbiter. Formal result: one author's posts occupy five of the top ten feed positions after reordering.

B: Reprocess. The source rule limits one author to two posts in the top ten. Send the reorder fragment back and require author diversity to be enforced.

Pair 8.5 - Phrasing B

A: Review responsibility: topic-diversity review. Formal result: top ten feed positions are mostly one topic and do not reach sixty percent topic diversity.

B: Reprocess. The source requires at least sixty percent topic diversity in the top ten. Send the feed-order fragment back and require the top positions to be balanced.

## Pair 8.6 - High-volume batching

A: Review responsibility: throughput arbiter. Formal result: a high-follower user's post writes all fifty thousand feed entries in one uncontrolled operation.

B: Reprocess. The source calls for progressive batching, close connections first, and rate limiting. Send the feed-update fragment back and require controlled batches and backlog tracking.

Pair 8.6 - Phrasing B

A: Review responsibility: partial-completion review. Formal result: a large distribution stops halfway with no resumable progress marker.

B: Concern. High-volume distribution must remain recoverable. Require the result to record batch progress, completed recipient groups, and remaining backlog.

## Pair 8.7 - Edit and delete behavior

A: Review responsibility: duplicate-injection arbiter. Formal result: edited post is reanalyzed and inserted again into feeds that already contained it.

B: Reprocess. The source says an edited post should update existing feed entries in place and should not be re-injected for users who already saw it. Send the edit-handling fragment back.

Pair 8.7 - Phrasing B

A: Review responsibility: deleted-post cleanup review. Formal result: deleted post remains in feeds with no removal action.

B: Reprocess. Deleted posts must be removed from all feeds immediately, with only a short cache delay allowed. Send the deletion fragment back and require batch removal to be shown.

## Pair 8.8 - Analytics completion

A: Review responsibility: completion-validity arbiter. Formal result: distribution is marked completed, but reach, impressions, distribution time, and tier breakdown are absent.

B: Reprocess. Completion depends on distribution metrics. Send the analytics fragment back and require reach, impressions, timing, and tier breakdown to be recorded before completion is claimed.

Pair 8.8 - Phrasing B

A: Review responsibility: analytics-evidence review. Formal result: feed updates completed, but no engagement or scroll-depth measurement is recorded.

B: Concern. The source lists engagement, time to first engagement, scroll depth, content type breakdown, and author diversity as key metrics. Require the analytics result to show the missing measurement plan or state why it is unavailable.

