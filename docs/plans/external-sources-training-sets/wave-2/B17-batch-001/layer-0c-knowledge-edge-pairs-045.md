# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: RabbitMQ connector

---

## B17-0C-045-001 — RabbitMQ: non-trigger terms

**Proper nouns — no predicate:** RabbitMQ, AMQP
**Domain nouns — no predicate:** exchange, queue, binding, routing key, vhost, consumer, publisher, message, ack, nack, dead letter, prefetch
**Acronyms — no predicate:** AMQP (Advanced Message Queuing Protocol), DLQ (Dead Letter Queue)

---

## B17-0C-045-002 — RabbitMQ: "vhost" does not supply integration vocabulary

"Vhost" is a RabbitMQ virtual host concept. It is a domain noun — it does not supply integration vocabulary. IMPLEMENT routes to 4.3 only when "connector", "webhook", or equivalent integration vocabulary is present.

---

## B17-0C-045-003 — RabbitMQ: "coordinate" is not IMPLEMENT

"Coordinate message delivery between RabbitMQ and the order service" — "coordinate" is not an IMPLEMENT trigger. Result: no predicates (no connector vocabulary either).

---

## B17-0C-045-004 — RabbitMQ: management API plugin does not add separate routing

"Build the RabbitMQ connector using the management API plugin" — "management API" does not add a separate API_CONTRACT decision. API_CONTRACT fires once from "connector" (Rule 4). Predicate set remains ["IMPLEMENT", "API_CONTRACT"] with DecisionCount 2.
