# Layer 0-D — Flow Factory Pairs (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-003-001 — ARCHITECTURE+TEST → 2 flows

A: ["ARCHITECTURE", "TEST"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","TEST"]) → [InfrastructureArchitectureFlow35, UnitTestingFlow51]
  TransitionType: documentation (IMPLEMENT absent)

---

## B14-factory-003-002 — ARCHITECTURE+QA → 2 flows

A: ["ARCHITECTURE", "QA"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","QA"]) → [InfrastructureArchitectureFlow35, QAGatingFlow73]
  Both flows independent. QA sign-off on architecture design.

---

## B14-factory-003-003 — ARCHITECTURE+API_CONTRACT → 2 flows

A: ["ARCHITECTURE", "API_CONTRACT"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","API_CONTRACT"]) → [InfrastructureArchitectureFlow35, APIContractFlow33]
  Teaching: API gateway architecture (ARCHITECTURE → 3.5) + OpenAPI contract definition (API_CONTRACT → 3.3). Both created.

---

## B14-factory-003-004 — ARCHITECTURE+AUTHORIZATION → 2 flows

A: ["ARCHITECTURE", "AUTHORIZATION"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","AUTHORIZATION"]) → [InfrastructureArchitectureFlow35, AuthorizationFlow]
  Teaching: RBAC architecture design creates both ARCHITECTURE flow (structural design) and AUTHORIZATION flow (permission model).

---

## B14-factory-003-005 — ARCHITECTURE+IMPLEMENT+TEST → 3 flows

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
B: FlowFactory.CreateMulti(["ARCHITECTURE","IMPLEMENT","TEST"]) → [InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51]
  TransitionType: feature_addition (IMPLEMENT present)
  Teaching: IaC workflow creates three concurrent flows.

