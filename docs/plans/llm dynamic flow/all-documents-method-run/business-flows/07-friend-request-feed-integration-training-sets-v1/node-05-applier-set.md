# Node-05 - Applier Set

## Purpose

Runtime A is one work piece, its transformation instruction, and the processed result from Node-04.
Runtime B is the concrete action and outcome.

## Pair 5.1 - Create pending request

A: Work piece: valid friend request. Transformation instruction: create request only after relationship validation. Processed result: users are not blocked, not already connected, and no duplicate pending state should be created.

B: Action outcome: one pending friend request is created, the recipient notification is sent, initial compatibility scoring starts, and no feed integration starts yet.

Pair 5.1 - Phrasing B

A: Work piece: request phase. Transformation instruction: request first, feed later. Processed result: request is allowed.

B: Action outcome: the social relationship is pending, not connected, and only request-phase work is active.

## Pair 5.2 - Auto-accept mutual pending

A: Work piece: mutual pending request. Transformation instruction: do not create two requests or two connections. Processed result: User B already sent a request to User A.

B: Action outcome: both pending requests collapse into one accepted bidirectional connection, one acceptance notification path, and one feed integration flow.

Pair 5.2 - Phrasing B

A: Work piece: reverse pending detected. Transformation instruction: make one connection. Processed result: mutual request exists.

B: Action outcome: the users become connected once, and duplicated downstream work is avoided.

## Pair 5.3 - Withdraw request

A: Work piece: pending request withdrawal. Transformation instruction: stop lifecycle before acceptance. Processed result: request is not accepted yet.

B: Action outcome: the pending request is deleted. If the recipient has not seen the notification, it is removed; if seen, it is updated to "request withdrawn." No connection or feed integration is created.

Pair 5.3 - Phrasing B

A: Work piece: sender cancels request. Transformation instruction: remove request-state effects only. Processed result: request can be withdrawn.

B: Action outcome: the request ends cleanly and never reaches the accepted flow.

## Pair 5.4 - Accept and start integration

A: Work piece: accepted request. Transformation instruction: create connection before feed integration. Processed result: accepted relationship is valid.

B: Action outcome: the bidirectional connection is stored, the requester is notified of acceptance, and feed integration starts from the connection identity.

Pair 5.4 - Phrasing B

A: Work piece: accepted friend request. Transformation instruction: relationship state first, integration second. Processed result: connection can be created.

B: Action outcome: both users are connected, and the feed-integration process is allowed to begin.

## Pair 5.5 - Apply weight calculation

A: Work piece: final weight calculation. Transformation instruction: combine all components and default missing branch after timeout. Processed result: base, group, event, purchase, and questionnaire components are present or defaulted.

B: Action outcome: final connection weight is calculated with the defined component weights and machine adjustment, then classified into strong, medium, weak, or dormant connection tier.

Pair 5.5 - Phrasing B

A: Work piece: combine compatibility evidence. Transformation instruction: preserve component reasons. Processed result: weight inputs are ready.

B: Action outcome: one connection-strength score is produced and can explain how shared groups, events, purchases, questionnaires, and base match contributed.

## Pair 5.6 - Apply historical post integration

A: Work piece: feed merge. Transformation instruction: use connection strength and privacy controls. Processed result: post count, feed zone, and allowed post types are known.

B: Action outcome: historical posts from the last thirty days are inserted into both users' feeds according to tier: twenty top-zone posts for strong, ten middle-zone posts for medium, five lower-zone high-engagement posts for weak, reduced further by privacy settings where required.

Pair 5.6 - Phrasing B

A: Work piece: integrate posts. Transformation instruction: do not exceed limits or privacy preferences. Processed result: feed plan is ready.

B: Action outcome: each feed receives the right amount of friend content in the right zone without exceeding consecutive-post, friend-content, or sharing rules.

## Pair 5.7 - Apply ongoing rebalancing

A: Work piece: ongoing connection strength. Transformation instruction: update feed behavior from engagement over time. Processed result: interaction frequency, engagement quality, and reciprocal actions are available.

B: Action outcome: connection strength evolves, feeds rebalance every six hours, low-performing integrated posts can be removed, and new posts from the friend receive a twenty-four-hour boost according to current tier.

Pair 5.7 - Phrasing B

A: Work piece: maintain feed relevance. Transformation instruction: connection strength is dynamic. Processed result: engagement signals are ready.

B: Action outcome: the connection's feed priority changes as the relationship becomes stronger, weaker, or dormant.

## Pair 5.8 - Apply delayed and partial fallback

A: Work piece: service timeout or feed maintenance. Transformation instruction: preserve valid relationship and retry delayed integration. Processed result: one weight service timed out or feed service unavailable.

B: Action outcome: missing weight component receives default 0.5 and async retry, or accepted connection is stored while feed integration is queued until service recovery. The final result marks the degraded or delayed state.

Pair 5.8 - Phrasing B

A: Work piece: complete what can safely complete. Transformation instruction: do not lose accepted connection. Processed result: partial failure exists.

B: Action outcome: the connection remains valid, and the unavailable analysis or feed work is retried or delayed instead of silently disappearing.

