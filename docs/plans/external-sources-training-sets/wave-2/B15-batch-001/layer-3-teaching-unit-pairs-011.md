# Layer 3 — Teaching Unit Pairs (011)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-011 — v6 corruption types for B15

v6-01 (Invented predicates):
- "Implement order processing" → ["ORDER_PROCESSING"] ✗
- "Build payment handler" → ["PAYMENT"] ✗
- "Create fulfillment service" → ["FULFILLMENT"] ✗
Fix: Only nine canonical predicates. ORDER_PROCESSING, PAYMENT, FULFILLMENT do not exist.

v6-02 (Missed predicate):
- "data contracts" → API_CONTRACT not fired ✗
- "domain model" → ARCHITECTURE not fired ✗
Fix: "contract" keyword always fires API_CONTRACT; "domain model" always fires ARCHITECTURE.

v6-03 (Predicate collapse):
- Four predicates → one IMPLEMENT ✗
Fix: Each action verb fires its own predicate. No collapsing permitted.
