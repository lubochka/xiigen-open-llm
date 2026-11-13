# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; document rag index flow mobility and authorization package is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, document rag index flow mobility and authorization package. Keep impact explanation and reprocess instruction separate. Allow approval boundary and document rag index flow mobility and authorization package to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, document rag index flow mobility and authorization package.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; document rag index flow mobility and authorization package is present and needs its own handling; machine readable lookup for all documents in this package is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, document rag index flow mobility and authorization package, machine readable lookup for all documents in this package. Keep reprocess instruction and approval boundary separate. Allow document rag index flow mobility and authorization package and machine readable lookup for all documents in this package to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, document rag index flow mobility and authorization package, machine readable lookup for all documents in this package.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: approval boundary is present and needs its own handling; document rag index flow mobility and authorization package is present and needs its own handling; machine readable lookup for all documents in this package is present and needs its own handling; document registry is present and needs its own handling.

B: Create work pieces for approval boundary, document rag index flow mobility and authorization package, machine readable lookup for all documents in this package, document registry. Keep approval boundary and document rag index flow mobility and authorization package separate. Allow machine readable lookup for all documents in this package and document registry to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, document rag index flow mobility and authorization package, machine readable lookup for all documents in this package, document registry.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: document rag index flow mobility and authorization package is present and needs its own handling; machine readable lookup for all documents in this package is present and needs its own handling; document registry is present and needs its own handling; superseded documents do not use for execution is present and needs its own handling.

B: Create work pieces for document rag index flow mobility and authorization package, machine readable lookup for all documents in this package, document registry, superseded documents do not use for execution. Keep document rag index flow mobility and authorization package and machine readable lookup for all documents in this package separate. Allow document registry and superseded documents do not use for execution to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: document rag index flow mobility and authorization package, machine readable lookup for all documents in this package, document registry, superseded documents do not use for execution.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Document RAG Index — Flow Mobility and Authorization Package: machine readable lookup for all documents in this package is present and needs its own handling; document registry is present and needs its own handling; superseded documents do not use for execution is present and needs its own handling; topic lookup find the right document for a given question is present and needs its own handling.

B: Create work pieces for machine readable lookup for all documents in this package, document registry, superseded documents do not use for execution, topic lookup find the right document for a given question. Keep machine readable lookup for all documents in this package and document registry separate. Allow superseded documents do not use for execution and topic lookup find the right document for a given question to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: machine readable lookup for all documents in this package, document registry, superseded documents do not use for execution, topic lookup find the right document for a given question.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

