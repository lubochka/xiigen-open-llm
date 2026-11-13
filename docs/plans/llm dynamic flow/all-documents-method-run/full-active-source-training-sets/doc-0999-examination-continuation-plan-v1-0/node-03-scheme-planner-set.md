# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: trigger and input is present and needs its own handling; node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling.

B: Create work pieces for trigger and input, node responsibility, parallel branch, wait and recovery. Keep trigger and input and node responsibility separate. Allow parallel branch and wait and recovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: trigger and input, node responsibility, parallel branch, wait and recovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling.

B: Create work pieces for node responsibility, parallel branch, wait and recovery, result merge. Keep node responsibility and parallel branch separate. Allow wait and recovery and result merge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: node responsibility, parallel branch, wait and recovery, result merge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling.

B: Create work pieces for parallel branch, wait and recovery, result merge, review before output. Keep parallel branch and wait and recovery separate. Allow result merge and review before output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: parallel branch, wait and recovery, result merge, review before output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling; document examination to training sets final method is present and needs its own handling.

B: Create work pieces for wait and recovery, result merge, review before output, document examination to training sets final method. Keep wait and recovery and result merge separate. Allow review before output and document examination to training sets final method to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: wait and recovery, result merge, review before output, document examination to training sets final method.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: result merge is present and needs its own handling; review before output is present and needs its own handling; document examination to training sets final method is present and needs its own handling; what this document is is present and needs its own handling.

B: Create work pieces for result merge, review before output, document examination to training sets final method, what this document is. Keep result merge and review before output separate. Allow document examination to training sets final method and what this document is to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: result merge, review before output, document examination to training sets final method, what this document is.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: review before output is present and needs its own handling; document examination to training sets final method is present and needs its own handling; what this document is is present and needs its own handling; why the previous sets failed is present and needs its own handling.

B: Create work pieces for review before output, document examination to training sets final method, what this document is, why the previous sets failed. Keep review before output and document examination to training sets final method separate. Allow what this document is and why the previous sets failed to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: review before output, document examination to training sets final method, what this document is, why the previous sets failed.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: document examination to training sets final method is present and needs its own handling; what this document is is present and needs its own handling; why the previous sets failed is present and needs its own handling; the pipeline is present and needs its own handling.

B: Create work pieces for document examination to training sets final method, what this document is, why the previous sets failed, the pipeline. Keep document examination to training sets final method and what this document is separate. Allow why the previous sets failed and the pipeline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: document examination to training sets final method, what this document is, why the previous sets failed, the pipeline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Document Examination to Training Sets — Final Method: what this document is is present and needs its own handling; why the previous sets failed is present and needs its own handling; the pipeline is present and needs its own handling; what a and b mean is present and needs its own handling.

B: Create work pieces for what this document is, why the previous sets failed, the pipeline, what a and b mean. Keep what this document is and why the previous sets failed separate. Allow the pipeline and what a and b mean to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what this document is, why the previous sets failed, the pipeline, what a and b mean.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

