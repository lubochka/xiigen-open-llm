# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Square POS connector

---

## B17-L1-021-001 — SquarePosConnector: two-flow orchestration

**Orchestrator input:**
```
SelectedFlows: [IntegrationConnectorFlow43 (4.3), APIContractFlow33 (3.3)]
TransitionType: feature_addition
ConnectorName: SquarePosConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build SquarePosConnector
Step 2: APIContractFlow33 (3.3) — document Square payments API contract
```

---

## B17-L1-021-002 — SquarePosConnector OAuth: four-flow orchestration

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
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
```

---

## B17-L1-021-003 — SquarePosConnector with QA gate: four flows, four-step orchestration

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  TestingFlow51 (5.1),
  QAGatingFlow73 (7.x)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build SquarePosConnector
Step 2: APIContractFlow33 (3.3) — API contract
Step 3: TestingFlow51 (5.1) — tests (requires Step 1)
Step 4: QAGatingFlow73 (7.x) — QA gate (absolute last, gates all prior flows)

QAPosition: last-absolute (enforced — QA cannot run until ALL other flows complete)
```

---

## B17-L1-021-004 — QA gating explanation for SquarePosConnector

**Why QA must be last:**
QAGatingFlow73 reviews the complete set of completed flows as its input. It cannot gate flows that haven't completed. The "last-absolute" position ensures:
1. IntegrationConnectorFlow43 has produced code
2. TestingFlow51 has run the tests and produced results
3. APIContractFlow33 has produced the contract spec
4. All three outputs are available for QA review

**QA does not repeat work:** QAGatingFlow73 reviews the work products from Steps 1-3. It does not re-run the connector build or re-run tests. It reviews and gates the assembled outputs.
