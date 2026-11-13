# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; when to load is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, when to load. Keep failure evidence and coverage decision separate. Allow release gate and when to load to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, when to load.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: coverage decision is present and needs its own handling; release gate is present and needs its own handling; when to load is present and needs its own handling; failure modes is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, when to load, failure modes. Keep coverage decision and release gate separate. Allow when to load and failure modes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, when to load, failure modes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: release gate is present and needs its own handling; when to load is present and needs its own handling; failure modes is present and needs its own handling; rule stub fix or algorithm fix is present and needs its own handling.

B: Create work pieces for release gate, when to load, failure modes, rule stub fix or algorithm fix. Keep release gate and when to load separate. Allow failure modes and rule stub fix or algorithm fix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, when to load, failure modes, rule stub fix or algorithm fix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: when to load is present and needs its own handling; failure modes is present and needs its own handling; rule stub fix or algorithm fix is present and needs its own handling; rule sibling guard parity is present and needs its own handling.

B: Create work pieces for when to load, failure modes, rule stub fix or algorithm fix, rule sibling guard parity. Keep when to load and failure modes separate. Allow rule stub fix or algorithm fix and rule sibling guard parity to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: when to load, failure modes, rule stub fix or algorithm fix, rule sibling guard parity.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Test-integrity-SKILL.md v1.0 — A passing test is not proof the branch ran: failure modes is present and needs its own handling; rule stub fix or algorithm fix is present and needs its own handling; rule sibling guard parity is present and needs its own handling; rule real integration at least one component is real is present and needs its own handling.

B: Create work pieces for failure modes, rule stub fix or algorithm fix, rule sibling guard parity, rule real integration at least one component is real. Keep failure modes and rule stub fix or algorithm fix separate. Allow rule sibling guard parity and rule real integration at least one component is real to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: failure modes, rule stub fix or algorithm fix, rule sibling guard parity, rule real integration at least one component is real.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

