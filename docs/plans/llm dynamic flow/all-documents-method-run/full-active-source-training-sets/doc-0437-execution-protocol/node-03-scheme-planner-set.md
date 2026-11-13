# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; execution protocol quick reference is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, execution protocol quick reference. Keep impact explanation and reprocess instruction separate. Allow approval boundary and execution protocol quick reference to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, execution protocol quick reference.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; execution protocol quick reference is present and needs its own handling; css debug history implementation is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, execution protocol quick reference, css debug history implementation. Keep reprocess instruction and approval boundary separate. Allow execution protocol quick reference and css debug history implementation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, execution protocol quick reference, css debug history implementation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: approval boundary is present and needs its own handling; execution protocol quick reference is present and needs its own handling; css debug history implementation is present and needs its own handling; before starting any phase is present and needs its own handling.

B: Create work pieces for approval boundary, execution protocol quick reference, css debug history implementation, before starting any phase. Keep approval boundary and execution protocol quick reference separate. Allow css debug history implementation and before starting any phase to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, execution protocol quick reference, css debug history implementation, before starting any phase.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: execution protocol quick reference is present and needs its own handling; css debug history implementation is present and needs its own handling; before starting any phase is present and needs its own handling; during phase execution is present and needs its own handling.

B: Create work pieces for execution protocol quick reference, css debug history implementation, before starting any phase, during phase execution. Keep execution protocol quick reference and css debug history implementation separate. Allow before starting any phase and during phase execution to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: execution protocol quick reference, css debug history implementation, before starting any phase, during phase execution.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for EXECUTION PROTOCOL — QUICK REFERENCE: css debug history implementation is present and needs its own handling; before starting any phase is present and needs its own handling; during phase execution is present and needs its own handling; code development is present and needs its own handling.

B: Create work pieces for css debug history implementation, before starting any phase, during phase execution, code development. Keep css debug history implementation and before starting any phase separate. Allow during phase execution and code development to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: css debug history implementation, before starting any phase, during phase execution, code development.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

