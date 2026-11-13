# Layer 0-D — Flow Factory Pairs (023)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Multi-connector orchestration factory

---

## B17-0D-023-001 — Multi-connector factory: same flows regardless of connector count

**Scenario A:** Single connector
```
Input: ["IMPLEMENT", "API_CONTRACT"]
SelectedFlows: [IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)]
```

**Scenario B:** Two connectors
```
Input: ["IMPLEMENT", "API_CONTRACT"]
SelectedFlows: [IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)]
```

**Scenario C:** Five connectors
```
Input: ["IMPLEMENT", "API_CONTRACT"]
SelectedFlows: [IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)]
```

**Rule:** The factory selects flows based on predicate sets, not on connector count. Same predicate set → same flow selection, regardless of how many connectors are named.

---

## B17-0D-023-002 — Multi-connector full pipeline (seven predicates)

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
Input: "Build the FedEx and Zuora connector pair, implement the OAuth token exchange, secure the credentials, document the API contracts, write end-to-end tests, and run QA."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43" },
  { subPhase: "3.3", flow: "APIContractFlow33" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34" },
  { subPhase: "3.4", flow: "AuthorizationFlow34" },
  { subPhase: "5.1", flow: "TestingFlow51" },
  { subPhase: "2.1", flow: "DocumentationFlow21" },
  { subPhase: "7.x", flow: "QAGatingFlow73", position: "last-absolute" }
]
TotalFlowsSelected: 7
```

---

## B17-0D-023-003 — "integrate" multi-connector → only APIContractFlow33

**Factory input:**
```
CompiledPredicateIds: ["API_CONTRACT"]
Input: "Integrate the Intercom, Mailgun, and Square connectors with the CRM layer."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TotalFlowsSelected: 1
```

**IntegrationConnectorFlow43 NOT selected.** Three connector names in the input do not trigger IMPLEMENT, and therefore do not trigger 4.3.

---

## B17-0D-023-004 — Factory probationary status evolution for multi-connector scenarios

```
State for connectors 001-012 (original batch):
  IntegrationConnectorFlow43: probationary_accepted

State for connectors 013-024 (this batch):
  IntegrationConnectorFlow43: deterministic_generic (new connectors, no candidate submitted yet)

Transition path:
  not_registered
    → (candidate submitted) → probationary (under review)
    → (Gate B passes) → probationary_accepted
    → (evidence threshold met across ≥9 instances) → trusted_learned

When trusted_learned:
  IntegrationConnectorFlow43 is promoted from probationary_accepted to trusted_learned.
  This applies to the shared flow (all connector names use the same IntegrationConnectorFlow43).
```
