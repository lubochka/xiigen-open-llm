# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan key provisioning ui is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, plan key provisioning ui. Keep impact explanation and reprocess instruction separate. Allow approval boundary and plan key provisioning ui to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, plan key provisioning ui.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan key provisioning ui is present and needs its own handling; session type generation is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, plan key provisioning ui, session type generation. Keep reprocess instruction and approval boundary separate. Allow plan key provisioning ui and session type generation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, plan key provisioning ui, session type generation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: approval boundary is present and needs its own handling; plan key provisioning ui is present and needs its own handling; session type generation is present and needs its own handling; branch crazy shannon is present and needs its own handling.

B: Create work pieces for approval boundary, plan key provisioning ui, session type generation, branch crazy shannon. Keep approval boundary and plan key provisioning ui separate. Allow session type generation and branch crazy shannon to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, plan key provisioning ui, session type generation, branch crazy shannon.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: plan key provisioning ui is present and needs its own handling; session type generation is present and needs its own handling; branch crazy shannon is present and needs its own handling; scope 8 files 280 lines total is present and needs its own handling.

B: Create work pieces for plan key provisioning ui, session type generation, branch crazy shannon, scope 8 files 280 lines total. Keep plan key provisioning ui and session type generation separate. Allow branch crazy shannon and scope 8 files 280 lines total to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: plan key provisioning ui, session type generation, branch crazy shannon, scope 8 files 280 lines total.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PLAN: Key Provisioning UI: session type generation is present and needs its own handling; branch crazy shannon is present and needs its own handling; scope 8 files 280 lines total is present and needs its own handling; date 2026 04 06 is present and needs its own handling.

B: Create work pieces for session type generation, branch crazy shannon, scope 8 files 280 lines total, date 2026 04 06. Keep session type generation and branch crazy shannon separate. Allow scope 8 files 280 lines total and date 2026 04 06 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: session type generation, branch crazy shannon, scope 8 files 280 lines total, date 2026 04 06.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

