# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen engine readiness plan is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen engine readiness plan. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen engine readiness plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen engine readiness plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen engine readiness plan is present and needs its own handling; from current state p1 complete p2 p26 pending is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen engine readiness plan, from current state p1 complete p2 p26 pending. Keep reprocess instruction and approval boundary separate. Allow xiigen engine readiness plan and from current state p1 complete p2 p26 pending to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen engine readiness plan, from current state p1 complete p2 p26 pending.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: approval boundary is present and needs its own handling; xiigen engine readiness plan is present and needs its own handling; from current state p1 complete p2 p26 pending is present and needs its own handling; date 2026 03 23 is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen engine readiness plan, from current state p1 complete p2 p26 pending, date 2026 03 23. Keep approval boundary and xiigen engine readiness plan separate. Allow from current state p1 complete p2 p26 pending and date 2026 03 23 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen engine readiness plan, from current state p1 complete p2 p26 pending, date 2026 03 23.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: xiigen engine readiness plan is present and needs its own handling; from current state p1 complete p2 p26 pending is present and needs its own handling; date 2026 03 23 is present and needs its own handling; the goal plain english is present and needs its own handling.

B: Create work pieces for xiigen engine readiness plan, from current state p1 complete p2 p26 pending, date 2026 03 23, the goal plain english. Keep xiigen engine readiness plan and from current state p1 complete p2 p26 pending separate. Allow date 2026 03 23 and the goal plain english to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen engine readiness plan, from current state p1 complete p2 p26 pending, date 2026 03 23, the goal plain english.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — Engine Readiness Plan: from current state p1 complete p2 p26 pending is present and needs its own handling; date 2026 03 23 is present and needs its own handling; the goal plain english is present and needs its own handling; what the docs already designed and is not yet built is present and needs its own handling.

B: Create work pieces for from current state p1 complete p2 p26 pending, date 2026 03 23, the goal plain english, what the docs already designed and is not yet built. Keep from current state p1 complete p2 p26 pending and date 2026 03 23 separate. Allow the goal plain english and what the docs already designed and is not yet built to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: from current state p1 complete p2 p26 pending, date 2026 03 23, the goal plain english, what the docs already designed and is not yet built.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

