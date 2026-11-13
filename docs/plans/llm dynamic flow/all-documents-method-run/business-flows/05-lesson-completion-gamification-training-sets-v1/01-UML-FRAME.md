# 01 - UML Frame

## Active Nodes

1. Domain splitter: receives a human prompt and returns all domain labels touched by it.
2. Domain translator array: one unit receives one prompt fragment plus one domain label and writes the fragment in that domain's precise language.
3. Scheme planner: receives all translated domain descriptions and returns ordered work pieces, parallel work pieces, wait states, recovery paths, and review points.
4. Domain processor array: one unit receives one planned work piece and returns the completed result for that piece.
5. Applier array: one unit receives one work piece, its transformation instruction, and the processed result, then returns action and outcome.
6. Result translator array: one unit receives one formal result, sees all results for consistency, and returns human-readable language.
7. Arbiter selector: receives formal results before formatting and chooses the review units required.
8. Arbiter units: review formal results and return pass, concern, or a reprocess instruction.
9. Formatter: receives approved translated fragments and returns one final answer.

## Array Nodes

Four nodes are arrays:

- Node 02 runs once per domain.
- Node 04 runs once per planned work piece.
- Node 05 runs once per planned work piece.
- Node 06 runs once per formal result.

This matters for Flow-05 because lesson completion splits into three parallel branches: gamification, learning-plan adaptation, and optional social distribution. Peer grading happens after distribution and can feed points and analytics back into the learning loop.

## Main Edges

### Edge 1 - Prompt to Domain Splitter

What moves: a human request about completed lesson answers, points, adaptive learning, social sharing, grading, privacy, or engagement.

What the target receives: the raw prompt.

What the target produces: an array of domain labels.

What breaks if missing: the flow cannot decide which specialist translation units need to run.

### Edge 2 - Domain Splitter to Domain Translator Array

What moves: one domain label and its matching prompt fragment.

What the target receives: one domain-shaped fragment at a time.

What the target produces: precise wording for that domain.

What breaks if missing: later planning may confuse points, learning adaptation, social sharing, peer grading, and privacy.

### Edge 3 - Domain Translator Array to Scheme Planner

What moves: the full set of translated domain statements.

What the target receives: all domain statements together.

What the target produces: a plan with the answer-storage step, three parallel branches, optional social branch skip, peer engagement after distribution, and review points.

What breaks if missing: the flow may wrongly require social sharing for private learners or may mark completion before gamification and learning adaptation have resolved.

### Edge 4 - Scheme Planner to Domain Processor Array

What moves: one planned work piece.

What the target receives: one piece with its domain context.

What the target produces: the completed result for that piece, including success and fallback outcomes.

What breaks if missing: the applier would not know how to handle duplicate submissions, point delays, invalid learning adaptations, or social opt-out.

### Edge 5 - Domain Processor Array to Applier Array

What moves: the processed result plus its transformation instruction.

What the target receives: a piece ready to act on.

What the target produces: the action taken and its outcome.

What breaks if missing: points, achievements, learning-plan changes, questionnaire posts, feed distribution, and peer-grading updates remain theoretical rather than applied.

### Edge 6 - Applier Array to Result Translator Array

What moves: formal results from applied pieces.

What the target receives: one formal result and all other formal results for consistency.

What the target produces: user-readable language.

What breaks if missing: the final answer would expose internal fragments instead of explaining what happened for the learner.

### Edge 7 - Applier Array to Arbiter Selector

What moves: the full formal result collection.

What the target receives: results before user-facing formatting.

What the target produces: selected review units.

What breaks if missing: privacy, point integrity, duplicate-submission, learning-plan safety, and grading abuse checks may not run.

### Edge 8 - Arbiter Selector to Arbiter Units

What moves: review responsibility and relevant result group.

What the target receives: a rule to check and the result it applies to.

What the target produces: pass, concern, or reprocess instruction.

What breaks if missing: the flow can detect a risk only if a review unit actually examines it.

### Edge 9 - Arbiter Units to Reprocessing

What moves: a rejected fragment and reason.

What the target receives: the exact fragment that must be reprocessed.

What the target produces: a corrected route through the flow.

What breaks if missing: a bad result can be identified but not repaired.

### Edge 10 - Approved Results to Formatter

What moves: approved translated fragments only.

What the target receives: natural-language fragments that passed review.

What the target produces: one combined answer for the intended audience.

What breaks if missing: the final answer may mix approved and rejected material or leave the user with disconnected fragments.

