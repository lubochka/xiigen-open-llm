# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; phase 4 complete memory library is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, phase 4 complete memory library. Keep impact explanation and reprocess instruction separate. Allow approval boundary and phase 4 complete memory library to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, phase 4 complete memory library.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; phase 4 complete memory library is present and needs its own handling; what was done is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, phase 4 complete memory library, what was done. Keep reprocess instruction and approval boundary separate. Allow phase 4 complete memory library and what was done to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, phase 4 complete memory library, what was done.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: approval boundary is present and needs its own handling; phase 4 complete memory library is present and needs its own handling; what was done is present and needs its own handling; skill 00a the retrieval system interfaces is present and needs its own handling.

B: Create work pieces for approval boundary, phase 4 complete memory library, what was done, skill 00a the retrieval system interfaces. Keep approval boundary and phase 4 complete memory library separate. Allow what was done and skill 00a the retrieval system interfaces to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, phase 4 complete memory library, what was done, skill 00a the retrieval system interfaces.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: phase 4 complete memory library is present and needs its own handling; what was done is present and needs its own handling; skill 00a the retrieval system interfaces is present and needs its own handling; skill 00b the retrieval system planner is present and needs its own handling.

B: Create work pieces for phase 4 complete memory library, what was done, skill 00a the retrieval system interfaces, skill 00b the retrieval system planner. Keep phase 4 complete memory library and what was done separate. Allow skill 00a the retrieval system interfaces and skill 00b the retrieval system planner to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: phase 4 complete memory library, what was done, skill 00a the retrieval system interfaces, skill 00b the retrieval system planner.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Phase 4 Complete — Memory Library: what was done is present and needs its own handling; skill 00a the retrieval system interfaces is present and needs its own handling; skill 00b the retrieval system planner is present and needs its own handling; skill 16 ai context service is present and needs its own handling.

B: Create work pieces for what was done, skill 00a the retrieval system interfaces, skill 00b the retrieval system planner, skill 16 ai context service. Keep what was done and skill 00a the retrieval system interfaces separate. Allow skill 00b the retrieval system planner and skill 16 ai context service to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what was done, skill 00a the retrieval system interfaces, skill 00b the retrieval system planner, skill 16 ai context service.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

