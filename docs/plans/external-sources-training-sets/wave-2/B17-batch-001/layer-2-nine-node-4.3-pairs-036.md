# Layer 2 — Nine-Node 4.3 Pairs (036)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-036-001 — Nine-node Gate B summary for connectors 025-036

| ConnectorName | Gate B condition | Files min |
|---|---|---|
| TwilioSmsConnector | 3 handlers + HMAC-SHA256 on X-Twilio-Signature + AuthToken store | 4 |
| SendGridEmailConnector | 3 handlers + HMAC-SHA256 webhook validation + test suite | 5 |
| PlaidBankingConnector | 3 handlers + OAuth token exchange + link token creation | 4 |
| DocuSignConnector | 3 handlers + OAuth auth code + token refresh + Connect webhook HMAC | 5 |
| CloudinaryConnector | 3 handlers + HMAC-SHA256 API secret validation | 4 |
| AlgoliaConnector | 3 handlers + admin token credential store + batch indexing | 4 |
| SegmentAnalyticsConnector | 4 methods (track/identify/group/batch) + write key + batch limit 500 | 4 |
| MixpanelConnector | 3 handlers + project token secure storage | 3 |
| ZendeskConnector | 3 handlers + API token credential store + HMAC-SHA256 webhook | 4 |
| SalesforceConnector | 3 handlers + OAuth connected app + token refresh + test suite | 5 |
| GitHubWebhookConnector | 3 handlers + HMAC-SHA256 X-Hub-Signature-256 + OAuth app token | 5 |

---

## B17-L2-036-002 — Nine-node common patterns for connectors 025-036

**N08 PASS patterns (025-036):**
- "X handlers implemented" — completeness
- "HMAC-SHA256 [header name] validation present" — webhook security
- "OAuth [flow type] + token refresh present" — auth
- "write key / project token / API token stored securely" — credential management
- "batch limit N enforced" — business rule (Segment)

**N08 FAIL patterns (probationary_rejected):**
- "Handler X NOT IMPLEMENTED"
- "filesGenerated < required minimum"
- "HMAC validation absent"
- "credential store not injected"

---

## B17-L2-036-003 — N01 input: always SelectedLogicalTransition

For all nine-node flows in B17:
N01 always receives the SelectedLogicalTransition as input.
The SelectedLogicalTransition is the A node in every Layer 2 pair.
The predicate set for N01 corresponds to the compiled predicates from the compiler (Layer 0-A).
