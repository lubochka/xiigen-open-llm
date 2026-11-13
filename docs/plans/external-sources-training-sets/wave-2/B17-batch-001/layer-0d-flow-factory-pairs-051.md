# Layer 0-D — Flow Factory Pairs (051)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Firebase connector

---

## B17-0D-051-001 — Firebase: webhook factory (no token)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-051-002 — Firebase: service account token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-051-003 — Firebase: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-051-004 — Firebase anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "push data to Firebase" | "push" fires no IMPLEMENT; no flows selected |
| Selecting AuthorizationFlow34 for "OIDC provider" | OIDC does not compile AUTHORIZATION |
| Inventing RealtimeDatabaseFlow | No domain-specific flows in B17 |
