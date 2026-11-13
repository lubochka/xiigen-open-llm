# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; testing patterns template is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, testing patterns template. Keep failure evidence and coverage decision separate. Allow release gate and testing patterns template to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, testing patterns template.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: coverage decision is present and needs its own handling; release gate is present and needs its own handling; testing patterns template is present and needs its own handling; file template is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, testing patterns template, file template. Keep coverage decision and release gate separate. Allow testing patterns template and file template to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, testing patterns template, file template.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: release gate is present and needs its own handling; testing patterns template is present and needs its own handling; file template is present and needs its own handling; testing patterns is present and needs its own handling.

B: Create work pieces for release gate, testing patterns template, file template, testing patterns. Keep release gate and testing patterns template separate. Allow file template and testing patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, testing patterns template, file template, testing patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: testing patterns template is present and needs its own handling; file template is present and needs its own handling; testing patterns is present and needs its own handling; test framework is present and needs its own handling.

B: Create work pieces for testing patterns template, file template, testing patterns, test framework. Keep testing patterns template and file template separate. Allow testing patterns and test framework to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: testing patterns template, file template, testing patterns, test framework.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Testing Patterns Template: file template is present and needs its own handling; testing patterns is present and needs its own handling; test framework is present and needs its own handling; test file organization is present and needs its own handling.

B: Create work pieces for file template, testing patterns, test framework, test file organization. Keep file template and testing patterns separate. Allow test framework and test file organization to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: file template, testing patterns, test framework, test file organization.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

