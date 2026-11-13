# Layer 0-A — Compiler Pairs (046)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-046-001 — MessageQueue: ARCHITECTURE fires on queue design vocabulary

"architect the message queue topology for the event-driven platform" → ARCHITECTURE ("architect", "topology")
"design the pub/sub (publish/subscribe) message routing architecture" → ARCHITECTURE ("design", "architecture", "pub/sub" = architectural pattern)
"structure the dead-letter queue and retry boundary" → ARCHITECTURE ("structure", "boundary")
"send a message to the queue" → gap ("send" is operational gap)

---

## B14-compiler-046-002 — MessageQueue: IMPLEMENT fires on queue IaC artifacts

"implement the Kafka cluster Helm chart" → IMPLEMENT ("implement" + "chart")
"write the SQS (Simple Queue Service) queue Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the RabbitMQ StatefulSet configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-046-003 — MessageQueue: SECURITY fires on queue encryption vocabulary

"enforce TLS for all Kafka broker connections" → SECURITY ("TLS")
"encrypt SQS messages at rest with KMS" → SECURITY ("encrypt", "at rest", "key")
"rotate SASL (Simple Authentication and Security Layer) credentials for Kafka" → SECURITY ("rotate" credentials)

---

## B14-compiler-046-004 — MessageQueue: AUTHORIZATION fires on queue access control

"grant produce/consume IAM roles to message publisher service accounts" → AUTHORIZATION ("grant", "IAM role", "service accounts")
"restrict Kafka topic access via ACL (Access Control List) RoleBinding" → AUTHORIZATION ("restrict", "RoleBinding" in access context)

---

## B14-compiler-046-005 — MessageQueue: API_CONTRACT fires on queue interface vocabulary

"define the message queue contract between OrderService and InventoryService" → API_CONTRACT ("define" + "contract" in interface context)
"specify the event schema contract for the order-placed topic" → API_CONTRACT ("specify" + "schema" + "contract")
"expose the queue endpoint as a service interface" → API_CONTRACT ("expose" + "service interface")
