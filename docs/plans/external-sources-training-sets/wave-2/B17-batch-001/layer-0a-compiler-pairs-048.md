# Layer 0-A — Compiler Pairs (048)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: API_CONTRACT + SECURITY combo reference (037-048)

---

## B17-0A-048-001 — API_CONTRACT + SECURITY combo: complete trigger table for connectors 037-048

| Connector | API_CONTRACT triggers | SECURITY triggers | AUTHORIZATION triggers |
|---|---|---|---|
| JiraConnector | connector, webhook | token | OAuth |
| SlackConnector | connector, webhook | token, HMAC-SHA256 validate | OAuth |
| PagerDutyConnector | connector, webhook | token | OAuth |
| DatadogConnector | connector | token (not API key) | — |
| SplunkConnector | connector, webhook | token, HMAC-SHA256 validate | OAuth |
| ElasticsearchConnector | connector | token (not API key) | OAuth |
| RedisConnector | connector | token | — |
| KafkaConnector | connector | token | OAuth |
| RabbitMQConnector | connector | token | OAuth |
| MongoDBConnector | connector | token | OAuth |
| PostgreSQLConnector | connector | token | OAuth |

---

## B17-0A-048-002 — "API key" vs "token": critical distinction for connectors 037-048

For all connectors 037-048:
- "API key" alone → does NOT fire SECURITY
- "API key token" → fires SECURITY (compound contains "token")
- "token" (any compound: access token, bearer token, auth token, agent token) → fires SECURITY
- "key" alone → no predicate

Rule 6 is triggered by the word "token" in any compound noun or standalone form.

---

## B17-0A-048-003 — "integrate" rule: applies equally to all connectors 037-048

For any connector 037-048, "integrate [ConnectorName]" or "integrate [ConnectorName] API":
- Fires API_CONTRACT only
- Does NOT fire IMPLEMENT
- TransitionType: documentation
- No connector code is generated
This is Rule 1 (absolute) and has NO exception based on connector domain.

---

## B17-0A-048-004 — Non-IMPLEMENT verbs for connectors 037-048

The following verbs appear in connector 037-048 inputs but do NOT fire IMPLEMENT:
- sync (Kafka, Redis)
- route (Kafka, RabbitMQ)
- query (PostgreSQL, MongoDB, Elasticsearch)
- store (MongoDB, Redis)
- cache (Redis)
- index (Elasticsearch, Splunk)
- orchestrate (any)
- coordinate (any)
- bridge (any)

IMPLEMENT trigger list for 037-048: build, implement, write, create, code, develop, refactor, restructure, rewrite.

---

## B17-0A-048-005 — Predicate set summary for connectors 037-048

| Pattern | Connectors | DecisionCount |
|---|---|---|
| [IMPL, API] | Datadog (API key), Redis (no token), MongoDB (no token) | 2 |
| [IMPL, API, SEC] | Jira+token, Slack+token, PagerDuty+token, Splunk+token, Elastic+token, Redis+token, Kafka+token, RabbitMQ+token, MongoDB+token, PostgreSQL+token | 3 |
| [IMPL, API, SEC, AUTH] | any above + OAuth | 4 |
