# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: source inventory is present and needs its own handling; metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling.

B: Create work pieces for source inventory, metadata and tags, retrieval routing, graph relationships. Keep source inventory and metadata and tags separate. Allow retrieval routing and graph relationships to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: source inventory, metadata and tags, retrieval routing, graph relationships.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: metadata and tags is present and needs its own handling; retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling.

B: Create work pieces for metadata and tags, retrieval routing, graph relationships, evidence selection. Keep metadata and tags and retrieval routing separate. Allow graph relationships and evidence selection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: metadata and tags, retrieval routing, graph relationships, evidence selection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: retrieval routing is present and needs its own handling; graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling.

B: Create work pieces for retrieval routing, graph relationships, evidence selection, quality measurement. Keep retrieval routing and graph relationships separate. Allow evidence selection and quality measurement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: retrieval routing, graph relationships, evidence selection, quality measurement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: graph relationships is present and needs its own handling; evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; why translation should be variant generation and not code porting is present and needs its own handling.

B: Create work pieces for graph relationships, evidence selection, quality measurement, why translation should be variant generation and not code porting. Keep graph relationships and evidence selection separate. Allow quality measurement and why translation should be variant generation and not code porting to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: graph relationships, evidence selection, quality measurement, why translation should be variant generation and not code porting.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: evidence selection is present and needs its own handling; quality measurement is present and needs its own handling; why translation should be variant generation and not code porting is present and needs its own handling; contract surface for synchronous apis is present and needs its own handling.

B: Create work pieces for evidence selection, quality measurement, why translation should be variant generation and not code porting, contract surface for synchronous apis. Keep evidence selection and quality measurement separate. Allow why translation should be variant generation and not code porting and contract surface for synchronous apis to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: evidence selection, quality measurement, why translation should be variant generation and not code porting, contract surface for synchronous apis.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: quality measurement is present and needs its own handling; why translation should be variant generation and not code porting is present and needs its own handling; contract surface for synchronous apis is present and needs its own handling; schema surface for documents and skill payloads is present and needs its own handling.

B: Create work pieces for quality measurement, why translation should be variant generation and not code porting, contract surface for synchronous apis, schema surface for documents and skill payloads. Keep quality measurement and why translation should be variant generation and not code porting separate. Allow contract surface for synchronous apis and schema surface for documents and skill payloads to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: quality measurement, why translation should be variant generation and not code porting, contract surface for synchronous apis, schema surface for documents and skill payloads.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: why translation should be variant generation and not code porting is present and needs its own handling; contract surface for synchronous apis is present and needs its own handling; schema surface for documents and skill payloads is present and needs its own handling; event surface for asynchronous orchestration is present and needs its own handling.

B: Create work pieces for why translation should be variant generation and not code porting, contract surface for synchronous apis, schema surface for documents and skill payloads, event surface for asynchronous orchestration. Keep why translation should be variant generation and not code porting and contract surface for synchronous apis separate. Allow schema surface for documents and skill payloads and event surface for asynchronous orchestration to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: why translation should be variant generation and not code porting, contract surface for synchronous apis, schema surface for documents and skill payloads, event surface for asynchronous orchestration.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Multi-Target Skill Translation for XIIGen Through Alternatives and GraphRAG: contract surface for synchronous apis is present and needs its own handling; schema surface for documents and skill payloads is present and needs its own handling; event surface for asynchronous orchestration is present and needs its own handling; conformance testing as the invariant enforcement layer is present and needs its own handling.

B: Create work pieces for contract surface for synchronous apis, schema surface for documents and skill payloads, event surface for asynchronous orchestration, conformance testing as the invariant enforcement layer. Keep contract surface for synchronous apis and schema surface for documents and skill payloads separate. Allow event surface for asynchronous orchestration and conformance testing as the invariant enforcement layer to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: contract surface for synchronous apis, schema surface for documents and skill payloads, event surface for asynchronous orchestration, conformance testing as the invariant enforcement layer.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

