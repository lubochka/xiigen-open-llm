# Layer 0-A — Compiler Pairs (014)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-014-001 — Analytics/EventLog migration: "ETL" → IMPLEMENT, feature_addition, 4.4

**Request:** "Build an ETL pipeline to load all legacy analytics events from CSV exports into the new event_log table."

**Compiler trace:**
- Token: "build" → IMPLEMENT
- Token: "ETL" (Extract, Transform, Load) → IMPLEMENT; 4.4 routing signal (data pipeline vocabulary)
- Token: "load" → IMPLEMENT; 4.4 sub-phase signal (data loading vocabulary)
- Token: "analytics events" → 4.4 sub-phase signal (data vocabulary)
- Token: "CSV exports" → 4.4 sub-phase signal (data file vocabulary)
- TransitionType: feature_addition (new ETL pipeline artifact created)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-014-002 — EventLog: "transform" → IMPLEMENT, 4.4 (not 4.3)

**Request:** "Transform legacy event log records from the denormalized format into the normalized event_log/event_properties tables."

**Compiler trace:**
- Token: "transform" → IMPLEMENT (data transformation vocabulary); 4.4 routing signal
- Token: "event log records" → 4.4 sub-phase signal
- Token: "denormalized" → NOT an ARCHITECTURE trigger; data format adjective
- Token: "normalized" → NOT an ARCHITECTURE trigger
- No "structure", "pattern", "architecture" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

**Teaching point:** ETL and transform route to 4.4, not 4.3. 4.3 is for connector/webhook/API vocabulary.

---

## B18-0A-014-003 — EventLog + SECURITY: "encrypted" fires SECURITY

**Request:** "Migrate analytics event logs to the new event_log table, ensuring all user_id fields are encrypted at rest."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 sub-phase signal
- Token: "analytics event logs" → 4.4 sub-phase signal
- Token: "encrypted" → SECURITY fires (Rule 14, absolute)
- No "structure" → ARCHITECTURE does NOT fire
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT", "SECURITY"], TransitionType: feature_addition, sub-phase: 4.4 + security

---

## B18-0A-014-004 — EventLog: "load" in data context → 4.4 (not ambiguous)

**Request:** "Load all historical event records from the backup warehouse into the new analytics_events table."

**Compiler trace:**
- Token: "load" → IMPLEMENT; 4.4 sub-phase signal (data loading vocabulary)
- Token: "historical event records" → 4.4 sub-phase signal
- Token: "backup warehouse" → 4.4 sub-phase signal (data storage vocabulary)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-014-005 — EventLog: "design the event log architecture" → ARCHITECTURE fires

**Request:** "Migrate event log records and design the event log architecture pattern for high-volume event ingestion."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT; 4.4 routing signal
- Token: "event log records" → 4.4 sub-phase signal
- Token: "design" → ARCHITECTURE (Rule 7)
- Token: "architecture" → ARCHITECTURE (Rule 7, absolute)
- Token: "pattern" → ARCHITECTURE (Rule 7, absolute)
- TransitionType: feature_addition (no refactoring verb)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1
