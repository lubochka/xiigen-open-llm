# Node-08 - Arbiter Units Set

## Purpose

Runtime A is one selected review responsibility with the formal result or result group to examine.
Runtime B is pass, concern, or a reprocess instruction.

## Pair 8.1 - Duplicate submission

A: Review responsibility: duplicate-submission arbiter. Formal result: the same questionnaireId, userId, and attemptNumber produced two point awards.

B: Reprocess. The source requires duplicate submissions to return the cached result. Send the questionnaire-attempt fragment back and require the second submission to reuse the original outcome without awarding points again.

Pair 8.1 - Phrasing B

A: Review responsibility: idempotency review. Formal result: double-clicking submit created a second social post.

B: Reprocess. A repeated attempt must not create another downstream branch. Send the submission-control fragment back and require the existing result to be returned.

## Pair 8.2 - Point integrity

A: Review responsibility: trusted-calculation arbiter. Formal result: points were accepted from the client.

B: Reprocess. The source says point values must be calculated on the trusted side only. Send the point-calculation fragment back and require points to be derived from stored answer evidence.

Pair 8.2 - Phrasing B

A: Review responsibility: point-breakdown review. Formal result: total points are shown without component reasons.

B: Concern. The award must show completion, correct-answer, perfect-score, speed, streak, and quality components where they apply. Require the point result to include its breakdown.

## Pair 8.3 - Achievement criteria

A: Review responsibility: milestone-criteria arbiter. Formal result: Speed Demon achievement unlocked even though the learner did not finish under five minutes.

B: Reprocess. Achievement unlocks must match actual event history. Send the achievement fragment back and require the trigger condition to be verified.

Pair 8.3 - Phrasing B

A: Review responsibility: duplicate-achievement review. Formal result: an already-unlocked achievement is unlocked again after criteria changed.

B: Concern. Existing unlocks should be grandfathered, but the same unlock should not be duplicated. Require the result to preserve the old unlock without adding another one.

## Pair 8.4 - Learning adaptation safety

A: Review responsibility: required-module arbiter. Formal result: learning adaptation removes required curriculum modules.

B: Reprocess. The source says validation must reject adaptations that remove required modules. Send the learning-plan fragment back and require the plan to remain unchanged or choose a valid adaptation.

Pair 8.4 - Phrasing B

A: Review responsibility: max-change review. Formal result: adaptation makes five changes in one run.

B: Reprocess. The source limits each adaptation to three changes. Send the adaptation fragment back and require the change set to be reduced or rejected.

## Pair 8.5 - Consent and private answers

A: Review responsibility: consent arbiter. Formal result: learner opted out of social sharing, but a questionnaire post was created.

B: Reprocess. Social sharing must be skipped entirely when the learner opts out or learning activity is private. Send the social-post fragment back and require no feed post to be created.

Pair 8.5 - Phrasing B

A: Review responsibility: private-answer review. Formal result: unapproved questionnaire answers appear in the social post.

B: Reprocess. Only selected and approved answer content may be shared. Send the questionnaire-post fragment back and require private answers to be removed.

## Pair 8.6 - Audience targeting

A: Review responsibility: learning-audience arbiter. Formal result: the questionnaire post is distributed with no limits on similar learners or businesses.

B: Reprocess. The source defines maximums and thresholds for learning distribution. Send the audience-targeting fragment back and require friends, groups, similar learners, and businesses to use their weights, limits, and eligibility thresholds.

Pair 8.6 - Phrasing B

A: Review responsibility: audience mix review. Formal result: the post goes only to businesses and ignores friends, groups, and similar learners.

B: Concern. The learning audience mix is incomplete. Require the result to include the missing audience paths or state why they are unavailable.

## Pair 8.7 - Peer grading abuse

A: Review responsibility: grading-rate arbiter. Formal result: one user graded one hundred answers in one minute and social points were awarded.

B: Reprocess. The source limits grading to twenty per hour and flags anomalies. Send the grading fragment back and require abusive grades to be limited and excluded from reward farming.

Pair 8.7 - Phrasing B

A: Review responsibility: pseudonymity review. Formal result: individual grader identity is revealed before three unique graders exist.

B: Reprocess. The source says grades are pseudonymous until the public threshold is reached. Send the grading-display fragment back and keep individual identity hidden.

## Pair 8.8 - Completion consistency

A: Review responsibility: branch-completion arbiter. Formal result: the flow is marked complete but learning adaptation has neither succeeded nor failed safely.

B: Reprocess. Completion requires the gamification and learning branches to resolve. Send the completion fragment back and require the learning branch to show applied adaptation, unchanged-plan fallback, or delayed failure state.

Pair 8.8 - Phrasing B

A: Review responsibility: notification-overlap review. Formal result: level-up, achievement, and system alerts overlap without separation.

B: Concern. The source calls for notification guardrails. Require the final notification result to separate milestone celebrations from standard system alerts.

