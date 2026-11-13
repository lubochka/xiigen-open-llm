# Node-06 - Result Translator Set

## Purpose

Runtime A is one formal result from Node-05, with all formal results visible for consistency.
Runtime B is the same result in user-readable language.

## Pair 6.1 - Attempt stored

A: Formal result: questionnaire attempt stored with learner, lesson, answers, score, timing, metadata, and attempt identity; downstream branches started after storage.

B: Your completed lesson answers were saved and scored first. After that, the platform used the saved result to calculate rewards, update learning, and handle any approved sharing.

Pair 6.1 - Phrasing B

A: Formal result: one trusted lesson-completion record exists before rewards and sharing.

B: The lesson attempt is now recorded, so the rest of the flow can work from a trusted result.

## Pair 6.2 - Points awarded

A: Formal result: points awarded with completion, correct-answer, all-correct, speed, streak, and quality components.

B: Points were added to the learner's progress, with each part of the award coming from completion, correctness, speed, streak, and answer quality.

Pair 6.2 - Phrasing B

A: Formal result: trusted point calculation applied and component reasons retained.

B: The learner received an explainable point award based on the completed questionnaire and valid bonus conditions.

## Pair 6.3 - Level or achievement unlocked

A: Formal result: user leveled up or achievement unlocked with previous level, new level, badge, rarity, and trigger reason.

B: The learner reached a new milestone. The result shows the level change or earned achievement, why it was earned, and what comes next.

Pair 6.3 - Phrasing B

A: Formal result: milestone criteria satisfied and notification prepared.

B: A valid level-up or achievement was recognized and is ready to be shown to the learner.

## Pair 6.4 - Learning plan adapted

A: Formal result: curriculum adjusted for remedial content, skipped basic module, pace change, or targeted practice within constraints.

B: The learner's plan was updated based on performance. The changes stay within the allowed limits and are meant to match the learner's current pace and needs.

Pair 6.4 - Phrasing B

A: Formal result: validated learning adaptation applied.

B: Future lessons were adjusted safely according to the learner's score, timing, and answer patterns.

## Pair 6.5 - Learning plan unchanged

A: Formal result: suggested adaptation rejected because it removed required modules, exceeded change limits, or violated lesson-spacing rules.

B: The learning plan was left unchanged because the suggested update did not pass the safety rules for curriculum changes.

Pair 6.5 - Phrasing B

A: Formal result: invalid adaptation blocked.

B: No curriculum change was applied, so the learner stays on the current plan until a valid adaptation is available.

## Pair 6.6 - Social post created

A: Formal result: questionnaire post created from selected approved answers and scores with sharing consent.

B: A learning post was created from the answers the learner allowed to share. It can now be distributed to the learning audience.

Pair 6.6 - Phrasing B

A: Formal result: consented questionnaire post exists.

B: The learner's selected answers were turned into shareable learning content without exposing unapproved material.

## Pair 6.7 - Social branch skipped

A: Formal result: social sharing disabled because learner opted out or learning activity is private; gamification and learning adaptation still ran.

B: The social post was skipped because the learner did not choose to share. Points and learning-plan updates still continued normally.

Pair 6.7 - Phrasing B

A: Formal result: privacy setting disabled feed sharing only.

B: The learner's answers stayed private, while the non-social parts of the completion flow still completed.

## Pair 6.8 - Peer feedback processed

A: Formal result: valid grades and categorized comments processed; social points awarded where allowed; public grade display waits for three unique graders.

B: Peer feedback was accepted where it passed the rules. Grades and comments can support learning engagement, and individual grader identity stays protected until enough graders participate.

Pair 6.8 - Phrasing B

A: Formal result: grading and comments handled with pseudonymity and rate limits.

B: The feedback loop is active, but it still protects identities and blocks abusive grading.

## Pair 6.9 - Failure fallback applied

A: Formal result: gamification failure delayed points for retry, learning failure left plan unchanged, or social distribution failure skipped the social post while lesson completion remained tracked.

B: One branch could not finish, so the flow used its safe fallback. The completed lesson remains recorded, and successful branches were not undone.

Pair 6.9 - Phrasing B

A: Formal result: branch failure handled without losing lesson completion.

B: The platform kept the learner's completion result and used the defined fallback for the part that failed.

