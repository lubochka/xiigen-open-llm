# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; finance task types catalog f13 is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, finance task types catalog f13. Keep impact explanation and reprocess instruction separate. Allow approval boundary and finance task types catalog f13 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, finance task types catalog f13.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; finance task types catalog f13 is present and needs its own handling; save point p2 tasks status complete is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, finance task types catalog f13, save point p2 tasks status complete. Keep reprocess instruction and approval boundary separate. Allow finance task types catalog f13 and save point p2 tasks status complete to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, finance task types catalog f13, save point p2 tasks status complete.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: approval boundary is present and needs its own handling; finance task types catalog f13 is present and needs its own handling; save point p2 tasks status complete is present and needs its own handling; backward compatibility t1 a task type unchanged is present and needs its own handling.

B: Create work pieces for approval boundary, finance task types catalog f13, save point p2 tasks status complete, backward compatibility t1 a task type unchanged. Keep approval boundary and finance task types catalog f13 separate. Allow save point p2 tasks status complete and backward compatibility t1 a task type unchanged to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, finance task types catalog f13, save point p2 tasks status complete, backward compatibility t1 a task type unchanged.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: finance task types catalog f13 is present and needs its own handling; save point p2 tasks status complete is present and needs its own handling; backward compatibility t1 a task type unchanged is present and needs its own handling; task type a task type finance durable saga entry gate is present and needs its own handling.

B: Create work pieces for finance task types catalog f13, save point p2 tasks status complete, backward compatibility t1 a task type unchanged, task type a task type finance durable saga entry gate. Keep finance task types catalog f13 and save point p2 tasks status complete separate. Allow backward compatibility t1 a task type unchanged and task type a task type finance durable saga entry gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: finance task types catalog f13, save point p2 tasks status complete, backward compatibility t1 a task type unchanged, task type a task type finance durable saga entry gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Finance TASK TYPES CATALOG F13: save point p2 tasks status complete is present and needs its own handling; backward compatibility t1 a task type unchanged is present and needs its own handling; task type a task type finance durable saga entry gate is present and needs its own handling; task type a task type three way match gate is present and needs its own handling.

B: Create work pieces for save point p2 tasks status complete, backward compatibility t1 a task type unchanged, task type a task type finance durable saga entry gate, task type a task type three way match gate. Keep save point p2 tasks status complete and backward compatibility t1 a task type unchanged separate. Allow task type a task type finance durable saga entry gate and task type a task type three way match gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: save point p2 tasks status complete, backward compatibility t1 a task type unchanged, task type a task type finance durable saga entry gate, task type a task type three way match gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

