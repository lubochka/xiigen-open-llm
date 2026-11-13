# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label. Runtime B is the same
fragment expressed in the formal wording of that domain.

## Pair 2.1 - Business profile construction

A: Fragment: "Create a structured business profile from questionnaire answers." Domain: business profile construction.

B: The completed questionnaire is the source for a BusinessProfile. Extract industry, business stage, team size, goals, challenges, location, target market, and business details from the responses. Store the profile as the user's business identity for personalization. This profile is required before matching, feed personalization, event recommendations, and learning coordination can use business context.

Pair 2.1 - Phrasing B

A: Fragment: "Turn questionnaire responses into a usable business profile." Domain: business profile construction.

B: Build a structured profile record from the user's questionnaire answers. The profile must contain the business fields needed by matching, learning, feed, and events. The profile becomes the shared context for the rest of onboarding.

## Pair 2.2 - Analytics and categorization

A: Fragment: "Analyze the profile and categorize the business." Domain: analytics and categorization.

B: Analyze the user's profile segments and behavior patterns. Produce business categories, industry tags, and maturity level. If analytic categorization disagrees strongly with the user's own industry answer, treat the user's answer as primary and mark the divergence for review.

Pair 2.2 - Phrasing B

A: Fragment: "Find segments, patterns, tags, and maturity level." Domain: analytics and categorization.

B: The analytics domain turns profile evidence into segmentation and category signals. User-provided business identity remains the first source of truth; machine categorization is a secondary signal.

## Pair 2.3 - Learning personalization

A: Fragment: "Generate a learning program from skill gaps and business challenges." Domain: learning personalization.

B: Use the business profile and questionnaire answers to identify skill gaps and challenges. Generate a curriculum with modules, duration, difficulty, skills, and learning path. Validate that the recommended content is available before the program is published.

Pair 2.3 - Phrasing B

A: Fragment: "Create a customized curriculum for this business owner." Domain: learning personalization.

B: Produce a learning plan that fits the user's goals, challenges, business stage, and available content. Do not reference content that has been deleted or restricted.

## Pair 2.4 - Matching

A: Fragment: "Find business matches and explain why each match is relevant." Domain: matching.

B: Run compatibility scoring against active businesses. Score using industry match weight 0.30, location proximity weight 0.25, business stage weight 0.25, and interest overlap weight 0.20. Produce ranked matches with match score, match type, and match reasons. Match reasons must be useful without revealing private business details.

Pair 2.4 - Phrasing B

A: Fragment: "Show ranked connection suggestions with reasons." Domain: matching.

B: Build a ranked list of suggested businesses. Classify each match as peer, mentor, complementary, or collaborative when the evidence supports it. Include reasons the user can understand safely.

## Pair 2.5 - Feed personalization

A: Fragment: "Adapt the content feed to the user's profile and matches." Domain: feed personalization.

B: Build a feed configuration using matched business content, industry trending content, learning-related content, and platform trending content. The document's weights are 40 percent matched business content, 25 percent industry trending content, 20 percent learning-related content, and 15 percent platform trending content.

Pair 2.5 - Phrasing B

A: Fragment: "Make the user's feed relevant after onboarding." Domain: feed personalization.

B: Personalize the feed from profile, match, category, and learning signals. Keep the feed fresh with the defined one-hour cache window.

## Pair 2.6 - Event recommendations

A: Fragment: "Adapt the events feed based on interests and learning path." Domain: event recommendations.

B: Recommend upcoming events using the user's business profile, industry, stage, interests, matches, and learning program. Event recommendations are cached for four hours and sent to the user interface and calendar path.

Pair 2.6 - Phrasing B

A: Fragment: "Show events that fit the user's industry and learning goals." Domain: event recommendations.

B: Choose events that match the user's onboarding evidence. Use learning and matching context so the events feed is not generic.

## Pair 2.7 - Notifications and user experience

A: Fragment: "Tell the user when their personalized experience is ready." Domain: notifications and user experience.

B: The user should see a "personalizing your experience" state while processing runs. When matches and feeds are ready, send a clear notification such as "Your personalized experience is ready" or "We found business matches for you." The message should guide the user to the ready experience, not expose internal event details.

Pair 2.7 - Phrasing B

A: Fragment: "Show progress and notify when onboarding is done." Domain: notifications and user experience.

B: Present a waiting state during personalization and a ready state after completion. The user language should focus on matches, feeds, events, and learning being ready.

## Pair 2.8 - Privacy and access control

A: Fragment: "Match results and learning data must be private." Domain: privacy and access control.

B: Match results are visible only to matched users under a bilateral consent model. Business profile fields have configurable privacy levels. Learning data is private to the user. Analytics storage should not include personal data. Similar-business behavior must use a minimum group size of five. Users can opt out of matching.

Pair 2.8 - Phrasing B

A: Fragment: "Do not reveal sensitive business details through matching or analytics." Domain: privacy and access control.

B: Protect confidential business details, internal match results, and learning preferences. Match explanations must not reveal another user's private business details.

## Pair 2.9 - Operations and resilience

A: Fragment: "If analytics, matching, learning, or feed personalization fails, degrade gracefully." Domain: operations and resilience.

B: Analytics failure falls back to industry defaults. Matching failure shows no suggestions yet and asks the user to check back soon. Feed failure shows generic trending content. Learning failure should not block the account. Matching has a thirty-second timeout and can return partial results with more matches coming later.

Pair 2.9 - Phrasing B

A: Fragment: "Onboarding personalization should not hard fail if one branch is slow." Domain: operations and resilience.

B: Treat non-critical branch failures as degraded personalization. Preserve progress, show safe fallback content, and continue when the branch recovers.

## Pair 2.10 - Product metrics

A: Fragment: "Track whether personalization creates value." Domain: product metrics.

B: Measure match acceptance rate, feed engagement rate, learning program start and completion, time from questionnaire completion to first match suggestion, Net Promoter Score correlation with match quality, and event attendance for recommended versus non-recommended events.

Pair 2.10 - Phrasing B

A: Fragment: "Report onboarding personalization quality." Domain: product metrics.

B: Track whether users accept matches, engage with the feed, start learning, attend recommended events, and reach value quickly after the questionnaire.
