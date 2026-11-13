# Dynamic Flow Locked Decision D-13 - Authority Boundary v1

## Decision

D-13 locks the authority boundary between priority, KNN memory, arbiters, and external AI.

Priority decides what to attempt.

KNN uses arbiter history to choose better memory.

Arbiters decide what is accepted.

External AI is an advisory help chain, not an accepting authority.

## Meaning

Priority may choose:

- which route to try first
- which evidence to load first
- which units to run
- which fallback to attempt
- whether the user should be asked
- whether the knowledge edge detector should inspect the path

KNN may choose:

- positive neighbors
- negative neighbors
- warning neighbors
- examples that passed similar arbiters
- examples that failed similar arbiters
- examples that suggest stronger arbiter selection

External AI may provide:

- missing explanation
- outside judgment
- candidate repair
- comparison
- challenge
- broader context
- possible structure to adapt

Arbiters decide:

- whether generated work is accepted
- whether a fragment is returned for reprocessing
- whether a user clarification is needed
- whether a learning gap should be recorded
- whether an adapted external answer can continue

## Non-Override Rule

No route priority, KNN confidence, budget pressure, style preference, historical similarity, supplier confidence, or external AI advice can override an arbiter rejection.

If they disagree, the arbiter verdict wins unless the user explicitly corrects the system.

## Why This Is Locked

Without this rule, the system can slowly drift into accepting weak work because another signal is convenient, cheap, confident, or fluent.

This decision keeps quality authority separate from routing, retrieval, cost control, formatting, and external help.

