# Layer 0-C — Knowledge-Edge Pairs (042)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Elastic (Elasticsearch) connector

---

## B17-0C-042-001 — Elastic: non-trigger terms

**Proper nouns — no predicate:** Elasticsearch, Elastic, Kibana, Logstash
**Domain nouns — no predicate:** index, shard, replica, mapping, query, aggregation, document, cluster, node, pipeline, ingest
**Acronyms — no predicate:** ELK (Elasticsearch Logstash Kibana), DSL (Domain-Specific Language)

---

## B17-0C-042-002 — Elastic: "API key" is not a token

"Elasticsearch API key" → no SECURITY.
"Elasticsearch bearer token" → SECURITY fires.
"Elasticsearch access token" → SECURITY fires.
Teaching: Elasticsearch uses both API keys and tokens. The predicate fires only when "token" is present.

---

## B17-0C-042-003 — Elastic: "index" as verb is not IMPLEMENT

"Index the product catalogue in Elasticsearch" uses "index" as a verb. This is NOT an IMPLEMENT trigger. Only the IMPLEMENT trigger list applies.

---

## B17-0C-042-004 — Elastic: Elastic Cloud vs self-hosted does not change predicates

Whether the input describes "Elastic Cloud" (managed) or a self-hosted Elasticsearch cluster, predicate rules are identical. Deployment topology is not a predicate trigger. The same trigger words (connector, webhook, token, OAuth) fire the same predicates regardless of hosting context.
