# Layer 0-D — Flow Factory Pairs (048)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-048-001 — ARCHITECTURE+SECURITY combo: FlowFactory for 2-predicate

FlowFactory.CreateMulti(["ARCHITECTURE", "SECURITY"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → SecurityAuditFlow63 (6.3)
  "Design the zero-trust network topology and enforce mTLS"
  FlowCount: 2 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3

---

## B14-factory-048-002 — ARCHITECTURE+SECURITY+AUTHORIZATION: FlowFactory for 3-predicate

FlowFactory.CreateMulti(["ARCHITECTURE", "SECURITY", "AUTHORIZATION"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → SecurityAuditFlow63 (6.3)
  → AuthorizationFlow (authorization sub-phase)
  FlowCount: 3 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → AUTHORIZATION

---

## B14-factory-048-003 — ARCHITECTURE+SECURITY: FlowFactory for SECURITY alone

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (6.3) | TrustState: deterministic_generic
  "Enforce zero-trust deny-all baseline" (SECURITY fires from zero-trust compound phrase)
  FlowCount: 1 | TransitionType: feature_addition

---

## B14-factory-048-004 — ARCHITECTURE+SECURITY: negated IMPLEMENT does not create flow

"Design the service mesh topology without implementing IaC."
  CompiledPredicateIds: ["ARCHITECTURE"] (negated IMPLEMENT = no IMPLEMENT predicate)
  FlowFactory.Create("ARCHITECTURE") → InfrastructureArchitectureFlow35 (3.5)
  FlowCount: 1 | TransitionType: feature_addition
  Rule: Negated action verbs produce no flow for the negated predicate.
