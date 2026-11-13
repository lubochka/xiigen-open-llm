# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen v17 phase 0 complete is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen v17 phase 0 complete. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen v17 phase 0 complete to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen v17 phase 0 complete.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen v17 phase 0 complete is present and needs its own handling; cleanup master documents is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen v17 phase 0 complete, cleanup master documents. Keep reprocess instruction and approval boundary separate. Allow xiigen v17 phase 0 complete and cleanup master documents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen v17 phase 0 complete, cleanup master documents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: approval boundary is present and needs its own handling; xiigen v17 phase 0 complete is present and needs its own handling; cleanup master documents is present and needs its own handling; what was done in phase 0 is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen v17 phase 0 complete, cleanup master documents, what was done in phase 0. Keep approval boundary and xiigen v17 phase 0 complete separate. Allow cleanup master documents and what was done in phase 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen v17 phase 0 complete, cleanup master documents, what was done in phase 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: xiigen v17 phase 0 complete is present and needs its own handling; cleanup master documents is present and needs its own handling; what was done in phase 0 is present and needs its own handling; cleanup is present and needs its own handling.

B: Create work pieces for xiigen v17 phase 0 complete, cleanup master documents, what was done in phase 0, cleanup. Keep xiigen v17 phase 0 complete and cleanup master documents separate. Allow what was done in phase 0 and cleanup to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen v17 phase 0 complete, cleanup master documents, what was done in phase 0, cleanup.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen V17 — Phase 0 COMPLETE: cleanup master documents is present and needs its own handling; what was done in phase 0 is present and needs its own handling; cleanup is present and needs its own handling; master documents added is present and needs its own handling.

B: Create work pieces for cleanup master documents, what was done in phase 0, cleanup, master documents added. Keep cleanup master documents and what was done in phase 0 separate. Allow cleanup and master documents added to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: cleanup master documents, what was done in phase 0, cleanup, master documents added.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

