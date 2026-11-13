# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; xiigen missing architecture features is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, xiigen missing architecture features. Keep graph relationships and evidence selection separate. Allow quality measurement and xiigen missing architecture features to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, xiigen missing architecture features.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; xiigen missing architecture features is present and needs its own handling; organized by architectural layer not by gap id is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, xiigen missing architecture features, organized by architectural layer not by gap id. Keep evidence selection and quality measurement separate. Allow xiigen missing architecture features and organized by architectural layer not by gap id to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, xiigen missing architecture features, organized by architectural layer not by gap id.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: quality measurement is present and needs its own handling; xiigen missing architecture features is present and needs its own handling; organized by architectural layer not by gap id is present and needs its own handling; reading map is present and needs its own handling.

B: Create work pieces for quality measurement, xiigen missing architecture features, organized by architectural layer not by gap id, reading map. Keep quality measurement and xiigen missing architecture features separate. Allow organized by architectural layer not by gap id and reading map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, xiigen missing architecture features, organized by architectural layer not by gap id, reading map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: xiigen missing architecture features is present and needs its own handling; organized by architectural layer not by gap id is present and needs its own handling; reading map is present and needs its own handling; layer 0 fabric interfaces is present and needs its own handling.

B: Create work pieces for xiigen missing architecture features, organized by architectural layer not by gap id, reading map, layer 0 fabric interfaces. Keep xiigen missing architecture features and organized by architectural layer not by gap id separate. Allow reading map and layer 0 fabric interfaces to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen missing architecture features, organized by architectural layer not by gap id, reading map, layer 0 fabric interfaces.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Architecture Features: organized by architectural layer not by gap id is present and needs its own handling; reading map is present and needs its own handling; layer 0 fabric interfaces is present and needs its own handling; layer 1 handler extensions is present and needs its own handling.

B: Create work pieces for organized by architectural layer not by gap id, reading map, layer 0 fabric interfaces, layer 1 handler extensions. Keep organized by architectural layer not by gap id and reading map separate. Allow layer 0 fabric interfaces and layer 1 handler extensions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: organized by architectural layer not by gap id, reading map, layer 0 fabric interfaces, layer 1 handler extensions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

