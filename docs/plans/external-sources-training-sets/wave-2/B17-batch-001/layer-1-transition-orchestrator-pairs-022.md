# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Avalara tax connector

---

## B17-L1-022-001 — AvalaraTaxConnector: two-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorName: AvalaraTaxConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build AvalaraTaxConnector
Step 2: APIContractFlow33 (3.3) — document Avalara AvaTax API contract
```

---

## B17-L1-022-002 — AvalaraTaxConnector with SECURITY: three-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
```

---

## B17-L1-022-003 — AvalaraTaxConnector full delivery: four flows

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

## B17-L1-022-004 — API_CONTRACT only: documentation orchestration

**Orchestrator input:**
```
SelectedFlows: [APIContractFlow33 (3.3)]
TransitionType: documentation
Input: "Integrate the Avalara AvaTax API for real-time sales tax calculation at checkout."
```

**Orchestration plan:**
```
Step 1: APIContractFlow33 (3.3) — document Avalara AvaTax API specification
Work product: AvaTax API contract document (no connector code)
```
