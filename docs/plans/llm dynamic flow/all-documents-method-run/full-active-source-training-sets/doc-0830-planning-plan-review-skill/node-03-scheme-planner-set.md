# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; origin is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, origin. Keep impact explanation and reprocess instruction separate. Allow approval boundary and origin to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, origin.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; origin is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, origin, when to invoke. Keep reprocess instruction and approval boundary separate. Allow origin and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, origin, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: approval boundary is present and needs its own handling; origin is present and needs its own handling; when to invoke is present and needs its own handling; flow check 1 count drift is present and needs its own handling.

B: Create work pieces for approval boundary, origin, when to invoke, flow check 1 count drift. Keep approval boundary and origin separate. Allow when to invoke and flow check 1 count drift to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, origin, when to invoke, flow check 1 count drift.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: origin is present and needs its own handling; when to invoke is present and needs its own handling; flow check 1 count drift is present and needs its own handling; every hit must be the same target number is present and needs its own handling.

B: Create work pieces for origin, when to invoke, flow check 1 count drift, every hit must be the same target number. Keep origin and when to invoke separate. Allow flow check 1 count drift and every hit must be the same target number to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: origin, when to invoke, flow check 1 count drift, every hit must be the same target number.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — flow check 1..flow check 12, flow check 22..flow check 30 + 3-Gate Approval v2.0.0: when to invoke is present and needs its own handling; flow check 1 count drift is present and needs its own handling; every hit must be the same target number is present and needs its own handling; flow check 2 path errors is present and needs its own handling.

B: Create work pieces for when to invoke, flow check 1 count drift, every hit must be the same target number, flow check 2 path errors. Keep when to invoke and flow check 1 count drift separate. Allow every hit must be the same target number and flow check 2 path errors to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to invoke, flow check 1 count drift, every hit must be the same target number, flow check 2 path errors.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

