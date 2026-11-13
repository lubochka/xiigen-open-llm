# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; count what executes not what asserts is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, count what executes not what asserts. Keep failure evidence and coverage decision separate. Allow release gate and count what executes not what asserts to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, count what executes not what asserts.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: coverage decision is present and needs its own handling; release gate is present and needs its own handling; count what executes not what asserts is present and needs its own handling; the audit procedure is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, count what executes not what asserts, the audit procedure. Keep coverage decision and release gate separate. Allow count what executes not what asserts and the audit procedure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, count what executes not what asserts, the audit procedure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: release gate is present and needs its own handling; count what executes not what asserts is present and needs its own handling; the audit procedure is present and needs its own handling; step 1 separate test changes into three categories is present and needs its own handling.

B: Create work pieces for release gate, count what executes not what asserts, the audit procedure, step 1 separate test changes into three categories. Keep release gate and count what executes not what asserts separate. Allow the audit procedure and step 1 separate test changes into three categories to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, count what executes not what asserts, the audit procedure, step 1 separate test changes into three categories.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: count what executes not what asserts is present and needs its own handling; the audit procedure is present and needs its own handling; step 1 separate test changes into three categories is present and needs its own handling; step 2 for each category c test identify the branch shift is present and needs its own handling.

B: Create work pieces for count what executes not what asserts, the audit procedure, step 1 separate test changes into three categories, step 2 for each category c test identify the branch shift. Keep count what executes not what asserts and the audit procedure separate. Allow step 1 separate test changes into three categories and step 2 for each category c test identify the branch shift to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: count what executes not what asserts, the audit procedure, step 1 separate test changes into three categories, step 2 for each category c test identify the branch shift.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Count What Executes, Not What Asserts: the audit procedure is present and needs its own handling; step 1 separate test changes into three categories is present and needs its own handling; step 2 for each category c test identify the branch shift is present and needs its own handling; step 3 build the branch coverage delta is present and needs its own handling.

B: Create work pieces for the audit procedure, step 1 separate test changes into three categories, step 2 for each category c test identify the branch shift, step 3 build the branch coverage delta. Keep the audit procedure and step 1 separate test changes into three categories separate. Allow step 2 for each category c test identify the branch shift and step 3 build the branch coverage delta to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the audit procedure, step 1 separate test changes into three categories, step 2 for each category c test identify the branch shift, step 3 build the branch coverage delta.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

