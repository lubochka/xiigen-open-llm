# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Mailgun email connector

---

## B17-L1-014-001 — MailgunEmailConnector: two-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorName: MailgunEmailConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build MailgunEmailConnector
Step 2: APIContractFlow33 (3.3) — document Mailgun API contract
```

---

## B17-L1-014-002 — MailgunEmailConnector with TEST and SECURITY: four-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4),
  TestingFlow51 (5.1)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build MailgunEmailConnector
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
Step 3: TestingFlow51 (5.1) — requires connector code from Step 1
```

**Dependency note:** TestingFlow51 runs after IntegrationConnectorFlow43 completes because tests require the connector implementation. APIContractFlow33 and SecurityAuditFlow34 can run in parallel with each other, after Step 1.

---

## B17-L1-014-003 — MailgunEmailConnector API_CONTRACT only: single-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [APIContractFlow33 (3.3)]
TransitionType: documentation
Input: "Integrate Mailgun into the notification service."
```

**Orchestration plan:**
```
Step 1: APIContractFlow33 (3.3) — document Mailgun API specification

Single flow, no dependencies.
Work product: API specification (not connector code).
```

---

## B17-L1-014-004 — MailgunEmailConnector full delivery: four flows

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
  ‖ IntegrationConnectorFlow43 (4.3) — build connector
  ‖ DocumentationFlow21 (2.1) — spec-driven documentation (no code dependency)
Step 2 (parallel, after Step 1):
  ‖ APIContractFlow33 (3.3) — contract spec
  ‖ TestingFlow51 (5.1) — requires connector code from Step 1

Note: DocumentationFlow21 can run in parallel with building (spec is known upfront).
TestingFlow51 depends on Step 1 completion.
```
