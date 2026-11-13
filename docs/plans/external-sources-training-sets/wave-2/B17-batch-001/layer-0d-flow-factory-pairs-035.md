# Layer 0-D — Flow Factory Pairs (035)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: GitHub source control connector factory

---

## B17-0D-035-001 — GitHubWebhookConnector factory: base case

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

## B17-0D-035-002 — GitHub factory: full OAuth case

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

## B17-0D-035-003 — GitHub factory: API_CONTRACT + TEST (no IMPLEMENT)

Input predicates: [API_CONTRACT, TEST] (from "Wire up GitHub webhook, add tests")

Factory output:
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" }
]
TransitionType: documentation
FallbackUsed: false
```

No IntegrationConnectorFlow43. TestingFlow51 selected but note: without 4.3, tests are for API contract review only (no connector code to test against).
