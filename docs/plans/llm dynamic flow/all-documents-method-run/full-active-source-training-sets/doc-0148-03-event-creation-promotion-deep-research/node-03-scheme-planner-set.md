# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; executive summary is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, executive summary. Keep impact explanation and reprocess instruction separate. Allow approval boundary and executive summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, executive summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; executive summary is present and needs its own handling; sources scope and explicit unknowns is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, executive summary, sources scope and explicit unknowns. Keep reprocess instruction and approval boundary separate. Allow executive summary and sources scope and explicit unknowns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, executive summary, sources scope and explicit unknowns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: approval boundary is present and needs its own handling; executive summary is present and needs its own handling; sources scope and explicit unknowns is present and needs its own handling; what was available and used is present and needs its own handling.

B: Create work pieces for approval boundary, executive summary, sources scope and explicit unknowns, what was available and used. Keep approval boundary and executive summary separate. Allow sources scope and explicit unknowns and what was available and used to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, executive summary, sources scope and explicit unknowns, what was available and used.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: executive summary is present and needs its own handling; sources scope and explicit unknowns is present and needs its own handling; what was available and used is present and needs its own handling; what was not available and therefore treated as unspecified is present and needs its own handling.

B: Create work pieces for executive summary, sources scope and explicit unknowns, what was available and used, what was not available and therefore treated as unspecified. Keep executive summary and sources scope and explicit unknowns separate. Allow what was available and used and what was not available and therefore treated as unspecified to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: executive summary, sources scope and explicit unknowns, what was available and used, what was not available and therefore treated as unspecified.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Extending the XIIGen Unified Flow System to Support the event creation flow Flow Creation and Execution: sources scope and explicit unknowns is present and needs its own handling; what was available and used is present and needs its own handling; what was not available and therefore treated as unspecified is present and needs its own handling; the event creation flow functional requirements inventory is present and needs its own handling.

B: Create work pieces for sources scope and explicit unknowns, what was available and used, what was not available and therefore treated as unspecified, the event creation flow functional requirements inventory. Keep sources scope and explicit unknowns and what was available and used separate. Allow what was not available and therefore treated as unspecified and the event creation flow functional requirements inventory to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: sources scope and explicit unknowns, what was available and used, what was not available and therefore treated as unspecified, the event creation flow functional requirements inventory.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

