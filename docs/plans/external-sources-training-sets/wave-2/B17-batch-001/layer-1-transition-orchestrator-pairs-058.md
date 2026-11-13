# Layer 1 — Transition Orchestrator Pairs (058)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: orchestrator | focus: Azure Blob Storage connector

---

## B17-L1-058-001 — AzureBlob: orchestration for [IMPL, API, SEC] (SAS token)

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build AzureBlobConnector
Step 2: APIContractFlow33 (3.3) — Azure Blob REST API contract [after Step 1]
Step 3: SecurityAuditFlow34 (3.4) — SAS token audit [after Step 1]
```

---

## B17-L1-058-002 — AzureBlob: OAuth AAD token orchestration

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3), SecurityAuditFlow34(3.4), AuthorizationFlow34(3.4)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build AzureBlobConnector
Step 2: APIContractFlow33 (3.3)
Step 3: SecurityAuditFlow34 (3.4) — AAD access token audit [parallel with Step 4]
Step 4: AuthorizationFlow34 (3.4) — OAuth 2.0 AAD review [parallel with Step 3]
```

---

## B17-L1-058-003 — AzureBlob: "integrate" orchestration

**Flows:** APIContractFlow33(3.3)

```
Step 1: APIContractFlow33 (3.3) — document Azure Blob Storage REST API contract
```

TransitionType: documentation

---

## B17-L1-058-004 — AzureBlob: basic orchestration (no token)

**Flows:** IntegrationConnectorFlow43(4.3), APIContractFlow33(3.3)

```
Step 1: IntegrationConnectorFlow43 (4.3) — build AzureBlobConnector
Step 2: APIContractFlow33 (3.3) — API contract [after Step 1]
```

Note: SecurityAuditFlow34 absent when only connection string or basic credential is described.
