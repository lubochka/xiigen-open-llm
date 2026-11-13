# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of domain translations from Node-02.
Runtime B is an ordered and parallel plan with work pieces, transformation instructions, wait points, recovery paths, and review points.

## Pair 3.1 - Full post distribution plan

A: The translated domains say that a post must be stored with visibility, analyzed for meaning, matched through business, social, and group paths, scored into one recipient list, placed into feed tiers, reordered for diversity, and measured by analytics.

B: Plan: first store the post with its author, content type, media, poll details if present, and visibility. After storage succeeds, announce that a post exists for processing. Run content analysis, social-connection lookup, group-membership lookup, and analytics start tracking. After content analysis completes, run business matching and send the analysis to ranking. Wait until business matches, friend connections, and group connections have either completed or reached their fallback result. Compile one recipient list. Calculate composite scores. Place the post into feed tiers in batches. Apply diversity controls and reorder existing feed content. Record distribution analytics. Review privacy, safety, ranking weights, join-point completeness, diversity, and failure handling before final wording is formatted.

Pair 3.1 - Phrasing B

A: The translated descriptions require publication, understanding the post, three audience searches, recipient scoring, feed insertion, reordering, and reporting.

B: Plan: create the post record first. Then start the downstream publication flow. Analyze the post. In parallel, collect social connections and group recipients; once analysis is ready, collect business matches and prepare ranking input. Ranking waits for all three audience sources or their defined fallback states. The ranked list drives feed placement, feed balancing, and analytics. Reviews check that private or unsafe content was not distributed, that the score model kept all required factors, and that fallback behavior was used when a branch failed.

## Pair 3.2 - Connections-only visibility

A: The translated domains say the post visibility is connections-only, business matching must be skipped, group rules apply only when allowed, and friend connections remain the primary audience.

B: Plan: store the post with connections-only visibility. Analyze content for moderation and later feed wording, but do not use business matching for distribution. Run the social-connection path. Run the group path only for groups where the visibility rule allows the post. Compile recipients from allowed social and group sources. Rank the allowed recipients without business-match contribution. Place into feeds only for permitted recipients. Review privacy enforcement before any formatting.

Pair 3.2 - Phrasing B

A: The translated descriptions say only connections may receive the post and the business audience path should not fire.

B: Plan: preserve the visibility restriction from the first step through final feed placement. Use content analysis for safety and classification. Gather direct and broader connection recipients. Include group members only when the source rule permits it. Ranking must not invent public business recipients. The privacy arbiter must inspect the formal results before the formatter receives the outcome.

## Pair 3.3 - Poll post boost

A: The translated domains say the post contains poll metadata, polls should receive higher engagement prediction, and the content must still pass the same publication and feed-placement path.

B: Plan: store the post and poll metadata together. Analyze the post and classify it as poll content. Run the same business, social, and group audience paths. When ranking recipients, add the engagement prediction signal associated with poll-type content while keeping the fixed score weights. Place and reorder feeds using the resulting scores. Report content-type engagement separately so poll performance can be measured.

Pair 3.3 - Phrasing B

A: The translated descriptions say a poll should travel through normal distribution but with a stronger expected-interaction signal.

B: Plan: treat the poll as a normal published post plus poll-specific information. Keep all audience paths active unless visibility limits them. Ranking may raise the engagement prediction value because the source says polls drive interaction, but it must not remove the other score factors. Analytics must record that the post type was a poll.

## Pair 3.4 - Mentioned users

A: The translated domains say users mentioned in the post receive direct notification regardless of matching score and see the post at premium tier.

B: Plan: store post content and mention data. Analyze content and run normal audience matching. In parallel with feed placement, create a mentioned-user path that selects mentioned users, sends them direct notification, and marks their feed placement as premium when visibility allows. Review privacy before notifying or placing the post. Analytics must identify mentioned-user reach separately from matched reach.

Pair 3.4 - Phrasing B

A: The translated descriptions say mentions override the normal score threshold but not privacy rules.

B: Plan: carry mention information from post creation into notification and feed placement. Mentioned users bypass matching score requirements, but they do not bypass visibility. The scheme keeps the direct-notification path separate from normal ranked distribution so review can verify the override is intentional.

## Pair 3.5 - High-follower author

A: The translated domains say a high-follower user may have more than fifty thousand recipients, requiring progressive batching, close connections first, and feed update rate limiting.

B: Plan: store and analyze the post. Estimate audience size. Run all allowed audience paths, then split distribution into progressive batches. Send the closest and highest-value recipients first, then broader audiences. Apply feed-update rate limits to protect the feed store. Record backlog and timing metrics. Review throughput, retry safety, and partial-completion behavior.

Pair 3.5 - Phrasing B

A: The translated descriptions say the audience is very large and feed writes must be controlled.

B: Plan: after recipient ranking, do not attempt one giant feed update. Partition recipients, begin with close connections, continue with wider reach, and monitor write backlog. The plan keeps the ranking logic intact while pacing distribution so a popular author does not overload the feed system.

## Pair 3.6 - Image-only post

A: The translated domains say a post may contain no text and only an image. Text analysis cannot run; image recognition may be used if available; otherwise distribute to friends and groups only without business matching.

B: Plan: store the image post and visibility. Try image recognition. If image recognition returns usable topics, use them for matching. If not, mark business matching unavailable, continue with social and group paths, and rank only the eligible recipients from those paths. Review fallback correctness so the flow does not pretend it has text analysis.

Pair 3.6 - Phrasing B

A: The translated descriptions say meaning extraction may fail because there is no text.

B: Plan: preserve the publication path, attempt the available non-text analysis, and clearly choose the reduced audience route when no content meaning is available. The fallback still distributes to friends and groups, but it does not create business matches from nothing.

## Pair 3.7 - All matching branches timeout

A: The translated domains say all three matching services may timeout. Ranking receives zero results at the join point. Fallback distribution goes to direct connections only.

B: Plan: let ranking wait for the three audience branches until their timeout boundaries are reached. If all three fail, create a fallback recipient list from direct connections only. Do not mark the audience as business-matched or group-matched. Place the post only for direct connections, record the degraded path, and select a failure-handling arbiter.

Pair 3.7 - Phrasing B

A: The translated descriptions say the join point can receive no audience lists and must still produce a safe limited distribution.

B: Plan: the join point treats missing business, friend, and group results as a degraded state, not as a successful empty audience. The fallback path uses direct connections, updates feeds for that limited set, and records why broader distribution did not occur.

## Pair 3.8 - Post edited or deleted after distribution

A: The translated domains say an edited post must be reanalyzed and reranked without re-injecting into feeds that already saw it, while a deleted post must be removed from feeds immediately with possible short eventual delay.

B: Plan: for edits, keep the existing feed entries, rerun analysis and ranking, then update those entries in place. Do not insert duplicate feed items. For deletion, publish removal intent, batch-remove the post from feeds, and accept that some cached feeds may show it for up to five minutes. Review duplicate prevention for edits and removal completeness for deletions.

Pair 3.8 - Phrasing B

A: The translated descriptions say post changes after distribution have two different paths: update existing feed entries for edits and remove all feed entries for deletions.

B: Plan: an edit restarts understanding and ranking but reuses already-created feed placements. A delete starts removal and cleanup. The scheme separates those paths because the correct action is not the same: edit means revise, delete means withdraw.

