# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Segment analytics connector

---

## B17-L1-017-001 — SegmentAnalyticsConnector: two-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorName: SegmentAnalyticsConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build SegmentAnalyticsConnector
Step 2: APIContractFlow33 (3.3) — document Segment analytics API contract
```

---

## B17-L1-017-002 — SegmentAnalyticsConnector with TEST: three-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  TestingFlow51 (5.1)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build connector
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3) — API contract spec
  ‖ TestingFlow51 (5.1) — requires connector code from Step 1
```

---

## B17-L1-017-003 — SegmentAnalyticsConnector refactoring: two-flow with refactoring metadata

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: refactoring
Input: "Refactor the Segment connector to use the Segment HTTP API v1.3 batch endpoint."
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — refactor SegmentAnalyticsConnector
Step 2: APIContractFlow33 (3.3) — update Segment API contract
TransitionType: refactoring
```

---

## B17-L1-017-004 — API_CONTRACT only: single-step documentation orchestration

**Orchestrator input:**
```
SelectedFlows: [APIContractFlow33 (3.3)]
TransitionType: documentation
Input: "Integrate the Segment analytics API to capture page views and user actions."
```

**Orchestration plan:**
```
Step 1: APIContractFlow33 (3.3) — generate Segment API contract specification
Work product: API specification (not connector code)
IntegrationConnectorFlow43 NOT scheduled
```

**Teaching point:** When the orchestrator receives only APIContractFlow33, the work product is a specification document. No connector code is generated. The output describes the Segment analytics API endpoints and schemas — it does not build a connector.
