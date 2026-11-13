# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; skill 20 authentication service is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, skill 20 authentication service. Keep graph relationships and evidence selection separate. Allow quality measurement and skill 20 authentication service to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, skill 20 authentication service.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; skill 20 authentication service is present and needs its own handling; jwt authentication the trust boundary is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, skill 20 authentication service, jwt authentication the trust boundary. Keep evidence selection and quality measurement separate. Allow skill 20 authentication service and jwt authentication the trust boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, skill 20 authentication service, jwt authentication the trust boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: quality measurement is present and needs its own handling; skill 20 authentication service is present and needs its own handling; jwt authentication the trust boundary is present and needs its own handling; genie dna integration is present and needs its own handling.

B: Create work pieces for quality measurement, skill 20 authentication service, jwt authentication the trust boundary, genie dna integration. Keep quality measurement and skill 20 authentication service separate. Allow jwt authentication the trust boundary and genie dna integration to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, skill 20 authentication service, jwt authentication the trust boundary, genie dna integration.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: skill 20 authentication service is present and needs its own handling; jwt authentication the trust boundary is present and needs its own handling; genie dna integration is present and needs its own handling; why auth is machine not freedom is present and needs its own handling.

B: Create work pieces for skill 20 authentication service, jwt authentication the trust boundary, genie dna integration, why auth is machine not freedom. Keep skill 20 authentication service and jwt authentication the trust boundary separate. Allow genie dna integration and why auth is machine not freedom to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: skill 20 authentication service, jwt authentication the trust boundary, genie dna integration, why auth is machine not freedom.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Skill 20: Authentication Service: jwt authentication the trust boundary is present and needs its own handling; genie dna integration is present and needs its own handling; why auth is machine not freedom is present and needs its own handling; dynamic document storage the content type rendering pattern is present and needs its own handling.

B: Create work pieces for jwt authentication the trust boundary, genie dna integration, why auth is machine not freedom, dynamic document storage the content type rendering pattern. Keep jwt authentication the trust boundary and genie dna integration separate. Allow why auth is machine not freedom and dynamic document storage the content type rendering pattern to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: jwt authentication the trust boundary, genie dna integration, why auth is machine not freedom, dynamic document storage the content type rendering pattern.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

