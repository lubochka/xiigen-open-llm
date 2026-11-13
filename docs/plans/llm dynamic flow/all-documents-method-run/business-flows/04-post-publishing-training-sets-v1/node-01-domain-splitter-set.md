# Node-01 - Domain Splitter Set

## Purpose

Runtime A is a human prompt about publishing posts and distributing them into feeds.
Runtime B is the array of domains the prompt touches.

## Pair 1.1 - Publish a post

A: "Let a business owner publish a text, image, video, article, or poll post with a chosen visibility."

B: [post creation and visibility, content storage]

Pair 1.1 - Phrasing B

A: "A user should be able to create a new post with media and decide who can see it."

B: [post creation and visibility, content storage]

## Pair 1.2 - Analyze content

A: "When a post is published, analyze its topics, keywords, people or companies mentioned, sentiment, category, and language."

B: [content analysis]

Pair 1.2 - Phrasing B

A: "Read the post content and understand what it is about before deciding who should see it."

B: [content analysis]

## Pair 1.3 - Match businesses

A: "Show the post to businesses whose profiles align with the post topic."

B: [business audience matching]

Pair 1.3 - Phrasing B

A: "Find business-level recipients based on questionnaire alignment, industry, size, and location."

B: [business audience matching]

## Pair 1.4 - Match social connections

A: "Distribute the post to friends, followers, second-degree connections, and people the author follows."

B: [social-connection matching]

Pair 1.4 - Phrasing B

A: "Use the author's relationship graph to identify direct friends, broader connections, followers, and following relationships."

B: [social-connection matching]

## Pair 1.5 - Match group members

A: "If the post is relevant to groups the author belongs to, include the right group members in the audience."

B: [group membership matching]

Pair 1.5 - Phrasing B

A: "Use group memberships and group relevance to add group recipients for the post."

B: [group membership matching]

## Pair 1.6 - Rank recipients

A: "Combine match score, friend score, group score, activity, recency, and predicted engagement to rank each recipient."

B: [composite ranking, recipient list compilation]

Pair 1.6 - Phrasing B

A: "Create one recipient list and score each person using business relevance, social closeness, group relevance, behavior, freshness, and likely engagement."

B: [composite ranking, recipient list compilation]

## Pair 1.7 - Place into feeds

A: "Put the post into recipient feeds in premium, high, medium, low, or minimal positions based on score."

B: [feed tier placement, feed distribution]

Pair 1.7 - Phrasing B

A: "Use the ranking result to decide where the post appears in each recipient's feed."

B: [feed tier placement, feed distribution]

## Pair 1.8 - Preserve diversity

A: "Do not let one author dominate the top ten feed positions, and keep topic diversity above sixty percent."

B: [feed diversity and reordering]

Pair 1.8 - Phrasing B

A: "Reorder feeds so the top positions stay varied by author and topic."

B: [feed diversity and reordering]

## Pair 1.9 - Enforce privacy and safety

A: "Private posts must never appear in other users' feeds, harmful posts should be flagged, and spam should be limited."

B: [privacy enforcement, content safety, rate limiting]

Pair 1.9 - Phrasing B

A: "Respect post visibility, moderate unsafe content, sanitize user text, and prevent spam publishing."

B: [privacy enforcement, content safety, rate limiting]

## Pair 1.10 - Track distribution

A: "After feeds are updated, report reach, impressions, distribution time, tier breakdown, and engagement metrics."

B: [distribution analytics]

Pair 1.10 - Phrasing B

A: "Measure how far the post traveled, how fast it was distributed, and how users reacted to it."

B: [distribution analytics]

