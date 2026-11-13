# Held-Out and Corruption Pairs (014)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: held-out-corruption

## B18-HO-014-001 — HO: ETL → 4.4 (not 4.3)

**Request:** "Build an ETL pipeline to load all legacy analytics events into the new event_log table."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 (ETL is data migration vocabulary)

**COR-014-001 (ETL→4.3):** Sub-phase: 4.3, flow: ApiContractFlow43.
- Corruption: ETL treats "extract" as API extraction vocabulary → 4.3
- Fix: ETL is always 4.4 in B18. 4.3 is for connector/API integration. ETL data pipelines route to DataMigrationFlow44.

---

## B18-HO-014-002 — HO: "extract" (ETL context) ≠ refactoring

**Request:** "Extract historical analytics events from 120 CSV files and load them into event_log."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 (ETL: extract data from CSV files)

**COR-014-002 (v6-02):** TransitionType: refactoring ("extract" fires refactoring edge).
- Corruption: "extract" always fires refactoring TransitionType
- Fix: "extract" in data/ETL context → feature_addition (4.4). Refactoring edge requires code class/method target.

---

## B18-HO-014-003 — HO: EventLog + "encrypted" → SECURITY fires

**Request:** "Migrate event log entries to the new table, ensuring all user_id fields are encrypted at rest."

**Correct classification:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + security

**COR-014-003:** SECURITY absent. "encrypted at rest" treated as a migration implementation detail, not a SECURITY trigger.
- Fix: "encrypted" is an absolute SECURITY trigger regardless of phrasing. Always fires SecurityReviewFlow.

---

## B18-HO-014-004 — HO: "architecture pattern" → ARCHITECTURE fires (two triggers)

**Request:** "Migrate event log records and design the event log architecture pattern for high-volume ingestion."

**Correct classification:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- Sub-phases: 4.4 + 3.1
- Reason: "architecture" (absolute) + "pattern" (absolute) both fire ARCHITECTURE; single ARCHITECTURE predicate in output

**COR-014-004 (v6-01):** ARCHITECTURE absent.
- Corruption: "architecture pattern" interpreted as a description of the event log type, not ARCHITECTURE trigger
- Fix: "architecture" and "pattern" are both absolute ARCHITECTURE triggers. One predicate fires.

---

## B18-HO-014-005 — HO: "load" (data context) → 4.4

**Request:** "Load all historical event records from the backup warehouse into analytics_events."

**Correct classification:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- Sub-phase: 4.4 ("load" is data loading vocabulary)

**COR-014-005:** "load" routed to a generic IMPLEMENT without sub-phase signal.
- Corruption: "load" not recognized as 4.4 data vocabulary
- Fix: "load" in data context is a 4.4 routing signal (part of ETL: Extract, Transform, Load). Always routes to DataMigrationFlow44.
