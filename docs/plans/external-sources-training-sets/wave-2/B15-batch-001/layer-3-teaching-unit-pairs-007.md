# Layer 3 — Teaching Unit Pairs (007)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: teaching_unit

## TU-007 — probationary_accepted vs probationary_rejected in nine-node flow

Gate B (Node 2/3 in nine-node flow) assesses implementation scope feasibility:

probationary_accepted: Scope is valid, constraints are achievable, interfaces are defined correctly.
→ Flow continues normally to Node 4 (implementation planning).

probationary_rejected: Scope is out of range, constraint violations, or infeasible requirements.
→ Fallback scope proposed at Node 4. If fallback accepted, flow continues with narrowed scope.

Example of rejection trigger: "guaranteed delivery to 47 real-time consumers from single service"
→ Out of scope for one service; requires event bus infrastructure.
Fallback: Publish to IEventBus interface; delivery = bus responsibility.
