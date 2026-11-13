# Layer 0-A — Compiler Pairs (041)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: compiler | focus: Splunk connector

---

## B17-0A-041-001 — Splunk: predicate firing

**Input:** "Build the Splunk log connector with HEC (HTTP Event Collector) webhook ingestion and token authentication."

| Trigger | Predicate | Rule |
|---|---|---|
| Build | IMPLEMENT | 11 |
| connector | API_CONTRACT | 4 |
| webhook | API_CONTRACT | 5 (reinforces; fires once) |
| token | SECURITY | 6 |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
TransitionType: feature_addition
Note: HEC (HTTP Event Collector) is a domain acronym — it fires no predicate alone.
Integration vocabulary present: connector, webhook → IMPLEMENT routes to 4.3

---

## B17-0A-041-002 — Splunk: "integrate" alone

**Input:** "Integrate the Splunk HEC API."

Triggers: "integrate" → API_CONTRACT (Rule 1)
CompiledPredicateIds: ["API_CONTRACT"]
TransitionType: documentation
IMPLEMENT absent — Rule 1 is unconditional for all connector domains.

---

## B17-0A-041-003 — Splunk: token compound

**Input:** "Implement the Splunk connector using the HEC authentication token and validate the HMAC-SHA256 payload signature."

| Trigger | Predicate |
|---|---|
| Implement | IMPLEMENT |
| connector | API_CONTRACT |
| token | SECURITY (Rule 6) |
| validate + HMAC-SHA256 payload signature | SECURITY (Rule 8; reinforces; fires once) |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
DecisionCount: 3 | UniqueSubPhases: 3 (4.3, 3.3, 3.4)

---

## B17-0A-041-004 — Splunk: log ingestion alone

**Input:** "Route application logs to Splunk for indexing and search."

Triggers: none (route is not IMPLEMENT; log, indexing, search are domain nouns)
CompiledPredicateIds: []
TransitionType: none

---

## B17-0A-041-005 — Splunk: full pipeline

**Input:** "Build the Splunk connector, implement OAuth 2.0 token refresh, write tests, and document the ingestion API."

| Trigger | Predicate |
|---|---|
| Build | IMPLEMENT |
| connector | API_CONTRACT |
| OAuth | AUTHORIZATION |
| token | SECURITY |
| write tests | TEST |
| document | DOCUMENT |

CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT"]
DecisionCount: 6 | UniqueSubPhases: 5 (4.3, 3.3, 3.4, 5.1, 2.1)
