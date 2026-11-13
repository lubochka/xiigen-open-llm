# Layer 0-B — Router Pairs (048)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Complete routing table for connectors 037-048

---

## B17-0B-048-001 — Complete routing table for connectors 037-048

| ConnectorName | Predicate set | IMPLEMENT sub-phase | Other sub-phases | DecisionCount |
|---|---|---|---|---|
| JiraConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| SlackConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| PagerDutyConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| DatadogConnector (API key) | [IMPL, API] | 4.3 | 3.3 | 2 |
| DatadogConnector (token) | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| SplunkConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| ElasticsearchConnector (API key) | [IMPL, API] | 4.3 | 3.3 | 2 |
| ElasticsearchConnector (token) | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| RedisConnector | [IMPL, API] or [IMPL, API, SEC] | 4.3 | 3.3 or 3.3+3.4 | 2 or 3 |
| KafkaConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| RabbitMQConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| MongoDBConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |
| PostgreSQLConnector | [IMPL, API, SEC] | 4.3 | 3.3, 3.4 | 3 |

Legend: IMPL=IMPLEMENT, API=API_CONTRACT, SEC=SECURITY

---

## B17-0B-048-002 — "integrate" alone: all connectors 037-048

For any connector 037-048, "Integrate [ConnectorName]" or "Integrate [ConnectorName] API":
- Predicates: [API_CONTRACT]
- SubPhase: 3.3
- TransitionType: documentation
- DecisionCount: 1

---

## B17-0B-048-003 — Key routing distinction: API key vs token (037-048)

| Input pattern | SECURITY routed | Flow selected |
|---|---|---|
| "API key" (Datadog, Elastic) | No | SecurityAuditFlow34 NOT selected |
| "token" compound | Yes | SecurityAuditFlow34 selected |
| "API key token" | Yes | Both "API key" and "token" present → token triggers |

Rule 6 is triggered by the word "token" — not "key", "credential", or "secret" alone.
