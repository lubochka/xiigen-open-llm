# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: trigger and input is present and needs its own handling; node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling.

B: Create work pieces for trigger and input, node responsibility, parallel branch, wait and recovery. Keep trigger and input and node responsibility separate. Allow parallel branch and wait and recovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: trigger and input, node responsibility, parallel branch, wait and recovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling.

B: Create work pieces for node responsibility, parallel branch, wait and recovery, result merge. Keep node responsibility and parallel branch separate. Allow wait and recovery and result merge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: node responsibility, parallel branch, wait and recovery, result merge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling.

B: Create work pieces for parallel branch, wait and recovery, result merge, review before output. Keep parallel branch and wait and recovery separate. Allow result merge and review before output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: parallel branch, wait and recovery, result merge, review before output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling; cross cutting foundation used by every module is present and needs its own handling.

B: Create work pieces for wait and recovery, result merge, review before output, cross cutting foundation used by every module. Keep wait and recovery and result merge separate. Allow review before output and cross cutting foundation used by every module to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: wait and recovery, result merge, review before output, cross cutting foundation used by every module.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: result merge is present and needs its own handling; review before output is present and needs its own handling; cross cutting foundation used by every module is present and needs its own handling; a organization master data is present and needs its own handling.

B: Create work pieces for result merge, review before output, cross cutting foundation used by every module, a organization master data. Keep result merge and review before output separate. Allow cross cutting foundation used by every module and a organization master data to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: result merge, review before output, cross cutting foundation used by every module, a organization master data.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: review before output is present and needs its own handling; cross cutting foundation used by every module is present and needs its own handling; a organization master data is present and needs its own handling; b security controls audit is present and needs its own handling.

B: Create work pieces for review before output, cross cutting foundation used by every module, a organization master data, b security controls audit. Keep review before output and cross cutting foundation used by every module separate. Allow a organization master data and b security controls audit to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: review before output, cross cutting foundation used by every module, a organization master data, b security controls audit.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: cross cutting foundation used by every module is present and needs its own handling; a organization master data is present and needs its own handling; b security controls audit is present and needs its own handling; c workflow document management is present and needs its own handling.

B: Create work pieces for cross cutting foundation used by every module, a organization master data, b security controls audit, c workflow document management. Keep cross cutting foundation used by every module and a organization master data separate. Allow b security controls audit and c workflow document management to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: cross cutting foundation used by every module, a organization master data, b security controls audit, c workflow document management.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Cross-cutting foundation used by every module: a organization master data is present and needs its own handling; b security controls audit is present and needs its own handling; c workflow document management is present and needs its own handling; d integration layer is present and needs its own handling.

B: Create work pieces for a organization master data, b security controls audit, c workflow document management, d integration layer. Keep a organization master data and b security controls audit separate. Allow c workflow document management and d integration layer to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: a organization master data, b security controls audit, c workflow document management, d integration layer.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

