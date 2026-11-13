# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility and the formal result or result group to examine.
Runtime B is pass, concern, or reprocess instruction.

## Pair 8.1 - Block no reveal

A: Review responsibility: block-no-reveal arbiter. Formal result: blocked user receives "User B blocked you" when request fails.

B: Reprocess. The source requires a generic "request could not be sent" response. Send the request-validation fragment back and remove block disclosure.

Pair 8.1 - Phrasing B

A: Review responsibility: blocked-user review. Formal result: blocked request still creates notification.

B: Reprocess. Block checks happen before request creation. Send the request fragment back and require no request, notification, match score, or feed integration.

## Pair 8.2 - Duplicate connection

A: Review responsibility: mutual-pending arbiter. Formal result: reverse pending requests create two connections and two feed integrations.

B: Reprocess. Mutual pending requests should auto-accept into one bidirectional connection and one integration flow. Send the pending-request fragment back.

Pair 8.2 - Phrasing B

A: Review responsibility: duplicate-connection review. Formal result: users already connected receive another connection record.

B: Reprocess. Existing connections must block new request creation. Send the relationship-state fragment back.

## Pair 8.3 - Request spam and gaming

A: Review responsibility: request-spam arbiter. Formal result: user sends more than twenty friend requests in one day.

B: Reprocess. The source limits friend request spam to twenty requests per day. Send the request-rate fragment back and block or delay excess requests.

Pair 8.3 - Phrasing B

A: Review responsibility: weight-gaming review. Formal result: rapid group joins sharply increase group weight without anomaly flag.

B: Concern. The source warns about users joining groups just for weight boost. Require rapid group-join patterns to be flagged before final weight is trusted.

## Pair 8.4 - Sensitive data

A: Review responsibility: sensitive-data arbiter. Formal result: purchase overlap result exposes individual purchases to the other user.

B: Reprocess. Purchase history overlap must not expose individual purchase data. Send the purchase-weight fragment back and replace raw purchases with aggregate overlap.

Pair 8.4 - Phrasing B

A: Review responsibility: privacy-of-weight-inputs review. Formal result: questionnaire similarity includes raw answers.

B: Reprocess. Questionnaire similarity must use aggregate scores or similarity values, not raw answers. Send the questionnaire-weight fragment back.

## Pair 8.5 - Formula and adjustment

A: Review responsibility: formula-weight arbiter. Formal result: final weight omits purchase component and changes group weight to fifty percent.

B: Reprocess. The source defines base twenty-five percent, group twenty percent, event twenty percent, purchase fifteen percent, questionnaire twenty percent. Send the final-weight fragment back.

Pair 8.5 - Phrasing B

A: Review responsibility: adjustment-range review. Formal result: machine adjustment is plus 0.6.

B: Reprocess. The adjustment range is minus 0.2 to plus 0.2. Send the adjustment fragment back and constrain it to the source range.

## Pair 8.6 - Feed integration limits

A: Review responsibility: post-volume arbiter. Formal result: weak connection receives twenty historical posts in top feed positions.

B: Reprocess. Weak connections receive five high-engagement posts in the bottom forty percent. Send the historical-feed fragment back.

Pair 8.6 - Phrasing B

A: Review responsibility: friend-content-cap review. Formal result: integrated friend content becomes fifty percent of total feed.

B: Reprocess. The source caps friend content at thirty percent of total feed. Send the feed-rebalance fragment back.

## Pair 8.7 - Privacy after connection

A: Review responsibility: feed-privacy arbiter. Formal result: user set integration to Minimal, but full historical feed merge occurred.

B: Reprocess. Feed integration level must control how much friend content enters the feed. Send the feed-integration fragment back and apply the Minimal setting.

Pair 8.7 - Phrasing B

A: Review responsibility: block-cleanup review. Formal result: user blocks a connection but integrated posts remain in feed.

B: Reprocess. Blocking immediately removes connection data and feed integration. Send the block-cleanup fragment back.

## Pair 8.8 - Failure and delay

A: Review responsibility: timeout-default arbiter. Formal result: one weight branch timed out and was treated as zero.

B: Reprocess. The source says missing components use default 0.5 and retry asynchronously. Send the missing-weight fragment back.

Pair 8.8 - Phrasing B

A: Review responsibility: queued-integration review. Formal result: feed service maintenance causes the accepted connection to be rolled back.

B: Reprocess. The connection should remain saved and integration should be queued. Send the delayed-feed fragment back.

