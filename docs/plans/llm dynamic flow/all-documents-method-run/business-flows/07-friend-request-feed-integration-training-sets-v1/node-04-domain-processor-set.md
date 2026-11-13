# Node-04 - Domain Processor Set

## Purpose

Runtime A is one planned work piece with domain context.
Runtime B is the completed result for that piece.

## Pair 4.1 - Validate request

A: Work piece: validate a friend request before creation. Domain context: friend request lifecycle, block handling, pending requests.

B: Processed result: the request can proceed only when the users are not blocked, not already connected, and do not already have a pending request requiring special handling. Blocked requests return generic failure. Reverse pending requests become mutual acceptance instead of duplicate requests.

Pair 4.1 - Phrasing B

A: Work piece: decide whether a connect request is allowed. Domain context: request validation.

B: Processed result: the relationship state is known before any notification, match score, or feed work begins.

## Pair 4.2 - Create request and initial match

A: Work piece: create pending request, notify recipient, and calculate initial match. Domain context: request phase.

B: Processed result: a pending request exists with sender, recipient, message, sender profile, and request context. Recipient notification is prepared. Initial match score contains profile similarity, interest overlap, network distance, activity alignment, and professional match.

Pair 4.2 - Phrasing B

A: Work piece: start request phase. Domain context: notification and matching.

B: Processed result: the recipient can respond to the request, and the flow has a base match score ready for later weight calculation.

## Pair 4.3 - Accept connection

A: Work piece: accept friend request and create bidirectional connection. Domain context: connection creation.

B: Processed result: one bidirectional connection exists with connection identity, request identity, sender, recipient, connection type, accepted time, and initial strength context. Acceptance notification is ready.

Pair 4.3 - Phrasing B

A: Work piece: turn pending relationship into connection. Domain context: acceptance.

B: Processed result: both users are connected once, not twice, and feed integration can start from the accepted connection.

## Pair 4.4 - Calculate sub-weights

A: Work piece: calculate group, event, purchase, and questionnaire sub-weights in parallel. Domain context: weight branches.

B: Processed result: group, event, purchase, and questionnaire branches each return a sub-weight with factor details or a missing-component fallback. Purchase and questionnaire branches return aggregate overlap or similarity, not raw private purchase or answer data.

Pair 4.4 - Phrasing B

A: Work piece: gather all compatibility evidence. Domain context: parallel weight analysis.

B: Processed result: four named weight results are ready for final combination or explicitly marked defaulted with retry.

## Pair 4.5 - Combine final weight

A: Work piece: combine base match, sub-weights, and machine adjustment. Domain context: final connection weight.

B: Processed result: final weight includes base match at twenty-five percent, group at twenty percent, event at twenty percent, purchase at fifteen percent, questionnaire at twenty percent, and an adjustment within minus 0.2 to plus 0.2. The result includes confidence and component weights.

Pair 4.5 - Phrasing B

A: Work piece: produce connection-strength score. Domain context: final weight.

B: Processed result: one score determines strong, medium, weak, or dormant connection behavior and explains which components shaped the score.

## Pair 4.6 - Integrate historical posts

A: Work piece: retrieve posts from both users from the last thirty days and insert into feeds. Domain context: historical post integration.

B: Processed result: strong connections receive twenty posts in the top twenty percent with max three consecutive; medium receive ten posts in the middle forty percent with diversity; weak receive five high-engagement posts in the bottom forty percent. Privacy settings may reduce post types or integration level.

Pair 4.6 - Phrasing B

A: Work piece: merge feeds according to connection strength. Domain context: feed integration.

B: Processed result: feed positions, post counts, time window, diversity limits, and privacy filters are all known before feed entries are written.

## Pair 4.7 - Rebalance ongoing feed

A: Work piece: update ongoing connection strength and rebalance feeds. Domain context: connection-strength evolution.

B: Processed result: feeds rebalance every six hours, friend content stays at or below thirty percent of total feed, low-performing integrated posts are removed, new friend posts receive a twenty-four-hour boost, and notifications follow connection strength tier.

Pair 4.7 - Phrasing B

A: Work piece: maintain connection relevance over time. Domain context: feed rebalancing.

B: Processed result: the connection is not frozen at acceptance; engagement changes future feed priority and notification intensity.

## Pair 4.8 - Handle abuse and failure

A: Work piece: process spam, bot manipulation, weight gaming, timeout, or feed maintenance. Domain context: abuse protection and delayed integration.

B: Processed result: request spam is limited, artificial connection patterns are flagged, rapid group-join weight gaming is detected, missing weight branches use default and retry, and feed maintenance queues integration without deleting the accepted connection.

Pair 4.8 - Phrasing B

A: Work piece: protect the connection flow from manipulation and service gaps. Domain context: resilience and abuse.

B: Processed result: the relationship stays correct while risky or delayed parts are limited, flagged, retried, or deferred.

