# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; how to test whether the knowledge base actually works is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, how to test whether the knowledge base actually works. Keep failure evidence and coverage decision separate. Allow release gate and how to test whether the knowledge base actually works to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, how to test whether the knowledge base actually works.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: coverage decision is present and needs its own handling; release gate is present and needs its own handling; how to test whether the knowledge base actually works is present and needs its own handling; what the screenshots proved is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, how to test whether the knowledge base actually works, what the screenshots proved. Keep coverage decision and release gate separate. Allow how to test whether the knowledge base actually works and what the screenshots proved to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, how to test whether the knowledge base actually works, what the screenshots proved.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: release gate is present and needs its own handling; how to test whether the knowledge base actually works is present and needs its own handling; what the screenshots proved is present and needs its own handling; the rule for every test prompt is present and needs its own handling.

B: Create work pieces for release gate, how to test whether the knowledge base actually works, what the screenshots proved, the rule for every test prompt. Keep release gate and how to test whether the knowledge base actually works separate. Allow what the screenshots proved and the rule for every test prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, how to test whether the knowledge base actually works, what the screenshots proved, the rule for every test prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: how to test whether the knowledge base actually works is present and needs its own handling; what the screenshots proved is present and needs its own handling; the rule for every test prompt is present and needs its own handling; the 10 test prompts is present and needs its own handling.

B: Create work pieces for how to test whether the knowledge base actually works, what the screenshots proved, the rule for every test prompt, the 10 test prompts. Keep how to test whether the knowledge base actually works and what the screenshots proved separate. Allow the rule for every test prompt and the 10 test prompts to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: how to test whether the knowledge base actually works, what the screenshots proved, the rule for every test prompt, the 10 test prompts.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for How to Test Whether the Knowledge Base Actually Works: what the screenshots proved is present and needs its own handling; the rule for every test prompt is present and needs its own handling; the 10 test prompts is present and needs its own handling; how to score each prompt is present and needs its own handling.

B: Create work pieces for what the screenshots proved, the rule for every test prompt, the 10 test prompts, how to score each prompt. Keep what the screenshots proved and the rule for every test prompt separate. Allow the 10 test prompts and how to score each prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what the screenshots proved, the rule for every test prompt, the 10 test prompts, how to score each prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

