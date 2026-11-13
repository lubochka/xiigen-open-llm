# Layer 0-C — Knowledge Edge Pairs (046)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-046-001 — MessageQueue: gap vocabulary — queue runtime operations

Confirmed gap vocabulary for message queue runtime operations (zero predicates):
  Action verbs: send, publish (message), consume, receive, poll, subscribe, unsubscribe, acknowledge (message ACK), reject, nack, replay, drain, purge, dead-letter (verb), requeue
  Queue runtime nouns: message payload, offset, partition (runtime), consumer group, subscription filter, DLQ message, backpressure

---

## B14-edge-046-002 — MessageQueue: disambiguation — "publish" as API_CONTRACT vs operational

"publish (API)" → fires API_CONTRACT: "publish the event bus API for external consumers" → API_CONTRACT
"publish (message)" → gap: "publish a message to the Kafka topic" → gap (operational event publishing)
Key: API_CONTRACT fires when "publish" is in the context of API publication/exposure to consumers (service contract); not when it means sending an event message at runtime.

---

## B14-edge-046-003 — MessageQueue: SECURITY boundary — queue encryption compound phrases

SECURITY fires for:
  "TLS for Kafka broker connections", "SASL (Simple Authentication and Security Layer) credential rotation", "encrypt SQS messages at rest with KMS", "rotate SASL credentials"
  "drain the SQS queue" → gap | "purge dead-letter messages" → gap

---

## B14-edge-046-004 — MessageQueue: API_CONTRACT boundary — message schema contracts

API_CONTRACT fires for:
  "define the message queue contract between services" → API_CONTRACT
  "specify the event schema contract" → API_CONTRACT
  "expose queue endpoint as service interface" → API_CONTRACT
  "send a message to the queue" → gap | "consume from the topic" → gap
