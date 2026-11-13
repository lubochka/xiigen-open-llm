# Layer 0-A — Compiler Pairs (044)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Kafka connector

---

## B17-0A-044-001 — Kafka: predicate firing

**Input:** "Build the Kafka event streaming connector for producer and consumer message handling."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: producer, consumer, streaming, event are domain nouns — no predicates.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-044-002 — Kafka: "integrate" alone

**Input:** "Integrate the Kafka Streams API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — streaming domain has no exception to Rule 1.

---

## B17-0A-044-003 — Kafka: SASL token fires SECURITY

**Input:** "Implement the Kafka connector with SASL (Simple Authentication and Security Layer) authentication token."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |

Note: SASL (Simple Authentication and Security Layer) is a domain acronym — does not fire predicates alone. "token" fires SECURITY.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-044-004 — Kafka: "orchestrate" is not IMPLEMENT

**Input:** "Orchestrate the Kafka topic partitions for the order processing pipeline."

Triggers: none ("orchestrate" is not in the IMPLEMENT trigger list)
CompiledPredicateIds: []
TransitionType: none

---

## B17-0A-044-005 — Kafka: OAuth token + test

**Input:** "Build the Kafka connector with OAuth token-based cluster authentication and write consumer integration tests."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |
| write + tests | TEST |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
DecisionCount: 5 | UniqueSubPhases: 4 (4.3, 3.3, 3.4, 5.1)
