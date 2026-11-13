# Layer 1 — Transition Orchestrator Pairs (044)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Kafka connector

---

## B17-L1-044-001 — Kafka: orchestration for [IMPL, API, SEC]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build KafkaConnector
Step 2: APIContractFlow33 (3.3) — Kafka Streams API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — SASL token audit [after Step 1]
```

---

## B17-L1-044-002 — Kafka: orchestration for [IMPL, API, SEC, AUTH, TEST]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4), AuthorizationFlow34(3.4), TestingFlow51(5.1)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build KafkaConnector
Step 2: APIContractFlow33 (3.3) — API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — token audit [parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) — OAuth review [parallel with Step 3]
Step 5: TestingFlow51 (5.1) — consumer integration tests [after Step 1; hard dependency]
```

---

## B17-L1-044-003 — Kafka: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Kafka Streams API contract
```

TransitionType: documentation

---

## B17-L1-044-004 — Kafka: "orchestrate" produces no orchestration plan

Input: "Orchestrate the Kafka topic partitions for the order processing pipeline."
No predicates compiled → no flows selected → no orchestration plan generated.
