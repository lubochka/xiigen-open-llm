# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: screen structure is present and needs its own handling; component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling.

B: Create work pieces for screen structure, component meaning, user action, data binding. Keep screen structure and component meaning separate. Allow user action and data binding to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: screen structure, component meaning, user action, data binding.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling.

B: Create work pieces for component meaning, user action, data binding, system type. Keep component meaning and user action separate. Allow data binding and system type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: component meaning, user action, data binding, system type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling.

B: Create work pieces for user action, data binding, system type, gap completion. Keep user action and data binding separate. Allow system type and gap completion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: user action, data binding, system type, gap completion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling; git branch ownership is present and needs its own handling.

B: Create work pieces for data binding, system type, gap completion, git branch ownership. Keep data binding and system type separate. Allow gap completion and git branch ownership to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: data binding, system type, gap completion, git branch ownership.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: system type is present and needs its own handling; gap completion is present and needs its own handling; git branch ownership is present and needs its own handling; the hard rules is present and needs its own handling.

B: Create work pieces for system type, gap completion, git branch ownership, the hard rules. Keep system type and gap completion separate. Allow git branch ownership and the hard rules to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: system type, gap completion, git branch ownership, the hard rules.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: gap completion is present and needs its own handling; git branch ownership is present and needs its own handling; the hard rules is present and needs its own handling; session start branch protocol is present and needs its own handling.

B: Create work pieces for gap completion, git branch ownership, the hard rules, session start branch protocol. Keep gap completion and git branch ownership separate. Allow the hard rules and session start branch protocol to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: gap completion, git branch ownership, the hard rules, session start branch protocol.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: git branch ownership is present and needs its own handling; the hard rules is present and needs its own handling; session start branch protocol is present and needs its own handling; what claude may do with git only these is present and needs its own handling.

B: Create work pieces for git branch ownership, the hard rules, session start branch protocol, what claude may do with git only these. Keep git branch ownership and the hard rules separate. Allow session start branch protocol and what claude may do with git only these to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: git branch ownership, the hard rules, session start branch protocol, what claude may do with git only these.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Git Branch Ownership: the hard rules is present and needs its own handling; session start branch protocol is present and needs its own handling; what claude may do with git only these is present and needs its own handling; before any commit the pre commit check is present and needs its own handling.

B: Create work pieces for the hard rules, session start branch protocol, what claude may do with git only these, before any commit the pre commit check. Keep the hard rules and session start branch protocol separate. Allow what claude may do with git only these and before any commit the pre commit check to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the hard rules, session start branch protocol, what claude may do with git only these, before any commit the pre commit check.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

