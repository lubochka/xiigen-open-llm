# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; document examination guidance knowledge base extraction is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, document examination guidance knowledge base extraction. Keep impact explanation and reprocess instruction separate. Allow approval boundary and document examination guidance knowledge base extraction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, document examination guidance knowledge base extraction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; document examination guidance knowledge base extraction is present and needs its own handling; the record format is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, document examination guidance knowledge base extraction, the record format. Keep reprocess instruction and approval boundary separate. Allow document examination guidance knowledge base extraction and the record format to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, document examination guidance knowledge base extraction, the record format.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: approval boundary is present and needs its own handling; document examination guidance knowledge base extraction is present and needs its own handling; the record format is present and needs its own handling; how to classify the quality signal is present and needs its own handling.

B: Create work pieces for approval boundary, document examination guidance knowledge base extraction, the record format, how to classify the quality signal. Keep approval boundary and document examination guidance knowledge base extraction separate. Allow the record format and how to classify the quality signal to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, document examination guidance knowledge base extraction, the record format, how to classify the quality signal.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: document examination guidance knowledge base extraction is present and needs its own handling; the record format is present and needs its own handling; how to classify the quality signal is present and needs its own handling; document type classification is present and needs its own handling.

B: Create work pieces for document examination guidance knowledge base extraction, the record format, how to classify the quality signal, document type classification. Keep document examination guidance knowledge base extraction and the record format separate. Allow how to classify the quality signal and document type classification to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: document examination guidance knowledge base extraction, the record format, how to classify the quality signal, document type classification.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Document Examination Guidance — Knowledge Base Extraction: the record format is present and needs its own handling; how to classify the quality signal is present and needs its own handling; document type classification is present and needs its own handling; how to write a good task description is present and needs its own handling.

B: Create work pieces for the record format, how to classify the quality signal, document type classification, how to write a good task description. Keep the record format and how to classify the quality signal separate. Allow document type classification and how to write a good task description to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the record format, how to classify the quality signal, document type classification, how to write a good task description.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

