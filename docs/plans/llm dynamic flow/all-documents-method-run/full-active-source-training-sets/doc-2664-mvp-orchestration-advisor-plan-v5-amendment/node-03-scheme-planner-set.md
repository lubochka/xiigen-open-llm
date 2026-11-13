# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; mvp orchestration advisor plan amendment v5 is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, mvp orchestration advisor plan amendment v5. Keep graph relationships and evidence selection separate. Allow quality measurement and mvp orchestration advisor plan amendment v5 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, mvp orchestration advisor plan amendment v5.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; mvp orchestration advisor plan amendment v5 is present and needs its own handling; amends mvp orchestration advisor plan v4 md approved is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, mvp orchestration advisor plan amendment v5, amends mvp orchestration advisor plan v4 md approved. Keep evidence selection and quality measurement separate. Allow mvp orchestration advisor plan amendment v5 and amends mvp orchestration advisor plan v4 md approved to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, mvp orchestration advisor plan amendment v5, amends mvp orchestration advisor plan v4 md approved.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: quality measurement is present and needs its own handling; mvp orchestration advisor plan amendment v5 is present and needs its own handling; amends mvp orchestration advisor plan v4 md approved is present and needs its own handling; and modifies existing phases nothing in v4 is removed is present and needs its own handling.

B: Create work pieces for quality measurement, mvp orchestration advisor plan amendment v5, amends mvp orchestration advisor plan v4 md approved, and modifies existing phases nothing in v4 is removed. Keep quality measurement and mvp orchestration advisor plan amendment v5 separate. Allow amends mvp orchestration advisor plan v4 md approved and and modifies existing phases nothing in v4 is removed to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, mvp orchestration advisor plan amendment v5, amends mvp orchestration advisor plan v4 md approved, and modifies existing phases nothing in v4 is removed.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: mvp orchestration advisor plan amendment v5 is present and needs its own handling; amends mvp orchestration advisor plan v4 md approved is present and needs its own handling; and modifies existing phases nothing in v4 is removed is present and needs its own handling; what v4 does not cover is present and needs its own handling.

B: Create work pieces for mvp orchestration advisor plan amendment v5, amends mvp orchestration advisor plan v4 md approved, and modifies existing phases nothing in v4 is removed, what v4 does not cover. Keep mvp orchestration advisor plan amendment v5 and amends mvp orchestration advisor plan v4 md approved separate. Allow and modifies existing phases nothing in v4 is removed and what v4 does not cover to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: mvp orchestration advisor plan amendment v5, amends mvp orchestration advisor plan v4 md approved, and modifies existing phases nothing in v4 is removed, what v4 does not cover.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for MVP Orchestration Advisor — Plan Amendment v5: amends mvp orchestration advisor plan v4 md approved is present and needs its own handling; and modifies existing phases nothing in v4 is removed is present and needs its own handling; what v4 does not cover is present and needs its own handling; amendment a new corpus type episode maps is present and needs its own handling.

B: Create work pieces for amends mvp orchestration advisor plan v4 md approved, and modifies existing phases nothing in v4 is removed, what v4 does not cover, amendment a new corpus type episode maps. Keep amends mvp orchestration advisor plan v4 md approved and and modifies existing phases nothing in v4 is removed separate. Allow what v4 does not cover and amendment a new corpus type episode maps to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: amends mvp orchestration advisor plan v4 md approved, and modifies existing phases nothing in v4 is removed, what v4 does not cover, amendment a new corpus type episode maps.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

