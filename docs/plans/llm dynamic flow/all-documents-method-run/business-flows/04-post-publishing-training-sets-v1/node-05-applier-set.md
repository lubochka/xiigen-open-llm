# Node-05 - Applier Set

## Purpose

Runtime A is one work piece, its transformation instruction, and the processed result from Node-04.
Runtime B is the concrete action taken and its outcome.

## Pair 5.1 - Store before starting distribution

A: Work piece: create the post record. Transformation instruction: do not begin downstream distribution until storage succeeds. Processed result: the post is valid and contains content, author, visibility, metadata, and mention information.

B: Action outcome: the post record is stored first. Only after storage succeeds does the flow announce that a post was created and make the post available to analysis, connection lookup, group lookup, and analytics tracking.

Pair 5.1 - Phrasing B

A: Work piece: prepare the publication state. Transformation instruction: preserve state-before-announcement order. Processed result: the valid post state exists.

B: Action outcome: the flow writes the post state before any later unit is asked to act on it. The publication announcement is allowed only because the stored post can now be read by the following stages.

## Pair 5.2 - Apply content analysis

A: Work piece: analyze post content. Transformation instruction: return usable meaning signals or a stated fallback. Processed result: rich analysis, reduced keyword result, image-recognition result, or no usable analysis.

B: Action outcome: the content analysis result is attached to the post. Rich analysis enables business matching. Reduced or missing analysis marks business matching as limited or unavailable and still allows allowed social and group distribution.

Pair 5.2 - Phrasing B

A: Work piece: create the meaning result for the post. Transformation instruction: expose reduced confidence instead of pretending analysis succeeded. Processed result: analysis outcome is known.

B: Action outcome: the flow carries either full meaning, partial meaning, or no usable meaning into the audience stage, and each later branch can see which case occurred.

## Pair 5.3 - Apply parallel audience matching

A: Work piece: run business, social, and group audience paths. Transformation instruction: keep the three paths independent until ranking joins them. Processed result: each branch has candidates, no candidates, or a defined fallback reason.

B: Action outcome: business matches, friend connections, and group connections are emitted as separate audience results. The flow does not combine them early. Ranking receives each branch result separately so it can merge them at the join point.

Pair 5.3 - Phrasing B

A: Work piece: gather all recipient sources. Transformation instruction: preserve branch identity. Processed result: audience branch outcomes are ready.

B: Action outcome: the audience stage returns three named lists or three named fallback states. The ranking stage can tell whether each recipient came from business relevance, social relationship, group membership, or more than one source.

## Pair 5.4 - Apply ranking formula

A: Work piece: compile and score recipients. Transformation instruction: use the document's fixed ranking weights. Processed result: recipient inputs include match, friend, group, activity, recency, and predicted engagement where available.

B: Action outcome: each recipient receives a composite score calculated from business match at twenty-five percent, friend score at twenty percent, group score at fifteen percent, activity at twenty percent, recency at ten percent, and predicted engagement at ten percent. Recipients are assigned to premium, high, medium, low, or minimal tier.

Pair 5.4 - Phrasing B

A: Work piece: turn audience candidates into ranked feed recipients. Transformation instruction: keep all required score factors visible. Processed result: candidate score inputs are ready.

B: Action outcome: the ranked recipient list is produced with one score per recipient, source information preserved, and a feed tier selected from the score thresholds.

## Pair 5.5 - Apply feed insertion and reordering

A: Work piece: update feeds and reorder them. Transformation instruction: place posts by score while enforcing diversity. Processed result: ranked recipients have tiers and position ranges.

B: Action outcome: feed entries are added in batches, with premium recipients receiving top placement and lower tiers receiving lower placement. After insertion, feeds are reordered so the top ten do not contain more than two posts from the same author and maintain at least sixty percent topic diversity.

Pair 5.5 - Phrasing B

A: Work piece: distribute the ranked post into feeds. Transformation instruction: preserve both relevance and variety. Processed result: the feed placement plan is ready.

B: Action outcome: the post appears in recipient feeds according to score, then each affected feed is balanced using author limits, topic diversity, freshness, and engagement signals.

## Pair 5.6 - Apply mentioned-user override

A: Work piece: handle mentioned users. Transformation instruction: notify mentioned users and place the post at premium tier when visibility allows. Processed result: mentions are present and eligible.

B: Action outcome: mentioned users receive direct notification regardless of normal matching score, and their feed entry is placed in the premium tier if the post visibility permits them to see it.

Pair 5.6 - Phrasing B

A: Work piece: process mentions. Transformation instruction: score override does not override privacy. Processed result: mentioned users are known.

B: Action outcome: the flow treats mentions as a direct attention path, sends notification, and promotes feed placement only for mentioned users who are allowed recipients.

## Pair 5.7 - Apply high-volume batching

A: Work piece: distribute a post from a high-follower author. Transformation instruction: use progressive batches and rate limits. Processed result: audience size may exceed fifty thousand.

B: Action outcome: distribution starts with closest connections, continues into broader recipient groups, and limits feed update speed to avoid overload. Backlog, batch progress, and distribution timing are recorded.

Pair 5.7 - Phrasing B

A: Work piece: place a large-audience post. Transformation instruction: avoid one uncontrolled feed-write burst. Processed result: ranked recipients are very large in number.

B: Action outcome: feed updates are divided into controlled batches, the highest-priority people are served first, and the flow remains resumable if later batches are delayed.

## Pair 5.8 - Apply edit and delete behavior

A: Work piece: handle a post changed after distribution. Transformation instruction: edit updates existing feed entries; delete removes them. Processed result: the change type is edit or delete.

B: Action outcome: if edited, the post is reanalyzed and reranked, then existing feed entries are updated in place without duplicate injection. If deleted, the post is removed from feeds immediately and the possible short cache delay is recorded.

Pair 5.8 - Phrasing B

A: Work piece: revise or withdraw distributed content. Transformation instruction: do not treat edits and deletes as the same action. Processed result: change path is known.

B: Action outcome: edits refresh the meaning and ranking of an existing feed item; deletions start removal from every feed that may contain the post.

