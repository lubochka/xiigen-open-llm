# 01 - UML Frame

## Source Round

This frame belongs to the training package for Jenkins and Azure DevOps module map. It records the dynamic-flow mapping used before writing any node pairs.

## Active Nodes

1. Domain splitter: user prompt -> array of domain labels.
2. Domain translator array: one domain fragment -> one formal domain statement.
3. Scheme planner: all formal domain statements -> ordered and parallel work pieces.
4. Domain processor array: one work piece -> complete processed result.
5. Applier array: one processed piece plus its instruction -> concrete outcome.
6. Result translator array: one formal result, with the full result collection visible -> user-readable result.
7. Arbiter selector: formal result collection -> required review units.
8. Arbiter units: review responsibility plus result -> pass, concern, or reprocess instruction.
9. Formatter: approved translated results -> one finished answer.

## Array Nodes

The domain translator, domain processor, applier, and result translator are arrays. Each unit handles one element independently. Their outputs later converge. This package keeps those elements separate so parallel flow is not flattened.

## Edges

### Edge 1 - User prompt to domain splitter

What moves: the human request in ordinary language.
What the target receives: one prompt that may mention several territories.
What the target must produce: an array of domain labels.
What breaks if missing: the rest of the flow receives one vague topic instead of separated work.

### Edge 2 - Domain splitter to domain translator array

What moves: each domain label paired with the matching part of the prompt.
What the target receives: one domain-scoped fragment.
What the target must produce: formal wording for that domain.
What breaks if missing: later nodes plan from raw human wording and ambiguity leaks forward.

### Edge 3 - Domain translator array to scheme planner

What moves: all formal domain statements together.
What the target receives: a multi-domain description of the same request.
What the target must produce: ordered work pieces, parallel work pieces, wait states, recovery paths, and review points.
What breaks if missing: the flow becomes a list of facts instead of an executable plan.

### Edge 4 - Scheme planner to domain processor array

What moves: each planned work piece with its domain context.
What the target receives: one work piece.
What the target must produce: a complete processed result for that work piece.
What breaks if missing: sub-tasks are not worked independently and parallel execution is lost.

### Edge 5 - Domain processor array to applier array

What moves: one processed result plus the instruction for applying it.
What the target receives: enough detail to act without rereading the source.
What the target must produce: concrete outcome and state.
What breaks if missing: the system knows what should happen but does not produce an applied result.

### Edge 6 - Applier array to result translator array

What moves: formal results R1...Rn.
What the target receives: one result while also seeing the full result set for consistency.
What the target must produce: user-readable wording for that result.
What breaks if missing: internal results leak directly to the user or contradictions are hidden.

### Edge 7 - Applier array to arbiter selector

What moves: the full formal result collection R1...Rn.
What the target receives: the unformatted result set and flow context.
What the target must produce: the review units that need to fire.
What breaks if missing: risky results reach formatting without the right review.

### Edge 8 - Arbiter selector to arbiter units

What moves: each selected review responsibility.
What the target receives: one review responsibility plus the affected result or result group.
What the target must produce: pass, concern, or reprocess instruction.
What breaks if missing: selected reviews are named but not performed.

### Edge 9 - Arbiter units back to the front of the flow

What moves: a reprocess fragment and the reason it failed.
What the target receives: a corrected prompt fragment for another pass through the flow.
What the target must produce: a new domain split for the affected fragment.
What breaks if missing: review can complain but cannot repair.

### Edge 10 - Approved result translations to formatter

What moves: only translated results that passed arbitration.
What the target receives: approved fragments and audience context.
What the target must produce: one finished answer.
What breaks if missing: the user receives disconnected fragments instead of a coherent answer.
