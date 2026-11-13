# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full formal result collection R1...Rn plus flow context.
Runtime B is the list of review units that must fire.

## Pair 7.1 - Request validation

A: Formal results show a friend request, sender, recipient, block status, existing connection state, or pending state.

B: Select request-validity arbiter, block-no-reveal arbiter, duplicate-connection arbiter, and mutual-pending arbiter.

Pair 7.1 - Phrasing B

A: Formal results include relationship-state checks before request creation.

B: Select relationship-state review, blocked-user review, and pending-request review.

## Pair 7.2 - Request abuse

A: Formal results show high request volume, repeated requests, bot-like connection patterns, or rapid group joins before request.

B: Select request-spam arbiter, bot-network arbiter, weight-gaming arbiter, and anomaly-review arbiter.

Pair 7.2 - Phrasing B

A: Formal results include suspicious connection behavior.

B: Select anti-abuse review, artificial-connection review, and rapid-context-change review.

## Pair 7.3 - Acceptance and connection

A: Formal results show accepted request and bidirectional connection creation.

B: Select accepted-state arbiter, single-connection arbiter, acceptance-notification arbiter, and feed-start-order arbiter.

Pair 7.3 - Phrasing B

A: Formal results move from pending to connected and start feed integration.

B: Select connection-state review and integration-start review.

## Pair 7.4 - Weight branches

A: Formal results show group, event, purchase, and questionnaire sub-weights.

B: Select parallel-weight-branch arbiter, missing-component arbiter, timeout-default arbiter, and sensitive-data arbiter.

Pair 7.4 - Phrasing B

A: Formal results include four weight branch outputs or fallbacks.

B: Select weight-branch completeness review, fallback review, and privacy-of-weight-inputs review.

## Pair 7.5 - Final weight formula

A: Formal results show final connection weight, component weights, machine adjustment, and confidence score.

B: Select formula-weight arbiter, adjustment-range arbiter, component-preservation arbiter, and tier-assignment arbiter.

Pair 7.5 - Phrasing B

A: Formal results include the combined connection-strength score.

B: Select score-formula review, machine-adjustment review, and connection-tier review.

## Pair 7.6 - Historical feed merge

A: Formal results show post counts, feed zones, position formula, and integrated posts.

B: Select historical-window arbiter, post-volume arbiter, feed-zone arbiter, max-consecutive arbiter, and friend-content-cap arbiter.

Pair 7.6 - Phrasing B

A: Formal results include last-thirty-day post insertion into both feeds.

B: Select feed-integration review, placement-limit review, and diversity review.

## Pair 7.7 - Privacy and block after connection

A: Formal results show feed integration level, shared post types, or blocking after connection.

B: Select feed-privacy arbiter, post-type-sharing arbiter, block-cleanup arbiter, and connection-data-removal arbiter.

Pair 7.7 - Phrasing B

A: Formal results include user controls over integrated content.

B: Select privacy-preference review and block-removal review.

## Pair 7.8 - Failure and delay

A: Formal results show weight timeout, feed maintenance, retry queue, or delayed integration.

B: Select degraded-weight arbiter, retry arbiter, queued-integration arbiter, and user-visible-state arbiter.

Pair 7.8 - Phrasing B

A: Formal results include a valid connection with incomplete or delayed feed work.

B: Select fallback-state review, delayed-merge review, and completion-wording review.

