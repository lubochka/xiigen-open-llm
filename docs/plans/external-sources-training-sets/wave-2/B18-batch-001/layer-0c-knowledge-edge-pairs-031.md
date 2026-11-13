# Layer 0-C — Knowledge-Edge Pairs (031)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-031-001 — Cross-domain ARCHITECTURE edge: "pattern" in DDL scenarios

**"pattern" fires ARCHITECTURE absolutely regardless of modifier:**

| Modifier + "pattern" | ARCHITECTURE fires? |
|---|---|
| "index pattern" | YES |
| "partitioning pattern" | YES |
| "null-safety pattern" | YES |
| "timestamp standardization pattern" | YES |
| "enum pattern" | YES |
| "trigger pattern" | YES |
| "sequence naming pattern" | YES |
| "constraint pattern" | YES |
| "migration pattern" (structural context) | YES |

---

## B18-0C-031-002 — Cross-domain ARCHITECTURE edge: "architecture" in DDL scenarios

**"architecture" fires ARCHITECTURE absolutely regardless of modifier:**

| Modifier + "architecture" | ARCHITECTURE fires? |
|---|---|
| "index architecture" | YES |
| "foreign key architecture" | YES |
| "partition architecture" | YES |
| "trigger architecture" | YES |
| "sequence architecture" | YES |
| "stored procedure architecture" | YES |
| "JSONB architecture" | YES |
| "constraint architecture" | YES |
| "data layer architecture" | YES |

---

## B18-0C-031-003 — Cross-domain ARCHITECTURE edge: "structure" in DDL scenarios

**"structure" fires ARCHITECTURE absolutely regardless of modifier:**

| Modifier + "structure" | ARCHITECTURE fires? |
|---|---|
| "index structure" | YES |
| "table structure" | YES |
| "partition structure" | YES |
| "trigger structure" | YES |
| "sequence structure" | YES |
| "enum type structure" | YES |
| "JSONB column structure" | YES |
| "null constraint structure" | YES |
| "timestamp column structure" | YES |
| "FK structure" | YES |

---

## B18-0C-031-004 — Negative edge map: DDL vocabulary that does NOT fire additional predicates

**DDL vocabulary that only fires IMPLEMENT + 4.4 (no other predicates):**

| Token | Does NOT fire |
|---|---|
| "index" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "foreign key" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "partition" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "materialized view" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "stored procedure" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "trigger" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "sequence" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "enum" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "JSONB" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "timestamp" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |
| "NOT NULL" alone | ARCHITECTURE, SECURITY, TEST, DOCUMENT |

---

## B18-0C-031-005 — Complete knowledge edge reference: 025-036 edge inventory

**Total new edge types introduced in files 025-036:**

| Edge type | New tokens | Rule |
|---|---|---|
| IMPLEMENT + 4.4 | create/rebuild/add index, add FK, migrate to partitioned, create view, write procedure, create trigger, reset sequence, add enum, add JSONB column, convert timestamps, add NOT NULL | Rules 2, 5, 6 |
| IMPLEMENT + refactoring | rename column, restructure table, consolidate FK, refactor procedure, rename enum, consolidate nullable, restructure sequences | Rules 1, 9, 20 |
| ARCHITECTURE (absolute) | "[DDL-object] structure/architecture/pattern" | Rules 3, 7, 11 |
| SECURITY (absolute) | PII, GDPR, compliance, encrypted, authentication | Rule 14 |
| TEST | "write tests", "add tests", "create tests" (verb form) | Rule 12 |
| DOCUMENT | "document", "write runbook", "annotate", "describe" | Rule 4 |
| NO ARCHITECTURE | "trigger" alone, "index" alone, "enum" alone, "partition" alone | Absence rule |
| NO SECURITY | "trigger" (DB object), "index", "sequence" | Absence rule |
