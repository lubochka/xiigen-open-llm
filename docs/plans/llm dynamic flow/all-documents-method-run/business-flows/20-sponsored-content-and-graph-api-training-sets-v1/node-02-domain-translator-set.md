# Node-02 - Domain Translator Set

## Purpose

Runtime A is one prompt fragment paired with one domain label.
Runtime B is the same request written in that domain's precise working language.

## Pair 2.1 - graph access gateway

A: Prompt fragment: "Let an app read profile, page, group, post, and connection data only when permissions allow it." Domain label: graph access gateway.

B: The graph access gateway responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Let an app read profile, page, group, post, and connection data only when permissions allow it.

Pair 2.1 - Rephrased

A: Domain label: graph access gateway. Fragment: "Allow an app read profile, page, group, post, and connection data only when permissions allow it."

B: Treat graph access gateway as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.2 - graph access gateway

A: Prompt fragment: "Plan a graph write where an app publishes or changes an object and subscribers receive updates." Domain label: graph access gateway.

B: The graph access gateway responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Plan a graph write where an app publishes or changes an object and subscribers receive updates.

Pair 2.2 - Rephrased

A: Domain label: graph access gateway. Fragment: "Plan a graph write where an app publishes or changes an object and subscribers receive updates."

B: Treat graph access gateway as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.3 - ad account and campaign hierarchy

A: Prompt fragment: "Build a campaign from advertiser account to campaign, ad set, creative, targeting, delivery, and reporting." Domain label: ad account and campaign hierarchy.

B: The ad account and campaign hierarchy responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Build a campaign from advertiser account to campaign, ad set, creative, targeting, delivery, and reporting.

Pair 2.3 - Rephrased

A: Domain label: ad account and campaign hierarchy. Fragment: "I need a campaign from advertiser account to campaign, ad set, creative, targeting, delivery, and reporting."

B: Treat ad account and campaign hierarchy as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.4 - ad review and policy enforcement

A: Prompt fragment: "Review ads before delivery and block policy violations or unsafe creative." Domain label: ad review and policy enforcement.

B: The ad review and policy enforcement responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Review ads before delivery and block policy violations or unsafe creative.

Pair 2.4 - Rephrased

A: Domain label: ad review and policy enforcement. Fragment: "Review ads before delivery and block policy violations or unsafe creative."

B: Treat ad review and policy enforcement as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.5 - auction ranking and pacing

A: Prompt fragment: "Rank eligible ads during a feed request while respecting audience, pacing, budget, and user experience." Domain label: auction ranking and pacing.

B: The auction ranking and pacing responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Rank eligible ads during a feed request while respecting audience, pacing, budget, and user experience.

Pair 2.5 - Rephrased

A: Domain label: auction ranking and pacing. Fragment: "Order eligible ads during a feed request while respecting audience, pacing, budget, and user experience."

B: Treat auction ranking and pacing as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.6 - feed request integration

A: Prompt fragment: "Explain how sponsored content connects to the same feed that shows organic social posts." Domain label: feed request integration.

B: The feed request integration responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Explain how sponsored content connects to the same feed that shows organic social posts.

Pair 2.6 - Rephrased

A: Domain label: feed request integration. Fragment: "I want to understand how sponsored content connects to the same feed that shows organic social posts."

B: Treat feed request integration as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.7 - measurement attribution and billing

A: Prompt fragment: "Measure impressions, clicks, conversions, billing, attribution, and advertiser insight." Domain label: measurement attribution and billing.

B: The measurement attribution and billing responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Measure impressions, clicks, conversions, billing, attribution, and advertiser insight.

Pair 2.7 - Rephrased

A: Domain label: measurement attribution and billing. Fragment: "Measure impressions, clicks, conversions, billing, attribution, and advertiser insight."

B: Treat measurement attribution and billing as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.8 - permission decision rules

A: Prompt fragment: "Protect graph access from apps that request fields without consent or outside allowed scope." Domain label: permission decision rules.

B: The permission decision rules responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Protect graph access from apps that request fields without consent or outside allowed scope.

Pair 2.8 - Rephrased

A: Domain label: permission decision rules. Fragment: "Preserve safe graph access from apps that request fields without consent or outside allowed scope."

B: Treat permission decision rules as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.9 - query planning and federation

A: Prompt fragment: "Federate graph requests across social objects without exposing fields the caller cannot see." Domain label: query planning and federation.

B: The query planning and federation responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Federate graph requests across social objects without exposing fields the caller cannot see.

Pair 2.9 - Rephrased

A: Domain label: query planning and federation. Fragment: "Federate graph requests across social objects without exposing fields the caller cannot see."

B: Treat query planning and federation as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

## Pair 2.10 - webhooks and subscriptions

A: Prompt fragment: "Send subscription updates when relevant graph objects change." Domain label: webhooks and subscriptions.

B: The webhooks and subscriptions responsibility must be represented as a separate territory in the sponsored content and graph access flow. It must preserve the user's requested outcome, name the information it depends on, name the visible result it should produce, and expose any approval, waiting, recovery, privacy, or measurement concern that affects the rest of the flow. The fragment asks for: Send subscription updates when relevant graph objects change.

Pair 2.10 - Rephrased

A: Domain label: webhooks and subscriptions. Fragment: "Forward subscription updates when relevant graph objects change."

B: Treat webhooks and subscriptions as its own responsibility inside sponsored content and graph access. State what is being requested, what must be known first, what result leaves this territory, and what risk or waiting condition must remain visible to later nodes.

