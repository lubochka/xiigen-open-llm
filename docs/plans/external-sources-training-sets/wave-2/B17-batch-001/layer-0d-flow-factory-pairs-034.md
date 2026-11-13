# Layer 0-D — Flow Factory Pairs (034)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Salesforce CRM connector factory

---

## B17-0D-034-001 — SalesforceConnector factory: full OAuth case

Input predicates: [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "AuthorizationFlow34", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

---

## B17-0D-034-002 — Salesforce factory: IMPLEMENT to 4.1 (no integration vocabulary)

Input predicates: [IMPLEMENT] (from "Implement Salesforce lead scoring algorithm")

Factory output:
```
SelectedFlows: [
  { subPhase: "4.1", flow: "BackendServiceFlow41", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

BackendServiceFlow41 selected. No APIContractFlow33 because API_CONTRACT not fired.

---

## B17-0D-034-003 — Salesforce factory: with TEST

Input predicates: [IMPLEMENT, API_CONTRACT, TEST]

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

TestingFlow51 must run after IntegrationConnectorFlow43 completes (hard sequential dependency).
