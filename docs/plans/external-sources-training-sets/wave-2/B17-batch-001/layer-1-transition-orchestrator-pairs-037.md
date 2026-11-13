# Layer 1 — Transition Orchestrator Pairs (037)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Jira connector

---

## B17-L1-037-001 — Jira: orchestration for [IMPL, API, SEC]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build JiraConnector
Step 2: APIContractFlow33 (3.3) — finalize Jira REST API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — API token security audit [after Step 1]
```

Steps 2 and 3 can run in parallel after Step 1 (both depend on Step 1 code).
TransitionType: feature_addition

---

## B17-L1-037-002 — Jira: orchestration for [API] alone

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Jira API contract
```

TransitionType: documentation | No IntegrationConnectorFlow43 — IMPLEMENT absent.

---

## B17-L1-037-003 — Jira: orchestration for [IMPL, API, SEC, AUTH]

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4), AuthorizationFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build JiraConnector
Step 2: APIContractFlow33 (3.3) — Jira API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — token security audit [after Step 1; parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) — OAuth flow review [after Step 1; parallel with Step 3]
```

---

## B17-L1-037-004 — Jira orchestration constraints

1. TestingFlow51 (5.1) ALWAYS after IntegrationConnectorFlow43 (4.3) — hard sequential dependency
2. APIContractFlow33 runs after 4.3 (contract after code)
3. SecurityAuditFlow34 and AuthorizationFlow34 can run in parallel after 4.3
