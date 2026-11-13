# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; stop planning start implementing is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, stop planning start implementing. Keep impact explanation and reprocess instruction separate. Allow approval boundary and stop planning start implementing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, stop planning start implementing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; stop planning start implementing is present and needs its own handling; consolidated implementation order for all session fixes is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, stop planning start implementing, consolidated implementation order for all session fixes. Keep reprocess instruction and approval boundary separate. Allow stop planning start implementing and consolidated implementation order for all session fixes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, stop planning start implementing, consolidated implementation order for all session fixes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: approval boundary is present and needs its own handling; stop planning start implementing is present and needs its own handling; consolidated implementation order for all session fixes is present and needs its own handling; the actual problem is present and needs its own handling.

B: Create work pieces for approval boundary, stop planning start implementing, consolidated implementation order for all session fixes, the actual problem. Keep approval boundary and stop planning start implementing separate. Allow consolidated implementation order for all session fixes and the actual problem to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, stop planning start implementing, consolidated implementation order for all session fixes, the actual problem.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: stop planning start implementing is present and needs its own handling; consolidated implementation order for all session fixes is present and needs its own handling; the actual problem is present and needs its own handling; the six changes ordered by impact effort is present and needs its own handling.

B: Create work pieces for stop planning start implementing, consolidated implementation order for all session fixes, the actual problem, the six changes ordered by impact effort. Keep stop planning start implementing and consolidated implementation order for all session fixes separate. Allow the actual problem and the six changes ordered by impact effort to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: stop planning start implementing, consolidated implementation order for all session fixes, the actual problem, the six changes ordered by impact effort.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Stop Planning. Start Implementing.: consolidated implementation order for all session fixes is present and needs its own handling; the actual problem is present and needs its own handling; the six changes ordered by impact effort is present and needs its own handling; change 1 two lines single biggest quality impact is present and needs its own handling.

B: Create work pieces for consolidated implementation order for all session fixes, the actual problem, the six changes ordered by impact effort, change 1 two lines single biggest quality impact. Keep consolidated implementation order for all session fixes and the actual problem separate. Allow the six changes ordered by impact effort and change 1 two lines single biggest quality impact to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: consolidated implementation order for all session fixes, the actual problem, the six changes ordered by impact effort, change 1 two lines single biggest quality impact.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

