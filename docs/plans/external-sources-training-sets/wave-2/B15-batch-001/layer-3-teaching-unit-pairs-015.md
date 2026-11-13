# Layer 3 — Teaching Unit Pairs (015)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-015-A — "bounded context" fires ARCHITECTURE

A: "Build the inventory allocation service within the inventory bounded context." Which predicates?
B: ["IMPLEMENT", "ARCHITECTURE"]. "bounded context" is a DDD (Domain-Driven Design) term and is a first-class ARCHITECTURE trigger, equivalent to "domain model". Two predicates → two routing decisions: ImplementationFlow41 + ArchitectureFlow31.

## TU-015-B — "data contracts" for reservation fires API_CONTRACT

A: "Implement the inventory allocation service and define the data contracts for stock reservation." Which predicates?
B: ["IMPLEMENT", "API_CONTRACT"]. "data contracts" → API_CONTRACT fires on "contract". The modifier "data" does not suppress the trigger. This is the canonical v6-02 test case.

## TU-015-C — "concurrent reservation" does not fire SECURITY; "harden" does

A: "Create the inventory allocation service handling concurrent reservation." How many predicates?
B: One: ["IMPLEMENT"]. "concurrent" is a non-functional requirement noun — not a SECURITY trigger. Now: "Create the inventory allocation service and harden it against race conditions." → Two: ["IMPLEMENT", "SECURITY"]. "harden" is the trigger.

## TU-015-D — Idempotent reservation: which business rule?

A: Why is inventory reservation idempotent and how is it enforced?
B: BR-ALLOC-002: reservation ID is a deterministic UUID(v5) computed from OrderId + ProductId. If the same order+product combination is submitted twice, the same AllocationId is returned (no duplicate write). This is the same idempotency pattern as BR-ORD-001 in the order service.

## TU-015-E — OperationResult<T> on expiry job

A: InventoryAllocationService has an ExpireStale method that runs as a background job. Should it return OperationResult<T>?
B: Yes. Rule 1 applies to all public methods regardless of invocation context. If ExpireStale is a public method (even on a background service), it returns OperationResult<int> (count of records released). Background jobs calling the method handle OperationResult failures by logging and continuing.
