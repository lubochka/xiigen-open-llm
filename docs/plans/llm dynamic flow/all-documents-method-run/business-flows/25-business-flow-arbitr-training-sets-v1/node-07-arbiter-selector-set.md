# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the formal result collection before formatting.
Runtime B is the list of review units that need to fire.

## Pair 7.1 - ordering reviewer

A: Formal results include proposed change detection, context gathering, related flow discovery for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [ordering reviewer, consistency reviewer]. Reason: the result collection contains proposed change detection, context gathering, related flow discovery and must be checked before any approved translation reaches the formatter.

Pair 7.1 - Rephrased

A: R1...Rn for business flow arbiter includes branches about proposed change detection, context gathering, related flow discovery.

B: Required reviewers: ordering reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.2 - privacy reviewer

A: Formal results include context gathering, related flow discovery, entity dependency analysis for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [privacy reviewer, consistency reviewer]. Reason: the result collection contains context gathering, related flow discovery, entity dependency analysis and must be checked before any approved translation reaches the formatter.

Pair 7.2 - Rephrased

A: R1...Rn for business flow arbiter includes branches about context gathering, related flow discovery, entity dependency analysis.

B: Required reviewers: privacy reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.3 - business-rule reviewer

A: Formal results include related flow discovery, entity dependency analysis, business rule validation for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [business-rule reviewer, consistency reviewer]. Reason: the result collection contains related flow discovery, entity dependency analysis, business rule validation and must be checked before any approved translation reaches the formatter.

Pair 7.3 - Rephrased

A: R1...Rn for business flow arbiter includes branches about related flow discovery, entity dependency analysis, business rule validation.

B: Required reviewers: business-rule reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.4 - safety reviewer

A: Formal results include entity dependency analysis, business rule validation, conflict explanation for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [safety reviewer, consistency reviewer]. Reason: the result collection contains entity dependency analysis, business rule validation, conflict explanation and must be checked before any approved translation reaches the formatter.

Pair 7.4 - Rephrased

A: R1...Rn for business flow arbiter includes branches about entity dependency analysis, business rule validation, conflict explanation.

B: Required reviewers: safety reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.5 - consistency reviewer

A: Formal results include business rule validation, conflict explanation, user decision request for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [consistency reviewer, consistency reviewer]. Reason: the result collection contains business rule validation, conflict explanation, user decision request and must be checked before any approved translation reaches the formatter.

Pair 7.5 - Rephrased

A: R1...Rn for business flow arbiter includes branches about business rule validation, conflict explanation, user decision request.

B: Required reviewers: consistency reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.6 - measurement reviewer

A: Formal results include conflict explanation, user decision request, resolution strategy for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [measurement reviewer, consistency reviewer]. Reason: the result collection contains conflict explanation, user decision request, resolution strategy and must be checked before any approved translation reaches the formatter.

Pair 7.6 - Rephrased

A: R1...Rn for business flow arbiter includes branches about conflict explanation, user decision request, resolution strategy.

B: Required reviewers: measurement reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.7 - recovery reviewer

A: Formal results include user decision request, resolution strategy, impact report for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [recovery reviewer, consistency reviewer]. Reason: the result collection contains user decision request, resolution strategy, impact report and must be checked before any approved translation reaches the formatter.

Pair 7.7 - Rephrased

A: R1...Rn for business flow arbiter includes branches about user decision request, resolution strategy, impact report.

B: Required reviewers: recovery reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.8 - audience-clarity reviewer

A: Formal results include resolution strategy, impact report, regression prevention for business flow arbiter. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [audience-clarity reviewer, consistency reviewer]. Reason: the result collection contains resolution strategy, impact report, regression prevention and must be checked before any approved translation reaches the formatter.

Pair 7.8 - Rephrased

A: R1...Rn for business flow arbiter includes branches about resolution strategy, impact report, regression prevention.

B: Required reviewers: audience-clarity reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

