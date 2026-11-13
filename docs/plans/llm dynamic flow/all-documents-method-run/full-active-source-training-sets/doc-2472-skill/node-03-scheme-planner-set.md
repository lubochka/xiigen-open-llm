# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; skill 21 permissions service is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, skill 21 permissions service. Keep configuration choice and dependency rule separate. Allow completion gate and skill 21 permissions service to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, skill 21 permissions service.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; skill 21 permissions service is present and needs its own handling; the critical role scope provider is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, skill 21 permissions service, the critical role scope provider. Keep dependency rule and completion gate separate. Allow skill 21 permissions service and the critical role scope provider to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, skill 21 permissions service, the critical role scope provider.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: completion gate is present and needs its own handling; skill 21 permissions service is present and needs its own handling; the critical role scope provider is present and needs its own handling; replication pattern from architecture docx is present and needs its own handling.

B: Create work pieces for completion gate, skill 21 permissions service, the critical role scope provider, replication pattern from architecture docx. Keep completion gate and skill 21 permissions service separate. Allow the critical role scope provider and replication pattern from architecture docx to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, skill 21 permissions service, the critical role scope provider, replication pattern from architecture docx.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: skill 21 permissions service is present and needs its own handling; the critical role scope provider is present and needs its own handling; replication pattern from architecture docx is present and needs its own handling; genie dna integration is present and needs its own handling.

B: Create work pieces for skill 21 permissions service, the critical role scope provider, replication pattern from architecture docx, genie dna integration. Keep skill 21 permissions service and the critical role scope provider separate. Allow replication pattern from architecture docx and genie dna integration to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: skill 21 permissions service, the critical role scope provider, replication pattern from architecture docx, genie dna integration.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Skill 21: Permissions Service: the critical role scope provider is present and needs its own handling; replication pattern from architecture docx is present and needs its own handling; genie dna integration is present and needs its own handling; role definitions as dynamic documents is present and needs its own handling.

B: Create work pieces for the critical role scope provider, replication pattern from architecture docx, genie dna integration, role definitions as dynamic documents. Keep the critical role scope provider and replication pattern from architecture docx separate. Allow genie dna integration and role definitions as dynamic documents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the critical role scope provider, replication pattern from architecture docx, genie dna integration, role definitions as dynamic documents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

