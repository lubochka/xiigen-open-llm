# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; what we know from the evidence is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, what we know from the evidence. Keep graph relationships and evidence selection separate. Allow quality measurement and what we know from the evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, what we know from the evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; what we know from the evidence is present and needs its own handling; what i need from you at each stop here is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, what we know from the evidence, what i need from you at each stop here. Keep evidence selection and quality measurement separate. Allow what we know from the evidence and what i need from you at each stop here to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, what we know from the evidence, what i need from you at each stop here.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: quality measurement is present and needs its own handling; what we know from the evidence is present and needs its own handling; what i need from you at each stop here is present and needs its own handling; what stays out of scope is present and needs its own handling.

B: Create work pieces for quality measurement, what we know from the evidence, what i need from you at each stop here, what stays out of scope. Keep quality measurement and what we know from the evidence separate. Allow what i need from you at each stop here and what stays out of scope to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, what we know from the evidence, what i need from you at each stop here, what stays out of scope.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: what we know from the evidence is present and needs its own handling; what i need from you at each stop here is present and needs its own handling; what stays out of scope is present and needs its own handling; if phase 1 shows grounding is broken is present and needs its own handling.

B: Create work pieces for what we know from the evidence, what i need from you at each stop here, what stays out of scope, if phase 1 shows grounding is broken. Keep what we know from the evidence and what i need from you at each stop here separate. Allow what stays out of scope and if phase 1 shows grounding is broken to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: what we know from the evidence, what i need from you at each stop here, what stays out of scope, if phase 1 shows grounding is broken.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: what i need from you at each stop here is present and needs its own handling; what stays out of scope is present and needs its own handling; if phase 1 shows grounding is broken is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for what i need from you at each stop here, what stays out of scope, if phase 1 shows grounding is broken, training example. Keep what i need from you at each stop here and what stays out of scope separate. Allow if phase 1 shows grounding is broken and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what i need from you at each stop here, what stays out of scope, if phase 1 shows grounding is broken, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

