# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; complete documentation update plan is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, complete documentation update plan. Keep impact explanation and reprocess instruction separate. Allow approval boundary and complete documentation update plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, complete documentation update plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; complete documentation update plan is present and needs its own handling; all files exact gaps what to write is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, complete documentation update plan, all files exact gaps what to write. Keep reprocess instruction and approval boundary separate. Allow complete documentation update plan and all files exact gaps what to write to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, complete documentation update plan, all files exact gaps what to write.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: approval boundary is present and needs its own handling; complete documentation update plan is present and needs its own handling; all files exact gaps what to write is present and needs its own handling; summary what docs know vs what was built is present and needs its own handling.

B: Create work pieces for approval boundary, complete documentation update plan, all files exact gaps what to write, summary what docs know vs what was built. Keep approval boundary and complete documentation update plan separate. Allow all files exact gaps what to write and summary what docs know vs what was built to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, complete documentation update plan, all files exact gaps what to write, summary what docs know vs what was built.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: complete documentation update plan is present and needs its own handling; all files exact gaps what to write is present and needs its own handling; summary what docs know vs what was built is present and needs its own handling; file by file gap analysis is present and needs its own handling.

B: Create work pieces for complete documentation update plan, all files exact gaps what to write, summary what docs know vs what was built, file by file gap analysis. Keep complete documentation update plan and all files exact gaps what to write separate. Allow summary what docs know vs what was built and file by file gap analysis to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: complete documentation update plan, all files exact gaps what to write, summary what docs know vs what was built, file by file gap analysis.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Complete Documentation Update Plan: all files exact gaps what to write is present and needs its own handling; summary what docs know vs what was built is present and needs its own handling; file by file gap analysis is present and needs its own handling; complete rewrite needed is present and needs its own handling.

B: Create work pieces for all files exact gaps what to write, summary what docs know vs what was built, file by file gap analysis, complete rewrite needed. Keep all files exact gaps what to write and summary what docs know vs what was built separate. Allow file by file gap analysis and complete rewrite needed to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: all files exact gaps what to write, summary what docs know vs what was built, file by file gap analysis, complete rewrite needed.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

