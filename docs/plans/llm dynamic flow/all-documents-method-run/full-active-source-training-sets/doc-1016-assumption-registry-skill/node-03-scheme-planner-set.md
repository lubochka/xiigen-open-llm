# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; why this exists is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, why this exists. Keep impact explanation and reprocess instruction separate. Allow approval boundary and why this exists to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, why this exists.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; why this exists is present and needs its own handling; assumption classes is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, why this exists, assumption classes. Keep reprocess instruction and approval boundary separate. Allow why this exists and assumption classes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, why this exists, assumption classes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: approval boundary is present and needs its own handling; why this exists is present and needs its own handling; assumption classes is present and needs its own handling; class 1 infrastructure verifiable by grep or file existence is present and needs its own handling.

B: Create work pieces for approval boundary, why this exists, assumption classes, class 1 infrastructure verifiable by grep or file existence. Keep approval boundary and why this exists separate. Allow assumption classes and class 1 infrastructure verifiable by grep or file existence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, why this exists, assumption classes, class 1 infrastructure verifiable by grep or file existence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: why this exists is present and needs its own handling; assumption classes is present and needs its own handling; class 1 infrastructure verifiable by grep or file existence is present and needs its own handling; class 2 behavioral verifiable by reading the code is present and needs its own handling.

B: Create work pieces for why this exists, assumption classes, class 1 infrastructure verifiable by grep or file existence, class 2 behavioral verifiable by reading the code. Keep why this exists and assumption classes separate. Allow class 1 infrastructure verifiable by grep or file existence and class 2 behavioral verifiable by reading the code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: why this exists, assumption classes, class 1 infrastructure verifiable by grep or file existence, class 2 behavioral verifiable by reading the code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Assumption-registry-SKILL.md v1.0 — Register and verify assumptions before plan execution: assumption classes is present and needs its own handling; class 1 infrastructure verifiable by grep or file existence is present and needs its own handling; class 2 behavioral verifiable by reading the code is present and needs its own handling; class 3 quantitative verifiable by running a command is present and needs its own handling.

B: Create work pieces for assumption classes, class 1 infrastructure verifiable by grep or file existence, class 2 behavioral verifiable by reading the code, class 3 quantitative verifiable by running a command. Keep assumption classes and class 1 infrastructure verifiable by grep or file existence separate. Allow class 2 behavioral verifiable by reading the code and class 3 quantitative verifiable by running a command to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: assumption classes, class 1 infrastructure verifiable by grep or file existence, class 2 behavioral verifiable by reading the code, class 3 quantitative verifiable by running a command.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

