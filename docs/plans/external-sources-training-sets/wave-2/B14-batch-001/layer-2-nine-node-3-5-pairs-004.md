# Layer 2 — Nine-Node Flow Pairs (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: nine_node_flow (InfrastructureArchitectureFlow35)

---

## B14-nine-004 — Message broker topology for EventBus

Request: "Architect the message broker topology for the EventBus — define consumer group structure, dead-letter queue policy, and partition strategy for the OrderCreated and PaymentProcessed events."

### Flow: InfrastructureArchitectureFlow35

**Node 1 — Input parsing**
  Target: EventBus message broker | Events: OrderCreated, PaymentProcessed
  Architecture class: event streaming topology

**Node 2 — Requirements extraction**
  Functional: consumer group isolation; dead-letter queue (DLQ) for failed messages; event partitioning
  Non-functional: throughput > 10,000 events/sec; message ordering per orderId

**Node 3 — Option generation**
  Option A: Single topic, single consumer group — simple; all consumers compete; no isolation
  Option B: Per-event-type topics, dedicated consumer groups — isolation; ordering per partition key
  Option C: Shared topic with header-based routing — flexible; complex consumer logic

**Node 4 — Trade-off analysis**
  Per-event-type topics: OrderCreated topic + PaymentProcessed topic; each with dedicated consumer groups; partition key = orderId ensures ordering per order; chosen
  DLQ: messages after 3 retries → dead-letter topic; alerts on DLQ depth > 100

**Node 5 — ADR**
  Decision: Per-event-type Kafka topics, partition key = orderId, 12 partitions per topic
  Rationale: Message ordering per order guaranteed; consumer group isolation enables independent scaling
  Trade-off: Two topics to maintain vs one; acceptable operational cost

**Node 6 — Component placement**
  Kafka cluster: 3 brokers | Partitions: 12 per topic | Replication factor: 3
  DLQ: separate topic (OrderCreated-DLQ, PaymentProcessed-DLQ) | Retention: 7 days

**Node 7 — Validation**
  Throughput simulation: 12,000 events/sec at 12 partitions → PASS (> 10,000)
  Ordering guarantee: partitioning by orderId → messages for same order in same partition → PASS
  Arbiter: ArchitectureCompletenessArbiter

**Node 8 — Output**
  ADR: EventBus-ADR-004-broker-topology.md | Partition topology diagram

**Node 9 — Verdict**
  PASS — Message topology complete; ordering and throughput constraints met; DLQ policy defined

