# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan validation report is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, plan validation report. Keep impact explanation and reprocess instruction separate. Allow approval boundary and plan validation report to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, plan validation report.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan validation report is present and needs its own handling; date 2026 03 18 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, plan validation report, date 2026 03 18. Keep reprocess instruction and approval boundary separate. Allow plan validation report and date 2026 03 18 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, plan validation report, date 2026 03 18.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: approval boundary is present and needs its own handling; plan validation report is present and needs its own handling; date 2026 03 18 is present and needs its own handling; skill 1 agent output format format enforcement is present and needs its own handling.

B: Create work pieces for approval boundary, plan validation report, date 2026 03 18, skill 1 agent output format format enforcement. Keep approval boundary and plan validation report separate. Allow date 2026 03 18 and skill 1 agent output format format enforcement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, plan validation report, date 2026 03 18, skill 1 agent output format format enforcement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: plan validation report is present and needs its own handling; date 2026 03 18 is present and needs its own handling; skill 1 agent output format format enforcement is present and needs its own handling; skill 2 xiigen core principles gate 0 8 principles is present and needs its own handling.

B: Create work pieces for plan validation report, date 2026 03 18, skill 1 agent output format format enforcement, skill 2 xiigen core principles gate 0 8 principles. Keep plan validation report and date 2026 03 18 separate. Allow skill 1 agent output format format enforcement and skill 2 xiigen core principles gate 0 8 principles to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: plan validation report, date 2026 03 18, skill 1 agent output format format enforcement, skill 2 xiigen core principles gate 0 8 principles.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PLAN VALIDATION REPORT: date 2026 03 18 is present and needs its own handling; skill 1 agent output format format enforcement is present and needs its own handling; skill 2 xiigen core principles gate 0 8 principles is present and needs its own handling; skill 3 infrastructure discovery is present and needs its own handling.

B: Create work pieces for date 2026 03 18, skill 1 agent output format format enforcement, skill 2 xiigen core principles gate 0 8 principles, skill 3 infrastructure discovery. Keep date 2026 03 18 and skill 1 agent output format format enforcement separate. Allow skill 2 xiigen core principles gate 0 8 principles and skill 3 infrastructure discovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: date 2026 03 18, skill 1 agent output format format enforcement, skill 2 xiigen core principles gate 0 8 principles, skill 3 infrastructure discovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

