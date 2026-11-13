# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; exports imports phase 1 exports phase 3 imports and calls it is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, exports imports phase 1 exports phase 3 imports and calls it. Keep configuration choice and dependency rule separate. Allow completion gate and exports imports phase 1 exports phase 3 imports and calls it to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, exports imports phase 1 exports phase 3 imports and calls it.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; exports imports phase 1 exports phase 3 imports and calls it is present and needs its own handling; apis consumers route exists something fetches from it is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, exports imports phase 1 exports phase 3 imports and calls it, apis consumers route exists something fetches from it. Keep dependency rule and completion gate separate. Allow exports imports phase 1 exports phase 3 imports and calls it and apis consumers route exists something fetches from it to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, exports imports phase 1 exports phase 3 imports and calls it, apis consumers route exists something fetches from it.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: completion gate is present and needs its own handling; exports imports phase 1 exports phase 3 imports and calls it is present and needs its own handling; apis consumers route exists something fetches from it is present and needs its own handling; data display database has data ui renders it is present and needs its own handling.

B: Create work pieces for completion gate, exports imports phase 1 exports phase 3 imports and calls it, apis consumers route exists something fetches from it, data display database has data ui renders it. Keep completion gate and exports imports phase 1 exports phase 3 imports and calls it separate. Allow apis consumers route exists something fetches from it and data display database has data ui renders it to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, exports imports phase 1 exports phase 3 imports and calls it, apis consumers route exists something fetches from it, data display database has data ui renders it.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: exports imports phase 1 exports phase 3 imports and calls it is present and needs its own handling; apis consumers route exists something fetches from it is present and needs its own handling; data display database has data ui renders it is present and needs its own handling; required context provided by milestone auditor is present and needs its own handling.

B: Create work pieces for exports imports phase 1 exports phase 3 imports and calls it, apis consumers route exists something fetches from it, data display database has data ui renders it, required context provided by milestone auditor. Keep exports imports phase 1 exports phase 3 imports and calls it and apis consumers route exists something fetches from it separate. Allow data display database has data ui renders it and required context provided by milestone auditor to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: exports imports phase 1 exports phase 3 imports and calls it, apis consumers route exists something fetches from it, data display database has data ui renders it, required context provided by milestone auditor.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Exports → Imports — Phase 1 exports , Phase 3 imports and calls it?: apis consumers route exists something fetches from it is present and needs its own handling; data display database has data ui renders it is present and needs its own handling; required context provided by milestone auditor is present and needs its own handling; step 1 build export import map is present and needs its own handling.

B: Create work pieces for apis consumers route exists something fetches from it, data display database has data ui renders it, required context provided by milestone auditor, step 1 build export import map. Keep apis consumers route exists something fetches from it and data display database has data ui renders it separate. Allow required context provided by milestone auditor and step 1 build export import map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: apis consumers route exists something fetches from it, data display database has data ui renders it, required context provided by milestone auditor, step 1 build export import map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

