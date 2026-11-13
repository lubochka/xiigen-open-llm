# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; date 2026 03 22 is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, date 2026 03 22. Keep graph relationships and evidence selection separate. Allow quality measurement and date 2026 03 22 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, date 2026 03 22.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; date 2026 03 22 is present and needs its own handling; flow completeness checker v1 3 31 31 is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, date 2026 03 22, flow completeness checker v1 3 31 31. Keep evidence selection and quality measurement separate. Allow date 2026 03 22 and flow completeness checker v1 3 31 31 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, date 2026 03 22, flow completeness checker v1 3 31 31.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: quality measurement is present and needs its own handling; date 2026 03 22 is present and needs its own handling; flow completeness checker v1 3 31 31 is present and needs its own handling; scope discipline is present and needs its own handling.

B: Create work pieces for quality measurement, date 2026 03 22, flow completeness checker v1 3 31 31, scope discipline. Keep quality measurement and date 2026 03 22 separate. Allow flow completeness checker v1 3 31 31 and scope discipline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, date 2026 03 22, flow completeness checker v1 3 31 31, scope discipline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: date 2026 03 22 is present and needs its own handling; flow completeness checker v1 3 31 31 is present and needs its own handling; scope discipline is present and needs its own handling; what changed from v3 v4 is present and needs its own handling.

B: Create work pieces for date 2026 03 22, flow completeness checker v1 3 31 31, scope discipline, what changed from v3 v4. Keep date 2026 03 22 and flow completeness checker v1 3 31 31 separate. Allow scope discipline and what changed from v3 v4 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: date 2026 03 22, flow completeness checker v1 3 31 31, scope discipline, what changed from v3 v4.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The event creation flow: EVENT CREATION & PROMOTION — REFERENCE PLAN v4: flow completeness checker v1 3 31 31 is present and needs its own handling; scope discipline is present and needs its own handling; what changed from v3 v4 is present and needs its own handling; wave assignment unchanged is present and needs its own handling.

B: Create work pieces for flow completeness checker v1 3 31 31, scope discipline, what changed from v3 v4, wave assignment unchanged. Keep flow completeness checker v1 3 31 31 and scope discipline separate. Allow what changed from v3 v4 and wave assignment unchanged to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: flow completeness checker v1 3 31 31, scope discipline, what changed from v3 v4, wave assignment unchanged.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

