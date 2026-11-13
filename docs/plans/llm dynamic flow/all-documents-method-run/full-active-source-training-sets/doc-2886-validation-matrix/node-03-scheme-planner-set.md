# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan validation matrix is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, plan validation matrix. Keep impact explanation and reprocess instruction separate. Allow approval boundary and plan validation matrix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, plan validation matrix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan validation matrix is present and needs its own handling; checks every requirement from the user s prompt against the plan is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, plan validation matrix, checks every requirement from the user s prompt against the plan. Keep reprocess instruction and approval boundary separate. Allow plan validation matrix and checks every requirement from the user s prompt against the plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, plan validation matrix, checks every requirement from the user s prompt against the plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: approval boundary is present and needs its own handling; plan validation matrix is present and needs its own handling; checks every requirement from the user s prompt against the plan is present and needs its own handling; user requirements plan coverage is present and needs its own handling.

B: Create work pieces for approval boundary, plan validation matrix, checks every requirement from the user s prompt against the plan, user requirements plan coverage. Keep approval boundary and plan validation matrix separate. Allow checks every requirement from the user s prompt against the plan and user requirements plan coverage to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, plan validation matrix, checks every requirement from the user s prompt against the plan, user requirements plan coverage.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: plan validation matrix is present and needs its own handling; checks every requirement from the user s prompt against the plan is present and needs its own handling; user requirements plan coverage is present and needs its own handling; core feature requirements is present and needs its own handling.

B: Create work pieces for plan validation matrix, checks every requirement from the user s prompt against the plan, user requirements plan coverage, core feature requirements. Keep plan validation matrix and checks every requirement from the user s prompt against the plan separate. Allow user requirements plan coverage and core feature requirements to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: plan validation matrix, checks every requirement from the user s prompt against the plan, user requirements plan coverage, core feature requirements.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION MATRIX: checks every requirement from the user s prompt against the plan is present and needs its own handling; user requirements plan coverage is present and needs its own handling; core feature requirements is present and needs its own handling; testing requirements is present and needs its own handling.

B: Create work pieces for checks every requirement from the user s prompt against the plan, user requirements plan coverage, core feature requirements, testing requirements. Keep checks every requirement from the user s prompt against the plan and user requirements plan coverage separate. Allow core feature requirements and testing requirements to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: checks every requirement from the user s prompt against the plan, user requirements plan coverage, core feature requirements, testing requirements.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

