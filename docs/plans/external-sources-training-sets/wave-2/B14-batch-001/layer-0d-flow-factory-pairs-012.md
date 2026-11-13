# Layer 0-D — Flow Factory Pairs (012)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B14-factory-012-001 — ARCHITECTURE+TEST: infra validation

A: ["ARCHITECTURE", "TEST"] | "architect the VPC peering + validate connectivity"
B: [InfrastructureArchitectureFlow35, UnitTestingFlow51]
  TransitionType: documentation (IMPLEMENT absent)

---

## B14-factory-012-002 — ARCHITECTURE+SECURITY+DOCUMENT: threat model documentation

A: ["ARCHITECTURE", "SECURITY", "DOCUMENT"] | "design the threat model + document the security architecture decisions"
B: [InfrastructureArchitectureFlow35, SecurityAuditFlow63, DocumentationFlow62]
  TransitionType: documentation. Threat model documentation: SECURITY fires on "threat model", DOCUMENT on "document", ARCHITECTURE on "architecture decisions".

---

## B14-factory-012-003 — ARCHITECTURE+IMPLEMENT+SECURITY: enforce at implementation

A: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"] | "design the network boundary + implement the firewall rules + enforce TLS"
B: [InfrastructureArchitectureFlow35, ImplementationFlow41, SecurityAuditFlow63]
  TransitionType: feature_addition. Three flows.

---

## B14-factory-012-004 — ARCHITECTURE+API_CONTRACT+DOCUMENT: API platform design

A: ["ARCHITECTURE", "API_CONTRACT", "DOCUMENT"] | "design the API gateway + define OpenAPI contracts + document the consumer guide"
B: [InfrastructureArchitectureFlow35, APIContractFlow33, DocumentationFlow62]
  TransitionType: documentation (no IMPLEMENT)

---

## B14-factory-012-005 — Deduplication: two "design" verbs → one ARCHITECTURE

A: "Design the topology and design the failover boundaries."
  Two "design" verbs → still one ARCHITECTURE predicate (set deduplication)
  CompiledPredicateIds: ["ARCHITECTURE"] (not ["ARCHITECTURE", "ARCHITECTURE"])
  B: [InfrastructureArchitectureFlow35] — one flow
  Teaching: Predicates are a set, not a multiset. Multiple triggers for the same predicate → one predicate.

