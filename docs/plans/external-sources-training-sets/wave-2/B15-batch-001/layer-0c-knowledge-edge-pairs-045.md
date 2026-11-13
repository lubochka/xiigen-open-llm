# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-045-001 — ImportService: ARCHITECTURE triggers

Confirmed ARCHITECTURE triggers:
- "import domain model" → ARCHITECTURE
- "data ingestion model" → ARCHITECTURE
- "bounded context" (import bounded context) → ARCHITECTURE

---

## B15-kedge-045-002 — ImportService: API_CONTRACT triggers

Confirmed API_CONTRACT triggers:
- "wire up the source system API for data ingestion" → API_CONTRACT
- "integrate with the data lake ingestion API" → API_CONTRACT
- "import service contracts" → API_CONTRACT
- "connect to the external data feed" → API_CONTRACT

---

## B15-kedge-045-003 — ImportService: SECURITY triggers

Confirmed SECURITY triggers:
- "authenticate the import source connections" → SECURITY
- "secure the file upload endpoint" → SECURITY
- "encrypt imported data in transit" → SECURITY

---

## B15-kedge-045-004 — ImportService: TEST trigger

"write integration tests for the CSV parsing pipeline" → TEST.
"test import error handling and rollback" → TEST.

---

## B15-kedge-045-005 — "ingest", "parse", "load" are IMPLEMENT trigger verbs

"ingest" → IMPLEMENT. "parse" → IMPLEMENT. "load" → IMPLEMENT (data loading context).
All three fire feature_addition TransitionType when "refactor" is absent.
v6-01 risk: treating "ingest" as an unknown verb and returning [] is incorrect.
