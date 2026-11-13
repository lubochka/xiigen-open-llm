# Layer 0-B — Router Pairs (047)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-047-001 — EventBusConfig: routing for ARCHITECTURE → 3.5 vs 3.1

"Architect the event bus topology for the microservices platform."
  infra vocabulary ("topology", "event bus", "platform") → 3.5 (not 3.1)
  ARCHITECTURE → 3.5 | TransitionType: feature_addition

"Design the event sourcing architecture with CQRS and aggregate boundaries."
  domain vocabulary ("event sourcing", "CQRS", "aggregate") > infra vocabulary → 3.1 (not 3.5)
  ARCHITECTURE → 3.1 | TransitionType: feature_addition

---

## B14-router-047-002 — EventBusConfig: routing for ARCHITECTURE + IMPLEMENT + SECURITY + API_CONTRACT

"Architect the event bus topology, implement EventBridge Terraform modules, enforce TLS, and define event schema contracts."
  ARCHITECTURE → 3.5 | IMPLEMENT → 4.1 | SECURITY → 6.3 | API_CONTRACT → 2.1
  Sequential: 3.5 → 2.1 → 6.3 → 4.1 | TransitionType: feature_addition
  AllSubPhaseIds: [3.5, 2.1, 4.1, 6.3]

---

## B14-router-047-003 — EventBusConfig: routing for API_CONTRACT alone

"Specify the CloudEvents schema contract for the order-created event."
  API_CONTRACT → 2.1 | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-047-004 — EventBusConfig: gap routing

"Publish an event to the EventBridge bus."
  Zero predicates → GapHandler | "publish" in operational event context is gap (vs "publish API" which fires API_CONTRACT)
  CapabilityGapWarnings: gap-devops-cloud-001
  Note: "publish (API)" fires API_CONTRACT; "publish (event message)" is operational gap — context determines routing.
