# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label.
Runtime B is the same request in domain language.

## Pair 2.1 - Friend request lifecycle

A: Fragment: "Let User A send a friend request to User B if they are not blocked and not already connected." Domain: friend request validation and lifecycle.

B: The connection domain must validate sender, recipient, block status, existing connection, existing pending request, and request context before creating a pending friend request.

Pair 2.1 - Phrasing B

A: Fragment: "Create a connect request only when the relationship is allowed." Domain: friend request validation and lifecycle.

B: A friend request can be created only when the relationship state permits it and the request does not reveal blocked status to the sender.

## Pair 2.2 - Notification and initial match

A: Fragment: "Notify the recipient and calculate an initial compatibility score." Domain: notification and initial compatibility scoring.

B: The request phase sends the recipient notification and calculates a base compatibility score from profile similarity, interest overlap, network distance, activity alignment, and professional match factors.

Pair 2.2 - Phrasing B

A: Fragment: "Send the request alert and estimate how compatible the two business profiles are." Domain: initial compatibility scoring.

B: The initial match result becomes the base match component used later in the final integration weight.

## Pair 2.3 - Acceptance and connection creation

A: Fragment: "When User B accepts, create a bidirectional connection and start feed integration." Domain: bidirectional connection creation.

B: Acceptance creates a two-way connection record, sends acceptance notification, and starts the feed integration process that calculates how strongly the users' feeds should merge.

Pair 2.3 - Phrasing B

A: Fragment: "Make both users connected and begin calculating how deeply their feeds should merge." Domain: connection creation and feed integration start.

B: The accepted request changes relationship state from pending to connected and triggers the weight-analysis phase.

## Pair 2.4 - Group overlap

A: Fragment: "Use shared groups, activity overlap, role similarity, and join-date proximity." Domain: group overlap weight.

B: The group-weight domain measures common groups, overlap in activity, similarity of group roles, and closeness of join dates, then returns a group sub-weight with factor details.

Pair 2.4 - Phrasing B

A: Fragment: "Measure how much the two users overlap in group memberships and group behavior." Domain: group overlap weight.

B: Shared group analysis contributes to connection strength by showing whether the two users participate in the same communities in similar ways.

## Pair 2.5 - Event overlap

A: Fragment: "Use co-attended events, event type match, attendance frequency, and future event overlap." Domain: event overlap weight.

B: The event-weight domain compares events both users attended, type similarity, attendance frequency, and future event overlap, then returns an event sub-weight.

Pair 2.5 - Phrasing B

A: Fragment: "Compare the events both users have attended or plan to attend." Domain: event overlap weight.

B: Co-attended and planned events act as evidence that the connection has shared context.

## Pair 2.6 - Purchase and questionnaire overlap

A: Fragment: "Compare marketplace purchase categories, price ranges, brands, wishlists, questionnaire answers, scores, and learning paths." Domain: purchase overlap and questionnaire similarity.

B: Purchase overlap compares categories, price range, brand match, and wishlist similarity without exposing individual purchase details. Questionnaire similarity compares shared questionnaires, answer similarity, score proximity, and learning path using aggregate or similarity values rather than raw private answers.

Pair 2.6 - Phrasing B

A: Fragment: "Use purchase behavior and learning questionnaire similarity to help decide connection strength." Domain: purchase overlap and questionnaire similarity.

B: These domains add commercial and learning alignment to the final weight while preserving sensitive purchase and answer privacy.

## Pair 2.7 - Final connection weight

A: Fragment: "Combine base match, group, event, purchase, questionnaire, and machine adjustment into one final integration weight." Domain: final connection weight.

B: The final-weight domain combines base match at twenty-five percent, group at twenty percent, event at twenty percent, purchase at fifteen percent, questionnaire at twenty percent, and an adjustment between minus 0.2 and plus 0.2.

Pair 2.7 - Phrasing B

A: Fragment: "Turn all compatibility evidence into one connection-strength score." Domain: final connection weight.

B: The final score determines connection tier, historical post volume, feed zone, and notification intensity.

## Pair 2.8 - Historical post integration

A: Fragment: "Insert relevant historical posts from the last thirty days into both users' feeds based on connection strength." Domain: historical post integration.

B: The feed integration domain retrieves recent posts from both users and places them into each other's feeds according to strength: strong connections get twenty posts near the top, medium get ten posts in the middle, weak get five high-engagement posts lower down.

Pair 2.8 - Phrasing B

A: Fragment: "Merge past posts into each user's feed at positions based on the weight." Domain: historical post integration.

B: Historical feed merge uses the final weight to choose post count, feed zone, position formula, maximum consecutive posts, and diversity limits.

## Pair 2.9 - Privacy controls

A: Fragment: "Respect feed integration level, shared post types, and block behavior." Domain: privacy controls.

B: The privacy domain enforces user-controlled integration level, allowed post types, and block behavior. Blocking immediately removes connection data and feed integration.

Pair 2.9 - Phrasing B

A: Fragment: "Users should control how much friend content enters their feed." Domain: privacy controls.

B: Feed integration must follow Full, Selective, or Minimal preference and post-type sharing choices.

## Pair 2.10 - Abuse and delayed integration

A: Fragment: "Prevent friend request spam, bot-network manipulation, weight gaming, and handle delayed feed integration." Domain: abuse protection and delayed integration.

B: The abuse domain limits requests to twenty per day, detects artificial connection networks, watches rapid group joins that inflate weight, and allows connection creation to succeed even when feed integration is queued for later recovery.

Pair 2.10 - Phrasing B

A: Fragment: "Limit abusive connection behavior and keep friendship valid if feed integration runs later." Domain: abuse protection and delayed integration.

B: The flow must separate relationship validity from feed-merge timing while still detecting request abuse and manipulation.

