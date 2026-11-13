# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; kernel handwritten small stable is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, kernel handwritten small stable. Keep graph relationships and evidence selection separate. Allow quality measurement and kernel handwritten small stable to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, kernel handwritten small stable.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; kernel handwritten small stable is present and needs its own handling; core fabric dna primitives is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, kernel handwritten small stable, core fabric dna primitives. Keep evidence selection and quality measurement separate. Allow kernel handwritten small stable and core fabric dna primitives to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, kernel handwritten small stable, core fabric dna primitives.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: quality measurement is present and needs its own handling; kernel handwritten small stable is present and needs its own handling; core fabric dna primitives is present and needs its own handling; layer 0 fabrics thin adapters swappable is present and needs its own handling.

B: Create work pieces for quality measurement, kernel handwritten small stable, core fabric dna primitives, layer 0 fabrics thin adapters swappable. Keep quality measurement and kernel handwritten small stable separate. Allow core fabric dna primitives and layer 0 fabrics thin adapters swappable to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, kernel handwritten small stable, core fabric dna primitives, layer 0 fabrics thin adapters swappable.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: kernel handwritten small stable is present and needs its own handling; core fabric dna primitives is present and needs its own handling; layer 0 fabrics thin adapters swappable is present and needs its own handling; flow engine runtime is present and needs its own handling.

B: Create work pieces for kernel handwritten small stable, core fabric dna primitives, layer 0 fabrics thin adapters swappable, flow engine runtime. Keep kernel handwritten small stable and core fabric dna primitives separate. Allow layer 0 fabrics thin adapters swappable and flow engine runtime to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: kernel handwritten small stable, core fabric dna primitives, layer 0 fabrics thin adapters swappable, flow engine runtime.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The core idea: build a tiny “kernel”, then let the engine generate everything else: core fabric dna primitives is present and needs its own handling; layer 0 fabrics thin adapters swappable is present and needs its own handling; flow engine runtime is present and needs its own handling; bootstrap flow the first self build orchestration no code view is present and needs its own handling.

B: Create work pieces for core fabric dna primitives, layer 0 fabrics thin adapters swappable, flow engine runtime, bootstrap flow the first self build orchestration no code view. Keep core fabric dna primitives and layer 0 fabrics thin adapters swappable separate. Allow flow engine runtime and bootstrap flow the first self build orchestration no code view to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: core fabric dna primitives, layer 0 fabrics thin adapters swappable, flow engine runtime, bootstrap flow the first self build orchestration no code view.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

