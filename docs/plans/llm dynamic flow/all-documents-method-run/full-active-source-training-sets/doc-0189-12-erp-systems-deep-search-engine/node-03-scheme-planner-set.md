# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; extending the engine to support erp style flow creation is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, extending the engine to support erp style flow creation. Keep graph relationships and evidence selection separate. Allow quality measurement and extending the engine to support erp style flow creation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, extending the engine to support erp style flow creation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; extending the engine to support erp style flow creation is present and needs its own handling; executive summary is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, extending the engine to support erp style flow creation, executive summary. Keep evidence selection and quality measurement separate. Allow extending the engine to support erp style flow creation and executive summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, extending the engine to support erp style flow creation, executive summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: quality measurement is present and needs its own handling; extending the engine to support erp style flow creation is present and needs its own handling; executive summary is present and needs its own handling; source derived requirements and gaps is present and needs its own handling.

B: Create work pieces for quality measurement, extending the engine to support erp style flow creation, executive summary, source derived requirements and gaps. Keep quality measurement and extending the engine to support erp style flow creation separate. Allow executive summary and source derived requirements and gaps to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, extending the engine to support erp style flow creation, executive summary, source derived requirements and gaps.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: extending the engine to support erp style flow creation is present and needs its own handling; executive summary is present and needs its own handling; source derived requirements and gaps is present and needs its own handling; what the available 12 sources say the flow must express is present and needs its own handling.

B: Create work pieces for extending the engine to support erp style flow creation, executive summary, source derived requirements and gaps, what the available 12 sources say the flow must express. Keep extending the engine to support erp style flow creation and executive summary separate. Allow source derived requirements and gaps and what the available 12 sources say the flow must express to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: extending the engine to support erp style flow creation, executive summary, source derived requirements and gaps, what the available 12 sources say the flow must express.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Extending the Engine to Support ERP-Style Flow Creation: executive summary is present and needs its own handling; source derived requirements and gaps is present and needs its own handling; what the available 12 sources say the flow must express is present and needs its own handling; gaps and missing details is present and needs its own handling.

B: Create work pieces for executive summary, source derived requirements and gaps, what the available 12 sources say the flow must express, gaps and missing details. Keep executive summary and source derived requirements and gaps separate. Allow what the available 12 sources say the flow must express and gaps and missing details to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: executive summary, source derived requirements and gaps, what the available 12 sources say the flow must express, gaps and missing details.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

