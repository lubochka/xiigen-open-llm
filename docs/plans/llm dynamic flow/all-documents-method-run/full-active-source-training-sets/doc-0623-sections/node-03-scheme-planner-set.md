# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Sections: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Sections: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Sections: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Sections: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; sections is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, sections. Keep impact explanation and reprocess instruction separate. Allow approval boundary and sections to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, sections.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Sections: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; sections is present and needs its own handling; output pattern recognition pattern new in v2 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, sections, output pattern recognition pattern new in v2. Keep reprocess instruction and approval boundary separate. Allow sections and output pattern recognition pattern new in v2 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, sections, output pattern recognition pattern new in v2.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Sections: approval boundary is present and needs its own handling; sections is present and needs its own handling; output pattern recognition pattern new in v2 is present and needs its own handling; cross boundary tracing boundary is present and needs its own handling.

B: Create work pieces for approval boundary, sections, output pattern recognition pattern new in v2, cross boundary tracing boundary. Keep approval boundary and sections separate. Allow output pattern recognition pattern new in v2 and cross boundary tracing boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, sections, output pattern recognition pattern new in v2, cross boundary tracing boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Sections: sections is present and needs its own handling; output pattern recognition pattern new in v2 is present and needs its own handling; cross boundary tracing boundary is present and needs its own handling; loop state analysis loop is present and needs its own handling.

B: Create work pieces for sections, output pattern recognition pattern new in v2, cross boundary tracing boundary, loop state analysis loop. Keep sections and output pattern recognition pattern new in v2 separate. Allow cross boundary tracing boundary and loop state analysis loop to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: sections, output pattern recognition pattern new in v2, cross boundary tracing boundary, loop state analysis loop.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Sections: output pattern recognition pattern new in v2 is present and needs its own handling; cross boundary tracing boundary is present and needs its own handling; loop state analysis loop is present and needs its own handling; contextual gap audit gap new in v2 is present and needs its own handling.

B: Create work pieces for output pattern recognition pattern new in v2, cross boundary tracing boundary, loop state analysis loop, contextual gap audit gap new in v2. Keep output pattern recognition pattern new in v2 and cross boundary tracing boundary separate. Allow loop state analysis loop and contextual gap audit gap new in v2 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: output pattern recognition pattern new in v2, cross boundary tracing boundary, loop state analysis loop, contextual gap audit gap new in v2.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

