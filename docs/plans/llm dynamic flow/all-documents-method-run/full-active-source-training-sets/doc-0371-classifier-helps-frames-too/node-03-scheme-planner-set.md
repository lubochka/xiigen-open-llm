# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; evidence from debug traces is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, evidence from debug traces. Keep graph relationships and evidence selection separate. Allow quality measurement and evidence from debug traces to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, evidence from debug traces.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; evidence from debug traces is present and needs its own handling; cart frame based is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, evidence from debug traces, cart frame based. Keep evidence selection and quality measurement separate. Allow evidence from debug traces and cart frame based to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, evidence from debug traces, cart frame based.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: quality measurement is present and needs its own handling; evidence from debug traces is present and needs its own handling; cart frame based is present and needs its own handling; newsfeed frame based is present and needs its own handling.

B: Create work pieces for quality measurement, evidence from debug traces, cart frame based, newsfeed frame based. Keep quality measurement and evidence from debug traces separate. Allow cart frame based and newsfeed frame based to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, evidence from debug traces, cart frame based, newsfeed frame based.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: evidence from debug traces is present and needs its own handling; cart frame based is present and needs its own handling; newsfeed frame based is present and needs its own handling; out of 41 nodes have frame instance parents with layoutmode none is present and needs its own handling.

B: Create work pieces for evidence from debug traces, cart frame based, newsfeed frame based, out of 41 nodes have frame instance parents with layoutmode none. Keep evidence from debug traces and cart frame based separate. Allow newsfeed frame based and out of 41 nodes have frame instance parents with layoutmode none to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: evidence from debug traces, cart frame based, newsfeed frame based, out of 41 nodes have frame instance parents with layoutmode none.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The retrieval system: Sibling Classifier Applies to FRAME Scenes Too: cart frame based is present and needs its own handling; newsfeed frame based is present and needs its own handling; out of 41 nodes have frame instance parents with layoutmode none is present and needs its own handling; nodes have position conflicts relative absolute is present and needs its own handling.

B: Create work pieces for cart frame based, newsfeed frame based, out of 41 nodes have frame instance parents with layoutmode none, nodes have position conflicts relative absolute. Keep cart frame based and newsfeed frame based separate. Allow out of 41 nodes have frame instance parents with layoutmode none and nodes have position conflicts relative absolute to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: cart frame based, newsfeed frame based, out of 41 nodes have frame instance parents with layoutmode none, nodes have position conflicts relative absolute.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

