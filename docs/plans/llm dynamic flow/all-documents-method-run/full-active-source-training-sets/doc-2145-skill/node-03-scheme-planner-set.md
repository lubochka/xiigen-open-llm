# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; quick start simple the retrieval system pipeline is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, quick start simple the retrieval system pipeline. Keep graph relationships and evidence selection separate. Allow quality measurement and quick start simple the retrieval system pipeline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, quick start simple the retrieval system pipeline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; quick start simple the retrieval system pipeline is present and needs its own handling; chunk documents is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, quick start simple the retrieval system pipeline, chunk documents. Keep evidence selection and quality measurement separate. Allow quick start simple the retrieval system pipeline and chunk documents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, quick start simple the retrieval system pipeline, chunk documents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: quality measurement is present and needs its own handling; quick start simple the retrieval system pipeline is present and needs its own handling; chunk documents is present and needs its own handling; create embeddings and store is present and needs its own handling.

B: Create work pieces for quality measurement, quick start simple the retrieval system pipeline, chunk documents, create embeddings and store. Keep quality measurement and quick start simple the retrieval system pipeline separate. Allow chunk documents and create embeddings and store to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, quick start simple the retrieval system pipeline, chunk documents, create embeddings and store.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: quick start simple the retrieval system pipeline is present and needs its own handling; chunk documents is present and needs its own handling; create embeddings and store is present and needs its own handling; create the retrieval system chain is present and needs its own handling.

B: Create work pieces for quick start simple the retrieval system pipeline, chunk documents, create embeddings and store, create the retrieval system chain. Keep quick start simple the retrieval system pipeline and chunk documents separate. Allow create embeddings and store and create the retrieval system chain to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: quick start simple the retrieval system pipeline, chunk documents, create embeddings and store, create the retrieval system chain.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The retrieval system & Retrieval - Semantic Search & Knowledge Augmentation: chunk documents is present and needs its own handling; create embeddings and store is present and needs its own handling; create the retrieval system chain is present and needs its own handling; query is present and needs its own handling.

B: Create work pieces for chunk documents, create embeddings and store, create the retrieval system chain, query. Keep chunk documents and create embeddings and store separate. Allow create the retrieval system chain and query to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: chunk documents, create embeddings and store, create the retrieval system chain, query.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

