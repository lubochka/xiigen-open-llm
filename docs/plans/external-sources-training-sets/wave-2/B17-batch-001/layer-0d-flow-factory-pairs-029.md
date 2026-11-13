# Layer 0-D — Flow Factory Pairs (029)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Cloudinary media connector factory

---

## B17-0D-029-001 — CloudinaryConnector factory: base case

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

## B17-0D-029-002 — CloudinaryConnector factory: with SECURITY

Input predicates: [IMPLEMENT, API_CONTRACT, SECURITY]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

---

## B17-0D-029-003 — CloudinaryConnector factory: with QA (last-absolute)

Input predicates: [IMPLEMENT, API_CONTRACT, TEST, QA]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" },
  { subPhase: "7.x", flow: "QAGatingFlow73", status: "deterministic_generic", position: "last-absolute" }
]
TransitionType: feature_addition
FallbackUsed: false
```

QAGatingFlow73 always carries `position: "last-absolute"` in factory output.
