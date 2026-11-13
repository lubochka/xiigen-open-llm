# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; the self build lifecycle is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, the self build lifecycle. Keep graph relationships and evidence selection separate. Allow quality measurement and the self build lifecycle to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, the self build lifecycle.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; the self build lifecycle is present and needs its own handling; registry update it updates the central node registry manifest is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, the self build lifecycle, registry update it updates the central node registry manifest. Keep evidence selection and quality measurement separate. Allow the self build lifecycle and registry update it updates the central node registry manifest to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, the self build lifecycle, registry update it updates the central node registry manifest.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: quality measurement is present and needs its own handling; the self build lifecycle is present and needs its own handling; registry update it updates the central node registry manifest is present and needs its own handling; required architectural pillars is present and needs its own handling.

B: Create work pieces for quality measurement, the self build lifecycle, registry update it updates the central node registry manifest, required architectural pillars. Keep quality measurement and the self build lifecycle separate. Allow registry update it updates the central node registry manifest and required architectural pillars to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, the self build lifecycle, registry update it updates the central node registry manifest, required architectural pillars.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: the self build lifecycle is present and needs its own handling; registry update it updates the central node registry manifest is present and needs its own handling; required architectural pillars is present and needs its own handling; fce self extension gap new factory contracts is present and needs its own handling.

B: Create work pieces for the self build lifecycle, registry update it updates the central node registry manifest, required architectural pillars, fce self extension gap new factory contracts. Keep the self build lifecycle and registry update it updates the central node registry manifest separate. Allow required architectural pillars and fce self extension gap new factory contracts to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the self build lifecycle, registry update it updates the central node registry manifest, required architectural pillars, fce self extension gap new factory contracts.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The Self-Build Lifecycle: registry update it updates the central node registry manifest is present and needs its own handling; required architectural pillars is present and needs its own handling; fce self extension gap new factory contracts is present and needs its own handling; af station loop generate review judge iterate is present and needs its own handling.

B: Create work pieces for registry update it updates the central node registry manifest, required architectural pillars, fce self extension gap new factory contracts, af station loop generate review judge iterate. Keep registry update it updates the central node registry manifest and required architectural pillars separate. Allow fce self extension gap new factory contracts and af station loop generate review judge iterate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: registry update it updates the central node registry manifest, required architectural pillars, fce self extension gap new factory contracts, af station loop generate review judge iterate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

