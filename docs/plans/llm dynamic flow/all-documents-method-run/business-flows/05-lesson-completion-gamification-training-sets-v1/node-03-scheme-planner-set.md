# Node-03 - Scheme Planner Set

## Purpose

Runtime A is the full set of domain translations from Node-02.
Runtime B is a plan with ordered work, parallel work, wait states, recovery paths, and review points.

## Pair 3.1 - Full lesson completion plan

A: The translated domains say that a learner completes a questionnaire, answers are stored and scored, gamification awards points and milestones, learning plan adapts, social post may be created, learning audience is targeted, peers can grade and comment, and analytics records the outcome.

B: Plan: first validate, score, and store the questionnaire attempt. After storage succeeds, start three branches in parallel: gamification, learning-plan adaptation, and social sharing if consent allows. The gamification branch calculates points, checks level-up, checks achievements, and sends reward notifications. The learning branch analyzes score, time, and answer patterns, then adjusts curriculum within the stated constraints. The social branch creates a questionnaire post from approved answer content, targets friends, learning groups, similar learners, and businesses, ranks the audience, distributes the post, and records social metrics. After distribution, peer grades and comments may award social points and analytics signals. Review duplicate submission, point integrity, adaptation safety, privacy, grading spam, notification conflicts, and completion consistency.

Pair 3.1 - Phrasing B

A: The translated descriptions require stored answers, rewards, adapted curriculum, optional sharing, peer feedback, and measurement.

B: Plan: preserve answer storage as the first required step. Then run rewards, learning changes, and consent-controlled social sharing side by side. Social engagement happens after the questionnaire post is visible. Completion is not valid until the gamification and learning branches have resolved, while social distribution can be skipped when privacy requires it.

## Pair 3.2 - Perfect score with speed bonus

A: The translated domains say all answers are correct, the learner completed under five minutes, a first-time speed achievement may unlock, and the learning plan should skip the next basic module.

B: Plan: store the attempt. In the gamification branch, award completion points, per-correct-answer points, all-correct bonus, under-five-minute speed bonus, and speed achievement if not already unlocked. In the learning branch, treat the score as above ninety percent and prepare a basic-module skip if allowed by curriculum constraints. In the social branch, create the questionnaire post only if sharing is allowed. Review point calculation, one-time achievement unlock, and required curriculum preservation.

Pair 3.2 - Phrasing B

A: The translated descriptions describe a fast perfect attempt that should reward and accelerate learning.

B: Plan: the reward branch calculates the full high-performance award and possible badge. The learning branch checks whether the learner can safely move past basic content. Social sharing remains governed by consent, not by the perfect score.

## Pair 3.3 - Struggling learner

A: The translated domains say the score is below sixty percent, the learner still receives completion points, the learning plan adds remedial content, the social post should show an in-progress badge rather than full scores, and encouragement is sent.

B: Plan: store the attempt and raw score. Award base completion points even though correctness is low. In learning adaptation, add remedial content and possibly targeted practice. If social sharing is enabled, create a limited questionnaire post marked in progress rather than exposing full scores. Send encouragement. Review privacy, tone, and adaptation limits.

Pair 3.3 - Phrasing B

A: The translated descriptions describe low performance that should support the learner rather than punish them.

B: Plan: preserve motivation with completion points, adapt the curriculum toward support, avoid exposing sensitive low scores in social content, and notify the learner in an encouraging way.

## Pair 3.4 - Thirty-day streak

A: The translated domains say a thirty-day streak earns fifty bonus points, a legendary Dedicated Learner achievement, a celebration post, and notifications to connections.

B: Plan: store the completed attempt in the learner's local streak day. Calculate streak bonus and achievement eligibility. If the thirty-day achievement is newly earned, unlock it and prepare celebration notification. If social sharing is allowed, create a streak celebration post and distribute to eligible connections. Review timezone handling, one-time achievement unlock, and social consent.

Pair 3.4 - Phrasing B

A: The translated descriptions describe a streak milestone that should award points, unlock a rare badge, and optionally share the celebration.

B: Plan: confirm the streak using the learner's timezone, award the streak bonus, unlock the legendary achievement only once, and create celebration sharing only when the learner's privacy settings allow it.

## Pair 3.5 - Social opt-out

A: The translated domains say the learner opts out of social sharing, so gamification and learning branches execute normally and answers remain private.

B: Plan: store and score the questionnaire. Run gamification and learning-plan adaptation. Do not create a questionnaire post. Do not target audiences. Do not distribute answers to feeds. Analytics may record private learning progress without exposing answer content. Review consent enforcement before final formatting.

Pair 3.5 - Phrasing B

A: The translated descriptions say private learning still earns points and adapts the plan, but no feed content is created.

B: Plan: social sharing is a removable branch. The rest of the flow remains active: rewards and curriculum changes continue, while answer publication and peer grading are skipped.

## Pair 3.6 - Duplicate submission

A: The translated domains say the user clicked submit twice for the same questionnaire attempt, and the idempotency key is questionnaireId plus userId plus attemptNumber.

B: Plan: check the attempt identity before scoring. If the attempt was already processed, return the cached result and do not award new points, unlock new achievements, adapt the plan again, or create another social post. Review duplicate-submission control and point integrity.

Pair 3.6 - Phrasing B

A: The translated descriptions say repeated clicking must not create repeated rewards.

B: Plan: treat the second submission as the same attempt, not as a new lesson completion. The already-created result is reused, and downstream branches do not run a second time.

## Pair 3.7 - Invalid learning adaptation

A: The translated domains say the learning model returns an adaptation that removes required curriculum modules or exceeds three changes.

B: Plan: store and score the attempt, run gamification normally, then validate the learning adaptation before applying it. Reject changes that remove required modules, exceed three changes, or violate the lesson-gap rule. Keep the existing plan when validation fails and record that learning adaptation was not applied.

Pair 3.7 - Phrasing B

A: The translated descriptions say curriculum suggestions must be checked before they affect the learner's plan.

B: Plan: adaptation is not accepted just because it was suggested. It must pass required-module, change-count, and lesson-spacing rules before the plan changes.

## Pair 3.8 - Peer grading spam

A: The translated domains say one user grades one hundred answers in a minute, while the rule allows twenty grades per hour and flags suspicious patterns.

B: Plan: accept only grading activity within the rate limit. Flag the pattern for anomaly review. Do not award social points for disallowed grading. Keep valid grades aggregated, preserve pseudonymity until the public threshold is reached, and record analytics separately for accepted and rejected grading activity.

Pair 3.8 - Phrasing B

A: The translated descriptions describe suspicious peer grading that could inflate points or distort feedback.

B: Plan: grading remains useful only when controlled. The flow limits the grader, blocks or flags abusive activity, protects identities until enough graders exist, and prevents social rewards from being farmed.

