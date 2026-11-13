# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; extending the engine to support declarative flow creation is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, extending the engine to support declarative flow creation. Keep impact explanation and reprocess instruction separate. Allow approval boundary and extending the engine to support declarative flow creation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, extending the engine to support declarative flow creation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; extending the engine to support declarative flow creation is present and needs its own handling; executive summary is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, extending the engine to support declarative flow creation, executive summary. Keep reprocess instruction and approval boundary separate. Allow extending the engine to support declarative flow creation and executive summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, extending the engine to support declarative flow creation, executive summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: approval boundary is present and needs its own handling; extending the engine to support declarative flow creation is present and needs its own handling; executive summary is present and needs its own handling; source review and assumptions is present and needs its own handling.

B: Create work pieces for approval boundary, extending the engine to support declarative flow creation, executive summary, source review and assumptions. Keep approval boundary and extending the engine to support declarative flow creation separate. Allow executive summary and source review and assumptions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, extending the engine to support declarative flow creation, executive summary, source review and assumptions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: extending the engine to support declarative flow creation is present and needs its own handling; executive summary is present and needs its own handling; source review and assumptions is present and needs its own handling; flow requirements model from the 01 documents is present and needs its own handling.

B: Create work pieces for extending the engine to support declarative flow creation, executive summary, source review and assumptions, flow requirements model from the 01 documents. Keep extending the engine to support declarative flow creation and executive summary separate. Allow source review and assumptions and flow requirements model from the 01 documents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: extending the engine to support declarative flow creation, executive summary, source review and assumptions, flow requirements model from the 01 documents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support Declarative Flow Creation: executive summary is present and needs its own handling; source review and assumptions is present and needs its own handling; flow requirements model from the 01 documents is present and needs its own handling; core primitives implied by the user registration flow is present and needs its own handling.

B: Create work pieces for executive summary, source review and assumptions, flow requirements model from the 01 documents, core primitives implied by the user registration flow. Keep executive summary and source review and assumptions separate. Allow flow requirements model from the 01 documents and core primitives implied by the user registration flow to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: executive summary, source review and assumptions, flow requirements model from the 01 documents, core primitives implied by the user registration flow.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

