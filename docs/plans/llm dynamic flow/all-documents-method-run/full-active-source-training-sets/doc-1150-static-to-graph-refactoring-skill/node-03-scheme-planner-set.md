# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, when to invoke. Keep impact explanation and reprocess instruction separate. Allow approval boundary and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; when to invoke is present and needs its own handling; what this prevents is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, when to invoke, what this prevents. Keep reprocess instruction and approval boundary separate. Allow when to invoke and what this prevents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, when to invoke, what this prevents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: approval boundary is present and needs its own handling; when to invoke is present and needs its own handling; what this prevents is present and needs its own handling; the refactoring protocol is present and needs its own handling.

B: Create work pieces for approval boundary, when to invoke, what this prevents, the refactoring protocol. Keep approval boundary and when to invoke separate. Allow what this prevents and the refactoring protocol to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, when to invoke, what this prevents, the refactoring protocol.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: when to invoke is present and needs its own handling; what this prevents is present and needs its own handling; the refactoring protocol is present and needs its own handling; step 1 identify static decisions in the algorithm is present and needs its own handling.

B: Create work pieces for when to invoke, what this prevents, the refactoring protocol, step 1 identify static decisions in the algorithm. Keep when to invoke and what this prevents separate. Allow the refactoring protocol and step 1 identify static decisions in the algorithm to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: when to invoke, what this prevents, the refactoring protocol, step 1 identify static decisions in the algorithm.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Static-to-graph-refactoring-SKILL.md v1.0 — Upgrade hardcoded decisions to graph-learned decisions: what this prevents is present and needs its own handling; the refactoring protocol is present and needs its own handling; step 1 identify static decisions in the algorithm is present and needs its own handling; step 3 refactor to configurable preserve static fallback is present and needs its own handling.

B: Create work pieces for what this prevents, the refactoring protocol, step 1 identify static decisions in the algorithm, step 3 refactor to configurable preserve static fallback. Keep what this prevents and the refactoring protocol separate. Allow step 1 identify static decisions in the algorithm and step 3 refactor to configurable preserve static fallback to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what this prevents, the refactoring protocol, step 1 identify static decisions in the algorithm, step 3 refactor to configurable preserve static fallback.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

