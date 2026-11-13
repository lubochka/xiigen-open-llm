# Layer 0-D — Flow Factory Pairs (018)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Zuora billing connector factory

---

## B17-0D-018-001 — ZuoraBillingConnector: two-flow selection

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase set: [4.3, 3.3]
ConnectorName: ZuoraBillingConnector
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

## B17-0D-018-002 — ZuoraBillingConnector OAuth: four flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase set: [4.3, 3.3, 3.4]
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "AuthorizationFlow34", status: "deterministic_generic" }
]
```

---

## B17-0D-018-003 — ZuoraBillingConnector full delivery: four unique sub-phases

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

## B17-0D-018-004 — Factory for API_CONTRACT alone ("integrate Zuora")

**Factory input:**
```
CompiledPredicateIds: ["API_CONTRACT"]
Input: "Integrate the Zuora billing API to manage recurring subscription lifecycle events."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
WorkProductType: "API specification document" (not connector code)
```
