# 10 - Simulation Check

## Source Being Simulated

`C:\Users\lubao\Documents\xiigen\business flows\07-friend-request-feed-integration.md`

The simulation checks whether these node sets can recreate the friend-request and feed-integration flow without using the source document as runtime input.

## Test Prompt 1 - Full Accepted Request

Runtime user prompt:

"User A sends User B a friend request. If User B accepts, create the connection, calculate connection strength from match, groups, events, purchases, and questionnaires, then integrate recent posts into both feeds according to that strength."

Expected behavior:

- Node-01 returns request lifecycle, notification, initial match, connection creation, feed integration start, group weight, event weight, purchase weight, questionnaire weight, final weight, historical feed integration, privacy, and rebalancing.
- Node-02 writes precise domain statements for each label.
- Node-03 keeps request phase separate from acceptance phase and runs the four weight branches in parallel after acceptance.
- Node-04 processes each branch with success, fallback, and privacy outcomes.
- Node-05 creates the request, creates the accepted connection, calculates final weight, integrates posts, and schedules rebalancing.
- Node-07 selects request, block, abuse, weight, privacy, formula, feed cap, and delay arbiters.
- Node-08 rejects blocked requests, duplicate connections, raw purchase or answer exposure, wrong formula, excessive feed integration, and bad timeout handling.
- Node-09 returns one answer explaining request, acceptance, weight, feed merge, and ongoing adjustment.

Pass condition:

Feed integration must not happen before acceptance, and final weight must not collapse the four weight branches into one vague compatibility step.

## Test Prompt 2 - Blocked User

Runtime user prompt:

"A blocked user tries to send a friend request."

Expected behavior:

- Node-01 returns block handling and request validation.
- Node-03 stops before request creation.
- Node-05 returns generic failure with no notification.
- Node-08 rejects any result that reveals the block or creates downstream work.
- Node-09 says the request could not be sent without revealing why.

Pass condition:

No final answer may disclose that the recipient blocked the sender.

## Test Prompt 3 - One Weight Service Timeout

Runtime user prompt:

"The event weight service times out during connection strength calculation."

Expected behavior:

- Node-01 returns weight branches, partial fallback, final weight.
- Node-03 waits up to the limit, uses default 0.5 for event weight, and schedules retry.
- Node-05 calculates final weight with fallback and reduced confidence.
- Node-08 rejects treating the missing component as zero or ignoring it.
- Node-09 explains fallback and retry.

Pass condition:

The missing branch must receive default 0.5 and async retry.

## Test Prompt 4 - Weak Connection

Runtime user prompt:

"Two users accept a friend request but have no shared groups, events, purchases, or questionnaire overlap."

Expected behavior:

- Node-01 returns connection creation, final weight, weak feed integration.
- Node-03 produces light feed integration.
- Node-05 inserts five high-engagement posts in lower positions.
- Node-08 rejects strong-tier feed behavior.
- Node-09 explains conservative integration.

Pass condition:

The result must not integrate twenty top-position posts.

## Test Prompt 5 - User Blocks After Connection

Runtime user prompt:

"After becoming friends, one user blocks the other."

Expected behavior:

- Node-01 returns block cleanup, feed privacy, connection data removal.
- Node-03 removes connection data and feed integration.
- Node-05 applies cleanup.
- Node-08 rejects leaving integrated posts in feeds.
- Node-09 explains cleanup.

Pass condition:

Connection and feed effects must be removed together.

## Missing-Training Detector

If simulation creates feed integration before acceptance, add Node-03 and Node-05 ordering pairs.

If mutual pending requests create duplicate connections, add Node-03, Node-05, and Node-08 mutual-pending pairs.

If raw purchases or questionnaire answers appear in output, add Node-02 and Node-08 privacy pairs.

If final weight ignores the formula, add Node-04, Node-05, and Node-08 formula pairs.

If weak connections receive strong feed placement, add Node-04, Node-05, Node-08, and Node-09 tier pairs.

If timeout is treated as zero instead of default 0.5 with retry, add Node-03 and Node-08 timeout pairs.

If block cleanup removes the connection but leaves feed content, add Node-07 and Node-08 block-cleanup pairs.

