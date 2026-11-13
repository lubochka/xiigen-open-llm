# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Executive summary: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Executive summary: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Executive summary: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Executive summary: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; executive summary is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, executive summary. Keep impact explanation and reprocess instruction separate. Allow approval boundary and executive summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, executive summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Executive summary: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; executive summary is present and needs its own handling; the additional delivery documentation layer you need is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, executive summary, the additional delivery documentation layer you need. Keep reprocess instruction and approval boundary separate. Allow executive summary and the additional delivery documentation layer you need to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, executive summary, the additional delivery documentation layer you need.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Executive summary: approval boundary is present and needs its own handling; executive summary is present and needs its own handling; the additional delivery documentation layer you need is present and needs its own handling; a minimal run evidence pack contract is present and needs its own handling.

B: Create work pieces for approval boundary, executive summary, the additional delivery documentation layer you need, a minimal run evidence pack contract. Keep approval boundary and executive summary separate. Allow the additional delivery documentation layer you need and a minimal run evidence pack contract to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, executive summary, the additional delivery documentation layer you need, a minimal run evidence pack contract.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Executive summary: executive summary is present and needs its own handling; the additional delivery documentation layer you need is present and needs its own handling; a minimal run evidence pack contract is present and needs its own handling; where evidence should appear visually is present and needs its own handling.

B: Create work pieces for executive summary, the additional delivery documentation layer you need, a minimal run evidence pack contract, where evidence should appear visually. Keep executive summary and the additional delivery documentation layer you need separate. Allow a minimal run evidence pack contract and where evidence should appear visually to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: executive summary, the additional delivery documentation layer you need, a minimal run evidence pack contract, where evidence should appear visually.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Executive summary: the additional delivery documentation layer you need is present and needs its own handling; a minimal run evidence pack contract is present and needs its own handling; where evidence should appear visually is present and needs its own handling; graph must encode both connectivity and reachability is present and needs its own handling.

B: Create work pieces for the additional delivery documentation layer you need, a minimal run evidence pack contract, where evidence should appear visually, graph must encode both connectivity and reachability. Keep the additional delivery documentation layer you need and a minimal run evidence pack contract separate. Allow where evidence should appear visually and graph must encode both connectivity and reachability to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the additional delivery documentation layer you need, a minimal run evidence pack contract, where evidence should appear visually, graph must encode both connectivity and reachability.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

