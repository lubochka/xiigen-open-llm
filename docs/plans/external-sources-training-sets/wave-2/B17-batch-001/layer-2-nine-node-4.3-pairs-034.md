# Layer 2 — Nine-Node 4.3 Pairs (034)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-034-001 — SalesforceConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the Salesforce CRM connector with REST API endpoint, OAuth 2.0 connected app token flow, and create tests for opportunity sync."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"] — contact upsert, opportunity sync, lead convert
N02: Salesforce REST API v57.0 — 3 handlers: UpsertContactHandler, SyncOpportunityHandler, ConvertLeadHandler
N03: ISalesforceConnector: UpsertContactAsync, SyncOpportunityAsync, ConvertLeadAsync
N04: HttpClient (injected), ISalesforceOAuthClient (connected app OAuth 2.0 + token refresh), ISalesforceCredentialStore (ConsumerKey + ConsumerSecret)
N05: 5 files (connector, interface, OAuth client, models, test suite)
N06: PATCH /services/data/v57.0/sobjects/Contact/{id}, POST /services/data/v57.0/sobjects/Opportunity, POST /services/data/v57.0/sobjects/Lead/convert; OAuth /services/oauth2/token; bearer token
N07: ArbiterSelectorUnit selects completeness + OAuth + test coverage + security review
N08: PASS — 3 handlers implemented, OAuth connected app flow + token refresh present, test suite covers opportunity sync, filesGenerated: 5
N09: SalesforceConnector complete, 5 files generated
```

Gate B: 3 handlers + OAuth connected app flow + token refresh + test suite for opportunity sync endpoint
