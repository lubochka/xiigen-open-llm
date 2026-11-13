# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

## Pair 2.1 - tenant registry and control plane

A: Prompt fragment: "Turn a single-tenant application into SaaS where tenant context is enforced everywhere." Domain label: tenant registry and control plane.

B: The tenant registry and control plane responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Turn a single-tenant application into SaaS where tenant context is enforced everywhere.

Pair 2.1 - Rephrased

A: Domain label: tenant registry and control plane. Fragment: "Modify a single-tenant application into SaaS where tenant context is enforced everywhere."

B: Treat tenant registry and control plane as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.2 - isolation model choice

A: Prompt fragment: "Choose between shared, separated, or dedicated data isolation based on risk, cost, and contracts." Domain label: isolation model choice.

B: The isolation model choice responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Choose between shared, separated, or dedicated data isolation based on risk, cost, and contracts.

Pair 2.2 - Rephrased

A: Domain label: isolation model choice. Fragment: "Select between shared, separated, or dedicated data isolation based on risk, cost, and contracts."

B: Treat isolation model choice as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.3 - identity and authorization

A: Prompt fragment: "Bind users to tenants and enforce tenant-aware permissions for every object." Domain label: identity and authorization.

B: The identity and authorization responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Bind users to tenants and enforce tenant-aware permissions for every object.

Pair 2.3 - Rephrased

A: Domain label: identity and authorization. Fragment: "Attach users to tenants and enforce tenant-aware permissions for every object."

B: Treat identity and authorization as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.4 - data partitioning and migration

A: Prompt fragment: "Migrate data with expand, backfill, route, verify, and then remove old assumptions." Domain label: data partitioning and migration.

B: The data partitioning and migration responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Migrate data with expand, backfill, route, verify, and then remove old assumptions.

Pair 2.4 - Rephrased

A: Domain label: data partitioning and migration. Fragment: "Move data with expand, backfill, route, verify, and then remove old assumptions."

B: Treat data partitioning and migration as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.5 - tenant configuration and customization

A: Prompt fragment: "Offer tenant-specific features and settings without creating separate product forks." Domain label: tenant configuration and customization.

B: The tenant configuration and customization responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Offer tenant-specific features and settings without creating separate product forks.

Pair 2.5 - Rephrased

A: Domain label: tenant configuration and customization. Fragment: "Provide tenant-specific features and settings without creating separate product forks."

B: Treat tenant configuration and customization as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.6 - onboarding and offboarding

A: Prompt fragment: "Automate tenant onboarding and offboarding with auditable state transitions." Domain label: onboarding and offboarding.

B: The onboarding and offboarding responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Automate tenant onboarding and offboarding with auditable state transitions.

Pair 2.6 - Rephrased

A: Domain label: onboarding and offboarding. Fragment: "Automate tenant onboarding and offboarding with auditable state transitions."

B: Treat onboarding and offboarding as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.7 - billing usage and quotas

A: Prompt fragment: "Meter usage per tenant and connect quotas to billing tiers." Domain label: billing usage and quotas.

B: The billing usage and quotas responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Meter usage per tenant and connect quotas to billing tiers.

Pair 2.7 - Rephrased

A: Domain label: billing usage and quotas. Fragment: "Measure usage per tenant and connect quotas to billing tiers."

B: Treat billing usage and quotas as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.8 - tenant observability

A: Prompt fragment: "Make logs, metrics, traces, and incidents visible by tenant and also across the system." Domain label: tenant observability.

B: The tenant observability responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Make logs, metrics, traces, and incidents visible by tenant and also across the system.

Pair 2.8 - Rephrased

A: Domain label: tenant observability. Fragment: "Make logs, metrics, traces, and incidents visible by tenant and also across the system."

B: Treat tenant observability as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.9 - backup restore and disaster recovery

A: Prompt fragment: "Recover one tenant safely without leaking or overwriting another tenant." Domain label: backup restore and disaster recovery.

B: The backup restore and disaster recovery responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Recover one tenant safely without leaking or overwriting another tenant.

Pair 2.9 - Rephrased

A: Domain label: backup restore and disaster recovery. Fragment: "Restore one tenant safely without leaking or overwriting another tenant."

B: Treat backup restore and disaster recovery as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.10 - tenant isolation testing

A: Prompt fragment: "Run isolation tests and rollback gates before releasing new tenant-aware behavior." Domain label: tenant isolation testing.

B: The tenant isolation testing responsibility must be represented as a separate territory in the multi-tenant adaptation flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Run isolation tests and rollback gates before releasing new tenant-aware behavior.

Pair 2.10 - Rephrased

A: Domain label: tenant isolation testing. Fragment: "Set up isolation tests and rollback gates before releasing new tenant-aware behavior."

B: Treat tenant isolation testing as its own responsibility inside multi-tenant adaptation. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

