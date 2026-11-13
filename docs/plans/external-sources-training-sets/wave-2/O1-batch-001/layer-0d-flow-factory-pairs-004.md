# Layer 0-D — Flow Factory Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-004-001 — probationary_accepted for 3.3 (ApiContract33FlowAdapter, Gate B passed)

Predicates: ["API_CONTRACT"]
Sub-phase: 3.3
Factory: ApiContract33FlowAdapter | TrustState: probationary_accepted | GateBPassed: true

Notes:
  This pair teaches the probationary_accepted state: Gate B passed but full promotion to
  trusted_learned is pending production evidence threshold.
  Gate B passed: ApiContract33FlowAdapter produced correct API contract output for O1 pilot runs.
  Promotion threshold: 10 production runs with precision >= 0.85 (not yet met).
  The adapter is used in production (unlike probationary_rejected which falls back to generic).
  FactoryState in output: probationary_accepted.

---

## FF-004-002 — Distinction: probationary_accepted vs trusted_learned

Predicates: ["SECURITY"] → trusted_learned | Predicates: ["API_CONTRACT"] → probationary_accepted

Comparison:
  trusted_learned (3.4 / SecurityVerification34FlowAdapter):
    - Gate B passed
    - Production evidence threshold met
    - Fully registered in the learned flow registry
    - No fallback needed

  probationary_accepted (3.3 / ApiContract33FlowAdapter):
    - Gate B passed
    - Production evidence threshold NOT YET met
    - Used in production but under monitoring
    - No fallback (unlike probationary_rejected)

  probationary_rejected (5.1 / UnitTestDesign51FlowAdapter):
    - Gate B NOT passed
    - Not used in production
    - Falls back to generic nine-node flow

---

## FF-004-003 — API_CONTRACT+AUTHORIZATION → probationary_accepted (3.3) + trusted_learned (3.4)

Predicates: ["API_CONTRACT", "AUTHORIZATION"]
Sub-phase: 3.3 + 3.4
Factory 1: ApiContract33FlowAdapter (3.3) | TrustState: probationary_accepted | GateBPassed: true
Factory 2: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned | GateBPassed: true

Notes:
  Both factories are used in production. ApiContract33FlowAdapter is under monitoring;
  SecurityVerification34FlowAdapter is fully trusted.

---

## FF-004-004 — API_CONTRACT+SECURITY+AUTHORIZATION → all three factory states visible

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"]
Sub-phase: 3.4 + 3.3
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: probationary_accepted

Notes:
  This combination shows both trust levels in a single orchestration. The 3.4 factory
  is fully trusted; the 3.3 factory is probationary but accepted. No generic fallback needed
  for either — both produce results directly.
