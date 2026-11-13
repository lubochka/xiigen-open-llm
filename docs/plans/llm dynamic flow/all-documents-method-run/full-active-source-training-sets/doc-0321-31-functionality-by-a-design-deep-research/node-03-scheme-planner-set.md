# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; problem definition and success criteria is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, problem definition and success criteria. Keep impact explanation and reprocess instruction separate. Allow approval boundary and problem definition and success criteria to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, problem definition and success criteria.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; problem definition and success criteria is present and needs its own handling; ingesting and normalizing design data from figma is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, problem definition and success criteria, ingesting and normalizing design data from figma. Keep reprocess instruction and approval boundary separate. Allow problem definition and success criteria and ingesting and normalizing design data from figma to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, problem definition and success criteria, ingesting and normalizing design data from figma.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: approval boundary is present and needs its own handling; problem definition and success criteria is present and needs its own handling; ingesting and normalizing design data from figma is present and needs its own handling; turning screens into machine readable ui graphs is present and needs its own handling.

B: Create work pieces for approval boundary, problem definition and success criteria, ingesting and normalizing design data from figma, turning screens into machine readable ui graphs. Keep approval boundary and problem definition and success criteria separate. Allow ingesting and normalizing design data from figma and turning screens into machine readable ui graphs to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, problem definition and success criteria, ingesting and normalizing design data from figma, turning screens into machine readable ui graphs.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: problem definition and success criteria is present and needs its own handling; ingesting and normalizing design data from figma is present and needs its own handling; turning screens into machine readable ui graphs is present and needs its own handling; evaluation governance and rollout strategy is present and needs its own handling.

B: Create work pieces for problem definition and success criteria, ingesting and normalizing design data from figma, turning screens into machine readable ui graphs, evaluation governance and rollout strategy. Keep problem definition and success criteria and ingesting and normalizing design data from figma separate. Allow turning screens into machine readable ui graphs and evaluation governance and rollout strategy to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: problem definition and success criteria, ingesting and normalizing design data from figma, turning screens into machine readable ui graphs, evaluation governance and rollout strategy.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Problem definition and success criteria: ingesting and normalizing design data from figma is present and needs its own handling; turning screens into machine readable ui graphs is present and needs its own handling; evaluation governance and rollout strategy is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for ingesting and normalizing design data from figma, turning screens into machine readable ui graphs, evaluation governance and rollout strategy, training example. Keep ingesting and normalizing design data from figma and turning screens into machine readable ui graphs separate. Allow evaluation governance and rollout strategy and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: ingesting and normalizing design data from figma, turning screens into machine readable ui graphs, evaluation governance and rollout strategy, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

