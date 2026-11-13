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

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; version v1 0 is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, version v1 0. Keep graph relationships and evidence selection separate. Allow quality measurement and version v1 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, version v1 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; version v1 0 is present and needs its own handling; status awaiting approval nothing executed until approved is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, version v1 0, status awaiting approval nothing executed until approved. Keep evidence selection and quality measurement separate. Allow version v1 0 and status awaiting approval nothing executed until approved to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, version v1 0, status awaiting approval nothing executed until approved.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: quality measurement is present and needs its own handling; version v1 0 is present and needs its own handling; status awaiting approval nothing executed until approved is present and needs its own handling; reference read and approve before claude code touches anything is present and needs its own handling.

B: Create work pieces for quality measurement, version v1 0, status awaiting approval nothing executed until approved, reference read and approve before claude code touches anything. Keep quality measurement and version v1 0 separate. Allow status awaiting approval nothing executed until approved and reference read and approve before claude code touches anything to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, version v1 0, status awaiting approval nothing executed until approved, reference read and approve before claude code touches anything.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: version v1 0 is present and needs its own handling; status awaiting approval nothing executed until approved is present and needs its own handling; reference read and approve before claude code touches anything is present and needs its own handling; what we know from the evidence is present and needs its own handling.

B: Create work pieces for version v1 0, status awaiting approval nothing executed until approved, reference read and approve before claude code touches anything, what we know from the evidence. Keep version v1 0 and status awaiting approval nothing executed until approved separate. Allow reference read and approve before claude code touches anything and what we know from the evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: version v1 0, status awaiting approval nothing executed until approved, reference read and approve before claude code touches anything, what we know from the evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — the retrieval system Grounding + Timeout Fix Plan: status awaiting approval nothing executed until approved is present and needs its own handling; reference read and approve before claude code touches anything is present and needs its own handling; what we know from the evidence is present and needs its own handling; what i need from you at each stop is present and needs its own handling.

B: Create work pieces for status awaiting approval nothing executed until approved, reference read and approve before claude code touches anything, what we know from the evidence, what i need from you at each stop. Keep status awaiting approval nothing executed until approved and reference read and approve before claude code touches anything separate. Allow what we know from the evidence and what i need from you at each stop to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: status awaiting approval nothing executed until approved, reference read and approve before claude code touches anything, what we know from the evidence, what i need from you at each stop.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

