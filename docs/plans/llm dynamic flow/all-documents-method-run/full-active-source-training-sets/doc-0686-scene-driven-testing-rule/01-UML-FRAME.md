# 01 - UML Frame

## Source Round

This frame belongs to the training package for Scene-Driven Testing. It records the dynamic-flow mapping before writing node pairs.

## Active Nodes

1. Domain splitter: user prompt -> array of domain labels.
2. Domain translator array: one domain fragment -> one formal domain statement.
3. Scheme planner: all formal domain statements -> ordered and parallel work pieces.
4. Domain processor array: one work piece -> complete processed result.
5. Applier array: one processed piece plus its instruction -> concrete outcome.
6. Result translator array: one formal result, with the full result collection visible -> user-readable result.
7. Arbiter selector: formal result collection -> required review units.
8. Arbiter units: review responsibility plus result -> pass, concern, or reprocess instruction.
9. Formatter: approved translated results -> one finished answer.

## Edges

### User Prompt To Domain Splitter

What moves: the human request in ordinary language.
What the target receives: one prompt that may contain several responsibilities.
What the target must produce: an array of domain labels.
What breaks if missing: the request is treated as one vague block.

### Domain Splitter To Domain Translator Array

What moves: each label with the matching prompt fragment.
What the target receives: one domain-scoped fragment.
What the target must produce: precise domain wording.
What breaks if missing: ambiguity travels forward.

### Domain Translator Array To Scheme Planner

What moves: all translated domain statements.
What the target receives: the full multi-domain meaning.
What the target must produce: ordered work pieces, parallel work pieces, waits, recovery, and reviews.
What breaks if missing: facts do not become a usable plan.

### Scheme Planner To Domain Processor Array

What moves: each planned work piece.
What the target receives: one work piece at a time.
What the target must produce: the complete processed result for that piece.
What breaks if missing: parallel flow collapses.

### Domain Processor Array To Applier Array

What moves: processed work and its instruction.
What the target receives: enough detail to act.
What the target must produce: a concrete result.
What breaks if missing: nothing becomes an outcome.

### Applier Array To Result Translator Array

What moves: formal results R1...Rn.
What the target receives: one result while seeing the full collection.
What the target must produce: readable wording or a contradiction flag.
What breaks if missing: raw formal results reach the user.

### Applier Array To Arbiter Selector

What moves: the full formal result collection.
What the target receives: results before formatting.
What the target must produce: selected reviewers.
What breaks if missing: risky results are not reviewed.

### Arbiter Selector To Arbiter Units

What moves: review responsibilities.
What the target receives: one responsibility and affected result.
What the target must produce: pass, concern, or reprocess.
What breaks if missing: review is named but not performed.

### Arbiter Units Back To Front Of Flow

What moves: a failed fragment with repair reason.
What the target receives: the affected fragment for another pass.
What the target must produce: a corrected split and re-run.
What breaks if missing: review cannot repair.

### Approved Translations To Formatter

What moves: approved readable fragments only.
What the target receives: safe fragments and audience.
What the target must produce: one coherent answer.
What breaks if missing: the user receives disconnected fragments.
