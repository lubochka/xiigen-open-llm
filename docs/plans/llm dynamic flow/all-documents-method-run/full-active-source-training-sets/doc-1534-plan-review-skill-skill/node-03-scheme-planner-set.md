# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan review skill 12 failure classes 3 gate approval is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, plan review skill 12 failure classes 3 gate approval. Keep impact explanation and reprocess instruction separate. Allow approval boundary and plan review skill 12 failure classes 3 gate approval to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, plan review skill 12 failure classes 3 gate approval.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan review skill 12 failure classes 3 gate approval is present and needs its own handling; origin is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, plan review skill 12 failure classes 3 gate approval, origin. Keep reprocess instruction and approval boundary separate. Allow plan review skill 12 failure classes 3 gate approval and origin to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, plan review skill 12 failure classes 3 gate approval, origin.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: approval boundary is present and needs its own handling; plan review skill 12 failure classes 3 gate approval is present and needs its own handling; origin is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for approval boundary, plan review skill 12 failure classes 3 gate approval, origin, when to invoke. Keep approval boundary and plan review skill 12 failure classes 3 gate approval separate. Allow origin and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, plan review skill 12 failure classes 3 gate approval, origin, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: plan review skill 12 failure classes 3 gate approval is present and needs its own handling; origin is present and needs its own handling; when to invoke is present and needs its own handling; the 12 failure classes is present and needs its own handling.

B: Create work pieces for plan review skill 12 failure classes 3 gate approval, origin, when to invoke, the 12 failure classes. Keep plan review skill 12 failure classes 3 gate approval and origin separate. Allow when to invoke and the 12 failure classes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: plan review skill 12 failure classes 3 gate approval, origin, when to invoke, the 12 failure classes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Plan Review Skill — 12 Failure Classes + 3-Gate Approval: origin is present and needs its own handling; when to invoke is present and needs its own handling; the 12 failure classes is present and needs its own handling; flow check 1 count drift is present and needs its own handling.

B: Create work pieces for origin, when to invoke, the 12 failure classes, flow check 1 count drift. Keep origin and when to invoke separate. Allow the 12 failure classes and flow check 1 count drift to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: origin, when to invoke, the 12 failure classes, flow check 1 count drift.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

