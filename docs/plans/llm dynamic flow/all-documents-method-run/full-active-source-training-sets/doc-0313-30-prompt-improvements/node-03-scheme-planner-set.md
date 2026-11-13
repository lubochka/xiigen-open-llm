# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; core principle prompts are versioned assets policies not strings is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, core principle prompts are versioned assets policies not strings. Keep impact explanation and reprocess instruction separate. Allow approval boundary and core principle prompts are versioned assets policies not strings to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, core principle prompts are versioned assets policies not strings.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; core principle prompts are versioned assets policies not strings is present and needs its own handling; a prompt assets immutable versions is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, core principle prompts are versioned assets policies not strings, a prompt assets immutable versions. Keep reprocess instruction and approval boundary separate. Allow core principle prompts are versioned assets policies not strings and a prompt assets immutable versions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, core principle prompts are versioned assets policies not strings, a prompt assets immutable versions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: approval boundary is present and needs its own handling; core principle prompts are versioned assets policies not strings is present and needs its own handling; a prompt assets immutable versions is present and needs its own handling; prompttemplate is present and needs its own handling.

B: Create work pieces for approval boundary, core principle prompts are versioned assets policies not strings, a prompt assets immutable versions, prompttemplate. Keep approval boundary and core principle prompts are versioned assets policies not strings separate. Allow a prompt assets immutable versions and prompttemplate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, core principle prompts are versioned assets policies not strings, a prompt assets immutable versions, prompttemplate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: core principle prompts are versioned assets policies not strings is present and needs its own handling; a prompt assets immutable versions is present and needs its own handling; prompttemplate is present and needs its own handling; promptversion is present and needs its own handling.

B: Create work pieces for core principle prompts are versioned assets policies not strings, a prompt assets immutable versions, prompttemplate, promptversion. Keep core principle prompts are versioned assets policies not strings and a prompt assets immutable versions separate. Allow prompttemplate and promptversion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: core principle prompts are versioned assets policies not strings, a prompt assets immutable versions, prompttemplate, promptversion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Core principle: prompts are versioned assets + policies, not strings: a prompt assets immutable versions is present and needs its own handling; prompttemplate is present and needs its own handling; promptversion is present and needs its own handling; promptpatch the discussion outcome is present and needs its own handling.

B: Create work pieces for a prompt assets immutable versions, prompttemplate, promptversion, promptpatch the discussion outcome. Keep a prompt assets immutable versions and prompttemplate separate. Allow promptversion and promptpatch the discussion outcome to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: a prompt assets immutable versions, prompttemplate, promptversion, promptpatch the discussion outcome.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

