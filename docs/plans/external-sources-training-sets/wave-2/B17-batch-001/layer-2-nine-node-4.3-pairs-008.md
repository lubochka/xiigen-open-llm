# Layer 2 — Nine-Node 4.3 Pairs (008)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-008-001 — PlaidBankingConnector: link token, exchange public token, retrieve transactions

**ConnectorName:** PlaidBankingConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
**TransitionType:** feature_addition

### N01 — Input node
```
Task: "Build the Plaid banking connector and document the account linking and transaction retrieval interfaces."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
SubPhase: 4.3 (IMPLEMENT), 3.3 (API_CONTRACT), 2.1 (DOCUMENT)
```

### N02 — Domain statement
```
Connector: PlaidBankingConnector
API: Plaid API v2023-09-14
Scope: Create link token, exchange public token for access token, retrieve transactions
Auth: client_id + secret in request body
```

### N03 — Interface contract
```
IPlaidBankingConnector:
  - CreateLinkToken(userId: string) → OperationResult<LinkToken>
  - ExchangePublicToken(publicToken: string) → OperationResult<AccessToken>
  - GetTransactions(accessToken: string, startDate: DateOnly, endDate: DateOnly)
    → OperationResult<IReadOnlyList<Transaction>>
  - GetAccountBalance(accessToken: string) → OperationResult<IReadOnlyList<Account>>
```

### N04 — Dependency resolution
```
Dependencies: HttpClient, IPlaidConfig
Config: PlaidClientId, PlaidSecret, PlaidEnvironment (sandbox/production), PlaidBaseUrl
```

### N05 — Implementation plan
```
Files:
1. PlaidBankingConnector.cs
2. IPlaidBankingConnector.cs
3. PlaidModels.cs — LinkToken, AccessToken, Transaction, Account DTOs
4. PlaidConfig.cs
5. PlaidBankingConnectorTests.cs
6. PlaidBankingConnector.md — documentation (account linking flow)
7. PlaidAPI-contract.yaml — OpenAPI specification
```

### N06 — Implementation execution
```
CreateLinkToken: POST /link/token/create
ExchangePublicToken: POST /item/public_token/exchange
GetTransactions: POST /transactions/get (with date range pagination)
GetAccountBalance: POST /accounts/balance/get
```

### N07 — Validation
```
Validate: All four interface methods implemented
Validate: Access token never logged or stored in plaintext
Validate: Transaction pagination handled (Plaid uses cursor-based pagination)
Validate: Documentation covers account linking flow sequence diagram
```

### N08 — Verdict
```
Verdict: PASS
PlaidBankingConnector complete with link token flow, public token exchange, transaction retrieval, and interface documentation.
```

### N09 — Output
```
PlaidBankingConnector complete.
Files generated: 7
ConnectorName: PlaidBankingConnector
SubPhase: 4.3 (primary), 2.1 (documentation also generated)
DocumentationIncluded: true
```

---

## B17-L2-008-002 — Gate B for PlaidBankingConnector

**Gate B:** Four interface methods implemented, access token not logged, pagination handled for GetTransactions, documentation includes account linking sequence, filesGenerated >= 6.
