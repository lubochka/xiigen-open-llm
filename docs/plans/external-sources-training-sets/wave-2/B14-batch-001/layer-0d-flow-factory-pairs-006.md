# Layer 0-D — Flow Factory Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-006-001 — ARCHITECTURE+TEST+SECURITY → 3 flows

A: ["ARCHITECTURE", "TEST", "SECURITY"]
B: FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, UnitTestingFlow51, SecurityAuditFlow63]
  Teaching: Security architecture validation: design the zero-trust architecture (ARCHITECTURE), write penetration tests (SECURITY+TEST), validate connectivity isolation (TEST).

---

## B14-factory-006-002 — ARCHITECTURE+QA+IMPLEMENT → 3 flows

A: ["ARCHITECTURE", "QA", "IMPLEMENT"]
B: FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, QAGatingFlow73, ImplementationFlow41]
  TransitionType: feature_addition

---

## B14-factory-006-003 — ARCHITECTURE: sub-phase 3.1 vs 3.5 factory selection

A: ["ARCHITECTURE"] | app vocabulary
B: FlowFactory.Create("ARCHITECTURE") → ApplicationArchitectureFlow31 (not 3.5)
  vs ["ARCHITECTURE"] | infra vocabulary
B: FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35
  Teaching: FlowFactory uses vocabulary context to select between 3.1 and 3.5. Infra vocab → 3.5. App/domain vocab → 3.1.

---

## B14-factory-006-004 — ARCHITECTURE+API_CONTRACT+IMPLEMENT → 3 flows

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"]
B: FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, APIContractFlow33, ImplementationFlow41]
  Teaching: API gateway platform: design gateway architecture (3.5), define OpenAPI contracts (3.3), implement routing logic (4.1). Three flows.

---

## B14-factory-006-005 — ARCHITECTURE+AUTHORIZATION+IMPLEMENT → 3 flows

A: ["ARCHITECTURE", "AUTHORIZATION", "IMPLEMENT"]
B: FlowFactory.CreateMulti([...]) → [InfrastructureArchitectureFlow35, AuthorizationFlow, ImplementationFlow41]
  Teaching: RBAC implementation: design RBAC architecture (ARCHITECTURE→3.5), define permission model (AUTHORIZATION), implement RBAC manifests (IMPLEMENT→4.1). Three flows.

