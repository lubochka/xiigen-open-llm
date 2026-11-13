# Layer 0-D — Flow Factory Pairs (050)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: AWS Cognito connector

---

## B17-0D-050-001 — Cognito: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-050-002 — Cognito: SAML factory (no AUTHORIZATION)

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

Note: SAML does not compile AUTHORIZATION; AuthorizationFlow34 NOT selected.

---

## B17-0D-050-003 — Cognito: authentication factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-050-004 — Cognito anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting AuthorizationFlow34 for "SAML federation" | SAML does not compile AUTHORIZATION; only OAuth does |
| Inventing IdentityPoolFlow | No domain-specific flows in B17 |
| Selecting AuthorizationFlow34 for "pre-authentication Lambda trigger" | "authentication" compiles SECURITY (Rule 8), not AUTHORIZATION |
