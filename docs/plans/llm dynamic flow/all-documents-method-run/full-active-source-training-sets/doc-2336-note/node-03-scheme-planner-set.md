# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; project scope used when exists in cwd is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, project scope used when exists in cwd. Keep graph relationships and evidence selection separate. Allow quality measurement and project scope used when exists in cwd to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, project scope used when exists in cwd.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; project scope used when exists in cwd is present and needs its own handling; global scope fallback when no or when flag is present is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, project scope used when exists in cwd, global scope fallback when no or when flag is present. Keep evidence selection and quality measurement separate. Allow project scope used when exists in cwd and global scope fallback when no or when flag is present to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, project scope used when exists in cwd, global scope fallback when no or when flag is present.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: quality measurement is present and needs its own handling; project scope used when exists in cwd is present and needs its own handling; global scope fallback when no or when flag is present is present and needs its own handling; never modify the note text capture verbatim including typos is present and needs its own handling.

B: Create work pieces for quality measurement, project scope used when exists in cwd, global scope fallback when no or when flag is present, never modify the note text capture verbatim including typos. Keep quality measurement and project scope used when exists in cwd separate. Allow global scope fallback when no or when flag is present and never modify the note text capture verbatim including typos to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, project scope used when exists in cwd, global scope fallback when no or when flag is present, never modify the note text capture verbatim including typos.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: project scope used when exists in cwd is present and needs its own handling; global scope fallback when no or when flag is present is present and needs its own handling; never modify the note text capture verbatim including typos is present and needs its own handling; never ask questions just write and confirm is present and needs its own handling.

B: Create work pieces for project scope used when exists in cwd, global scope fallback when no or when flag is present, never modify the note text capture verbatim including typos, never ask questions just write and confirm. Keep project scope used when exists in cwd and global scope fallback when no or when flag is present separate. Allow never modify the note text capture verbatim including typos and never ask questions just write and confirm to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: project scope used when exists in cwd, global scope fallback when no or when flag is present, never modify the note text capture verbatim including typos, never ask questions just write and confirm.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Project scope : — used when exists in cwd: global scope fallback when no or when flag is present is present and needs its own handling; never modify the note text capture verbatim including typos is present and needs its own handling; never ask questions just write and confirm is present and needs its own handling; timestamp format use local time 24 hour no seconds is present and needs its own handling.

B: Create work pieces for global scope fallback when no or when flag is present, never modify the note text capture verbatim including typos, never ask questions just write and confirm, timestamp format use local time 24 hour no seconds. Keep global scope fallback when no or when flag is present and never modify the note text capture verbatim including typos separate. Allow never ask questions just write and confirm and timestamp format use local time 24 hour no seconds to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: global scope fallback when no or when flag is present, never modify the note text capture verbatim including typos, never ask questions just write and confirm, timestamp format use local time 24 hour no seconds.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

