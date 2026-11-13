# Layer 0-D — Flow Factory Pairs (058)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Azure Blob Storage connector

---

## B17-0D-058-001 — AzureBlob: basic factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)

---

## B17-0D-058-002 — AzureBlob: SAS token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)

---

## B17-0D-058-003 — AzureBlob: OAuth AAD token factory

**Predicates:** ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]

Factory output:
- IntegrationConnectorFlow43(4.3)
- APIContractFlow33(3.3)
- SecurityAuditFlow34(3.4)
- AuthorizationFlow34(3.4)

---

## B17-0D-058-004 — AzureBlob anti-patterns

| Anti-pattern | Correction |
|---|---|
| Selecting SecurityAuditFlow34 for "connection string" | Connection string is not a token |
| Selecting AuthorizationFlow34 for "RBAC" | RBAC does not compile AUTHORIZATION; only OAuth does |
| Inventing BlobStorageFlow | No domain-specific flows in B17 |
