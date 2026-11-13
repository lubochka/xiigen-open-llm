# Node-04 - Domain Processor Set

## Purpose

Runtime A is one planned work piece with domain context.
Runtime B is the completed result for that piece.

## Pair 4.1 - Store questionnaire attempt

A: Work piece: validate, score, and store the completed questionnaire. Domain context: questionnaire answer validation and storage.

B: Processed result: the attempt contains learner, lesson, questionnaire, answer list, correctness, time spent, confidence, raw score, metadata, and attempt identity. If the same attempt already exists, the stored result is returned and no new downstream reward or sharing work is allowed.

Pair 4.1 - Phrasing B

A: Work piece: prepare the trusted lesson-completion record. Domain context: answer storage.

B: Processed result: one authoritative completion result exists for the attempt, and it is safe for gamification, learning adaptation, and optional social sharing to read it.

## Pair 4.2 - Calculate points

A: Work piece: calculate points from completion, correct answers, all-correct bonus, speed, streak, answer quality, and social engagement where applicable. Domain context: gamification points.

B: Processed result: the total point award lists each component separately. It includes base completion points, per-correct-answer points, perfect-score bonus, time bonus, streak bonus, and quality bonus. It rejects point values that came from the client rather than trusted calculation.

Pair 4.2 - Phrasing B

A: Work piece: produce the gamification reward. Domain context: points and streaks.

B: Processed result: the reward explains why the learner received the number of points shown and preserves enough detail to audit inflation or repeated awards.

## Pair 4.3 - Check level and achievement

A: Work piece: decide whether the point result changes the learner's level or unlocks achievements. Domain context: levels and achievements.

B: Processed result: a level-up exists only when the new total crosses the next threshold. An achievement unlock exists only when the required history is satisfied and the achievement was not already unlocked unless the source says old unlocks are grandfathered.

Pair 4.3 - Phrasing B

A: Work piece: process milestones after points are awarded. Domain context: milestones.

B: Processed result: milestone output includes previous level, new level, next requirement, and any achievement with name, rarity, badge, description, and trigger reason.

## Pair 4.4 - Adapt learning plan

A: Work piece: adjust curriculum using score, time, wrong-answer patterns, and constraints. Domain context: adaptive learning-plan adjustment.

B: Processed result: the adaptation proposes remedial content for score below sixty percent, skipping basic modules for score above ninety percent, slower pace for very long completion time, faster pace for very short completion time, or targeted practice for wrong patterns. It rejects changes that remove required modules, exceed three changes, or occur too soon after the last adaptation.

Pair 4.4 - Phrasing B

A: Work piece: update the learner's future modules safely. Domain context: learning adaptation.

B: Processed result: the plan either changes within allowed limits or remains unchanged with a reason. The learner is not pushed into an invalid curriculum.

## Pair 4.5 - Create questionnaire post

A: Work piece: create a social post from selected questionnaire answers. Domain context: social questionnaire-post creation and privacy.

B: Processed result: a questionnaire post exists only when sharing consent allows it. It contains selected answers, score presentation allowed by privacy, social settings, and any badge such as in-progress. If the learner is private or opts out, the processed result is social branch skipped.

Pair 4.5 - Phrasing B

A: Work piece: prepare shareable learning content. Domain context: social sharing.

B: Processed result: the post is feed-ready only with approved content. Private answers remain private, and skipped sharing is a valid outcome.

## Pair 4.6 - Target learning audience

A: Work piece: identify recipients for the questionnaire post. Domain context: learning-specific audience targeting.

B: Processed result: the audience mix contains friends up to one hundred active in the last thirty days, learning-focused groups up to ten groups, similar learners above similarity 0.6 up to two hundred, and businesses above relevance 0.7 up to fifty. Each recipient keeps source and relevance information for ranking.

Pair 4.6 - Phrasing B

A: Work piece: find who should see learning content. Domain context: audience targeting.

B: Processed result: recipients are selected from learning-relevant paths with weights and maximums preserved, not from an unrestricted general feed audience.

## Pair 4.7 - Process peer grades and comments

A: Work piece: accept peer grades and comments after the questionnaire post is distributed. Domain context: peer grading and categorized comments.

B: Processed result: grades contain accuracy, depth, clarity, and creativity values from one to five. Public grade display waits for at least three unique graders. Comments are categorized as support, question, challenge, or insight. Accepted grades and comments may produce social points and notifications.

Pair 4.7 - Phrasing B

A: Work piece: turn peer feedback into valid learning engagement. Domain context: grading.

B: Processed result: feedback is structured, rate-limited, identity-protected until threshold, and ready for social rewards and analytics.

## Pair 4.8 - Record analytics and notifications

A: Work piece: record engagement and notify the learner about points, levels, achievements, grades, comments, and learning changes. Domain context: notifications and analytics.

B: Processed result: analytics includes completion rate, points per session, streak distribution, level-up engagement, questionnaire-post engagement, peer grading participation, and adaptation impact. Notifications are separated by purpose so achievement alerts do not conflict with normal system alerts.

Pair 4.8 - Phrasing B

A: Work piece: measure and communicate the flow outcome. Domain context: notifications and analytics.

B: Processed result: the learner receives the relevant progress messages, and the platform records learning and engagement measurements without exposing private answer content.

