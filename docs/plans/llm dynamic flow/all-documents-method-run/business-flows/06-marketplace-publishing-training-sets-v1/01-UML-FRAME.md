# 01 - UML Frame

## Active Nodes

1. Domain splitter: receives a human prompt and emits all touched domain labels.
2. Domain translator array: one translator receives one prompt fragment plus one domain and writes that fragment in domain language.
3. Scheme planner: receives all domain translations and emits ordered pieces, parallel pieces, wait states, recovery paths, and review points.
4. Domain processor array: one processor receives one planned piece and emits the completed result for that piece.
5. Applier array: one applier receives one piece, its transformation instruction, and the processed result, then emits action and outcome.
6. Result translator array: one translator receives one formal result and sees all formal results for consistency.
7. Arbiter selector: receives all formal results before formatting and chooses review units.
8. Arbiter units: examine results and return pass, concern, or reprocess instruction.
9. Formatter: receives approved translated fragments and returns one finished answer.

## Array Nodes

Four nodes are arrays:

- Node 02 runs once per domain.
- Node 04 runs once per planned work piece.
- Node 05 runs once per planned work piece.
- Node 06 runs once per formal result.

This matters for Flow-06 because a new marketplace item activates several paths in parallel: target-audience profiling, marketplace-post generation, and cooperator matching. Distribution then fans out to friends, groups, and cooperators, each with different card formats and discount rules.

## Main Edges

### Edge 1 - Prompt to Domain Splitter

What moves: a human request about marketplace listings, inventory, pricing, audiences, cooperators, feeds, notifications, or marketplace safety.

What the target receives: the raw prompt.

What the target produces: domain labels.

What breaks if missing: the flow cannot decide which marketplace, inventory, matching, feed, notification, and review work must run.

### Edge 2 - Domain Splitter to Domain Translator Array

What moves: one domain label and the prompt fragment that belongs to it.

What the target receives: one labeled fragment.

What the target produces: precise domain wording.

What breaks if missing: planning may confuse listing state, stock, pricing, audience profiling, synergy scoring, and feed distribution.

### Edge 3 - Domain Translator Array to Scheme Planner

What moves: all translated domain statements.

What the target receives: the complete domain picture.

What the target produces: a plan with item storage, listing activation, three parallel branches, feed distribution, notification, analytics, and review points.

What breaks if missing: cooperator matching may start without target-audience analysis, or distribution may happen before the listing is active.

### Edge 4 - Scheme Planner to Domain Processor Array

What moves: one planned work piece.

What the target receives: one work piece and context.

What the target produces: success, fallback, and failure outcomes for that piece.

What breaks if missing: the applier cannot handle sold-out items, duplicate listings, expired offers, pre-orders, service listings, or competing cooperators.

### Edge 5 - Domain Processor Array to Applier Array

What moves: a processed result and transformation instruction.

What the target receives: action-ready result.

What the target produces: the concrete action and outcome.

What breaks if missing: listings, marketplace posts, cooperator cards, feed entries, and notifications remain only planned.

### Edge 6 - Applier Array to Result Translator Array

What moves: formal results R1...Rn.

What the target receives: one formal result and all sibling results.

What the target produces: readable wording.

What breaks if missing: the seller would receive internal fragments instead of a useful explanation.

### Edge 7 - Applier Array to Arbiter Selector

What moves: formal results before formatting.

What the target receives: all results and flow context.

What the target produces: selected review units.

What breaks if missing: fake listings, price manipulation, inventory fraud, cooperator spam, duplicate listings, and compliance gaps may pass.

### Edge 8 - Arbiter Selector to Arbiter Units

What moves: each review responsibility.

What the target receives: the rule and result group to examine.

What the target produces: pass, concern, or reprocess instruction.

What breaks if missing: risky marketplace results are not checked before the formatter.

### Edge 9 - Arbiter Units to Reprocessing

What moves: a rejected fragment and reason.

What the target receives: the exact fragment to reprocess.

What the target produces: corrected routing.

What breaks if missing: detected marketplace problems cannot be repaired.

### Edge 10 - Approved Results to Formatter

What moves: approved natural-language fragments only.

What the target receives: reviewed fragments.

What the target produces: one final answer.

What breaks if missing: rejected or contradictory marketplace results may appear in the final answer.

