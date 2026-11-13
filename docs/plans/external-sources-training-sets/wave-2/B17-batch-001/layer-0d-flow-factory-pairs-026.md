# Layer 0-D — Flow Factory Pairs (026)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: SendGrid email connector factory

---

## B17-0D-026-001 — SendGridEmailConnector factory: base case

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

## B17-0D-026-002 — SendGridEmailConnector factory: with TEST and DOCUMENT

Input predicates: [IMPLEMENT, API_CONTRACT, TEST, DOCUMENT]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" },
  { subPhase: "2.1", flow: "DocumentationFlow21", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

Four distinct flows — four distinct sub-phases. No shared destinations in this predicate set.

---

## B17-0D-026-003 — SendGrid: refactor factory output

Input predicates: [IMPLEMENT, API_CONTRACT] with TransitionType=refactoring

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: refactoring
FallbackUsed: false
```

TransitionType is "refactoring" — the SelectedFlows are identical to feature_addition. TransitionType is metadata on the factory output, not a different flow selection.
