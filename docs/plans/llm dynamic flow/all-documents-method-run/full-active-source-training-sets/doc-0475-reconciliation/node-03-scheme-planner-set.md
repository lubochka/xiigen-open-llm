# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; figma to code v6 3 reconciliation audit is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, figma to code v6 3 reconciliation audit. Keep configuration choice and dependency rule separate. Allow completion gate and figma to code v6 3 reconciliation audit to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, figma to code v6 3 reconciliation audit.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; figma to code v6 3 reconciliation audit is present and needs its own handling; purpose is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, figma to code v6 3 reconciliation audit, purpose. Keep dependency rule and completion gate separate. Allow figma to code v6 3 reconciliation audit and purpose to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, figma to code v6 3 reconciliation audit, purpose.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: completion gate is present and needs its own handling; figma to code v6 3 reconciliation audit is present and needs its own handling; purpose is present and needs its own handling; fixes shipped impact on matrix docs is present and needs its own handling.

B: Create work pieces for completion gate, figma to code v6 3 reconciliation audit, purpose, fixes shipped impact on matrix docs. Keep completion gate and figma to code v6 3 reconciliation audit separate. Allow purpose and fixes shipped impact on matrix docs to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, figma to code v6 3 reconciliation audit, purpose, fixes shipped impact on matrix docs.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: figma to code v6 3 reconciliation audit is present and needs its own handling; purpose is present and needs its own handling; fixes shipped impact on matrix docs is present and needs its own handling; fixes reverted matrix must reflect original behavior is present and needs its own handling.

B: Create work pieces for figma to code v6 3 reconciliation audit, purpose, fixes shipped impact on matrix docs, fixes reverted matrix must reflect original behavior. Keep figma to code v6 3 reconciliation audit and purpose separate. Allow fixes shipped impact on matrix docs and fixes reverted matrix must reflect original behavior to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: figma to code v6 3 reconciliation audit, purpose, fixes shipped impact on matrix docs, fixes reverted matrix must reflect original behavior.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Reconciliation Audit: purpose is present and needs its own handling; fixes shipped impact on matrix docs is present and needs its own handling; fixes reverted matrix must reflect original behavior is present and needs its own handling; ai enhancement panel new in v6 3 not in matrix v5 is present and needs its own handling.

B: Create work pieces for purpose, fixes shipped impact on matrix docs, fixes reverted matrix must reflect original behavior, ai enhancement panel new in v6 3 not in matrix v5. Keep purpose and fixes shipped impact on matrix docs separate. Allow fixes reverted matrix must reflect original behavior and ai enhancement panel new in v6 3 not in matrix v5 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: purpose, fixes shipped impact on matrix docs, fixes reverted matrix must reflect original behavior, ai enhancement panel new in v6 3 not in matrix v5.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

