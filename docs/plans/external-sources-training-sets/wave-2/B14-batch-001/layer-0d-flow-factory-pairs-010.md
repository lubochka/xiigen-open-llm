# Layer 0-D — Flow Factory Pairs (010)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-010-001 — ARCHITECTURE: service registry design

A: ["ARCHITECTURE"] | "design the service registry and discovery architecture"
B: [InfrastructureArchitectureFlow35] | TransitionType: documentation

---

## B14-factory-010-002 — ARCHITECTURE: backup/restore topology

A: ["ARCHITECTURE"] | "design the backup and restore architecture"
B: [InfrastructureArchitectureFlow35] | TransitionType: documentation

---

## B14-factory-010-003 — ARCHITECTURE+IMPLEMENT: deploy pipeline

A: ["ARCHITECTURE", "IMPLEMENT"] | "architect the CI/CD pipeline + implement the pipeline steps"
B: [InfrastructureArchitectureFlow35, ImplementationFlow41] | TransitionType: feature_addition

---

## B14-factory-010-004 — ARCHITECTURE+TEST+DOCUMENT: connectivity + docs

A: ["ARCHITECTURE", "TEST", "DOCUMENT"]
B: [InfrastructureArchitectureFlow35, UnitTestingFlow51, DocumentationFlow62]
  TransitionType: documentation (IMPLEMENT absent)

---

## B14-factory-010-005 — ARCHITECTURE+AUTHORIZATION+SECURITY: identity platform

A: ["ARCHITECTURE", "AUTHORIZATION", "SECURITY"]
B: [InfrastructureArchitectureFlow35, AuthorizationFlow, SecurityAuditFlow63]
  Teaching: Identity and access management platform design fires all three: ARCHITECTURE (structure), AUTHORIZATION (permission model), SECURITY (enforcement audit).

