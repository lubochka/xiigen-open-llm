# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; flow design check catalog v2 0 s4 additions is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, flow design check catalog v2 0 s4 additions. Keep graph relationships and evidence selection separate. Allow quality measurement and flow design check catalog v2 0 s4 additions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, flow design check catalog v2 0 s4 additions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; flow design check catalog v2 0 s4 additions is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, flow design check catalog v2 0 s4 additions, when to invoke. Keep evidence selection and quality measurement separate. Allow flow design check catalog v2 0 s4 additions and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, flow design check catalog v2 0 s4 additions, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: quality measurement is present and needs its own handling; flow design check catalog v2 0 s4 additions is present and needs its own handling; when to invoke is present and needs its own handling; check id format is present and needs its own handling.

B: Create work pieces for quality measurement, flow design check catalog v2 0 s4 additions, when to invoke, check id format. Keep quality measurement and flow design check catalog v2 0 s4 additions separate. Allow when to invoke and check id format to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, flow design check catalog v2 0 s4 additions, when to invoke, check id format.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: flow design check catalog v2 0 s4 additions is present and needs its own handling; when to invoke is present and needs its own handling; check id format is present and needs its own handling; capability checks cap xxx is present and needs its own handling.

B: Create work pieces for flow design check catalog v2 0 s4 additions, when to invoke, check id format, capability checks cap xxx. Keep flow design check catalog v2 0 s4 additions and when to invoke separate. Allow check id format and capability checks cap xxx to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: flow design check catalog v2 0 s4 additions, when to invoke, check id format, capability checks cap xxx.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Flow Design Check Catalog v2.0 S4 additions: when to invoke is present and needs its own handling; check id format is present and needs its own handling; capability checks cap xxx is present and needs its own handling; cap 001 file exists is present and needs its own handling.

B: Create work pieces for when to invoke, check id format, capability checks cap xxx, cap 001 file exists. Keep when to invoke and check id format separate. Allow capability checks cap xxx and cap 001 file exists to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to invoke, check id format, capability checks cap xxx, cap 001 file exists.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

