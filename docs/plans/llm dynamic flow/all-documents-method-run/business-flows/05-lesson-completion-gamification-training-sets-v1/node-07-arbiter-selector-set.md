# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the full collection of formal results R1...Rn plus flow context.
Runtime B is the list of review units that must fire before formatting.

## Pair 7.1 - Duplicate attempt

A: Formal results show the same questionnaireId, userId, and attemptNumber submitted twice with two point awards.

B: Select duplicate-submission arbiter, point-integrity arbiter, and downstream-repeat arbiter.

Pair 7.1 - Phrasing B

A: Formal results include repeated completion for the same learner, lesson, and attempt identity.

B: Select idempotency review, duplicate-reward review, and repeated-social-post review.

## Pair 7.2 - Point calculation

A: Formal results show a point award derived from submitted point values or missing component reasons.

B: Select trusted-calculation arbiter, point-breakdown arbiter, and point-inflation arbiter.

Pair 7.2 - Phrasing B

A: Formal results award points without showing completion, correctness, speed, streak, and quality components.

B: Select reward-integrity review and component-completeness review.

## Pair 7.3 - Achievement unlock

A: Formal results show an achievement unlock or level-up.

B: Select milestone-criteria arbiter, duplicate-achievement arbiter, and notification-conflict arbiter.

Pair 7.3 - Phrasing B

A: Formal results include a badge, rarity, or level change that depends on event history.

B: Select achievement-history review, level-threshold review, and user-alert review.

## Pair 7.4 - Learning adaptation

A: Formal results show curriculum changes after performance analysis.

B: Select curriculum-safety arbiter, max-change arbiter, required-module arbiter, and lesson-spacing arbiter.

Pair 7.4 - Phrasing B

A: Formal results adjust difficulty, pace, content type, or next modules.

B: Select learning-plan validation review and adaptation-limit review.

## Pair 7.5 - Social sharing

A: Formal results show questionnaire post creation or feed distribution from answers.

B: Select consent arbiter, private-answer arbiter, social-settings arbiter, and audience-targeting arbiter.

Pair 7.5 - Phrasing B

A: Formal results include selected answers, scores, and social distribution.

B: Select sharing-permission review, answer-exposure review, and learning-audience review.

## Pair 7.6 - Peer grading

A: Formal results show grades, comments, social points, or public grade display.

B: Select grading-rate arbiter, pseudonymity arbiter, unique-grader-threshold arbiter, and social-point arbiter.

Pair 7.6 - Phrasing B

A: Formal results include peer scores on accuracy, depth, clarity, creativity, or categorized comments.

B: Select peer-feedback review, identity-protection review, and reward-from-grading review.

## Pair 7.7 - Branch completion

A: Formal results claim the flow is complete while gamification or learning-plan branch is delayed, failed, or missing.

B: Select branch-completion arbiter, safe-fallback arbiter, and completion-state arbiter.

Pair 7.7 - Phrasing B

A: Formal results include completion status but not both gamification and learning branch outcomes.

B: Select parallel-branch consistency review and flow-completion review.

## Pair 7.8 - Analytics and notification

A: Formal results show notifications and analytics events for points, achievements, grades, and learning changes.

B: Select analytics-coverage arbiter, notification-overlap arbiter, and privacy-preserving-metrics arbiter.

Pair 7.8 - Phrasing B

A: Formal results include measurement and user messages after the lesson flow.

B: Select metrics completeness review, alert-conflict review, and private-data-in-analytics review.

