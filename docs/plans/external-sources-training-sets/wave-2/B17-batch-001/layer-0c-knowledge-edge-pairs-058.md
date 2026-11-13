# Layer 0-C — Knowledge-Edge Pairs (058)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Azure Blob Storage connector

---

## B17-0C-058-001 — AzureBlob: non-trigger terms

**Proper nouns — no predicate:** Azure, Microsoft Azure, Azure Blob Storage
**Domain nouns — no predicate:** container, blob, tier (hot/cool/archive), lifecycle, replication, endpoint, storage account, connection string
**Acronyms — no predicate:** AAD (Azure Active Directory), RBAC (Role-Based Access Control), SAS (Shared Access Signature when used as noun only)

---

## B17-0C-058-002 — AzureBlob: SAS token compound fires SECURITY; SAS alone does not

"SAS URL" alone → no SECURITY.
"SAS (Shared Access Signature) token" → SECURITY fires (compound "SAS token" contains "token").
"Azure Blob SAS token" → SECURITY fires.
"Connection string" → no SECURITY (no "token").

---

## B17-0C-058-003 — AzureBlob: RBAC does not fire AUTHORIZATION

"Azure RBAC (Role-Based Access Control) for Blob Storage" → no predicate ("RBAC" is an acronym, not "OAuth").
"Azure Blob with OAuth 2.0 AAD token" → AUTHORIZATION fires (OAuth present), SECURITY fires (token present).

---

## B17-0C-058-004 — AzureBlob: "store" and "stream" are not IMPLEMENT

"Store media files in Azure Blob Storage" → no predicate.
"Stream video content from Azure Blob" → no predicate.
"Build the Azure Blob connector to store and stream media" → IMPLEMENT fires from "Build"; connector fires API_CONTRACT.
