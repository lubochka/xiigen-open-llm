# Layer 2 — Nine-Node 4.3 Pairs (060)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-060-001 — Nine-node Gate B summary for connectors 049-060

| ConnectorName | Gate B condition | Files min |
|---|---|---|
| Auth0Connector | 3 handlers + OAuth 2.0 client credentials token refresh + access token store | 5 |
| CognitoConnector | 3 handlers + OAuth 2.0 token exchange + ID/access token store | 5 |
| FirebaseConnector | 3 handlers + service account authentication token store | 4 |
| SupabaseConnector | 3 handlers + JWT (JSON Web Token) access token store | 4 |
| VercelConnector | 3 handlers + HMAC-SHA256 X-Vercel-Signature validation + bearer token store | 4 |
| NetlifyConnector | 3 handlers + personal access token store | 4 |
| CloudflareConnector | 3 handlers + HMAC-SHA256 webhook validation + API token store | 4 |
| AWSS3Connector | 3 handlers + presigned URL access token store | 4 |
| GCSConnector | 3 handlers + OAuth 2.0 service account token exchange + access token store | 5 |
| AzureBlobConnector | 3 handlers + OAuth 2.0 AAD token exchange + SAS token + access token store | 5 |
| HashiCorpVaultConnector | 3 handlers + Vault token store + token renewal on lease expiry | 4 |

---

## B17-L2-060-002 — N01 input: always SelectedLogicalTransition

For all nine-node flows in B17:
N01 always receives the SelectedLogicalTransition as input.
The predicate set in N01 corresponds to the compiled predicates from Layer 0-A.

---

## B17-L2-060-003 — Complete B17 nine-node connector results (037-060)

All 24 connectors (037-060): N08 PASS at Gate B.
- Connectors with OAuth (Auth0, Cognito, GCS, AzureBlob): OAuth token exchange + token refresh → 5 files
- Connectors with HMAC webhook (Jira, Slack, PagerDuty, Splunk, Vercel, Cloudflare): HMAC-SHA256 validation present → 4 files
- Connectors with token only (Elastic, Redis, Kafka, RabbitMQ, MongoDB, PostgreSQL, Firebase, Supabase, Netlify, AWSS3, HashiCorpVault): token stored securely → 3-4 files
- Connectors with API key only (Datadog): API key stored → 3 files
