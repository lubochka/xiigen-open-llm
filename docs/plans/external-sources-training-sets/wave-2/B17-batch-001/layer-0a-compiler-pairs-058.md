# Layer 0-A — Compiler Pairs (058)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Azure Blob Storage connector

---

## B17-0A-058-001 — AzureBlob: predicate firing

**Input:** "Build the Azure Blob Storage connector for container management and object upload."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
TransitionType: feature_addition
Note: container, Blob, object are domain nouns — no predicates.
Integration vocabulary present: connector → IMPLEMENT routes to 4.3

---

## B17-0A-058-002 — AzureBlob: "integrate" alone

**Input:** "Integrate the Azure Blob Storage REST API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent.

---

## B17-0A-058-003 — AzureBlob: SAS token fires SECURITY

**Input:** "Implement the Azure Blob connector with SAS (Shared Access Signature) token for scoped container access."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token (in SAS token) | SECURITY (Rule 6) |

Note: SAS (Shared Access Signature) is a domain acronym — "token" in "SAS token" fires SECURITY.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3

---

## B17-0A-058-004 — AzureBlob: OAuth + token (AAD)

**Input:** "Build the Azure Blob connector with OAuth 2.0 Azure Active Directory access token."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |

Note: AAD (Azure Active Directory) is a product acronym — fires no predicate.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"]
DecisionCount: 4 | UniqueSubPhases: 3

---

## B17-0A-058-005 — AzureBlob: "store" and "stream" are not IMPLEMENT

**Input:** "Store media files in Azure Blob Storage and stream them to the CDN."

Triggers: none ("store" and "stream" are not IMPLEMENT triggers)
CompiledPredicateIds: []
TransitionType: none
