# Layer 2 — Nine-Node 4.3 Pairs (048)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: nine-node-4.3

---

## B17-L2-048-001 — Nine-node Gate B summary for connectors 037-048

| ConnectorName | Gate B condition | Files min |
|---|---|---|
| JiraConnector | 3 handlers + HMAC-SHA256 X-Hub-Signature validation + API token store | 4 |
| SlackConnector | 3 handlers + HMAC-SHA256 X-Slack-Signature validation + bot token store | 4 |
| PagerDutyConnector | 3 handlers + HMAC-SHA256 webhook signature + API token store | 4 |
| DatadogConnector | 3 handlers + API key store | 3 |
| SplunkConnector | 3 handlers + HMAC-SHA256 payload validation + HEC token store | 4 |
| ElasticsearchConnector | 3 handlers + bearer token store | 4 |
| RedisConnector | 3 handlers + ACL authentication token store | 4 |
| KafkaConnector | 3 handlers + SASL authentication token store | 4 |
| RabbitMQConnector | 3 handlers + vhost access token store | 4 |
| MongoDBConnector | 3 handlers + Atlas access token store | 4 |
| PostgreSQLConnector | 3 handlers + service account access token store | 4 |

---

## B17-L2-048-002 — N01 input: always SelectedLogicalTransition

For all nine-node flows in B17:
N01 always receives the SelectedLogicalTransition as input.
The SelectedLogicalTransition is the A node in every Layer 2 pair.
The predicate set for N01 corresponds to the compiled predicates from the compiler (Layer 0-A).

---

## B17-L2-048-003 — N08 PASS/FAIL patterns for 037-048

**N08 PASS patterns:**
- "X handlers implemented" — completeness
- "HMAC-SHA256 [header name] validation present" — webhook security
- "[credential type] stored securely" — credential management
- "filesGenerated: N ≥ required minimum" — file count

**N08 FAIL patterns (probationary_rejected):**
- "Handler X NOT IMPLEMENTED"
- "filesGenerated < required minimum"
- "HMAC validation absent" (when SECURITY compiled)
- "credential store not injected"
