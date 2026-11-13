# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

## Pair 2.1 - form definition builder

A: Prompt fragment: "Let a non-developer build a multi-step form with fields, rules, calculations, and stored settings." Domain label: form definition builder.

B: The form definition builder responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Let a non-developer build a multi-step form with fields, rules, calculations, and stored settings.

Pair 2.1 - Rephrased

A: Domain label: form definition builder. Fragment: "Allow a non-developer build a multi-step form with fields, rules, calculations, and stored settings."

B: Treat form definition builder as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.2 - runtime form rendering

A: Prompt fragment: "Render a form, validate input, handle errors, and create a stored submission." Domain label: runtime form rendering.

B: The runtime form rendering responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Render a form, validate input, handle errors, and create a stored submission.

Pair 2.2 - Rephrased

A: Domain label: runtime form rendering. Fragment: "Render a form, validate input, handle errors, and create a stored submission."

B: Treat runtime form rendering as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.3 - submission pipeline

A: Prompt fragment: "After a lead submits a form, notify sales and create a follow-up task." Domain label: submission pipeline.

B: The submission pipeline responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: After a lead submits a form, notify sales and create a follow-up task.

Pair 2.3 - Rephrased

A: Domain label: submission pipeline. Fragment: "When a lead submits a form, notify sales and create a follow-up task."

B: Treat submission pipeline as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.4 - payments

A: Prompt fragment: "Process a paid booking form with payment, confirmation, entry storage, and failure recovery." Domain label: payments.

B: The payments responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Process a paid booking form with payment, confirmation, entry storage, and failure recovery.

Pair 2.4 - Rephrased

A: Domain label: payments. Fragment: "Process a paid booking form with payment, confirmation, entry storage, and failure recovery."

B: Treat payments as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.5 - file uploads and media

A: Prompt fragment: "Accept uploaded files safely and attach them to the entry for later review." Domain label: file uploads and media.

B: The file uploads and media responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Accept uploaded files safely and attach them to the entry for later review.

Pair 2.5 - Rephrased

A: Domain label: file uploads and media. Fragment: "Receive uploaded files safely and attach them to the entry for later review."

B: Treat file uploads and media as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.6 - integration feeds

A: Prompt fragment: "Send a submission to an external system using a feed while mapping field values correctly." Domain label: integration feeds.

B: The integration feeds responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Send a submission to an external system using a feed while mapping field values correctly.

Pair 2.6 - Rephrased

A: Domain label: integration feeds. Fragment: "Forward a submission to an external system using a feed while mapping field values correctly."

B: Treat integration feeds as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.7 - rule and condition engine

A: Prompt fragment: "Run a support triage flow based on form answers and route it to the right queue." Domain label: rule and condition engine.

B: The rule and condition engine responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Run a support triage flow based on form answers and route it to the right queue.

Pair 2.7 - Rephrased

A: Domain label: rule and condition engine. Fragment: "Set up a support triage flow based on form answers and route it to the right queue."

B: Treat rule and condition engine as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.8 - queue retry and idempotency

A: Prompt fragment: "Retry failed notifications and integrations without creating duplicate entries." Domain label: queue retry and idempotency.

B: The queue retry and idempotency responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Retry failed notifications and integrations without creating duplicate entries.

Pair 2.8 - Rephrased

A: Domain label: queue retry and idempotency. Fragment: "Retry failed notifications and integrations without creating duplicate entries."

B: Treat queue retry and idempotency as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.9 - admin operations

A: Prompt fragment: "Give admins logs, resend, export, retry, and permission controls." Domain label: admin operations.

B: The admin operations responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Give admins logs, resend, export, retry, and permission controls.

Pair 2.9 - Rephrased

A: Domain label: admin operations. Fragment: "Provide admins logs, resend, export, retry, and permission controls."

B: Treat admin operations as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.10 - submission pipeline

A: Prompt fragment: "Connect form submissions to a larger automation engine through events." Domain label: submission pipeline.

B: The submission pipeline responsibility must be represented as a separate territory in the forms and flows flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Connect form submissions to a larger automation engine through events.

Pair 2.10 - Rephrased

A: Domain label: submission pipeline. Fragment: "Link together form submissions to a larger automation engine through events."

B: Treat submission pipeline as its own responsibility inside forms and flows. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

