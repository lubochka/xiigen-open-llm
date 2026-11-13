# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Zuora billing connector

---

## B17-L1-018-001 — ZuoraBillingConnector: two-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorName: ZuoraBillingConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build ZuoraBillingConnector
Step 2: APIContractFlow33 (3.3) — document Zuora subscription billing API contract
```

---

## B17-L1-018-002 — ZuoraBillingConnector OAuth: four-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4),
  AuthorizationFlow34 (3.4)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build ZuoraBillingConnector with OAuth
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
```

---

## B17-L1-018-003 — ZuoraBillingConnector full delivery: four flows

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  TestingFlow51 (5.1),
  DocumentationFlow21 (2.1)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1 (parallel):
  ‖ IntegrationConnectorFlow43 (4.3)
  ‖ DocumentationFlow21 (2.1)
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ TestingFlow51 (5.1)
```

---

## B17-L1-018-004 — ZuoraBillingConnector API_CONTRACT only

**Orchestrator input:**
```
SelectedFlows: [APIContractFlow33 (3.3)]
TransitionType: documentation
```

**Orchestration plan:**
```
Step 1: APIContractFlow33 (3.3) — Zuora billing API specification
```
