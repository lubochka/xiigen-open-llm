# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; fix 20 responsive group positioning is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, fix 20 responsive group positioning. Keep impact explanation and reprocess instruction separate. Allow approval boundary and fix 20 responsive group positioning to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, fix 20 responsive group positioning.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; fix 20 responsive group positioning is present and needs its own handling; the problem is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, fix 20 responsive group positioning, the problem. Keep reprocess instruction and approval boundary separate. Allow fix 20 responsive group positioning and the problem to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, fix 20 responsive group positioning, the problem.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: approval boundary is present and needs its own handling; fix 20 responsive group positioning is present and needs its own handling; the problem is present and needs its own handling; the solution percentage based positioning for group children is present and needs its own handling.

B: Create work pieces for approval boundary, fix 20 responsive group positioning, the problem, the solution percentage based positioning for group children. Keep approval boundary and fix 20 responsive group positioning separate. Allow the problem and the solution percentage based positioning for group children to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, fix 20 responsive group positioning, the problem, the solution percentage based positioning for group children.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: fix 20 responsive group positioning is present and needs its own handling; the problem is present and needs its own handling; the solution percentage based positioning for group children is present and needs its own handling; phase 1 root group container quick win is present and needs its own handling.

B: Create work pieces for fix 20 responsive group positioning, the problem, the solution percentage based positioning for group children, phase 1 root group container quick win. Keep fix 20 responsive group positioning and the problem separate. Allow the solution percentage based positioning for group children and phase 1 root group container quick win to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: fix 20 responsive group positioning, the problem, the solution percentage based positioning for group children, phase 1 root group container quick win.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Fix 20: Responsive GROUP Positioning: the problem is present and needs its own handling; the solution percentage based positioning for group children is present and needs its own handling; phase 1 root group container quick win is present and needs its own handling; phase 2 children use percentage positioning is present and needs its own handling.

B: Create work pieces for the problem, the solution percentage based positioning for group children, phase 1 root group container quick win, phase 2 children use percentage positioning. Keep the problem and the solution percentage based positioning for group children separate. Allow phase 1 root group container quick win and phase 2 children use percentage positioning to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the problem, the solution percentage based positioning for group children, phase 1 root group container quick win, phase 2 children use percentage positioning.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

