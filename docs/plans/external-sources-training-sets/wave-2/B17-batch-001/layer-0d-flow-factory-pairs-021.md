# Layer 0-D — Flow Factory Pairs (021)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Square POS connector factory

---

## B17-0D-021-001 — SquarePosConnector: two-flow selection

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase set: [4.3, 3.3]
ConnectorName: SquarePosConnector
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

## B17-0D-021-002 — SquarePosConnector OAuth: four flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
ConnectorName: SquarePosConnector (OAuth merchant authorization)
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

## B17-0D-021-003 — SquarePosConnector with QA: four flows including QAGatingFlow73

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "QA"]
SubPhase set: [4.3, 3.3, 5.1, 7.x]
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" },
  { subPhase: "7.x", flow: "QAGatingFlow73", status: "deterministic_generic", position: "last-absolute" }
]
```

**QAGatingFlow73 metadata:** `position: last-absolute` means this flow always executes after all other selected flows complete, regardless of ordering logic.

---

## B17-0D-021-004 — Factory position enforcement for QAGatingFlow73

**Rule 21:** QAGatingFlow73 is always selected last and executes last.

**Factory enforcement:**
```
selectFlows(predicateSet) {
  flows = [] // collect in canonical order
  if (IMPLEMENT) add IntegrationConnectorFlow43 (4.3)
  if (API_CONTRACT) add APIContractFlow33 (3.3)
  if (SECURITY) add SecurityAuditFlow34 (3.4)
  if (AUTHORIZATION) add AuthorizationFlow34 (3.4)
  if (TEST) add TestingFlow51 (5.1)
  if (DOCUMENT) add DocumentationFlow21 (2.1)
  if (QA) add QAGatingFlow73 (7.x) // always last
  return flows
}
```

**This ordering is enforced regardless of input order.** Even if "QA" appears first in the input text, QAGatingFlow73 is appended last.
