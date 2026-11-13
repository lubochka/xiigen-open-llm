# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: session intent is present and needs its own handling; creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling.

B: Create work pieces for session intent, creation evidence, decision trail, resume state. Keep session intent and creation evidence separate. Allow decision trail and resume state to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: session intent, creation evidence, decision trail, resume state.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling.

B: Create work pieces for creation evidence, decision trail, resume state, open work. Keep creation evidence and decision trail separate. Allow resume state and open work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: creation evidence, decision trail, resume state, open work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling.

B: Create work pieces for decision trail, resume state, open work, validation notes. Keep decision trail and resume state separate. Allow open work and validation notes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: decision trail, resume state, open work, validation notes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling; session 0 plan review fc checks is present and needs its own handling.

B: Create work pieces for resume state, open work, validation notes, session 0 plan review fc checks. Keep resume state and open work separate. Allow validation notes and session 0 plan review fc checks to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: resume state, open work, validation notes, session 0 plan review fc checks.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: open work is present and needs its own handling; validation notes is present and needs its own handling; session 0 plan review fc checks is present and needs its own handling; consumer claude code is present and needs its own handling.

B: Create work pieces for open work, validation notes, session 0 plan review fc checks, consumer claude code. Keep open work and validation notes separate. Allow session 0 plan review fc checks and consumer claude code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: open work, validation notes, session 0 plan review fc checks, consumer claude code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: validation notes is present and needs its own handling; session 0 plan review fc checks is present and needs its own handling; consumer claude code is present and needs its own handling; critical run live counts first do not trust numbers in state json is present and needs its own handling.

B: Create work pieces for validation notes, session 0 plan review fc checks, consumer claude code, critical run live counts first do not trust numbers in state json. Keep validation notes and session 0 plan review fc checks separate. Allow consumer claude code and critical run live counts first do not trust numbers in state json to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: validation notes, session 0 plan review fc checks, consumer claude code, critical run live counts first do not trust numbers in state json.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: session 0 plan review fc checks is present and needs its own handling; consumer claude code is present and needs its own handling; critical run live counts first do not trust numbers in state json is present and needs its own handling; first read live counts before any fc check is present and needs its own handling.

B: Create work pieces for session 0 plan review fc checks, consumer claude code, critical run live counts first do not trust numbers in state json, first read live counts before any fc check. Keep session 0 plan review fc checks and consumer claude code separate. Allow critical run live counts first do not trust numbers in state json and first read live counts before any fc check to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: session 0 plan review fc checks, consumer claude code, critical run live counts first do not trust numbers in state json, first read live counts before any fc check.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Session 0 — PLAN REVIEW FC Checks: consumer claude code is present and needs its own handling; critical run live counts first do not trust numbers in state json is present and needs its own handling; first read live counts before any fc check is present and needs its own handling; record as server spec files is present and needs its own handling.

B: Create work pieces for consumer claude code, critical run live counts first do not trust numbers in state json, first read live counts before any fc check, record as server spec files. Keep consumer claude code and critical run live counts first do not trust numbers in state json separate. Allow first read live counts before any fc check and record as server spec files to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: consumer claude code, critical run live counts first do not trust numbers in state json, first read live counts before any fc check, record as server spec files.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

