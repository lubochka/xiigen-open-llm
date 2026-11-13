# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; orchestration advisor code fix plan is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, orchestration advisor code fix plan. Keep graph relationships and evidence selection separate. Allow quality measurement and orchestration advisor code fix plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, orchestration advisor code fix plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; orchestration advisor code fix plan is present and needs its own handling; seven issues found in the source code is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, orchestration advisor code fix plan, seven issues found in the source code. Keep evidence selection and quality measurement separate. Allow orchestration advisor code fix plan and seven issues found in the source code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, orchestration advisor code fix plan, seven issues found in the source code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: quality measurement is present and needs its own handling; orchestration advisor code fix plan is present and needs its own handling; seven issues found in the source code is present and needs its own handling; phase 1 fix the two critical correctness issues is present and needs its own handling.

B: Create work pieces for quality measurement, orchestration advisor code fix plan, seven issues found in the source code, phase 1 fix the two critical correctness issues. Keep quality measurement and orchestration advisor code fix plan separate. Allow seven issues found in the source code and phase 1 fix the two critical correctness issues to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, orchestration advisor code fix plan, seven issues found in the source code, phase 1 fix the two critical correctness issues.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: orchestration advisor code fix plan is present and needs its own handling; seven issues found in the source code is present and needs its own handling; phase 1 fix the two critical correctness issues is present and needs its own handling; phase 1a give the scope reasoning node the user s question is present and needs its own handling.

B: Create work pieces for orchestration advisor code fix plan, seven issues found in the source code, phase 1 fix the two critical correctness issues, phase 1a give the scope reasoning node the user s question. Keep orchestration advisor code fix plan and seven issues found in the source code separate. Allow phase 1 fix the two critical correctness issues and phase 1a give the scope reasoning node the user s question to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: orchestration advisor code fix plan, seven issues found in the source code, phase 1 fix the two critical correctness issues, phase 1a give the scope reasoning node the user s question.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Orchestration Advisor — Code Fix Plan: seven issues found in the source code is present and needs its own handling; phase 1 fix the two critical correctness issues is present and needs its own handling; phase 1a give the scope reasoning node the user s question is present and needs its own handling; phase 2 fix the two latency issues is present and needs its own handling.

B: Create work pieces for seven issues found in the source code, phase 1 fix the two critical correctness issues, phase 1a give the scope reasoning node the user s question, phase 2 fix the two latency issues. Keep seven issues found in the source code and phase 1 fix the two critical correctness issues separate. Allow phase 1a give the scope reasoning node the user s question and phase 2 fix the two latency issues to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: seven issues found in the source code, phase 1 fix the two critical correctness issues, phase 1a give the scope reasoning node the user s question, phase 2 fix the two latency issues.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

