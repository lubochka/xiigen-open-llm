# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen gap review expected vs actual is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen gap review expected vs actual. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen gap review expected vs actual to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen gap review expected vs actual.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen gap review expected vs actual is present and needs its own handling; session type investigation is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen gap review expected vs actual, session type investigation. Keep reprocess instruction and approval boundary separate. Allow xiigen gap review expected vs actual and session type investigation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen gap review expected vs actual, session type investigation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: approval boundary is present and needs its own handling; xiigen gap review expected vs actual is present and needs its own handling; session type investigation is present and needs its own handling; no code high level analysis only is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen gap review expected vs actual, session type investigation, no code high level analysis only. Keep approval boundary and xiigen gap review expected vs actual separate. Allow session type investigation and no code high level analysis only to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen gap review expected vs actual, session type investigation, no code high level analysis only.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: xiigen gap review expected vs actual is present and needs its own handling; session type investigation is present and needs its own handling; no code high level analysis only is present and needs its own handling; what luba expects is present and needs its own handling.

B: Create work pieces for xiigen gap review expected vs actual, session type investigation, no code high level analysis only, what luba expects. Keep xiigen gap review expected vs actual and session type investigation separate. Allow no code high level analysis only and what luba expects to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen gap review expected vs actual, session type investigation, no code high level analysis only, what luba expects.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGEN — GAP REVIEW: EXPECTED vs ACTUAL: session type investigation is present and needs its own handling; no code high level analysis only is present and needs its own handling; what luba expects is present and needs its own handling; the gap 5 structural absences is present and needs its own handling.

B: Create work pieces for session type investigation, no code high level analysis only, what luba expects, the gap 5 structural absences. Keep session type investigation and no code high level analysis only separate. Allow what luba expects and the gap 5 structural absences to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: session type investigation, no code high level analysis only, what luba expects, the gap 5 structural absences.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

