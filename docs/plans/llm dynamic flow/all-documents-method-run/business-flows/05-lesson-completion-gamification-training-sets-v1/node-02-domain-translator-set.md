# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment plus one domain label from Node-01.
Runtime B is the same request in the exact language of that domain.

## Pair 2.1 - Questionnaire answer validation and storage

A: Fragment: "Validate the answers, score them, and store the attempt." Domain: questionnaire answer validation and storage.

B: The questionnaire attempt must be accepted only when all required answers are present, tied to the learner, lesson, question identifiers, timing, confidence, score, and metadata, and stored so duplicate submissions can return the original result.

Pair 2.1 - Phrasing B

A: Fragment: "Save the attempt and calculate the first score." Domain: questionnaire answer validation and storage.

B: The answer-storage domain must preserve the completed attempt, the raw correctness score, answer details, time spent, and the attempt identity needed to prevent double counting.

## Pair 2.2 - Gamification points

A: Fragment: "Give points for completion, correct answers, speed, streaks, and answer quality." Domain: gamification points.

B: The gamification domain calculates total reward from base completion points, per-correct-answer points, all-correct bonus, speed bonus, streak bonus, and quality bonus for detailed or insightful text answers.

Pair 2.2 - Phrasing B

A: Fragment: "Calculate the learner's reward from accuracy, completion, time, streak, and reflection quality." Domain: gamification points.

B: The point result must show every contributing reason for the award so the learner can understand how completion, correctness, pace, streak, and answer quality affected the total.

## Pair 2.3 - Levels and achievements

A: Fragment: "Level the user up and unlock earned achievements." Domain: levels and achievements.

B: The level domain compares the learner's new point total with level thresholds, emits a level-up when crossed, and unlocks achievements only when their criteria are met by actual history.

Pair 2.3 - Phrasing B

A: Fragment: "Trigger a celebration when the learner reaches a new level or earns a badge." Domain: levels and achievements.

B: Achievement output must include the badge name, rarity, reason, and triggering event, while level output must include previous level, new level, and next requirement.

## Pair 2.4 - Adaptive learning-plan adjustment

A: Fragment: "Use score, time, and wrong-answer patterns to adjust next lessons." Domain: adaptive learning-plan adjustment.

B: The learning-plan domain analyzes performance patterns and chooses curriculum changes: remedial content for score below sixty percent, skipping basic modules for score above ninety percent, pace slowing for very long completion time, pace speed-up for very fast completion, and targeted practice for repeated wrong patterns.

Pair 2.4 - Phrasing B

A: Fragment: "Change curriculum difficulty, pace, and next modules based on performance." Domain: adaptive learning-plan adjustment.

B: Learning adaptation must preserve required curriculum modules, make no more than three changes per adaptation, and respect the minimum lesson gap between adaptations.

## Pair 2.5 - Social questionnaire-post creation

A: Fragment: "Turn selected questionnaire answers and scores into a social post." Domain: social questionnaire-post creation.

B: The social-post domain creates a questionnaire post from only the answers and scores allowed for sharing, carries social settings, and starts distribution only when the learner has consented to share.

Pair 2.5 - Phrasing B

A: Fragment: "Make a shareable learning post from the user's answers." Domain: social questionnaire-post creation.

B: The questionnaire post represents the learner's completed lesson in feed-ready form, but it must not expose private answers or unapproved scores.

## Pair 2.6 - Learning-specific audience targeting

A: Fragment: "Share with friends, learning groups, similar learners, and relevant businesses using the defined audience weights." Domain: learning-specific audience targeting.

B: The audience domain targets friends at thirty percent, learning-focused groups at twenty percent, similar learners at thirty percent with similarity above 0.6, and matched businesses at twenty percent with relevance above 0.7, while respecting the stated maximums.

Pair 2.6 - Phrasing B

A: Fragment: "Find the right learning audience using friends, groups, similar learners, and business relevance." Domain: learning-specific audience targeting.

B: Learning-post distribution must use a learning-centered audience mix rather than a generic feed audience, with explicit limits for friends, groups, similar learners, and businesses.

## Pair 2.7 - Peer grading and comments

A: Fragment: "Let peers grade answers on accuracy, depth, clarity, and creativity and leave categorized comments." Domain: peer grading and categorized comments.

B: The grading domain receives peer scores on four criteria from one to five and comment categories of support, question, challenge, or insight, then returns social feedback events that can affect points and analytics.

Pair 2.7 - Phrasing B

A: Fragment: "Allow other users to review answer quality and comment in learning-specific categories." Domain: peer grading and categorized comments.

B: Peer engagement must distinguish numerical grading from categorized commentary, because grades affect scoring and comments drive notification and learning conversation.

## Pair 2.8 - Privacy and consent

A: Fragment: "If the learner opts out of sharing, skip the social post entirely." Domain: privacy and consent.

B: The privacy domain requires opt-in sharing per question and disables the social branch entirely when learning activity is private, while gamification and learning adaptation continue normally.

Pair 2.8 - Phrasing B

A: Fragment: "Run points and learning updates, but do not create a feed post without consent." Domain: privacy and consent.

B: Consent controls the social branch only; it must not block points, scoring, or curriculum adjustment, and it must prevent answer exposure in feeds.

## Pair 2.9 - Point integrity and anti-farming

A: Fragment: "Do not let the client submit point values or repeat the questionnaire to farm rewards." Domain: point integrity and anti-farming.

B: The integrity domain requires trusted point calculation, duplicate-submission idempotency, one questionnaire completion per lesson per hour, validated achievement unlocks, and anomaly detection for suspicious point spikes.

Pair 2.9 - Phrasing B

A: Fragment: "Calculate points on the trusted side, rate-limit completions, and make duplicate submissions return the same result." Domain: point integrity and anti-farming.

B: Rewards must be based on stored answer evidence and event history, not on user-submitted point numbers or repeated clicks.

## Pair 2.10 - Notifications and analytics

A: Fragment: "Measure completion rate, points per session, streak lengths, level-up engagement, peer grading, and learning-plan impact." Domain: notifications and analytics.

B: The analytics domain records lesson completion, average points, streak distribution, level-up reactions, questionnaire-post engagement, peer grading participation, and how learning-plan changes affect future completion.

Pair 2.10 - Phrasing B

A: Fragment: "Report how completion, rewards, social sharing, and adapted plans affect engagement and learning." Domain: notifications and analytics.

B: Notification and measurement outputs must show rewards and milestones to the learner while collecting evidence about learning progress and community engagement.

