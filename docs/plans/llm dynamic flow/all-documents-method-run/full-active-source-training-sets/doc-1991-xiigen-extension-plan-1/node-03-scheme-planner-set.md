# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen extension plan is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen extension plan. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen extension plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen extension plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen extension plan is present and needs its own handling; date 2026 03 24 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen extension plan, date 2026 03 24. Keep reprocess instruction and approval boundary separate. Allow xiigen extension plan and date 2026 03 24 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen extension plan, date 2026 03 24.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: approval boundary is present and needs its own handling; xiigen extension plan is present and needs its own handling; date 2026 03 24 is present and needs its own handling; what this plan covers is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen extension plan, date 2026 03 24, what this plan covers. Keep approval boundary and xiigen extension plan separate. Allow date 2026 03 24 and what this plan covers to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen extension plan, date 2026 03 24, what this plan covers.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: xiigen extension plan is present and needs its own handling; date 2026 03 24 is present and needs its own handling; what this plan covers is present and needs its own handling; phase zero engine correctness is present and needs its own handling.

B: Create work pieces for xiigen extension plan, date 2026 03 24, what this plan covers, phase zero engine correctness. Keep xiigen extension plan and date 2026 03 24 separate. Allow what this plan covers and phase zero engine correctness to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen extension plan, date 2026 03 24, what this plan covers, phase zero engine correctness.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGEN EXTENSION PLAN: date 2026 03 24 is present and needs its own handling; what this plan covers is present and needs its own handling; phase zero engine correctness is present and needs its own handling; 5 6 sessions tier 0 blocks everything tier 1 silent corruption is present and needs its own handling.

B: Create work pieces for date 2026 03 24, what this plan covers, phase zero engine correctness, 5 6 sessions tier 0 blocks everything tier 1 silent corruption. Keep date 2026 03 24 and what this plan covers separate. Allow phase zero engine correctness and 5 6 sessions tier 0 blocks everything tier 1 silent corruption to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: date 2026 03 24, what this plan covers, phase zero engine correctness, 5 6 sessions tier 0 blocks everything tier 1 silent corruption.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

