# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; supersedes design round 0 open questions resolved by luba is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, supersedes design round 0 open questions resolved by luba. Keep configuration choice and dependency rule separate. Allow completion gate and supersedes design round 0 open questions resolved by luba to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, supersedes design round 0 open questions resolved by luba.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; supersedes design round 0 open questions resolved by luba is present and needs its own handling; goal reminder a planning skill is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, supersedes design round 0 open questions resolved by luba, goal reminder a planning skill. Keep dependency rule and completion gate separate. Allow supersedes design round 0 open questions resolved by luba and goal reminder a planning skill to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, supersedes design round 0 open questions resolved by luba, goal reminder a planning skill.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: completion gate is present and needs its own handling; supersedes design round 0 open questions resolved by luba is present and needs its own handling; goal reminder a planning skill is present and needs its own handling; artifact boundaries is present and needs its own handling.

B: Create work pieces for completion gate, supersedes design round 0 open questions resolved by luba, goal reminder a planning skill, artifact boundaries. Keep completion gate and supersedes design round 0 open questions resolved by luba separate. Allow goal reminder a planning skill and artifact boundaries to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, supersedes design round 0 open questions resolved by luba, goal reminder a planning skill, artifact boundaries.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: supersedes design round 0 open questions resolved by luba is present and needs its own handling; goal reminder a planning skill is present and needs its own handling; artifact boundaries is present and needs its own handling; decision 1 react i18n library and bundle first loading g1 g6 is present and needs its own handling.

B: Create work pieces for supersedes design round 0 open questions resolved by luba, goal reminder a planning skill, artifact boundaries, decision 1 react i18n library and bundle first loading g1 g6. Keep supersedes design round 0 open questions resolved by luba and goal reminder a planning skill separate. Allow artifact boundaries and decision 1 react i18n library and bundle first loading g1 g6 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: supersedes design round 0 open questions resolved by luba, goal reminder a planning skill, artifact boundaries, decision 1 react i18n library and bundle first loading g1 g6.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The language translation flow — i18n-translation: Architectural Design R1: goal reminder a planning skill is present and needs its own handling; artifact boundaries is present and needs its own handling; decision 1 react i18n library and bundle first loading g1 g6 is present and needs its own handling; bundle first loading luba confirmed is present and needs its own handling.

B: Create work pieces for goal reminder a planning skill, artifact boundaries, decision 1 react i18n library and bundle first loading g1 g6, bundle first loading luba confirmed. Keep goal reminder a planning skill and artifact boundaries separate. Allow decision 1 react i18n library and bundle first loading g1 g6 and bundle first loading luba confirmed to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: goal reminder a planning skill, artifact boundaries, decision 1 react i18n library and bundle first loading g1 g6, bundle first loading luba confirmed.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

