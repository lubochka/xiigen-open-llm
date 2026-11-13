# Layer 1 — Transition Orchestrator Pairs (047)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: PostgreSQL connector

---

## B17-L1-047-001 — PostgreSQL: orchestration for [IMPL, API]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build PostgreSQLConnector
Step 2: APIContractFlow33 (3.3) — PostgreSQL connection API contract [after Step 1]
```

---

## B17-L1-047-002 — PostgreSQL: orchestration for [IMPL, API, SEC] (bearer token)

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build PostgreSQLConnector
Step 2: APIContractFlow33 (3.3) — API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — bearer token audit [after Step 1]
```

---

## B17-L1-047-003 — PostgreSQL: API_CONTRACT + SECURITY orchestration teaching

**Context:** "connector exposes an API endpoint and uses a bearer token"

Both API_CONTRACT (from "connector") and SECURITY (from "bearer token") compile independently.
The orchestrator treats them as separate flows at their respective sub-phases.
This is not a combined "API+SECURITY" step — they are two independent orchestration entries.

---

## B17-L1-047-004 — PostgreSQL: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document PostgreSQL JDBC driver contract
```

TransitionType: documentation
