# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label from Node-01.
Runtime B is the same request written in the language of that domain, with the source meaning preserved.

## Pair 2.1 - Post creation and visibility

A: Fragment: "Let a business owner publish a text, image, video, article, or poll post with a chosen visibility." Domain: post creation and visibility.

B: The post publication request requires accepting multiple content forms, storing the post with its author and visibility choice, and treating visibility as a rule that follows the post through every later distribution step.

Pair 2.1 - Phrasing B

A: Fragment: "A user should be able to create a new post with media and decide who can see it." Domain: post creation and visibility.

B: The publishing step must capture the post body, any attached media or poll information, the author, and the allowed audience, so later feed placement cannot ignore the chosen visibility.

## Pair 2.2 - Content analysis

A: Fragment: "Analyze topics, keywords, entities, sentiment, category, and language." Domain: content analysis.

B: The content analysis domain must inspect the published post and return the subject areas, important terms, named items, emotional tone, category labels, and detected language used by later matching and ranking.

Pair 2.2 - Phrasing B

A: Fragment: "Read the post content and understand what it is about before deciding who should see it." Domain: content analysis.

B: Before audience selection, the system must create an understanding of the post that includes its meaning, classification, language, and signals that can be used for relevance scoring.

## Pair 2.3 - Business audience matching

A: Fragment: "Show the post to businesses whose profiles align with the post topic." Domain: business audience matching.

B: The business matching domain must compare the analyzed post against business profiles using questionnaire alignment, industry relevance, size compatibility, and location proximity, then return candidate recipients with match reasons and scores.

Pair 2.3 - Phrasing B

A: Fragment: "Find business-level recipients based on questionnaire alignment, industry, size, and location." Domain: business audience matching.

B: Business recipients are selected by comparing post meaning to profile traits and returning each matched business user with the factors that explain why the post is relevant to them.

## Pair 2.4 - Social-connection matching

A: Fragment: "Distribute the post to friends, followers, second-degree connections, and people the author follows." Domain: social-connection matching.

B: The social matching domain must resolve the author's relationship graph and classify recipients as direct friends, second-degree connections, followers, or following relationships with connection strength values.

Pair 2.4 - Phrasing B

A: Fragment: "Use the author's relationship graph to identify direct friends, broader connections, followers, and following relationships." Domain: social-connection matching.

B: The connection path returns social recipients with relationship type, strength, and mutual connection context so ranking can distinguish close relationships from weaker ones.

## Pair 2.5 - Group membership matching

A: Fragment: "If the post is relevant to groups the author belongs to, include the right group members in the audience." Domain: group membership matching.

B: The group matching domain must identify the author's relevant groups, find eligible members, respect posting rules, and return group recipients with group relevance and member role information.

Pair 2.5 - Phrasing B

A: Fragment: "Use group memberships and group relevance to add group recipients for the post." Domain: group membership matching.

B: Group recipients are chosen from memberships where the post belongs, and each recipient carries the group context needed for ranking and feed placement.

## Pair 2.6 - Composite ranking

A: Fragment: "Combine match score, friend score, group score, activity, recency, and predicted engagement to rank each recipient." Domain: composite ranking.

B: The ranking domain must compile all candidate recipients and calculate one priority score per recipient using these weights: business match twenty-five percent, friend relationship twenty percent, group relevance fifteen percent, activity twenty percent, recency ten percent, and predicted engagement ten percent.

Pair 2.6 - Phrasing B

A: Fragment: "Score each person using business relevance, social closeness, group relevance, behavior, freshness, and likely engagement." Domain: composite ranking.

B: Each recipient receives a combined ranking score formed from relevance, relationship strength, group fit, prior behavior, time freshness, and expected interaction, and the score determines feed priority.

## Pair 2.7 - Feed tier placement

A: Fragment: "Put the post into recipient feeds in premium, high, medium, low, or minimal positions based on score." Domain: feed tier placement.

B: The feed placement domain must convert each recipient score into a tier: premium for scores above 0.8, high above 0.6, medium above 0.4, low above 0.2, and minimal above 0.0, then place the post in the corresponding feed range.

Pair 2.7 - Phrasing B

A: Fragment: "Use the ranking result to decide where the post appears in each recipient's feed." Domain: feed tier placement.

B: Feed insertion uses the recipient's composite score to choose the visible priority band and position range for the post.

## Pair 2.8 - Feed diversity and reordering

A: Fragment: "Do not let one author dominate the top ten feed positions, and keep topic diversity above sixty percent." Domain: feed diversity and reordering.

B: The feed reordering domain must enforce a maximum of two posts per author in the top ten, maintain at least sixty percent topic diversity, and reposition existing feed items using freshness, engagement, and diversity penalties.

Pair 2.8 - Phrasing B

A: Fragment: "Reorder feeds so the top positions stay varied by author and topic." Domain: feed diversity and reordering.

B: After insertion, the feed must be balanced so highly scored content remains visible without crowding out different authors or topics.

## Pair 2.9 - Privacy and safety

A: Fragment: "Private posts must never appear in other users' feeds, harmful posts should be flagged, and spam should be limited." Domain: privacy enforcement and content safety.

B: The safety domain must enforce visibility at feed placement, moderate harmful content, detect spam, sanitize unsafe embedded content, prevent graph exposure, and watch for coordinated ranking manipulation.

Pair 2.9 - Phrasing B

A: Fragment: "Respect post visibility, moderate unsafe content, sanitize user text, and prevent spam publishing." Domain: privacy enforcement and content safety.

B: The flow must treat privacy and safety as gates before distribution: hidden content stays hidden, unsafe content is stopped or flagged, repeated posting is limited, and ranking abuse is detected.

## Pair 2.10 - Distribution analytics

A: Fragment: "Report reach, impressions, distribution time, tier breakdown, and engagement metrics." Domain: distribution analytics.

B: The analytics domain must collect distribution reach, impressions, timing, tier counts, feed engagement, scroll behavior, first-engagement timing, content-type performance, and author diversity.

Pair 2.10 - Phrasing B

A: Fragment: "Measure how far the post traveled, how fast it was distributed, and how users reacted to it." Domain: distribution analytics.

B: After distribution, the flow must summarize where the post appeared, how quickly it appeared, how prominently it was placed, and what engagement signals followed.

