# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; initialize milestone context is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, initialize milestone context. Keep impact explanation and reprocess instruction separate. Allow approval boundary and initialize milestone context to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, initialize milestone context.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; initialize milestone context is present and needs its own handling; determine milestone scope is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, initialize milestone context, determine milestone scope. Keep reprocess instruction and approval boundary separate. Allow initialize milestone context and determine milestone scope to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, initialize milestone context, determine milestone scope.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: approval boundary is present and needs its own handling; initialize milestone context is present and needs its own handling; determine milestone scope is present and needs its own handling; get phases in milestone sorted numerically handles decimals is present and needs its own handling.

B: Create work pieces for approval boundary, initialize milestone context, determine milestone scope, get phases in milestone sorted numerically handles decimals. Keep approval boundary and initialize milestone context separate. Allow determine milestone scope and get phases in milestone sorted numerically handles decimals to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, initialize milestone context, determine milestone scope, get phases in milestone sorted numerically handles decimals.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: initialize milestone context is present and needs its own handling; determine milestone scope is present and needs its own handling; get phases in milestone sorted numerically handles decimals is present and needs its own handling; read all phase verifications is present and needs its own handling.

B: Create work pieces for initialize milestone context, determine milestone scope, get phases in milestone sorted numerically handles decimals, read all phase verifications. Keep initialize milestone context and determine milestone scope separate. Allow get phases in milestone sorted numerically handles decimals and read all phase verifications to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: initialize milestone context, determine milestone scope, get phases in milestone sorted numerically handles decimals, read all phase verifications.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Initialize Milestone Context: determine milestone scope is present and needs its own handling; get phases in milestone sorted numerically handles decimals is present and needs its own handling; read all phase verifications is present and needs its own handling; repeat for each phase number from roadmap md is present and needs its own handling.

B: Create work pieces for determine milestone scope, get phases in milestone sorted numerically handles decimals, read all phase verifications, repeat for each phase number from roadmap md. Keep determine milestone scope and get phases in milestone sorted numerically handles decimals separate. Allow read all phase verifications and repeat for each phase number from roadmap md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: determine milestone scope, get phases in milestone sorted numerically handles decimals, read all phase verifications, repeat for each phase number from roadmap md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

