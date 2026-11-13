# 01 - UML Frame

## Active Nodes

1. Domain splitter: receives one human prompt and returns all domain labels touched by that prompt.
2. Domain translator array: one translator receives one prompt fragment and one domain label, then writes that fragment in the language of that domain.
3. Scheme planner: receives all translated domain descriptions and returns planned work pieces with the transformation instruction for each piece.
4. Domain processor array: one processor receives one planned work piece and returns the complete processed result for that piece.
5. Applier array: one applier receives one planned work piece, its transformation instruction, and the processed result, then returns the concrete action and outcome.
6. Result translator array: one translator receives one formal result, while seeing all formal results for consistency, and returns user-readable language.
7. Arbiter selector: receives all formal results and chooses the review units needed before final formatting.
8. Arbiter units: each review unit receives the formal result or result group it must examine and returns pass, concern, or reprocess instruction.
9. Formatter: receives only approved user-readable fragments and returns one final answer.

## Array Nodes

Four nodes are arrays:

- Node 02 runs once per domain.
- Node 04 runs once per planned work piece.
- Node 05 runs once per planned work piece.
- Node 06 runs once per formal result.

These arrays matter for this document because post publishing has three matching paths that run at the same time: business matching, social-connection matching, and group-membership matching. The ranking result cannot be planned as a simple line of work unless the join point is kept.

## Main Edges

### Edge 1 - User Prompt to Domain Splitter

What moves: the user's request about publishing a post and placing it in feeds.

What the target receives: the raw prompt exactly as the user expressed it.

What the target produces: all domain labels touched by the prompt, such as content analysis, social matching, group matching, ranking, feed distribution, privacy, safety, analytics, or notification.

What breaks if missing: the flow cannot decide which translator units should run, and parallel audience paths may never start.

### Edge 2 - Domain Splitter to Domain Translator Array

What moves: one domain label with the prompt fragment that belongs to it.

What the target receives: one fragment at a time, such as "private posts must not appear in other users' feeds" with the privacy domain.

What the target produces: a precise domain statement that keeps the source meaning and names open questions when needed.

What breaks if missing: later planning receives casual wording and may lose the difference between matching, ranking, privacy, and feed placement.

### Edge 3 - Domain Translator Array to Scheme Planner

What moves: the full set of translated domain statements.

What the target receives: all translated statements together.

What the target produces: ordered pieces, parallel pieces, wait states, recovery paths, and review points.

What breaks if missing: the three matching branches may be collapsed into one branch, and the Ranking Service join point may be lost.

### Edge 4 - Scheme Planner to Domain Processor Array

What moves: one planned piece and its domain context.

What the target receives: one piece, such as "analyze post content" or "compile recipient list after all three audience lists arrive."

What the target produces: the completed result for that piece, including success, fallback, and wait outcomes.

What breaks if missing: appliers would act on partial plans without enough detail to handle timeouts, unsupported language, empty image-only text, or deleted posts.

### Edge 5 - Domain Processor Array to Applier Array

What moves: the processed result for one planned piece, paired with the transformation instruction.

What the target receives: a result ready to turn into an action outcome.

What the target produces: the action taken and the state that follows it, including record-before-event ordering where the document defines a stored post followed by a published event.

What breaks if missing: the flow may announce events before the underlying state exists, or may skip feed updates and analytics events.

### Edge 6 - Applier Array to Result Translator Array

What moves: formal results R1...Rn.

What the target receives: one formal result and the full result collection for consistency.

What the target produces: user-readable wording that preserves the result but hides unnecessary internal detail.

What breaks if missing: the user would receive internal fragments instead of a coherent explanation of what happened.

### Edge 7 - Applier Array to Arbiter Selector

What moves: the same formal result collection R1...Rn.

What the target receives: all formal results and the flow context before user-facing wording is formatted.

What the target produces: the list of review units needed for this run.

What breaks if missing: privacy, safety, ranking, feed diversity, timing, and fallback checks may never run.

### Edge 8 - Arbiter Selector to Arbiter Units

What moves: each selected review responsibility.

What the target receives: a review purpose and the formal result or result group to check.

What the target produces: pass, concern, or reprocess instruction.

What breaks if missing: a private post could be placed in public feeds, harmful content could be distributed, or all three matching paths could time out without fallback.

### Edge 9 - Arbiter Units to Front-of-Flow Reprocessing

What moves: a rejected fragment fbx with the reason it must be fixed.

What the target receives: a specific fragment that must be reprocessed, not the entire finished answer.

What the target produces: a corrected route through the flow.

What breaks if missing: the system can detect a failed rule but cannot repair the affected part.

### Edge 10 - Result Translator Array and Passing Arbiter Units to Formatter

What moves: approved translated fragments only.

What the target receives: readable fragments that passed review.

What the target produces: one final answer shaped for the intended audience.

What breaks if missing: failed or contradictory results could appear in the final response, or approved fragments could remain scattered.

