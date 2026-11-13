# Layer 0-C — Knowledge-Edge Pairs (051)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-051-001 — ShardingMigration refactoring ARCHITECTURE edge

**Request:** "Restructure the payments table for horizontal sharding and write the migration."

| Token | Edge fired |
|---|---|
| "restructure" | IMPLEMENT; TransitionType: refactoring (Rule 1) |
| "horizontal sharding" | 4.4 sub-phase vocabulary |
| "write the migration" | IMPLEMENT reinforcement |

**Note:** No "structure"/"architecture"/"pattern" token → ARCHITECTURE does NOT fire.

**Output:** ["IMPLEMENT"]
**TransitionType:** refactoring

---

## B18-0C-051-002 — DataArchival with ARCHITECTURE edge set

**Request:** "Design the archival data structure and write the migration to move stale records to archive tables."

| Token | Edge fired |
|---|---|
| "design" | ARCHITECTURE (Rule 11) |
| "archival data structure" | ARCHITECTURE (Rule 3, "structure" absolute) |
| "write the migration" | IMPLEMENT (Rule 2) |
| "archive tables" | 4.4 vocabulary |

**Output:** ["IMPLEMENT", "ARCHITECTURE"]

---

## B18-0C-051-003 — CQRSProjection + TEST edge set

**Request:** "Implement the CQRS projection tables migration and write tests to verify all events are correctly projected."

| Token | Edge fired |
|---|---|
| "implement the ... migration" | IMPLEMENT (Rule 2) |
| "projection tables" | 4.4 DDL vocabulary |
| "write tests" | TEST (Rule 12) |

**Output:** ["IMPLEMENT", "TEST"]

---

## B18-0C-051-004 — MultiTenantSchema SECURITY edge

**Request:** "Write the multi-tenant isolation migration to add row-level security controls."

| Token | Edge fired |
|---|---|
| "write the ... migration" | IMPLEMENT (Rule 2) |
| "row-level security controls" | SECURITY (Rule 14, "security" absolute) |
| "tenant isolation" | 4.4 vocabulary |

**Output:** ["IMPLEMENT", "SECURITY"]

---

## B18-0C-051-005 — TestDataMigration PII + SECURITY edge

**Request:** "Write the test data seeding migration and ensure all PII fields are anonymized before population."

| Token | Edge fired |
|---|---|
| "write the test data seeding migration" | IMPLEMENT (Rule 2); "seed migration" = 4.4 |
| "PII fields" | SECURITY (Rule 14, absolute) |
| "anonymized" | SECURITY (Rule 14, reinforcement) |

**Output:** ["IMPLEMENT", "SECURITY"]
