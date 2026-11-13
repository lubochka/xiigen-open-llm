# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; read the output pattern before reading the code is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, read the output pattern before reading the code. Keep impact explanation and reprocess instruction separate. Allow approval boundary and read the output pattern before reading the code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, read the output pattern before reading the code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; read the output pattern before reading the code is present and needs its own handling; pattern library is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, read the output pattern before reading the code, pattern library. Keep reprocess instruction and approval boundary separate. Allow read the output pattern before reading the code and pattern library to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, read the output pattern before reading the code, pattern library.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: approval boundary is present and needs its own handling; read the output pattern before reading the code is present and needs its own handling; pattern library is present and needs its own handling; pattern 1 stagnation flatline at low score is present and needs its own handling.

B: Create work pieces for approval boundary, read the output pattern before reading the code, pattern library, pattern 1 stagnation flatline at low score. Keep approval boundary and read the output pattern before reading the code separate. Allow pattern library and pattern 1 stagnation flatline at low score to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, read the output pattern before reading the code, pattern library, pattern 1 stagnation flatline at low score.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: read the output pattern before reading the code is present and needs its own handling; pattern library is present and needs its own handling; pattern 1 stagnation flatline at low score is present and needs its own handling; pattern 2 oscillation score jumps chaotically is present and needs its own handling.

B: Create work pieces for read the output pattern before reading the code, pattern library, pattern 1 stagnation flatline at low score, pattern 2 oscillation score jumps chaotically. Keep read the output pattern before reading the code and pattern library separate. Allow pattern 1 stagnation flatline at low score and pattern 2 oscillation score jumps chaotically to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: read the output pattern before reading the code, pattern library, pattern 1 stagnation flatline at low score, pattern 2 oscillation score jumps chaotically.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Read the Output Pattern Before Reading the Code: pattern library is present and needs its own handling; pattern 1 stagnation flatline at low score is present and needs its own handling; pattern 2 oscillation score jumps chaotically is present and needs its own handling; pattern 4 first iteration works second ignores it is present and needs its own handling.

B: Create work pieces for pattern library, pattern 1 stagnation flatline at low score, pattern 2 oscillation score jumps chaotically, pattern 4 first iteration works second ignores it. Keep pattern library and pattern 1 stagnation flatline at low score separate. Allow pattern 2 oscillation score jumps chaotically and pattern 4 first iteration works second ignores it to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: pattern library, pattern 1 stagnation flatline at low score, pattern 2 oscillation score jumps chaotically, pattern 4 first iteration works second ignores it.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

