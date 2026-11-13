# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; pre q0 absorption is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, pre q0 absorption. Keep impact explanation and reprocess instruction separate. Allow approval boundary and pre q0 absorption to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, pre q0 absorption.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; pre q0 absorption is present and needs its own handling; session start gate is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, pre q0 absorption, session start gate. Keep reprocess instruction and approval boundary separate. Allow pre q0 absorption and session start gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, pre q0 absorption, session start gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: approval boundary is present and needs its own handling; pre q0 absorption is present and needs its own handling; session start gate is present and needs its own handling; what is known before the plan is present and needs its own handling.

B: Create work pieces for approval boundary, pre q0 absorption, session start gate, what is known before the plan. Keep approval boundary and pre q0 absorption separate. Allow session start gate and what is known before the plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, pre q0 absorption, session start gate, what is known before the plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: pre q0 absorption is present and needs its own handling; session start gate is present and needs its own handling; what is known before the plan is present and needs its own handling; claude code execution plan batch 5 fleet validation is present and needs its own handling.

B: Create work pieces for pre q0 absorption, session start gate, what is known before the plan, claude code execution plan batch 5 fleet validation. Keep pre q0 absorption and session start gate separate. Allow what is known before the plan and claude code execution plan batch 5 fleet validation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: pre q0 absorption, session start gate, what is known before the plan, claude code execution plan batch 5 fleet validation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PRE-Q0 Absorption: session start gate is present and needs its own handling; what is known before the plan is present and needs its own handling; claude code execution plan batch 5 fleet validation is present and needs its own handling; one file per run one task per run h0 cadence is present and needs its own handling.

B: Create work pieces for session start gate, what is known before the plan, claude code execution plan batch 5 fleet validation, one file per run one task per run h0 cadence. Keep session start gate and what is known before the plan separate. Allow claude code execution plan batch 5 fleet validation and one file per run one task per run h0 cadence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: session start gate, what is known before the plan, claude code execution plan batch 5 fleet validation, one file per run one task per run h0 cadence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

