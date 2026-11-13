# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; find existing test directories is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, find existing test directories. Keep failure evidence and coverage decision separate. Allow release gate and find existing test directories to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, find existing test directories.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: coverage decision is present and needs its own handling; release gate is present and needs its own handling; find existing test directories is present and needs its own handling; find existing test files for convention matching is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, find existing test directories, find existing test files for convention matching. Keep coverage decision and release gate separate. Allow find existing test directories and find existing test files for convention matching to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, find existing test directories, find existing test files for convention matching.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: release gate is present and needs its own handling; find existing test directories is present and needs its own handling; find existing test files for convention matching is present and needs its own handling; check for test runners is present and needs its own handling.

B: Create work pieces for release gate, find existing test directories, find existing test files for convention matching, check for test runners. Keep release gate and find existing test directories separate. Allow find existing test files for convention matching and check for test runners to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, find existing test directories, find existing test files for convention matching, check for test runners.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: find existing test directories is present and needs its own handling; find existing test files for convention matching is present and needs its own handling; check for test runners is present and needs its own handling; write test with clear arrange act assert structure is present and needs its own handling.

B: Create work pieces for find existing test directories, find existing test files for convention matching, check for test runners, write test with clear arrange act assert structure. Keep find existing test directories and find existing test files for convention matching separate. Allow check for test runners and write test with clear arrange act assert structure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: find existing test directories, find existing test files for convention matching, check for test runners, write test with clear arrange act assert structure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Find existing test directories: find existing test files for convention matching is present and needs its own handling; check for test runners is present and needs its own handling; write test with clear arrange act assert structure is present and needs its own handling; run the test is present and needs its own handling.

B: Create work pieces for find existing test files for convention matching, check for test runners, write test with clear arrange act assert structure, run the test. Keep find existing test files for convention matching and check for test runners separate. Allow write test with clear arrange act assert structure and run the test to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: find existing test files for convention matching, check for test runners, write test with clear arrange act assert structure, run the test.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

