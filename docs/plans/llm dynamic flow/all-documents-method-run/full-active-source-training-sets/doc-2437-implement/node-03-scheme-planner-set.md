# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; skill 06 ai providers implementation prompts is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, skill 06 ai providers implementation prompts. Keep configuration choice and dependency rule separate. Allow completion gate and skill 06 ai providers implementation prompts to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, skill 06 ai providers implementation prompts.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; skill 06 ai providers implementation prompts is present and needs its own handling; step 1 create base provider is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, skill 06 ai providers implementation prompts, step 1 create base provider. Keep dependency rule and completion gate separate. Allow skill 06 ai providers implementation prompts and step 1 create base provider to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, skill 06 ai providers implementation prompts, step 1 create base provider.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: completion gate is present and needs its own handling; skill 06 ai providers implementation prompts is present and needs its own handling; step 1 create base provider is present and needs its own handling; step 2 implement claude provider is present and needs its own handling.

B: Create work pieces for completion gate, skill 06 ai providers implementation prompts, step 1 create base provider, step 2 implement claude provider. Keep completion gate and skill 06 ai providers implementation prompts separate. Allow step 1 create base provider and step 2 implement claude provider to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, skill 06 ai providers implementation prompts, step 1 create base provider, step 2 implement claude provider.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: skill 06 ai providers implementation prompts is present and needs its own handling; step 1 create base provider is present and needs its own handling; step 2 implement claude provider is present and needs its own handling; step 3 implement openai provider is present and needs its own handling.

B: Create work pieces for skill 06 ai providers implementation prompts, step 1 create base provider, step 2 implement claude provider, step 3 implement openai provider. Keep skill 06 ai providers implementation prompts and step 1 create base provider separate. Allow step 2 implement claude provider and step 3 implement openai provider to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: skill 06 ai providers implementation prompts, step 1 create base provider, step 2 implement claude provider, step 3 implement openai provider.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Skill 06: AI Providers — Implementation Prompts: step 1 create base provider is present and needs its own handling; step 2 implement claude provider is present and needs its own handling; step 3 implement openai provider is present and needs its own handling; step 4 implement gemini provider is present and needs its own handling.

B: Create work pieces for step 1 create base provider, step 2 implement claude provider, step 3 implement openai provider, step 4 implement gemini provider. Keep step 1 create base provider and step 2 implement claude provider separate. Allow step 3 implement openai provider and step 4 implement gemini provider to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: step 1 create base provider, step 2 implement claude provider, step 3 implement openai provider, step 4 implement gemini provider.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

