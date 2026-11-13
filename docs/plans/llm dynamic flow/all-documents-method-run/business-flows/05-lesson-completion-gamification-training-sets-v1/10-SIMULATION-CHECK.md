# 10 - Simulation Check

## Source Being Simulated

`C:\Users\lubao\Documents\xiigen\business flows\05-lesson-completion-gamification.md`

The simulation checks whether these node sets can recreate the lesson-completion flow without using the source document as runtime input.

## Test Prompt 1 - Full Lesson Completion

Runtime user prompt:

"A learner submits a completed lesson questionnaire. Save and score it, award points and achievements, adjust the learning plan, create a social questionnaire post if sharing is allowed, distribute it to the learning audience, and record peer grading and analytics."

Expected behavior:

- Node-01 returns questionnaire storage, raw scoring, gamification, levels, learning adaptation, social post creation, learning audience targeting, peer grading, privacy, notifications, and analytics.
- Node-02 writes precise domain statements for each label.
- Node-03 stores the attempt first and then runs gamification, learning adaptation, and consent-controlled social sharing in parallel.
- Node-04 processes success and fallback outcomes for each branch.
- Node-05 applies points, milestones, learning updates, social sharing, audience distribution, peer feedback, and failure handling.
- Node-07 selects duplicate, point integrity, adaptation, consent, grading, branch-completion, notification, and analytics reviews.
- Node-08 sends back any duplicate points, invalid adaptation, unconsented sharing, or abusive grading.
- Node-09 returns one answer that explains rewards, learning updates, optional sharing, feedback, and metrics.

Pass condition:

The simulated flow must keep the three branches separate and must not require social sharing when privacy disables it.

## Test Prompt 2 - Social Opt-Out

Runtime user prompt:

"The learner completed the questionnaire but opted out of sharing answers. Still award points and update the learning plan."

Expected behavior:

- Node-01 returns questionnaire storage, gamification, learning adaptation, privacy and consent.
- Node-03 skips social post creation and feed distribution.
- Node-05 applies rewards and learning adaptation only.
- Node-08 rejects any questionnaire post or feed distribution.
- Node-09 says answers stayed private while non-social work completed.

Pass condition:

No final answer may say a questionnaire post was created.

## Test Prompt 3 - Duplicate Submit

Runtime user prompt:

"The user clicked submit twice for the same questionnaire attempt."

Expected behavior:

- Node-01 returns duplicate submission control and point integrity.
- Node-03 checks the attempt identity before scoring.
- Node-05 returns the cached result and does not award points again.
- Node-07 selects duplicate and reward integrity reviews.
- Node-08 rejects a second point award, second adaptation, or second social post.

Pass condition:

The simulation must produce one learner result, not two.

## Test Prompt 4 - Invalid Learning Adaptation

Runtime user prompt:

"The learning model suggests removing required modules and making five changes after this lesson."

Expected behavior:

- Node-01 returns adaptive learning-plan adjustment and curriculum safety.
- Node-03 validates the adaptation before applying it.
- Node-05 leaves the plan unchanged when the adaptation violates rules.
- Node-08 rejects removal of required modules and more than three changes.
- Node-09 explains that the plan stayed unchanged because the suggestion was unsafe.

Pass condition:

The simulation must not apply an invalid curriculum change.

## Test Prompt 5 - Peer Grading Spam

Runtime user prompt:

"One user graded one hundred answers in one minute and the system tried to award social points."

Expected behavior:

- Node-01 returns peer grading, social points, and anti-farming.
- Node-03 applies rate-limit and anomaly review.
- Node-05 blocks abusive grading from reward farming.
- Node-07 selects grading-rate, pseudonymity, and social-point reviews.
- Node-08 sends the grading fragment back for reprocessing.
- Node-09 explains only valid peer feedback counts.

Pass condition:

The simulated result must not award social points for abusive grading.

## Missing-Training Detector

If the simulation combines gamification, learning adaptation, and social sharing into one undifferentiated step, add Node-03 branch pairs.

If points can be awarded before answers are stored, add Node-04 and Node-05 storage-before-reward pairs.

If social opt-out still creates feed content, add Node-02, Node-03, Node-05, and Node-08 privacy pairs.

If achievements unlock without criteria, add Node-04 and Node-08 milestone pairs.

If invalid curriculum changes are applied, add Node-04, Node-05, and Node-08 adaptation-safety pairs.

If peer grades reveal identity before three graders, add Node-07 and Node-08 pseudonymity pairs.

If completion is claimed while gamification or learning branch is unresolved, add Node-07 and Node-08 branch-completion pairs.

