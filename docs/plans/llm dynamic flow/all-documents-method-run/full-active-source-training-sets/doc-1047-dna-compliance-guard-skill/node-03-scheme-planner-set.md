# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; when to load is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, when to load. Keep impact explanation and reprocess instruction separate. Allow approval boundary and when to load to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, when to load.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; when to load is present and needs its own handling; pre commit gate run on every modified cs file is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, when to load, pre commit gate run on every modified cs file. Keep reprocess instruction and approval boundary separate. Allow when to load and pre commit gate run on every modified cs file to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, when to load, pre commit gate run on every modified cs file.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: approval boundary is present and needs its own handling; when to load is present and needs its own handling; pre commit gate run on every modified cs file is present and needs its own handling; rule 1 every public method returns operationresult t is present and needs its own handling.

B: Create work pieces for approval boundary, when to load, pre commit gate run on every modified cs file, rule 1 every public method returns operationresult t. Keep approval boundary and when to load separate. Allow pre commit gate run on every modified cs file and rule 1 every public method returns operationresult t to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, when to load, pre commit gate run on every modified cs file, rule 1 every public method returns operationresult t.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: when to load is present and needs its own handling; pre commit gate run on every modified cs file is present and needs its own handling; rule 1 every public method returns operationresult t is present and needs its own handling; rule 2 no concrete instantiation in production code is present and needs its own handling.

B: Create work pieces for when to load, pre commit gate run on every modified cs file, rule 1 every public method returns operationresult t, rule 2 no concrete instantiation in production code. Keep when to load and pre commit gate run on every modified cs file separate. Allow rule 1 every public method returns operationresult t and rule 2 no concrete instantiation in production code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: when to load, pre commit gate run on every modified cs file, rule 1 every public method returns operationresult t, rule 2 no concrete instantiation in production code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Dna-compliance-guard-SKILL.md v1.0 — Pre-commit gate for all .cs files: pre commit gate run on every modified cs file is present and needs its own handling; rule 1 every public method returns operationresult t is present and needs its own handling; rule 2 no concrete instantiation in production code is present and needs its own handling; rule 10 no external ml frameworks is present and needs its own handling.

B: Create work pieces for pre commit gate run on every modified cs file, rule 1 every public method returns operationresult t, rule 2 no concrete instantiation in production code, rule 10 no external ml frameworks. Keep pre commit gate run on every modified cs file and rule 1 every public method returns operationresult t separate. Allow rule 2 no concrete instantiation in production code and rule 10 no external ml frameworks to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: pre commit gate run on every modified cs file, rule 1 every public method returns operationresult t, rule 2 no concrete instantiation in production code, rule 10 no external ml frameworks.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

