# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full collection of formal results plus flow context. Runtime B
is the exact arbiter list to run.

## Pair 7.1 - Profile and personal business data

A: Results include business profile creation with revenue range, team size, goals, challenges, and target market.

B: Select arbiters:
- privacy arbiter: confidential business details are handled.
- access control arbiter: public versus connections-only fields must be respected.
- event sequence arbiter: BusinessProfileCreated must follow profile storage.

Pair 7.1 - Phrasing B

A: Business profile result contains confidential business details.

B: Select [privacy arbiter, access control arbiter, event sequence arbiter].

## Pair 7.2 - Matching suggestions

A: Results include ranked business matches, match scores, and match reasons.

B: Select arbiters:
- privacy arbiter: match reasons must not reveal private business details.
- matching integrity arbiter: weights and score composition must match the document.
- consent arbiter: match visibility must follow bilateral consent and opt-out rules.

Pair 7.2 - Phrasing B

A: Business matches are ready for the user.

B: Select [privacy arbiter, matching integrity arbiter, consent arbiter].

## Pair 7.3 - Learning program

A: Results include generated curriculum and learning preferences.

B: Select arbiters:
- content availability arbiter: curriculum must not reference unavailable content.
- privacy arbiter: learning preferences are private to the user.
- personalization arbiter: curriculum should reflect goals, challenges, and skill gaps.

Pair 7.3 - Phrasing B

A: Learning path was generated.

B: Select [content availability arbiter, privacy arbiter, personalization arbiter].

## Pair 7.4 - Feed and event personalization

A: Results include personalized content feed and event feed.

B: Select arbiters:
- personalization arbiter: feed and events must use the right inputs.
- cache arbiter: feed cache and event cache must use their documented lifetimes.
- audience language arbiter: final wording should not expose internal scoring or events.

Pair 7.4 - Phrasing B

A: Feeds are ready for the dashboard.

B: Select [personalization arbiter, cache arbiter, audience language arbiter].

## Pair 7.5 - Timeout or degraded path

A: Results include matching timeout, partial results, fallback feed, or generic content.

B: Select arbiters:
- resilience arbiter: the branch must degrade gracefully without hard failure.
- user experience arbiter: the user message must be calm and useful.
- completion arbiter: OnboardingCompleted must not claim unavailable work is complete.

Pair 7.5 - Phrasing B

A: Personalization completed with partial or fallback results.

B: Select [resilience arbiter, user experience arbiter, completion arbiter].

## Pair 7.6 - Analytics conflict

A: Results include user-reported Healthcare and model-predicted Tech.

B: Select arbiters:
- categorization arbiter: user input must remain primary.
- review-routing arbiter: strong divergence should be marked for review.
- privacy arbiter: analytics output must not store personal data unnecessarily.

Pair 7.6 - Phrasing B

A: ML categorization disagrees with the user.

B: Select [categorization arbiter, review-routing arbiter, privacy arbiter].

## Pair 7.7 - Opt-out

A: Results include user opted out of matching and no match suggestions should be exposed.

B: Select arbiters:
- consent arbiter: opt-out must be respected.
- formatter consistency arbiter: final output must not mention match suggestions.
- privacy arbiter: no match results should be stored or shown against the user's choice.

Pair 7.7 - Phrasing B

A: User declined matching.

B: Select [consent arbiter, formatter consistency arbiter, privacy arbiter].

## Pair 7.8 - Full onboarding completion

A: Results include profile created, analytics done or safely degraded, learning generated, matches ready or safely delayed, feeds personalized, events personalized, and ready notification.

B: Select arbiters:
- event sequence arbiter
- completion arbiter
- privacy arbiter
- personalization arbiter
- resilience arbiter
- formatter consistency arbiter

Pair 7.8 - Phrasing B

A: The whole business onboarding flow is ready for final answer.

B: Select arbiters for event order, completion, privacy, personalization, resilience, and final consistency.
