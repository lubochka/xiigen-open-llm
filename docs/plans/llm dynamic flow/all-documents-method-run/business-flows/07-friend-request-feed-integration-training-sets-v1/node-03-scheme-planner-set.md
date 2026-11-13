# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of domain translations from Node-02.
Runtime B is an ordered and parallel plan with wait states, recovery paths, and review points.

## Pair 3.1 - Full friend request plan

A: The translated domains say that User A sends a request, the relationship must be allowed, User B is notified, initial match is calculated, acceptance creates a bidirectional connection, four sub-weight branches run, final connection weight is calculated, historical posts merge into feeds, and connection strength evolves over time.

B: Plan: validate the request first, checking block, existing connection, and pending state. Create the pending request and notify the recipient. Calculate the initial match score. If the recipient accepts, create the bidirectional connection and notify the requester. Start feed integration only after acceptance. Run four weight branches in parallel: group, event, purchase, and questionnaire. Wait up to the defined limit for all four; if one times out, use default 0.5 for that component and retry asynchronously. Combine all weights with machine adjustment. Retrieve historical posts from the last thirty days and insert the correct number into each feed according to strength tier and privacy settings. Schedule ongoing rebalancing. Review block behavior, duplicate pending requests, request spam, privacy, purchase and questionnaire data exposure, weight formula, feed caps, and delayed integration.

Pair 3.1 - Phrasing B

A: The translated descriptions require request lifecycle, acceptance, compatibility analysis, feed merge, and ongoing strength adjustment.

B: Plan: no feed merge happens before acceptance. No final weight happens before the four weight branches have either returned or fallen back. No historical posts enter feeds until privacy controls and connection strength are known.

## Pair 3.2 - Heavy shared context

A: The translated domains say the users share five groups, three events, and similar purchase history, so the final weight is likely strong and twenty historical posts should enter top feed positions.

B: Plan: after acceptance, run the four sub-weight branches. Expect high group, event, and purchase weights. Combine them with base match and questionnaire similarity. If final weight exceeds 0.8, classify as strong, integrate twenty posts from the last thirty days into the top twenty percent of each feed, enforce maximum three consecutive friend posts, and enable full notifications when privacy allows.

Pair 3.2 - Phrasing B

A: The translated descriptions describe a high-context connection that should receive deep feed integration.

B: Plan: strong shared evidence raises connection strength and increases post volume and feed priority, but diversity and privacy limits still apply.

## Pair 3.3 - Cold connection

A: The translated domains say the users met externally and have no platform overlap, so weight depends mostly on base profile similarity and likely stays below 0.5.

B: Plan: validate and accept the connection normally. Run all four weight branches, but expect low or empty group, event, purchase, and questionnaire results. Combine with base match. If final weight is below 0.5, classify as weak and insert five high-engagement historical posts into lower feed positions. Allow strength to grow later through interaction.

Pair 3.3 - Phrasing B

A: The translated descriptions describe a valid relationship with little evidence in the platform.

B: Plan: the connection is allowed, but feed integration begins lightly. The system does not invent shared context to force a strong score.

## Pair 3.4 - Mutual pending request

A: The translated domains say User B already sent a request to User A. Detect mutual pending requests, auto-accept both, and proceed to integration.

B: Plan: when User A sends the request, check pending requests in both directions. If reverse pending exists, convert the mutual pending state into one accepted bidirectional connection. Do not create two connections or two feed integrations. Continue with acceptance notification and weight calculation once.

Pair 3.4 - Phrasing B

A: The translated descriptions say both users already requested each other.

B: Plan: mutual pending requests collapse into one connection acceptance and one integration flow.

## Pair 3.5 - Withdrawn request

A: The translated domains say the sender withdraws before acceptance. Delete the request, remove unseen notification, or show withdrawn if already seen.

B: Plan: find the pending request. If not accepted, delete it. If the recipient has not seen the notification, remove it. If seen, update the notification to say the request was withdrawn. Do not create a connection, do not calculate acceptance weights, and do not merge feeds.

Pair 3.5 - Phrasing B

A: The translated descriptions say the request ends before the recipient accepts.

B: Plan: withdrawal stops the lifecycle at request state. Feed integration never starts.

## Pair 3.6 - Blocked user

A: The translated domains say a blocked user tries to send a request. Check the block list before creating request and return a generic failure without revealing the block.

B: Plan: validate block state before request creation. If blocked, do not create pending request, notification, match score, or analytics that reveals block reason to the sender. Return a generic "request could not be sent" result. Review privacy and no-reveal behavior.

Pair 3.6 - Phrasing B

A: The translated descriptions say block status should stop the request without disclosing sensitive relationship information.

B: Plan: blocked requests stop at validation and use a neutral failure message.

## Pair 3.7 - Weight service timeout

A: The translated domains say one weight calculation service times out. Weight calculation waits max ten seconds, uses available weights with default 0.5 for the missing component, and retries missing component asynchronously.

B: Plan: start all four sub-weight branches. At the wait limit, collect completed branches. For any missing branch, assign default 0.5 and mark retry pending. Calculate final weight with the default included. Feed integration can proceed with degraded confidence, and the missing component may update later through retry.

Pair 3.7 - Phrasing B

A: The translated descriptions say the final weight can proceed with partial evidence when one branch is unavailable.

B: Plan: the missing branch is not ignored and not treated as zero. It receives the source-defined default and a retry path.

## Pair 3.8 - Feed service maintenance

A: The translated domains say connection is established during feed service maintenance. Save connection, queue integration event, and run feed integration when service recovers.

B: Plan: create the bidirectional connection normally after acceptance. If feed integration cannot run now, queue the integration work. The user sees the new connection in the connection list, but historical posts appear later after recovery. Review delayed integration and retry.

Pair 3.8 - Phrasing B

A: The translated descriptions say friendship state should not be rolled back just because feed merge is delayed.

B: Plan: relationship creation and feed integration are separate outcomes. The connection can exist while feed work waits for recovery.

