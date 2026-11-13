# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the formal result collection before formatting.
Runtime B is the list of review units that need to fire.

## Pair 7.1 - ordering reviewer

A: Formal results include graph access gateway, schema and field projection, app identity and permissions for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [ordering reviewer, consistency reviewer]. Reason: the result collection contains graph access gateway, schema and field projection, app identity and permissions and must be checked before any approved translation reaches the formatter.

Pair 7.1 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about graph access gateway, schema and field projection, app identity and permissions.

B: Required reviewers: ordering reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.2 - privacy reviewer

A: Formal results include schema and field projection, app identity and permissions, permission decision rules for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [privacy reviewer, consistency reviewer]. Reason: the result collection contains schema and field projection, app identity and permissions, permission decision rules and must be checked before any approved translation reaches the formatter.

Pair 7.2 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about schema and field projection, app identity and permissions, permission decision rules.

B: Required reviewers: privacy reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.3 - business-rule reviewer

A: Formal results include app identity and permissions, permission decision rules, query planning and federation for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [business-rule reviewer, consistency reviewer]. Reason: the result collection contains app identity and permissions, permission decision rules, query planning and federation and must be checked before any approved translation reaches the formatter.

Pair 7.3 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about app identity and permissions, permission decision rules, query planning and federation.

B: Required reviewers: business-rule reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.4 - safety reviewer

A: Formal results include permission decision rules, query planning and federation, webhooks and subscriptions for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [safety reviewer, consistency reviewer]. Reason: the result collection contains permission decision rules, query planning and federation, webhooks and subscriptions and must be checked before any approved translation reaches the formatter.

Pair 7.4 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about permission decision rules, query planning and federation, webhooks and subscriptions.

B: Required reviewers: safety reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.5 - consistency reviewer

A: Formal results include query planning and federation, webhooks and subscriptions, ad account and campaign hierarchy for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [consistency reviewer, consistency reviewer]. Reason: the result collection contains query planning and federation, webhooks and subscriptions, ad account and campaign hierarchy and must be checked before any approved translation reaches the formatter.

Pair 7.5 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about query planning and federation, webhooks and subscriptions, ad account and campaign hierarchy.

B: Required reviewers: consistency reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.6 - measurement reviewer

A: Formal results include webhooks and subscriptions, ad account and campaign hierarchy, targeting and audience rules for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [measurement reviewer, consistency reviewer]. Reason: the result collection contains webhooks and subscriptions, ad account and campaign hierarchy, targeting and audience rules and must be checked before any approved translation reaches the formatter.

Pair 7.6 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about webhooks and subscriptions, ad account and campaign hierarchy, targeting and audience rules.

B: Required reviewers: measurement reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.7 - recovery reviewer

A: Formal results include ad account and campaign hierarchy, targeting and audience rules, creative rendering for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [recovery reviewer, consistency reviewer]. Reason: the result collection contains ad account and campaign hierarchy, targeting and audience rules, creative rendering and must be checked before any approved translation reaches the formatter.

Pair 7.7 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about ad account and campaign hierarchy, targeting and audience rules, creative rendering.

B: Required reviewers: recovery reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.8 - audience-clarity reviewer

A: Formal results include targeting and audience rules, creative rendering, ad review and policy enforcement for sponsored content and graph access. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [audience-clarity reviewer, consistency reviewer]. Reason: the result collection contains targeting and audience rules, creative rendering, ad review and policy enforcement and must be checked before any approved translation reaches the formatter.

Pair 7.8 - Rephrased

A: R1...Rn for sponsored content and graph access includes branches about targeting and audience rules, creative rendering, ad review and policy enforcement.

B: Required reviewers: audience-clarity reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

