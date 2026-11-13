# Node-01 - Domain Splitter Set

## Purpose

Runtime A is a human prompt about friend requests and feed integration.
Runtime B is the array of domains touched by the prompt.

## Pair 1.1 - Send request

A: "Let User A send a friend request to User B if they are not blocked and not already connected."

B: [friend request validation and lifecycle, block and pending-request handling]

Pair 1.1 - Phrasing B

A: "Create a connect request only when the relationship is allowed."

B: [friend request validation and lifecycle, block and pending-request handling]

## Pair 1.2 - Notify and score initial match

A: "After a friend request is sent, notify the recipient and calculate an initial compatibility score."

B: [notification for request and acceptance, initial compatibility scoring]

Pair 1.2 - Phrasing B

A: "Send the request alert and estimate how compatible the two business profiles are."

B: [notification for request and acceptance, initial compatibility scoring]

## Pair 1.3 - Accept request

A: "When User B accepts, create a bidirectional connection and start feed integration."

B: [bidirectional connection creation, feed integration start, notification for request and acceptance]

Pair 1.3 - Phrasing B

A: "On acceptance, make both users connected and begin calculating how deeply their feeds should merge."

B: [bidirectional connection creation, feed integration start, notification for request and acceptance]

## Pair 1.4 - Calculate group weight

A: "Use shared groups, activity overlap, role similarity, and join-date proximity as part of the connection weight."

B: [group overlap weight]

Pair 1.4 - Phrasing B

A: "Measure how much the two users overlap in group memberships and group behavior."

B: [group overlap weight]

## Pair 1.5 - Calculate event weight

A: "Use co-attended events, event type match, attendance frequency, and future event overlap."

B: [event overlap weight]

Pair 1.5 - Phrasing B

A: "Compare the events both users have attended or plan to attend."

B: [event overlap weight]

## Pair 1.6 - Calculate purchase and questionnaire weights

A: "Compare marketplace purchase categories, price ranges, brands, wishlists, questionnaire answers, scores, and learning paths."

B: [purchase overlap weight, questionnaire similarity weight]

Pair 1.6 - Phrasing B

A: "Use purchase behavior and learning questionnaire similarity to help decide connection strength."

B: [purchase overlap weight, questionnaire similarity weight]

## Pair 1.7 - Final connection weight

A: "Combine base match, group, event, purchase, questionnaire, and machine adjustment into one final integration weight."

B: [final connection weight and machine adjustment]

Pair 1.7 - Phrasing B

A: "Turn all compatibility evidence into one connection-strength score."

B: [final connection weight and machine adjustment]

## Pair 1.8 - Historical post integration

A: "Insert relevant historical posts from the last thirty days into both users' feeds based on connection strength."

B: [historical post integration, connection-strength tiers]

Pair 1.8 - Phrasing B

A: "After the connection is accepted, merge past posts into each user's feed at positions based on the weight."

B: [historical post integration, connection-strength tiers]

## Pair 1.9 - Privacy and block controls

A: "Respect feed integration level, shared post types, and block behavior; blocking should remove all connection data and feed integration."

B: [privacy controls for feed integration and shared post types, block and pending-request handling]

Pair 1.9 - Phrasing B

A: "Users should control how much friend content enters their feed and blocking should immediately clean up the connection."

B: [privacy controls for feed integration and shared post types, block and pending-request handling]

## Pair 1.10 - Abuse and delayed integration

A: "Prevent friend request spam, bot-network manipulation, weight gaming from rapid group joins, and handle delayed feed integration during maintenance."

B: [request spam, bot manipulation, and weight gaming protection, delayed feed integration and partial weight fallback]

Pair 1.10 - Phrasing B

A: "Limit abusive connection behavior and keep the friendship valid even if feed integration has to run later."

B: [request spam, bot manipulation, and weight gaming protection, delayed feed integration and partial weight fallback]

