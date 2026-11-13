# Selected follow-up work order

Work order: QUALITY-PRODUCT-ORCHESTRATION-FULL-HOST-TOKEN-READINESS-REPAIR-01

Build OrchestrationReasoning trained source-disjoint runtime eval with ablation, then prove full Claude/Cursor/Codex host runtime readiness and token reduction with quality gates before any product_ready=true claim.

## Required scope

- OrchestrationReasoning train-ready approval or explicit no-training gap packet
- source-disjoint held-out runtime eval for OrchestrationReasoning
- row/scenario/pair/exact-prompt/output-hash/candidate-order/fixture lookup leakage checks
- fresh-load and continue-training probes for every trained target being claimed
- full host runtime evidence for Claude, Cursor, and Codex or explicit host-layer gap matrix
- token reduction with quality gates on the same pairs

## Forbidden until separate authority

- production locator mutation
- checkpoint promotion
- external AI/web/model calls
- product_ready=true claim
