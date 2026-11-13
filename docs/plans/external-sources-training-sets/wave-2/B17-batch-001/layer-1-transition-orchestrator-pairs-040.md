# Layer 1 — Transition Orchestrator Pairs (040)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Datadog connector

---

## B17-L1-040-001 — Datadog: orchestration for [IMPL, API] (API key)

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build DatadogConnector
Step 2: APIContractFlow33 (3.3) — Datadog Metrics API contract [after Step 1]
```

TransitionType: feature_addition

---

## B17-L1-040-002 — Datadog: orchestration for [IMPL, API, SEC] (agent token)

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build DatadogConnector
Step 2: APIContractFlow33 (3.3) — API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — agent token audit [after Step 1]
```

---

## B17-L1-040-003 — Datadog: refactor orchestration

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — refactor DatadogConnector for v2 API
Step 2: APIContractFlow33 (3.3) — updated API contract [after Step 1]
```

TransitionType: refactoring

---

## B17-L1-040-004 — Datadog: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Datadog Metrics API contract
```

TransitionType: documentation
