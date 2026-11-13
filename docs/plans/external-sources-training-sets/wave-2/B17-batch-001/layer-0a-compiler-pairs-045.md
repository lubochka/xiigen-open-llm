# Layer 0-A — Compiler Pairs (045)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: RabbitMQ connector

---

## B17-0A-045-001 — RabbitMQ: predicate firing

**Input:** "Build the RabbitMQ message broker connector with queue consumer and publisher handlers."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: queue, consumer, publisher, broker are domain nouns — no predicates.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-045-002 — RabbitMQ: "integrate" alone

**Input:** "Integrate the RabbitMQ AMQP (Advanced Message Queuing Protocol) API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
Note: AMQP (Advanced Message Queuing Protocol) is a domain acronym — fires no predicate.

---

## B17-0A-045-003 — RabbitMQ: authentication token

**Input:** "Implement the RabbitMQ connector with vhost access token authentication."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-045-004 — RabbitMQ: "coordinate" is not IMPLEMENT

**Input:** "Coordinate message delivery between RabbitMQ and the order service."

Triggers: none ("coordinate" is not an IMPLEMENT trigger)
CompiledPredicateIds: []
TransitionType: none

---

## B17-0A-045-005 — RabbitMQ: OAuth + token + document

**Input:** "Build the RabbitMQ connector with OAuth token access, and document the message queue API contract."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |
| document | DOCUMENT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "DOCUMENT"]
DecisionCount: 5 | UniqueSubPhases: 4 (4.3, 3.3, 3.4, 2.1)
