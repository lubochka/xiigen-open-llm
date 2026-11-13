# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; extending the existing engine to support flow creation is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, extending the existing engine to support flow creation. Keep impact explanation and reprocess instruction separate. Allow approval boundary and extending the existing engine to support flow creation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, extending the existing engine to support flow creation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; extending the existing engine to support flow creation is present and needs its own handling; executive summary is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, extending the existing engine to support flow creation, executive summary. Keep reprocess instruction and approval boundary separate. Allow extending the existing engine to support flow creation and executive summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, extending the existing engine to support flow creation, executive summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: approval boundary is present and needs its own handling; extending the existing engine to support flow creation is present and needs its own handling; executive summary is present and needs its own handling; evidence base and interpretation of flow creation is present and needs its own handling.

B: Create work pieces for approval boundary, extending the existing engine to support flow creation, executive summary, evidence base and interpretation of flow creation. Keep approval boundary and extending the existing engine to support flow creation separate. Allow executive summary and evidence base and interpretation of flow creation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, extending the existing engine to support flow creation, executive summary, evidence base and interpretation of flow creation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: extending the existing engine to support flow creation is present and needs its own handling; executive summary is present and needs its own handling; evidence base and interpretation of flow creation is present and needs its own handling; what the current 20 evidence implies about flows is present and needs its own handling.

B: Create work pieces for extending the existing engine to support flow creation, executive summary, evidence base and interpretation of flow creation, what the current 20 evidence implies about flows. Keep extending the existing engine to support flow creation and executive summary separate. Allow evidence base and interpretation of flow creation and what the current 20 evidence implies about flows to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: extending the existing engine to support flow creation, executive summary, evidence base and interpretation of flow creation, what the current 20 evidence implies about flows.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Extending the Existing Engine to Support Flow Creation: executive summary is present and needs its own handling; evidence base and interpretation of flow creation is present and needs its own handling; what the current 20 evidence implies about flows is present and needs its own handling; operational definition used in this report is present and needs its own handling.

B: Create work pieces for executive summary, evidence base and interpretation of flow creation, what the current 20 evidence implies about flows, operational definition used in this report. Keep executive summary and evidence base and interpretation of flow creation separate. Allow what the current 20 evidence implies about flows and operational definition used in this report to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: executive summary, evidence base and interpretation of flow creation, what the current 20 evidence implies about flows, operational definition used in this report.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

