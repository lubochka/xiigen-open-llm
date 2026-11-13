# Layer 0-C — Knowledge-Edge Pairs (056)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-056-001 — Complete edge reference: 20 most common ambiguous edge cases

**The 20 most commonly ambiguous token → edge mappings in B18 037-060:**

1. "structured data" → ARCHITECTURE? **YES** — "structure" is noun in compound
2. "data structure" → ARCHITECTURE? **YES** — "structure" is noun
3. "event sourcing" alone → ARCHITECTURE? **NO** — domain name, no absolute token
4. "CQRS alone" → ARCHITECTURE? **NO** — acronym alone
5. "multi-tenant" alone → SECURITY? **NO** — domain name alone
6. "audit" alone → SECURITY? **NO** — data vocabulary
7. "archive" alone → SECURITY? **NO** — data operation
8. "anonymize" → SECURITY? **YES** — PII handling verb
9. "mask (PII)" → SECURITY? **YES** — masking = PII handling
10. "test fixtures" → TEST? **NO** — "fixtures" = 4.4 seed vocabulary
11. "test data migration" → TEST? **NO** — "test data" = 4.4 vocabulary
12. "verify migration" → TEST? **NO** — "verify" alone is neutral
13. "the design document" → ARCHITECTURE? **NO** — "design" is noun/modifier
14. "design the schema" → ARCHITECTURE? **YES** — "design" is verb
15. "row-level security" → SECURITY? **YES** — "security" absolute
16. "null-safe column" → SECURITY? **NO** — "safe" ≠ "security"
17. "TLS replication" → SECURITY? **YES** — "TLS" = encryption protocol
18. "document table" → DOCUMENT? **NO** — "document" as DB noun
19. "document the pattern" → both ARCHITECTURE and DOCUMENT? **YES** — both fire
20. "partial index" alone → SECURITY? **NO** — DDL vocabulary

---

## B18-0C-056-002 — Complete edge reference: IMPLEMENT from "backfill"

**"backfill" token fires IMPLEMENT + feature_addition + 4.4:**

| Request | Predicates | Sub-phase |
|---|---|---|
| "backfill the tenant_id for existing rows" | IMPLEMENT; feature_addition | 4.4 |
| "backfill NULL values before adding NOT NULL constraint" | IMPLEMENT; feature_addition | 4.4 |
| "backfill anonymized values for PII columns" | IMPLEMENT + SECURITY | 4.4 |
| "backfill the soft-delete flag" | IMPLEMENT; feature_addition | 4.4 |

---

## B18-0C-056-003 — Complete edge reference: IMPLEMENT from "configure"

**"configure" token fires IMPLEMENT + feature_addition + 4.4 in DB context:**

| Request | Predicates | Sub-phase |
|---|---|---|
| "configure replication between nodes" | IMPLEMENT; feature_addition | 4.4 |
| "configure TLS for replication traffic" | IMPLEMENT + SECURITY | 4.4 |
| "configure row-level security policies" | IMPLEMENT + SECURITY | 4.4 |
| "configure database partitioning" | IMPLEMENT; feature_addition | 4.4 |

---

## B18-0C-056-004 — Complete edge reference: IMPLEMENT from "promote"

**"promote" fires IMPLEMENT + feature_addition + 4.4 in replica context:**

| Request | Predicates | Sub-phase |
|---|---|---|
| "promote read replica to primary" | IMPLEMENT; feature_addition | 4.4 |
| "promote standby to active" | IMPLEMENT; feature_addition | 4.4 |
| "promote the replica and write tests" | IMPLEMENT + TEST | 4.4 + 5.x |
| "promote replica with encrypted channel" | IMPLEMENT + SECURITY | 4.4 |

---

## B18-0C-056-005 — Complete edge reference: IMPLEMENT from "rollback"

**"rollback" as verb fires IMPLEMENT + feature_addition + 4.4:**

| Request | Predicates | Sub-phase |
|---|---|---|
| "write the rollback migration" | IMPLEMENT; feature_addition | 4.4 |
| "rollback the schema to previous version" | IMPLEMENT; feature_addition | 4.4 |
| "rollback the index creation" | IMPLEMENT; feature_addition | 4.4 |
| "write rollback and write tests to verify" | IMPLEMENT + TEST | 4.4 + 5.x |
