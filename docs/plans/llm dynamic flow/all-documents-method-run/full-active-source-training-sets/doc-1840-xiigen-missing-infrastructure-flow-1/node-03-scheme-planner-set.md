# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; xiigen missing infrastructure inventory is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, xiigen missing infrastructure inventory. Keep graph relationships and evidence selection separate. Allow quality measurement and xiigen missing infrastructure inventory to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, xiigen missing infrastructure inventory.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; xiigen missing infrastructure inventory is present and needs its own handling; date 2026 03 23 is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, xiigen missing infrastructure inventory, date 2026 03 23. Keep evidence selection and quality measurement separate. Allow xiigen missing infrastructure inventory and date 2026 03 23 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, xiigen missing infrastructure inventory, date 2026 03 23.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: quality measurement is present and needs its own handling; xiigen missing infrastructure inventory is present and needs its own handling; date 2026 03 23 is present and needs its own handling; kernel packages kernel is present and needs its own handling.

B: Create work pieces for quality measurement, xiigen missing infrastructure inventory, date 2026 03 23, kernel packages kernel. Keep quality measurement and xiigen missing infrastructure inventory separate. Allow date 2026 03 23 and kernel packages kernel to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, xiigen missing infrastructure inventory, date 2026 03 23, kernel packages kernel.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: xiigen missing infrastructure inventory is present and needs its own handling; date 2026 03 23 is present and needs its own handling; kernel packages kernel is present and needs its own handling; tenantkeygenerator is present and needs its own handling.

B: Create work pieces for xiigen missing infrastructure inventory, date 2026 03 23, kernel packages kernel, tenantkeygenerator. Keep xiigen missing infrastructure inventory and date 2026 03 23 separate. Allow kernel packages kernel and tenantkeygenerator to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen missing infrastructure inventory, date 2026 03 23, kernel packages kernel, tenantkeygenerator.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — Missing Infrastructure Inventory: date 2026 03 23 is present and needs its own handling; kernel packages kernel is present and needs its own handling; tenantkeygenerator is present and needs its own handling; iidempotencystore is present and needs its own handling.

B: Create work pieces for date 2026 03 23, kernel packages kernel, tenantkeygenerator, iidempotencystore. Keep date 2026 03 23 and kernel packages kernel separate. Allow tenantkeygenerator and iidempotencystore to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: date 2026 03 23, kernel packages kernel, tenantkeygenerator, iidempotencystore.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

