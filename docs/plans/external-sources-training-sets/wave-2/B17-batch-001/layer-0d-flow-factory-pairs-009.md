# Layer 0-D — Flow Factory Pairs (009)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: flow-factory

---

## B17-0D-009-001 — Five-predicate full factory map

**Scenario:** EnterpriseIdentityBridge — Auth0 OAuth connector with full security, authorization, and test coverage.

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
ConnectorName: EnterpriseIdentityBridge
TransitionType: feature_addition
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "probationary_accepted" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "AuthorizationFlow34", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" }
]
DecisionCount: 5
UniqueSubPhases: 4 (4.3, 3.3, 3.4, 5.1)
```

---

## B17-0D-009-002 — Five-flow execution ordering

**Execution ordering:**
```
1. IntegrationConnectorFlow43 (4.3) — build the connector first
2. SecurityAuditFlow34 (3.4) — security review while building
   ‖ AuthorizationFlow34 (3.4) — parallel: OAuth flow design concurrent with security
   ‖ APIContractFlow33 (3.3) — parallel: API contract definition
3. TestingFlow51 (5.1) — tests run after connector is built (depends on 4.3)
```

**Parallel window:** SecurityAuditFlow34, AuthorizationFlow34, and APIContractFlow33 can all proceed in parallel after 4.3 begins.

**Sequential dependency:** TestingFlow51 (5.1) depends on IntegrationConnectorFlow43 (4.3) completing.

---

## B17-0D-009-003 — Factory status breakdown for five flows

| Flow | Sub-phase | Factory status | Reason |
|---|---|---|---|
| IntegrationConnectorFlow43 | 4.3 | probationary_accepted | Candidate flow, Gate B passed |
| APIContractFlow33 | 3.3 | deterministic_generic | Standard API contract process |
| SecurityAuditFlow34 | 3.4 | deterministic_generic | Standard security audit |
| AuthorizationFlow34 | 3.4 | deterministic_generic | Standard OAuth review |
| TestingFlow51 | 5.1 | deterministic_generic | Standard test scaffolding |

**Only IntegrationConnectorFlow43 has a probationary/learned status.** The others are generic deterministic flows.

---

## B17-0D-009-004 — Five-predicate: complete DecisionCount breakdown

```
IMPLEMENT → 1 decision
API_CONTRACT → 1 decision
SECURITY → 1 decision
AUTHORIZATION → 1 decision
TEST → 1 decision
─────────────────────
Total DecisionCount: 5

Unique sub-phases: 4.3, 3.3, 3.4, 5.1 = 4 unique sub-phases
Note: 3.4 is counted once as a unique sub-phase even though 2 decisions route there.
```
