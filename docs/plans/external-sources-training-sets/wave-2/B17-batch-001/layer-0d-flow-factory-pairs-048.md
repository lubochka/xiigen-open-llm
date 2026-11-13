# Layer 0-D — Flow Factory Pairs (048)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: flow-factory | focus: Complete factory reference for connectors 037-048

---

## B17-0D-048-001 — Factory output table for connectors 037-048

| Connector | Flows selected | DecisionCount | UniqueSubPhases |
|---|---|---|---|
| JiraConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| SlackConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| PagerDutyConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| DatadogConnector (API key) | 4.3, 3.3 | 2 | 2 |
| DatadogConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| SplunkConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| ElasticsearchConnector (API key) | 4.3, 3.3 | 2 | 2 |
| ElasticsearchConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| RedisConnector (no token) | 4.3, 3.3 | 2 | 2 |
| RedisConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| KafkaConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| RabbitMQConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| MongoDBConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |
| PostgreSQLConnector (token) | 4.3, 3.3, 3.4 | 3 | 3 |

---

## B17-0D-048-002 — Factory anti-patterns for connectors 037-048

| Anti-pattern | Correction |
|---|---|
| Selecting IntegrationConnectorFlow43 for "integrate" alone | IMPLEMENT absent → only APIContractFlow33 |
| Selecting SecurityAuditFlow34 for API key (no token) | API key is not a token; SECURITY not compiled |
| Selecting SecurityAuditFlow34 for TLS / SSL | TLS/SSL are not SECURITY triggers |
| Inventing domain-specific flows (CacheFlow, StreamingFlow, etc.) | B17 uses only deterministic_generic flows |
| Selecting 4.1 instead of 4.3 when connector vocabulary present | connector/webhook → always 4.3 |

---

## B17-0D-048-003 — Factory decision tree reminder for 037-048

```
Predicate set arrives at factory
  ↓
IMPLEMENT + integration vocabulary → 4.3 → IntegrationConnectorFlow43
IMPLEMENT (no integration vocabulary) → 4.1 → BackendServiceFlow41
API_CONTRACT → 3.3 → APIContractFlow33
SECURITY → 3.4 → SecurityAuditFlow34
AUTHORIZATION → 3.4 → AuthorizationFlow34
TEST → 5.1 → TestingFlow51
DOCUMENT → 2.1 → DocumentationFlow21
QA → 7.x → QAGatingFlow73 [last-absolute]
```
