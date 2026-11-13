# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: screen structure is present and needs its own handling; component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling.

B: Create work pieces for screen structure, component meaning, user action, data binding. Keep screen structure and component meaning separate. Allow user action and data binding to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: screen structure, component meaning, user action, data binding.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling.

B: Create work pieces for component meaning, user action, data binding, system type. Keep component meaning and user action separate. Allow data binding and system type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: component meaning, user action, data binding, system type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling.

B: Create work pieces for user action, data binding, system type, gap completion. Keep user action and data binding separate. Allow system type and gap completion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: user action, data binding, system type, gap completion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling; source meaning is present and needs its own handling.

B: Create work pieces for data binding, system type, gap completion, source meaning. Keep data binding and system type separate. Allow gap completion and source meaning to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: data binding, system type, gap completion, source meaning.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: system type is present and needs its own handling; gap completion is present and needs its own handling; source meaning is present and needs its own handling; audience need is present and needs its own handling.

B: Create work pieces for system type, gap completion, source meaning, audience need. Keep system type and gap completion separate. Allow source meaning and audience need to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: system type, gap completion, source meaning, audience need.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: gap completion is present and needs its own handling; source meaning is present and needs its own handling; audience need is present and needs its own handling; version evidence is present and needs its own handling.

B: Create work pieces for gap completion, source meaning, audience need, version evidence. Keep gap completion and source meaning separate. Allow audience need and version evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: gap completion, source meaning, audience need, version evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: source meaning is present and needs its own handling; audience need is present and needs its own handling; version evidence is present and needs its own handling; edge condition is present and needs its own handling.

B: Create work pieces for source meaning, audience need, version evidence, edge condition. Keep source meaning and audience need separate. Allow version evidence and edge condition to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: source meaning, audience need, version evidence, edge condition.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PK ; j word/PK ; j word/ rels/PK ; j Oe ! word/ rels/document.xml.rels N ! p Z 1 Y ?q L 1v+5 93 29 fb hQ EUs m;b/ r +L D F X A = F : 7 b ~W= e O= ' 9: Q HWV @ B: audience need is present and needs its own handling; version evidence is present and needs its own handling; edge condition is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for audience need, version evidence, edge condition, training example. Keep audience need and version evidence separate. Allow edge condition and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: audience need, version evidence, edge condition, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

