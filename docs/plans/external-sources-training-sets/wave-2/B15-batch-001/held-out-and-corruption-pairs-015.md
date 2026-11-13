# Held-Out and Corruption Pairs (015)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-015-A — Held-out: InventoryAllocationService bounded context

A: "Create the inventory allocation service within the inventory bounded context."
B: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | feature_addition | "bounded context" fires ARCHITECTURE

## HC-015-B — Held-out: "concurrent reservation" does not fire SECURITY

A: "Create the inventory allocation service handling concurrent reservation requests."
B: Predicates: ["IMPLEMENT"] | "concurrent" is not a SECURITY trigger.

## HC-015-C — Held-out: "harden against race conditions" fires SECURITY

A: "Create the inventory allocation service and harden it against race conditions."
B: Predicates: ["IMPLEMENT", "SECURITY"] | "harden" → SECURITY fires.

## HC-015-D — Corruption v6-02: "bounded context" not caught as ARCHITECTURE

v6 corrupt: "Build the inventory allocation service within the inventory bounded context" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | "bounded context" is a first-class ARCHITECTURE trigger.
Error: v6-02. "bounded context" must fire ARCHITECTURE same as "domain model".

## HC-015-E — Corruption v6-03: IMPLEMENT+API_CONTRACT+SECURITY collapsed

v6 corrupt: "Build the inventory allocation service, define the reservation API contracts, and secure the allocation endpoint" → DecisionCount: 1
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | DecisionCount: 3
Error: v6-03 multi-branch collapse.
