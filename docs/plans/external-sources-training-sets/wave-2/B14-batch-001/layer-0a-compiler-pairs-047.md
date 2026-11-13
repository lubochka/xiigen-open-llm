# Layer 0-A — Compiler Pairs (047)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-047-001 — EventBusConfig: ARCHITECTURE fires on event bus design vocabulary

"architect the event bus topology for the microservices platform" → ARCHITECTURE ("architect", "topology")
"design the event sourcing architecture with CQRS" → ARCHITECTURE ("design", "architecture") + 3.1 signal ("CQRS", "event sourcing" are domain patterns) → disambiguation needed
"structure the event routing and replay model" → ARCHITECTURE ("structure", "model")
Note: "event sourcing" + "CQRS" push toward 3.1; "topology" + "bus" push toward 3.5; density determines routing.

---

## B14-compiler-047-002 — EventBusConfig: IMPLEMENT fires on event bus IaC artifacts

"implement the EventBridge rule Terraform module" → IMPLEMENT ("implement" + "module")
"write the event bus Helm chart configuration" → IMPLEMENT ("write" + "chart" + "configuration")
"scaffold the SNS (Simple Notification Service) topic and subscription configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-047-003 — EventBusConfig: SECURITY fires on event bus security vocabulary

"enforce event payload encryption at rest and in transit" → SECURITY ("encrypt", "at rest", "in transit")
"rotate EventBridge API keys on 60-day schedule" → SECURITY ("rotate" + "API key")
"configure mTLS for event bus service mesh integration" → SECURITY ("mTLS")

---

## B14-compiler-047-004 — EventBusConfig: API_CONTRACT fires on event schema vocabulary

"define the event contract for the order-created event bus topic" → API_CONTRACT ("define" + "contract")
"specify the CloudEvents (CE) schema for event bus messages" → API_CONTRACT ("specify" + "schema" in API context)
"expose the event bus endpoint as a published API" → API_CONTRACT ("expose" + "publish API")

---

## B14-compiler-047-005 — EventBusConfig: multi-predicate compilation

"Architect the event bus topology, implement the EventBridge Terraform modules, enforce TLS, and define the event schema contracts."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "SECURITY", "API_CONTRACT"]
  CapabilityGapWarnings: gap-devops-cloud-001 ("EventBridge", "SNS", "SQS" are gap platform names)
