# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: session intent is present and needs its own handling; creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling.

B: Create work pieces for session intent, creation evidence, decision trail, resume state. Keep session intent and creation evidence separate. Allow decision trail and resume state to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: session intent, creation evidence, decision trail, resume state.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling.

B: Create work pieces for creation evidence, decision trail, resume state, open work. Keep creation evidence and decision trail separate. Allow resume state and open work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: creation evidence, decision trail, resume state, open work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling.

B: Create work pieces for decision trail, resume state, open work, validation notes. Keep decision trail and resume state separate. Allow open work and validation notes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: decision trail, resume state, open work, validation notes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling; session 0 plan review gate is present and needs its own handling.

B: Create work pieces for resume state, open work, validation notes, session 0 plan review gate. Keep resume state and open work separate. Allow validation notes and session 0 plan review gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: resume state, open work, validation notes, session 0 plan review gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: open work is present and needs its own handling; validation notes is present and needs its own handling; session 0 plan review gate is present and needs its own handling; learning pipeline testing phases 14 17 is present and needs its own handling.

B: Create work pieces for open work, validation notes, session 0 plan review gate, learning pipeline testing phases 14 17. Keep open work and validation notes separate. Allow session 0 plan review gate and learning pipeline testing phases 14 17 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: open work, validation notes, session 0 plan review gate, learning pipeline testing phases 14 17.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: validation notes is present and needs its own handling; session 0 plan review gate is present and needs its own handling; learning pipeline testing phases 14 17 is present and needs its own handling; consumer claude code runs fc checks is present and needs its own handling.

B: Create work pieces for validation notes, session 0 plan review gate, learning pipeline testing phases 14 17, consumer claude code runs fc checks. Keep validation notes and session 0 plan review gate separate. Allow learning pipeline testing phases 14 17 and consumer claude code runs fc checks to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: validation notes, session 0 plan review gate, learning pipeline testing phases 14 17, consumer claude code runs fc checks.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: session 0 plan review gate is present and needs its own handling; learning pipeline testing phases 14 17 is present and needs its own handling; consumer claude code runs fc checks is present and needs its own handling; gate a fc checks 15 items is present and needs its own handling.

B: Create work pieces for session 0 plan review gate, learning pipeline testing phases 14 17, consumer claude code runs fc checks, gate a fc checks 15 items. Keep session 0 plan review gate and learning pipeline testing phases 14 17 separate. Allow consumer claude code runs fc checks and gate a fc checks 15 items to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: session 0 plan review gate, learning pipeline testing phases 14 17, consumer claude code runs fc checks, gate a fc checks 15 items.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for SESSION 0 — PLAN REVIEW GATE: learning pipeline testing phases 14 17 is present and needs its own handling; consumer claude code runs fc checks is present and needs its own handling; gate a fc checks 15 items is present and needs its own handling; flow check 1 count drift pass is present and needs its own handling.

B: Create work pieces for learning pipeline testing phases 14 17, consumer claude code runs fc checks, gate a fc checks 15 items, flow check 1 count drift pass. Keep learning pipeline testing phases 14 17 and consumer claude code runs fc checks separate. Allow gate a fc checks 15 items and flow check 1 count drift pass to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: learning pipeline testing phases 14 17, consumer claude code runs fc checks, gate a fc checks 15 items, flow check 1 count drift pass.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

