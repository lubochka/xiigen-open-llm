# Layer 0-D — Flow Factory Pairs (046)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: MongoDB connector

---

## B17-0D-046-001 — MongoDB: basic factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-046-002 — MongoDB: access token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-046-003 — MongoDB: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-046-004 — MongoDB anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "x.509 certificate" alone | Certificate alone is not a SECURITY trigger |
| Inventing DatabaseConnectorFlow | No domain-specific flows in B17 |
| Selecting 4.1 for "build MongoDB connector" | connector vocabulary → 4.3 |
