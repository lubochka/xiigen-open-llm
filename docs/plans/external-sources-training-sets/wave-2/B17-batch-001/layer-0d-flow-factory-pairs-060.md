# Layer 0-D — Flow Factory Pairs (060)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Complete factory reference for connectors 049-060

---

## B17-0D-060-001 — Factory output table for connectors 049-060

| Connector | Flows selected | DecisionCount | UniqueSubPhases |
|---|---|---|---|
| Auth0Connector | 4.3, 3.3, 3.4, 3.4 | 4 | 3 |
| CognitoConnector | 4.3, 3.3, 3.4, 3.4 | 4 | 3 |
| FirebaseConnector | 4.3, 3.3, 3.4 | 3 | 3 |
| SupabaseConnector | 4.3, 3.3, 3.4 | 3 | 3 |
| VercelConnector | 4.3, 3.3, 3.4 | 3 | 3 |
| NetlifyConnector | 4.3, 3.3, 3.4 | 3 | 3 |
| CloudflareConnector | 4.3, 3.3, 3.4 | 3 | 3 |
| AWSS3Connector | 4.3, 3.3, 3.4 | 3 | 3 |
| GCSConnector | 4.3, 3.3, 3.4, 3.4 | 4 | 3 |
| AzureBlobConnector | 4.3, 3.3, 3.4, 3.4 | 4 | 3 |
| HashiCorpVaultConnector | 4.3, 3.3, 3.4 | 3 | 3 |

---

## B17-0D-060-002 — Factory anti-patterns for connectors 049-060

| Anti-pattern | Correction |
|---|---|
| Selecting AuthorizationFlow34 for OIDC/SAML/PKCE | Only "OAuth" compiles AUTHORIZATION |
| Selecting SecurityAuditFlow34 for signed URL (GCS/S3) | Signed URL is not a token |
| Selecting SecurityAuditFlow34 for connection string (AzureBlob) | Connection string is not a token |
| Selecting SecurityAuditFlow34 for secret ID alone (HashiCorp Vault) | Secret ID requires "authenticate" or "token" to compile SECURITY |
| Inventing domain-specific flows (IdentityFlow, StorageFlow, SecretsFlow) | B17 uses only deterministic_generic standard flows |
| Selecting IntegrationConnectorFlow43 for "deploy", "publish", "retrieve", "upload" | None of these fire IMPLEMENT |

---

## B17-0D-060-003 — Factory decision tree: complete B17 (037-060)

```
IMPLEMENT + connector vocab → 4.3 → IntegrationConnectorFlow43 [deterministic_generic]
IMPLEMENT (no connector vocab) → 4.1 → BackendServiceFlow41 [deterministic_generic]
API_CONTRACT → 3.3 → APIContractFlow33 [deterministic_generic]
SECURITY → 3.4 → SecurityAuditFlow34 [deterministic_generic]
AUTHORIZATION → 3.4 → AuthorizationFlow34 [deterministic_generic]
TEST → 5.1 → TestingFlow51 [deterministic_generic]
DOCUMENT → 2.1 → DocumentationFlow21 [deterministic_generic]
QA → 7.x → QAGatingFlow73 [deterministic_generic] [last-absolute]
```

No GapHandler exists in B17. Unrecognized vocabulary produces no flow.
