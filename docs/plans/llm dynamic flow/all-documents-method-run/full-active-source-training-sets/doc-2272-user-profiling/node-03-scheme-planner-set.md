# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; user profiling detection heuristics reference is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, user profiling detection heuristics reference. Keep graph relationships and evidence selection separate. Allow quality measurement and user profiling detection heuristics reference to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, user profiling detection heuristics reference.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; user profiling detection heuristics reference is present and needs its own handling; how to use this document is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, user profiling detection heuristics reference, how to use this document. Keep evidence selection and quality measurement separate. Allow user profiling detection heuristics reference and how to use this document to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, user profiling detection heuristics reference, how to use this document.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: quality measurement is present and needs its own handling; user profiling detection heuristics reference is present and needs its own handling; how to use this document is present and needs its own handling; dimensions is present and needs its own handling.

B: Create work pieces for quality measurement, user profiling detection heuristics reference, how to use this document, dimensions. Keep quality measurement and user profiling detection heuristics reference separate. Allow how to use this document and dimensions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, user profiling detection heuristics reference, how to use this document, dimensions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: user profiling detection heuristics reference is present and needs its own handling; how to use this document is present and needs its own handling; dimensions is present and needs its own handling; communication style is present and needs its own handling.

B: Create work pieces for user profiling detection heuristics reference, how to use this document, dimensions, communication style. Keep user profiling detection heuristics reference and how to use this document separate. Allow dimensions and communication style to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: user profiling detection heuristics reference, how to use this document, dimensions, communication style.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for User Profiling: Detection Heuristics Reference: how to use this document is present and needs its own handling; dimensions is present and needs its own handling; communication style is present and needs its own handling; unscored 0 messages with relevant signals for this dimension is present and needs its own handling.

B: Create work pieces for how to use this document, dimensions, communication style, unscored 0 messages with relevant signals for this dimension. Keep how to use this document and dimensions separate. Allow communication style and unscored 0 messages with relevant signals for this dimension to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: how to use this document, dimensions, communication style, unscored 0 messages with relevant signals for this dimension.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

