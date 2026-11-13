# 01 - UML Frame

## Active Nodes

1. Domain splitter: receives a human prompt and emits all touched social-network domains.
2. Domain translator array: writes one prompt fragment in one domain's language.
3. Scheme planner: turns domain wording into ordered and parallel work pieces.
4. Domain processor array: processes one work piece.
5. Applier array: applies one processed piece into a result.
6. Result translator array: translates one formal result into user language.
7. Arbiter selector: chooses reviews before formatting.
8. Arbiter units: pass, flag, or reprocess.
9. Formatter: combines approved translated fragments.

## Array Nodes

Four nodes are arrays:

- Node 02 runs once per domain.
- Node 04 runs once per planned work piece.
- Node 05 runs once per planned work piece.
- Node 06 runs once per formal result.

This matters for Flow-11 because a social network has many simultaneous domains: graph, feed, content, media, search, messaging, notification, moderation, privacy, recommendations, analytics, and infrastructure.

## Main Edges

### Edge 1 - Prompt to Domain Splitter

What moves: a user request about a Facebook-like or LinkedIn-like social network.

What the target receives: raw human wording.

What the target produces: all module and flow domains touched by the prompt.

What breaks if missing: the system may plan feed without graph, messaging without trust gates, or search without permissions.

### Edge 2 - Domain Splitter to Domain Translator Array

What moves: one domain label and prompt fragment.

What the target receives: a single labeled fragment.

What the target produces: precise domain wording.

What breaks if missing: planning may lose the difference between content creation, feed ranking, messaging, moderation, and social graph.

### Edge 3 - Domain Translator Array to Scheme Planner

What moves: all translated domain statements.

What the target receives: the full social-network module picture.

What the target produces: a plan with onboarding, profile, graph, content/media, engagement, feed, messaging, search, moderation, privacy, recommendations, analytics, and infrastructure.

What breaks if missing: flows become isolated modules rather than a connected social product.

### Edge 4 - Scheme Planner to Domain Processor Array

What moves: one planned module or flow piece.

What the target receives: one piece and context.

What the target produces: a complete processed result for that piece.

What breaks if missing: appliers cannot handle media readiness, privacy filtering, idempotent reactions, message requests, or moderation enforcement.

### Edge 5 - Domain Processor Array to Applier Array

What moves: processed result and transformation instruction.

What the target receives: action-ready result.

What the target produces: concrete module behavior or flow outcome.

What breaks if missing: the social product remains a description instead of executable behavior.

### Edge 6 - Applier Array to Result Translator Array

What moves: formal results.

What the target receives: one formal result and all sibling results.

What the target produces: user-readable wording.

What breaks if missing: the final answer is not understandable.

### Edge 7 - Applier Array to Arbiter Selector

What moves: formal results before formatting.

What the target receives: all results and context.

What the target produces: selected review units.

What breaks if missing: privacy, block, moderation, spam, feed quality, ad spacing, and permissions checks may not run.

### Edge 8 - Arbiter Selector to Arbiter Units

What moves: review responsibility and result group.

What the target receives: the rule to check.

What the target produces: pass, concern, or reprocess instruction.

What breaks if missing: violations remain in the final result.

### Edge 9 - Arbiter Units to Reprocessing

What moves: rejected fragment and reason.

What the target receives: exact fragment to correct.

What the target produces: corrected route.

What breaks if missing: failures cannot be repaired.

### Edge 10 - Approved Results to Formatter

What moves: approved translated fragments.

What the target receives: reviewed fragments.

What the target produces: one final answer.

What breaks if missing: the response may include unsafe or contradictory social-network behavior.

