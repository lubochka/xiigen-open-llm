# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; doc update plan addendum is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, doc update plan addendum. Keep graph relationships and evidence selection separate. Allow quality measurement and doc update plan addendum to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, doc update plan addendum.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; doc update plan addendum is present and needs its own handling; changes made after the original plan was written is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, doc update plan addendum, changes made after the original plan was written. Keep evidence selection and quality measurement separate. Allow doc update plan addendum and changes made after the original plan was written to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, doc update plan addendum, changes made after the original plan was written.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: quality measurement is present and needs its own handling; doc update plan addendum is present and needs its own handling; changes made after the original plan was written is present and needs its own handling; part 1 status corrections planned implemented is present and needs its own handling.

B: Create work pieces for quality measurement, doc update plan addendum, changes made after the original plan was written, part 1 status corrections planned implemented. Keep quality measurement and doc update plan addendum separate. Allow changes made after the original plan was written and part 1 status corrections planned implemented to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, doc update plan addendum, changes made after the original plan was written, part 1 status corrections planned implemented.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: doc update plan addendum is present and needs its own handling; changes made after the original plan was written is present and needs its own handling; part 1 status corrections planned implemented is present and needs its own handling; part 2 new items not in original plan is present and needs its own handling.

B: Create work pieces for doc update plan addendum, changes made after the original plan was written, part 1 status corrections planned implemented, part 2 new items not in original plan. Keep doc update plan addendum and changes made after the original plan was written separate. Allow part 1 status corrections planned implemented and part 2 new items not in original plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: doc update plan addendum, changes made after the original plan was written, part 1 status corrections planned implemented, part 2 new items not in original plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for DOC UPDATE PLAN — Addendum: changes made after the original plan was written is present and needs its own handling; part 1 status corrections planned implemented is present and needs its own handling; part 2 new items not in original plan is present and needs its own handling; a design generator full prompt overhaul is present and needs its own handling.

B: Create work pieces for changes made after the original plan was written, part 1 status corrections planned implemented, part 2 new items not in original plan, a design generator full prompt overhaul. Keep changes made after the original plan was written and part 1 status corrections planned implemented separate. Allow part 2 new items not in original plan and a design generator full prompt overhaul to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: changes made after the original plan was written, part 1 status corrections planned implemented, part 2 new items not in original plan, a design generator full prompt overhaul.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

