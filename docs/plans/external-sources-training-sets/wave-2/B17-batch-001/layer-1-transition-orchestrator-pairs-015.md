# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: FedEx/UPS shipping connector

---

## B17-L1-015-001 — FedExShippingConnector: two-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorName: FedExShippingConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build FedExShippingConnector
Step 2: APIContractFlow33 (3.3) — document FedEx shipping API contract
```

---

## B17-L1-015-002 — UpsShippingConnector: two-flow orchestration (parallel to FedEx)

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorName: UpsShippingConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build UpsShippingConnector
Step 2: APIContractFlow33 (3.3) — document UPS shipping API contract
```

**Note:** If both FedEx and UPS connectors are requested in the same work item, both share a single orchestration plan with IntegrationConnectorFlow43. They are not separate orchestration plans — the nine-node flow handles both connectors in sequence or in parallel branches within the same flow execution.

---

## B17-L1-015-003 — FedExShippingConnector OAuth: four-flow orchestration

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
Step 1: IntegrationConnectorFlow43 (4.3) — build FedExShippingConnector with OAuth
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
```

---

## B17-L1-015-004 — Refactoring orchestration: same plan, different TransitionType

**Orchestrator input (refactoring):**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: refactoring
Input: "Refactor the FedEx connector to support the FedEx Ship v1 API schema changes."
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — refactor FedExShippingConnector
Step 2: APIContractFlow33 (3.3) — update FedEx API contract

TransitionType: refactoring (connector interface backward compatibility enforced at N08)
```

**Rule:** Refactoring orchestration has the same step structure as feature_addition orchestration. The N08 arbiter adds a backward compatibility check for refactoring transitions. The orchestrator itself does not change the plan for refactoring.
