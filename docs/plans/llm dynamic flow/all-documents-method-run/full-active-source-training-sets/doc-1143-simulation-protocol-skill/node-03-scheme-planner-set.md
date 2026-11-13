# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; the principle translated from planning simulation protocol is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, the principle translated from planning simulation protocol. Keep failure evidence and coverage decision separate. Allow release gate and the principle translated from planning simulation protocol to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, the principle translated from planning simulation protocol.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: coverage decision is present and needs its own handling; release gate is present and needs its own handling; the principle translated from planning simulation protocol is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, the principle translated from planning simulation protocol, when to invoke. Keep coverage decision and release gate separate. Allow the principle translated from planning simulation protocol and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, the principle translated from planning simulation protocol, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: release gate is present and needs its own handling; the principle translated from planning simulation protocol is present and needs its own handling; when to invoke is present and needs its own handling; the step table format is present and needs its own handling.

B: Create work pieces for release gate, the principle translated from planning simulation protocol, when to invoke, the step table format. Keep release gate and the principle translated from planning simulation protocol separate. Allow when to invoke and the step table format to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, the principle translated from planning simulation protocol, when to invoke, the step table format.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: the principle translated from planning simulation protocol is present and needs its own handling; when to invoke is present and needs its own handling; the step table format is present and needs its own handling; simulation semanticcompilerpipeline with bug domain input is present and needs its own handling.

B: Create work pieces for the principle translated from planning simulation protocol, when to invoke, the step table format, simulation semanticcompilerpipeline with bug domain input. Keep the principle translated from planning simulation protocol and when to invoke separate. Allow the step table format and simulation semanticcompilerpipeline with bug domain input to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the principle translated from planning simulation protocol, when to invoke, the step table format, simulation semanticcompilerpipeline with bug domain input.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Simulation-protocol-SKILL.md v1.0 — Trace what actually happens before claiming it works: when to invoke is present and needs its own handling; the step table format is present and needs its own handling; simulation semanticcompilerpipeline with bug domain input is present and needs its own handling; break condition input is missing is present and needs its own handling.

B: Create work pieces for when to invoke, the step table format, simulation semanticcompilerpipeline with bug domain input, break condition input is missing. Keep when to invoke and the step table format separate. Allow simulation semanticcompilerpipeline with bug domain input and break condition input is missing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to invoke, the step table format, simulation semanticcompilerpipeline with bug domain input, break condition input is missing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

