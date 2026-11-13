# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Multi-connector orchestration

---

## B17-L1-023-001 — Multi-connector two-flow orchestration (same plan as single connector)

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorNames: [StripePaymentConnector, AvalaraTaxConnector]
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build both connectors (handled within flow)
Step 2: APIContractFlow33 (3.3) — document combined API contracts
```

**Rule:** Multiple connectors in one work item share a single orchestration plan. IntegrationConnectorFlow43 handles all connector builds within its nine-node execution.

---

## B17-L1-023-002 — Multi-connector full pipeline: seven-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4),
  AuthorizationFlow34 (3.4),
  TestingFlow51 (5.1),
  DocumentationFlow21 (2.1),
  QAGatingFlow73 (7.x)
]
TransitionType: feature_addition
Input: "Build the FedEx and Zuora connector pair, implement OAuth token exchange, secure credentials, document contracts, write tests, run QA."
```

**Orchestration plan:**
```
Step 1 (parallel):
  ‖ IntegrationConnectorFlow43 (4.3) — build connector pair
  ‖ DocumentationFlow21 (2.1) — spec-driven documentation
Step 2 (parallel, after Step 1):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
Step 3 (after Step 1):
  TestingFlow51 (5.1) — tests require connector code
Step 4 (absolute last):
  QAGatingFlow73 (7.x) — gates all prior flows

TotalSteps: 4
QAPosition: last-absolute
```

---

## B17-L1-023-003 — "integrate" multi-connector: single-step documentation

**Orchestrator input:**
```
SelectedFlows: [APIContractFlow33 (3.3)]
TransitionType: documentation
Input: "Integrate the Intercom, Mailgun, and Square connectors with the CRM layer."
```

**Orchestration plan:**
```
Step 1: APIContractFlow33 (3.3) — multi-connector API contract specification
Work product: combined integration specification for three connectors (no code)
```

---

## B17-L1-023-004 — Orchestrator dependencies rule summary

**Dependency rules applied in all connector orchestrations:**

| Flow | Can start when... |
|---|---|
| IntegrationConnectorFlow43 (4.3) | Step 1 (first always) |
| APIContractFlow33 (3.3) | After Step 1 (or in parallel — interface known upfront) |
| SecurityAuditFlow34 (3.4) | After Step 1 (or in parallel — design known upfront) |
| AuthorizationFlow34 (3.4) | After Step 1 (or in parallel) |
| DocumentationFlow21 (2.1) | Step 1 in parallel (spec-driven — no code dependency) |
| TestingFlow51 (5.1) | After IntegrationConnectorFlow43 completes (code dependency) |
| QAGatingFlow73 (7.x) | After ALL other flows complete (last-absolute) |
