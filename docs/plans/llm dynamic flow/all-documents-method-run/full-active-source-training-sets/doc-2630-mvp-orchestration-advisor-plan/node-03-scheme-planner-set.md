# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; orchestration advisor implementation plan is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, orchestration advisor implementation plan. Keep graph relationships and evidence selection separate. Allow quality measurement and orchestration advisor implementation plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, orchestration advisor implementation plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; orchestration advisor implementation plan is present and needs its own handling; what we are building is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, orchestration advisor implementation plan, what we are building. Keep evidence selection and quality measurement separate. Allow orchestration advisor implementation plan and what we are building to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, orchestration advisor implementation plan, what we are building.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: quality measurement is present and needs its own handling; orchestration advisor implementation plan is present and needs its own handling; what we are building is present and needs its own handling; before you start read the counter file is present and needs its own handling.

B: Create work pieces for quality measurement, orchestration advisor implementation plan, what we are building, before you start read the counter file. Keep quality measurement and orchestration advisor implementation plan separate. Allow what we are building and before you start read the counter file to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, orchestration advisor implementation plan, what we are building, before you start read the counter file.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: orchestration advisor implementation plan is present and needs its own handling; what we are building is present and needs its own handling; before you start read the counter file is present and needs its own handling; three documents to produce before writing any code is present and needs its own handling.

B: Create work pieces for orchestration advisor implementation plan, what we are building, before you start read the counter file, three documents to produce before writing any code. Keep orchestration advisor implementation plan and what we are building separate. Allow before you start read the counter file and three documents to produce before writing any code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: orchestration advisor implementation plan, what we are building, before you start read the counter file, three documents to produce before writing any code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Implementation Plan: what we are building is present and needs its own handling; before you start read the counter file is present and needs its own handling; three documents to produce before writing any code is present and needs its own handling; infrastructure requirements is present and needs its own handling.

B: Create work pieces for what we are building, before you start read the counter file, three documents to produce before writing any code, infrastructure requirements. Keep what we are building and before you start read the counter file separate. Allow three documents to produce before writing any code and infrastructure requirements to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what we are building, before you start read the counter file, three documents to produce before writing any code, infrastructure requirements.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

