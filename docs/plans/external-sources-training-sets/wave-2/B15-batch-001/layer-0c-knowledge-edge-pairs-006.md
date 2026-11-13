# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: knowledge_edge

## KE-006-001 — "probationary_accepted" vs "probationary_rejected"
probationary_accepted: Gate B passed → ImplementationFlow41 executes normally
probationary_rejected: Gate B failed → fallback used (service cannot be built as specified; rework required)
Both statuses are valid outcomes of the nine-node flow.

## KE-006-002 — OperationResult<T> in B15 service implementations
All public methods in B15 service implementations return OperationResult<T>.
Never throw for business logic. Never return null. Never return bool without context.
This applies to: order creation, status updates, fulfillment logic, payment processing.

## KE-006-003 — BaseService pattern in B15
All B15 services requiring session persistence inherit BaseService(ISessionStorage storage).
Order state, fulfillment records, payment logs → ISessionStorage.
Fabric-first: inject interfaces, not concrete types.

## KE-006-004 — v6 corruption types for B15
v6-01: Invented predicates (ORDER_PROCESSING, FULFILLMENT, PAYMENT — none exist in nine-predicate lexicon)
v6-02: Missed API_CONTRACT ("data contracts" not caught), missed ARCHITECTURE ("domain model" not caught)
v6-03: Multi-predicate collapse (four predicates → one IMPLEMENT)
