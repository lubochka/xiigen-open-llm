# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; phase prompt template is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, phase prompt template. Keep configuration choice and dependency rule separate. Allow completion gate and phase prompt template to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, phase prompt template.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; phase prompt template is present and needs its own handling; file template is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, phase prompt template, file template. Keep dependency rule and completion gate separate. Allow phase prompt template and file template to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, phase prompt template, file template.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: completion gate is present and needs its own handling; phase prompt template is present and needs its own handling; file template is present and needs its own handling; only reference prior plan summarys if genuinely needed is present and needs its own handling.

B: Create work pieces for completion gate, phase prompt template, file template, only reference prior plan summarys if genuinely needed. Keep completion gate and phase prompt template separate. Allow file template and only reference prior plan summarys if genuinely needed to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, phase prompt template, file template, only reference prior plan summarys if genuinely needed.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: phase prompt template is present and needs its own handling; file template is present and needs its own handling; only reference prior plan summarys if genuinely needed is present and needs its own handling; this plan uses types exports from prior plan is present and needs its own handling.

B: Create work pieces for phase prompt template, file template, only reference prior plan summarys if genuinely needed, this plan uses types exports from prior plan. Keep phase prompt template and file template separate. Allow only reference prior plan summarys if genuinely needed and this plan uses types exports from prior plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: phase prompt template, file template, only reference prior plan summarys if genuinely needed, this plan uses types exports from prior plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Phase Prompt Template: file template is present and needs its own handling; only reference prior plan summarys if genuinely needed is present and needs its own handling; this plan uses types exports from prior plan is present and needs its own handling; prior plan made decision that affects this plan is present and needs its own handling.

B: Create work pieces for file template, only reference prior plan summarys if genuinely needed, this plan uses types exports from prior plan, prior plan made decision that affects this plan. Keep file template and only reference prior plan summarys if genuinely needed separate. Allow this plan uses types exports from prior plan and prior plan made decision that affects this plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: file template, only reference prior plan summarys if genuinely needed, this plan uses types exports from prior plan, prior plan made decision that affects this plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

