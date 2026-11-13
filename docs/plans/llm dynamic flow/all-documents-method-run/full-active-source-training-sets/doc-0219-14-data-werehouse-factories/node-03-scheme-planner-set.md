# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; merged 2026 02 26 source flow14 plan p0 md phase p1 is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, merged 2026 02 26 source flow14 plan p0 md phase p1. Keep configuration choice and dependency rule separate. Allow completion gate and merged 2026 02 26 source flow14 plan p0 md phase p1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, merged 2026 02 26 source flow14 plan p0 md phase p1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; merged 2026 02 26 source flow14 plan p0 md phase p1 is present and needs its own handling; prerequisite state entering the notifications flow p1 is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, merged 2026 02 26 source flow14 plan p0 md phase p1, prerequisite state entering the notifications flow p1. Keep dependency rule and completion gate separate. Allow merged 2026 02 26 source flow14 plan p0 md phase p1 and prerequisite state entering the notifications flow p1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, merged 2026 02 26 source flow14 plan p0 md phase p1, prerequisite state entering the notifications flow p1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: completion gate is present and needs its own handling; merged 2026 02 26 source flow14 plan p0 md phase p1 is present and needs its own handling; prerequisite state entering the notifications flow p1 is present and needs its own handling; design records design record 58 design record 59 is present and needs its own handling.

B: Create work pieces for completion gate, merged 2026 02 26 source flow14 plan p0 md phase p1, prerequisite state entering the notifications flow p1, design records design record 58 design record 59. Keep completion gate and merged 2026 02 26 source flow14 plan p0 md phase p1 separate. Allow prerequisite state entering the notifications flow p1 and design records design record 58 design record 59 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, merged 2026 02 26 source flow14 plan p0 md phase p1, prerequisite state entering the notifications flow p1, design records design record 58 design record 59.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: merged 2026 02 26 source flow14 plan p0 md phase p1 is present and needs its own handling; prerequisite state entering the notifications flow p1 is present and needs its own handling; design records design record 58 design record 59 is present and needs its own handling; design record 58 immutable raw zone bronze layer is present and needs its own handling.

B: Create work pieces for merged 2026 02 26 source flow14 plan p0 md phase p1, prerequisite state entering the notifications flow p1, design records design record 58 design record 59, design record 58 immutable raw zone bronze layer. Keep merged 2026 02 26 source flow14 plan p0 md phase p1 and prerequisite state entering the notifications flow p1 separate. Allow design records design record 58 design record 59 and design record 58 immutable raw zone bronze layer to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: merged 2026 02 26 source flow14 plan p0 md phase p1, prerequisite state entering the notifications flow p1, design records design record 58 design record 59, design record 58 immutable raw zone bronze layer.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — the notifications flow PHASE 1: CONNECTION + INGESTION + RAW ZONE: prerequisite state entering the notifications flow p1 is present and needs its own handling; design records design record 58 design record 59 is present and needs its own handling; design record 58 immutable raw zone bronze layer is present and needs its own handling; factory family 52 connector authentication management is present and needs its own handling.

B: Create work pieces for prerequisite state entering the notifications flow p1, design records design record 58 design record 59, design record 58 immutable raw zone bronze layer, factory family 52 connector authentication management. Keep prerequisite state entering the notifications flow p1 and design records design record 58 design record 59 separate. Allow design record 58 immutable raw zone bronze layer and factory family 52 connector authentication management to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: prerequisite state entering the notifications flow p1, design records design record 58 design record 59, design record 58 immutable raw zone bronze layer, factory family 52 connector authentication management.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

