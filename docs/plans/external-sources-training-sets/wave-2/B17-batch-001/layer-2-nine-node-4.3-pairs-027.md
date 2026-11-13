# Layer 2 — Nine-Node 4.3 Pairs (027)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-027-001 — PlaidBankingConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the Plaid banking connector with OAuth token exchange and link token handling."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] — link token, OAuth exchange, bank account data
N02: Plaid API v2020-09-14 — 3 handlers: CreateLinkTokenHandler, ExchangePublicTokenHandler, FetchTransactionsHandler
N03: IPlaidBankingConnector: CreateLinkTokenAsync, ExchangePublicTokenAsync, FetchTransactionsAsync
N04: HttpClient (injected), IPlaidOAuthExchanger (OAuth token exchange), IPlaidCredentialStore (ClientId + Secret)
N05: 4 files (connector, interface, OAuth exchanger, models)
N06: POST /link/token/create, POST /item/public_token/exchange, POST /transactions/get; OAuth /oauth/token endpoint; bearer token for all requests
N07: ArbiterSelectorUnit selects completeness + OAuth + security review
N08: PASS — 3 handlers implemented, OAuth token exchange present, link token creation present, secure credential storage, filesGenerated: 4
N09: PlaidBankingConnector complete, 4 files generated
```

Gate B: 3 handlers + OAuth token exchange + link token creation + secure credential storage
