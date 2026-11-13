# Layer 0-D — Flow Factory Pairs (049)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Auth0 connector

---

## B17-0D-049-001 — Auth0: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-049-002 — Auth0: OIDC alone factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

Note: OIDC does not compile AUTHORIZATION; AuthorizationFlow34 is NOT selected.

---

## B17-0D-049-003 — Auth0: "integrate" factory

**Predicates:** ["API_CONTRACT"]

Factory output: APIContractFlow33(3.3) only.

---

## B17-0D-049-004 — Auth0: full seven-predicate factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)
- TestingFlow51(5.1)
- DocumentationFlow21(2.1)
- QAGatingFlow73(7.x) [last-absolute]

SelectedFlows: 7 | UniqueSubPhases: 6
QAGatingFlow73 is always last; starts only when all other flows complete.
