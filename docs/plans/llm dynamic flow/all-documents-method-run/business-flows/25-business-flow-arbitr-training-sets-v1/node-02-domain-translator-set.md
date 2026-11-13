# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

## Pair 2.1 - proposed change detection

A: Prompt fragment: "A change wants to delete a field used by checkout and subscription renewal; identify impacted processes before approval." Domain label: proposed change detection.

B: The proposed change detection responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: A change wants to delete a field used by checkout and subscription renewal; identify impacted processes before approval.

Pair 2.1 - Rephrased

A: Domain label: proposed change detection. Fragment: "A change wants to delete a field used by checkout and subscription renewal; identify impacted processes before approval."

B: Treat proposed change detection as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.2 - context gathering

A: Prompt fragment: "Gather relevant flows, documentation, database entities, and rules before judging a change." Domain label: context gathering.

B: The context gathering responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Gather relevant flows, documentation, database entities, and rules before judging a change.

Pair 2.2 - Rephrased

A: Domain label: context gathering. Fragment: "Gather relevant flows, documentation, database entities, and rules before judging a change."

B: Treat context gathering as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.3 - business rule validation

A: Prompt fragment: "Validate whether an edit breaks existing business logic, data dependencies, or previous flow promises." Domain label: business rule validation.

B: The business rule validation responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Validate whether an edit breaks existing business logic, data dependencies, or previous flow promises.

Pair 2.3 - Rephrased

A: Domain label: business rule validation. Fragment: "Check whether an edit breaks existing business logic, data dependencies, or previous flow promises."

B: Treat business rule validation as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.4 - conflict explanation

A: Prompt fragment: "Explain affected processes to the user and ask what resolution path they want." Domain label: conflict explanation.

B: The conflict explanation responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Explain affected processes to the user and ask what resolution path they want.

Pair 2.4 - Rephrased

A: Domain label: conflict explanation. Fragment: "I want to understand affected processes to the user and ask what resolution path they want."

B: Treat conflict explanation as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.5 - resolution strategy

A: Prompt fragment: "Offer choices such as refactor dependent flows, reject the change, keep compatibility, or force proceed." Domain label: resolution strategy.

B: The resolution strategy responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Offer choices such as refactor dependent flows, reject the change, keep compatibility, or force proceed.

Pair 2.5 - Rephrased

A: Domain label: resolution strategy. Fragment: "Provide choices such as refactor dependent flows, reject the change, keep compatibility, or force proceed."

B: Treat resolution strategy as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.6 - regression prevention

A: Prompt fragment: "Block silent regressions when an automated agent changes shared entities." Domain label: regression prevention.

B: The regression prevention responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Block silent regressions when an automated agent changes shared entities.

Pair 2.6 - Rephrased

A: Domain label: regression prevention. Fragment: "Stop silent regressions when an automated agent changes shared entities."

B: Treat regression prevention as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.7 - documentation and data review

A: Prompt fragment: "Review documents and flow definitions to understand how a field participates in several processes." Domain label: documentation and data review.

B: The documentation and data review responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Review documents and flow definitions to understand how a field participates in several processes.

Pair 2.7 - Rephrased

A: Domain label: documentation and data review. Fragment: "Review documents and flow definitions to understand how a field participates in several processes."

B: Treat documentation and data review as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.8 - impact report

A: Prompt fragment: "Produce a human-readable impact report naming the exact broken flows and why." Domain label: impact report.

B: The impact report responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Produce a human-readable impact report naming the exact broken flows and why.

Pair 2.8 - Rephrased

A: Domain label: impact report. Fragment: "Produce a human-readable impact report naming the exact broken flows and why."

B: Treat impact report as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.9 - resolution strategy

A: Prompt fragment: "Continue safely after the user selects a resolution and apply the chosen strategy." Domain label: resolution strategy.

B: The resolution strategy responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Continue safely after the user selects a resolution and apply the chosen strategy.

Pair 2.9 - Rephrased

A: Domain label: resolution strategy. Fragment: "Continue safely after the user selects a resolution and apply the chosen strategy."

B: Treat resolution strategy as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.10 - cross-flow safety gate

A: Prompt fragment: "Register business-flow review as a required gate before finalizing risky work." Domain label: cross-flow safety gate.

B: The cross-flow safety gate responsibility must be represented as a separate territory in the business flow arbiter flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Register business-flow review as a required gate before finalizing risky work.

Pair 2.10 - Rephrased

A: Domain label: cross-flow safety gate. Fragment: "Register business-flow review as a required gate before finalizing risky work."

B: Treat cross-flow safety gate as its own responsibility inside business flow arbiter. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

