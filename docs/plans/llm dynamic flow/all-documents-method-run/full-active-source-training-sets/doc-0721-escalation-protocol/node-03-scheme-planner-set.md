# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; escalation protocol is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, escalation protocol. Keep impact explanation and reprocess instruction separate. Allow approval boundary and escalation protocol to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, escalation protocol.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; escalation protocol is present and needs its own handling; when to escalate triggers is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, escalation protocol, when to escalate triggers. Keep reprocess instruction and approval boundary separate. Allow escalation protocol and when to escalate triggers to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, escalation protocol, when to escalate triggers.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: approval boundary is present and needs its own handling; escalation protocol is present and needs its own handling; when to escalate triggers is present and needs its own handling; mandatory escalation triggers always escalate immediately is present and needs its own handling.

B: Create work pieces for approval boundary, escalation protocol, when to escalate triggers, mandatory escalation triggers always escalate immediately. Keep approval boundary and escalation protocol separate. Allow when to escalate triggers and mandatory escalation triggers always escalate immediately to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, escalation protocol, when to escalate triggers, mandatory escalation triggers always escalate immediately.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: escalation protocol is present and needs its own handling; when to escalate triggers is present and needs its own handling; mandatory escalation triggers always escalate immediately is present and needs its own handling; regression detected finds any node that went from is present and needs its own handling.

B: Create work pieces for escalation protocol, when to escalate triggers, mandatory escalation triggers always escalate immediately, regression detected finds any node that went from. Keep escalation protocol and when to escalate triggers separate. Allow mandatory escalation triggers always escalate immediately and regression detected finds any node that went from to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: escalation protocol, when to escalate triggers, mandatory escalation triggers always escalate immediately, regression detected finds any node that went from.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Escalation Protocol: when to escalate triggers is present and needs its own handling; mandatory escalation triggers always escalate immediately is present and needs its own handling; regression detected finds any node that went from is present and needs its own handling; product decision required the next step requires choosing between is present and needs its own handling.

B: Create work pieces for when to escalate triggers, mandatory escalation triggers always escalate immediately, regression detected finds any node that went from, product decision required the next step requires choosing between. Keep when to escalate triggers and mandatory escalation triggers always escalate immediately separate. Allow regression detected finds any node that went from and product decision required the next step requires choosing between to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to escalate triggers, mandatory escalation triggers always escalate immediately, regression detected finds any node that went from, product decision required the next step requires choosing between.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

