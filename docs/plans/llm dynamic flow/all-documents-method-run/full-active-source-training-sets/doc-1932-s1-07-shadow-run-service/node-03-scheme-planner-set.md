# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; session s1 07 ishadowrunservice deferredshadowrunprovider b 1 is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, session s1 07 ishadowrunservice deferredshadowrunprovider b 1. Keep graph relationships and evidence selection separate. Allow quality measurement and session s1 07 ishadowrunservice deferredshadowrunprovider b 1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, session s1 07 ishadowrunservice deferredshadowrunprovider b 1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; session s1 07 ishadowrunservice deferredshadowrunprovider b 1 is present and needs its own handling; type generation is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, session s1 07 ishadowrunservice deferredshadowrunprovider b 1, type generation. Keep evidence selection and quality measurement separate. Allow session s1 07 ishadowrunservice deferredshadowrunprovider b 1 and type generation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, session s1 07 ishadowrunservice deferredshadowrunprovider b 1, type generation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: quality measurement is present and needs its own handling; session s1 07 ishadowrunservice deferredshadowrunprovider b 1 is present and needs its own handling; type generation is present and needs its own handling; to tracked pending state with stalledafterflows alert is present and needs its own handling.

B: Create work pieces for quality measurement, session s1 07 ishadowrunservice deferredshadowrunprovider b 1, type generation, to tracked pending state with stalledafterflows alert. Keep quality measurement and session s1 07 ishadowrunservice deferredshadowrunprovider b 1 separate. Allow type generation and to tracked pending state with stalledafterflows alert to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, session s1 07 ishadowrunservice deferredshadowrunprovider b 1, type generation, to tracked pending state with stalledafterflows alert.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: session s1 07 ishadowrunservice deferredshadowrunprovider b 1 is present and needs its own handling; type generation is present and needs its own handling; to tracked pending state with stalledafterflows alert is present and needs its own handling; duration 1 5 hours is present and needs its own handling.

B: Create work pieces for session s1 07 ishadowrunservice deferredshadowrunprovider b 1, type generation, to tracked pending state with stalledafterflows alert, duration 1 5 hours. Keep session s1 07 ishadowrunservice deferredshadowrunprovider b 1 and type generation separate. Allow to tracked pending state with stalledafterflows alert and duration 1 5 hours to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: session s1 07 ishadowrunservice deferredshadowrunprovider b 1, type generation, to tracked pending state with stalledafterflows alert, duration 1 5 hours.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for SESSION S1-07 — IShadowRunService + DeferredShadowRunProvider B-1: type generation is present and needs its own handling; to tracked pending state with stalledafterflows alert is present and needs its own handling; duration 1 5 hours is present and needs its own handling; depends on nothing can run in parallel with s1 03 through s1 06 is present and needs its own handling.

B: Create work pieces for type generation, to tracked pending state with stalledafterflows alert, duration 1 5 hours, depends on nothing can run in parallel with s1 03 through s1 06. Keep type generation and to tracked pending state with stalledafterflows alert separate. Allow duration 1 5 hours and depends on nothing can run in parallel with s1 03 through s1 06 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: type generation, to tracked pending state with stalledafterflows alert, duration 1 5 hours, depends on nothing can run in parallel with s1 03 through s1 06.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

