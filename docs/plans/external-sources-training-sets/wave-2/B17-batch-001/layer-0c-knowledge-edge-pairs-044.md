# Layer 0-C — Knowledge-Edge Pairs (044)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Kafka connector

---

## B17-0C-044-001 — Kafka: non-trigger terms

**Proper nouns — no predicate:** Kafka, Apache Kafka, Confluent
**Domain nouns — no predicate:** topic, partition, offset, consumer group, producer, broker, retention, compaction, lag, schema registry
**Acronyms — no predicate:** SASL (Simple Authentication and Security Layer), KSQL, CDC (Change Data Capture)

---

## B17-0C-044-002 — Kafka: SASL acronym vs SASL token

"SASL authentication" alone → SECURITY fires (Rule 8: "authentication" fires SECURITY).
Wait — correction: "authentication" fires SECURITY (Rule 8). Yes.
"SASL authentication token" → SECURITY fires (Rule 6: token present).
"SASL mechanism" alone → no SECURITY (neither token nor authentication phrase).
Teaching: "authentication" triggers Rule 8; the acronym "SASL" alone does not.

---

## B17-0C-044-003 — Kafka: "orchestrate" is not IMPLEMENT

"Orchestrate the Kafka topic partitions" — "orchestrate" is not in the IMPLEMENT trigger list.
This is a B17 anti-trigger that appears specifically with streaming/messaging connectors.

---

## B17-0C-044-004 — Kafka: consumer group does not add API_CONTRACT

"Consumer group" is a domain noun. It does not add API_CONTRACT. API_CONTRACT fires only from "connector", "webhook", or "integrate"/"wire up" triggers. Consumer/producer terminology is domain-opaque.
