# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, when to invoke. Keep graph relationships and evidence selection separate. Allow quality measurement and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; when to invoke is present and needs its own handling; the 4 tier test is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, when to invoke, the 4 tier test. Keep evidence selection and quality measurement separate. Allow when to invoke and the 4 tier test to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, when to invoke, the 4 tier test.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: quality measurement is present and needs its own handling; when to invoke is present and needs its own handling; the 4 tier test is present and needs its own handling; tier 1 static constant is present and needs its own handling.

B: Create work pieces for quality measurement, when to invoke, the 4 tier test, tier 1 static constant. Keep quality measurement and when to invoke separate. Allow the 4 tier test and tier 1 static constant to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, when to invoke, the 4 tier test, tier 1 static constant.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: when to invoke is present and needs its own handling; the 4 tier test is present and needs its own handling; tier 1 static constant is present and needs its own handling; tier 2 configurable parameter is present and needs its own handling.

B: Create work pieces for when to invoke, the 4 tier test, tier 1 static constant, tier 2 configurable parameter. Keep when to invoke and the 4 tier test separate. Allow tier 1 static constant and tier 2 configurable parameter to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: when to invoke, the 4 tier test, tier 1 static constant, tier 2 configurable parameter.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Four-tier-decision-classification-SKILL.md v1.0 — Classify every decision in a design: the 4 tier test is present and needs its own handling; tier 1 static constant is present and needs its own handling; tier 2 configurable parameter is present and needs its own handling; tier 3 graph rag decision is present and needs its own handling.

B: Create work pieces for the 4 tier test, tier 1 static constant, tier 2 configurable parameter, tier 3 graph rag decision. Keep the 4 tier test and tier 1 static constant separate. Allow tier 2 configurable parameter and tier 3 graph rag decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the 4 tier test, tier 1 static constant, tier 2 configurable parameter, tier 3 graph rag decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

