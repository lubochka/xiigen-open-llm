# Layer 0-D — Flow Factory Pairs (054)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Netlify connector

---

## B17-0D-054-001 — Netlify: webhook factory (no token)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-054-002 — Netlify: access token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-054-003 — Netlify: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-054-004 — Netlify anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "publish to Netlify" | "publish" fires no IMPLEMENT |
| Inventing StaticSiteDeployFlow | No domain-specific flows in B17 |
| Selecting AuthorizationFlow34 for "OIDC provider" in Netlify Identity | OIDC does not compile AUTHORIZATION |
