# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; negative example what the engine rejects is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, negative example what the engine rejects. Keep impact explanation and reprocess instruction separate. Allow approval boundary and negative example what the engine rejects to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, negative example what the engine rejects.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; negative example what the engine rejects is present and needs its own handling; artifact numbers the design intelligence flow is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, negative example what the engine rejects, artifact numbers the design intelligence flow. Keep reprocess instruction and approval boundary separate. Allow negative example what the engine rejects and artifact numbers the design intelligence flow to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, negative example what the engine rejects, artifact numbers the design intelligence flow.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: approval boundary is present and needs its own handling; negative example what the engine rejects is present and needs its own handling; artifact numbers the design intelligence flow is present and needs its own handling; status complete is present and needs its own handling.

B: Create work pieces for approval boundary, negative example what the engine rejects, artifact numbers the design intelligence flow, status complete. Keep approval boundary and negative example what the engine rejects separate. Allow artifact numbers the design intelligence flow and status complete to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, negative example what the engine rejects, artifact numbers the design intelligence flow, status complete.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: negative example what the engine rejects is present and needs its own handling; artifact numbers the design intelligence flow is present and needs its own handling; status complete is present and needs its own handling; edge condition is present and needs its own handling.

B: Create work pieces for negative example what the engine rejects, artifact numbers the design intelligence flow, status complete, edge condition. Keep negative example what the engine rejects and artifact numbers the design intelligence flow separate. Allow status complete and edge condition to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: negative example what the engine rejects, artifact numbers the design intelligence flow, status complete, edge condition.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for NEGATIVE EXAMPLE what the engine rejects: artifact numbers the design intelligence flow is present and needs its own handling; status complete is present and needs its own handling; edge condition is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for artifact numbers the design intelligence flow, status complete, edge condition, training example. Keep artifact numbers the design intelligence flow and status complete separate. Allow edge condition and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: artifact numbers the design intelligence flow, status complete, edge condition, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

