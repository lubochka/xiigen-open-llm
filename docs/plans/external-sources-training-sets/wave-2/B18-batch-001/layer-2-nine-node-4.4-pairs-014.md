# Layer 2 — Nine-Node 4.4 Pairs (014)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: nine-node-4.4

## B18-N9-014-001 — AnalyticsEventLogMigration: N01 input

**Scenario:** ETL pipeline to load legacy analytics event CSV exports into event_log table.

**N01 input:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- ConnectorName: AnalyticsEventLogMigration
- DomainId: "software-development"
- Flow: DataMigrationFlow44
- Sub-phase: 4.4
- SelectedLogicalTransition: IMPLEMENT alone → DataMigrationFlow44, feature_addition

---

## B18-N9-014-002 — N02-N04: AnalyticsEventLogMigration scope

**N02 — Scope:**
- Source: 120 CSV files; ~4.5M event records
- Target: event_log (event_id, event_type, user_id, session_id, timestamp, properties_json)
- Batch size: 10,000 rows per batch

**N03 — Source Analysis:**
- CSV columns: ts, user_id, session, event_name, page, action, metadata (JSON)
- 4,500,000 rows across 120 files; ~37,500 rows per file
- 3% rows with malformed metadata JSON

**N04 — Target Specification:**
- event_log: (event_id UUID, event_type VARCHAR, user_id UUID, session_id VARCHAR, event_timestamp TIMESTAMP, properties JSONB)
- Column mapping: ts→event_timestamp, event_name→event_type, metadata→properties
- Malformed JSON rows: store raw string with error_flag = true

---

## B18-N9-014-003 — N05-N07: ETL script generation

**N05 — Script Generation:**
1. AnalyticsEventLogETL.cs — extract from CSV, transform columns, validate JSON, load to event_log
2. EventLogBatchLoader.cs — batch insert with error handling
3. EventLogMigration_down.sql — DELETE from event_log where migration_batch_id = current
4. EventLogMigration_verify.sql — row count verification query

**N06 — Execution:** process 120 CSV files sequentially; 10K rows per batch; error rows logged separately

**N07 — Pre-Gate:** verify CSV row count sum = 4,500,000; spot-check 10 files

---

## B18-N9-014-004 — N08: Gate B evaluation

**N08 — Gate B:**
- Type: row_count_assertion
- Condition: count(event_log) == 4,500,000 (includes error_flag rows) ✓
- Secondary: transformation_error_count captured in migration_log ✓
- All event_type values mapped ✓

**N08 verdict: PASS**

---

## B18-N9-014-005 — N09: AnalyticsEventLogMigration output

**N09 output:**
```
{
  "connector_name": "AnalyticsEventLogMigration",
  "flow": "DataMigrationFlow44",
  "transition_type": "feature_addition",
  "gate_b_condition": "count(event_log) == 4,500,000",
  "gate_b_status": "PASS",
  "files_generated": 4,
  "artifacts": [
    "AnalyticsEventLogETL.cs",
    "EventLogBatchLoader.cs",
    "EventLogMigration_down.sql",
    "EventLogMigration_verify.sql"
  ],
  "n08_verdict": "PASS"
}
```

---

## B18-N9-014-006 — ETL vs standard migration: key node differences

| Node | Standard migration | ETL pipeline |
|---|---|---|
| N03 | Analyze database source table | Analyze CSV file format and columns |
| N05 | Generate SQL up/down scripts | Generate ETL code (extract/transform/load) |
| N06 | SQL batch execution | File-by-file extract; transform; batch insert |
| N08 | Row count: target == source table | Row count: loaded == CSV total rows |
