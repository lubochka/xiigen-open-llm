# Layer 0-C — Knowledge-Edge Pairs (048)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Complete edge reference for connectors 037-048

---

## B17-0C-048-001 — Complete non-trigger terms for connectors 037-048

**Proper nouns — no predicate:**
Jira, Atlassian, Slack, PagerDuty, Datadog, Splunk, Elasticsearch, Elastic, Redis, Redis Labs, Kafka, Apache Kafka, Confluent, RabbitMQ, MongoDB, Atlas, PostgreSQL, Postgres

**Domain nouns — no predicate:**
issue, ticket, sprint, channel, workspace, incident, alert, metric, trace, index, shard, key, value, topic, partition, exchange, queue, collection, document, table, schema

**Acronyms — no predicate:**
JIRA, APM (Application Performance Monitoring), HEC (HTTP Event Collector), ELK (Elasticsearch Logstash Kibana), SASL (Simple Authentication and Security Layer), AMQP (Advanced Message Queuing Protocol), BSON (Binary JSON), SQL, JDBC (Java Database Connectivity), SLO (Service Level Objective)

---

## B17-0C-048-002 — "API key" is not a token: applies to all connectors 037-048

**Does NOT fire SECURITY:**
- Datadog API key
- Elasticsearch API key
- MongoDB admin key
- Redis password (without "token")
- Kafka bootstrap key

**DOES fire SECURITY (Rule 6):**
- any compound containing "token": access token, bearer token, auth token, API token, agent token, personal access token

---

## B17-0C-048-003 — SECURITY triggers: fires vs does not for 037-048

**Fires SECURITY:**
- token (any compound)
- authentication / authenticate (any connector)
- HMAC-SHA256 validation phrase (Jira, Slack, PagerDuty, Splunk webhooks)

**Does NOT fire SECURITY:**
- API key (no token)
- TLS / SSL (encryption layer, not auth trigger)
- signing secret (Slack-specific, not a trigger)
- certificate (x.509 or other)
- password (without token)
- "secure" as adjective alone

---

## B17-0C-048-004 — AUTHORIZATION: only "OAuth" fires (037-048)

**Fires AUTHORIZATION:** OAuth, OAuth 2.0, OAuth token, OAuth flow
**Does NOT fire AUTHORIZATION:**
- SASL (Kafka)
- LDAP (Lightweight Directory Access Protocol) (any)
- Basic auth (Redis, RabbitMQ)
- certificate (MongoDB)
- API key (Datadog, Elastic)
