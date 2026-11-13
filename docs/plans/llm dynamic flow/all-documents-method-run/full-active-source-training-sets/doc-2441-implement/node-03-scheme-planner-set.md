# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; skill 08 flow definition implementation guide is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, skill 08 flow definition implementation guide. Keep impact explanation and reprocess instruction separate. Allow approval boundary and skill 08 flow definition implementation guide to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, skill 08 flow definition implementation guide.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; skill 08 flow definition implementation guide is present and needs its own handling; prerequisites is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, skill 08 flow definition implementation guide, prerequisites. Keep reprocess instruction and approval boundary separate. Allow skill 08 flow definition implementation guide and prerequisites to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, skill 08 flow definition implementation guide, prerequisites.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: approval boundary is present and needs its own handling; skill 08 flow definition implementation guide is present and needs its own handling; prerequisites is present and needs its own handling; step 1 create models is present and needs its own handling.

B: Create work pieces for approval boundary, skill 08 flow definition implementation guide, prerequisites, step 1 create models. Keep approval boundary and skill 08 flow definition implementation guide separate. Allow prerequisites and step 1 create models to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, skill 08 flow definition implementation guide, prerequisites, step 1 create models.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: skill 08 flow definition implementation guide is present and needs its own handling; prerequisites is present and needs its own handling; step 1 create models is present and needs its own handling; step 2 implement flowvalidator is present and needs its own handling.

B: Create work pieces for skill 08 flow definition implementation guide, prerequisites, step 1 create models, step 2 implement flowvalidator. Keep skill 08 flow definition implementation guide and prerequisites separate. Allow step 1 create models and step 2 implement flowvalidator to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: skill 08 flow definition implementation guide, prerequisites, step 1 create models, step 2 implement flowvalidator.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Skill 08 — Flow Definition: Implementation Guide: prerequisites is present and needs its own handling; step 1 create models is present and needs its own handling; step 2 implement flowvalidator is present and needs its own handling; step 3 implement flowdefinitionservice crud is present and needs its own handling.

B: Create work pieces for prerequisites, step 1 create models, step 2 implement flowvalidator, step 3 implement flowdefinitionservice crud. Keep prerequisites and step 1 create models separate. Allow step 2 implement flowvalidator and step 3 implement flowdefinitionservice crud to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: prerequisites, step 1 create models, step 2 implement flowvalidator, step 3 implement flowdefinitionservice crud.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

