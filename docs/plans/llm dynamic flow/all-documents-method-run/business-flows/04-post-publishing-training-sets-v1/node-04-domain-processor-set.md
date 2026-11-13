# Node-04 - Domain Processor Set

## Purpose

Runtime A is one planned work piece with its domain context.
Runtime B is the complete processed result for that piece, including success, fallback, and wait-state outcomes.

## Pair 4.1 - Store published post

A: Work piece: store a newly published post with author, content type, media, poll details when present, and visibility. Domain context: post creation and visibility.

B: Processed result: the post can move forward only after the stored record contains the author, content body or media reference, content type, visibility, mentions, hashtags, and metadata. If validation fails, no downstream distribution begins. If validation succeeds, the visibility value becomes a rule attached to every later audience and feed result.

Pair 4.1 - Phrasing B

A: Work piece: prepare the publication record before any audience work starts. Domain context: post creation and visibility.

B: Processed result: publication has a durable post state, and that state includes enough information for analysis, matching, notification, feed placement, and privacy review. Without the stored state, later events and feed entries are not allowed.

## Pair 4.2 - Analyze post content

A: Work piece: extract topics, keywords, entities, sentiment, categories, and language from a post. Domain context: content analysis.

B: Processed result: successful analysis returns the meaning signals needed by matching and ranking. If the post has no text, image recognition may supply meaning. If unsupported language is detected, basic keyword extraction is the reduced result and the result must say matching quality is lower.

Pair 4.2 - Phrasing B

A: Work piece: understand the post before matching recipients. Domain context: content analysis.

B: Processed result: the analysis path either returns rich meaning signals, a reduced keyword-only result, or a no-text fallback. Each outcome tells later nodes whether business matching can rely on the analysis or must be limited.

## Pair 4.3 - Find business matches

A: Work piece: identify business recipients whose profiles align with the analyzed post. Domain context: business audience matching.

B: Processed result: business matches contain recipient identity, match score, match factors, and match reason. The score considers questionnaire alignment, industry relevance, size compatibility, and location proximity. If content analysis is unavailable or visibility excludes public reach, the processed result is no business matching for this run.

Pair 4.3 - Phrasing B

A: Work piece: compare post meaning to business profiles. Domain context: business audience matching.

B: Processed result: the branch returns scored business candidates only when analysis and visibility allow it. It must not create business recipients from an image-only post with no recognized meaning or from a connections-only post.

## Pair 4.4 - Find social connections

A: Work piece: identify friends, followers, second-degree connections, and following relationships for the author. Domain context: social-connection matching.

B: Processed result: social recipients contain user identity, relationship type, connection strength, and mutual connection context. Direct friends receive the strongest relationship value, second-degree connections and followers receive lower values, and the output remains eligible for ranking even if business or group matching fails.

Pair 4.4 - Phrasing B

A: Work piece: collect recipients from the author's relationship network. Domain context: social-connection matching.

B: Processed result: the social branch returns a relationship-aware audience list that ranking can use to distinguish close connections from weaker graph relationships.

## Pair 4.5 - Find group recipients

A: Work piece: identify group members for groups where the post is relevant and allowed. Domain context: group membership matching.

B: Processed result: group recipients contain group identity, user identity, member role, and group relevance score. If group posting rules or visibility do not allow the post, the branch returns no eligible group recipients with the reason preserved for review.

Pair 4.5 - Phrasing B

A: Work piece: use group memberships to find additional recipients. Domain context: group membership matching.

B: Processed result: the group path returns only members from groups where the post belongs and where the visibility rule permits distribution.

## Pair 4.6 - Compile recipient list and score

A: Work piece: wait for business, social, and group lists, then compile one ranked recipient list. Domain context: recipient list compilation and composite ranking.

B: Processed result: the join point accepts completed or fallback branch outcomes, merges duplicate recipients, preserves each recipient source, calculates a composite score using all available score factors, and assigns a tier and position range. If all matching branches time out, the result becomes direct-connections-only fallback instead of full distribution.

Pair 4.6 - Phrasing B

A: Work piece: combine audience paths and calculate feed priority. Domain context: composite ranking.

B: Processed result: one recipient list exists after the wait point, with each recipient carrying source, raw score inputs, combined score, tier, and position guidance. Missing branches are recorded as missing, not silently ignored.

## Pair 4.7 - Update and reorder feeds

A: Work piece: distribute the post to feeds, apply tier placement, enforce author and topic diversity, and reorder existing feed content. Domain context: feed distribution and reordering.

B: Processed result: feed updates are batched by recipient count, placed according to tier, limited by diversity rules, and reordered with time decay and engagement boost. The top ten cannot contain more than two posts by the same author, and topic diversity must remain at least sixty percent.

Pair 4.7 - Phrasing B

A: Work piece: place the ranked post into recipient feeds without damaging feed quality. Domain context: feed distribution and reordering.

B: Processed result: each feed receives the post at an appropriate priority while the feed as a whole stays varied by author and topic. The result records update count, tier spread, and reorder summary.

## Pair 4.8 - Track analytics and completion

A: Work piece: aggregate distribution metrics after feed update and reordering. Domain context: distribution analytics.

B: Processed result: analytics collects reach, impressions, distribution time, tier breakdown, feed engagement, scroll depth, time to first engagement, content type performance, and author diversity. Completion is not valid until the distribution metrics exist.

Pair 4.8 - Phrasing B

A: Work piece: measure the result of post distribution. Domain context: distribution analytics.

B: Processed result: the flow ends with evidence of where the post appeared, how it was positioned, how long distribution took, and how the audience interacted with it.

