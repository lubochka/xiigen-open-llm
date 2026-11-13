# Node-05 - Applier Set

## Purpose

Runtime A is one work piece, its transformation instruction, and the processed result from Node-04.
Runtime B is the concrete action and outcome.

## Pair 5.1 - Store answers before rewards

A: Work piece: completed questionnaire attempt. Transformation instruction: store and score before rewards, adaptation, or sharing. Processed result: one valid attempt record exists.

B: Action outcome: the answer attempt is stored with raw score and attempt identity first. Only after that stored result exists do gamification, learning adaptation, and consent-controlled social sharing begin.

Pair 5.1 - Phrasing B

A: Work piece: answer submission. Transformation instruction: do not award from an unstored attempt. Processed result: trusted answer record exists.

B: Action outcome: the flow uses the stored attempt as the source for every reward, plan change, and optional social post.

## Pair 5.2 - Award gamification points

A: Work piece: point calculation. Transformation instruction: calculate trusted points and reject submitted point values. Processed result: component-level point award is ready.

B: Action outcome: the learner receives points with separate reasons for completion, correct answers, all-correct bonus, speed bonus, streak bonus, answer quality, and valid social engagement where applicable.

Pair 5.2 - Phrasing B

A: Work piece: reward learner. Transformation instruction: show why points were earned. Processed result: audited point components are available.

B: Action outcome: the point award is applied to the learner's progress and can be explained without trusting any client-provided number.

## Pair 5.3 - Apply level-up and achievement

A: Work piece: milestone check. Transformation instruction: unlock only when thresholds and criteria are met. Processed result: level-up and achievement decisions are known.

B: Action outcome: if the learner crossed a level threshold, the new level is applied and a level notification is prepared. If an achievement criterion is met for the first time, the badge is unlocked and the achievement notification is prepared.

Pair 5.3 - Phrasing B

A: Work piece: apply milestone results. Transformation instruction: avoid duplicate unlocks. Processed result: milestone eligibility is confirmed.

B: Action outcome: the learner sees only earned level changes and newly valid achievements, with old unlocks preserved when criteria change later.

## Pair 5.4 - Apply learning adaptation

A: Work piece: learning-plan adaptation. Transformation instruction: apply only validated changes. Processed result: suggested plan changes pass or fail constraints.

B: Action outcome: when valid, the learner's curriculum is adjusted for remedial content, basic-module skip, pace change, or targeted practice. When invalid, the plan remains unchanged and the failed adaptation is recorded.

Pair 5.4 - Phrasing B

A: Work piece: update future lessons. Transformation instruction: protect required curriculum and change limits. Processed result: adaptation result is ready.

B: Action outcome: the learner's plan changes only within the allowed boundaries: no removed required modules, no more than three changes, and no adaptation too close to the previous one.

## Pair 5.5 - Apply social opt-in or opt-out

A: Work piece: create questionnaire post. Transformation instruction: social sharing requires consent. Processed result: consent is present or absent.

B: Action outcome: if sharing is allowed, a questionnaire post is created from selected answers and approved score presentation. If sharing is not allowed, the social branch is skipped entirely and answers remain private.

Pair 5.5 - Phrasing B

A: Work piece: publish learning content. Transformation instruction: never expose private answers. Processed result: sharing permission is known.

B: Action outcome: the flow either creates feed-ready learning content from allowed answer material or records a deliberate social-skip result.

## Pair 5.6 - Apply learning audience distribution

A: Work piece: distribute questionnaire post. Transformation instruction: use learning-specific audience weights and limits. Processed result: target recipients are grouped by friends, groups, similar learners, and businesses.

B: Action outcome: the post is ranked and distributed to the approved learning audience: friends, learning groups, similar learners, and relevant businesses, preserving source, relevance, maximum counts, and audience weights.

Pair 5.6 - Phrasing B

A: Work piece: place learning post in feeds. Transformation instruction: do not use a generic unrestricted feed audience. Processed result: learning audience is ready.

B: Action outcome: the feed distribution path uses the learning audience mix from the source document and records distribution metrics for the questionnaire post.

## Pair 5.7 - Apply peer grading

A: Work piece: peer grades and comments. Transformation instruction: accept valid feedback, protect identities until threshold, and award only allowed social points. Processed result: feedback passed or failed rate and threshold checks.

B: Action outcome: valid grades are aggregated across accuracy, depth, clarity, and creativity. Comments are stored by category. Social points are awarded for allowed grading outcomes, and grader identity stays hidden until at least three unique graders exist.

Pair 5.7 - Phrasing B

A: Work piece: process community feedback. Transformation instruction: block grading spam and preserve pseudonymity. Processed result: feedback validity is known.

B: Action outcome: peer feedback becomes structured learning engagement, while excessive or suspicious grading is limited and excluded from reward farming.

## Pair 5.8 - Apply failure handling

A: Work piece: handle branch failure. Transformation instruction: preserve the safe default for each branch. Processed result: gamification failure, learning adaptation failure, or social distribution failure occurred.

B: Action outcome: if gamification fails, points are delayed and retried, and the learner sees them on a later refresh. If learning adaptation fails, the plan remains unchanged. If social distribution fails, no social post is created, while learning completion still remains tracked.

Pair 5.8 - Phrasing B

A: Work piece: continue after non-critical failure. Transformation instruction: keep the learner's completed lesson valid. Processed result: one branch could not complete.

B: Action outcome: the flow does not erase the lesson completion. It uses the defined fallback for the failing branch and keeps the successful branches intact.

