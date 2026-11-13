# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; core principle is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, core principle. Keep impact explanation and reprocess instruction separate. Allow approval boundary and core principle to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, core principle.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; core principle is present and needs its own handling; the 4 layers is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, core principle, the 4 layers. Keep reprocess instruction and approval boundary separate. Allow core principle and the 4 layers to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, core principle, the 4 layers.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: approval boundary is present and needs its own handling; core principle is present and needs its own handling; the 4 layers is present and needs its own handling; layer 1 entry point guard is present and needs its own handling.

B: Create work pieces for approval boundary, core principle, the 4 layers, layer 1 entry point guard. Keep approval boundary and core principle separate. Allow the 4 layers and layer 1 entry point guard to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, core principle, the 4 layers, layer 1 entry point guard.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: core principle is present and needs its own handling; the 4 layers is present and needs its own handling; layer 1 entry point guard is present and needs its own handling; layer 2 stage boundary guard is present and needs its own handling.

B: Create work pieces for core principle, the 4 layers, layer 1 entry point guard, layer 2 stage boundary guard. Keep core principle and the 4 layers separate. Allow layer 1 entry point guard and layer 2 stage boundary guard to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: core principle, the 4 layers, layer 1 entry point guard, layer 2 stage boundary guard.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Defense-in-depth-SKILL.md v1.0 — Validate at every layer data passes through: the 4 layers is present and needs its own handling; layer 1 entry point guard is present and needs its own handling; layer 2 stage boundary guard is present and needs its own handling; layer 3 business logic guard is present and needs its own handling.

B: Create work pieces for the 4 layers, layer 1 entry point guard, layer 2 stage boundary guard, layer 3 business logic guard. Keep the 4 layers and layer 1 entry point guard separate. Allow layer 2 stage boundary guard and layer 3 business logic guard to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the 4 layers, layer 1 entry point guard, layer 2 stage boundary guard, layer 3 business logic guard.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

