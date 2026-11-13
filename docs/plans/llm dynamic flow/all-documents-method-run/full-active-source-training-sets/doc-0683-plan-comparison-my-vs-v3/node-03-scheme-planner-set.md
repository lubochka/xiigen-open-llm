# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan comparison my plan vs v3 ground truth is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, plan comparison my plan vs v3 ground truth. Keep impact explanation and reprocess instruction separate. Allow approval boundary and plan comparison my plan vs v3 ground truth to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, plan comparison my plan vs v3 ground truth.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plan comparison my plan vs v3 ground truth is present and needs its own handling; verdict my plan was wrong on root cause v3 is correct is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, plan comparison my plan vs v3 ground truth, verdict my plan was wrong on root cause v3 is correct. Keep reprocess instruction and approval boundary separate. Allow plan comparison my plan vs v3 ground truth and verdict my plan was wrong on root cause v3 is correct to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, plan comparison my plan vs v3 ground truth, verdict my plan was wrong on root cause v3 is correct.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: approval boundary is present and needs its own handling; plan comparison my plan vs v3 ground truth is present and needs its own handling; verdict my plan was wrong on root cause v3 is correct is present and needs its own handling; root cause the critical difference is present and needs its own handling.

B: Create work pieces for approval boundary, plan comparison my plan vs v3 ground truth, verdict my plan was wrong on root cause v3 is correct, root cause the critical difference. Keep approval boundary and plan comparison my plan vs v3 ground truth separate. Allow verdict my plan was wrong on root cause v3 is correct and root cause the critical difference to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, plan comparison my plan vs v3 ground truth, verdict my plan was wrong on root cause v3 is correct, root cause the critical difference.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: plan comparison my plan vs v3 ground truth is present and needs its own handling; verdict my plan was wrong on root cause v3 is correct is present and needs its own handling; root cause the critical difference is present and needs its own handling; my plan said is present and needs its own handling.

B: Create work pieces for plan comparison my plan vs v3 ground truth, verdict my plan was wrong on root cause v3 is correct, root cause the critical difference, my plan said. Keep plan comparison my plan vs v3 ground truth and verdict my plan was wrong on root cause v3 is correct separate. Allow root cause the critical difference and my plan said to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: plan comparison my plan vs v3 ground truth, verdict my plan was wrong on root cause v3 is correct, root cause the critical difference, my plan said.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PLAN COMPARISON: My Plan vs v3 Ground Truth: verdict my plan was wrong on root cause v3 is correct is present and needs its own handling; root cause the critical difference is present and needs its own handling; my plan said is present and needs its own handling; v3 says is present and needs its own handling.

B: Create work pieces for verdict my plan was wrong on root cause v3 is correct, root cause the critical difference, my plan said, v3 says. Keep verdict my plan was wrong on root cause v3 is correct and root cause the critical difference separate. Allow my plan said and v3 says to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: verdict my plan was wrong on root cause v3 is correct, root cause the critical difference, my plan said, v3 says.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

