# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; for claude code per document execution all four collections is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, for claude code per document execution all four collections. Keep impact explanation and reprocess instruction separate. Allow approval boundary and for claude code per document execution all four collections to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, for claude code per document execution all four collections.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; for claude code per document execution all four collections is present and needs its own handling; purpose extract task description high level steps from each file is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, for claude code per document execution all four collections, purpose extract task description high level steps from each file. Keep reprocess instruction and approval boundary separate. Allow for claude code per document execution all four collections and purpose extract task description high level steps from each file to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, for claude code per document execution all four collections, purpose extract task description high level steps from each file.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: approval boundary is present and needs its own handling; for claude code per document execution all four collections is present and needs its own handling; purpose extract task description high level steps from each file is present and needs its own handling; output structured json record ready for lightrag ingestion is present and needs its own handling.

B: Create work pieces for approval boundary, for claude code per document execution all four collections, purpose extract task description high level steps from each file, output structured json record ready for lightrag ingestion. Keep approval boundary and for claude code per document execution all four collections separate. Allow purpose extract task description high level steps from each file and output structured json record ready for lightrag ingestion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, for claude code per document execution all four collections, purpose extract task description high level steps from each file, output structured json record ready for lightrag ingestion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: for claude code per document execution all four collections is present and needs its own handling; purpose extract task description high level steps from each file is present and needs its own handling; output structured json record ready for lightrag ingestion is present and needs its own handling; what this guidance produces is present and needs its own handling.

B: Create work pieces for for claude code per document execution all four collections, purpose extract task description high level steps from each file, output structured json record ready for lightrag ingestion, what this guidance produces. Keep for claude code per document execution all four collections and purpose extract task description high level steps from each file separate. Allow output structured json record ready for lightrag ingestion and what this guidance produces to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: for claude code per document execution all four collections, purpose extract task description high level steps from each file, output structured json record ready for lightrag ingestion, what this guidance produces.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — the retrieval system Corpus Extraction: purpose extract task description high level steps from each file is present and needs its own handling; output structured json record ready for lightrag ingestion is present and needs its own handling; what this guidance produces is present and needs its own handling; step 1 classify the document type is present and needs its own handling.

B: Create work pieces for purpose extract task description high level steps from each file, output structured json record ready for lightrag ingestion, what this guidance produces, step 1 classify the document type. Keep purpose extract task description high level steps from each file and output structured json record ready for lightrag ingestion separate. Allow what this guidance produces and step 1 classify the document type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: purpose extract task description high level steps from each file, output structured json record ready for lightrag ingestion, what this guidance produces, step 1 classify the document type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

