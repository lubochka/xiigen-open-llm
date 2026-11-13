# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; phase 6 complete the retrieval system design patterns is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, phase 6 complete the retrieval system design patterns. Keep graph relationships and evidence selection separate. Allow quality measurement and phase 6 complete the retrieval system design patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, phase 6 complete the retrieval system design patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; phase 6 complete the retrieval system design patterns is present and needs its own handling; summary is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, phase 6 complete the retrieval system design patterns, summary. Keep evidence selection and quality measurement separate. Allow phase 6 complete the retrieval system design patterns and summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, phase 6 complete the retrieval system design patterns, summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: quality measurement is present and needs its own handling; phase 6 complete the retrieval system design patterns is present and needs its own handling; summary is present and needs its own handling; files created is present and needs its own handling.

B: Create work pieces for quality measurement, phase 6 complete the retrieval system design patterns, summary, files created. Keep quality measurement and phase 6 complete the retrieval system design patterns separate. Allow summary and files created to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, phase 6 complete the retrieval system design patterns, summary, files created.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: phase 6 complete the retrieval system design patterns is present and needs its own handling; summary is present and needs its own handling; files created is present and needs its own handling; next phase 7 platform services is present and needs its own handling.

B: Create work pieces for phase 6 complete the retrieval system design patterns, summary, files created, next phase 7 platform services. Keep phase 6 complete the retrieval system design patterns and summary separate. Allow files created and next phase 7 platform services to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: phase 6 complete the retrieval system design patterns, summary, files created, next phase 7 platform services.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Phase 6 Complete — the retrieval system + Design Patterns ✅: summary is present and needs its own handling; files created is present and needs its own handling; next phase 7 platform services is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for summary, files created, next phase 7 platform services, training example. Keep summary and files created separate. Allow next phase 7 platform services and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: summary, files created, next phase 7 platform services, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

