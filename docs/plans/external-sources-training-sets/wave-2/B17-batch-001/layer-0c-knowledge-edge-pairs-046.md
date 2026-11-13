# Layer 0-C — Knowledge-Edge Pairs (046)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: MongoDB connector

---

## B17-0C-046-001 — MongoDB: non-trigger terms

**Proper nouns — no predicate:** MongoDB, Atlas, Compass
**Domain nouns — no predicate:** collection, document, field, index, aggregation pipeline, replica set, shard, atlas, cluster, BSON (Binary JSON)
**Acronyms — no predicate:** BSON (Binary JSON), ODM (Object Document Mapper)

---

## B17-0C-046-002 — MongoDB: "store" and "retrieve" are not IMPLEMENT triggers

"Store user documents in MongoDB" — "store" is not an IMPLEMENT trigger.
"Retrieve documents by field" — "retrieve" is not an IMPLEMENT trigger.
Both are data operation verbs, not build verbs.

---

## B17-0C-046-003 — MongoDB: Atlas Data API does not double-fire API_CONTRACT

"Build the MongoDB Atlas connector using the Atlas Data API" — "connector" fires API_CONTRACT once. "Atlas Data API" uses "API" which alone is not a trigger. API_CONTRACT fires once only; predicate set is ["IMPLEMENT", "API_CONTRACT"].

---

## B17-0C-046-004 — MongoDB: "x.509 certificate" does not fire SECURITY

"Build the MongoDB connector with x.509 certificate authentication" — "x.509 certificate" is a TLS client auth mechanism, not a token or HMAC phrase. SECURITY does NOT fire from certificate authentication alone. Only "token" or the Rule 8 phrases fire SECURITY.
Exception: if the sentence also says "authentication" → Rule 8 fires SECURITY.
