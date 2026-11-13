# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; step 4 generate skills in the primary language is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, step 4 generate skills in the primary language. Keep configuration choice and dependency rule separate. Allow completion gate and step 4 generate skills in the primary language to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, step 4 generate skills in the primary language.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; step 4 generate skills in the primary language is present and needs its own handling; the copy adapt rewrite classification is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, step 4 generate skills in the primary language, the copy adapt rewrite classification. Keep dependency rule and completion gate separate. Allow step 4 generate skills in the primary language and the copy adapt rewrite classification to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, step 4 generate skills in the primary language, the copy adapt rewrite classification.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: completion gate is present and needs its own handling; step 4 generate skills in the primary language is present and needs its own handling; the copy adapt rewrite classification is present and needs its own handling; how to classify a file is present and needs its own handling.

B: Create work pieces for completion gate, step 4 generate skills in the primary language, the copy adapt rewrite classification, how to classify a file. Keep completion gate and step 4 generate skills in the primary language separate. Allow the copy adapt rewrite classification and how to classify a file to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, step 4 generate skills in the primary language, the copy adapt rewrite classification, how to classify a file.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: step 4 generate skills in the primary language is present and needs its own handling; the copy adapt rewrite classification is present and needs its own handling; how to classify a file is present and needs its own handling; real example from xiigen is present and needs its own handling.

B: Create work pieces for step 4 generate skills in the primary language, the copy adapt rewrite classification, how to classify a file, real example from xiigen. Keep step 4 generate skills in the primary language and the copy adapt rewrite classification separate. Allow how to classify a file and real example from xiigen to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: step 4 generate skills in the primary language, the copy adapt rewrite classification, how to classify a file, real example from xiigen.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Step 4 · Generate Skills in the Primary Language: the copy adapt rewrite classification is present and needs its own handling; how to classify a file is present and needs its own handling; real example from xiigen is present and needs its own handling; the skill file structure is present and needs its own handling.

B: Create work pieces for the copy adapt rewrite classification, how to classify a file, real example from xiigen, the skill file structure. Keep the copy adapt rewrite classification and how to classify a file separate. Allow real example from xiigen and the skill file structure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the copy adapt rewrite classification, how to classify a file, real example from xiigen, the skill file structure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

