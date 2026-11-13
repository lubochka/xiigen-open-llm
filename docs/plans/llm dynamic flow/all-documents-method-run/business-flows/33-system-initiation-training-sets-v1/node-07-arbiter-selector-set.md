# Node-07 - Arbiter Selector Set

## Purpose

Runtime A is the formal result collection before formatting.
Runtime B is the list of review units that need to fire.

## Pair 7.1 - ordering reviewer

A: Formal results include kernel boot, fabric primitives, flow runtime for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [ordering reviewer, consistency reviewer]. Reason: the result collection contains kernel boot, fabric primitives, flow runtime and must be checked before any approved translation reaches the formatter.

Pair 7.1 - Rephrased

A: R1...Rn for system initiation includes branches about kernel boot, fabric primitives, flow runtime.

B: Required reviewers: ordering reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.2 - privacy reviewer

A: Formal results include fabric primitives, flow runtime, plan import registries for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [privacy reviewer, consistency reviewer]. Reason: the result collection contains fabric primitives, flow runtime, plan import registries and must be checked before any approved translation reaches the formatter.

Pair 7.2 - Rephrased

A: R1...Rn for system initiation includes branches about fabric primitives, flow runtime, plan import registries.

B: Required reviewers: privacy reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.3 - business-rule reviewer

A: Formal results include flow runtime, plan import registries, build workspace for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [business-rule reviewer, consistency reviewer]. Reason: the result collection contains flow runtime, plan import registries, build workspace and must be checked before any approved translation reaches the formatter.

Pair 7.3 - Rephrased

A: R1...Rn for system initiation includes branches about flow runtime, plan import registries, build workspace.

B: Required reviewers: business-rule reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.4 - safety reviewer

A: Formal results include plan import registries, build workspace, parallel factory branches for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [safety reviewer, consistency reviewer]. Reason: the result collection contains plan import registries, build workspace, parallel factory branches and must be checked before any approved translation reaches the formatter.

Pair 7.4 - Rephrased

A: R1...Rn for system initiation includes branches about plan import registries, build workspace, parallel factory branches.

B: Required reviewers: safety reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.5 - consistency reviewer

A: Formal results include build workspace, parallel factory branches, server-side family work for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [consistency reviewer, consistency reviewer]. Reason: the result collection contains build workspace, parallel factory branches, server-side family work and must be checked before any approved translation reaches the formatter.

Pair 7.5 - Rephrased

A: R1...Rn for system initiation includes branches about build workspace, parallel factory branches, server-side family work.

B: Required reviewers: consistency reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.6 - measurement reviewer

A: Formal results include parallel factory branches, server-side family work, client-side family work for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [measurement reviewer, consistency reviewer]. Reason: the result collection contains parallel factory branches, server-side family work, client-side family work and must be checked before any approved translation reaches the formatter.

Pair 7.6 - Rephrased

A: R1...Rn for system initiation includes branches about parallel factory branches, server-side family work, client-side family work.

B: Required reviewers: measurement reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.7 - recovery reviewer

A: Formal results include server-side family work, client-side family work, test harness and evidence for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [recovery reviewer, consistency reviewer]. Reason: the result collection contains server-side family work, client-side family work, test harness and evidence and must be checked before any approved translation reaches the formatter.

Pair 7.7 - Rephrased

A: R1...Rn for system initiation includes branches about server-side family work, client-side family work, test harness and evidence.

B: Required reviewers: recovery reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

## Pair 7.8 - audience-clarity reviewer

A: Formal results include client-side family work, test harness and evidence, join gate and promotion for system initiation. One or more results can affect ordering, privacy, consistency, recovery, user decisions, or measurement.

B: Select review units: [audience-clarity reviewer, consistency reviewer]. Reason: the result collection contains client-side family work, test harness and evidence, join gate and promotion and must be checked before any approved translation reaches the formatter.

Pair 7.8 - Rephrased

A: R1...Rn for system initiation includes branches about client-side family work, test harness and evidence, join gate and promotion.

B: Required reviewers: audience-clarity reviewer and consistency reviewer. They must inspect the formal results alongside result translation, before final output is shaped for the user.

