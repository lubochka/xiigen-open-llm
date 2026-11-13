# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; planningsessionstartup a planning skill is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, planningsessionstartup a planning skill. Keep graph relationships and evidence selection separate. Allow quality measurement and planningsessionstartup a planning skill to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, planningsessionstartup a planning skill.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; planningsessionstartup a planning skill is present and needs its own handling; purpose is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, planningsessionstartup a planning skill, purpose. Keep evidence selection and quality measurement separate. Allow planningsessionstartup a planning skill and purpose to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, planningsessionstartup a planning skill, purpose.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: quality measurement is present and needs its own handling; planningsessionstartup a planning skill is present and needs its own handling; purpose is present and needs its own handling; pattern is present and needs its own handling.

B: Create work pieces for quality measurement, planningsessionstartup a planning skill, purpose, pattern. Keep quality measurement and planningsessionstartup a planning skill separate. Allow purpose and pattern to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, planningsessionstartup a planning skill, purpose, pattern.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: planningsessionstartup a planning skill is present and needs its own handling; purpose is present and needs its own handling; pattern is present and needs its own handling; question 1 what is the current baseline is present and needs its own handling.

B: Create work pieces for planningsessionstartup a planning skill, purpose, pattern, question 1 what is the current baseline. Keep planningsessionstartup a planning skill and purpose separate. Allow pattern and question 1 what is the current baseline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: planningsessionstartup a planning skill, purpose, pattern, question 1 what is the current baseline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PlanningSessionStartup a planning skill: purpose is present and needs its own handling; pattern is present and needs its own handling; question 1 what is the current baseline is present and needs its own handling; question 2 what has already been decided is present and needs its own handling.

B: Create work pieces for purpose, pattern, question 1 what is the current baseline, question 2 what has already been decided. Keep purpose and pattern separate. Allow question 1 what is the current baseline and question 2 what has already been decided to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: purpose, pattern, question 1 what is the current baseline, question 2 what has already been decided.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

