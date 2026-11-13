# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen master gap summary is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen master gap summary. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen master gap summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen master gap summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen master gap summary is present and needs its own handling; three gap categories is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen master gap summary, three gap categories. Keep reprocess instruction and approval boundary separate. Allow xiigen master gap summary and three gap categories to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen master gap summary, three gap categories.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: approval boundary is present and needs its own handling; xiigen master gap summary is present and needs its own handling; three gap categories is present and needs its own handling; a vision gaps xiigen vs the draw io end state is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen master gap summary, three gap categories, a vision gaps xiigen vs the draw io end state. Keep approval boundary and xiigen master gap summary separate. Allow three gap categories and a vision gaps xiigen vs the draw io end state to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen master gap summary, three gap categories, a vision gaps xiigen vs the draw io end state.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: xiigen master gap summary is present and needs its own handling; three gap categories is present and needs its own handling; a vision gaps xiigen vs the draw io end state is present and needs its own handling; b infrastructure gaps what the running engine is missing is present and needs its own handling.

B: Create work pieces for xiigen master gap summary, three gap categories, a vision gaps xiigen vs the draw io end state, b infrastructure gaps what the running engine is missing. Keep xiigen master gap summary and three gap categories separate. Allow a vision gaps xiigen vs the draw io end state and b infrastructure gaps what the running engine is missing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen master gap summary, three gap categories, a vision gaps xiigen vs the draw io end state, b infrastructure gaps what the running engine is missing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGEN — MASTER GAP SUMMARY: three gap categories is present and needs its own handling; a vision gaps xiigen vs the draw io end state is present and needs its own handling; b infrastructure gaps what the running engine is missing is present and needs its own handling; reading order is present and needs its own handling.

B: Create work pieces for three gap categories, a vision gaps xiigen vs the draw io end state, b infrastructure gaps what the running engine is missing, reading order. Keep three gap categories and a vision gaps xiigen vs the draw io end state separate. Allow b infrastructure gaps what the running engine is missing and reading order to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: three gap categories, a vision gaps xiigen vs the draw io end state, b infrastructure gaps what the running engine is missing, reading order.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

