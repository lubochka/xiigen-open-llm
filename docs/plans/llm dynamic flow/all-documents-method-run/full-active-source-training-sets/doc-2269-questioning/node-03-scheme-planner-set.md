# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; requirements needs clear enough vision to scope v1 features is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, requirements needs clear enough vision to scope v1 features. Keep impact explanation and reprocess instruction separate. Allow approval boundary and requirements needs clear enough vision to scope v1 features to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, requirements needs clear enough vision to scope v1 features.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; requirements needs clear enough vision to scope v1 features is present and needs its own handling; ask your follow up as plain text not via askuserquestion is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, requirements needs clear enough vision to scope v1 features, ask your follow up as plain text not via askuserquestion. Keep reprocess instruction and approval boundary separate. Allow requirements needs clear enough vision to scope v1 features and ask your follow up as plain text not via askuserquestion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, requirements needs clear enough vision to scope v1 features, ask your follow up as plain text not via askuserquestion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: approval boundary is present and needs its own handling; requirements needs clear enough vision to scope v1 features is present and needs its own handling; ask your follow up as plain text not via askuserquestion is present and needs its own handling; wait for them to type at the normal prompt is present and needs its own handling.

B: Create work pieces for approval boundary, requirements needs clear enough vision to scope v1 features, ask your follow up as plain text not via askuserquestion, wait for them to type at the normal prompt. Keep approval boundary and requirements needs clear enough vision to scope v1 features separate. Allow ask your follow up as plain text not via askuserquestion and wait for them to type at the normal prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, requirements needs clear enough vision to scope v1 features, ask your follow up as plain text not via askuserquestion, wait for them to type at the normal prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: requirements needs clear enough vision to scope v1 features is present and needs its own handling; ask your follow up as plain text not via askuserquestion is present and needs its own handling; wait for them to type at the normal prompt is present and needs its own handling; interrogation firing questions without building on answers is present and needs its own handling.

B: Create work pieces for requirements needs clear enough vision to scope v1 features, ask your follow up as plain text not via askuserquestion, wait for them to type at the normal prompt, interrogation firing questions without building on answers. Keep requirements needs clear enough vision to scope v1 features and ask your follow up as plain text not via askuserquestion separate. Allow wait for them to type at the normal prompt and interrogation firing questions without building on answers to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: requirements needs clear enough vision to scope v1 features, ask your follow up as plain text not via askuserquestion, wait for them to type at the normal prompt, interrogation firing questions without building on answers.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Research needs: what domain to research, what the user already knows, what unknowns exist: ask your follow up as plain text not via askuserquestion is present and needs its own handling; wait for them to type at the normal prompt is present and needs its own handling; interrogation firing questions without building on answers is present and needs its own handling; rushing minimizing questions to get to the work is present and needs its own handling.

B: Create work pieces for ask your follow up as plain text not via askuserquestion, wait for them to type at the normal prompt, interrogation firing questions without building on answers, rushing minimizing questions to get to the work. Keep ask your follow up as plain text not via askuserquestion and wait for them to type at the normal prompt separate. Allow interrogation firing questions without building on answers and rushing minimizing questions to get to the work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: ask your follow up as plain text not via askuserquestion, wait for them to type at the normal prompt, interrogation firing questions without building on answers, rushing minimizing questions to get to the work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

