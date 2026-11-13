# 01 - UML Frame

## Active Nodes

1. Domain splitter: receives a human prompt and returns all touched domain labels.
2. Domain translator array: receives one prompt fragment plus one domain and writes formal domain wording.
3. Scheme planner: receives all domain wordings and returns ordered pieces, parallel pieces, wait states, recovery paths, and review points.
4. Domain processor array: receives one planned piece and returns the completed result.
5. Applier array: receives one processed piece and returns action and outcome.
6. Result translator array: receives one formal result, with all results visible, and returns readable wording.
7. Arbiter selector: receives formal results before formatting and chooses review units.
8. Arbiter units: examine results and return pass, concern, or reprocess instruction.
9. Formatter: receives approved translated fragments and returns one finished answer.

## Array Nodes

Four nodes are arrays:

- Node 02 runs once per domain.
- Node 04 runs once per planned work piece.
- Node 05 runs once per planned work piece.
- Node 06 runs once per formal result.

This matters for Flow-07 because after acceptance four weight services run in parallel: group, event, purchase, and questionnaire. Final feed integration must wait for those weights or defined fallback values before historical posts are merged.

## Main Edges

### Edge 1 - Prompt to Domain Splitter

What moves: a human request about connecting users, accepting a friend request, calculating connection strength, integrating posts, or controlling privacy.

What the target receives: the raw prompt.

What the target produces: domain labels.

What breaks if missing: request, matching, weight, feed, privacy, and anti-abuse work may not be routed.

### Edge 2 - Domain Splitter to Domain Translator Array

What moves: one domain label and prompt fragment.

What the target receives: one labeled fragment.

What the target produces: precise domain wording.

What breaks if missing: later planning may confuse request state, connection graph, weight analysis, feed merge, privacy, and abuse controls.

### Edge 3 - Domain Translator Array to Scheme Planner

What moves: all translated domain statements.

What the target receives: the complete domain picture.

What the target produces: request phase, acceptance phase, four parallel weight branches, final weight join point, historical feed merge, ongoing rebalancing, and review points.

What breaks if missing: feed integration may start before acceptance or before weight calculation resolves.

### Edge 4 - Scheme Planner to Domain Processor Array

What moves: one planned work piece.

What the target receives: one piece and its context.

What the target produces: full result for that piece, including fallback and privacy outcomes.

What breaks if missing: appliers cannot handle mutual pending requests, withdrawn requests, blocked users, service timeout, or maintenance delay.

### Edge 5 - Domain Processor Array to Applier Array

What moves: processed result and transformation instruction.

What the target receives: action-ready output.

What the target produces: concrete action and outcome.

What breaks if missing: request creation, connection storage, weight calculation, feed merge, and notification remain only planned.

### Edge 6 - Applier Array to Result Translator Array

What moves: formal results R1...Rn.

What the target receives: one formal result and all siblings.

What the target produces: user-readable language.

What breaks if missing: the user receives internal result fragments instead of an explanation.

### Edge 7 - Applier Array to Arbiter Selector

What moves: formal results before formatting.

What the target receives: all formal results and flow context.

What the target produces: selected review units.

What breaks if missing: privacy, block, request spam, weight gaming, purchase privacy, and feed-cap reviews may never run.

### Edge 8 - Arbiter Selector to Arbiter Units

What moves: selected review responsibilities.

What the target receives: the rule and result group to examine.

What the target produces: pass, concern, or reprocess instruction.

What breaks if missing: risky friend integration results are not checked.

### Edge 9 - Arbiter Units to Reprocessing

What moves: a rejected fragment with reason.

What the target receives: exact fragment to reprocess.

What the target produces: corrected route.

What breaks if missing: the system can find a failure but cannot repair it.

### Edge 10 - Approved Results to Formatter

What moves: approved natural-language fragments.

What the target receives: reviewed fragments only.

What the target produces: one final answer.

What breaks if missing: rejected or contradictory connection results may appear in the final response.

