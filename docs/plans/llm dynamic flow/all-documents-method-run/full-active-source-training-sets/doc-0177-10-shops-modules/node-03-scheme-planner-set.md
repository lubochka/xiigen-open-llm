# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; big picture two halves of the product is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, big picture two halves of the product. Keep impact explanation and reprocess instruction separate. Allow approval boundary and big picture two halves of the product to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, big picture two halves of the product.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; big picture two halves of the product is present and needs its own handling; a public experience front end is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, big picture two halves of the product, a public experience front end. Keep reprocess instruction and approval boundary separate. Allow big picture two halves of the product and a public experience front end to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, big picture two halves of the product, a public experience front end.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: approval boundary is present and needs its own handling; big picture two halves of the product is present and needs its own handling; a public experience front end is present and needs its own handling; b admin experience back office is present and needs its own handling.

B: Create work pieces for approval boundary, big picture two halves of the product, a public experience front end, b admin experience back office. Keep approval boundary and big picture two halves of the product separate. Allow a public experience front end and b admin experience back office to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, big picture two halves of the product, a public experience front end, b admin experience back office.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: big picture two halves of the product is present and needs its own handling; a public experience front end is present and needs its own handling; b admin experience back office is present and needs its own handling; core modules common to wordpress like and shopify like is present and needs its own handling.

B: Create work pieces for big picture two halves of the product, a public experience front end, b admin experience back office, core modules common to wordpress like and shopify like. Keep big picture two halves of the product and a public experience front end separate. Allow b admin experience back office and core modules common to wordpress like and shopify like to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: big picture two halves of the product, a public experience front end, b admin experience back office, core modules common to wordpress like and shopify like.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Big picture: two halves of the product: a public experience front end is present and needs its own handling; b admin experience back office is present and needs its own handling; core modules common to wordpress like and shopify like is present and needs its own handling; identity access auth roles permissions is present and needs its own handling.

B: Create work pieces for a public experience front end, b admin experience back office, core modules common to wordpress like and shopify like, identity access auth roles permissions. Keep a public experience front end and b admin experience back office separate. Allow core modules common to wordpress like and shopify like and identity access auth roles permissions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: a public experience front end, b admin experience back office, core modules common to wordpress like and shopify like, identity access auth roles permissions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

