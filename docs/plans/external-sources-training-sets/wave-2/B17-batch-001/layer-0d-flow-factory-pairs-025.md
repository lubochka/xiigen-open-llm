# Layer 0-D — Flow Factory Pairs (025)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Twilio SMS connector factory

---

## B17-0D-025-001 — TwilioSmsConnector factory selection

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

## B17-0D-025-002 — TwilioSmsConnector: "integrate" alone — no 4.3 flow

Input predicates: [API_CONTRACT]

Factory output:
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
TransitionType: documentation
FallbackUsed: false
```

Anti-pattern: Selecting IntegrationConnectorFlow43 when IMPLEMENT is absent.
4.3 requires IMPLEMENT. API_CONTRACT alone never selects IntegrationConnectorFlow43.

---

## B17-0D-025-003 — TwilioSmsConnector: full auth factory output

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

Both SecurityAuditFlow34 and AuthorizationFlow34 are selected — they co-exist at 3.4.
