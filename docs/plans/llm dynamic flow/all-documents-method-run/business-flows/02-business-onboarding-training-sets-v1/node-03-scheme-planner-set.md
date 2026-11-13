# Node-03 - Scheme Planner Set

## Purpose

Runtime A is all translated domain descriptions for one path. Runtime B is an
actual plan: sub-tasks b1...bn and transformational prompts fb1...fbn.

## Pair 3.1 - Full happy path

A: Domain translations describe completed questionnaire, business profile creation, analytics categorization, learning program generation, matching, feed personalization, event personalization, notification, and onboarding completion.

B:
b1: Start from QuestionnaireCompleted.
fb1: Treat the completed questionnaire as the trigger for three parallel branches.

b2: Create the business profile.
fb2: Extract structured business details and publish BusinessProfileCreated after the profile exists.

b3: Analyze and categorize the user.
fb3: Segment the user and publish UserProfileAnalyzed and BusinessCategorized when ready.

b4: Generate the learning program.
fb4: Use business profile and challenges to produce curriculum, then store learning preferences.

b5: Run matching after the business profile is available.
fb5: Score active businesses using the documented weights and produce ranked matches with safe reasons.

b6: Personalize content feed and event feed.
fb6: Use profile, matches, categories, and learning preferences to create feed and event recommendations.

b7: Notify and complete onboarding.
fb7: Notify the user after the experience is ready and publish OnboardingCompleted after required steps are complete.

Pair 3.1 - Phrasing B

A: "Plan the full business onboarding personalization flow."

B: Start from questionnaire completion; fan out to profile, analytics, and learning; join on profile for matching; personalize feeds and events; notify the user; then close onboarding.

## Pair 3.2 - Parallel branch plan

A: Domain translations emphasize that three branches run concurrently after QuestionnaireCompleted.

B:
b1: Business profile branch.
fb1: Extract and store business details from the questionnaire.

b2: Analytics branch.
fb2: Analyze the user independently and store segmentation evidence.

b3: Learning branch.
fb3: Begin learning work, but use BusinessProfileCreated when the curriculum needs structured business data.

b4: Branch convergence.
fb4: Do not wait for analytics to finish before allowing critical profile and matching work; allow non-critical branches to finish asynchronously.

Pair 3.2 - Phrasing B

A: "Plan the fan-out after the questionnaire is completed."

B: Run profile creation, analytics, and learning as separate branches. Let matching wait for the profile, not for every optional signal.

## Pair 3.3 - Matching plan

A: Domain translations describe ranked business matches, match types, scores, and reasons.

B:
b1: Collect matching inputs.
fb1: Require business profile and categorization where available.

b2: Score candidate businesses.
fb2: Apply industry 0.30, location 0.25, stage 0.25, and interest overlap 0.20.

b3: Classify match type.
fb3: Choose peer, mentor, complementary, or collaborative based on the evidence.

b4: Generate safe match reasons.
fb4: Explain relevance without exposing private business details.

b5: Publish match and suggestion readiness.
fb5: Produce BusinessMatchesFound and ConnectionSuggestionsReady.

Pair 3.3 - Phrasing B

A: "Plan how matching turns a business profile into connection suggestions."

B: Gather inputs, score businesses, classify match type, write safe reasons, then announce matches and suggestions.

## Pair 3.4 - Feed and event personalization plan

A: Domain translations describe personalized content feed and event feed after matching and learning signals.

B:
b1: Build content feed configuration.
fb1: Use matched business content, industry trending content, learning content, and platform trending content in the documented mix.

b2: Build event recommendations.
fb2: Use industry, stage, learning path, and match context to select relevant events.

b3: Cache personalization results.
fb3: Apply feed cache for one hour and event recommendations cache for four hours.

b4: Send results to the user interface.
fb4: Publish feed and event personalization results for rendering.

Pair 3.4 - Phrasing B

A: "Plan the personalized dashboard feeds."

B: Produce content feed and event feed separately, cache each on its own lifetime, and make both available to the interface.

## Pair 3.5 - Niche industry plan

A: Domain translations describe a very specialized industry with few exact matches.

B:
b1: Try exact industry matches.
fb1: Use the normal scoring model first.

b2: Broaden matching criteria.
fb2: Include adjacent industries, similar challenges, and same business stage.

b3: Adjust learning.
fb3: Focus more on cross-industry skills when exact industry content is thin.

b4: Explain the result.
fb4: Tell the user the platform found broader but relevant suggestions.

Pair 3.5 - Phrasing B

A: "Plan matching when the user's industry is niche."

B: Search exact first, broaden safely, adjust learning, and explain why broader matches are shown.

## Pair 3.6 - Re-personalization plan

A: Domain translations describe a returning user who retakes the questionnaire.

B:
b1: Accept updated answers.
fb1: Treat the new questionnaire as a re-personalization trigger.

b2: Debounce high-frequency updates.
fb2: Only process if more than five minutes passed since the last completion; keep the latest answers.

b3: Re-score old matches.
fb3: Do not delete old matches; re-score them with new evidence.

b4: Transition the feed gradually.
fb4: Avoid a sudden overnight change in the user's feed.

Pair 3.6 - Phrasing B

A: "Plan what happens when a user updates their questionnaire."

B: Debounce, keep newest answers, re-score instead of deleting, and shift personalization gradually.

## Pair 3.7 - Graceful degradation plan

A: Domain translations describe analytics failure, matching timeout, learning failure, and feed failure.

B:
b1: Detect branch failure or timeout.
fb1: Identify which branch failed without treating all onboarding as failed.

b2: Choose fallback.
fb2: Analytics uses industry defaults; matching shows check-back-soon or partial results; feed shows generic trending content.

b3: Preserve work for continuation.
fb3: Continue when delayed branch results become available.

b4: Keep user-facing language calm.
fb4: Explain the available experience, not the internal failure.

Pair 3.7 - Phrasing B

A: "Plan onboarding when one personalization branch fails."

B: Isolate the failed branch, show the best available experience, preserve recovery, and avoid hard failure.

## Pair 3.8 - Privacy plan

A: Domain translations describe confidential business details, private learning data, bilateral match visibility, k-anonymity, and opt-out.

B:
b1: Apply business profile privacy.
fb1: Keep confidential fields protected and support public versus connections-only fields.

b2: Apply match visibility.
fb2: Show match results only to matched users and do not reveal private details in reasons.

b3: Apply learning privacy.
fb3: Keep learning data private to the user and aggregate analytics anonymously.

b4: Apply opt-out and k-anonymity.
fb4: Respect matching opt-out and use minimum group size five for similar-business features.

Pair 3.8 - Phrasing B

A: "Plan privacy controls for personalization."

B: Protect business details, match results, learning data, analytics, opt-out, and similar-business grouping.
