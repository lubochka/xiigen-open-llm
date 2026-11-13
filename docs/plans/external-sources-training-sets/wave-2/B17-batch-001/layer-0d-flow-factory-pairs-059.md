# Layer 0-D — Flow Factory Pairs (059)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: HashiCorp Vault connector

---

## B17-0D-059-001 — HashiCorpVault: token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-059-002 — HashiCorpVault: authenticate factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

Note: "authenticate" (Rule 8) and "token" (Rule 6) both compile SECURITY — same factory output.

---

## B17-0D-059-003 — HashiCorpVault: OAuth + token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-059-004 — HashiCorpVault anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "retrieve secrets from Vault" | "retrieve" fires no IMPLEMENT |
| Inventing SecretsManagerFlow | No domain-specific flows in B17 |
| Selecting SecurityAuditFlow34 for "AppRole secret ID" alone (no "token", no "authenticate") | Secret ID is not a SECURITY trigger without "token" or "authenticate" |
