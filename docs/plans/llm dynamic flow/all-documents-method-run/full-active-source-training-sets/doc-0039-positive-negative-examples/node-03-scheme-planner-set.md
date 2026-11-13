# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; what correct and incorrect outputs look like is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, what correct and incorrect outputs look like. Keep failure evidence and coverage decision separate. Allow release gate and what correct and incorrect outputs look like to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, what correct and incorrect outputs look like.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: coverage decision is present and needs its own handling; release gate is present and needs its own handling; what correct and incorrect outputs look like is present and needs its own handling; date 2026 03 19 is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, what correct and incorrect outputs look like, date 2026 03 19. Keep coverage decision and release gate separate. Allow what correct and incorrect outputs look like and date 2026 03 19 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, what correct and incorrect outputs look like, date 2026 03 19.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: release gate is present and needs its own handling; what correct and incorrect outputs look like is present and needs its own handling; date 2026 03 19 is present and needs its own handling; document purpose is present and needs its own handling.

B: Create work pieces for release gate, what correct and incorrect outputs look like, date 2026 03 19, document purpose. Keep release gate and what correct and incorrect outputs look like separate. Allow date 2026 03 19 and document purpose to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, what correct and incorrect outputs look like, date 2026 03 19, document purpose.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: what correct and incorrect outputs look like is present and needs its own handling; date 2026 03 19 is present and needs its own handling; document purpose is present and needs its own handling; benchmark run output is present and needs its own handling.

B: Create work pieces for what correct and incorrect outputs look like, date 2026 03 19, document purpose, benchmark run output. Keep what correct and incorrect outputs look like and date 2026 03 19 separate. Allow document purpose and benchmark run output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: what correct and incorrect outputs look like, date 2026 03 19, document purpose, benchmark run output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen the retrieval system Benchmark — Positive and Negative Examples: date 2026 03 19 is present and needs its own handling; document purpose is present and needs its own handling; benchmark run output is present and needs its own handling; positive example correct benchmark run result is present and needs its own handling.

B: Create work pieces for date 2026 03 19, document purpose, benchmark run output, positive example correct benchmark run result. Keep date 2026 03 19 and document purpose separate. Allow benchmark run output and positive example correct benchmark run result to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: date 2026 03 19, document purpose, benchmark run output, positive example correct benchmark run result.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

