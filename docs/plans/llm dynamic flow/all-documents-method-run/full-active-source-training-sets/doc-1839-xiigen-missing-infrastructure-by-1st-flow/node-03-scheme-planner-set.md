# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen missing infrastructure features is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen missing infrastructure features. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen missing infrastructure features to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen missing infrastructure features.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen missing infrastructure features is present and needs its own handling; date 2026 03 23 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen missing infrastructure features, date 2026 03 23. Keep reprocess instruction and approval boundary separate. Allow xiigen missing infrastructure features and date 2026 03 23 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen missing infrastructure features, date 2026 03 23.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: approval boundary is present and needs its own handling; xiigen missing infrastructure features is present and needs its own handling; date 2026 03 23 is present and needs its own handling; how to read this document is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen missing infrastructure features, date 2026 03 23, how to read this document. Keep approval boundary and xiigen missing infrastructure features separate. Allow date 2026 03 23 and how to read this document to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen missing infrastructure features, date 2026 03 23, how to read this document.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: xiigen missing infrastructure features is present and needs its own handling; date 2026 03 23 is present and needs its own handling; how to read this document is present and needs its own handling; what it is the feature that doesn t exist is present and needs its own handling.

B: Create work pieces for xiigen missing infrastructure features, date 2026 03 23, how to read this document, what it is the feature that doesn t exist. Keep xiigen missing infrastructure features and date 2026 03 23 separate. Allow how to read this document and what it is the feature that doesn t exist to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen missing infrastructure features, date 2026 03 23, how to read this document, what it is the feature that doesn t exist.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Features: date 2026 03 23 is present and needs its own handling; how to read this document is present and needs its own handling; what it is the feature that doesn t exist is present and needs its own handling; what breaks without it which phase gates fail is present and needs its own handling.

B: Create work pieces for date 2026 03 23, how to read this document, what it is the feature that doesn t exist, what breaks without it which phase gates fail. Keep date 2026 03 23 and how to read this document separate. Allow what it is the feature that doesn t exist and what breaks without it which phase gates fail to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: date 2026 03 23, how to read this document, what it is the feature that doesn t exist, what breaks without it which phase gates fail.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

