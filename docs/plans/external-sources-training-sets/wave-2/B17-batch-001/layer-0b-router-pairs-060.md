# Layer 0-B — Router Pairs (060)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Complete routing table for connectors 049-060

---

## B17-0B-060-001 — Complete routing table for connectors 049-060

| ConnectorName | Predicate set | IMPLEMENT sub-phase | Other sub-phases | DecisionCount |
|---|---|---|---|---|
| Auth0Connector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| CognitoConnector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| FirebaseConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| SupabaseConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| VercelConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| NetlifyConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| CloudflareConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| AWSS3Connector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| GCSConnector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| AzureBlobConnector | [IMPL, API, SEC, AUTH] | 4.3 | 3.3, 3.4, 3.4 | 4 |
| HashiCorpVaultConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |

Legend: IMPL=IMPLEMENT, API=API_CONTRACT, SEC=SECURITY, AUTH=AUTHORIZATION

---

## B17-0B-060-002 — "integrate" alone: all connectors 049-060

For any connector 049-060, "Integrate [ConnectorName]" or "Integrate [ConnectorName] API":
- Predicates: [API_CONTRACT]
- SubPhase: 3.3
- TransitionType: documentation
- DecisionCount: 1
Rule 1 (absolute) — no exception.

---

## B17-0B-060-003 — Auth classification for connectors 049-060

| Category | Connectors | Auth predicate pattern |
|---|---|---|
| OAuth + token (4 predicates) | Auth0, Cognito, GCS, AzureBlob | [IMPL, API, SEC, AUTH] → DecisionCount 4, UniqueSubPhases 3 |
| Token only (3 predicates) | Firebase, Supabase, Vercel, Netlify, Cloudflare, AWSS3, HashiCorpVault | [IMPL, API, SEC] → DecisionCount 3, UniqueSubPhases 3 |
| No auth trigger (2 predicates) | Any connector with API key only (no token) | [IMPL, API] → DecisionCount 2, UniqueSubPhases 2 |
