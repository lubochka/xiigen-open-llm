# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: trigger and input is present and needs its own handling; node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling.

B: Create work pieces for trigger and input, node responsibility, parallel branch, wait and recovery. Keep trigger and input and node responsibility separate. Allow parallel branch and wait and recovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: trigger and input, node responsibility, parallel branch, wait and recovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling.

B: Create work pieces for node responsibility, parallel branch, wait and recovery, result merge. Keep node responsibility and parallel branch separate. Allow wait and recovery and result merge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: node responsibility, parallel branch, wait and recovery, result merge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling.

B: Create work pieces for parallel branch, wait and recovery, result merge, review before output. Keep parallel branch and wait and recovery separate. Allow result merge and review before output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: parallel branch, wait and recovery, result merge, review before output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling; sub episode map is present and needs its own handling.

B: Create work pieces for wait and recovery, result merge, review before output, sub episode map. Keep wait and recovery and result merge separate. Allow review before output and sub episode map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: wait and recovery, result merge, review before output, sub episode map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: result merge is present and needs its own handling; review before output is present and needs its own handling; sub episode map is present and needs its own handling; each sub phase treated as its own episode connected to its parent is present and needs its own handling.

B: Create work pieces for result merge, review before output, sub episode map, each sub phase treated as its own episode connected to its parent. Keep result merge and review before output separate. Allow sub episode map and each sub phase treated as its own episode connected to its parent to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: result merge, review before output, sub episode map, each sub phase treated as its own episode connected to its parent.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: review before output is present and needs its own handling; sub episode map is present and needs its own handling; each sub phase treated as its own episode connected to its parent is present and needs its own handling; plain human language throughout same format as parent episodes is present and needs its own handling.

B: Create work pieces for review before output, sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes. Keep review before output and sub episode map separate. Allow each sub phase treated as its own episode connected to its parent and plain human language throughout same format as parent episodes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: review before output, sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: sub episode map is present and needs its own handling; each sub phase treated as its own episode connected to its parent is present and needs its own handling; plain human language throughout same format as parent episodes is present and needs its own handling; flow plan episodes 1 flow preparation episode total 28 this pass is present and needs its own handling.

B: Create work pieces for sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, flow plan episodes 1 flow preparation episode total 28 this pass. Keep sub episode map and each sub phase treated as its own episode connected to its parent separate. Allow plain human language throughout same format as parent episodes and flow plan episodes 1 flow preparation episode total 28 this pass to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, flow plan episodes 1 flow preparation episode total 28 this pass.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: each sub phase treated as its own episode connected to its parent is present and needs its own handling; plain human language throughout same format as parent episodes is present and needs its own handling; flow plan episodes 1 flow preparation episode total 28 this pass is present and needs its own handling; check for plan document read header phase list version history is present and needs its own handling.

B: Create work pieces for each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, flow plan episodes 1 flow preparation episode total 28 this pass, check for plan document read header phase list version history. Keep each sub phase treated as its own episode connected to its parent and plain human language throughout same format as parent episodes separate. Allow flow plan episodes 1 flow preparation episode total 28 this pass and check for plan document read header phase list version history to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, flow plan episodes 1 flow preparation episode total 28 this pass, check for plan document read header phase list version history.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

