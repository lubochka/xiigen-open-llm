# Layer 0-D — Flow Factory Pairs (017)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Segment analytics connector factory

---

## B17-0D-017-001 — SegmentAnalyticsConnector: minimal two-flow selection

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase set: [4.3, 3.3]
ConnectorName: SegmentAnalyticsConnector
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

## B17-0D-017-002 — SegmentAnalyticsConnector with TEST: three flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
SubPhase set: [4.3, 3.3, 5.1]
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" }
]
```

---

## B17-0D-017-003 — SegmentAnalyticsConnector with DOCUMENT: three flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
SubPhase set: [4.3, 3.3, 2.1]
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "2.1", flow: "DocumentationFlow21", status: "deterministic_generic" }
]
```

---

## B17-0D-017-004 — Refactoring path for SegmentAnalyticsConnector

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: refactoring
ConnectorName: SegmentAnalyticsConnector (API version upgrade)
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: refactoring
```

**Teaching point:** Refactoring and feature_addition cases select identical flows. TransitionType is a work item attribute, not a factory routing decision. The gate B condition (TransitionType.refactoring → connector interface must maintain backward compatibility) is enforced at the N08 arbiter level, not the factory level.
