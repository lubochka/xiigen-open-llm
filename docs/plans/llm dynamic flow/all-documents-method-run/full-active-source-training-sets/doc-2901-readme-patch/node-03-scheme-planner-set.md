# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; rev 3 4 patch phase transition protocol fix is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, rev 3 4 patch phase transition protocol fix. Keep impact explanation and reprocess instruction separate. Allow approval boundary and rev 3 4 patch phase transition protocol fix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, rev 3 4 patch phase transition protocol fix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; rev 3 4 patch phase transition protocol fix is present and needs its own handling; apply between sessions at any stop point is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, rev 3 4 patch phase transition protocol fix, apply between sessions at any stop point. Keep reprocess instruction and approval boundary separate. Allow rev 3 4 patch phase transition protocol fix and apply between sessions at any stop point to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, rev 3 4 patch phase transition protocol fix, apply between sessions at any stop point.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: approval boundary is present and needs its own handling; rev 3 4 patch phase transition protocol fix is present and needs its own handling; apply between sessions at any stop point is present and needs its own handling; what this fixes is present and needs its own handling.

B: Create work pieces for approval boundary, rev 3 4 patch phase transition protocol fix, apply between sessions at any stop point, what this fixes. Keep approval boundary and rev 3 4 patch phase transition protocol fix separate. Allow apply between sessions at any stop point and what this fixes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, rev 3 4 patch phase transition protocol fix, apply between sessions at any stop point, what this fixes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: rev 3 4 patch phase transition protocol fix is present and needs its own handling; apply between sessions at any stop point is present and needs its own handling; what this fixes is present and needs its own handling; files in this patch is present and needs its own handling.

B: Create work pieces for rev 3 4 patch phase transition protocol fix, apply between sessions at any stop point, what this fixes, files in this patch. Keep rev 3 4 patch phase transition protocol fix and apply between sessions at any stop point separate. Allow what this fixes and files in this patch to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: rev 3 4 patch phase transition protocol fix, apply between sessions at any stop point, what this fixes, files in this patch.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Rev 3.4 Patch — Phase Transition Protocol Fix: apply between sessions at any stop point is present and needs its own handling; what this fixes is present and needs its own handling; files in this patch is present and needs its own handling; how to apply is present and needs its own handling.

B: Create work pieces for apply between sessions at any stop point, what this fixes, files in this patch, how to apply. Keep apply between sessions at any stop point and what this fixes separate. Allow files in this patch and how to apply to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: apply between sessions at any stop point, what this fixes, files in this patch, how to apply.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

