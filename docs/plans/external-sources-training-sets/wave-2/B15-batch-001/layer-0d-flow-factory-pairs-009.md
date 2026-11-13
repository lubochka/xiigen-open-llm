# Layer 0-D — Flow Factory Pairs (009)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-009-001 — IMPLEMENT+API_CONTRACT (data contracts specific)
Request: "Implement the order service with defined data contracts."
Predicates: ["IMPLEMENT", "API_CONTRACT"]
"data contracts" → API_CONTRACT (contract noun fires regardless of modifier)
Factory: APIContractFlow33 + ImplementationFlow41 | TransitionType: feature_addition

## FF-009-002 — ARCHITECTURE+IMPLEMENT (domain model specific)
Request: "Build the order service with a domain model for order state transitions."
Predicates: ["IMPLEMENT", "ARCHITECTURE"]
"domain model" → ARCHITECTURE (model fires ARCHITECTURE)
Factory: ArchitectureFlow31 + ImplementationFlow41 | TransitionType: feature_addition

## FF-009-003 — IMPLEMENT alone (scaffold specific)
Request: "Scaffold the fulfillment microservice."
Predicates: ["IMPLEMENT"]
Factory: ImplementationFlow41 | TransitionType: feature_addition

## FF-009-004 — IMPLEMENT alone (bootstrap specific)
Request: "Bootstrap the order event consumer service."
Predicates: ["IMPLEMENT"]
Factory: ImplementationFlow41 | TransitionType: feature_addition

## FF-009-005 — Four predicate: ARCH+API+IMPL+TEST
Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "TEST"]
Factory: ArchitectureFlow31 + APIContractFlow33 + ImplementationFlow41 + TestingFlow
