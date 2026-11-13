# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; embeddings guide for the retrieval system is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, embeddings guide for the retrieval system. Keep graph relationships and evidence selection separate. Allow quality measurement and embeddings guide for the retrieval system to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, embeddings guide for the retrieval system.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; embeddings guide for the retrieval system is present and needs its own handling; table of contents is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, embeddings guide for the retrieval system, table of contents. Keep evidence selection and quality measurement separate. Allow embeddings guide for the retrieval system and table of contents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, embeddings guide for the retrieval system, table of contents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: quality measurement is present and needs its own handling; embeddings guide for the retrieval system is present and needs its own handling; table of contents is present and needs its own handling; overview is present and needs its own handling.

B: Create work pieces for quality measurement, embeddings guide for the retrieval system, table of contents, overview. Keep quality measurement and embeddings guide for the retrieval system separate. Allow table of contents and overview to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, embeddings guide for the retrieval system, table of contents, overview.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: embeddings guide for the retrieval system is present and needs its own handling; table of contents is present and needs its own handling; overview is present and needs its own handling; compare embedding models and understand their tradeoffs is present and needs its own handling.

B: Create work pieces for embeddings guide for the retrieval system, table of contents, overview, compare embedding models and understand their tradeoffs. Keep embeddings guide for the retrieval system and table of contents separate. Allow overview and compare embedding models and understand their tradeoffs to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: embeddings guide for the retrieval system, table of contents, overview, compare embedding models and understand their tradeoffs.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Embeddings Guide for the retrieval system: table of contents is present and needs its own handling; overview is present and needs its own handling; compare embedding models and understand their tradeoffs is present and needs its own handling; apply advanced chunking strategies for optimal document retrieval is present and needs its own handling.

B: Create work pieces for table of contents, overview, compare embedding models and understand their tradeoffs, apply advanced chunking strategies for optimal document retrieval. Keep table of contents and overview separate. Allow compare embedding models and understand their tradeoffs and apply advanced chunking strategies for optimal document retrieval to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: table of contents, overview, compare embedding models and understand their tradeoffs, apply advanced chunking strategies for optimal document retrieval.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

