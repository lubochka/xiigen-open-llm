# Layer 0-D — Flow Factory Pairs (011)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-011-001 — probationary_rejected with detailed rejection reason (Gate B insufficient)

Predicates: ["TEST"]
Sub-phase: 5.1
Factory: UnitTestDesign51FlowAdapter | TrustState: probationary_rejected | GateBPassed: false

Rejection details:
  Gate B assertion: minimum precision >= 0.85 on test case coverage for JWT token scenarios
  Actual precision achieved: 0.71 (below threshold)
  Evaluation set: 15 JWT authentication unit test scenarios
  Failure reason: adapter produced incomplete test skeletons — missing boundary assertions
    for token expiry edge cases and tenant isolation boundary conditions
  Decision: probationary_rejected; generic nine-node fallback used for 5.1

---

## FF-011-002 — Promotion path from probationary_rejected to probationary_accepted

Reference pair — what is needed to promote UnitTestDesign51FlowAdapter.

Current state: probationary_rejected
Promotion requirement:
  1. Fix the test skeleton generation for token expiry edge cases
  2. Fix the tenant isolation boundary assertion coverage
  3. Re-run Gate B evaluation: must achieve precision >= 0.85 on the standard 15-scenario set
  4. If Gate B passes → TrustState changes to probationary_accepted (not directly to trusted_learned)
  5. After 10 production runs with precision >= 0.85 → promote to trusted_learned

Until promotion: generic nine-node fallback remains active for 5.1 in O1 scenario.

---

## FF-011-003 — Promotion path from probationary_accepted to trusted_learned (for 3.3)

Reference pair — what is needed to promote ApiContract33FlowAdapter.

Current state: probationary_accepted (Gate B passed, production evidence pending)
Promotion requirement:
  1. Gate B already passed for ApiContract33FlowAdapter
  2. Accumulate 10 production runs with precision >= 0.85 on O1 API contract scenarios
  3. No Gate B re-evaluation needed — it already passed
  4. When production threshold met → TrustState changes to trusted_learned

Current production count: 3 of 10 (as of 2026-05-05)
Projected promotion: batch O1-batch-003 or later

---

## FF-011-004 — Factory state transition diagram

Reference pair — valid TrustState transitions.

Valid TrustState transitions:
  not_registered → probationary_rejected (Gate B fails on first evaluation)
  not_registered → probationary_accepted (Gate B passes on first evaluation)
  probationary_rejected → probationary_accepted (re-evaluation after fix; Gate B passes)
  probationary_accepted → trusted_learned (production evidence threshold met)
  trusted_learned → probationary_rejected (regression detected in production; demoted)
  deterministic_generic — stable state; no promotion path (generic flows are not promoted)

Invalid transitions:
  probationary_rejected → trusted_learned (must pass Gate B first → probationary_accepted)
  probationary_accepted → probationary_rejected (regression path exists but requires demotion event)

---

## FF-011-005 — Factory state impact on orchestrator behavior

Reference pair — how TrustState affects the orchestrator.

trusted_learned: specialized adapter used; orchestrator trusts output fully
probationary_accepted: specialized adapter used; orchestrator monitors precision metrics
probationary_rejected: generic fallback used; orchestrator logs adapter rejection event
deterministic_generic: generic flow used; no monitoring needed (baseline behavior)
not_registered: no factory found; orchestrator treats as unroutable_transition

Orchestrator behavior difference:
  trusted_learned vs probationary_accepted: identical functional behavior; only monitoring differs
  probationary_rejected vs deterministic_generic: both use generic flow; rejected state is logged
    as an ongoing quality issue; generic state is the permanent baseline (no quality issue logged)
