# Layer 0-D — Flow Factory Pairs (019)
# Batch: B17-batch-001 | Scenario: B17 — Third-party payment provider connector integration
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Okta identity connector factory

---

## B17-0D-019-001 — OktaIdentityConnector: four flows (IMPLEMENT + API_CONTRACT + SECURITY + AUTHORIZATION)

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
SubPhase set: [4.3, 3.3, 3.4]
ConnectorName: OktaIdentityConnector
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "AuthorizationFlow34", status: "deterministic_generic" }
]
TransitionType: feature_addition
```

**Okta note:** OktaIdentityConnector typically fires SECURITY and AUTHORIZATION (OAuth-based identity provider). Four flows are the standard selection for Okta.

---

## B17-0D-019-002 — OktaIdentityConnector OIDC-only: three flows (no AUTHORIZATION)

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
Input: "Create the Okta OIDC connector and implement the authentication token introspection endpoint."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" }
]
```

**Note:** AuthorizationFlow34 NOT selected because AUTHORIZATION did not fire (OIDC ≠ OAuth).

---

## B17-0D-019-003 — OktaIdentityConnector with TEST: five flows

**Factory input:**
```
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"]
SubPhase set: [4.3, 3.3, 3.4, 5.1]
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "4.3", flow: "IntegrationConnectorFlow43", status: "deterministic_generic" },
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "SecurityAuditFlow34", status: "deterministic_generic" },
  { subPhase: "3.4", flow: "AuthorizationFlow34", status: "deterministic_generic" },
  { subPhase: "5.1", flow: "TestingFlow51", status: "deterministic_generic" }
]
```

---

## B17-0D-019-004 — API_CONTRACT alone: only APIContractFlow33 (integrate Okta)

**Factory input:**
```
CompiledPredicateIds: ["API_CONTRACT"]
Input: "Integrate Okta as the identity provider for the enterprise customer portal."
```

**Factory output:**
```
SelectedFlows: [
  { subPhase: "3.3", flow: "APIContractFlow33", status: "deterministic_generic" }
]
IntegrationConnectorFlow43 NOT selected
SecurityAuditFlow34 NOT selected
AuthorizationFlow34 NOT selected
```

**Rule 10:** API_CONTRACT alone selects only APIContractFlow33 (3.3). Security and authorization flows are not selected because SECURITY and AUTHORIZATION predicates were not in the predicate set.
