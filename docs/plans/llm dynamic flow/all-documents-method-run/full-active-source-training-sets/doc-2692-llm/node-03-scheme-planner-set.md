# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; inverted index term postingslist docid frequency positions is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, inverted index term postingslist docid frequency positions. Keep graph relationships and evidence selection separate. Allow quality measurement and inverted index term postingslist docid frequency positions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, inverted index term postingslist docid frequency positions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; inverted index term postingslist docid frequency positions is present and needs its own handling; atomic persistence write tmp then file move overwrite true is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, inverted index term postingslist docid frequency positions, atomic persistence write tmp then file move overwrite true. Keep evidence selection and quality measurement separate. Allow inverted index term postingslist docid frequency positions and atomic persistence write tmp then file move overwrite true to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, inverted index term postingslist docid frequency positions, atomic persistence write tmp then file move overwrite true.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: quality measurement is present and needs its own handling; inverted index term postingslist docid frequency positions is present and needs its own handling; atomic persistence write tmp then file move overwrite true is present and needs its own handling; ols linear regression closed form slope xi x yi xi x is present and needs its own handling.

B: Create work pieces for quality measurement, inverted index term postingslist docid frequency positions, atomic persistence write tmp then file move overwrite true, ols linear regression closed form slope xi x yi xi x. Keep quality measurement and inverted index term postingslist docid frequency positions separate. Allow atomic persistence write tmp then file move overwrite true and ols linear regression closed form slope xi x yi xi x to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, inverted index term postingslist docid frequency positions, atomic persistence write tmp then file move overwrite true, ols linear regression closed form slope xi x yi xi x.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: inverted index term postingslist docid frequency positions is present and needs its own handling; atomic persistence write tmp then file move overwrite true is present and needs its own handling; ols linear regression closed form slope xi x yi xi x is present and needs its own handling; multinomial naive bayes with laplace smoothing is present and needs its own handling.

B: Create work pieces for inverted index term postingslist docid frequency positions, atomic persistence write tmp then file move overwrite true, ols linear regression closed form slope xi x yi xi x, multinomial naive bayes with laplace smoothing. Keep inverted index term postingslist docid frequency positions and atomic persistence write tmp then file move overwrite true separate. Allow ols linear regression closed form slope xi x yi xi x and multinomial naive bayes with laplace smoothing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: inverted index term postingslist docid frequency positions, atomic persistence write tmp then file move overwrite true, ols linear regression closed form slope xi x yi xi x, multinomial naive bayes with laplace smoothing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Fabric-first — all external dependencies behind interfaces; no new ConcreteClass inside services: atomic persistence write tmp then file move overwrite true is present and needs its own handling; ols linear regression closed form slope xi x yi xi x is present and needs its own handling; multinomial naive bayes with laplace smoothing is present and needs its own handling; k means distance weighted centroid initialization is present and needs its own handling.

B: Create work pieces for atomic persistence write tmp then file move overwrite true, ols linear regression closed form slope xi x yi xi x, multinomial naive bayes with laplace smoothing, k means distance weighted centroid initialization. Keep atomic persistence write tmp then file move overwrite true and ols linear regression closed form slope xi x yi xi x separate. Allow multinomial naive bayes with laplace smoothing and k means distance weighted centroid initialization to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: atomic persistence write tmp then file move overwrite true, ols linear regression closed form slope xi x yi xi x, multinomial naive bayes with laplace smoothing, k means distance weighted centroid initialization.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

