# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: HubSpot CRM connector

---

## B17-L1-016-001 — HubSpotCrmConnector: four-flow full delivery orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  TestingFlow51 (5.1),
  DocumentationFlow21 (2.1)
]
TransitionType: feature_addition
ConnectorName: HubSpotCrmConnector
```

**Orchestration plan:**
```
Step 1 (parallel):
  ‖ IntegrationConnectorFlow43 (4.3) — build HubSpotCrmConnector
  ‖ DocumentationFlow21 (2.1) — spec-driven documentation
Step 2 (parallel, after Step 1):
  ‖ APIContractFlow33 (3.3) — CRM API contract
  ‖ TestingFlow51 (5.1) — requires connector from Step 1
```

---

## B17-L1-016-002 — HubSpotCrmConnector OAuth private app: four flows + security

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
Step 1: IntegrationConnectorFlow43 (4.3) — build HubSpotCrmConnector with OAuth
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
```

---

## B17-L1-016-003 — HubSpotCrmConnector webhook security: three-flow orchestration

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

## B17-L1-016-004 — Orchestration metadata

```
TransitionId: "transition-B17-L1-016-4.3-feature_addition"
TransitionType: feature_addition
PhaseId: "4"
SubPhaseId: "4.3"
DomainId: "software-development"
ConnectorName: HubSpotCrmConnector
PredicateIds: ["IMPLEMENT", "API_CONTRACT"]
ContextCarryForward: true
Metadata:
  transition_classification_basis: "IMPLEMENT (build) + API_CONTRACT (connector, API) → 4.3; CRM contact sync = feature_addition"
  primary_intent: "create"
  domain_signal: "external_service_integration"
```
