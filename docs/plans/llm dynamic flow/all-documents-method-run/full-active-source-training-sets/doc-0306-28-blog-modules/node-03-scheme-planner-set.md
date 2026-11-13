# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; content model module entities posts pages is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, content model module entities posts pages. Keep impact explanation and reprocess instruction separate. Allow approval boundary and content model module entities posts pages to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, content model module entities posts pages.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; content model module entities posts pages is present and needs its own handling; editor module admin ui rich text blocks is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, content model module entities posts pages, editor module admin ui rich text blocks. Keep reprocess instruction and approval boundary separate. Allow content model module entities posts pages and editor module admin ui rich text blocks to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, content model module entities posts pages, editor module admin ui rich text blocks.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: approval boundary is present and needs its own handling; content model module entities posts pages is present and needs its own handling; editor module admin ui rich text blocks is present and needs its own handling; taxonomy module categories tags terms is present and needs its own handling.

B: Create work pieces for approval boundary, content model module entities posts pages, editor module admin ui rich text blocks, taxonomy module categories tags terms. Keep approval boundary and content model module entities posts pages separate. Allow editor module admin ui rich text blocks and taxonomy module categories tags terms to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, content model module entities posts pages, editor module admin ui rich text blocks, taxonomy module categories tags terms.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: content model module entities posts pages is present and needs its own handling; editor module admin ui rich text blocks is present and needs its own handling; taxonomy module categories tags terms is present and needs its own handling; users roles permissions module authn authz is present and needs its own handling.

B: Create work pieces for content model module entities posts pages, editor module admin ui rich text blocks, taxonomy module categories tags terms, users roles permissions module authn authz. Keep content model module entities posts pages and editor module admin ui rich text blocks separate. Allow taxonomy module categories tags terms and users roles permissions module authn authz to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: content model module entities posts pages, editor module admin ui rich text blocks, taxonomy module categories tags terms, users roles permissions module authn authz.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Content Model Module Entities / Posts / Pages: editor module admin ui rich text blocks is present and needs its own handling; taxonomy module categories tags terms is present and needs its own handling; users roles permissions module authn authz is present and needs its own handling; routing url management permalinks is present and needs its own handling.

B: Create work pieces for editor module admin ui rich text blocks, taxonomy module categories tags terms, users roles permissions module authn authz, routing url management permalinks. Keep editor module admin ui rich text blocks and taxonomy module categories tags terms separate. Allow users roles permissions module authn authz and routing url management permalinks to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: editor module admin ui rich text blocks, taxonomy module categories tags terms, users roles permissions module authn authz, routing url management permalinks.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

