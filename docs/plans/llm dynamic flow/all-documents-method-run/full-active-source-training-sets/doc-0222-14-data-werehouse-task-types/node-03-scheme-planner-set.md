# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; merged 2026 02 26 source flow14 plan p0 md phase p4 is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, merged 2026 02 26 source flow14 plan p0 md phase p4. Keep impact explanation and reprocess instruction separate. Allow approval boundary and merged 2026 02 26 source flow14 plan p0 md phase p4 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, merged 2026 02 26 source flow14 plan p0 md phase p4.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; merged 2026 02 26 source flow14 plan p0 md phase p4 is present and needs its own handling; backward compatibility t1 a task type unchanged is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, merged 2026 02 26 source flow14 plan p0 md phase p4, backward compatibility t1 a task type unchanged. Keep reprocess instruction and approval boundary separate. Allow merged 2026 02 26 source flow14 plan p0 md phase p4 and backward compatibility t1 a task type unchanged to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, merged 2026 02 26 source flow14 plan p0 md phase p4, backward compatibility t1 a task type unchanged.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: approval boundary is present and needs its own handling; merged 2026 02 26 source flow14 plan p0 md phase p4 is present and needs its own handling; backward compatibility t1 a task type unchanged is present and needs its own handling; task type a task type dimension fact refresh cycle is present and needs its own handling.

B: Create work pieces for approval boundary, merged 2026 02 26 source flow14 plan p0 md phase p4, backward compatibility t1 a task type unchanged, task type a task type dimension fact refresh cycle. Keep approval boundary and merged 2026 02 26 source flow14 plan p0 md phase p4 separate. Allow backward compatibility t1 a task type unchanged and task type a task type dimension fact refresh cycle to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, merged 2026 02 26 source flow14 plan p0 md phase p4, backward compatibility t1 a task type unchanged, task type a task type dimension fact refresh cycle.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: merged 2026 02 26 source flow14 plan p0 md phase p4 is present and needs its own handling; backward compatibility t1 a task type unchanged is present and needs its own handling; task type a task type dimension fact refresh cycle is present and needs its own handling; task type a task type mart build kpi refresh is present and needs its own handling.

B: Create work pieces for merged 2026 02 26 source flow14 plan p0 md phase p4, backward compatibility t1 a task type unchanged, task type a task type dimension fact refresh cycle, task type a task type mart build kpi refresh. Keep merged 2026 02 26 source flow14 plan p0 md phase p4 and backward compatibility t1 a task type unchanged separate. Allow task type a task type dimension fact refresh cycle and task type a task type mart build kpi refresh to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: merged 2026 02 26 source flow14 plan p0 md phase p4, backward compatibility t1 a task type unchanged, task type a task type dimension fact refresh cycle, task type a task type mart build kpi refresh.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 4: TASK TYPES a task type–a task type + TEMPLATES 33–35: backward compatibility t1 a task type unchanged is present and needs its own handling; task type a task type dimension fact refresh cycle is present and needs its own handling; task type a task type mart build kpi refresh is present and needs its own handling; task type a task type cross system identity join is present and needs its own handling.

B: Create work pieces for backward compatibility t1 a task type unchanged, task type a task type dimension fact refresh cycle, task type a task type mart build kpi refresh, task type a task type cross system identity join. Keep backward compatibility t1 a task type unchanged and task type a task type dimension fact refresh cycle separate. Allow task type a task type mart build kpi refresh and task type a task type cross system identity join to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: backward compatibility t1 a task type unchanged, task type a task type dimension fact refresh cycle, task type a task type mart build kpi refresh, task type a task type cross system identity join.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

