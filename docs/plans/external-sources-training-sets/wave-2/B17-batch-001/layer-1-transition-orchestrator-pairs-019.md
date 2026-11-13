# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Okta identity connector

---

## B17-L1-019-001 — OktaIdentityConnector: four-flow orchestration (standard Okta OAuth)

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4),
  AuthorizationFlow34 (3.4)
]
TransitionType: feature_addition
ConnectorName: OktaIdentityConnector
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build OktaIdentityConnector
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
```

---

## B17-L1-019-002 — OktaIdentityConnector with TEST: five flows

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4),
  AuthorizationFlow34 (3.4),
  TestingFlow51 (5.1)
]
TransitionType: feature_addition
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3)
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)
  ‖ AuthorizationFlow34 (3.4)
Step 3: TestingFlow51 (5.1) — requires connector from Step 1
```

---

## B17-L1-019-003 — OktaIdentityConnector OIDC-only: three flows (no AUTHORIZATION)

**Orchestrator input:**
```
SelectedFlows: [
  IntegrationConnectorFlow43 (4.3),
  APIContractFlow33 (3.3),
  SecurityAuditFlow34 (3.4)
]
TransitionType: feature_addition
Input: "Create the Okta OIDC connector and implement the authentication token introspection endpoint."
```

**Orchestration plan:**
```
Step 1: IntegrationConnectorFlow43 (4.3) — build OktaIdentityConnector (OIDC)
Step 2 (parallel):
  ‖ APIContractFlow33 (3.3)
  ‖ SecurityAuditFlow34 (3.4)

AuthorizationFlow34 NOT scheduled (AUTHORIZATION did not fire — "OIDC" ≠ "OAuth")
```

---

## B17-L1-019-004 — OktaIdentityConnector API_CONTRACT only

**Orchestrator input:**
```
SelectedFlows: [APIContractFlow33 (3.3)]
TransitionType: documentation
Input: "Integrate Okta as the identity provider for the enterprise customer portal."
```

**Orchestration plan:**
```
Step 1: APIContractFlow33 (3.3) — document Okta identity API contract specification
Work product: Identity provider integration specification
```
