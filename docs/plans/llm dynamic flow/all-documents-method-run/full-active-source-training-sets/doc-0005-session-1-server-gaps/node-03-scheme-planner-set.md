# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: session intent is present and needs its own handling; creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling.

B: Create work pieces for session intent, creation evidence, decision trail, resume state. Keep session intent and creation evidence separate. Allow decision trail and resume state to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: session intent, creation evidence, decision trail, resume state.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling.

B: Create work pieces for creation evidence, decision trail, resume state, open work. Keep creation evidence and decision trail separate. Allow resume state and open work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: creation evidence, decision trail, resume state, open work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling.

B: Create work pieces for decision trail, resume state, open work, validation notes. Keep decision trail and resume state separate. Allow open work and validation notes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: decision trail, resume state, open work, validation notes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling; session 1 server middleware decorator unit tests is present and needs its own handling.

B: Create work pieces for resume state, open work, validation notes, session 1 server middleware decorator unit tests. Keep resume state and open work separate. Allow validation notes and session 1 server middleware decorator unit tests to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: resume state, open work, validation notes, session 1 server middleware decorator unit tests.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: open work is present and needs its own handling; validation notes is present and needs its own handling; session 1 server middleware decorator unit tests is present and needs its own handling; consumer claude code is present and needs its own handling.

B: Create work pieces for open work, validation notes, session 1 server middleware decorator unit tests, consumer claude code. Keep open work and validation notes separate. Allow session 1 server middleware decorator unit tests and consumer claude code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: open work, validation notes, session 1 server middleware decorator unit tests, consumer claude code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: validation notes is present and needs its own handling; session 1 server middleware decorator unit tests is present and needs its own handling; consumer claude code is present and needs its own handling; prerequisite session 0 fc checks all pass and luba approved is present and needs its own handling.

B: Create work pieces for validation notes, session 1 server middleware decorator unit tests, consumer claude code, prerequisite session 0 fc checks all pass and luba approved. Keep validation notes and session 1 server middleware decorator unit tests separate. Allow consumer claude code and prerequisite session 0 fc checks all pass and luba approved to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: validation notes, session 1 server middleware decorator unit tests, consumer claude code, prerequisite session 0 fc checks all pass and luba approved.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: session 1 server middleware decorator unit tests is present and needs its own handling; consumer claude code is present and needs its own handling; prerequisite session 0 fc checks all pass and luba approved is present and needs its own handling; scope is present and needs its own handling.

B: Create work pieces for session 1 server middleware decorator unit tests, consumer claude code, prerequisite session 0 fc checks all pass and luba approved, scope. Keep session 1 server middleware decorator unit tests and consumer claude code separate. Allow prerequisite session 0 fc checks all pass and luba approved and scope to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: session 1 server middleware decorator unit tests, consumer claude code, prerequisite session 0 fc checks all pass and luba approved, scope.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Session 1 — Server: Middleware + Decorator Unit Tests: consumer claude code is present and needs its own handling; prerequisite session 0 fc checks all pass and luba approved is present and needs its own handling; scope is present and needs its own handling; step 1 verify baseline before touching anything is present and needs its own handling.

B: Create work pieces for consumer claude code, prerequisite session 0 fc checks all pass and luba approved, scope, step 1 verify baseline before touching anything. Keep consumer claude code and prerequisite session 0 fc checks all pass and luba approved separate. Allow scope and step 1 verify baseline before touching anything to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: consumer claude code, prerequisite session 0 fc checks all pass and luba approved, scope, step 1 verify baseline before touching anything.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

