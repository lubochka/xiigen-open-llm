# Layer 2 — Nine-Node 4.3 Pairs (010)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: nine-node-4.3

---

## B17-L2-010-001 — ShopifyConnector: refactoring scenario (legacy → new REST API)

**ConnectorName:** ShopifyConnector
**Input predicates:** ["IMPLEMENT", "API_CONTRACT"]
**TransitionType:** refactoring

### N01 — Input node
```
Task: "Refactor the legacy Shopify connector to use the new Admin REST API 2024-01."
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
SubPhase: 4.3
TransitionType: refactoring
RefactorScope: ShopifyConnector-v1 → ShopifyConnector-v2
```

### N02 — Domain statement
```
Connector: ShopifyConnector (v2 — Admin REST API 2024-01)
Replacing: ShopifyConnector-v1 (legacy private app API)
Scope: Product management, order retrieval, webhook subscription
Breaking changes: Authentication changed from API Key to OAuth token; endpoint prefixes updated
```

### N03 — Interface contract (preserved from v1, implementation migrated)
```
IShopifyConnector:
  - GetProducts(limit: int, pageInfo: string?) → OperationResult<ShopifyProductPage>
  - GetOrders(status: OrderStatus, createdAfter: DateTime?) → OperationResult<IReadOnlyList<Order>>
  - CreateWebhookSubscription(topic: string, callbackUrl: string) → OperationResult<Webhook>
```

### N04 — Dependency resolution
```
Old: X-Shopify-Access-Token header with private app credential
New: OAuth 2.0 access token (Connected App)
Config: ShopifyShopDomain, ShopifyAccessToken (OAuth), ApiVersion = "2024-01"
```

### N05 — Migration plan
```
Files to migrate (6 files):
1. ShopifyConnector.cs — update authentication, endpoint URLs
2. IShopifyConnector.cs — interface unchanged (no breaking changes to consumers)
3. ShopifyModels.cs — update pagination model (v1 page param → cursor-based)
4. ShopifyConfig.cs — update auth config
5. ShopifyWebhookSubscriber.cs — update webhook registration API
6. ShopifyConnectorTests.cs — update test fixtures for new auth
```

### N06 — Migration execution
```
GetProducts: /admin/api/2024-01/products.json (cursor pagination)
GetOrders: /admin/api/2024-01/orders.json
CreateWebhookSubscription: POST /admin/api/2024-01/webhooks.json
Auth: Authorization: Bearer {oauth_token} (replaces X-Shopify-Access-Token)
```

### N07 — Validation
```
Validate: Interface IShopifyConnector unchanged (no breaking changes for consumers)
Validate: Cursor-based pagination implemented (replaces deprecated offset pagination)
Validate: OAuth token used instead of private app API key
```

### N08 — Verdict
```
Verdict: PASS
ShopifyConnector migrated to Admin REST API 2024-01. Interface preserved. 6 files updated.
```

### N09 — Output
```
ShopifyConnector refactored.
Files migrated: 6
ConnectorName: ShopifyConnector
SubPhase: 4.3
TransitionType: refactoring
```

---

## B17-L2-010-002 — Refactoring nine-node: N02 difference

**For refactoring, N02 must state:** what is being replaced, not what is being built from scratch.
"Refactor [X] from [old] to [new]. [N] files affected."

This distinguishes the refactoring nine-node trace from the feature_addition trace at the domain statement level.
