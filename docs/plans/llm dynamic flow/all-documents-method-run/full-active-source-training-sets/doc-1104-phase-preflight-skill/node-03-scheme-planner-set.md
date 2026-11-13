# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, when to invoke. Keep configuration choice and dependency rule separate. Allow completion gate and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; when to invoke is present and needs its own handling; default preflight checks run every session is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, when to invoke, default preflight checks run every session. Keep dependency rule and completion gate separate. Allow when to invoke and default preflight checks run every session to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, when to invoke, default preflight checks run every session.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: completion gate is present and needs its own handling; when to invoke is present and needs its own handling; default preflight checks run every session is present and needs its own handling; check 1 build is green is present and needs its own handling.

B: Create work pieces for completion gate, when to invoke, default preflight checks run every session, check 1 build is green. Keep completion gate and when to invoke separate. Allow default preflight checks run every session and check 1 build is green to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, when to invoke, default preflight checks run every session, check 1 build is green.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: when to invoke is present and needs its own handling; default preflight checks run every session is present and needs its own handling; check 1 build is green is present and needs its own handling; expected build succeeded 0 error s is present and needs its own handling.

B: Create work pieces for when to invoke, default preflight checks run every session, check 1 build is green, expected build succeeded 0 error s. Keep when to invoke and default preflight checks run every session separate. Allow check 1 build is green and expected build succeeded 0 error s to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: when to invoke, default preflight checks run every session, check 1 build is green, expected build succeeded 0 error s.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Phase-preflight-SKILL.md v1.0 — Verify all assumptions before first line of code: default preflight checks run every session is present and needs its own handling; check 1 build is green is present and needs its own handling; expected build succeeded 0 error s is present and needs its own handling; check 2 tests are green record baseline is present and needs its own handling.

B: Create work pieces for default preflight checks run every session, check 1 build is green, expected build succeeded 0 error s, check 2 tests are green record baseline. Keep default preflight checks run every session and check 1 build is green separate. Allow expected build succeeded 0 error s and check 2 tests are green record baseline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: default preflight checks run every session, check 1 build is green, expected build succeeded 0 error s, check 2 tests are green record baseline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

