# Layer 0-D — Flow Factory Pairs (022)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Avalara tax connector factory

---

## B17-0D-022-001 — AvalaraTaxConnector: two-flow selection

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase set: [4.3, 3.3]
ConnectorName: AvalaraTaxConnector
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: feature_addition
```

---

## B17-0D-022-002 — AvalaraTaxConnector with SECURITY (token auth): three flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
ConnectorName: AvalaraTaxConnector (bearer token variant)
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" }
]
```

---

## B17-0D-022-003 — AvalaraTaxConnector full delivery: four flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
SubPhase set: [4.3, 3.3, 5.1, 2.1]
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" },
  { subPhase: "2.1", flow: "DocumentationFlow21", status: "deterministic_generic" }
]
```

---

## B17-0D-022-004 — Factory reference: all available flows for B17

**Complete flow registry for B17:**

| SubPhase | FlowId | Status | Trigger predicate |
|---|---|---|---|
| 4.3 | IntegrationConnectorFlow43 | probationary_accepted (original) / deterministic_generic (new connectors) | IMPLEMENT (with integration vocabulary) |
| 3.3 | APIContractFlow33 | deterministic_generic | API_CONTRACT |
| 3.4 | SecurityAuditFlow34 | deterministic_generic | SECURITY |
| 3.4 | AuthorizationFlow34 | deterministic_generic | AUTHORIZATION |
| 5.1 | TestingFlow51 | deterministic_generic | TEST |
| 2.1 | DocumentationFlow21 | deterministic_generic | DOCUMENT |
| 7.x | QAGatingFlow73 | deterministic_generic | QA (always last) |

**There are no other flows available in B17.** No GapHandler exists (Rule 19).
