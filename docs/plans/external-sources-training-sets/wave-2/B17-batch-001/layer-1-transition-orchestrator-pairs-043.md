# Layer 1 — Transition Orchestrator Pairs (043)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Redis connector

---

## B17-L1-043-001 — Redis: orchestration for [IMPL, API]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build RedisConnector
Step 2: APIContractFlow33 (3.3) — Redis client API contract [after Step 1]
```

---

## B17-L1-043-002 — Redis: orchestration for [IMPL, API, SEC] (ACL token)

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build RedisConnector
Step 2: APIContractFlow33 (3.3) — API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — ACL token audit [after Step 1]
```

---

## B17-L1-043-003 — Redis: refactor orchestration

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — refactor RedisConnector for connection pooling + TLS
Step 2: APIContractFlow33 (3.3) — updated API contract [after Step 1]
```

TransitionType: refactoring

---

## B17-L1-043-004 — Redis: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Redis client SDK contract
```

TransitionType: documentation
