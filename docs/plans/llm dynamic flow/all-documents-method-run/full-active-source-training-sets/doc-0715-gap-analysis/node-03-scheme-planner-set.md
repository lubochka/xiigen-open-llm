# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; fix plan vs decision matrix gap analysis is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, fix plan vs decision matrix gap analysis. Keep graph relationships and evidence selection separate. Allow quality measurement and fix plan vs decision matrix gap analysis to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, fix plan vs decision matrix gap analysis.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; fix plan vs decision matrix gap analysis is present and needs its own handling; tl dr is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, fix plan vs decision matrix gap analysis, tl dr. Keep evidence selection and quality measurement separate. Allow fix plan vs decision matrix gap analysis and tl dr to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, fix plan vs decision matrix gap analysis, tl dr.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: quality measurement is present and needs its own handling; fix plan vs decision matrix gap analysis is present and needs its own handling; tl dr is present and needs its own handling; coverage map is present and needs its own handling.

B: Create work pieces for quality measurement, fix plan vs decision matrix gap analysis, tl dr, coverage map. Keep quality measurement and fix plan vs decision matrix gap analysis separate. Allow tl dr and coverage map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, fix plan vs decision matrix gap analysis, tl dr, coverage map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: fix plan vs decision matrix gap analysis is present and needs its own handling; tl dr is present and needs its own handling; coverage map is present and needs its own handling; why group1 group2 are out of scope is present and needs its own handling.

B: Create work pieces for fix plan vs decision matrix gap analysis, tl dr, coverage map, why group1 group2 are out of scope. Keep fix plan vs decision matrix gap analysis and tl dr separate. Allow coverage map and why group1 group2 are out of scope to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: fix plan vs decision matrix gap analysis, tl dr, coverage map, why group1 group2 are out of scope.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Fix Plan vs Decision Matrix — Gap Analysis: tl dr is present and needs its own handling; coverage map is present and needs its own handling; why group1 group2 are out of scope is present and needs its own handling; line number discrepancy is present and needs its own handling.

B: Create work pieces for tl dr, coverage map, why group1 group2 are out of scope, line number discrepancy. Keep tl dr and coverage map separate. Allow why group1 group2 are out of scope and line number discrepancy to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: tl dr, coverage map, why group1 group2 are out of scope, line number discrepancy.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

