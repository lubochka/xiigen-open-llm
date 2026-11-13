# Layer 0-D — Flow Factory Pairs (008)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-008-001 — ARCHITECTURE+IMPLEMENT: Terraform IaC

A: ["ARCHITECTURE", "IMPLEMENT"] | context: "architect the IaC topology + implement Terraform modules"
B: [InfrastructureArchitectureFlow35, ImplementationFlow41] | TransitionType: feature_addition
  Teaching: IaC workflows are the canonical ARCHITECTURE+IMPLEMENT pairing.

---

## B14-factory-008-002 — ARCHITECTURE+TEST: connectivity validation

A: ["ARCHITECTURE", "TEST"] | context: "architect the network topology + validate connectivity"
B: [InfrastructureArchitectureFlow35, UnitTestingFlow51]
  Teaching: Infrastructure tests (connectivity assertions, provisioning validation) pair ARCHITECTURE with TEST.

---

## B14-factory-008-003 — ARCHITECTURE+DOCUMENT: C4 diagram

A: ["ARCHITECTURE", "DOCUMENT"] | context: "produce a C4 diagram"
B: [InfrastructureArchitectureFlow35, DocumentationFlow62]
  Teaching: C4 diagrams are architectural documentation: ARCHITECTURE (design artifact) + DOCUMENT (written output).

---

## B14-factory-008-004 — ARCHITECTURE+SECURITY: zero-trust design + audit

A: ["ARCHITECTURE", "SECURITY"] | context: "design zero-trust + audit existing network boundaries"
B: [InfrastructureArchitectureFlow35, SecurityAuditFlow63]
  Teaching: Zero-trust architecture design fires ARCHITECTURE; auditing existing boundary implementation fires SECURITY.

---

## B14-factory-008-005 — ARCHITECTURE+QA: architecture review gate

A: ["ARCHITECTURE", "QA"] | context: "design infrastructure + QA sign-off before production"
B: [InfrastructureArchitectureFlow35, QAGatingFlow73]
  Teaching: Architecture sign-off requests fire QA → 7.3 alongside ARCHITECTURE → 3.5. Independent flows.

