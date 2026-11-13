# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; the iron rule is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, the iron rule. Keep failure evidence and coverage decision separate. Allow release gate and the iron rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, the iron rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: coverage decision is present and needs its own handling; release gate is present and needs its own handling; the iron rule is present and needs its own handling; step 1 assign identifier is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, the iron rule, step 1 assign identifier. Keep coverage decision and release gate separate. Allow the iron rule and step 1 assign identifier to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, the iron rule, step 1 assign identifier.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: release gate is present and needs its own handling; the iron rule is present and needs its own handling; step 1 assign identifier is present and needs its own handling; step 2 write 3 failing tests before any fix is present and needs its own handling.

B: Create work pieces for release gate, the iron rule, step 1 assign identifier, step 2 write 3 failing tests before any fix. Keep release gate and the iron rule separate. Allow step 1 assign identifier and step 2 write 3 failing tests before any fix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, the iron rule, step 1 assign identifier, step 2 write 3 failing tests before any fix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: the iron rule is present and needs its own handling; step 1 assign identifier is present and needs its own handling; step 2 write 3 failing tests before any fix is present and needs its own handling; confirm all 3 fail before fixing is present and needs its own handling.

B: Create work pieces for the iron rule, step 1 assign identifier, step 2 write 3 failing tests before any fix, confirm all 3 fail before fixing. Keep the iron rule and step 1 assign identifier separate. Allow step 2 write 3 failing tests before any fix and confirm all 3 fail before fixing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the iron rule, step 1 assign identifier, step 2 write 3 failing tests before any fix, confirm all 3 fail before fixing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Bug-to-tests-SKILL.md v1.0 — Write 3 failing tests BEFORE fixing anything: step 1 assign identifier is present and needs its own handling; step 2 write 3 failing tests before any fix is present and needs its own handling; confirm all 3 fail before fixing is present and needs its own handling; expected failed 3 is present and needs its own handling.

B: Create work pieces for step 1 assign identifier, step 2 write 3 failing tests before any fix, confirm all 3 fail before fixing, expected failed 3. Keep step 1 assign identifier and step 2 write 3 failing tests before any fix separate. Allow confirm all 3 fail before fixing and expected failed 3 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: step 1 assign identifier, step 2 write 3 failing tests before any fix, confirm all 3 fail before fixing, expected failed 3.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

