# Layer 0-D — Flow Factory Pairs (016)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: HubSpot CRM connector factory

---

## B17-0D-016-001 — HubSpotCrmConnector: four flows for IMPLEMENT + API_CONTRACT + TEST + DOCUMENT

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
SubPhase set: [4.3, 3.3, 5.1, 2.1]
ConnectorName: HubSpotCrmConnector
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" },
  { subPhase: "2.1", flow: "DocumentationFlow21", status: "deterministic_generic" }
]
TransitionType: feature_addition
```

---

## B17-0D-016-002 — HubSpotCrmConnector OAuth: four flows with SECURITY + AUTHORIZATION

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase set: [4.3, 3.3, 3.4]
ConnectorName: HubSpotCrmConnector (OAuth private app)
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

## B17-0D-016-003 — API_CONTRACT alone: only 3.3 (HubSpot "integrate" case)

**Factory input:**
```
CompiledPredicateIds: ["API_CONTRACT"]
Input: "Integrate HubSpot CRM with the lead management workflow."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
```

**IntegrationConnectorFlow43 NOT selected.** Rule 10: 4.3 requires IMPLEMENT.

---

## B17-0D-016-004 — Refactoring path: TransitionType changes, flow selection unchanged

**Factory input (refactoring):**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring
ConnectorName: HubSpotCrmConnector (schema update)
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: refactoring
```

**Rule:** TransitionType (refactoring vs feature_addition) does not change which flows are selected. The factory selects flows based on predicate-to-sub-phase routing. TransitionType is metadata on the work item, not a flow selector.
