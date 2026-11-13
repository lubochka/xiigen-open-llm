# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-026-001 — TriggerMigration edges

**Active edges for TriggerMigration scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write a migration (trigger)" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "create trigger" | IMPLEMENT; 4.4 | DDL vocabulary |
| "audit log structure" | ARCHITECTURE (via "structure") | Rule 3 (absolute) |
| "trigger architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "PII fields" | SECURITY (absolute) | Rule 14 |
| "GDPR compliance" | SECURITY (via both GDPR + compliance) | Rule 14 |
| "trigger" alone | NO SECURITY edge | database object only |

---

## B18-0C-026-002 — SequenceReset edges

**Active edges for SequenceReset scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "reset sequence" | IMPLEMENT; feature_addition; 4.4 | DDL vocabulary |
| "populate sequence" | IMPLEMENT; feature_addition; 4.4 | Rule: populate = seeding-equivalent |
| "restructure sequences" | IMPLEMENT + refactoring | Rule 1 |
| "sequence architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "uniform naming pattern" | ARCHITECTURE (via "pattern") | Rule 7 |
| "sequence structure" | ARCHITECTURE (via "structure") | Rule 3 |

---

## B18-0C-026-003 — EnumTypeChange edges

**Active edges for EnumTypeChange scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "write migration (enum)" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "add enum value" | IMPLEMENT; 4.4 | DDL vocabulary |
| "rename enum value" | IMPLEMENT + refactoring | Rule 9 |
| "enum type structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "state machine architecture" | ARCHITECTURE (via "architecture") | Rule 7 |
| "migrate enum data" | IMPLEMENT; feature_addition; 4.4 | Rule 5 |

---

## B18-0C-026-004 — JSONBColumn edges

**Active edges for JSONBColumn scenarios:**

| Token | Edge | Rule |
|---|---|---|
| "add JSONB column" | IMPLEMENT; feature_addition; 4.4 | DDL vocabulary |
| "write migration (JSONB)" | IMPLEMENT; feature_addition; 4.4 | Rule 2 |
| "migrate to JSONB" | IMPLEMENT; feature_addition; 4.4 | Rule 5 |
| "restructure → JSONB" | IMPLEMENT + refactoring | Rule 1 |
| "JSONB column structure" | ARCHITECTURE (via "structure") | Rule 3 |
| "PII fields within JSON" | SECURITY (absolute) | Rule 14 |
| "encrypted" | SECURITY (absolute) | Rule 14 |

---

## B18-0C-026-005 — TimestampTimezone + NullToNotNull edges

**TimestampTimezone active edges:**

| Token | Edge |
|---|---|
| "write migration (timestamp)" | IMPLEMENT; 4.4 |
| "convert timestamps" | IMPLEMENT; feature_addition; 4.4 |
| "rename created_at" | IMPLEMENT + refactoring |
| "timestamp standardization pattern" | ARCHITECTURE (via "pattern") |
| "timezone architecture" | ARCHITECTURE (via "architecture") |

**NullToNotNull active edges:**

| Token | Edge |
|---|---|
| "add NOT NULL" | IMPLEMENT; feature_addition; 4.4 |
| "backfill nulls" | IMPLEMENT; feature_addition; 4.4 |
| "consolidate nullable columns" | IMPLEMENT + refactoring |
| "null-safety pattern" | ARCHITECTURE (via "pattern") |
| "constraint architecture" | ARCHITECTURE (via "architecture") |
| "structured schema" | NO ARCHITECTURE ("structured" = adjective, not "structure" noun) |
