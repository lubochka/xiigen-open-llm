# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; origin the goal these hypotheses serve is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, origin the goal these hypotheses serve. Keep impact explanation and reprocess instruction separate. Allow approval boundary and origin the goal these hypotheses serve to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, origin the goal these hypotheses serve.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; origin the goal these hypotheses serve is present and needs its own handling; what these requests require distilled is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, origin the goal these hypotheses serve, what these requests require distilled. Keep reprocess instruction and approval boundary separate. Allow origin the goal these hypotheses serve and what these requests require distilled to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, origin the goal these hypotheses serve, what these requests require distilled.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: approval boundary is present and needs its own handling; origin the goal these hypotheses serve is present and needs its own handling; what these requests require distilled is present and needs its own handling; the goal being tested is present and needs its own handling.

B: Create work pieces for approval boundary, origin the goal these hypotheses serve, what these requests require distilled, the goal being tested. Keep approval boundary and origin the goal these hypotheses serve separate. Allow what these requests require distilled and the goal being tested to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, origin the goal these hypotheses serve, what these requests require distilled, the goal being tested.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: origin the goal these hypotheses serve is present and needs its own handling; what these requests require distilled is present and needs its own handling; the goal being tested is present and needs its own handling; edge condition is present and needs its own handling.

B: Create work pieces for origin the goal these hypotheses serve, what these requests require distilled, the goal being tested, edge condition. Keep origin the goal these hypotheses serve and what these requests require distilled separate. Allow the goal being tested and edge condition to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: origin the goal these hypotheses serve, what these requests require distilled, the goal being tested, edge condition.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Origin — The Goal These Hypotheses Serve: what these requests require distilled is present and needs its own handling; the goal being tested is present and needs its own handling; edge condition is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for what these requests require distilled, the goal being tested, edge condition, training example. Keep what these requests require distilled and the goal being tested separate. Allow edge condition and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what these requests require distilled, the goal being tested, edge condition, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

