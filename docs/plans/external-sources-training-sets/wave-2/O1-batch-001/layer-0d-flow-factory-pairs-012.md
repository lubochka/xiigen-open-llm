# Layer 0-D — Flow Factory Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-012-001 — Complete factory map for O1

Reference pair — full factory registry for O1 scenario.

| Sub-phase | Factory name | TrustState | Gate B | Notes |
|---|---|---|---|---|
| 3.4 | SecurityVerification34FlowAdapter | trusted_learned | passed | Primary for SECURITY+AUTHORIZATION |
| 3.3 | ApiContract33FlowAdapter | probationary_accepted | passed | Production evidence pending |
| 5.1 | UnitTestDesign51FlowAdapter | probationary_rejected | failed | Generic fallback active |
| 2.1 | RequirementsAuthoringFlow21 | deterministic_generic | N/A | Permanent baseline |
| 4.1 | ImplementationFlow41 | deterministic_generic | N/A | Permanent baseline |
| 7.x | QAGatingFlow73 | deterministic_generic | N/A | Permanent baseline |
| 3.x | ArchitectureFlow | deterministic_generic | N/A | Scenario-specific name |

---

## FF-012-002 — Factory selection summary: predicate → sub-phase → factory

Reference pair — quick-reference mapping.

SECURITY → 3.4 → SecurityVerification34FlowAdapter (trusted_learned)
AUTHORIZATION → 3.4 → SecurityVerification34FlowAdapter (trusted_learned)
SECURITY+AUTHORIZATION → 3.4 → SecurityVerification34FlowAdapter (one selection, two evidence predicates)
API_CONTRACT → 3.3 → ApiContract33FlowAdapter (probationary_accepted)
TEST → 5.1 → generic nine-node (UnitTestDesign51FlowAdapter probationary_rejected, not used)
DOCUMENT → 2.1 → RequirementsAuthoringFlow21 (deterministic_generic)
IMPLEMENT → 4.1 → ImplementationFlow41 (deterministic_generic)
QA → 7.x → QAGatingFlow73 (deterministic_generic)

---

## FF-012-003 — Gate B definitions for O1 factories

Reference pair — Gate B assertion for each factory.

SecurityVerification34FlowAdapter (3.4):
  Gate B: precision >= 0.85 on 20-scenario JWT+tenant security evaluation set
  Status: PASSED (precision: 0.91) → trusted_learned

ApiContract33FlowAdapter (3.3):
  Gate B: correct OpenAPI contract generation for >= 8 of 10 O1 API contract scenarios
  Status: PASSED (8/10 correct) → probationary_accepted (production evidence pending)

UnitTestDesign51FlowAdapter (5.1):
  Gate B: precision >= 0.85 on 15 JWT authentication unit test scenarios
  Status: FAILED (precision: 0.71) → probationary_rejected

RequirementsAuthoringFlow21, ImplementationFlow41, QAGatingFlow73 (deterministic_generic):
  Gate B: N/A — generic flows are not subject to Gate B evaluation

---

## FF-012-004 — v6-01 factory corruption summary

Reference pair — v6-01 invented factory patterns to reject.

Invented factory names that must never appear in O1 output:
  JwtAuthFlow, JwtAuthFlowAdapter
  TenantIsolationFlow, TenantIsolationFlowAdapter
  MultiFactorFlow, MfaFlow, MfaFlowAdapter
  SessionBlacklistFlow, SessionBlacklistFlowAdapter
  TenantScopedFlow, TenantAwareFlow
  OAuthFlow (differs from SecurityVerification34FlowAdapter)
  RbacFlow, RbacFlowAdapter

Detection rule: any factory name not in the FF-012-001 registry table is a v6-01 invented factory.
The factory registry is closed — new factories require a batch registration process,
not ad-hoc creation by the model.
