# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the principle is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, the principle. Keep configuration choice and dependency rule separate. Allow completion gate and the principle to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, the principle.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the principle is present and needs its own handling; experiment pair structure translated from dpo triple is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, the principle, experiment pair structure translated from dpo triple. Keep dependency rule and completion gate separate. Allow the principle and experiment pair structure translated from dpo triple to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, the principle, experiment pair structure translated from dpo triple.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: completion gate is present and needs its own handling; the principle is present and needs its own handling; experiment pair structure translated from dpo triple is present and needs its own handling; capturing experiment pairs in code is present and needs its own handling.

B: Create work pieces for completion gate, the principle, experiment pair structure translated from dpo triple, capturing experiment pairs in code. Keep completion gate and the principle separate. Allow experiment pair structure translated from dpo triple and capturing experiment pairs in code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, the principle, experiment pair structure translated from dpo triple, capturing experiment pairs in code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: the principle is present and needs its own handling; experiment pair structure translated from dpo triple is present and needs its own handling; capturing experiment pairs in code is present and needs its own handling; when experiment pairs are contaminated is present and needs its own handling.

B: Create work pieces for the principle, experiment pair structure translated from dpo triple, capturing experiment pairs in code, when experiment pairs are contaminated. Keep the principle and experiment pair structure translated from dpo triple separate. Allow capturing experiment pairs in code and when experiment pairs are contaminated to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the principle, experiment pair structure translated from dpo triple, capturing experiment pairs in code, when experiment pairs are contaminated.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Dpo-triples-to-experiment-pairs-SKILL.md v1.0 — Experiment training data translated from DPO triples: experiment pair structure translated from dpo triple is present and needs its own handling; capturing experiment pairs in code is present and needs its own handling; when experiment pairs are contaminated is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for experiment pair structure translated from dpo triple, capturing experiment pairs in code, when experiment pairs are contaminated, training example. Keep experiment pair structure translated from dpo triple and capturing experiment pairs in code separate. Allow when experiment pairs are contaminated and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: experiment pair structure translated from dpo triple, capturing experiment pairs in code, when experiment pairs are contaminated, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

