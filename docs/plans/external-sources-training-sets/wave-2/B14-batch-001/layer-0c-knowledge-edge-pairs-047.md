# Layer 0-C — Knowledge Edge Pairs (047)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-047-001 — EventBusConfig: disambiguation — "event sourcing" architecture → 3.1 vs 3.5

"Design the event sourcing architecture with CQRS (Command Query Responsibility Segregation) and aggregate boundaries."
  domain vocabulary: "event sourcing", "CQRS", "aggregate" → 3.1 signals
  infra vocabulary: none significant
  → ARCHITECTURE fires → routes to 3.1 (domain vocabulary dominates)

"Architect the event bus topology for the microservices platform."
  infra vocabulary: "event bus", "topology", "microservices platform" → 3.5 signals
  → ARCHITECTURE fires → routes to 3.5

Rule: "event sourcing" + "CQRS" are domain-level patterns → 3.1. "Event bus" + "topology" are infrastructure → 3.5.

---

## B14-edge-047-002 — EventBusConfig: gap vocabulary — event bus runtime operations

Confirmed gap vocabulary for event bus runtime operations (zero predicates):
  Action verbs: publish (event message), consume, subscribe, listen, poll, relay, replay, forward, route (runtime), filter (event filter), transform (event transform)
  Runtime nouns: event payload, event envelope, routing key, event subscription, event listener, event handler

---

## B14-edge-047-003 — EventBusConfig: SECURITY boundary — event bus encryption

SECURITY fires for:
  "encrypt event payload at rest and in transit", "rotate EventBridge API keys", "configure mTLS for event bus service mesh", "TLS for event bus endpoints"
  "replay events" → gap | "filter events" → gap

---

## B14-edge-047-004 — EventBusConfig: API_CONTRACT boundary — event schema contracts

API_CONTRACT fires for:
  "define the event contract" → API_CONTRACT | "specify the CloudEvents schema" → API_CONTRACT
  "expose event bus endpoint as published API" → API_CONTRACT
  "publish event to bus" → gap (operational event publishing)
  "define event routing rules" (operational routing config) → gap; "define event contract" (schema/contract) → API_CONTRACT
