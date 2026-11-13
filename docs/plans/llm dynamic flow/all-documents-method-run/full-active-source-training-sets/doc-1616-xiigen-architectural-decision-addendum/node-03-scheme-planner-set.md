# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; xiigen architectural decision addendum is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, xiigen architectural decision addendum. Keep graph relationships and evidence selection separate. Allow quality measurement and xiigen architectural decision addendum to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, xiigen architectural decision addendum.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; xiigen architectural decision addendum is present and needs its own handling; appended to xiigen session load plan v3 md is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, xiigen architectural decision addendum, appended to xiigen session load plan v3 md. Keep evidence selection and quality measurement separate. Allow xiigen architectural decision addendum and appended to xiigen session load plan v3 md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, xiigen architectural decision addendum, appended to xiigen session load plan v3 md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: quality measurement is present and needs its own handling; xiigen architectural decision addendum is present and needs its own handling; appended to xiigen session load plan v3 md is present and needs its own handling; date 2026 04 01 is present and needs its own handling.

B: Create work pieces for quality measurement, xiigen architectural decision addendum, appended to xiigen session load plan v3 md, date 2026 04 01. Keep quality measurement and xiigen architectural decision addendum separate. Allow appended to xiigen session load plan v3 md and date 2026 04 01 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, xiigen architectural decision addendum, appended to xiigen session load plan v3 md, date 2026 04 01.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: xiigen architectural decision addendum is present and needs its own handling; appended to xiigen session load plan v3 md is present and needs its own handling; date 2026 04 01 is present and needs its own handling; the governing question for every decision is present and needs its own handling.

B: Create work pieces for xiigen architectural decision addendum, appended to xiigen session load plan v3 md, date 2026 04 01, the governing question for every decision. Keep xiigen architectural decision addendum and appended to xiigen session load plan v3 md separate. Allow date 2026 04 01 and the governing question for every decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen architectural decision addendum, appended to xiigen session load plan v3 md, date 2026 04 01, the governing question for every decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGEN — ARCHITECTURAL DECISION ADDENDUM: appended to xiigen session load plan v3 md is present and needs its own handling; date 2026 04 01 is present and needs its own handling; the governing question for every decision is present and needs its own handling; the two failure modes and how to detect each is present and needs its own handling.

B: Create work pieces for appended to xiigen session load plan v3 md, date 2026 04 01, the governing question for every decision, the two failure modes and how to detect each. Keep appended to xiigen session load plan v3 md and date 2026 04 01 separate. Allow the governing question for every decision and the two failure modes and how to detect each to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: appended to xiigen session load plan v3 md, date 2026 04 01, the governing question for every decision, the two failure modes and how to detect each.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

