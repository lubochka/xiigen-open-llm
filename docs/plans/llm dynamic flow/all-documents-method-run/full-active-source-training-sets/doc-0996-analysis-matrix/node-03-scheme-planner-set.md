# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; analysis matrix is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, analysis matrix. Keep impact explanation and reprocess instruction separate. Allow approval boundary and analysis matrix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, analysis matrix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; analysis matrix is present and needs its own handling; what was uploaded is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, analysis matrix, what was uploaded. Keep reprocess instruction and approval boundary separate. Allow analysis matrix and what was uploaded to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, analysis matrix, what was uploaded.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: approval boundary is present and needs its own handling; analysis matrix is present and needs its own handling; what was uploaded is present and needs its own handling; flow mobility auth package file by file analysis is present and needs its own handling.

B: Create work pieces for approval boundary, analysis matrix, what was uploaded, flow mobility auth package file by file analysis. Keep approval boundary and analysis matrix separate. Allow what was uploaded and flow mobility auth package file by file analysis to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, analysis matrix, what was uploaded, flow mobility auth package file by file analysis.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: analysis matrix is present and needs its own handling; what was uploaded is present and needs its own handling; flow mobility auth package file by file analysis is present and needs its own handling; orchestrator learning sets file by file analysis is present and needs its own handling.

B: Create work pieces for analysis matrix, what was uploaded, flow mobility auth package file by file analysis, orchestrator learning sets file by file analysis. Keep analysis matrix and what was uploaded separate. Allow flow mobility auth package file by file analysis and orchestrator learning sets file by file analysis to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: analysis matrix, what was uploaded, flow mobility auth package file by file analysis, orchestrator learning sets file by file analysis.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Analysis Matrix: what was uploaded is present and needs its own handling; flow mobility auth package file by file analysis is present and needs its own handling; orchestrator learning sets file by file analysis is present and needs its own handling; flow portability prompt analysis is present and needs its own handling.

B: Create work pieces for what was uploaded, flow mobility auth package file by file analysis, orchestrator learning sets file by file analysis, flow portability prompt analysis. Keep what was uploaded and flow mobility auth package file by file analysis separate. Allow orchestrator learning sets file by file analysis and flow portability prompt analysis to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what was uploaded, flow mobility auth package file by file analysis, orchestrator learning sets file by file analysis, flow portability prompt analysis.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

