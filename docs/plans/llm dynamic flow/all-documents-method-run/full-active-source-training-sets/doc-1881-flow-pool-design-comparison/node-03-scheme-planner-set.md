# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; flow pool context arbiter design comparison is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, flow pool context arbiter design comparison. Keep impact explanation and reprocess instruction separate. Allow approval boundary and flow pool context arbiter design comparison to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, flow pool context arbiter design comparison.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; flow pool context arbiter design comparison is present and needs its own handling; sources compared is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, flow pool context arbiter design comparison, sources compared. Keep reprocess instruction and approval boundary separate. Allow flow pool context arbiter design comparison and sources compared to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, flow pool context arbiter design comparison, sources compared.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: approval boundary is present and needs its own handling; flow pool context arbiter design comparison is present and needs its own handling; sources compared is present and needs its own handling; dimension by dimension comparison is present and needs its own handling.

B: Create work pieces for approval boundary, flow pool context arbiter design comparison, sources compared, dimension by dimension comparison. Keep approval boundary and flow pool context arbiter design comparison separate. Allow sources compared and dimension by dimension comparison to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, flow pool context arbiter design comparison, sources compared, dimension by dimension comparison.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: flow pool context arbiter design comparison is present and needs its own handling; sources compared is present and needs its own handling; dimension by dimension comparison is present and needs its own handling; task numbers is present and needs its own handling.

B: Create work pieces for flow pool context arbiter design comparison, sources compared, dimension by dimension comparison, task numbers. Keep flow pool context arbiter design comparison and sources compared separate. Allow dimension by dimension comparison and task numbers to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: flow pool context arbiter design comparison, sources compared, dimension by dimension comparison, task numbers.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for FLOW POOL + CONTEXT ARBITER — DESIGN COMPARISON: sources compared is present and needs its own handling; dimension by dimension comparison is present and needs its own handling; task numbers is present and needs its own handling; pool scope trace id is present and needs its own handling.

B: Create work pieces for sources compared, dimension by dimension comparison, task numbers, pool scope trace id. Keep sources compared and dimension by dimension comparison separate. Allow task numbers and pool scope trace id to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: sources compared, dimension by dimension comparison, task numbers, pool scope trace id.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

