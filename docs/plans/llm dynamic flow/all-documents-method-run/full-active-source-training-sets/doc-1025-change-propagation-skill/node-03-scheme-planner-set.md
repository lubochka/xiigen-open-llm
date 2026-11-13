# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the prior question is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, the prior question. Keep configuration choice and dependency rule separate. Allow completion gate and the prior question to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, the prior question.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the prior question is present and needs its own handling; dependency map for llmfromscratch is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, the prior question, dependency map for llmfromscratch. Keep dependency rule and completion gate separate. Allow the prior question and dependency map for llmfromscratch to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, the prior question, dependency map for llmfromscratch.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: completion gate is present and needs its own handling; the prior question is present and needs its own handling; dependency map for llmfromscratch is present and needs its own handling; core changes highest blast radius is present and needs its own handling.

B: Create work pieces for completion gate, the prior question, dependency map for llmfromscratch, core changes highest blast radius. Keep completion gate and the prior question separate. Allow dependency map for llmfromscratch and core changes highest blast radius to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, the prior question, dependency map for llmfromscratch, core changes highest blast radius.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: the prior question is present and needs its own handling; dependency map for llmfromscratch is present and needs its own handling; core changes highest blast radius is present and needs its own handling; algorithm changes is present and needs its own handling.

B: Create work pieces for the prior question, dependency map for llmfromscratch, core changes highest blast radius, algorithm changes. Keep the prior question and dependency map for llmfromscratch separate. Allow core changes highest blast radius and algorithm changes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the prior question, dependency map for llmfromscratch, core changes highest blast radius, algorithm changes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Change-propagation-SKILL.md v1.0 — What will be stale the moment this change is made?: dependency map for llmfromscratch is present and needs its own handling; core changes highest blast radius is present and needs its own handling; algorithm changes is present and needs its own handling; desktop changes is present and needs its own handling.

B: Create work pieces for dependency map for llmfromscratch, core changes highest blast radius, algorithm changes, desktop changes. Keep dependency map for llmfromscratch and core changes highest blast radius separate. Allow algorithm changes and desktop changes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: dependency map for llmfromscratch, core changes highest blast radius, algorithm changes, desktop changes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

