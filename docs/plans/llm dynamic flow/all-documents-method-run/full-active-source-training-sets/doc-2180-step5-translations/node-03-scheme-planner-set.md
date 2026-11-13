# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 5 translate to alternative languages is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, step 5 translate to alternative languages. Keep impact explanation and reprocess instruction separate. Allow approval boundary and step 5 translate to alternative languages to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, step 5 translate to alternative languages.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 5 translate to alternative languages is present and needs its own handling; when to start translations is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, step 5 translate to alternative languages, when to start translations. Keep reprocess instruction and approval boundary separate. Allow step 5 translate to alternative languages and when to start translations to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, step 5 translate to alternative languages, when to start translations.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: approval boundary is present and needs its own handling; step 5 translate to alternative languages is present and needs its own handling; when to start translations is present and needs its own handling; the translation rules is present and needs its own handling.

B: Create work pieces for approval boundary, step 5 translate to alternative languages, when to start translations, the translation rules. Keep approval boundary and step 5 translate to alternative languages separate. Allow when to start translations and the translation rules to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, step 5 translate to alternative languages, when to start translations, the translation rules.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: step 5 translate to alternative languages is present and needs its own handling; when to start translations is present and needs its own handling; the translation rules is present and needs its own handling; rule 1 same contract different syntax is present and needs its own handling.

B: Create work pieces for step 5 translate to alternative languages, when to start translations, the translation rules, rule 1 same contract different syntax. Keep step 5 translate to alternative languages and when to start translations separate. Allow the translation rules and rule 1 same contract different syntax to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: step 5 translate to alternative languages, when to start translations, the translation rules, rule 1 same contract different syntax.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Step 5 · Translate to Alternative Languages: when to start translations is present and needs its own handling; the translation rules is present and needs its own handling; rule 1 same contract different syntax is present and needs its own handling; rule 2 same queue pattern is present and needs its own handling.

B: Create work pieces for when to start translations, the translation rules, rule 1 same contract different syntax, rule 2 same queue pattern. Keep when to start translations and the translation rules separate. Allow rule 1 same contract different syntax and rule 2 same queue pattern to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to start translations, the translation rules, rule 1 same contract different syntax, rule 2 same queue pattern.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

