# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; orchestration advisor bug fix plan is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, orchestration advisor bug fix plan. Keep graph relationships and evidence selection separate. Allow quality measurement and orchestration advisor bug fix plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, orchestration advisor bug fix plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; orchestration advisor bug fix plan is present and needs its own handling; three bugs confirmed from screenshots is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, orchestration advisor bug fix plan, three bugs confirmed from screenshots. Keep evidence selection and quality measurement separate. Allow orchestration advisor bug fix plan and three bugs confirmed from screenshots to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, orchestration advisor bug fix plan, three bugs confirmed from screenshots.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: quality measurement is present and needs its own handling; orchestration advisor bug fix plan is present and needs its own handling; three bugs confirmed from screenshots is present and needs its own handling; bug 1 all three columns show the same model critical is present and needs its own handling.

B: Create work pieces for quality measurement, orchestration advisor bug fix plan, three bugs confirmed from screenshots, bug 1 all three columns show the same model critical. Keep quality measurement and orchestration advisor bug fix plan separate. Allow three bugs confirmed from screenshots and bug 1 all three columns show the same model critical to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, orchestration advisor bug fix plan, three bugs confirmed from screenshots, bug 1 all three columns show the same model critical.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: orchestration advisor bug fix plan is present and needs its own handling; three bugs confirmed from screenshots is present and needs its own handling; bug 1 all three columns show the same model critical is present and needs its own handling; bug 2 finding descriptions repeat the title high priority is present and needs its own handling.

B: Create work pieces for orchestration advisor bug fix plan, three bugs confirmed from screenshots, bug 1 all three columns show the same model critical, bug 2 finding descriptions repeat the title high priority. Keep orchestration advisor bug fix plan and three bugs confirmed from screenshots separate. Allow bug 1 all three columns show the same model critical and bug 2 finding descriptions repeat the title high priority to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: orchestration advisor bug fix plan, three bugs confirmed from screenshots, bug 1 all three columns show the same model critical, bug 2 finding descriptions repeat the title high priority.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Bug Fix Plan: three bugs confirmed from screenshots is present and needs its own handling; bug 1 all three columns show the same model critical is present and needs its own handling; bug 2 finding descriptions repeat the title high priority is present and needs its own handling; bug 3 some retrievals return the wrong episode medium priority is present and needs its own handling.

B: Create work pieces for three bugs confirmed from screenshots, bug 1 all three columns show the same model critical, bug 2 finding descriptions repeat the title high priority, bug 3 some retrievals return the wrong episode medium priority. Keep three bugs confirmed from screenshots and bug 1 all three columns show the same model critical separate. Allow bug 2 finding descriptions repeat the title high priority and bug 3 some retrievals return the wrong episode medium priority to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: three bugs confirmed from screenshots, bug 1 all three columns show the same model critical, bug 2 finding descriptions repeat the title high priority, bug 3 some retrievals return the wrong episode medium priority.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

