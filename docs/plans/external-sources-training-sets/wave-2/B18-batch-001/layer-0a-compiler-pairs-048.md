# Layer 0-A — Compiler Pairs (048)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-048-001 — Complete reference: SECURITY trigger map for 037-048 domains

**When SECURITY fires in 037-048 domains:**

| Domain | SECURITY trigger token | Why it fires |
|---|---|---|
| DataAnonymization | "PII", "GDPR", "sensitive data" | PII/GDPR = absolute Rule 14 triggers |
| PIIMasking | "mask", "PII", "credit card", "SSN" | Masking = PII handling vocabulary |
| DataArchival | "PII", "encrypted", "GDPR" | PII/encrypted = absolute; archival alone ≠ SECURITY |
| ShardingMigration | "credentials", "encrypted" | Only fires with explicit security tokens |
| ReplicationSetup | "encrypted", "TLS" | Encryption protocol = absolute SECURITY |
| ReadReplicaPromotion | (rare; only on "credentials") | Promotion alone ≠ SECURITY |
| FullTextSearchIndex | (rare; only on "PII in documents") | Index type alone ≠ SECURITY |
| GeoIndex | (rare; only on "location PII") | Spatial alone ≠ SECURITY |
| CompositeIndex | (uncommon) | Index DDL ≠ SECURITY |
| CoveringIndex | (uncommon) | Index DDL ≠ SECURITY |
| PartialIndex | (uncommon) | Index DDL ≠ SECURITY |

**Key rule:** "anonymize", "mask", "archive" as verbs DO fire SECURITY. "index", "shard", "replica" alone do NOT fire SECURITY.

---

## B18-0A-048-002 — Complete reference: TransitionType determination for 037-048 domains

**TransitionType by verb for 037-048 domains:**

| Verb | Domain | TransitionType |
|---|---|---|
| "write migration" | any | feature_addition |
| "anonymize" | DataAnonymization | feature_addition |
| "restructure table for sharding" | ShardingMigration | refactoring |
| "consolidate shard keys" | ShardingMigration | refactoring |
| "create index" | any index domain | feature_addition |
| "rebuild index" (data) | any index domain | feature_addition |
| "refactor index structure" | any index domain | refactoring |
| "promote replica" | ReadReplicaPromotion | feature_addition |
| "migrate data to new shard" | ShardingMigration | feature_addition |

---

## B18-0A-048-003 — Complete reference: Gate B types for 037-048 domains

**Gate B type selection for new domains (037-048):**

| Domain | Gate B type | Pass condition |
|---|---|---|
| DataAnonymization | row_count_assertion | count(pii_fields IS NOT NULL) == 0 after anonymization |
| PIIMasking | row_count_assertion | count(masked_fields matching regex) == total_rows |
| DataArchival | row_count_assertion | count(archive_table) == count(archived_rows) |
| ShardingMigration | row_count_assertion | count(all_shards) == count(source) |
| ReplicationSetup | schema_change_verification | replication_lag == 0 AND replica_count == expected |
| ReadReplicaPromotion | schema_change_verification | new_primary_accepts_writes == true |
| FullTextSearchIndex | schema_change_verification | index_exists AND query_plan_uses_fts == true |
| GeoIndex | schema_change_verification | spatial_index_exists AND geo_queries_use_index == true |
| CompositeIndex | schema_change_verification | composite_index_exists AND query_plan_uses_index == true |
| CoveringIndex | schema_change_verification | covering_index_exists AND heap_fetches == 0 |
| PartialIndex | schema_change_verification | partial_index_exists AND filtered_query_uses_index == true |

---

## B18-0A-048-004 — Complete reference: v6 corruption types for 037-048 domains

**v6-01 (ARCHITECTURE suppressed) — 037-048 examples:**

| Request fragment | Should fire ARCHITECTURE | v6-01 error |
|---|---|---|
| "anonymization pattern for GDPR" | YES — "pattern" absolute | Misses "pattern" token |
| "sharded architecture" | YES — "architecture" absolute | Misses "architecture" token |
| "replication architecture" | YES — "architecture" absolute | Misses "architecture" token |
| "search index structure" | YES — "structure" absolute | Misses "structure" token |
| "composite index pattern" | YES — "pattern" absolute | Misses "pattern" token |

**v6-03 (wrong sub-phase) — 037-048 examples:**

| Request fragment | Vocabulary | Correct sub-phase | v6-03 error |
|---|---|---|---|
| "anonymize customer data" | data migration | 4.4 | Routed to 4.1 (service) |
| "archive orders to cold storage" | data archival | 4.4 | Routed to 4.1 |
| "create full-text search index" | index DDL | 4.4 | Routed to 4.1 |
| "promote replica to primary" | database operation | 4.4 | Routed to 4.1 |

---

## B18-0A-048-005 — Complete reference: 037-048 disambiguation test

**Disambiguation test for 037-048 domains:**

1. "anonymize" alone → SECURITY fires? **Yes** — anonymize = PII handling vocabulary
2. "archive orders" alone → SECURITY fires? **No** — archive alone is 4.4 data vocabulary
3. "shard key structure" → ARCHITECTURE fires? **Yes** — "structure" absolute trigger
4. "replication setup" alone → ARCHITECTURE fires? **No** — "setup" is not a trigger
5. "create GiST index" → sub-phase? **4.4** — index creation = DDL migration vocabulary
6. "covering index structure" → ARCHITECTURE fires? **Yes** — "structure" absolute trigger
7. "populate test fixtures" → TEST fires? **No** — "populate" + "fixtures" = 4.4 seed vocabulary
8. "write tests to verify index" → TEST fires? **Yes** — "write tests" = verb form Rule 12
9. "TLS encrypted replication" → SECURITY fires? **Yes** — "encrypted" absolute trigger
10. "restructure for sharding" → TransitionType? **refactoring** — "restructure" Rule 1
