# Node-01 - Domain Splitter Set

## Purpose

Runtime A is a human prompt about business onboarding and personalization.
Runtime B is the array of domains the prompt touches.

## Pair 1.1 - Profile creation

A: "After a user finishes the questionnaire, create a structured business profile from the answers."

B: [business profile construction]

Reason: The document says the Business Service creates a structured business profile from questionnaire responses.

Pair 1.1 - Phrasing B

A: "Turn the onboarding questionnaire answers into the user's business profile."

B: [business profile construction]

## Pair 1.2 - Analytics segmentation

A: "Analyze the user's profile and categorize the business."

B: [analytics and categorization]

Reason: The document says Analytics Service segments the user, identifies patterns, and categorizes the business.

Pair 1.2 - Phrasing B

A: "Use the profile to find the user's segment, behavior patterns, tags, and maturity level."

B: [analytics and categorization]

## Pair 1.3 - Learning program

A: "Generate a personalized learning program from the user's skill gaps and business challenges."

B: [learning personalization]

Reason: The document says Learning Service generates a customized learning program based on skill gaps and business challenges.

Pair 1.3 - Phrasing B

A: "Give the new business owner a curriculum that fits their goals, challenges, and stage."

B: [learning personalization]

## Pair 1.4 - Matching

A: "Find businesses that match this user and explain why each match is relevant."

B: [matching]

Reason: The document defines compatibility scoring, match types, ranked suggestions, scores, and reasons.

Pair 1.4 - Phrasing B

A: "Show business connection suggestions with match scores and match reasons."

B: [matching]

## Pair 1.5 - Feed and events

A: "Personalize the user's content feed and events feed after matching is ready."

B: [feed personalization, event recommendations]

Reason: The document says Feed Service adapts posts/articles/updates and Events Service adapts upcoming events.

Pair 1.5 - Phrasing B

A: "Use the profile, matches, and learning path to choose posts and events for the new user."

B: [feed personalization, event recommendations]

## Pair 1.6 - Ready notification

A: "Notify the user when their personalized experience is ready."

B: [notifications and user experience]

Reason: The document includes a user loading state and notification that match suggestions are ready.

Pair 1.6 - Phrasing B

A: "After personalization finishes, tell the user that matches and feeds are ready."

B: [notifications and user experience]

## Pair 1.7 - Privacy controls

A: "Users must be able to opt out of matching, and match reasons must not reveal private business details."

B: [privacy and access control, matching]

Reason: The document defines matching opt-out, bilateral visibility, private business fields, and safe match reasons.

Pair 1.7 - Phrasing B

A: "Make matching private: only matched users see results, and explanations must not expose sensitive details."

B: [privacy and access control, matching]

## Pair 1.8 - Graceful degradation

A: "If matching takes too long, show partial results and tell the user more matches are coming soon."

B: [operations and resilience, matching, notifications and user experience]

Reason: The document defines a 30-second matching timeout and a graceful message.

Pair 1.8 - Phrasing B

A: "When matching is slow, do not fail onboarding; return what is ready and keep searching."

B: [operations and resilience, matching, notifications and user experience]

## Pair 1.9 - Metrics

A: "Track match acceptance, feed engagement, learning starts, and time to first match."

B: [product metrics]

Reason: The product manager section names those metrics.

Pair 1.9 - Phrasing B

A: "Measure whether onboarding personalization improves retention and engagement."

B: [product metrics]

## Pair 1.10 - Full flow request

A: "After the questionnaire, build the profile, run analytics and learning in parallel, find matches, personalize feeds, and finish onboarding."

B: [business profile construction, analytics and categorization, learning personalization, matching, feed personalization, event recommendations, notifications and user experience, operations and resilience]

Reason: This prompt covers the full business onboarding flow.

Pair 1.10 - Phrasing B

A: "Run the whole personalization layer from completed questionnaire to ready dashboard."

B: [business profile construction, analytics and categorization, learning personalization, matching, feed personalization, event recommendations, notifications and user experience, operations and resilience]
