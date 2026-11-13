# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; architectural integration is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, architectural integration. Keep impact explanation and reprocess instruction separate. Allow approval boundary and architectural integration to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, architectural integration.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; architectural integration is present and needs its own handling; the arbiter workflow example entity deletion is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, architectural integration, the arbiter workflow example entity deletion. Keep reprocess instruction and approval boundary separate. Allow architectural integration and the arbiter workflow example entity deletion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, architectural integration, the arbiter workflow example entity deletion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: approval boundary is present and needs its own handling; architectural integration is present and needs its own handling; the arbiter workflow example entity deletion is present and needs its own handling; step 1 change detection context gathering is present and needs its own handling.

B: Create work pieces for approval boundary, architectural integration, the arbiter workflow example entity deletion, step 1 change detection context gathering. Keep approval boundary and architectural integration separate. Allow the arbiter workflow example entity deletion and step 1 change detection context gathering to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, architectural integration, the arbiter workflow example entity deletion, step 1 change detection context gathering.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: architectural integration is present and needs its own handling; the arbiter workflow example entity deletion is present and needs its own handling; step 1 change detection context gathering is present and needs its own handling; step 2 ai validation the arbiter prompt is present and needs its own handling.

B: Create work pieces for architectural integration, the arbiter workflow example entity deletion, step 1 change detection context gathering, step 2 ai validation the arbiter prompt. Keep architectural integration and the arbiter workflow example entity deletion separate. Allow step 1 change detection context gathering and step 2 ai validation the arbiter prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: architectural integration, the arbiter workflow example entity deletion, step 1 change detection context gathering, step 2 ai validation the arbiter prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Architectural Integration: the arbiter workflow example entity deletion is present and needs its own handling; step 1 change detection context gathering is present and needs its own handling; step 2 ai validation the arbiter prompt is present and needs its own handling; the aidispatcher runs a specialized evaluation prompt is present and needs its own handling.

B: Create work pieces for the arbiter workflow example entity deletion, step 1 change detection context gathering, step 2 ai validation the arbiter prompt, the aidispatcher runs a specialized evaluation prompt. Keep the arbiter workflow example entity deletion and step 1 change detection context gathering separate. Allow step 2 ai validation the arbiter prompt and the aidispatcher runs a specialized evaluation prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the arbiter workflow example entity deletion, step 1 change detection context gathering, step 2 ai validation the arbiter prompt, the aidispatcher runs a specialized evaluation prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

