# Layer 0-D — Flow Factory Pairs (028)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: DocuSign e-signature connector factory

---

## B17-0D-028-001 — DocuSignConnector factory: base case

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

## B17-0D-028-002 — DocuSignConnector factory: full OAuth + token case

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

## B17-0D-028-003 — DocuSign: refactor factory output

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

---

## B17-0D-028-004 — DocuSign anti-patterns

| Anti-pattern | Description | Correction |
|---|---|---|
| Selecting IntegrationConnectorFlow43 for "integrate" alone | API_CONTRACT alone → no 4.3 | Only select 4.3 when IMPLEMENT fires |
| Inventing AUTHORIZATION for "e-signature" | Domain noun ≠ AUTHORIZATION | AUTHORIZATION requires "OAuth" |
| Suppressing SECURITY for "access token refresh" | "token" fires SECURITY unconditionally | Rule 6 has no exceptions |
