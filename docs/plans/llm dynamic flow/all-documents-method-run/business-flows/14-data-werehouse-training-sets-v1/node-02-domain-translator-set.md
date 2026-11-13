# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

## Pair 2.1 - ClickUp organization hierarchy

A: Prompt fragment: "Build a warehouse that brings ClickUp spaces, folders, lists, tasks, statuses, owners, comments, and time into one model." Domain label: ClickUp organization hierarchy.

B: The ClickUp organization hierarchy responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Build a warehouse that brings ClickUp spaces, folders, lists, tasks, statuses, owners, comments, and time into one model.

Pair 2.1 - Rephrased

A: Domain label: ClickUp organization hierarchy. Fragment: "I need a warehouse that brings ClickUp spaces, folders, lists, tasks, statuses, owners, comments, and time into one model."

B: Treat ClickUp organization hierarchy as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.2 - Zoho CRM and sales data

A: Prompt fragment: "Join Zoho leads, deals, customers, invoices, support cases, and campaigns to operational work." Domain label: Zoho CRM and sales data.

B: The Zoho CRM and sales data responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Join Zoho leads, deals, customers, invoices, support cases, and campaigns to operational work.

Pair 2.2 - Rephrased

A: Domain label: Zoho CRM and sales data. Fragment: "Combine Zoho leads, deals, customers, invoices, support cases, and campaigns to operational work."

B: Treat Zoho CRM and sales data as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.3 - connectors and ingestion

A: Prompt fragment: "Explain how incremental extraction from sources lands safely before transformation." Domain label: connectors and ingestion.

B: The connectors and ingestion responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Explain how incremental extraction from sources lands safely before transformation.

Pair 2.3 - Rephrased

A: Domain label: connectors and ingestion. Fragment: "I want to understand how incremental extraction from sources lands safely before transformation."

B: Treat connectors and ingestion as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.4 - identity standardization

A: Prompt fragment: "Unify people, accounts, projects, and customers across ClickUp and Zoho." Domain label: identity standardization.

B: The identity standardization responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Unify people, accounts, projects, and customers across ClickUp and Zoho.

Pair 2.4 - Rephrased

A: Domain label: identity standardization. Fragment: "Unify people, accounts, projects, and customers across ClickUp and Zoho."

B: Treat identity standardization as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.5 - core modeling and marts

A: Prompt fragment: "Create core models and analytics marts for sales-to-delivery, work-to-financials, and service-to-product." Domain label: core modeling and marts.

B: The core modeling and marts responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Create core models and analytics marts for sales-to-delivery, work-to-financials, and service-to-product.

Pair 2.5 - Rephrased

A: Domain label: core modeling and marts. Fragment: "Help me create core models and analytics marts for sales-to-delivery, work-to-financials, and service-to-product."

B: Treat core modeling and marts as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.6 - semantic metrics layer

A: Prompt fragment: "Define one accepted meaning for delivery time, lead conversion, billed value, workload, and support resolution." Domain label: semantic metrics layer.

B: The semantic metrics layer responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Define one accepted meaning for delivery time, lead conversion, billed value, workload, and support resolution.

Pair 2.6 - Rephrased

A: Domain label: semantic metrics layer. Fragment: "Set one accepted meaning for delivery time, lead conversion, billed value, workload, and support resolution."

B: Treat semantic metrics layer as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.7 - observability and data quality

A: Prompt fragment: "Detect missing records, late syncs, broken joins, and metric drift." Domain label: observability and data quality.

B: The observability and data quality responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Detect missing records, late syncs, broken joins, and metric drift.

Pair 2.7 - Rephrased

A: Domain label: observability and data quality. Fragment: "Detect missing records, late syncs, broken joins, and metric drift."

B: Treat observability and data quality as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.8 - governance security and privacy

A: Prompt fragment: "Protect private customer and employee data while still allowing useful reporting." Domain label: governance security and privacy.

B: The governance security and privacy responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Protect private customer and employee data while still allowing useful reporting.

Pair 2.8 - Rephrased

A: Domain label: governance security and privacy. Fragment: "Preserve safe private customer and employee data while still allowing useful reporting."

B: Treat governance security and privacy as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.9 - activation back into operations

A: Prompt fragment: "Push insight back to operations, such as creating follow-up tasks from a risk signal." Domain label: activation back into operations.

B: The activation back into operations responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Push insight back to operations, such as creating follow-up tasks from a risk signal.

Pair 2.9 - Rephrased

A: Domain label: activation back into operations. Fragment: "Push insight back to operations, such as creating follow-up tasks from a risk signal."

B: Treat activation back into operations as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.10 - Zoho CRM and sales data

A: Prompt fragment: "Show sales-to-execution from campaign and deal through project delivery and billing outcome." Domain label: Zoho CRM and sales data.

B: The Zoho CRM and sales data responsibility must be represented as a separate territory in the data warehouse flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Show sales-to-execution from campaign and deal through project delivery and billing outcome.

Pair 2.10 - Rephrased

A: Domain label: Zoho CRM and sales data. Fragment: "Show sales-to-execution from campaign and deal through project delivery and billing outcome."

B: Treat Zoho CRM and sales data as its own responsibility inside data warehouse. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

