# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; build a message map before debugging a bridge is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, build a message map before debugging a bridge. Keep configuration choice and dependency rule separate. Allow completion gate and build a message map before debugging a bridge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, build a message map before debugging a bridge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; build a message map before debugging a bridge is present and needs its own handling; find all sends from ui to sandbox is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, build a message map before debugging a bridge, find all sends from ui to sandbox. Keep dependency rule and completion gate separate. Allow build a message map before debugging a bridge and find all sends from ui to sandbox to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, build a message map before debugging a bridge, find all sends from ui to sandbox.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: completion gate is present and needs its own handling; build a message map before debugging a bridge is present and needs its own handling; find all sends from ui to sandbox is present and needs its own handling; find all listeners in sandbox is present and needs its own handling.

B: Create work pieces for completion gate, build a message map before debugging a bridge, find all sends from ui to sandbox, find all listeners in sandbox. Keep completion gate and build a message map before debugging a bridge separate. Allow find all sends from ui to sandbox and find all listeners in sandbox to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, build a message map before debugging a bridge, find all sends from ui to sandbox, find all listeners in sandbox.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: build a message map before debugging a bridge is present and needs its own handling; find all sends from ui to sandbox is present and needs its own handling; find all listeners in sandbox is present and needs its own handling; find all sandbox ui sends is present and needs its own handling.

B: Create work pieces for build a message map before debugging a bridge, find all sends from ui to sandbox, find all listeners in sandbox, find all sandbox ui sends. Keep build a message map before debugging a bridge and find all sends from ui to sandbox separate. Allow find all listeners in sandbox and find all sandbox ui sends to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: build a message map before debugging a bridge, find all sends from ui to sandbox, find all listeners in sandbox, find all sandbox ui sends.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Build a Message Map Before Debugging a Bridge: find all sends from ui to sandbox is present and needs its own handling; find all listeners in sandbox is present and needs its own handling; find all sandbox ui sends is present and needs its own handling; find all ui listeners is present and needs its own handling.

B: Create work pieces for find all sends from ui to sandbox, find all listeners in sandbox, find all sandbox ui sends, find all ui listeners. Keep find all sends from ui to sandbox and find all listeners in sandbox separate. Allow find all sandbox ui sends and find all ui listeners to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: find all sends from ui to sandbox, find all listeners in sandbox, find all sandbox ui sends, find all ui listeners.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

