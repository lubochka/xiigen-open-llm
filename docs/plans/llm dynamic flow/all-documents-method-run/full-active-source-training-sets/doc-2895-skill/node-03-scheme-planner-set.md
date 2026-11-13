# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan execution feedback v1 0 is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, plan execution feedback v1 0. Keep impact explanation and reprocess instruction separate. Allow approval boundary and plan execution feedback v1 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, plan execution feedback v1 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan execution feedback v1 0 is present and needs its own handling; track planning accuracy learn from every session is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, plan execution feedback v1 0, track planning accuracy learn from every session. Keep reprocess instruction and approval boundary separate. Allow plan execution feedback v1 0 and track planning accuracy learn from every session to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, plan execution feedback v1 0, track planning accuracy learn from every session.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: approval boundary is present and needs its own handling; plan execution feedback v1 0 is present and needs its own handling; track planning accuracy learn from every session is present and needs its own handling; the problem is present and needs its own handling.

B: Create work pieces for approval boundary, plan execution feedback v1 0, track planning accuracy learn from every session, the problem. Keep approval boundary and plan execution feedback v1 0 separate. Allow track planning accuracy learn from every session and the problem to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, plan execution feedback v1 0, track planning accuracy learn from every session, the problem.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: plan execution feedback v1 0 is present and needs its own handling; track planning accuracy learn from every session is present and needs its own handling; the problem is present and needs its own handling; protocol is present and needs its own handling.

B: Create work pieces for plan execution feedback v1 0, track planning accuracy learn from every session, the problem, protocol. Keep plan execution feedback v1 0 and track planning accuracy learn from every session separate. Allow the problem and protocol to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: plan execution feedback v1 0, track planning accuracy learn from every session, the problem, protocol.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Plan Execution Feedback v1.0: track planning accuracy learn from every session is present and needs its own handling; the problem is present and needs its own handling; protocol is present and needs its own handling; at session end after section a is written is present and needs its own handling.

B: Create work pieces for track planning accuracy learn from every session, the problem, protocol, at session end after section a is written. Keep track planning accuracy learn from every session and the problem separate. Allow protocol and at session end after section a is written to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: track planning accuracy learn from every session, the problem, protocol, at session end after section a is written.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

