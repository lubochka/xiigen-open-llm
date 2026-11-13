# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Prime Directives: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Prime Directives: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Prime Directives: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Prime Directives: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; prime directives is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, prime directives. Keep impact explanation and reprocess instruction separate. Allow approval boundary and prime directives to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, prime directives.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Prime Directives: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; prime directives is present and needs its own handling; the 9 immutable laws of skill creation is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, prime directives, the 9 immutable laws of skill creation. Keep reprocess instruction and approval boundary separate. Allow prime directives and the 9 immutable laws of skill creation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, prime directives, the 9 immutable laws of skill creation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Prime Directives: approval boundary is present and needs its own handling; prime directives is present and needs its own handling; the 9 immutable laws of skill creation is present and needs its own handling; directive 1 read master coordinate md first is present and needs its own handling.

B: Create work pieces for approval boundary, prime directives, the 9 immutable laws of skill creation, directive 1 read master coordinate md first. Keep approval boundary and prime directives separate. Allow the 9 immutable laws of skill creation and directive 1 read master coordinate md first to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, prime directives, the 9 immutable laws of skill creation, directive 1 read master coordinate md first.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Prime Directives: prime directives is present and needs its own handling; the 9 immutable laws of skill creation is present and needs its own handling; directive 1 read master coordinate md first is present and needs its own handling; directive 2 never exceed 500 lines in skill md is present and needs its own handling.

B: Create work pieces for prime directives, the 9 immutable laws of skill creation, directive 1 read master coordinate md first, directive 2 never exceed 500 lines in skill md. Keep prime directives and the 9 immutable laws of skill creation separate. Allow directive 1 read master coordinate md first and directive 2 never exceed 500 lines in skill md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: prime directives, the 9 immutable laws of skill creation, directive 1 read master coordinate md first, directive 2 never exceed 500 lines in skill md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Prime Directives: the 9 immutable laws of skill creation is present and needs its own handling; directive 1 read master coordinate md first is present and needs its own handling; directive 2 never exceed 500 lines in skill md is present and needs its own handling; extract to references is present and needs its own handling.

B: Create work pieces for the 9 immutable laws of skill creation, directive 1 read master coordinate md first, directive 2 never exceed 500 lines in skill md, extract to references. Keep the 9 immutable laws of skill creation and directive 1 read master coordinate md first separate. Allow directive 2 never exceed 500 lines in skill md and extract to references to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the 9 immutable laws of skill creation, directive 1 read master coordinate md first, directive 2 never exceed 500 lines in skill md, extract to references.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

