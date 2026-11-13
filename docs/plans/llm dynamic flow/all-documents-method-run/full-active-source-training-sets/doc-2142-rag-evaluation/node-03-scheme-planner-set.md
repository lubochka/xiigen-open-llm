# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; the retrieval system evaluation reference is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, the retrieval system evaluation reference. Keep impact explanation and reprocess instruction separate. Allow approval boundary and the retrieval system evaluation reference to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, the retrieval system evaluation reference.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; the retrieval system evaluation reference is present and needs its own handling; table of contents is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, the retrieval system evaluation reference, table of contents. Keep reprocess instruction and approval boundary separate. Allow the retrieval system evaluation reference and table of contents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, the retrieval system evaluation reference, table of contents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: approval boundary is present and needs its own handling; the retrieval system evaluation reference is present and needs its own handling; table of contents is present and needs its own handling; evaluation fundamentals is present and needs its own handling.

B: Create work pieces for approval boundary, the retrieval system evaluation reference, table of contents, evaluation fundamentals. Keep approval boundary and the retrieval system evaluation reference separate. Allow table of contents and evaluation fundamentals to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, the retrieval system evaluation reference, table of contents, evaluation fundamentals.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: the retrieval system evaluation reference is present and needs its own handling; table of contents is present and needs its own handling; evaluation fundamentals is present and needs its own handling; the retrieval system evaluation components is present and needs its own handling.

B: Create work pieces for the retrieval system evaluation reference, table of contents, evaluation fundamentals, the retrieval system evaluation components. Keep the retrieval system evaluation reference and table of contents separate. Allow evaluation fundamentals and the retrieval system evaluation components to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the retrieval system evaluation reference, table of contents, evaluation fundamentals, the retrieval system evaluation components.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The retrieval system Evaluation Reference: table of contents is present and needs its own handling; evaluation fundamentals is present and needs its own handling; the retrieval system evaluation components is present and needs its own handling; ground truth requirements is present and needs its own handling.

B: Create work pieces for table of contents, evaluation fundamentals, the retrieval system evaluation components, ground truth requirements. Keep table of contents and evaluation fundamentals separate. Allow the retrieval system evaluation components and ground truth requirements to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: table of contents, evaluation fundamentals, the retrieval system evaluation components, ground truth requirements.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

