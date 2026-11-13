# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: session intent is present and needs its own handling; creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling.

B: Create work pieces for session intent, creation evidence, decision trail, resume state. Keep session intent and creation evidence separate. Allow decision trail and resume state to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: session intent, creation evidence, decision trail, resume state.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling.

B: Create work pieces for creation evidence, decision trail, resume state, open work. Keep creation evidence and decision trail separate. Allow resume state and open work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: creation evidence, decision trail, resume state, open work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling.

B: Create work pieces for decision trail, resume state, open work, validation notes. Keep decision trail and resume state separate. Allow open work and validation notes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: decision trail, resume state, open work, validation notes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling; checkpoint human verify most common 90 is present and needs its own handling.

B: Create work pieces for resume state, open work, validation notes, checkpoint human verify most common 90. Keep resume state and open work separate. Allow validation notes and checkpoint human verify most common 90 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: resume state, open work, validation notes, checkpoint human verify most common 90.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: open work is present and needs its own handling; validation notes is present and needs its own handling; checkpoint human verify most common 90 is present and needs its own handling; checkpoint decision 9 is present and needs its own handling.

B: Create work pieces for open work, validation notes, checkpoint human verify most common 90, checkpoint decision 9. Keep open work and validation notes separate. Allow checkpoint human verify most common 90 and checkpoint decision 9 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: open work, validation notes, checkpoint human verify most common 90, checkpoint decision 9.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: validation notes is present and needs its own handling; checkpoint human verify most common 90 is present and needs its own handling; checkpoint decision 9 is present and needs its own handling; checkpoint human action 1 rare is present and needs its own handling.

B: Create work pieces for validation notes, checkpoint human verify most common 90, checkpoint decision 9, checkpoint human action 1 rare. Keep validation notes and checkpoint human verify most common 90 separate. Allow checkpoint decision 9 and checkpoint human action 1 rare to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: validation notes, checkpoint human verify most common 90, checkpoint decision 9, checkpoint human action 1 rare.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: checkpoint human verify most common 90 is present and needs its own handling; checkpoint decision 9 is present and needs its own handling; checkpoint human action 1 rare is present and needs its own handling; stop immediately do not proceed to next task is present and needs its own handling.

B: Create work pieces for checkpoint human verify most common 90, checkpoint decision 9, checkpoint human action 1 rare, stop immediately do not proceed to next task. Keep checkpoint human verify most common 90 and checkpoint decision 9 separate. Allow checkpoint human action 1 rare and stop immediately do not proceed to next task to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: checkpoint human verify most common 90, checkpoint decision 9, checkpoint human action 1 rare, stop immediately do not proceed to next task.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds: checkpoint decision 9 is present and needs its own handling; checkpoint human action 1 rare is present and needs its own handling; stop immediately do not proceed to next task is present and needs its own handling; display checkpoint clearly using the format below is present and needs its own handling.

B: Create work pieces for checkpoint decision 9, checkpoint human action 1 rare, stop immediately do not proceed to next task, display checkpoint clearly using the format below. Keep checkpoint decision 9 and checkpoint human action 1 rare separate. Allow stop immediately do not proceed to next task and display checkpoint clearly using the format below to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: checkpoint decision 9, checkpoint human action 1 rare, stop immediately do not proceed to next task, display checkpoint clearly using the format below.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

