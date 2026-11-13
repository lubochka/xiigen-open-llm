# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; diagnose before touching the test is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, diagnose before touching the test. Keep failure evidence and coverage decision separate. Allow release gate and diagnose before touching the test to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, diagnose before touching the test.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: coverage decision is present and needs its own handling; release gate is present and needs its own handling; diagnose before touching the test is present and needs its own handling; the audit procedure is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, diagnose before touching the test, the audit procedure. Keep coverage decision and release gate separate. Allow diagnose before touching the test and the audit procedure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, diagnose before touching the test, the audit procedure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: release gate is present and needs its own handling; diagnose before touching the test is present and needs its own handling; the audit procedure is present and needs its own handling; step 1 name the branch the test is supposed to exercise is present and needs its own handling.

B: Create work pieces for release gate, diagnose before touching the test, the audit procedure, step 1 name the branch the test is supposed to exercise. Keep release gate and diagnose before touching the test separate. Allow the audit procedure and step 1 name the branch the test is supposed to exercise to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, diagnose before touching the test, the audit procedure, step 1 name the branch the test is supposed to exercise.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: diagnose before touching the test is present and needs its own handling; the audit procedure is present and needs its own handling; step 1 name the branch the test is supposed to exercise is present and needs its own handling; step 3 check whether the proposed test fixture passes all guards is present and needs its own handling.

B: Create work pieces for diagnose before touching the test, the audit procedure, step 1 name the branch the test is supposed to exercise, step 3 check whether the proposed test fixture passes all guards. Keep diagnose before touching the test and the audit procedure separate. Allow step 1 name the branch the test is supposed to exercise and step 3 check whether the proposed test fixture passes all guards to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: diagnose before touching the test, the audit procedure, step 1 name the branch the test is supposed to exercise, step 3 check whether the proposed test fixture passes all guards.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Diagnose Before Touching the Test: the audit procedure is present and needs its own handling; step 1 name the branch the test is supposed to exercise is present and needs its own handling; step 3 check whether the proposed test fixture passes all guards is present and needs its own handling; worked example two col grid dead branch is present and needs its own handling.

B: Create work pieces for the audit procedure, step 1 name the branch the test is supposed to exercise, step 3 check whether the proposed test fixture passes all guards, worked example two col grid dead branch. Keep the audit procedure and step 1 name the branch the test is supposed to exercise separate. Allow step 3 check whether the proposed test fixture passes all guards and worked example two col grid dead branch to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the audit procedure, step 1 name the branch the test is supposed to exercise, step 3 check whether the proposed test fixture passes all guards, worked example two col grid dead branch.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

