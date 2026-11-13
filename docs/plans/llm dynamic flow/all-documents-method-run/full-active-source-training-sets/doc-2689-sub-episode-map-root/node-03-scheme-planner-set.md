# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; sub episode map is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, sub episode map. Keep graph relationships and evidence selection separate. Allow quality measurement and sub episode map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, sub episode map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; sub episode map is present and needs its own handling; each sub phase treated as its own episode connected to its parent is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, sub episode map, each sub phase treated as its own episode connected to its parent. Keep evidence selection and quality measurement separate. Allow sub episode map and each sub phase treated as its own episode connected to its parent to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, sub episode map, each sub phase treated as its own episode connected to its parent.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: quality measurement is present and needs its own handling; sub episode map is present and needs its own handling; each sub phase treated as its own episode connected to its parent is present and needs its own handling; plain human language throughout same format as parent episodes is present and needs its own handling.

B: Create work pieces for quality measurement, sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes. Keep quality measurement and sub episode map separate. Allow each sub phase treated as its own episode connected to its parent and plain human language throughout same format as parent episodes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: sub episode map is present and needs its own handling; each sub phase treated as its own episode connected to its parent is present and needs its own handling; plain human language throughout same format as parent episodes is present and needs its own handling; document references is present and needs its own handling.

B: Create work pieces for sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, document references. Keep sub episode map and each sub phase treated as its own episode connected to its parent separate. Allow plain human language throughout same format as parent episodes and document references to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: sub episode map, each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, document references.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Sub-Episode Map: each sub phase treated as its own episode connected to its parent is present and needs its own handling; plain human language throughout same format as parent episodes is present and needs its own handling; document references is present and needs its own handling; how to read this is present and needs its own handling.

B: Create work pieces for each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, document references, how to read this. Keep each sub phase treated as its own episode connected to its parent and plain human language throughout same format as parent episodes separate. Allow document references and how to read this to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: each sub phase treated as its own episode connected to its parent, plain human language throughout same format as parent episodes, document references, how to read this.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

