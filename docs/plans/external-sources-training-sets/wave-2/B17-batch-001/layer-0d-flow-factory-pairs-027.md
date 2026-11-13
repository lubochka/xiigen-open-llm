# Layer 0-D — Flow Factory Pairs (027)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Plaid banking connector factory

---

## B17-0D-027-001 — PlaidBankingConnector factory: full 4-predicate case

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

Four flows selected. Two flows share sub-phase 3.4. DecisionCount: 4. UniqueSubPhases: 3.

---

## B17-0D-027-002 — Plaid: IMPLEMENT to 4.1 when no integration vocabulary

Input predicates: [IMPLEMENT, SECURITY] (from "Implement Plaid link token creation")

Factory output:
```
SelectedFlows: [
  { subPhase: "4.1", flow: "BackendServiceFlow41", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

BackendServiceFlow41 is selected (not IntegrationConnectorFlow43) because no integration vocabulary was present.

---

## B17-0D-027-003 — Plaid: API_CONTRACT + DOCUMENT (no IMPLEMENT)

Input predicates: [API_CONTRACT, DOCUMENT]

Factory output:
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "2.1", flow: "DocumentationFlow21", status: "deterministic_generic" }
]
TransitionType: documentation
FallbackUsed: false
```

TransitionType is documentation when IMPLEMENT is absent.
