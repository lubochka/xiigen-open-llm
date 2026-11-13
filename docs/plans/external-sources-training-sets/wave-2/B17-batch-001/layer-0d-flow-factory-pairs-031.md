# Layer 0-D — Flow Factory Pairs (031)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Segment analytics connector factory

---

## B17-0D-031-001 — SegmentAnalyticsConnector factory: base case

Input predicates: [IMPLEMENT, API_CONTRACT]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

---

## B17-0D-031-002 — Segment factory: refactor with TEST

Input predicates: [IMPLEMENT, API_CONTRACT, TEST] with TransitionType=refactoring

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" }
]
TransitionType: refactoring
FallbackUsed: false
```

TestingFlow51 has a hard sequential dependency on IntegrationConnectorFlow43 regardless of TransitionType.

---

## B17-0D-031-003 — Segment factory: DOCUMENT case

Input predicates: [IMPLEMENT, API_CONTRACT, DOCUMENT]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "2.1", flow: "DocumentationFlow21", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

DocumentationFlow21 can run in parallel with IntegrationConnectorFlow43 (spec-driven; no code dependency).
