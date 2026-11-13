# Layer 0-D — Flow Factory Pairs (059)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-059-001 — ZeroTrustConfig: FlowFactory for ARCHITECTURE + SECURITY

FlowFactory.CreateMulti(["ARCHITECTURE", "SECURITY"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → SecurityAuditFlow63 (6.3)
  "Architect the zero-trust network architecture and enforce deny-all mTLS"
  FlowCount: 2 | TransitionType: feature_addition | Sequential: 3.5 → 6.3

---

## B14-factory-059-002 — ZeroTrustConfig: FlowFactory for 5 predicates

FlowFactory.CreateMulti(["ARCHITECTURE", "SECURITY", "AUTHORIZATION", "IMPLEMENT", "DOCUMENT"]):
  → InfrastructureArchitectureFlow35 (3.5)
  → SecurityAuditFlow63 (6.3)
  → AuthorizationFlow (authorization sub-phase)
  → BackendServiceFlow41 (4.1)
  → DocumentationFlow62 (6.2)
  FlowCount: 5 | TransitionType: feature_addition
  Sequential: 3.5 → 6.3 → AUTHORIZATION → 4.1 → 6.2

---

## B14-factory-059-003 — ZeroTrustConfig: FlowFactory for SECURITY alone (zero-trust compound phrase)

FlowFactory.Create("SECURITY"):
  → SecurityAuditFlow63 (6.3) | TransitionType: feature_addition
  "Enable zero-trust mode on the Istio mesh" — SECURITY fires from zero-trust compound phrase
  FlowCount: 1 | TrustState: deterministic_generic
  Note: Even with gap verb "enable", SECURITY fires from zero-trust compound → 1 flow created.

---

## B14-factory-059-004 — ZeroTrustConfig: FlowFactory zero predicates

FlowFactory.Create([]) → GapHandler ONLY when "zero-trust" is negated:
  "Disable zero-trust for the development namespace" → zero predicates (negation of zero-trust compound)
  CapabilityGapWarnings: gap-devops-cloud-001
  All other "zero-trust" mentions (positive assertions) fire SECURITY → minimum 1 flow.
