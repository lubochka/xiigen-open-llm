# Layer 0-D — Flow Factory Pairs (033)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Zendesk support connector factory

---

## B17-0D-033-001 — ZendeskConnector factory: base case

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

## B17-0D-033-002 — Zendesk factory: "authorization" English word — no AUTHORIZATION flow

Input: "Build Zendesk connector with proper authorization for support agents."
Predicates: [IMPLEMENT, API_CONTRACT] — AUTHORIZATION not fired.

Factory output:
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: feature_addition
FallbackUsed: false
```

AuthorizationFlow34 is NOT selected. The English word "authorization" is not a trigger.

---

## B17-0D-033-003 — Zendesk factory: full OAuth case

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
