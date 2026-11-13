# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; the 3 shared infrastructure categories is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, the 3 shared infrastructure categories. Keep graph relationships and evidence selection separate. Allow quality measurement and the 3 shared infrastructure categories to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, the 3 shared infrastructure categories.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; the 3 shared infrastructure categories is present and needs its own handling; category 1 shared base types core cs is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, the 3 shared infrastructure categories, category 1 shared base types core cs. Keep evidence selection and quality measurement separate. Allow the 3 shared infrastructure categories and category 1 shared base types core cs to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, the 3 shared infrastructure categories, category 1 shared base types core cs.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: quality measurement is present and needs its own handling; the 3 shared infrastructure categories is present and needs its own handling; category 1 shared base types core cs is present and needs its own handling; category 2 shared fabric interfaces is present and needs its own handling.

B: Create work pieces for quality measurement, the 3 shared infrastructure categories, category 1 shared base types core cs, category 2 shared fabric interfaces. Keep quality measurement and the 3 shared infrastructure categories separate. Allow category 1 shared base types core cs and category 2 shared fabric interfaces to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, the 3 shared infrastructure categories, category 1 shared base types core cs, category 2 shared fabric interfaces.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: the 3 shared infrastructure categories is present and needs its own handling; category 1 shared base types core cs is present and needs its own handling; category 2 shared fabric interfaces is present and needs its own handling; category 3 shared test infrastructure is present and needs its own handling.

B: Create work pieces for the 3 shared infrastructure categories, category 1 shared base types core cs, category 2 shared fabric interfaces, category 3 shared test infrastructure. Keep the 3 shared infrastructure categories and category 1 shared base types core cs separate. Allow category 2 shared fabric interfaces and category 3 shared test infrastructure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the 3 shared infrastructure categories, category 1 shared base types core cs, category 2 shared fabric interfaces, category 3 shared test infrastructure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Shared-infrastructure-SKILL.md v1.0 — Shared services that multiple subprojects need: category 1 shared base types core cs is present and needs its own handling; category 2 shared fabric interfaces is present and needs its own handling; category 3 shared test infrastructure is present and needs its own handling; before adding any new class to any subproject is present and needs its own handling.

B: Create work pieces for category 1 shared base types core cs, category 2 shared fabric interfaces, category 3 shared test infrastructure, before adding any new class to any subproject. Keep category 1 shared base types core cs and category 2 shared fabric interfaces separate. Allow category 3 shared test infrastructure and before adding any new class to any subproject to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: category 1 shared base types core cs, category 2 shared fabric interfaces, category 3 shared test infrastructure, before adding any new class to any subproject.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

