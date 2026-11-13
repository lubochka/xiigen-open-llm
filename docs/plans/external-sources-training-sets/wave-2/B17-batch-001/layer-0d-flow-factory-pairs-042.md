# Layer 0-D — Flow Factory Pairs (042)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Elastic (Elasticsearch) connector

---

## B17-0D-042-001 — Elastic: API key factory (no SECURITY)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

SecurityAuditFlow34 NOT selected.

---

## B17-0D-042-002 — Elastic: bearer token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-042-003 — Elastic: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-042-004 — Elastic anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for Elastic API key | API key is not a token; SECURITY not compiled |
| Inventing SearchIndexingFlow | No domain-specific flows in B17 |
| Selecting BackendServiceFlow41 for "build Elastic connector" | connector vocabulary routes to 4.3 |
