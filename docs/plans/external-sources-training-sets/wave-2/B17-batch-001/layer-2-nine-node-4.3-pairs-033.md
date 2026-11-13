# Layer 2 — Nine-Node 4.3 Pairs (033)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-033-001 — ZendeskConnector nine-node flow

**A (Input):** SelectedLogicalTransition: "Build the Zendesk support connector with ticket webhook and API token authentication."

**B (Nine-node execution):**
```
N01: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] — create ticket, update ticket, handle ticket webhook
N02: Zendesk Ticketing REST API v2 — 3 handlers: CreateTicketHandler, UpdateTicketHandler, HandleTicketWebhookHandler
N03: IZendeskConnector: CreateTicketAsync, UpdateTicketAsync, HandleTicketWebhookAsync
N04: HttpClient (injected), IZendeskCredentialStore (subdomain + email/token API key), IZendeskWebhookValidator (HMAC-SHA256)
N05: 4 files (connector, interface, credential store, models)
N06: POST /api/v2/tickets.json, PUT /api/v2/tickets/{id}.json, POST webhook; Basic auth {email}/token:{api_token}; webhook HMAC-SHA256
N07: ArbiterSelectorUnit selects completeness + security review
N08: PASS — 3 handlers implemented, API token stored in credential store, HMAC-SHA256 webhook validation present, filesGenerated: 4
N09: ZendeskConnector complete, 4 files generated
```

Gate B: 3 handlers + API token credential store + HMAC-SHA256 webhook validation
